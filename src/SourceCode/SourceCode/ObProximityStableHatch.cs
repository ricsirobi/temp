using System;
using System.Collections.Generic;
using UnityEngine;

public class ObProximityStableHatch : ObProximity
{
	public static bool pIsHatching;

	public int _HatchingAchievementID = 201387;

	public int _AssignToNestAchievementID = 627;

	public int _NestAllocationAchievementID = 201713;

	public HatcheryManager _HatcheryManager;

	public LocaleString _AssignPetText = new LocaleString("Do you want to assign the Pet to Nest?");

	public LocaleString _BuyStablesText = new LocaleString("You dont have any available Nests.DO you want to buy a Stable ?");

	public LocaleString _AssignPetTitleText = new LocaleString("Assign Nest");

	public LocaleString _BuyStablesTitleText = new LocaleString("Buy Stable");

	public string _DragonCustomizationAsset = "RS_DATA/PfUiDragonCustomizationDO.unity3d/PfUiDragonCustomizationDO";

	public CutSceneData[] _BondingCutscenes;

	public StoreLoader.Selection _StoreInfo;

	[SerializeField]
	private bool m_AllowPetFollow;

	[SerializeField]
	private Vector3 m_DragonStartOffset = Vector3.zero;

	private KAUIGenericDB mKAUIGenericDB;

	private int mCurrentBondingCutSceneIdx = -1;

	private Vector3 mOriginalAvatarPosition = Vector3.zero;

	private Quaternion mOriginalAvatarRotation = Quaternion.identity;

	private bool mShowCutScene;

	private Vector3 mPlatformPos = Vector3.zero;

	private RaisedPetData mPrevPetData;

	private bool mIsPrevHatchedPet;

	[SerializeField]
	private bool m_CheckIncubatorState;

	private SanctuaryPet mCurrentPetOnPedestal;

	private RaisedPetData mPedestalPetData;

	private UiDragonsStable mUiStable;

	private bool mIsCustomizationNeeded;

	private bool mWaitForWaitListToComplete;

	private bool mCheckedForTaskCompletion = true;

	public GameObject pPedestalObject { get; set; }

	private void OnPetAssignedToNest(int petID)
	{
		if (mPedestalPetData != null && mPedestalPetData.RaisedPetID == petID)
		{
			CheckPetHasNest();
		}
	}

	private void Start()
	{
		mWaitForWaitListToComplete = true;
		StableManager.OnPetMovedToNest = (PetMovedToNest)Delegate.Combine(StableManager.OnPetMovedToNest, new PetMovedToNest(OnPetAssignedToNest));
		CoCommonLevel component = GameObject.Find("PfCommonLevel").GetComponent<CoCommonLevel>();
		if (component != null && component.pWaitListCompleted)
		{
			WaitListCompleted();
		}
		else
		{
			CoCommonLevel.WaitListCompleted += WaitListCompleted;
		}
	}

	private void OnEnable()
	{
		pIsHatching = false;
	}

	private void OnDestroy()
	{
		StableManager.OnPetMovedToNest = (PetMovedToNest)Delegate.Remove(StableManager.OnPetMovedToNest, new PetMovedToNest(OnPetAssignedToNest));
		CoCommonLevel.WaitListCompleted -= WaitListCompleted;
	}

	public void OnActivate()
	{
		if (mPedestalPetData != null && StableData.GetByPetID(mPedestalPetData.RaisedPetID) == null && !pIsHatching)
		{
			AssignNest();
		}
	}

	public override void Update()
	{
		if ((!_UseGlobalActive || ObClickable.pGlobalActive) && _Active && !(AvAvatar.pObject == null) && _Range != 0f && !pIsHatching && !RsResourceManager.pLevelLoadingScreen && !mWaitForWaitListToComplete && m_CheckIncubatorState)
		{
			Incubator currentIncubator = _HatcheryManager.GetCurrentIncubator();
			CheckIncubatorState(currentIncubator);
			m_CheckIncubatorState = false;
		}
	}

