using System;
using System.Collections.Generic;
using UnityEngine;

public class UserNotifyDragonTicket : UserNotify
{
	public delegate void OnUNDragonTicketDone(bool inSuccess);

	private static bool mShouldShowDragonSelection = false;

	private static int mStableCount = -1;

	private static UserNotifyDragonTicket mInstance;

	private OnUNDragonTicketDone mCallback;

	private AvAvatarState mPrevAvatarState;

	private bool mIsDragonCreated;

	private bool mIsCheckingTicketsAgain;

	public Transform _DragonStayMarker;

	public string _NewDragonDialog = "PfUiRewardsDB";

	public string _DragonCreationSceneName = "HatcheryINTDM";

	public string _DragonCustomizationAsset = "RS_DATA/PfUiDragonCustomizationDO.unity3d/PfUiDragonCustomizationDO";

	public LocaleString _HatchADragonFirstText = new LocaleString("You need to hatch a dragon before you can receive your reward!");

	public LocaleString _NestNotAvailableText = new LocaleString("All your stables are full.");

	public LocaleString _PurchaseStableText = new LocaleString("purchase another stable");

	public LocaleString _AssignNestForDragonText = new LocaleString("[Review] Assign nest to your dragon.");

	public StoreLoader.Selection _StoreInfo;

	public int _CreatePetAchievementID = 201387;

	[HideInInspector]
	public bool _IsNestAvailable = true;

	[HideInInspector]
	public bool _ShouldShowDragonSelection = true;

	private UserItemData mItem;

	private bool mIsInStartupLoop;

	private bool mIsWaitForDragonPicture;

	public static UserNotifyDragonTicket pInstance => mInstance;

	private void Awake()
	{
		mInstance = this;
	}

	private void OnDestroy()
	{
		if (mInstance == this)
		{
			mInstance = null;
		}
	}

	public override void OnWaitBeginImpl()
	{
		mIsInStartupLoop = true;
		CheckTickets();
	}

	protected new void OnWaitEnd()
	{
		if (mShouldShowDragonSelection)
		{
			mShouldShowDragonSelection = false;
			if (_ShouldShowDragonSelection)
			{
				UiDragonsStable.OpenDragonListUI(base.gameObject);
				return;
			}
			_ShouldShowDragonSelection = true;
			OnStableUIClosed();
			return;
		}
		if ((KAUIStore.pInstance == null || (KAUIStore.pInstance != null && !KAUIStore.pInstance.GetVisibility())) && mPrevAvatarState != AvAvatarState.PAUSED)
		{
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = AvAvatarState.IDLE;
			mIsCheckingTicketsAgain = false;
		}
		if (mIsInStartupLoop)
		{
			mIsInStartupLoop = false;
			base.OnWaitEnd();
		}
		if (mCallback != null)
		{
			mCallback(mIsDragonCreated);
			mCallback = null;
			mIsDragonCreated = false;
		}
	}

	private UserItemData[] GetItemDataList(List<int> itemIdList)
	{
		List<UserItemData> list = new List<UserItemData>();
		if (ParentData.pIsReady)
		{
			UserItemData[] items = ParentData.pInstance.pInventory.GetItems(454);
			if (items != null && items.Length != 0)
			{
				list.AddRange(items);
			}
		}
		if (CommonInventoryData.pIsReady)
		{
			UserItemData[] items2 = CommonInventoryData.pInstance.GetItems(454);
			if (items2 != null && items2.Length != 0)
			{
				list.AddRange(items2);
			}
		}
		if (itemIdList == null || itemIdList.Count == 0 || list.Count == 0)
		{
			return list.ToArray();
		}
		List<UserItemData> list2 = new List<UserItemData>();
		foreach (UserItemData item in list)
		{
			if (itemIdList.Contains(item.Item.ItemID))
			{
				list2.Add(item);
			}
		}
		return list2.ToArray();
	}

