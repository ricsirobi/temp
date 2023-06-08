using UnityEngine;

public class PetBundleLoader
{
	private GameObject mPetGeometry;

	private Texture mPetTexture;

	private PetBundleLoadedCallback LoadingDoneCallBack;

	private object LoadingDoneCallBackData;

	private PetDataPet mData;

	private string mBundleUrl;

	private string mGeometryPrefabName;

	private string mTextureName;

	private AssetBundle mBundle;

	private GameObject mParent;

	private bool mAttachAfterLoad;

	public GameObject pPetGeometry => mPetGeometry;

	public bool IsPetAllowed(PetDataPet pd)
	{
		if (mParent == null)
		{
			return true;
		}
		AvAvatarController component = mParent.GetComponent<AvAvatarController>();
		if (component == null)
		{
			return false;
		}
		if (!component.IsMember())
		{
			return PetData.IsFreePet(pd);
		}
		return true;
	}

	public void LoadPets(AvAvatarController AAC, PetDataPet pd, bool attachAfterLoad, PetBundleLoadedCallback inCallback, object inCallbackData)
	{
		if (AAC == null)
		{
			Debug.LogError("Error: AAC shouldn't be null. Use LoadPetFromPetDataPet instead.");
			return;
		}
		mParent = AAC.gameObject;
		mAttachAfterLoad = attachAfterLoad;
		string userID = AAC.GetUserID();
		if (pd == null)
		{
			LoadPetByPlayerID(userID, inCallback, inCallbackData);
		}
		else
		{
			LoadPetFromPetDataPet(pd, inCallback, inCallbackData);
		}
	}

	public void LoadPets(GameObject go, PetDataPet pd, bool attachAfterLoad, PetBundleLoadedCallback inCallback, object inCallbackData)
	{
		if (!(go == null))
		{
			mParent = go;
			mAttachAfterLoad = attachAfterLoad;
			LoadPetFromPetDataPet(pd, inCallback, inCallbackData);
		}
	}

	public void LoadPetByPlayerID(string playerID, PetBundleLoadedCallback inCallback, object inCallbackData)
	{
		LoadingDoneCallBack = inCallback;
		LoadingDoneCallBackData = inCallbackData;
		WsWebService.GetCurrentPetByUserID(playerID, GetCurrentPetEventHandler, null);
	}

	public void GetCurrentPetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			bool flag = false;
			PetData petData = (PetData)inObject;
			if (petData == null || petData.Pet == null)
			{
				flag = true;
			}
			if (flag)
			{
				ExecCallback(loadSucceeded: false);
				UtDebug.LogError("Failed loading Pet.");
			}
			else if (IsPetAllowed(petData.Pet[0]))
			{
				LoadPetFromPetDataPet(petData.Pet[0], LoadingDoneCallBack, LoadingDoneCallBackData);
			}
			break;
		}
		case WsServiceEvent.ERROR:
			ExecCallback(loadSucceeded: false);
			UtDebug.LogError("Failed loading Pet.");
			break;
		}
	}

	public void LoadPetFromPetDataPet(PetDataPet pd, PetBundleLoadedCallback inCallback, object inCallbackData)
	{
		mData = pd;
		string[] array = pd.Geometry.Split('/');
		mBundleUrl = array[0] + "/" + array[1];
		mGeometryPrefabName = array[2];
		mTextureName = pd.Texture.Substring(mBundleUrl.Length + 1);
		LoadingDoneCallBack = inCallback;
		LoadingDoneCallBackData = inCallbackData;
		RsResourceManager.Load(mBundleUrl, BundleLoadEventHandler);
	}

	private void BundleLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent.Equals(RsResourceLoadEvent.COMPLETE))
		{
			mBundle = (AssetBundle)inObject;
			mPetGeometry = (GameObject)Object.Instantiate(mBundle.LoadAsset(mGeometryPrefabName));
			mPetTexture = (Texture)mBundle.LoadAsset(mTextureName);
			UtUtilities.SetObjectTexture(mPetGeometry, 0, mPetTexture);
			mPetGeometry.BroadcastMessage("SetData", mData, SendMessageOptions.DontRequireReceiver);
			if (mParent != null && mAttachAfterLoad)
			{
				AttachPetToPlayer(mParent.transform);
			}
			ExecCallback(loadSucceeded: true);
		}
	}

	private void AttachPetToPlayer(Transform playerTransform)
	{
		mPetGeometry.SetActive(value: true);
		mPetGeometry.BroadcastMessage("OnAttachToAvatar", playerTransform, SendMessageOptions.DontRequireReceiver);
		mPetGeometry.BroadcastMessage("OnAvatarSetPositionDone", playerTransform.position, SendMessageOptions.DontRequireReceiver);
	}

	private void ExecCallback(bool loadSucceeded)
	{
		if (LoadingDoneCallBack != null)
		{
			LoadingDoneCallBack(this, loadSucceeded, LoadingDoneCallBackData);
		}
	}

	public static void LoadPetsForAvatar(GameObject avatar)
	{
		LoadPetsForAvatar(avatar, null);
	}

	public static void LoadPetsForAvatar(GameObject avatar, PetDataPet pd)
	{
		DetachPetsForAvatar(avatar);
		AvAvatarController component = avatar.GetComponent<AvAvatarController>();
		if (component != null)
		{
			new PetBundleLoader().LoadPets(component, pd, attachAfterLoad: true, PetLoadedCallback, avatar);
		}
		else if (avatar != null)
		{
			new PetBundleLoader().LoadPets(avatar, pd, attachAfterLoad: true, PetLoadedCallback, avatar);
		}
	}

	public static void PetLoadedCallback(PetBundleLoader bundleLoader, bool loadSucceeded, object callbackData)
	{
		if (loadSucceeded && AvAvatar.IsCurrentPlayer((GameObject)callbackData))
		{
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SetPet(bundleLoader.mData);
			}
			SetPetReturnButtonVisibility(vis: true);
		}
	}

	public static void DetachPetsForAvatar(GameObject avatar)
	{
		if (avatar == null)
		{
			return;
		}
		AvAvatarController component = avatar.GetComponent<AvAvatarController>();
		if (component != null && component.pPetObject != null)
		{
			component.pPetObject.OnAvatarDetachPets();
		}
		if (AvAvatar.IsCurrentPlayer(avatar))
		{
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SetPet(null);
			}
			SetPetReturnButtonVisibility(vis: false);
		}
	}

	public static void SetPetReturnButtonVisibility(bool vis)
	{
	}
}
