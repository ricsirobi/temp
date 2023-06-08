using System;
using System.Collections.Generic;
using UnityEngine;

public class AgeUpCustomizedCutscene : KAMonoBase
{
	public static Action _UIAgeUpCallback;

	public CutSceneData _AgeUpCutsceneData;

	public GameObject[] _CutsceneObjects;

	public Camera _CutsceneCamera;

	public Vector3 _CutsceneStartPos = Vector3.zero;

	public RaisedPetStage[] _CutSceneStages;

	public int[] _FUETaskIDs;

	public string _FUEAgeUpCustomizationBundleURL;

	private Vector3 mOriginalAvatarPosition = Vector3.zero;

	private Quaternion mOriginalAvatarRotation = Quaternion.identity;

	private DragonAgeUpConfig.OnDragonAgeUpDone mCallback;

	private List<Camera> mDisabledCameras = new List<Camera>();

	private bool mPrevAvatarActive = true;

	private bool mResetFollowAvatar = true;

	private AvAvatarState mPrevAvatarState;

	private SanctuaryPet mPreviousPet;

	private SanctuaryPet mNewPet;

	private RaisedPetStage mFromStage = RaisedPetStage.BABY;

	private int mPetCreated;

	private RaisedPetData mPetData;

	private bool mWasAvatarMounted;

	private bool mPrevMountState;

	private SnChannel mPetSoundChannel;

	private bool mSnChannelDefaultRollOff;

	private float mSnChannelDefaultVolume;

	private GameObject mMessageObj;

	public void Init(RaisedPetStage fromStage, RaisedPetData inData, DragonAgeUpConfig.OnDragonAgeUpDone inCallback, GameObject messageObj = null)
	{
		mCallback = inCallback;
		mFromStage = fromStage;
		mPrevAvatarState = AvAvatar.pState;
		mPetData = inData;
		mMessageObj = messageObj;
		if (_CutsceneCamera != null)
		{
			_CutsceneCamera.gameObject.SetActive(value: false);
		}
		WsTokenMonitor.pReloadSceneAllowed = false;
		if (Array.Exists(_CutSceneStages, (RaisedPetStage x) => x == inData.pStage))
		{
			RaisedPetData raisedPetData = new RaisedPetData(inData);
			KAUICursorManager.SetDefaultCursor("Loading");
			SanctuaryManager.CreatePet(inData, SanctuaryManager.pInstance._PetOffScreenPosition, Quaternion.identity, base.gameObject, "Full", applyCustomSkin: true);
			SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(inData.PetTypeID);
			raisedPetData.pStage = mFromStage;
			int ageIndex = RaisedPetData.GetAgeIndex(mFromStage);
			SantuayPetResourceInfo santuayPetResourceInfo = Array.Find(sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList, (SantuayPetResourceInfo p) => p._Gender == inData.Gender);
			if (santuayPetResourceInfo != null)
			{
				raisedPetData.Geometry = santuayPetResourceInfo._Prefab;
			}
			SanctuaryManager.CreatePet(raisedPetData, SanctuaryManager.pInstance._PetOffScreenPosition, Quaternion.identity, base.gameObject, "Full", applyCustomSkin: true);
		}
		else
		{
			ActivateObjects(inActive: false);
			LoadCurrentPet();
		}
	}