	public void CheckIncubatorState(Incubator incubator)
	{
		if (incubator != null && incubator.pMyState == Incubator.IncubatorStates.IDLE)
		{
			mPedestalPetData = RaisedPetData.GetHatchingPet(incubator.pID);
			if (!CheckPetHasNest())
			{
				mIsPrevHatchedPet = true;
				SanctuaryManager.CreatePet(mPedestalPetData, incubator.transform.position + incubator.pDragonPositionOffset, incubator.transform.rotation, incubator.gameObject, "Full");
			}
		}
	}

	public bool IsEmptyNestAvailable()
	{
		bool result = false;
		StableData.UpdateInfo();
		for (int i = 0; i < StableData.pStableList.Count; i++)
		{
			if (StableData.pStableList[i].GetEmptyNest() != null)
			{
				result = true;
			}
		}
		return result;
	}

	public void ShowAssignNestPopUp()
	{
		ShowDB();
		mKAUIGenericDB.SetText(_AssignPetText.GetLocalizedString(), interactive: false);
		mKAUIGenericDB.SetTitle(_AssignPetTitleText.GetLocalizedString());
		mKAUIGenericDB._YesMessage = "AssignNest";
		mKAUIGenericDB._NoMessage = "No";
	}

	private void OpenStore()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		if (mKAUIGenericDB != null)
		{
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
		}
		if (_StoreInfo != null)
		{
			StoreLoader.Load(setDefaultMenuItem: true, _StoreInfo._Category, _StoreInfo._Store, base.gameObject);
		}
	}

	public void OnStoreClosed()
	{
		if (mPedestalPetData != null && IsEmptyNestAvailable())
		{
			AssignNest();
		}
	}

	private void AssignNest()
	{
		if (!IsEmptyNestAvailable())
		{
			ShowDB();
			mKAUIGenericDB.SetText(_BuyStablesText.GetLocalizedString(), interactive: false);
			mKAUIGenericDB.SetTitle(_BuyStablesTitleText.GetLocalizedString());
			mKAUIGenericDB._YesMessage = "OpenStore";
			mKAUIGenericDB._NoMessage = "No";
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: false);
		}
		else
		{
			if (mKAUIGenericDB != null)
			{
				UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			}
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.EnableAllInputs(inActive: false);
			AvAvatar.SetUIActive(inActive: false);
			UiDragonsStable.OpenStableListUI(base.gameObject);
		}
	}

	public void OnStableUIOpened(UiDragonsStable UiStable)
	{
		mUiStable = UiStable;
		mUiStable.pUiStablesInfoCard.pSelectedPetID = mPedestalPetData.RaisedPetID;
		mUiStable.pUiStablesListCard.pCurrentMode = UiStablesListCard.Mode.NestAllocation;
		mUiStable.pUiStablesInfoCard.SetCloseButtonVisibility(visible: false);
	}

	private void OnStableUIClosed()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pInputEnabled = true;
	}

	public void SetPetHandler(bool success)
	{
		if (success)
		{
			if (!(mCurrentPetOnPedestal != null))
			{
				return;
			}
			if (SanctuaryManager.pCurPetInstance != null)
			{
				UnityEngine.Object.Destroy(SanctuaryManager.pCurPetInstance.gameObject);
			}
			SanctuaryManager.pCurPetInstance = mCurrentPetOnPedestal;
			SanctuaryManager.SetAndSaveCurrentType(mPedestalPetData.PetTypeID);
			SanctuaryManager.pCurPetData = mPedestalPetData;
			SanctuaryManager.RefreshPetMeter();
			mCurrentPetOnPedestal = null;
			if (m_AllowPetFollow)
			{
				SanctuaryManager.pCurPetInstance.gameObject.transform.position = AvAvatar.position + m_DragonStartOffset;
				SanctuaryManager.pCurPetInstance.gameObject.transform.LookAt(AvAvatar.mTransform);
				if (SanctuaryManager.pCurPetInstance.AIActor != null)
				{
					SanctuaryManager.pCurPetInstance.AIActor.SetState(AISanctuaryPetFSM.NORMAL);
				}
				SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: true);
				SanctuaryManager.pInstance.pSetFollowAvatar = false;
			}
			WsWebService.SetUserAchievementAndGetReward(_NestAllocationAchievementID, AchievementEventHandler, null);
			StableManager.RefreshActivePet();
		}
		else
		{
			UtDebug.LogError("Unable to make hatched pet as active dragon!", 0);
		}
	}

	private void No()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
	}

	private void ShowDB()
	{
		mOriginalAvatarPosition = AvAvatar.position;
		mOriginalAvatarRotation = AvAvatar.mTransform.rotation;
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		if (mKAUIGenericDB == null)
		{
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "PlaceEggToHatch");
		}
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
	}

	private void ShowDBOffline()
	{
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "ConnectOnline");
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
	}

	private void OkConnect()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
	}

	public void HatchDragonEgg(int inType, bool pickedEgg = false)
	{
		pIsHatching = true;
		mShowCutScene = true;
		mOriginalAvatarPosition = AvAvatar.position;
		mOriginalAvatarRotation = AvAvatar.mTransform.rotation;
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: false);
		mCheckedForTaskCompletion = !pickedEgg;
		KAUICursorManager.SetDefaultCursor("Loading");
		Incubator currentIncubator = _HatcheryManager.GetCurrentIncubator();
		if (currentIncubator != null)
		{
			UnityEngine.Object.Destroy(currentIncubator.mPickedUpEgg);
		}
		if (!mCheckedForTaskCompletion)
		{
			RaisedPetData.CreateNewPet(inType, setAsSelectedPet: true, unselectOtherPets: true, SetupRaisedPetData(inType), null, RaisedPetCreateCallback, null);
		}
		else
		{
			StartHatching();
		}
		if (!ProductData.pPairData.GetBoolValue(AnalyticEvent.HATCH_EGG.ToString(), defaultVal: false))
		{
			AnalyticAgent.LogEvent("AppsFlyer", AnalyticEvent.HATCH_EGG, new Dictionary<string, string>());
			ProductData.pPairData.SetValueAndSave(AnalyticEvent.HATCH_EGG.ToString(), true.ToString());
		}
	}

	private RaisedPetData SetupRaisedPetData(int inType)
	{
		RaisedPetData petData = RaisedPetData.InitDefault(inType, RaisedPetStage.BABY, "", Gender.Male, addToActivePets: false);
		if (petData.Gender == Gender.Unknown)
		{
			petData.Gender = ((UnityEngine.Random.Range(0, 100) % 2 == 0) ? Gender.Male : Gender.Female);
		}
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(petData.PetTypeID);
		SantuayPetResourceInfo[] array = Array.FindAll(sanctuaryPetTypeInfo._AgeData[0]._PetResList, (SantuayPetResourceInfo p) => p._Gender == petData.Gender);
		int num = UnityEngine.Random.Range(0, array.Length);
		SantuayPetResourceInfo santuayPetResourceInfo = array[num];
		petData.Geometry = santuayPetResourceInfo._Prefab;
		petData.SetState(RaisedPetStage.BABY, savedata: true);
		petData.Texture = null;
		PetSkillRequirements[] skillsRequired = sanctuaryPetTypeInfo._AgeData[0]._SkillsRequired;
		foreach (PetSkillRequirements petSkillRequirements in skillsRequired)
		{
			petData.UpdateSkillData(petSkillRequirements._Skill.ToString(), 0f, save: false);
		}
		petData.UserID = new Guid(UserInfo.pInstance.UserID);
		petData.SetupSaveData();
		return petData;
	}

	public void RaisedPetCreateCallback(int ptype, RaisedPetData pdata, object inUserData)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (pdata == null)
		{
			UtDebug.LogError("Egg Create failed");
			pIsHatching = false;
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", SanctuaryManager.pInstance._CreatePetFailureText.GetLocalizedString(), base.gameObject, "OnCreatePetFailOK");
			return;
		}
		if (MissionManager.IsTaskActive("Action", "Name", "HatchDragon"))
		{
			pdata.SetAttrData("HatchQuestPending", true.ToString(), DataType.BOOL);
		}
		SanctuaryManager.SetAndSaveCurrentType(pdata.PetTypeID);
		mPedestalPetData = pdata;
		StartHatching();
	}

	public void DoHatch()
	{
		if (mCheckedForTaskCompletion)
		{
			mPedestalPetData = RaisedPetData.GetHatchingPet(StableManager.pCurIncubatorID);
			if (mPedestalPetData == null)
			{
				Debug.LogError(" rettuen here");
				return;
			}
			if (mPedestalPetData.Gender == Gender.Unknown)
			{
				mPedestalPetData.Gender = ((UnityEngine.Random.Range(0, 100) % 2 == 0) ? Gender.Male : Gender.Female);
			}
			SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(mPedestalPetData.PetTypeID);
			SantuayPetResourceInfo[] array = Array.FindAll(sanctuaryPetTypeInfo._AgeData[0]._PetResList, (SantuayPetResourceInfo p) => p._Gender == mPedestalPetData.Gender);
			int num = UnityEngine.Random.Range(0, array.Length);
			SantuayPetResourceInfo santuayPetResourceInfo = sanctuaryPetTypeInfo._AgeData[0]._PetResList[num];
			mPedestalPetData.Geometry = santuayPetResourceInfo._Prefab;
			mPedestalPetData.SetState(RaisedPetStage.BABY, savedata: true);
			RaisedPetData raisedPetData = mPedestalPetData;
			int pStage = (int)mPedestalPetData.pStage;
			raisedPetData.SetAttrData("PetStage", pStage.ToString() ?? "", DataType.INT);
			mPedestalPetData.Texture = null;
			PetSkillRequirements[] skillsRequired = sanctuaryPetTypeInfo._AgeData[0]._SkillsRequired;
			foreach (PetSkillRequirements petSkillRequirements in skillsRequired)
			{
				mPedestalPetData.UpdateSkillData(petSkillRequirements._Skill.ToString(), 0f, save: false);
			}
		}
		else
		{
			SanctuaryManager.pInstance.pCreateInstance = false;
			SanctuaryManager.pCurPetData = mPedestalPetData;
		}
		mPedestalPetData.SaveDataReal();
		mIsPrevHatchedPet = false;
		SanctuaryManager.CreatePet(mPedestalPetData, Vector3.zero, Quaternion.identity, base.gameObject, "Full");
	}

	public virtual void OnPetReady(SanctuaryPet pet)
	{
		mCurrentPetOnPedestal = pet;
		KAUICursorManager.SetDefaultCursor("Arrow");
		pet.SetFollowAvatar(follow: false);
		pet.SetClickActivateObject(SanctuaryManager.pInstance._PetClickActivateObject);
		SanctuaryManager.pInstance.HandleZzzParticles(pet.pData.pIsSleeping, pet);
		if (mIsPrevHatchedPet)
		{
			mCurrentPetOnPedestal.PlayAnimation("IdleSit", WrapMode.Loop);
			mCurrentPetOnPedestal.AIActor.SetState(AISanctuaryPetFSM.IDLE);
			mIsCustomizationNeeded = !pet.pData.pIsNameCustomized;
			if (!RsResourceManager.pLevelLoadingScreen)
			{
				if (mIsCustomizationNeeded)
				{
					LoadTempPet(mCurrentPetOnPedestal);
					StartDragonCustomization();
				}
				else
				{
					ShowAssignNestPopUp();
				}
			}
		}
		if (!mIsPrevHatchedPet)
		{
			pet.gameObject.SetActive(value: false);
			if (mCheckedForTaskCompletion)
			{
				LoadTempPet(mCurrentPetOnPedestal);
			}
			else
			{
				if (SanctuaryManager.pCurPetInstance != null)
				{
					UnityEngine.Object.Destroy(SanctuaryManager.pCurPetInstance.gameObject);
				}
				SanctuaryManager.pCurPetInstance = pet;
				if (SanctuaryManager.pInstance != null)
				{
					SanctuaryManager.pInstance.OnPetReady(pet);
				}
			}
			mCurrentBondingCutSceneIdx = 0;
			if (!ShowCutScene())
			{
				StartDragonCustomization();
			}
			else if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.ActivateAll(active: false);
			}
		}
		if (PlayfabManager<PlayFabManagerDO>.Instance != null)
		{
			PlayfabManager<PlayFabManagerDO>.Instance.UpdateCharacterStatistics("Dragons", RaisedPetData.GetActiveDragons().Count);
		}
	}

	private void AchievementEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			UtDebug.LogError("!!!" + inType.ToString() + " failed!!!!");
			break;
		case WsServiceEvent.COMPLETE:
			if (inObject != null)
			{
				GameUtilities.AddRewards((AchievementReward[])inObject, inUseRewardManager: false, inImmediateShow: false);
			}
			else
			{
				UtDebug.LogError("!!!" + inType.ToString() + " did not return valid object!!!!");
			}
			break;
		}
	}

	private void OnCutSceneDone()
	{
		if (mCurrentBondingCutSceneIdx == 0)
		{
			StartDragonCustomization();
		}
		else if (mCurrentBondingCutSceneIdx == 1)
		{
			if (_HatcheryManager.pEgg != null)
			{
				UnityEngine.Object.Destroy(_HatcheryManager.pEgg);
			}
			DisableCutScene();
			if (mCheckedForTaskCompletion)
			{
				ResetPetInfo();
				mCheckedForTaskCompletion = false;
			}
			ResetAvatar();
			pIsHatching = false;
			AssignNest();
		}
	}

	private void LoadTempPet(SanctuaryPet pet)
	{
		mPrevPetData = SanctuaryManager.pCurPetData;
		SanctuaryManager.pCurPetData = mPedestalPetData;
		SanctuaryManager.LoadTempPet(pet, cacheInstance: true);
		if (SanctuaryManager.pCurPetInstance.AIActor != null)
		{
			SanctuaryManager.pCurPetInstance.AIActor.SetState(AISanctuaryPetFSM.IDLE);
		}
	}

	private void ResetPetInfo()
	{
		mCurrentPetOnPedestal.SetState(Character_State.idle);
		SanctuaryManager.ResetToActivePet();
		if (mPrevPetData != null)
		{
			SanctuaryManager.pCurPetData = mPrevPetData;
		}
		Incubator currentIncubator = _HatcheryManager.GetCurrentIncubator();
		if (currentIncubator != null)
		{
			mCurrentPetOnPedestal.SetPosition(currentIncubator.transform.position + currentIncubator.pDragonPositionOffset);
			mCurrentPetOnPedestal.transform.rotation = currentIncubator.transform.rotation;
			mCurrentPetOnPedestal.PlayAnimation("IdleSit", WrapMode.Loop);
			mCurrentPetOnPedestal.AIActor.SetState(AISanctuaryPetFSM.IDLE);
		}
	}

	private void GetPetHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inType == WsServiceType.SET_SELECTED_PET)
			{
				if ((bool)inObject)
				{
					UtDebug.Log("Set Selected Pet set successful");
				}
				else
				{
					UtDebug.LogError("Set Selected Pet data set failed");
				}
			}
			break;
		case WsServiceEvent.ERROR:
			UtDebug.Log("Web Servical Error for type:" + inType);
			break;
		}
	}

	public void StartHatching()
	{
		mShowCutScene = true;
		mCurrentBondingCutSceneIdx = 0;
		AvAvatar.pState = AvAvatarState.IDLE;
		KAUICursorManager.SetDefaultCursor("Loading");
		AvAvatar.SetUIActive(inActive: false);
		DoHatch();
		WsWebService.SetUserAchievementAndGetReward(_HatchingAchievementID, AchievementEventHandler, null);
	}

	private void ResetPlatformPos(CutSceneData data)
	{
		if (data._PlatformMarker != null)
		{
			if (mPlatformPos == Vector3.zero)
			{
				mPlatformPos = data._PlatformMarker.position;
			}
			else
			{
				data._PlatformMarker.position = mPlatformPos;
			}
		}
	}

	private bool ShowCutScene()
	{
		if (mShowCutScene && _BondingCutscenes != null && mCurrentBondingCutSceneIdx < _BondingCutscenes.Length && _BondingCutscenes[mCurrentBondingCutSceneIdx]._Cutscene != null)
		{
			CutSceneData cutSceneData = _BondingCutscenes[mCurrentBondingCutSceneIdx];
			cutSceneData._Cutscene._MessageObject = base.gameObject;
			cutSceneData._Cutscene.gameObject.SetActive(value: true);
			ResetPlatformPos(cutSceneData);
			if (cutSceneData._AvatarMarker != null)
			{
				AvAvatar.SetDisplayNameVisible(inVisible: false);
				AvAvatar.pAvatarCam.SetActive(value: false);
				AvAvatar.pState = AvAvatarState.NONE;
				AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
				if (component != null)
				{
					if (AvatarData.pInstanceInfo.FlightSuitEquipped())
					{
						component.ShowArmorWing(show: false);
					}
					component.enabled = false;
				}
				AvAvatar.SetParentTransform(cutSceneData._AvatarMarker.parent);
				AvAvatar.mTransform.localPosition = Vector3.zero;
				AvAvatar.mTransform.localRotation = Quaternion.identity;
				AvAvatar.mTransform.localScale = Vector3.one;
				Animator componentInChildren = AvAvatar.mTransform.GetComponentInChildren<Animator>();
				if (componentInChildren != null)
				{
					componentInChildren.enabled = false;
				}
			}
			if (cutSceneData._DragonMarker != null && mCurrentPetOnPedestal != null)
			{
				mCurrentPetOnPedestal.gameObject.SetActive(value: true);
				mCurrentPetOnPedestal.transform.parent = cutSceneData._DragonMarker.parent;
				mCurrentPetOnPedestal.transform.localPosition = Vector3.zero;
				mCurrentPetOnPedestal.transform.localRotation = Quaternion.identity;
				if (mCurrentPetOnPedestal.AIActor != null)
				{
					mCurrentPetOnPedestal.AIActor.SetState(AISanctuaryPetFSM.CINEMATIC);
				}
			}
			if (cutSceneData._Egg != null && _HatcheryManager.pEgg != null)
			{
				_HatcheryManager.pEgg.transform.SetParent(cutSceneData._Egg.transform.parent);
				_HatcheryManager.pEgg.transform.localPosition = cutSceneData._Egg.transform.localPosition;
				_HatcheryManager.pEgg.transform.localScale = cutSceneData._Egg.transform.localScale;
				cutSceneData._Egg.SetActive(value: false);
			}
			if (cutSceneData._SoundTrack != null)
			{
				SnChannel.Play(cutSceneData._SoundTrack, "Music_Pool", 0, inForce: true);
			}
			return true;
		}
		return false;
	}

	private void ResetAvatar()
	{
		mShowCutScene = false;
		AvAvatar.SetParentTransform(null);
		AvAvatar.SetActive(inActive: true);
		AvAvatar.SetPosition(mOriginalAvatarPosition);
		AvAvatar.mTransform.rotation = mOriginalAvatarRotation;
		AvAvatar.mTransform.localScale = Vector3.one;
		AvAvatarController component = AvAvatar.mTransform.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component.enabled = true;
		}
		AvAvatar.pState = AvAvatarState.IDLE;
		Animator componentInChildren = AvAvatar.mTransform.GetComponentInChildren<Animator>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = true;
		}
		AvAvatar.SetDisplayNameVisible(inVisible: true);
		if (mCurrentPetOnPedestal != null)
		{
			mCurrentPetOnPedestal.transform.parent = null;
			mCurrentPetOnPedestal.SetFollowAvatar(follow: false);
		}
		CheckForTaskCompletion();
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.ActivateAll(active: true);
		}
	}

	private void CheckForTaskCompletion()
	{
		if (!mCheckedForTaskCompletion && MissionManager.pInstance != null)
		{
			mCheckedForTaskCompletion = true;
			MissionManager.pInstance.CheckForTaskCompletion("Action", "HatchDragon");
		}
	}

	public void ActivateDragonCreationUIObj()
	{
		SnChannel.StopPool("VO_Pool");
		KAInput.ResetInputAxes();
		string[] array = _DragonCustomizationAsset.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnDragonCustomizationLoaded, typeof(GameObject));
	}

	public void OnDragonCustomizationLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			AvAvatar.pAvatarCam.SetActive(value: true);
			DisableCutScene();
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiDragonCustomization";
			UiDragonCustomization component = obj.GetComponent<UiDragonCustomization>();
			component.pPetData = mCurrentPetOnPedestal.pData;
			component.SetUiForJournal(isJournal: false);
			component._MessageObject = base.gameObject;
			AvAvatar.pState = AvAvatarState.NONE;
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pAvatarCam.SetActive(value: true);
			DisableCutScene();
			Debug.LogError("Failed to load Avatar Equipment....");
			break;
		}
	}

	private void DisableCutScene()
	{
		if (mShowCutScene && _BondingCutscenes != null && mCurrentBondingCutSceneIdx < _BondingCutscenes.Length && _BondingCutscenes[mCurrentBondingCutSceneIdx]._Cutscene != null)
		{
			_BondingCutscenes[mCurrentBondingCutSceneIdx]._Cutscene.gameObject.SetActive(value: false);
		}
	}

	private void StartDragonCustomization()
	{
		ActivateDragonCreationUIObj();
	}

	private void OnDragonCustomizationClosed()
	{
		if (mIsCustomizationNeeded)
		{
			mIsCustomizationNeeded = false;
			ResetAvatar();
			ResetPetInfo();
			ShowAssignNestPopUp();
		}
		else
		{
			mCurrentBondingCutSceneIdx = _BondingCutscenes.Length - 1;
			if (!ShowCutScene())
			{
				ResetAvatar();
			}
		}
	}

	private bool CheckPetHasNest()
	{
		if (mPedestalPetData != null && StableData.GetByPetID(mPedestalPetData.RaisedPetID) != null)
		{
			mPedestalPetData.pIncubatorID = -1;
			mPedestalPetData.SaveDataReal();
			PetAssignedCallBack();
			return true;
		}
		return false;
	}

	public void PetAssignedCallBack()
	{
		AchievementTask achievementTask = new AchievementTask(_AssignToNestAchievementID);
		UserAchievementTask.Set(achievementTask);
		if (mPedestalPetData != null && SanctuaryManager.pCurPetData != null)
		{
			if (mPedestalPetData.RaisedPetID != SanctuaryManager.pCurPetData.RaisedPetID)
			{
				RaisedPetData.SetSelectedPet(mPedestalPetData.RaisedPetID, unselectOtherPets: true, SetPetHandler, mPedestalPetData);
			}
			else
			{
				mCurrentPetOnPedestal = null;
				if (m_AllowPetFollow)
				{
					SanctuaryManager.pCurPetInstance.gameObject.transform.position = AvAvatar.position + m_DragonStartOffset;
					SanctuaryManager.pCurPetInstance.gameObject.transform.LookAt(AvAvatar.mTransform);
					if (SanctuaryManager.pCurPetInstance.AIActor != null)
					{
						SanctuaryManager.pCurPetInstance.AIActor.SetState(AISanctuaryPetFSM.NORMAL);
					}
					SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: true);
					SanctuaryManager.pInstance.pSetFollowAvatar = false;
				}
				WsWebService.SetUserAchievementAndGetReward(_NestAllocationAchievementID, AchievementEventHandler, null);
				StableManager.RefreshActivePet();
			}
		}
		mIsPrevHatchedPet = false;
		Incubator currentIncubator = _HatcheryManager.GetCurrentIncubator();
		if (currentIncubator != null && currentIncubator.IsIdle())
		{
			currentIncubator.SetIncubatorState(Incubator.IncubatorStates.WAITING_FOR_EGG);
		}
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "NestAssigned");
		}
	}

	private void HideDragonBabyPedestal()
	{
		pPedestalObject.transform.parent = _BondingCutscenes[1]._Cutscene.transform;
	}

	private void ShowDragonBabyPedestal()
	{
		pPedestalObject.SetActive(value: true);
	}

	private void WaitListCompleted()
	{
		mWaitForWaitListToComplete = false;
	}
}
