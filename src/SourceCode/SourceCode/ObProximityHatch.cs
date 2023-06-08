using System;
using System.Collections.Generic;
using UnityEngine;

public class ObProximityHatch : ObProximity
{
	public static bool pIsHatching;

	public bool _LoadLastLevel;

	public int _DefaultStableItemID;

	public int _HatchingAchievementID = 201387;

	public HatcheryManager _HatcheryManager;

	public LocaleString _GrowDragonText = new LocaleString("Grow your dragon to the X stage?");

	public LocaleString _GrowDragonTitleText = new LocaleString("Grow your dragon");

	public LocaleString _PlaceEggTitleText = new LocaleString("Hatch Egg");

	public LocaleString _PublishHatchText = new LocaleString("Congratulations!  You hatched a {dragon type}.  Would you like to share this?");

	public LocaleString _PublishHatchTitleText = new LocaleString("Hatch");

	public LocaleString _PublishGrowthText = new LocaleString("Congratulations!  Your dragon grew to {age}.  Would you like to share this?");

	public LocaleString _PublishGrowthTitleText = new LocaleString("Growth");

	public Vector3 _DragonStartOffset = Vector3.zero;

	public string _DragonCustomizationAsset = "RS_DATA/PfUiDragonCustomizationDO.unity3d/PfUiDragonCustomizationDO";

	public string _DragonAgeUpCustomizationAsset = "RS_DATA/PfUiDragonCustomizationDO.unity3d/PfUiDragonCustomizationDO";

	public CutSceneData[] _BondingCutscenes;

	private int mPetLevelIndex = -1;

	private bool mInitUI;

	private KAUIGenericDB mKAUIGenericDB;

	private int mCurrentBondingCutSceneIdx = -1;

	private Vector3 mOriginalAvatarPosition = Vector3.zero;

	private Quaternion mOriginalAvatarRotation = Quaternion.identity;

	private bool mShowCutScene;

	private bool mCheckedForTaskCompletion = true;

	private void OnEnable()
	{
		pIsHatching = false;
	}

	public override void Update()
	{
		if ((_UseGlobalActive && !ObClickable.pGlobalActive) || !_Active || AvAvatar.pObject == null || _Range == 0f || pIsHatching)
		{
			return;
		}
		if ((AvAvatar.position - (base.transform.position + base.transform.TransformDirection(_Offset))).magnitude > _Range)
		{
			mInitUI = true;
		}
		else
		{
			if (!mInitUI)
			{
				return;
			}
			mInitUI = false;
			if (!(SanctuaryManager.pCurPetInstance != null))
			{
				return;
			}
			UserAchievementInfo userAchievementInfo = PetRankData.GetUserAchievementInfo(SanctuaryManager.pCurPetData);
			bool flag = false;
			mPetLevelIndex = -1;
			if (userAchievementInfo != null)
			{
				SetLevelIndex();
				if (mPetLevelIndex > 0 && mPetLevelIndex > SanctuaryManager.pCurPetInstance.pAge && (SanctuaryManager.pCurPetInstance.pTypeInfo._AgeUpMissionID <= 0 || MissionManager.IsMissionCompleted(SanctuaryManager.pCurPetInstance.pTypeInfo._AgeUpMissionID)))
				{
					flag = true;
				}
			}
			if (flag)
			{
				ShowDB();
				mKAUIGenericDB.SetText(_GrowDragonText.GetLocalizedString(), interactive: false);
				mKAUIGenericDB.SetTitle(_GrowDragonTitleText.GetLocalizedString());
				mKAUIGenericDB._YesMessage = "GrowDragon";
				mKAUIGenericDB._NoMessage = "NoPlaceEgg";
			}
		}
	}

