using System;
using System.Collections;
using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class MyRoomsIntMain : KAMonoBase
{
	public class SaveCallbackMessageData
	{
		public GameObject _MessageObject;

		public string _SuccessMessage;

		public string _FailureMessage;

		public SaveCallbackMessageData(GameObject msgObject, string onSuccess, string onFailure)
		{
			_MessageObject = msgObject;
			_SuccessMessage = onSuccess;
			_FailureMessage = onFailure;
		}
	}

	[Serializable]
	public class BuildModeCameraConfig
	{
		public MyRoomObjectType _ObjectType;

		public float _LookAtHeight;

		public float _BuildModeFOV = 45f;

		public Vector3 _BuildModeCameraOffset = new Vector3(0f, 5.5f, -6f);

		public float _BuildModeFocusHeight = 1.5f;
	}

	protected Dictionary<string, MyRoomData> mDictRoomData = new Dictionary<string, MyRoomData>();

	public List<BuildModeCameraConfig> _BuildModeCameraConfig = new List<BuildModeCameraConfig>();

	public int _RoomPairID;

	public MyRoomData[] _Rooms;

	public float _WalkModeFOV = 45f;

	public Vector3 _WalkModeCameraOffset = new Vector3(0f, 3f, -4f);

	public float _WalkModeFocusHeight = 1.5f;

	public float _BuildModeFOV = 45f;

	public Vector3 _BuildModeCameraOffset = new Vector3(0f, 5.5f, -6f);

	public float _BuildModeFocusHeight = 1.5f;

	public GameObject[] _SafeTeleportMarkers;

	public Material _HighlightMaterial;

	public Vector3 _RayCastPointOffset = new Vector3(0f, 0.13f, 0.16f);

	public int _VisitAchievementID = 9;

	public int _VisitAchievementForUserID = 90;

	public int _SocialVisitAchievementID = 104;

	public int _SocialVisitBuddyAchievementID = 113;

	public int _SocialOtherPlayerVisitBuddyAchievementID = 125;

	public Transform _PetBedMarker;

	public UiMyRoomsInt _UiMyRoomsInt;

	public bool _CameraConfigUpdated;

	private static bool mNeedSaving;

	private MyRoomObjectType mSelectedObjectType = MyRoomObjectType.Default;

	protected int mRoomsLoaded;

	protected bool mInitializeRoomData;

	protected bool mLoadedRoomData;

	protected SaveCallbackMessageData mSaveCallbackData;

	private string mHouseWarming_IconURL;

	private KAUIGenericDB mHouseWarmingUI;

	protected UiToolbar mUiToolbar;

	private bool mDefaultItemsInitialized;

	protected ObStatus mObStatus;

	private bool mIsPetInitialized;

	private bool mIsSkyBoxInitialized;

	private Transform mPetSleepMarker;

	private int mAwardDefaultItemsRoomSaveCount;

	private string mAwardDefaultItemsSavingRoom;

	private bool mPetOnBed = true;

	private int mCurrentCreativePoints;

	private int mMaxCreativePoints;

	private static bool mDisableBuildMode;

	private bool mPairDataRequested;

	private bool mPairDataReady;

	private PairData mPairData;

	private List<int> mCategoryIgnoreList = new List<int>();

	private bool mIsBuildMode;

	private static MyRoomsIntMain mInstance;

	private string mCurrentRoomID = "";

	public Dictionary<string, MyRoomData> pDictRoomData => mDictRoomData;

	public MyRoomObjectType pSelectedObjectType
	{
		get
		{
			return mSelectedObjectType;
		}
		set
		{
			mSelectedObjectType = value;
		}
	}

	public int pCurrentCreativePoints => mCurrentCreativePoints;

	public int pMaxCreativePoints
	{
		get
		{
			return mMaxCreativePoints;
		}
		set
		{
			mMaxCreativePoints = value;
		}
	}

	public static bool pDisableBuildMode
	{
		get
		{
			return mDisableBuildMode;
		}
		set
		{
			mDisableBuildMode = value;
		}
	}

	public List<int> pCategoryIgnoreList
	{
		get
		{
			return mCategoryIgnoreList;
		}
		set
		{
			mCategoryIgnoreList = value;
		}
	}

	public int pRoomObjectCount
	{
		get
		{
			if (mDictRoomData.ContainsKey(mInstance.mCurrentRoomID))
			{
				return mDictRoomData[mCurrentRoomID].pRoomObjectCount;
			}
			return 0;
		}
	}

	public bool pIsBuildMode => mIsBuildMode;

	public static MyRoomsIntMain pInstance => mInstance;

	public string pCurrentRoomID => mCurrentRoomID;

	public GameObject pSkyBoxObject
	{
		get
		{
			if (mDictRoomData.ContainsKey(mCurrentRoomID))
			{
				return mDictRoomData[mCurrentRoomID].pSkyBoxObject;
			}
			return null;
		}
	}

	public GameObject pPetBedObject
	{
		get
		{
			if (mDictRoomData.ContainsKey(mCurrentRoomID))
			{
				return mDictRoomData[mCurrentRoomID].pPetBedObject;
			}
			return null;
		}
	}

	public GameObject pDecorationObject
	{
		get
		{
			if (mDictRoomData.ContainsKey(mCurrentRoomID))
			{
				return mDictRoomData[mCurrentRoomID].pDecorationObject;
			}
			return null;
		}
	}

	public GameObject pFloorObject
	{
		get
		{
			if (mDictRoomData.ContainsKey(mCurrentRoomID))
			{
				return mDictRoomData[mCurrentRoomID].pFloorObject;
			}
			return null;
		}
	}

	public GameObject pCushionObject
	{
		get
		{
			if (mDictRoomData.ContainsKey(mCurrentRoomID))
			{
				return mDictRoomData[mCurrentRoomID].pCushionObject;
			}
			return null;
		}
	}

	public GameObject pWallPaperObject
	{
		get
		{
			if (mDictRoomData.ContainsKey(mCurrentRoomID))
			{
				return mDictRoomData[mCurrentRoomID].pWallPaperObject;
			}
			return null;
		}
	}

	public bool pApplyWallpaperAllWalls
	{
		get
		{
			if (mDictRoomData.ContainsKey(mCurrentRoomID))
			{
				return mDictRoomData[mCurrentRoomID]._ApplyWallpaperAllWalls;
			}
			return false;
		}
	}

	public virtual void Save()
	{
		SetBuildMode(inBuildMode: false, new SaveCallbackMessageData(base.gameObject, "OnUserItemSaveSuccess", "OnUserItemSaveError"));
	}

	public virtual void SetBuildMode(bool inBuildMode, SaveCallbackMessageData callbackData = null)
	{
		bool flag = false;
		mSaveCallbackData = callbackData;
		if (mIsBuildMode && !inBuildMode && mNeedSaving)
		{
			mNeedSaving = false;
			flag = true;
			SaveExplicit(callbackData);
		}
		mIsBuildMode = inBuildMode;
		mSelectedObjectType = MyRoomObjectType.Default;
		_CameraConfigUpdated = false;
		if (!flag)
		{
			if (mSaveCallbackData != null)
			{
				OnSave(success: true);
			}
			else
			{
				OnBuildModeChanged(mIsBuildMode);
			}
		}
	}

	public void SaveExplicit(SaveCallbackMessageData callbackData = null)
	{
		mSaveCallbackData = callbackData;
		if (UserItemPositionList.Save(mCurrentRoomID, OnUserItemPositionEvent))
		{
			pDisableBuildMode = true;
		}
		else
		{
			OnSave(success: true);
		}
	}

	public virtual void OnUserItemSaveComplete(bool saveSuccess)
	{
		_UiMyRoomsInt._MyRoomBuilder.OnExit();
		_UiMyRoomsInt.BuildModeClosed();
		OnBuildModeChanged(mIsBuildMode);
	}

	public void OnUserItemSaveSuccess()
	{
		OnUserItemSaveComplete(saveSuccess: true);
	}

	public virtual void OnUserItemSaveError()
	{
	}

	private void OnSaveErrorDBClose()
	{
		OnUserItemSaveComplete(saveSuccess: false);
	}

	public virtual void Start()
	{
		mInstance = this;
		GameObject gameObject = GameObject.Find("PfCommonLevel/PfUiToolbar");
		if (gameObject != null)
		{
			mUiToolbar = gameObject.GetComponent<UiToolbar>();
		}
		mObStatus = GetComponent<ObStatus>();
		if (MainStreetMMOClient.pInstance != null && MyRoomsIntLevel.pInstance != null && MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
		{
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("SHE", ExtensionResponseReceived);
		}
		mPetSleepMarker = new GameObject("petBedMarker").transform;
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
			AvAvatar.pSubState = AvAvatarSubState.NORMAL;
			UnityEngine.Object.Destroy(SanctuaryManager.pCurPetInstance.gameObject);
		}
	}

	private void RoomPairDataEventHandler(bool success, PairData pData, object inUserData)
	{
		mPairDataReady = true;
		mPairData = pData;
	}

	public virtual void OnLevelReady()
	{
		CaAvatarCam component = AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>();
		if (component != null)
		{
			component.mUpdateAvatarCamParam = false;
		}
		if (!(MyRoomsIntLevel.pInstance != null) || !MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
		{
			CheckForHouseWarmingGift();
		}
	}

	public virtual void OnWaitListCompleted()
	{
		if ((!(MyRoomsIntLevel.pInstance != null) || !MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt()) && !mPetOnBed)
		{
			MovePetToBed();
		}
	}

	public bool CheckForHouseWarmingGift()
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			MyRoomData myRoomData = mDictRoomData[mCurrentRoomID];
			if (!mPairData.KeyExists(myRoomData._TutHouseWarming))
			{
				WsUserMessage.pBlockMessages = true;
				mPairData.SetValueAndSave(myRoomData._TutHouseWarming, "1");
				ShowHouseWarming();
				return true;
			}
			PlayTutorial(myRoomData);
		}
		return false;
	}

	public int GetRoomItemSlotCount(string key, int defaultValue)
	{
		if (mPairData != null)
		{
			if (mPairData.KeyExists(key))
			{
				return mPairData.GetIntValue(key, defaultValue);
			}
			return defaultValue;
		}
		return defaultValue;
	}

	public void UpdateRoomItemSlotCount(string key, string value)
	{
		if (mPairData != null)
		{
			mPairData.SetValueAndSave(key, value);
		}
	}

	private void PlayTutorial()
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			PlayTutorial(mDictRoomData[mCurrentRoomID]);
		}
	}

	private void PlayTutorial(MyRoomData inRoomData)
	{
		if (inRoomData == null || !(inRoomData._TutorialTextAsset != null))
		{
			return;
		}
		if (!string.IsNullOrEmpty(inRoomData._LongIntroName))
		{
			if (!TutorialManager.StartTutorial(inRoomData._TutorialTextAsset, inRoomData._LongIntroName, bMarkDone: true, 12u, null) && !string.IsNullOrEmpty(inRoomData._ShortIntroName))
			{
				TutorialManager.StartTutorial(inRoomData._TutorialTextAsset, inRoomData._ShortIntroName, bMarkDone: false, 12u, null);
			}
		}
		else if (!string.IsNullOrEmpty(inRoomData._ShortIntroName))
		{
			TutorialManager.StartTutorial(inRoomData._TutorialTextAsset, inRoomData._ShortIntroName, bMarkDone: false, 12u, null);
		}
	}

	private void CheckForUserVisit()
	{
		WsWebService.SetUserActivity(new UserActivity
		{
			UserID = new Guid(MyRoomsIntLevel.pInstance.GetMyRoomsIntHostID()),
			RelatedUserID = new Guid(UserInfo.pInstance.UserID),
			UserActivityTypeID = 8
		}, UserVisitActivityHandler, null);
	}

	private void UserVisitActivityHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE)
		{
			UserActivity userActivity = (UserActivity)inObject;
			if (userActivity != null)
			{
				Guid? userID = userActivity.UserID;
				UtDebug.Log("USER VISIT ACTIVITY SAVED FOR:: " + userID.ToString());
			}
			else
			{
				UtDebug.LogError("USER VISIT ACTIVITY SAVE FAIL!!!!");
			}
		}
	}

	private void ExtensionResponseReceived(object sender, MMOExtensionResponseReceivedEventArgs args)
	{
		if (args.ResponseDataObject["0"].ToString() == "SHE" && mRoomsLoaded == _Rooms.Length)
		{
			Reset();
			string text = (args.ResponseDataObject.ContainsKey("4") ? args.ResponseDataObject["4"].ToString() : "");
			int result = -1;
			int.TryParse(text, out result);
			if (result < 0)
			{
				text = "";
			}
			if (MyRoomsIntLevel.pInstance != null && MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
			{
				UserItemPositionList.Init(MyRoomsIntLevel.pInstance.GetMyRoomsIntHostID(), text, OnUserItemPositionEvent);
			}
			else
			{
				UserItemPositionList.Init(UserInfo.pInstance.UserID, text, OnUserItemPositionEvent);
			}
		}
	}

	public virtual void Reset()
	{
		mRoomsLoaded = 0;
	}

	public void OnStoreClosed()
	{
		PetBundleLoader.DetachPetsForAvatar(AvAvatar.pObject);
	}

	public virtual void LoadRoomData()
	{
		mInitializeRoomData = true;
		mLoadedRoomData = true;
	}

	public virtual void Update()
	{
		UpdateCamera();
		if (UserInfo.pIsReady && !mPairDataRequested)
		{
			mPairDataRequested = true;
			if (MyRoomsIntLevel.pInstance != null && !MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
			{
				PairData.Load(_RoomPairID, RoomPairDataEventHandler, null);
			}
			else
			{
				mPairDataReady = true;
			}
		}
		if (!mInitializeRoomData && CommonInventoryData.pIsReady && UserInfo.pIsReady)
		{
			LoadRoomData();
		}
		if (!mDefaultItemsInitialized && CommonInventoryData.pIsReady && UserInfo.pIsReady && MyRoomsIntLevel.pInstance != null && mPairDataReady && mLoadedRoomData)
		{
			mDefaultItemsInitialized = true;
			mRoomsLoaded = 0;
			MyRoomData[] rooms = _Rooms;
			foreach (MyRoomData myRoomData in rooms)
			{
				myRoomData.Initialize();
				mDictRoomData.Add(myRoomData._RoomID, myRoomData);
			}
			if (MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
			{
				string myRoomsIntHostID = MyRoomsIntLevel.pInstance.GetMyRoomsIntHostID();
				rooms = _Rooms;
				foreach (MyRoomData myRoomData2 in rooms)
				{
					UserItemPositionList.Init(myRoomsIntHostID, myRoomData2._RoomID, OnUserItemPositionEvent);
				}
				CheckForUserVisit();
				SetVisitAchievements();
				SetLoadComplete();
			}
			else
			{
				bool flag = false;
				bool flag2 = false;
				rooms = _Rooms;
				foreach (MyRoomData myRoomData3 in rooms)
				{
					flag2 = false;
					if (!string.IsNullOrEmpty(myRoomData3._TutHouseWarming) && !mPairData.KeyExists(myRoomData3._TutHouseWarming) && myRoomData3._HouseWarmingItem > 0)
					{
						CommonInventoryData.pInstance.AddItem(myRoomData3._HouseWarmingItem, updateServer: false);
						mPairData.SetValueAndSave(myRoomData3._TutHouseWarming, "1");
					}
					MyRoomDefaultItems[] defaultItems = myRoomData3._DefaultItems;
					foreach (MyRoomDefaultItems myRoomDefaultItems in defaultItems)
					{
						if (mPairData.KeyExists(myRoomDefaultItems._TutName))
						{
							continue;
						}
						MyRoomDefaultItem[] items = myRoomDefaultItems._Items;
						foreach (MyRoomDefaultItem myRoomDefaultItem in items)
						{
							CommonInventoryData.pInstance.AddItem(myRoomDefaultItem._ItemID, updateServer: false);
							if (!myRoomDefaultItem._InventoryOnly)
							{
								flag2 = true;
							}
						}
						if (myRoomDefaultItems._Items.Length != 0)
						{
							flag = true;
						}
					}
					if (!flag2)
					{
						UserItemPositionList.Init(UserInfo.pInstance.UserID, myRoomData3._RoomID, OnUserItemPositionEvent);
					}
				}
				if (flag)
				{
					CommonInventoryData.pInstance.Save(AwardDefaultItemsEventHandler, null);
				}
			}
		}
		if (!mIsPetInitialized && mRoomsLoaded == _Rooms.Length && mAwardDefaultItemsRoomSaveCount == 0)
		{
			if (SanctuaryManager.pInstance != null && SanctuaryManager.IsActivePetDataReady())
			{
				if (MyRoomsIntLevel.pInstance != null && !MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
				{
					if (SanctuaryManager.IsPetInstanceAllowed(SanctuaryManager.pCurPetData))
					{
						SanctuaryManager.CreatePet(SanctuaryManager.pCurPetData, SanctuaryManager.pInstance._PetOffScreenPosition, Quaternion.identity, base.gameObject, "Basic");
					}
					else
					{
						SetLoadComplete();
					}
				}
				else
				{
					MainStreetMMOClient.pInstance.SetRaisedPetString("");
				}
				mIsPetInitialized = true;
			}
			else if (SanctuaryManager.pCurPetData == null)
			{
				SetLoadComplete();
				mIsPetInitialized = true;
			}
		}
		if (!mIsSkyBoxInitialized && mRoomsLoaded == _Rooms.Length)
		{
			mIsSkyBoxInitialized = true;
			DisableAllSkyBoxes();
			if (_Rooms.Length != 0)
			{
				mCurrentRoomID = _Rooms[0]._RoomID;
			}
			ChangeRoom(mCurrentRoomID);
		}
		if (mAwardDefaultItemsRoomSaveCount > 0 && mAwardDefaultItemsSavingRoom == null)
		{
			mAwardDefaultItemsSavingRoom = _Rooms[_Rooms.Length - mAwardDefaultItemsRoomSaveCount]._RoomID;
			if (!UserItemPositionList.Save(mAwardDefaultItemsSavingRoom, OnUserItemPositionEvent))
			{
				mAwardDefaultItemsSavingRoom = null;
			}
			mAwardDefaultItemsRoomSaveCount--;
		}
		if (mPetOnBed)
		{
			return;
		}
		SanctuaryPet pCurPetInstance = SanctuaryManager.pCurPetInstance;
		if ((bool)pCurPetInstance && Vector3.Distance(pCurPetInstance.transform.position, _PetBedMarker.position) < 1f)
		{
			pCurPetInstance.SetAvatar(AvAvatar.mTransform);
			pCurPetInstance.MoveToDone(_PetBedMarker);
			pCurPetInstance.SetState(Character_State.idle);
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = AvAvatarState.IDLE;
			mPetOnBed = true;
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.CheckForTaskCompletion("Visit", RsResourceManager.pCurrentLevel, 0);
			}
			PositionPetOnBed();
		}
	}

	protected virtual void UpdateCamera()
	{
		if (mIsBuildMode)
		{
			foreach (BuildModeCameraConfig item in _BuildModeCameraConfig)
			{
				if (mSelectedObjectType == item._ObjectType)
				{
					PanCamera(item._BuildModeCameraOffset, item._BuildModeFocusHeight, item._BuildModeFOV, item._LookAtHeight);
					break;
				}
			}
			return;
		}
		PanCamera(_WalkModeCameraOffset, _WalkModeFocusHeight, _WalkModeFOV);
	}

	private void GetPetHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			if (inType != WsServiceType.GET_ACTIVE_RAISED_PETS_BY_TYPEIDS)
			{
				break;
			}
			RaisedPetData[] array = (RaisedPetData[])inObject;
			if (array != null && array.Length != 0)
			{
				RaisedPetData.ResolvePetArray(array, isactive: true);
				if (SanctuaryManager.IsPetInstanceAllowed(array[0]))
				{
					SanctuaryManager.CreatePet(array[0], Vector3.zero, Quaternion.identity, base.gameObject, "Basic");
				}
				else
				{
					SetLoadComplete();
				}
			}
			else
			{
				SetLoadComplete();
			}
			break;
		}
		case WsServiceEvent.ERROR:
			UtDebug.Log("Web Servical Error for type:" + inType);
			SetLoadComplete();
			break;
		}
	}

	public virtual void OnPetReady(SanctuaryPet pet)
	{
		pet.OnFlyDismountImmediate(AvAvatar.pObject);
		SanctuaryManager.pCurPetInstance = pet;
		SanctuaryManager.pCurPetData = pet.pData;
		pet.pMeterPaused = true;
		pet.SetFollowAvatar(follow: false);
		pet.SetFidgetOnOff(isOn: false);
		pet.SetClickableActive(active: false);
		StartCoroutine("DelayedPositionPetOnBed");
		pet.AIActor.SetState(AISanctuaryPetFSM.REST);
	}

	public IEnumerator DelayedPositionPetOnBed()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		SetLoadComplete();
		if (mPetOnBed)
		{
			PositionPetOnBed();
		}
	}

	public void PositionPetOnBed()
	{
		PositionPetOnBed(SanctuaryManager.pCurPetInstance, petUsingTOW: true);
	}

	public void PositionPetOnBed(SanctuaryPet pet, bool petUsingTOW)
	{
		if (!(pet != null) || !(_PetBedMarker != null))
		{
			return;
		}
		if (pet.pData.pIsSleeping)
		{
			if (MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
			{
				pet.DoWakeUp();
			}
			else
			{
				SanctuaryManager.pInstance.SendPetToBed(pet);
			}
		}
		Vector3 vector = new Vector3(0f, -5000f, 0f);
		Quaternion rotation = Quaternion.identity;
		vector = _PetBedMarker.transform.position;
		if (petUsingTOW)
		{
			rotation = _PetBedMarker.transform.rotation;
		}
		if (Physics.Raycast(new Ray(vector + new Vector3(0f, 3f, 0f), -Vector3.up), out var hitInfo, float.PositiveInfinity, UtUtilities.GetGroundRayCheckLayers()))
		{
			vector = hitInfo.point;
			mPetSleepMarker.transform.position = vector - new Vector3(0f, 0.25f, 0f);
			mPetSleepMarker.transform.rotation = rotation;
		}
		pet.transform.position = vector;
		pet.transform.rotation = rotation;
		SanctuaryManager.pInstance.HandleZzzParticles(pet.pData.pIsSleeping, pet);
		SanctuaryManager.pInstance._PetSleepMarkerMale = mPetSleepMarker;
		SanctuaryManager.pInstance._PetSleepMarkerFemale = mPetSleepMarker;
	}

	public void InitMoveToBed()
	{
		if (MyRoomsIntLevel.pInstance != null && !MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
		{
			mPetOnBed = false;
		}
	}

	private void MovePetToBed()
	{
		SanctuaryPet pCurPetInstance = SanctuaryManager.pCurPetInstance;
		if (pCurPetInstance != null)
		{
			pCurPetInstance.SetAvatar(AvAvatar.mTransform);
			pCurPetInstance.SetFollowAvatar(follow: false);
			pCurPetInstance.MoveToAvatar();
			pCurPetInstance._NoWalk = false;
			pCurPetInstance.SetAvatar(null);
			pCurPetInstance.DoMoveTo(_PetBedMarker, endAlign: true, fly: false);
			pCurPetInstance.SetMoveSpeed(pCurPetInstance._RunSpeed * 0.5f);
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
		}
	}

	public void PanCamera(Vector3 offsetPosition, float focusHeight, float inFOV, float lookAtHeight = 0f)
	{
		if (!_CameraConfigUpdated && AvAvatar.pAvatarCam != null)
		{
			CaAvatarCam obj = (CaAvatarCam)AvAvatar.pAvatarCam.GetComponent(typeof(CaAvatarCam));
			obj.SetPosition(offsetPosition, focusHeight, 0f, lookAtHeight);
			obj.camera.fieldOfView = inFOV;
			_CameraConfigUpdated = true;
		}
	}

	public static void EnableRoomObjectsClickable(bool isEnable, bool isEnableRoomItem = true)
	{
		if (mInstance != null && mInstance.mDictRoomData.ContainsKey(mInstance.mCurrentRoomID))
		{
			if (MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
			{
				isEnable = false;
				isEnableRoomItem = false;
			}
			mInstance.mDictRoomData[mInstance.mCurrentRoomID].EnableRoomObjectsClickable(isEnable, isEnableRoomItem);
		}
	}

	public virtual void AwardDefaultItemsEventHandler(bool inSaveSuccess, object inUserData)
	{
		if (inSaveSuccess)
		{
			if (_Rooms != null && _Rooms.Length != 0)
			{
				mAwardDefaultItemsRoomSaveCount = _Rooms.Length;
				mAwardDefaultItemsSavingRoom = null;
			}
			string text = mCurrentRoomID;
			MyRoomData[] rooms = _Rooms;
			foreach (MyRoomData myRoomData in rooms)
			{
				MyRoomDefaultItems[] defaultItems = myRoomData._DefaultItems;
				foreach (MyRoomDefaultItems myRoomDefaultItems in defaultItems)
				{
					if (mPairData.KeyExists(myRoomDefaultItems._TutName))
					{
						continue;
					}
					mPairData.SetValue(myRoomDefaultItems._TutName, "1");
					mCurrentRoomID = myRoomData._RoomID;
					MyRoomDefaultItem[] items = myRoomDefaultItems._Items;
					foreach (MyRoomDefaultItem myRoomDefaultItem in items)
					{
						if (!myRoomDefaultItem._InventoryOnly)
						{
							UserItemData userItemData = CommonInventoryData.pInstance.FindItem(myRoomDefaultItem._ItemID);
							if (userItemData != null)
							{
								AddRoomObject(new GameObject(), userItemData, myRoomDefaultItem._Position, new Vector3(0f, myRoomDefaultItem._RotationY, 0f), null, isUpdateLocalList: true);
								CommonInventoryData.pInstance.RemoveItem(userItemData, 1);
							}
						}
					}
				}
			}
			PairData.Save(_RoomPairID);
			mNeedSaving = false;
			mCurrentRoomID = text;
		}
		else
		{
			MyRoomData[] rooms = _Rooms;
			foreach (MyRoomData myRoomData2 in rooms)
			{
				UserItemPositionList.Init(UserInfo.pInstance.UserID, myRoomData2._RoomID, OnUserItemPositionEvent);
			}
		}
	}

	public void AttachRaycastPoints(GameObject go)
	{
		MyRoomObject component = go.GetComponent<MyRoomObject>();
		if (!(component != null))
		{
			return;
		}
		component.pRayCastPoints = null;
		if (component.collider != null)
		{
			Vector3 center = component.collider.bounds.center;
			Vector3 extents = component.collider.bounds.extents;
			if (component._ObjectType == MyRoomObjectType.WallHanging)
			{
				component.pRayCastPoints = new GameObject[3];
				component.pRayCastPoints[0] = new GameObject("WH_RayTopLeft");
				component.pRayCastPoints[0].transform.position = center + new Vector3(0f - extents.x, extents.y, 0f - extents.z + _RayCastPointOffset.z);
				component.pRayCastPoints[0].transform.parent = go.transform;
				component.pRayCastPoints[1] = new GameObject("WH_RayTopRight");
				component.pRayCastPoints[1].transform.position = center + new Vector3(extents.x, extents.y, 0f - extents.z + _RayCastPointOffset.z);
				component.pRayCastPoints[1].transform.parent = go.transform;
				component.pRayCastPoints[2] = new GameObject("WH_RayCenter");
				component.pRayCastPoints[2].transform.position = center + new Vector3(0f, 0f - extents.y + _RayCastPointOffset.y, 0f);
				component.pRayCastPoints[2].transform.parent = go.transform;
			}
			else if (component._ObjectType == MyRoomObjectType.OnFloor)
			{
				component.pRayCastPoints = new GameObject[1];
				component.pRayCastPoints[0] = new GameObject("F_RayCenterBottom");
				component.pRayCastPoints[0].transform.position = center + new Vector3(0f, 0f - extents.y + _RayCastPointOffset.y, 0f);
				component.pRayCastPoints[0].transform.parent = go.transform;
			}
		}
	}

	public virtual void OnUserItemPositionEvent(WsServiceType inType, WsServiceEvent inEvent, string inRoomID)
	{
		switch (inType)
		{
		case WsServiceType.SET_USER_ITEM_POSITION_LIST:
			switch (inEvent)
			{
			case WsServiceEvent.ERROR:
				pDisableBuildMode = false;
				mAwardDefaultItemsSavingRoom = null;
				OnSave(success: false);
				break;
			case WsServiceEvent.COMPLETE:
				pDisableBuildMode = false;
				OnSave(success: true);
				mAwardDefaultItemsSavingRoom = null;
				if (mObStatus != null && !mObStatus.pIsReady)
				{
					UserItemPositionList.Init(UserInfo.pInstance.UserID, inRoomID, OnUserItemPositionEvent);
				}
				break;
			}
			break;
		case WsServiceType.GET_USER_ITEM_POSITION_LIST:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (mDictRoomData.ContainsKey(inRoomID))
				{
					mDictRoomData[inRoomID].RecreateRoomItems();
				}
				break;
			case WsServiceEvent.ERROR:
				if (mDictRoomData.ContainsKey(inRoomID))
				{
					mDictRoomData[inRoomID].ShowDefaultRoomItems();
				}
				break;
			}
			break;
		}
	}

	public virtual void OnSave(bool success)
	{
		if (mSaveCallbackData != null && mSaveCallbackData._MessageObject != null)
		{
			mSaveCallbackData._MessageObject.SendMessage(success ? mSaveCallbackData._SuccessMessage : mSaveCallbackData._FailureMessage, SendMessageOptions.DontRequireReceiver);
			mSaveCallbackData = null;
		}
	}

	public void SafeTeleportAvatar()
	{
		if (!(MyRoomsIntLevel.pInstance != null) || !MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
		{
			return;
		}
		if (_SafeTeleportMarkers.Length != 0)
		{
			int num = UnityEngine.Random.Range(0, _SafeTeleportMarkers.Length);
			Transform transform = _SafeTeleportMarkers[num].transform;
			if (!GameObject.Find("PfUiSelectAvatar"))
			{
				AvAvatar.TeleportTo(transform.position, transform.forward);
			}
		}
		else
		{
			Debug.LogError("ERROR: No safe teleport markers are defined for MyRoomsIntMain!!");
		}
	}

	private void OnMapMyHouseBtnClick()
	{
		if (MainStreetMMOClient.pInstance != null)
		{
			AvAvatar.SetUIActive(inActive: false);
			MainStreetMMOClient.pInstance.JoinOwnerSpace("MyPodInt", UserInfo.pInstance.UserID);
		}
		else
		{
			AvAvatar.SetUIActive(inActive: true);
		}
	}

	public virtual void AddRoomObject(GameObject inGameObject, UserItemData inUserItemData, GameObject inParentData, bool isUpdateLocalList)
	{
		mNeedSaving = true;
		if (mDictRoomData.ContainsKey(mInstance.mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].AddRoomObject(inGameObject, inUserItemData, inParentData, isUpdateLocalList);
		}
		ObClickable component = inGameObject.GetComponent<ObClickable>();
		if (component != null)
		{
			component._Active = true;
		}
	}

	public void AddRoomObject(GameObject inGameObject, UserItemData inUserItemData, Vector3 inPosition, Vector3 inRotation, GameObject inParentData, bool isUpdateLocalList)
	{
		mNeedSaving = true;
		if (mDictRoomData.ContainsKey(mInstance.mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].AddRoomObject(inGameObject, inUserItemData, inPosition, inRotation, inParentData, isUpdateLocalList);
		}
	}

	public virtual void UpdateRoomObject(GameObject inGameObject, UserItemData inUserItemData, GameObject inParentData)
	{
		mNeedSaving = true;
		UserItemPositionList.UpdateObject(mCurrentRoomID, inGameObject, inUserItemData, inParentData);
		MyRoomObject component = inGameObject.GetComponent<MyRoomObject>();
		if (!(component != null))
		{
			return;
		}
		MyRoomObject.ChildData[] pChildList = component.pChildList;
		foreach (MyRoomObject.ChildData childData in pChildList)
		{
			MyRoomObject component2 = childData._Child.GetComponent<MyRoomObject>();
			if (component2 != null)
			{
				UserItemPositionList.UpdateObject(mCurrentRoomID, childData._Child, component2.pUserItemData, component2.pParentObject);
			}
		}
	}

	public virtual void RemoveRoomObject(GameObject inObject, bool isDestroy)
	{
		mNeedSaving = true;
		if (mDictRoomData.ContainsKey(mInstance.mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].RemoveRoomObject(inObject, isDestroy);
		}
	}

	private void ShowHouseWarming()
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			MyRoomData myRoomData = mDictRoomData[mCurrentRoomID];
			if (myRoomData._HouseWarmingItem > 0)
			{
				AvAvatar.SetUIActive(inActive: false);
				AvAvatar.pInputEnabled = false;
				if (mUiToolbar != null)
				{
					mUiToolbar.mIdleMgr.StopIdles();
				}
				if (myRoomData._HouseWarmingClip != null)
				{
					SnChannel.Play(myRoomData._HouseWarmingClip, "VO_Pool", inForce: true);
				}
				GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUiGenericDB"));
				mHouseWarmingUI = gameObject.GetComponent<KAUIGenericDB>();
				mHouseWarmingUI.SetPriority(15);
				mHouseWarmingUI._MessageObject = base.gameObject;
				mHouseWarmingUI.SetTextByID(myRoomData._HouseWarmingText._ID, myRoomData._HouseWarmingText._Text, interactive: false);
				mHouseWarmingUI.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: true);
				mHouseWarmingUI._OKMessage = "OnHouseWarmingExit";
				mHouseWarmingUI._CloseMessage = "OnHouseWarmingExit";
				mHouseWarmingUI.FindItem("TxtDialog").SetPosition(myRoomData._HouseWarmingTextPos.x, myRoomData._HouseWarmingTextPos.y);
				ItemData.Load(myRoomData._HouseWarmingItem, OnRewardItemDataEventHandler, null);
			}
			else
			{
				OnHouseWarmingExit();
			}
		}
		else
		{
			OnHouseWarmingExit();
		}
	}

	private void OnRewardItemDataEventHandler(int itemID, ItemData dataItem, object inUserData)
	{
		if (dataItem != null)
		{
			mHouseWarming_IconURL = dataItem.IconName;
			string[] array = dataItem.IconName.Split('/');
			RsResourceManager.Load(array[0] + "/" + array[1], OnRewardIconLoadEvent);
		}
	}

	private void OnRewardIconLoadEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE && mHouseWarmingUI != null && mHouseWarming_IconURL != null)
		{
			MyRoomData myRoomData = mDictRoomData[mCurrentRoomID];
			KAWidget kAWidget = mHouseWarmingUI.FindItem("OKBtn");
			kAWidget.SetPosition(myRoomData._HouseWarmingOKBtnPos.x, myRoomData._HouseWarmingOKBtnPos.y);
			kAWidget.SetVisibility(inVisible: true);
			Texture2D inTexture = (Texture2D)RsResourceManager.LoadAssetFromBundle(mHouseWarming_IconURL);
			KAWidget kAWidget2 = mHouseWarmingUI.AddWidget(typeof(KAWidget), inTexture, Shader.Find("Unlit/Transparent Colored"), "Icon");
			kAWidget2.SetPosition(myRoomData._HouseWarmingIconPos.x, myRoomData._HouseWarmingIconPos.y);
			kAWidget2.SetScale(new Vector3(myRoomData._HouseWarmingIconScale.x, myRoomData._HouseWarmingIconScale.y, 1f));
		}
	}

	private void OnHouseWarmingExit()
	{
		if (mHouseWarmingUI != null)
		{
			UnityEngine.Object.Destroy(mHouseWarmingUI.gameObject);
		}
		WsUserMessage.pBlockMessages = false;
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pInputEnabled = true;
		if (mUiToolbar != null)
		{
			mUiToolbar.mIdleMgr.StartIdles();
		}
		PlayTutorial();
	}

	public void RemoveUnwantedLayers(GameObject inObject)
	{
		string[] array = new string[1] { "Marker" };
		foreach (string layerName in array)
		{
			foreach (Transform item in inObject.transform)
			{
				if (item.gameObject.layer == LayerMask.NameToLayer(layerName))
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
				else
				{
					RemoveUnwantedLayers(item.gameObject);
				}
			}
		}
	}

	public virtual void UpdateRoomLoadedCount()
	{
		mRoomsLoaded++;
	}

	public void SetLoadComplete()
	{
		if (mObStatus != null)
		{
			SafeTeleportAvatar();
			DismountAvatar();
			mObStatus.pIsReady = true;
		}
	}

	public void RemoveSkyBox()
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].RemoveSkyBox();
		}
	}

	public void AddSkyBox(UserItemData inItemData, string inAssetName)
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].AddSkyBox(inItemData, inAssetName);
		}
	}

	public void AddPetBedObject(UserItemData inItemData, string inAssetName, ItemDataTexture[] inTexture)
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].AddPetBedObject(inItemData, inAssetName, inTexture);
		}
	}

	public void AddFloorObject(UserItemData inItemData, string inAssetName)
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].AddFloorObject(inItemData, inAssetName);
		}
	}

	public void AddCushionObject(UserItemData inItemData, string inAssetName)
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].AddCushionObject(inItemData, inAssetName);
		}
	}

	public void AddWallPaperObject(UserItemData inItemData, string inAssetName)
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].AddWallPaperObject(inItemData, inAssetName);
		}
	}

	public void AddWindow(bool isUpdateServer, UserItemPosition inUserItemPosition, UserItemData inItemData)
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].AddWindow(isUpdateServer, inUserItemPosition, inItemData);
		}
	}

	public void AddDecoration(UserItemData inItemData, string inAssetName, ItemDataTexture[] inTexture, bool inUseTranform, Vector3 inPosition, Quaternion inRotation)
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].AddDecoration(inItemData, inAssetName, inTexture, inUseTranform, inPosition, inRotation);
		}
	}

	public void ApplyWallPaper(GameObject inObject, Texture inTex, bool isTemp)
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].ApplyWallPaper(inObject, inTex, isTemp);
		}
	}

	public void RevertToPrevWallPaper()
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].RevertToPrevWallPaper();
		}
	}

	public GameObject ReturnSelectedWall(GameObject inObject)
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			return mDictRoomData[mCurrentRoomID].ReturnSelectedWall(inObject);
		}
		return null;
	}

	public void ChangeRoom(string inRoomID)
	{
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].ToggleSkyBox(active: false);
		}
		mCurrentRoomID = inRoomID;
		if (mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			mDictRoomData[mCurrentRoomID].ToggleSkyBox(active: true);
		}
	}

	private void DisableAllSkyBoxes()
	{
		MyRoomData[] rooms = _Rooms;
		foreach (MyRoomData myRoomData in rooms)
		{
			if (mDictRoomData.ContainsKey(myRoomData._RoomID))
			{
				mDictRoomData[myRoomData._RoomID].ToggleSkyBox(active: false);
			}
		}
	}

	private void DismountAvatar()
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			if (component.pIsPlayerGliding)
			{
				component.OnGlideLanding();
			}
			component.SoftReset();
			component.pState = AvAvatarState.IDLE;
		}
	}

	public void SetVisitAchievements()
	{
		if (MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
		{
			string myRoomsIntHostID = MyRoomsIntLevel.pInstance.GetMyRoomsIntHostID();
			List<AchievementTask> list = new List<AchievementTask>();
			list.Add(new AchievementTask(_VisitAchievementID, myRoomsIntHostID));
			list.Add(new AchievementTask(myRoomsIntHostID, _VisitAchievementForUserID, UserInfo.pInstance.UserID));
			if (BuddyList.pIsReady && BuddyList.pInstance.GetBuddyStatus(myRoomsIntHostID) == BuddyStatus.Approved)
			{
				list.Add(new AchievementTask(_SocialVisitBuddyAchievementID, myRoomsIntHostID));
				list.Add(new AchievementTask(myRoomsIntHostID, _SocialOtherPlayerVisitBuddyAchievementID, UserInfo.pInstance.UserID));
			}
			else
			{
				list.Add(new AchievementTask(_SocialVisitAchievementID, myRoomsIntHostID));
				list.Add(new AchievementTask(myRoomsIntHostID, _SocialVisitAchievementID, UserInfo.pInstance.UserID));
			}
			UserAchievementTask.Set(list.ToArray());
		}
	}

	public virtual Vector3 CalculatePosition(Vector3 inPos, Transform inObj)
	{
		return inPos;
	}

	public virtual Vector3 CalculatePosition(Vector3 inPos)
	{
		return CalculatePosition(inPos, null);
	}

	public virtual void ObjectCreatedCallback(GameObject inObject, UserItemData inItemData, bool inSaved)
	{
		if (!(inObject == null))
		{
			MyRoomItem component = inObject.GetComponent<MyRoomItem>();
			if (component?.pClickable != null && !pIsBuildMode)
			{
				component.pClickable._Active = true;
			}
		}
	}

	public virtual void ObjectDestroyCallback(GameObject inObject)
	{
	}

	public virtual void OnBuildModeChanged(bool inBuildMode)
	{
		if (!mDictRoomData.ContainsKey(mCurrentRoomID))
		{
			return;
		}
		MyRoomData myRoomData = mDictRoomData[mCurrentRoomID];
		if (myRoomData == null || myRoomData.pRoomObjectCount <= 0)
		{
			return;
		}
		foreach (GameObject pGameObjectReference in myRoomData.pGameObjectReferenceList)
		{
			pGameObjectReference.BroadcastMessage("OnBuildModeChanged", inBuildMode, SendMessageOptions.DontRequireReceiver);
		}
	}

	public virtual void UpdateCreativePoints(int amount)
	{
		mCurrentCreativePoints += amount;
	}

	public bool HasCreativePointsLimitReached(int additionalPoints)
	{
		return mCurrentCreativePoints + additionalPoints > mMaxCreativePoints;
	}
}