	public bool CheckTickets(List<int> itemIdList = null, OnUNDragonTicketDone inCallBack = null)
	{
		if (!SanctuaryManager.pInstance._CreateInstance)
		{
			mCallback = inCallBack;
			OnWaitEnd();
			return false;
		}
		if (!mIsCheckingTicketsAgain)
		{
			mPrevAvatarState = AvAvatar.pState;
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			mCallback = inCallBack;
		}
		UserItemData[] itemDataList = GetItemDataList(itemIdList);
		if (itemDataList == null || itemDataList.Length == 0)
		{
			OnWaitEnd();
			return false;
		}
		StableData.UpdateInfo();
		List<RaisedPetData> activeDragons = RaisedPetData.GetActiveDragons();
		UserItemData[] array = itemDataList;
		foreach (UserItemData userItemData in array)
		{
			if (!HasDragonForItem(userItemData, activeDragons) && (SubscriptionInfo.pIsMember || !userItemData.Item.Locked))
			{
				bool checkForNest = SanctuaryData.GetPetCustomizationType(userItemData.Item.GetAttribute("PetTypeID", -1)) != PetCustomizationType.None;
				mItem = userItemData;
				AwardDragon(checkForNest);
				return true;
			}
		}
		OnWaitEnd();
		return false;
	}

	private bool IsHatchDragonFirst(UserItemData inItem)
	{
		if (inItem == null)
		{
			return true;
		}
		bool flag = inItem.Item.GetAttribute("HatchDragonFirst", defaultValue: false);
		if (flag && MissionManager.pInstance != null)
		{
			flag = !((MissionManagerDO)MissionManager.pInstance).CompletedHatchTask();
		}
		return flag;
	}

	public void AwardDragon(bool CheckForNest = true)
	{
		if (!IsHatchDragonFirst(mItem))
		{
			StableManager.EnsureActivePetHasNest();
			_IsNestAvailable = IsEmptyNestAvailable();
			if (CheckForNest && !_IsNestAvailable)
			{
				if (mStableCount != StableData.pStableList.Count)
				{
					mStableCount = StableData.pStableList.Count;
					UiChatHistory.AddSystemNotification(_NestNotAvailableText.GetLocalizedString(), new MessageInfo(), OnStableMessageClicked, ignoreDuplicateMessage: false, _PurchaseStableText.GetLocalizedString());
				}
				OnWaitEnd();
				return;
			}
			mShouldShowDragonSelection = false;
			GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources(_NewDragonDialog);
			if (gameObject != null)
			{
				UnityEngine.Object.Instantiate(gameObject).GetComponent<UiRewardsDB>().Show(mItem.Item.Description, mItem.Item.AssetName, base.gameObject);
				if (!CheckForNest && !_IsNestAvailable)
				{
					UiChatHistory.AddSystemNotification(_NestNotAvailableText.GetLocalizedString(), new MessageInfo(), OnStableMessageClicked, ignoreDuplicateMessage: false, _PurchaseStableText.GetLocalizedString());
				}
			}
			else
			{
				OnWaitEnd();
			}
		}
		else
		{
			if (KAUIStore.pInstance == null || (KAUIStore.pInstance != null && !KAUIStore.pInstance.GetVisibility()))
			{
				UiNotification.ShowBig(_HatchADragonFirstText.GetLocalizedString());
			}
			else
			{
				UiChatHistory.AddSystemNotification(_HatchADragonFirstText.GetLocalizedString(), new MessageInfo());
			}
			OnWaitEnd();
		}
	}

	private void OnStableMessageClicked(object obj)
	{
		if (_StoreInfo != null)
		{
			StoreLoader.Load(setDefaultMenuItem: true, _StoreInfo._Category, _StoreInfo._Store, base.gameObject);
		}
	}

	private void OnAssignStableMessageClicked(object obj)
	{
		UiDragonsStable.OpenDragonListUI(base.gameObject);
	}

	public bool IsEmptyNestAvailable()
	{
		bool result = false;
		int i = 0;
		for (int count = StableData.pStableList.Count; i < count; i++)
		{
			if (StableData.pStableList[i].GetEmptyNest() != null)
			{
				result = true;
			}
		}
		return result;
	}

