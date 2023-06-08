using System;
using System.Collections;
using UnityEngine;

public class GauntletMMOPlayer : MonoBehaviour
{
	[NonSerialized]
	public GameObject _Object;

	[NonSerialized]
	public string _UserID;

	[NonSerialized]
	public int _Score;

	[NonSerialized]
	public int _Accuracy;

	[NonSerialized]
	public bool _IsReady;

	private AvatarData.InstanceInfo mInstanceInfo;

	private string mName;

	private Texture2D mImage;

	private KAWidget mNameItem;

	private KAWidget mImageItem;

	private KAWidget mStatusItem;

	private bool mIsInstanceInfoReady;

	private const string IDLE_ANIM = "CadetIdle";

	public string pName => mName;

	public void Awake()
	{
		GameObject gameObject = GameObject.Find("PfPictureCamera");
		RenderTexture targetTexture = new RenderTexture(256, 256, 24);
		if (gameObject != null)
		{
			gameObject.GetComponent<Camera>().targetTexture = targetTexture;
		}
		else
		{
			Debug.LogError("Could not find PfPictureCamera, RenderTexture will not be set.");
		}
		mImage = new Texture2D(256, 256, TextureFormat.ARGB32, mipChain: false);
	}

	public static GauntletMMOPlayer CreatePlayer(string inUserID, int inGender)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfAvatar"));
		gameObject.name = "Avatar " + inUserID;
		AvAvatarBlink componentInChildren = gameObject.GetComponentInChildren<AvAvatarBlink>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = false;
		}
		PlayAnim(gameObject, "CadetIdle", WrapMode.Loop);
		GauntletMMOPlayer gauntletMMOPlayer = gameObject.AddComponent<GauntletMMOPlayer>();
		gauntletMMOPlayer.CreatePlayer(inUserID, gameObject);
		return gauntletMMOPlayer;
	}

	private void CreatePlayer(string inUserID, GameObject inObject)
	{
		_Object = inObject;
		_UserID = inUserID;
		AvAvatarController obj = (AvAvatarController)_Object.GetComponent(typeof(AvAvatarController));
		obj._ControlMode = AvAvatarControlMode.NONE;
		obj.pState = AvAvatarState.NONE;
		_Object.transform.position = new Vector3(0f, -5000f, 0f);
		AvatarData.SetDisplayNameVisible(_Object, inVisible: false, isMember: false);
		mInstanceInfo = new AvatarData.InstanceInfo();
		mInstanceInfo.mAvatar = _Object;
		AvatarData.Load(mInstanceInfo, inUserID);
		bool flag = false;
		if (Nicknames.pInstance != null)
		{
			string nickname = Nicknames.pInstance.GetNickname(inUserID);
			if (!string.IsNullOrEmpty(nickname))
			{
				UpdateName(nickname);
				flag = true;
			}
		}
		if (!flag)
		{
			WsWebService.GetDisplayNameByUserID(inUserID, ServiceEventHandler, null);
		}
	}

	public void SetItemData(KAWidget inImageItem, KAWidget inNameItem, KAWidget inStatusItem)
	{
		ResetData();
		mNameItem = inNameItem;
		mImageItem = inImageItem;
		mStatusItem = inStatusItem;
		if (mImageItem != null)
		{
			Texture2D texture2D = (Texture2D)mImageItem.GetTexture();
			if (texture2D != null)
			{
				Color[] pixels = texture2D.GetPixels();
				mImage.SetPixels(pixels);
				mImage.Apply();
			}
			mImageItem.SetTexture(mImage);
			mImageItem.SetVisibility(inVisible: true);
		}
		UpdateName(mName);
	}

	public void UpdateStatus(LocaleString inTxtReady, LocaleString inTxtNotReady)
	{
		if (mStatusItem != null)
		{
			if (_IsReady)
			{
				mStatusItem.SetTextByID(inTxtReady._ID, inTxtReady._Text);
			}
			else
			{
				mStatusItem.SetTextByID(inTxtNotReady._ID, inTxtNotReady._Text);
			}
			mStatusItem.SetVisibility(inVisible: true);
		}
	}

	private void ResetData()
	{
		if (mImageItem != null)
		{
			mImageItem.SetVisibility(inVisible: false);
		}
		if (mNameItem != null)
		{
			mNameItem.SetVisibility(inVisible: false);
		}
		if (mStatusItem != null)
		{
			mStatusItem.SetVisibility(inVisible: false);
		}
	}

	public void DestroyMe()
	{
		ResetData();
		UnityEngine.Object.Destroy(_Object);
	}

	public static void PlayAnim(GameObject inAvatar, string inAnimName, WrapMode inWrapMode)
	{
		if (!(inAvatar != null))
		{
			return;
		}
		Component[] componentsInChildren = inAvatar.GetComponentsInChildren<Animation>();
		componentsInChildren = componentsInChildren;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Animation animation = (Animation)componentsInChildren[i];
			if (!(animation[inAnimName] == null))
			{
				animation[inAnimName].wrapMode = inWrapMode;
				animation.Play(inAnimName);
			}
		}
	}

	public void TakeAShot()
	{
		GameObject gameObject = GameObject.Find("PfPictureCamera");
		if (gameObject != null)
		{
			AvPictureCamera component = gameObject.GetComponent<AvPictureCamera>();
			if (component != null)
			{
				Light component2 = gameObject.transform.Find("Directional light").GetComponent<Light>();
				component2.enabled = true;
				UtUtilities.SetLayerRecursively(_Object, LayerMask.NameToLayer("LoadScreen"));
				Vector3 position = _Object.transform.TransformPoint(new Vector3(0f, 1.18f, 1.4f));
				component.transform.position = position;
				component.transform.LookAt(_Object.transform.TransformPoint(new Vector3(0f, 1.18f, 0f)), Vector3.up);
				component.OnTakePicture(_Object, lookAtCamera: false);
				component.OnCopyRenderBuffer(mImage);
				UtUtilities.SetLayerRecursively(_Object, LayerMask.NameToLayer("MMOAvatar"));
				component2.enabled = false;
			}
		}
	}

	public void Update()
	{
		if (!mIsInstanceInfoReady && mInstanceInfo.pIsReady)
		{
			mIsInstanceInfoReady = true;
			StartCoroutine("WaitForIdleAnim");
		}
	}

	private IEnumerator WaitForIdleAnim()
	{
		yield return new WaitForEndOfFrame();
		AvatarData.RemovePartScale(mInstanceInfo);
		TakeAShot();
		if (mImageItem != null)
		{
			mImageItem.SetVisibility(inVisible: true);
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			UpdateName((string)inObject);
			break;
		case WsServiceEvent.ERROR:
			Debug.LogError("Error !!! get user name failed");
			break;
		}
	}

	private void UpdateName(string inName)
	{
		mName = inName;
		if (mNameItem != null)
		{
			mNameItem.SetText(mName);
			mNameItem.SetVisibility(inVisible: true);
		}
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}
}
