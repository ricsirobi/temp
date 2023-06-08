using UnityEngine;

public class SchoolIntro : UserNotify
{
	public static bool _ForcePlayCutscene;

	public string _IntroCutSceneName = "SchoolIntroCutScene";

	public string _SpawnIntoSceneName = "HatcheryINTDO";

	public SnChannel _FlyThroughChannel;

	public SplineControl _CameraSpline;

	public SplineControl _NightFurySpline;

	public Transform _Hiccup;

	public GameObject _RespawnPlane;

	public Vector3 _HiccupOffset = new Vector3(-0.05f, -0.4f, -0.32f);

	private bool mHasPlayedCutscene;

	private bool mIsLevelReady;

	private Transform mSeatBone;

	private NPCHiccup mHiccupScript;

	private SanctuaryPet mNPCToothless;

	private NPCPetManager mPetManager;

	private bool mEnded;

	public void Awake()
	{
		mPetManager = _Hiccup.GetComponent<NPCPetManager>();
		if (mPetManager != null)
		{
			mPetManager.OnPetReadyEvent += OnNPCPetReady;
		}
	}

	public override void OnWaitBeginImpl()
	{
		mIsLevelReady = true;
		if (!_ForcePlayCutscene)
		{
			mHasPlayedCutscene = ProductData.TutorialComplete(_IntroCutSceneName);
		}
		bool flag = UtPlatform.IsMobile();
		if (!mHasPlayedCutscene && !flag)
		{
			MainStreetMMOClient.ConnectToRoom(isConnect: false);
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pAvatarCam.SetActive(value: false);
			AvAvatar.SetDisplayNameVisible(inVisible: false);
			UICursorManager.pCursorManager.SetVisibility(inHide: false);
			if (_FlyThroughChannel != null)
			{
				_FlyThroughChannel.Play();
			}
			_CameraSpline.gameObject.SetActive(value: true);
			_NightFurySpline.gameObject.SetActive(value: true);
			_RespawnPlane.SetActive(value: false);
			MountAvatarRider();
		}
		else if (!mHasPlayedCutscene)
		{
			TutorialManager.MarkTutorialDone(_IntroCutSceneName);
			CleanUp(isActive: true);
			LoadLevel();
		}
		else
		{
			CleanUp(isActive: true);
			base.transform.parent.gameObject.SetActive(value: false);
			OnWaitEnd();
		}
	}

	private void LoadLevel()
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		RsResourceManager.LoadLevel(_SpawnIntoSceneName);
	}

	private void OnNPCPetReady(SanctuaryPet pet)
	{
		if (!mEnded)
		{
			mNPCToothless = pet;
		}
		CleanUp(isActive: false);
	}

	private void CleanUp(bool isActive)
	{
		if (mNPCToothless != null && mNPCToothless.gameObject.activeInHierarchy != isActive)
		{
			mNPCToothless.gameObject.SetActive(isActive);
		}
		if (mPetManager != null)
		{
			mPetManager.OnPetReadyEvent -= OnNPCPetReady;
			mPetManager = null;
		}
		if (AvatarData.pDisplayYourName)
		{
			AvAvatar.SetDisplayNameVisible(inVisible: true);
		}
	}

	private void MountAvatarRider()
	{
		mSeatBone = _NightFurySpline.transform.Find("Main_Root/Root_J/AvatarConstraint_J");
		if (mSeatBone == null)
		{
			mSeatBone = _NightFurySpline.transform.Find("Main_Root/Root_J/Spine_J/Shoulders_J/AvatarConstraint_J");
		}
		if (mSeatBone == null)
		{
			mSeatBone = _NightFurySpline.transform.Find("Main_Root/Root_J/Spine_J/Shoulder_J/AvatarConstraint_J");
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetParentTransform(mSeatBone);
		AvAvatar.mTransform.localRotation = _NightFurySpline.transform.localRotation;
		AvAvatar.mTransform.localPosition = Vector3.zero;
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component.pPlayerMounted = true;
		}
		if (_Hiccup != null)
		{
			_Hiccup.parent = mSeatBone;
			_Hiccup.localPosition = _HiccupOffset;
			_Hiccup.localRotation = _NightFurySpline.transform.rotation;
			mHiccupScript = _Hiccup.GetComponentInChildren<NPCHiccup>();
			if (mHiccupScript != null)
			{
				mHiccupScript.SetState(Character_State.action);
				mHiccupScript.PlayAnim("FlyForward", -1, 1f, 1);
			}
		}
	}

	private void DismountAvatarRider()
	{
		AvAvatar.SetParentTransform(null);
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component.pPlayerMounted = false;
		}
	}

	private void Update()
	{
		if (mIsLevelReady && _CameraSpline.mEndReached)
		{
			EndIntro();
		}
		else if (mHiccupScript != null)
		{
			mHiccupScript.SetPetFollow(inFollow: false);
		}
	}

	private void EndIntro()
	{
		mEnded = true;
		if (_FlyThroughChannel != null)
		{
			_FlyThroughChannel.Stop();
		}
		if (!_ForcePlayCutscene)
		{
			TutorialManager.MarkTutorialDone(_IntroCutSceneName);
		}
		CleanUp(isActive: true);
		MainStreetMMOClient.ConnectToRoom(isConnect: true);
		if (base.transform != null && base.transform.parent != null && base.transform.parent.gameObject != null)
		{
			base.transform.parent.gameObject.SetActive(value: false);
		}
		if (_RespawnPlane != null)
		{
			_RespawnPlane.SetActive(value: true);
		}
		AvAvatar.SetUIActive(inActive: true);
		if (AvAvatar.pAvatarCam != null)
		{
			AvAvatar.pAvatarCam.SetActive(value: true);
		}
		if (UICursorManager.pCursorManager != null)
		{
			UICursorManager.pCursorManager.SetVisibility(inHide: true);
		}
		DismountAvatarRider();
		LoadLevel();
	}
}