	private void ShowDB()
	{
		mOriginalAvatarPosition = AvAvatar.position;
		mOriginalAvatarRotation = AvAvatar.mTransform.rotation;
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "PlaceEggToHatch");
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
	}

	private void StartDragonCustomization()
	{
		ActivateDragonCreationUIObj();
	}

	public void ActivateDragonCreationUIObj()
	{
		SnChannel.StopPool("VO_Pool");
		KAInput.ResetInputAxes();
		string[] array = _DragonCustomizationAsset.Split('/');
		if (SanctuaryManager.pCurPetData != null && RaisedPetData.GetAgeIndex(SanctuaryManager.pCurPetData.pStage) > 0)
		{
			array = _DragonAgeUpCustomizationAsset.Split('/');
		}
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
			component.pPetData = SanctuaryManager.pCurPetData;
			component.SetUiForJournal(isJournal: false);
			component._MessageObject = base.gameObject;
			AvAvatar.pState = AvAvatarState.NONE;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pAvatarCam.SetActive(value: true);
			DisableCutScene();
			Debug.LogError("Failed to load Avatar Equipment....");
			break;
		}
	}

	private void GrowDragon()
	{
		UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
		SanctuaryManager.pCurPetInstance.SetAge(mPetLevelIndex, save: true, resetSkills: true);
		StartDragonCustomization();
	}

	private void SetLevelIndex()
	{
		RaisedPetData pCurPetData = SanctuaryManager.pCurPetData;
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(pCurPetData.PetTypeID);
		UserAchievementInfo userAchievementInfo = PetRankData.GetUserAchievementInfo(pCurPetData);
		int ageIndex = RaisedPetData.GetAgeIndex(pCurPetData.pStage);
		for (int i = 0; i < SanctuaryData.pInstance._PetLevels.Length; i++)
		{
			PetAgeLevel obj = SanctuaryData.pInstance._PetLevels[i];
			int ageIndex2 = RaisedPetData.GetAgeIndex(obj._Age);
			int level = obj._Level;
			if (ageIndex < ageIndex2 && ageIndex2 > mPetLevelIndex && userAchievementInfo.RankID >= level && ageIndex2 < sanctuaryPetTypeInfo._AgeData.Length)
			{
				mPetLevelIndex = ageIndex2;
			}
		}
	}

	public void HatchDragonEgg(int inType, Incubator incubator = null)
	{
		if (SanctuaryManager.pMountedState || (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted))
		{
			SanctuaryManager.pMountedState = false;
			if (SanctuaryManager.pCurPetInstance != null)
			{
				SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
			}
			AvAvatar.pSubState = AvAvatarSubState.NORMAL;
		}
		pIsHatching = true;
		mShowCutScene = true;
		mOriginalAvatarPosition = AvAvatar.position;
		mOriginalAvatarRotation = AvAvatar.mTransform.rotation;
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		if (incubator != null)
		{
			UnityEngine.Object.Destroy(incubator.mPickedUpEgg);
		}
		RaisedPetData.CreateNewPet(inType, setAsSelectedPet: true, unselectOtherPets: true, SetupRaisedPetData(inType), null, RaisedPetCreateCallback, null);
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

	public void OnCreatePetFailOK()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
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
		DoHatch(pdata);
		WsWebService.SetUserAchievementAndGetReward(_HatchingAchievementID, AchievementEventHandler, null);
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

	public void DoHatch(RaisedPetData mData)
	{
		SanctuaryManager.pInstance.pCreateInstance = false;
		SanctuaryManager.pCurPetData = mData;
		SanctuaryManager.CreatePet(mData, Vector3.zero, Quaternion.identity, base.gameObject, "Full");
	}

	private void NoPlaceEgg()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
	}

	public virtual void OnPetReady(SanctuaryPet pet)
	{
		pIsHatching = false;
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (SanctuaryManager.pCurPetInstance != null)
		{
			UnityEngine.Object.Destroy(SanctuaryManager.pCurPetInstance.gameObject);
		}
		SanctuaryManager.pCurPetInstance = pet;
		if (SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.OnPetReady(pet);
		}
		pet.gameObject.SetActive(value: false);
		StableData.UpdateInfo();
		if (StableData.GetByPetID(SanctuaryManager.pCurPetData.RaisedPetID) == null && !AddPetToNest() && _DefaultStableItemID > 0)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			CommonInventoryData.pInstance.AddItem(_DefaultStableItemID, 1);
			CommonInventoryData.pInstance.Save(InventorySaveEventHandler, null);
			return;
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
		if (PlayfabManager<PlayFabManagerDO>.Instance != null)
		{
			PlayfabManager<PlayFabManagerDO>.Instance.UpdateCharacterStatistics("Dragons", RaisedPetData.GetActiveDragons().Count);
		}
	}

	private bool AddPetToNest()
	{
		bool result = false;
		for (int i = 0; i < StableData.pStableList.Count; i++)
		{
			StableData stableData = StableData.pStableList[i];
			NestData emptyNest = stableData.GetEmptyNest();
			if (emptyNest != null)
			{
				StableData.AddPetToNest(stableData.ID, emptyNest.ID, SanctuaryManager.pCurPetInstance.pData.RaisedPetID);
				result = true;
				break;
			}
		}
		return result;
	}

	private void InventorySaveEventHandler(bool success, object inUserData)
	{
		if (success)
		{
			StableData.UpdateInfo();
			AddPetToNest();
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
			ResetAvatar();
		}
	}

	private void OnDragonCustomizationClosed()
	{
		mCurrentBondingCutSceneIdx = _BondingCutscenes.Length - 1;
		mCheckedForTaskCompletion = false;
		if (!ShowCutScene())
		{
			ResetAvatar();
		}
	}

	private bool ShowCutScene()
	{
		if (mShowCutScene && _BondingCutscenes != null && mCurrentBondingCutSceneIdx < _BondingCutscenes.Length && _BondingCutscenes[mCurrentBondingCutSceneIdx]._Cutscene != null)
		{
			CutSceneData cutSceneData = _BondingCutscenes[mCurrentBondingCutSceneIdx];
			cutSceneData._Cutscene._MessageObject = base.gameObject;
			cutSceneData._Cutscene.gameObject.SetActive(value: true);
			if (cutSceneData._AvatarMarker != null)
			{
				AvAvatar.SetDisplayNameVisible(inVisible: false);
				AvAvatar.pAvatarCam.SetActive(value: false);
				AvAvatar.pState = AvAvatarState.NONE;
				AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
				if (component != null)
				{
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
			if (cutSceneData._DragonMarker != null && SanctuaryManager.pCurPetInstance != null)
			{
				SanctuaryManager.pCurPetInstance.gameObject.SetActive(value: true);
				SanctuaryManager.pCurPetInstance.transform.parent = cutSceneData._DragonMarker.parent;
				SanctuaryManager.pCurPetInstance.transform.localPosition = Vector3.zero;
				SanctuaryManager.pCurPetInstance.transform.localRotation = Quaternion.identity;
				if (SanctuaryManager.pCurPetInstance.AIActor != null)
				{
					SanctuaryManager.pCurPetInstance.AIActor.SetState(AISanctuaryPetFSM.CINEMATIC);
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
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.transform.parent = null;
			SanctuaryManager.pCurPetInstance.gameObject.transform.position = AvAvatar.position + _DragonStartOffset;
			SanctuaryManager.pCurPetInstance.gameObject.transform.LookAt(AvAvatar.mTransform);
			if (SanctuaryManager.pCurPetInstance.AIActor != null)
			{
				SanctuaryManager.pCurPetInstance.AIActor.SetState(AISanctuaryPetFSM.NORMAL);
			}
			SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: true);
			SanctuaryManager.pInstance.pSetFollowAvatar = false;
		}
		CheckForTaskCompletion();
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.ActivateAll(active: true);
		}
		if (_LoadLastLevel)
		{
			RsResourceManager.LoadLevel(RsResourceManager.pLastLevel);
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

	private void DisableCutScene()
	{
		if (mShowCutScene && _BondingCutscenes != null && mCurrentBondingCutSceneIdx < _BondingCutscenes.Length && _BondingCutscenes[mCurrentBondingCutSceneIdx]._Cutscene != null)
		{
			_BondingCutscenes[mCurrentBondingCutSceneIdx]._Cutscene.gameObject.SetActive(value: false);
		}
	}

	private void DestroyGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
		}
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		CheckForTaskCompletion();
	}
}