	public void OnPetReady(SanctuaryPet pet)
	{
		if (pet != null)
		{
			mPetCreated++;
			if (pet.pData.pStage == mFromStage)
			{
				mPreviousPet = pet;
			}
			else
			{
				mNewPet = pet;
			}
			pet.pMeterPaused = true;
			pet.SetFollowAvatar(follow: false);
			if (mPetCreated > 1)
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				SetCutscene();
			}
		}
	}

	private void SetCutscene()
	{
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.ActivateAll(active: false);
		}
		mPrevAvatarActive = AvAvatar.pObject.activeSelf;
		AvAvatar.pObject.SetActive(value: true);
		if (SanctuaryManager.pCurPetInstance.pIsMounted || SanctuaryManager.pMountedState)
		{
			mWasAvatarMounted = true;
			mPrevMountState = SanctuaryManager.pMountedState;
			SanctuaryManager.pMountedState = false;
			SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
			AvAvatar.pSubState = AvAvatarSubState.NORMAL;
		}
		StartCutScene();
	}

	private void StartCutScene()
	{
		AvAvatar.pState = AvAvatarState.NONE;
		mOriginalAvatarPosition = AvAvatar.position;
		mOriginalAvatarRotation = AvAvatar.mTransform.rotation;
		mDisabledCameras.Clear();
		Camera[] allCameras = Camera.allCameras;
		foreach (Camera camera in allCameras)
		{
			if (camera != _CutsceneCamera)
			{
				mDisabledCameras.Add(camera);
				camera.gameObject.SetActive(value: false);
			}
		}
		_CutsceneCamera.gameObject.SetActive(value: true);
		base.transform.position = _CutsceneStartPos;
		base.transform.rotation = Quaternion.identity;
		mResetFollowAvatar = SanctuaryManager.pCurPetInstance.gameObject.activeSelf && SanctuaryManager.pCurPetInstance._FollowAvatar;
		if (mResetFollowAvatar)
		{
			SanctuaryManager.pInstance._FollowAvatar = false;
			SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: false);
			SanctuaryManager.pCurPetInstance.SetPosition(new Vector3(0f, -1000f, 0f));
		}
		if (_AgeUpCutsceneData._AvatarMarker != null)
		{
			AvAvatar.SetDisplayNameVisible(inVisible: false);
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component.enabled = false;
			}
			AvAvatar.SetParentTransform(_AgeUpCutsceneData._AvatarMarker.parent);
			AvAvatar.mTransform.localPosition = Vector3.zero;
			AvAvatar.mTransform.localRotation = Quaternion.identity;
			AvAvatar.mTransform.localScale = Vector3.one;
			Animator componentInChildren = AvAvatar.mTransform.GetComponentInChildren<Animator>();
			if (componentInChildren != null)
			{
				componentInChildren.enabled = false;
			}
		}
		if (_AgeUpCutsceneData._DragonMarker != null)
		{
			mPreviousPet.transform.parent = _AgeUpCutsceneData._DragonMarker.parent;
			mPreviousPet.transform.localPosition = Vector3.zero;
			mPreviousPet.transform.localRotation = Quaternion.identity;
			if (mPreviousPet.AIActor != null)
			{
				mPreviousPet.AIActor.SetState(AISanctuaryPetFSM.CINEMATIC);
			}
			mNewPet.transform.parent = _AgeUpCutsceneData._DragonMarker.parent;
			mNewPet.transform.localPosition = Vector3.zero;
			mNewPet.transform.localRotation = Quaternion.identity;
			mNewPet.gameObject.SetActive(value: false);
			mPetSoundChannel = mNewPet.GetComponent<SnChannel>();
			mSnChannelDefaultVolume = mPetSoundChannel.pVolume;
			mNewPet.LoadAnimSfx("Celebrate");
			mSnChannelDefaultRollOff = mPetSoundChannel.pUseRolloffDistance;
			mPetSoundChannel.pUseRolloffDistance = false;
			mPetSoundChannel.pVolume = 1f;
		}
		Animation component2 = GetComponent<Animation>();
		if (component2 != null)
		{
			component2.Play();
		}
		if (_AgeUpCutsceneData._SoundTrack != null)
		{
			SnChannel.Play(_AgeUpCutsceneData._SoundTrack, "SFX_Pool", 0, inForce: true);
		}
	}

	private void OnCutSceneDone()
	{
		if (mNewPet != null)
		{
			mPetSoundChannel.pUseRolloffDistance = mSnChannelDefaultRollOff;
			mPetSoundChannel.pVolume = mSnChannelDefaultVolume;
		}
		ResetAvatar();
		LoadCurrentPet();
	}

	public virtual void OnPlayAnim(string aname)
	{
		if (mNewPet != null)
		{
			mNewPet.PlayAnimSFX(aname, looping: false);
		}
	}

	private void ActivateObjects(bool inActive)
	{
		if (_CutsceneObjects != null && _CutsceneObjects.Length != 0)
		{
			GameObject[] cutsceneObjects = _CutsceneObjects;
			for (int i = 0; i < cutsceneObjects.Length; i++)
			{
				cutsceneObjects[i].SetActive(inActive);
			}
		}
		if (_CutsceneCamera != null)
		{
			_CutsceneCamera.gameObject.SetActive(inActive);
		}
	}

	private void OnAgeUpSwap()
	{
		mPreviousPet.gameObject.SetActive(value: false);
		mNewPet.gameObject.SetActive(value: true);
		mNewPet.PlayAnimation("IdleStand", WrapMode.Loop);
		if (mNewPet.AIActor != null)
		{
			mNewPet.AIActor.SetState(AISanctuaryPetFSM.CINEMATIC);
		}
	}

	private void ResetAvatar()
	{
		ActivateObjects(inActive: false);
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.ActivateAll(active: true);
		}
		AvAvatar.SetParentTransform(null);
		AvAvatar.SetPosition(mOriginalAvatarPosition);
		AvAvatar.mTransform.rotation = mOriginalAvatarRotation;
		AvAvatar.mTransform.localScale = Vector3.one;
		AvAvatarController component = AvAvatar.mTransform.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component.enabled = true;
		}
		AvAvatar.pState = mPrevAvatarState;
		Animator componentInChildren = AvAvatar.mTransform.GetComponentInChildren<Animator>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = true;
		}
		AvAvatar.SetDisplayNameVisible(inVisible: true);
		if (mResetFollowAvatar && SanctuaryManager.pCurPetInstance.mAvatar != null)
		{
			SanctuaryManager.pInstance._FollowAvatar = true;
			SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: true);
		}
		if (mWasAvatarMounted)
		{
			SanctuaryManager.pMountedState = mPrevMountState;
			AvAvatar.pState = AvAvatarState.PAUSED;
		}
		AvAvatar.pObject.SetActive(value: false);
		if (mPreviousPet != null)
		{
			UnityEngine.Object.Destroy(mPreviousPet.gameObject);
		}
		if (mNewPet != null)
		{
			UnityEngine.Object.Destroy(mNewPet.gameObject);
		}
		if (mDisabledCameras.Count <= 0)
		{
			return;
		}
		foreach (Camera mDisabledCamera in mDisabledCameras)
		{
			mDisabledCamera.gameObject.SetActive(value: true);
		}
	}

	private void LoadCurrentPet()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		if (mPetData.RaisedPetID == SanctuaryManager.pCurPetInstance.pData.RaisedPetID)
		{
			SanctuaryManager.pInstance.CreateCurrentPet(SanctuaryManager.pCurPetInstance.pData, RaisedPetData.GetAgeIndex(SanctuaryManager.pCurPetInstance.pData.pStage), base.gameObject);
			if (UiAvatarControls.pInstance != null)
			{
				UiAvatarControls.pInstance.pIsReady = false;
			}
		}
		else
		{
			ShowAgeUpCustomization();
		}
	}

	private void OnPetPictureDone()
	{
		ShowAgeUpCustomization();
	}

	private void OnPetPictureDoneFailed()
	{
		UtDebug.LogError("Failed to get Pet Picture");
		ShowAgeUpCustomization();
	}

	private void ShowAgeUpCustomization()
	{
		if (UiDragonCustomization.pInstance == null)
		{
			string text = GameConfig.GetKeyData("AgeUpCustomizationAsset");
			int[] fUETaskIDs = _FUETaskIDs;
			foreach (int taskID in fUETaskIDs)
			{
				if (MissionManager.pInstance.pActiveTasks.Find((Task x) => x.TaskID == taskID) != null)
				{
					text = _FUEAgeUpCustomizationBundleURL;
					break;
				}
			}
			string[] array = text.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnDragonCustomizationLoaded, typeof(GameObject));
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			OnDragonCustomizationClosed();
		}
	}

	private void PlayBrokenEggShellAnim()
	{
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(SanctuaryManager.pCurrentPetType);
		string value = "";
		if (sanctuaryPetTypeInfo != null)
		{
			value = "Hatch" + sanctuaryPetTypeInfo._Name;
		}
		if (!string.IsNullOrEmpty(value) && _AgeUpCutsceneData._Egg != null)
		{
			_AgeUpCutsceneData._Egg.GetComponent<Animation>().Play(value);
		}
	}

	private void OnDragonCustomizationLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiDragonCustomization";
			UiDragonCustomization component = obj.GetComponent<UiDragonCustomization>();
			component.pPetData = mPetData;
			component.SetUiForJournal(isJournal: false);
			component._MessageObject = base.gameObject;
			KAUICursorManager.SetDefaultCursor("Arrow");
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pAvatarCam.SetActive(value: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			OnDragonCustomizationClosed();
			Debug.LogError("Failed to load Dragon customization....");
			break;
		}
	}

	private void OnDragonCustomizationClosed()
	{
		if (mPrevAvatarActive)
		{
			AvAvatar.pObject.SetActive(value: true);
		}
		AvAvatar.pState = mPrevAvatarState;
		mCallback?.Invoke();
		mCallback = null;
		if (_UIAgeUpCallback != null)
		{
			_UIAgeUpCallback();
		}
		else if (mMessageObj != null)
		{
			mMessageObj.SendMessage("OnDragonCustomizationDone", SendMessageOptions.DontRequireReceiver);
		}
		WsTokenMonitor.pReloadSceneAllowed = true;
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