	public bool HasDragonForItem(UserItemData ItemData, List<RaisedPetData> Pets)
	{
		int attribute = ItemData.Item.GetAttribute("PetTypeID", -1);
		if (attribute == -1)
		{
			return true;
		}
		List<string> list = null;
		if (SanctuaryData.pInstance != null)
		{
			list = SanctuaryData.GetUniquePetTicketItemsList(attribute);
		}
		int quantity = ItemData.Quantity;
		int num = 0;
		string text = ItemData.Item.ItemID.ToString();
		RaisedPetData[] array = Pets.ToArray();
		foreach (RaisedPetData raisedPetData in array)
		{
			RaisedPetAttribute raisedPetAttribute = raisedPetData.FindAttrData("TicketID");
			if (raisedPetAttribute != null && (raisedPetAttribute.Value == text || (list != null && list.Contains(raisedPetAttribute.Value.ToString()))))
			{
				num++;
				if (list == null)
				{
					Pets.Remove(raisedPetData);
				}
				if (num >= quantity)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void OnCloseNewDragonDialog()
	{
		int attribute = mItem.Item.GetAttribute("PetTypeID", -1);
		RaisedPetData.CreateNewPet(attribute, setAsSelectedPet: true, unselectOtherPets: true, SetupRaisedPetData(attribute), null, RaisedPetCreateCallback, true);
	}

	public void RaisedPetCreateCallback(int ptype, RaisedPetData pdata, object inUserData)
	{
		if (pdata == null)
		{
			UtDebug.LogError("Create Dragon failed");
			OnWaitEnd();
			return;
		}
		int num = -1;
		if (mItem.UserItemAttributes != null)
		{
			num = mItem.UserItemAttributes.GetIntValue("LevelAchievement", -1);
		}
		if (num != -1)
		{
			WsWebService.SetAchievementByEntityIDs(num, new Guid?[1] { pdata.EntityID }, "", AchievementEventHandler, null);
		}
		WsWebService.SetUserAchievementAndGetReward(_CreatePetAchievementID, AchievementEventHandler, null);
		if (IsEmptyNestAvailable())
		{
			if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
			{
				SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
			}
			if (inUserData != null && (bool)inUserData)
			{
				RaisedPetData.SetAllPetUnselected();
				pdata.IsSelected = true;
			}
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SetRaisedPet(pdata, -1);
			}
			if (SanctuaryManager.pInstance != null)
			{
				SanctuaryManager.pInstance.pCreateInstance = false;
			}
			SanctuaryManager.SetAndSaveCurrentType(pdata.PetTypeID);
			SanctuaryManager.pCurPetData = pdata;
			SanctuaryManager.CreatePet(pdata, Vector3.zero, Quaternion.identity, base.gameObject, "Full");
		}
		else
		{
			SanctuaryManager.CreatePet(pdata, Vector3.zero, Quaternion.identity, base.gameObject, "Full");
		}
	}

	public static void Reset()
	{
		mStableCount = -1;
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

	public RaisedPetData SetupRaisedPetData(int inType)
	{
		RaisedPetData petData = RaisedPetData.InitDefault(inType, RaisedPetStage.ADULT, "", Gender.Male, addToActivePets: false);
		if (petData.Gender == Gender.Unknown)
		{
			petData.Gender = ((UnityEngine.Random.Range(0, 100) % 2 == 0) ? Gender.Male : Gender.Female);
		}
		int num = ParseAgeAttribute(mItem);
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(petData.PetTypeID);
		SantuayPetResourceInfo[] array = Array.FindAll(sanctuaryPetTypeInfo._AgeData[num]._PetResList, (SantuayPetResourceInfo p) => p._Gender == petData.Gender);
		int num2 = UnityEngine.Random.Range(0, array.Length);
		SantuayPetResourceInfo santuayPetResourceInfo = array[num2];
		petData.Geometry = santuayPetResourceInfo._Prefab;
		petData.SetState(RaisedPetData.GetGrowthStage(num), savedata: true);
		petData.Texture = null;
		PetSkillRequirements[] skillsRequired = sanctuaryPetTypeInfo._AgeData[num]._SkillsRequired;
		foreach (PetSkillRequirements petSkillRequirements in skillsRequired)
		{
			petData.UpdateSkillData(petSkillRequirements._Skill.ToString(), 0f, save: false);
		}
		petData.SetAttrData("TicketID", mItem.Item.ItemID.ToString(), DataType.INT);
		if (!SanctuaryData.IsNameChangeAllowed(petData))
		{
			petData.Name = SanctuaryData.GetPetDefaultName(petData);
			petData.pIsNameCustomized = true;
		}
		if (!SanctuaryData.IsColorChangeAllowed(petData))
		{
			Color[] petDefaultColors = SanctuaryData.GetPetDefaultColors(petData);
			petData.SetColors(petDefaultColors);
		}
		if (SanctuaryData.GetPetCustomizationType(petData) == PetCustomizationType.None)
		{
			RaisedPetData raisedPetData = petData;
			int i = (int)petData.pStage;
			raisedPetData.SetAttrData("_LastCustomizedStage", i.ToString() ?? "", DataType.INT);
		}
		petData.UserID = new Guid(UserInfo.pInstance.UserID);
		petData.SetupSaveData();
		return petData;
	}

	private int ParseAgeAttribute(UserItemData itemData)
	{
		if (itemData == null)
		{
			return -1;
		}
		string attribute = mItem.Item.GetAttribute("PetStage", "ADULT");
		RaisedPetStage rs = RaisedPetStage.NONE;
		if (!string.IsNullOrEmpty(attribute) && Enum.IsDefined(typeof(RaisedPetStage), attribute))
		{
			rs = (RaisedPetStage)Enum.Parse(typeof(RaisedPetStage), attribute);
		}
		return RaisedPetData.GetAgeIndex(rs);
	}

	private void AssignDragonToStable(RaisedPetData mData)
	{
		List<StableData> byType = StableData.GetByType(mItem.Item.GetAttribute("StableType", string.Empty));
		bool flag = false;
		if (byType != null && byType.Count > 0)
		{
			foreach (StableData item in byType)
			{
				NestData emptyNest = item.GetEmptyNest();
				if (emptyNest != null)
				{
					StableManager.MovePetToNest(item.ID, emptyNest.ID, mData.RaisedPetID);
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			return;
		}
		int i = 0;
		for (int count = StableData.pStableList.Count; i < count; i++)
		{
			NestData emptyNest2 = StableData.pStableList[i].GetEmptyNest();
			if (emptyNest2 != null)
			{
				StableManager.MovePetToNest(StableData.pStableList[i].ID, emptyNest2.ID, mData.RaisedPetID);
				break;
			}
		}
	}

	public virtual void OnPetReady(SanctuaryPet pet)
	{
		if (mIsWaitForDragonPicture)
		{
			return;
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		bool flag = IsEmptyNestAvailable();
		if (SanctuaryData.GetPetCustomizationType(pet.pData.PetTypeID) == PetCustomizationType.None)
		{
			mIsWaitForDragonPicture = true;
			SanctuaryManager.pInstance.TakePicture(pet.gameObject, base.gameObject);
		}
		if (flag)
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
			AssignDragonToStable(pet.pData);
			pet.gameObject.SetActive(value: false);
		}
		else
		{
			UnityEngine.Object.Destroy(pet.gameObject);
		}
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.pIsReady = false;
		}
		mItem = null;
		mIsDragonCreated = true;
		if (SanctuaryData.GetPetCustomizationType(pet.pData.PetTypeID) != PetCustomizationType.None)
		{
			mShouldShowDragonSelection = true;
			ActivateDragonCreationUIObj();
		}
		else if (flag)
		{
			OnStableUIClosed();
		}
		else
		{
			mIsCheckingTicketsAgain = true;
			CheckTickets();
		}
	}

	private void OnPetPictureDone()
	{
		mIsWaitForDragonPicture = false;
	}

	private void OnPetPictureDoneFailed()
	{
		UtDebug.LogError("Failed to get Pet Picture");
		mIsWaitForDragonPicture = false;
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
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiDragonCustomization";
			UiDragonCustomization component = obj.GetComponent<UiDragonCustomization>();
			component.pPetData = SanctuaryManager.pCurPetInstance.pData;
			component.SetUiForJournal(isJournal: false);
			component._MessageObject = base.gameObject;
			AvAvatar.pState = AvAvatarState.NONE;
			if (UtPlatform.IsMobile())
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				RsResourceManager.DestroyLoadScreen();
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pAvatarCam.SetActive(value: true);
			if (UtPlatform.IsMobile())
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				RsResourceManager.DestroyLoadScreen();
			}
			Debug.LogError("Failed to load Avatar Equipment....");
			break;
		}
	}

	public void OnDragonCustomizationClosed()
	{
		mShouldShowDragonSelection = false;
		if (_ShouldShowDragonSelection)
		{
			UiDragonsStable.OpenDragonListUI(base.gameObject);
			return;
		}
		_ShouldShowDragonSelection = true;
		OnStableUIClosed();
	}

	public void OnStableUIClosed()
	{
		if (StableManager.pCurrentStableData == null)
		{
			SanctuaryManager.pCurPetInstance.gameObject.SetActive(value: true);
			SanctuaryManager.pCurPetInstance.SetAvatar(AvAvatar.mTransform, SpawnTeleportEffect: true, SanctuaryManager.pInstance._FollowAvatar);
			if (SanctuaryManager.pInstance._FollowAvatar)
			{
				SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: true);
				SanctuaryManager.pCurPetInstance.MoveToAvatar(postponed: true);
			}
			else if (_DragonStayMarker != null)
			{
				SanctuaryManager.pInstance.EnableAllPets(enable: false, _DragonStayMarker);
			}
		}
		mIsCheckingTicketsAgain = true;
		CheckTickets();
	}
}
