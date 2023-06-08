using System;
using System.Collections.Generic;
using UnityEngine;

public class PetCSM : ObContextSensitive
{
	[Serializable]
	public class ItemUseAchievement
	{
		public int _ItemID;

		public int _AchievementID;

		public bool _ShowReward;
	}

	public ContextSensitiveState[] _Menus;

	public string _FeedInventoryCSItemName = "Feed";

	public string _CSIconTemplateName = "IconTemplate";

	public string _CSFeedItemTemplateName = "FeedItemTemplate";

	public LocaleString _StoreCSItemName = new LocaleString("Store");

	public string _StoreIconName = "IcoDWDragonsHUDStore";

	public string _StableCSItemName = "Stable";

	public string _AgeUpCSItemName = "AgeUp";

	public string _CustomizeCSItemName = "Customize";

	public LocaleString _FullEnergyText = new LocaleString("Your dragon's full. You cannot feed it anymore.");

	public int _EatFishAchievementTaskID = 177;

	public int _DragonsFeedCategoryID = 415;

	public int _FishCategoryID = 408;

	public GameObject _FeedFX;

	public string _FeedAnim = "Gulp";

	public StoreLoader.Selection _StoreInfo;

	public List<ItemUseAchievement> _ItemUseAchievement;

	public string _MultipleDragonTutorialName = "PfMultipleDragonsTutorialDO";

	protected KAUIGenericDB mGenericDBUi;

	private bool mIsFeedCSMOpened;

	protected override void Start()
	{
		base.Start();
		MissionManager.AddMissionEventHandler(OnMissionEvent);
	}

	protected override void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
		inStatesArrData = _Menus;
	}

	protected override void OnMenuActive(ContextSensitiveStateType inMenuType)
	{
		base.OnMenuActive(inMenuType);
		if (inMenuType == ContextSensitiveStateType.ONCLICK)
		{
			UpdateFeedItems();
		}
		AvAvatar.pInputEnabled = false;
		KAUI.SetExclusive(base.pUI);
	}

	protected void UpdateFeedItems()
	{
		ClearPreviousFeedChildrenData();
		if (CommonInventoryData.pInstance == null)
		{
			return;
		}
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(_DragonsFeedCategoryID);
		ContextData contextData = GetContextData(_FeedInventoryCSItemName);
		ContextData contextData2 = GetContextData(_CSFeedItemTemplateName);
		if (items != null && contextData != null && contextData2 != null && items.Length != 0)
		{
			UserItemData[] array = items;
			for (int i = 0; i < array.Length; i++)
			{
				UserItemData userItemData2 = (contextData2.pUserItemData = array[i]);
				contextData2._IconSpriteName = userItemData2.Item.IconName;
				AddChildIntoDataListItem(contextData, userItemData2.Item.ItemName);
				string itemName = userItemData2.Item.ItemName;
				AddIntoDataList(userItemData2.Item.ItemName, itemName, contextData2);
			}
		}
		else
		{
			contextData2 = GetContextData(_CSIconTemplateName);
			contextData2._IconSpriteName = _StoreIconName;
			AddChildIntoDataListItem(contextData, _StoreCSItemName._Text);
			AddIntoDataList(_StoreCSItemName._Text, _StoreCSItemName.GetLocalizedString(), contextData2);
		}
	}

	private void ClearPreviousFeedChildrenData()
	{
		ContextData contextData = GetContextData(_FeedInventoryCSItemName);
		string[] childrenNames = contextData._ChildrenNames;
		foreach (string inName in childrenNames)
		{
			RemoveFromDataList(inName);
		}
		contextData.RemoveAllChildren();
	}

	private void OnWidgetClicked(string inName)
	{
		if (inName != "BackBtn")
		{
			return;
		}
		if (!mIsFeedCSMOpened)
		{
			DestroyMenu(checkProximity: false);
		}
		else
		{
			if (base.pUI == null || base.pUI.pUIContextSensitiveMenuList == null || base.pUI.pUIContextSensitiveMenuList.Count <= 0)
			{
				return;
			}
			foreach (UiContextSensetiveMenu pUIContextSensitiveMenu in base.pUI.pUIContextSensitiveMenuList)
			{
				KAWidget kAWidget = pUIContextSensitiveMenu.FindItem(_FeedInventoryCSItemName);
				if (!(kAWidget == null))
				{
					base.pUI.OnClick(kAWidget);
					break;
				}
			}
		}
	}

	protected override void ProcessForClickOutside()
	{
		KAWidget pSelectedWidget = KAUIManager.pInstance.pSelectedWidget;
		bool flag = false;
		if (null != pSelectedWidget)
		{
			foreach (GameObject pMenu in base.pUI.pMenuList)
			{
				if (null == pMenu)
				{
					continue;
				}
				UiContextSensetiveMenu componentInChildren = pMenu.GetComponentInChildren<UiContextSensetiveMenu>();
				if (componentInChildren.transform.parent == null)
				{
					continue;
				}
				KAScrollBar componentInChildren2 = componentInChildren.transform.parent.GetComponentInChildren<KAScrollBar>();
				if (null != componentInChildren2)
				{
					if (pSelectedWidget == componentInChildren2._DownArrow || pSelectedWidget == componentInChildren2._UpArrow)
					{
						flag = true;
					}
					else if (null != pSelectedWidget.transform.parent && pSelectedWidget.transform.parent.gameObject.GetInstanceID() == componentInChildren2.gameObject.GetInstanceID())
					{
						flag = true;
					}
					if (pSelectedWidget.name == "ArrowLeftBkg" || pSelectedWidget.name == "ArrowRightBkg")
					{
						flag = true;
					}
				}
				if (!flag)
				{
					ContextWidgetUserData contextWidgetUserData = (ContextWidgetUserData)pSelectedWidget.GetUserData();
					if (contextWidgetUserData?._Data != null && contextWidgetUserData._Data.pTarget == base.gameObject)
					{
						flag = true;
					}
				}
			}
		}
		if ((pSelectedWidget != null || _UIFollowingTarget == ObClickable.pMouseOverObject) && (pSelectedWidget == null || base.pUI == pSelectedWidget.pUI || flag || InteractiveTutManager._CurrentActiveTutorialObject != null))
		{
			return;
		}
		if (!mIsFeedCSMOpened && !IsAgeUpTutorialActive())
		{
			DestroyMenu(checkProximity: false);
		}
		else
		{
			if (base.pUI == null || base.pUI.pUIContextSensitiveMenuList == null || base.pUI.pUIContextSensitiveMenuList.Count <= 0)
			{
				return;
			}
			foreach (UiContextSensetiveMenu pUIContextSensitiveMenu in base.pUI.pUIContextSensitiveMenuList)
			{
				KAWidget kAWidget = pUIContextSensitiveMenu.FindItem(_FeedInventoryCSItemName);
				if (!(kAWidget == null) && !IsAgeUpTutorialActive())
				{
					base.pUI.OnClick(kAWidget);
					break;
				}
			}
		}
	}

	private bool IsAgeUpTutorialActive()
	{
		Task task = null;
		if (!(FUEManager.pInstance != null) || !(MissionManager.pInstance != null))
		{
			return false;
		}
		return MissionManager.pInstance.GetTask(FUEManager.pInstance._AgeUpTutorialTask)?._Active ?? false;
	}

	public virtual void OnContextParentAction(string inName)
	{
		if (inName == _FeedInventoryCSItemName)
		{
			mIsFeedCSMOpened = !mIsFeedCSMOpened;
		}
	}

	public virtual void OnContextAction(string inName)
	{
		if (IsAgeUpTutorialActive() && inName != _AgeUpCSItemName)
		{
			return;
		}
		if (inName == "Store")
		{
			if (_StoreInfo != null)
			{
				StoreLoader.Load(setDefaultMenuItem: true, _StoreInfo._Category, _StoreInfo._Store, base.gameObject);
			}
			CloseMenu(checkProximity: true);
		}
		else if (inName == _StableCSItemName)
		{
			KAUI component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
			if (component != null && component.IsActive())
			{
				AvAvatar.pState = AvAvatarState.PAUSED;
				AvAvatar.EnableAllInputs(inActive: false);
				AvAvatar.SetUIActive(inActive: false);
				UiDragonsStable.OpenDragonListUI(base.gameObject);
			}
			CloseMenu(checkProximity: true);
		}
		else if (inName == _AgeUpCSItemName)
		{
			if (IsAgeUpTutorialActive())
			{
				FUEManager.pInstance._AgeUpTutorial.StartNextTutorial();
				UiDragonsAgeUp.Init(null, closeOnAgeUp: true, null, isTicketPurchased: false, null, UiDragonsAgeUp.pBundleDownloadCallback);
			}
			else
			{
				UiDragonsAgeUp.Init();
			}
			CloseMenu(checkProximity: true);
		}
		else if (inName == _CustomizeCSItemName)
		{
			JournalLoader.Load("EquipBtn", "CustomiseDragon", setDefaultMenuItem: true, null);
			CloseMenu(checkProximity: true);
		}
		else if (inName != _FeedInventoryCSItemName)
		{
			float maxMeter = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, SanctuaryManager.pCurPetInstance.pData);
			float meterValue = SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.ENERGY);
			float maxMeter2 = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.HAPPINESS, SanctuaryManager.pCurPetInstance.pData);
			float meterValue2 = SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.HAPPINESS);
			if (meterValue >= maxMeter && meterValue2 >= maxMeter2 && !MissionManager.IsTaskActive("Action", "Name", "Peteat"))
			{
				AvAvatar.SetUIActive(inActive: false);
				AvAvatar.pState = AvAvatarState.PAUSED;
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _FullEnergyText.GetLocalizedString(), base.gameObject, "OnDBClose");
			}
			else
			{
				RemoveItemFromFishInventory(inName);
			}
		}
	}

	protected virtual void OnInventoryDataSave(bool success, object inUserData)
	{
		if (success)
		{
			UserItemData userItemData = (UserItemData)inUserData;
			SanctuaryManager.pCurPetInstance.DoEat(userItemData.Item);
			if (userItemData.Quantity == 0)
			{
				RemoveDataFromStateAndDataList(_FeedInventoryCSItemName, userItemData.Item.ItemName);
			}
			UserAchievementTask.Set(_EatFishAchievementTaskID);
			if (IsFeedTypeFish(userItemData.Item.ItemName))
			{
				RaisedPetAttribute raisedPetAttribute = SanctuaryManager.pCurPetInstance.pData.FindAttrData("fish");
				int num = 1;
				if (raisedPetAttribute != null)
				{
					num = int.Parse(raisedPetAttribute.Value) + num;
				}
				SanctuaryManager.pCurPetInstance.pData.SetAttrData("fish", num.ToString(), DataType.INT);
			}
			AwardAchievementForItemUse(userItemData.Item);
			SanctuaryManager.pCurPetInstance.AIActor.PlayCustomAnim(_FeedAnim);
			UpdateFeedItems();
			if (base.pUI != null)
			{
				base.pUI.RefreshChildItems("Feed");
			}
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	protected virtual void OnDBClose()
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
	}

	public void AwardAchievementForItemUse(ItemData iData)
	{
		if (_ItemUseAchievement == null)
		{
			return;
		}
		foreach (ItemUseAchievement item in _ItemUseAchievement)
		{
			if (item._ItemID == iData.ItemID)
			{
				WsWebService.SetUserAchievementAndGetReward(item._AchievementID, AchievementEventHandler, item._ShowReward);
			}
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
				GameUtilities.AddRewards((AchievementReward[])inObject, inUseRewardManager: false, (bool)inUserData);
			}
			else
			{
				UtDebug.LogError("!!!" + inType.ToString() + " did not return valid object!!!!");
			}
			break;
		}
	}

	protected bool IsFeedTypeFish(string inName)
	{
		if (CommonInventoryData.pInstance == null)
		{
			return false;
		}
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(_FishCategoryID);
		if (items == null)
		{
			return false;
		}
		UserItemData[] array = items;
		foreach (UserItemData userItemData in array)
		{
			if (inName == userItemData.Item.ItemName)
			{
				return true;
			}
		}
		return false;
	}

	protected ItemData GetFeedItemData(string inName)
	{
		if (CommonInventoryData.pInstance == null)
		{
			return null;
		}
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(_DragonsFeedCategoryID);
		if (items == null)
		{
			return null;
		}
		UserItemData[] array = items;
		foreach (UserItemData userItemData in array)
		{
			if (inName == userItemData.Item.ItemName)
			{
				return userItemData.Item;
			}
		}
		return null;
	}

	protected void RemoveItemFromFishInventory(string inName)
	{
		if (SanctuaryManager.pCurPetInstance == null || CommonInventoryData.pInstance == null)
		{
			return;
		}
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(_DragonsFeedCategoryID);
		if (items == null)
		{
			return;
		}
		UserItemData[] array = items;
		foreach (UserItemData userItemData in array)
		{
			if (!(inName != userItemData.Item.ItemName))
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				CommonInventoryData.pInstance.RemoveItem(userItemData.Item.ItemID, updateServer: false);
				CommonInventoryData.pInstance.Save(OnInventoryDataSave, userItemData);
				break;
			}
		}
	}

	protected void RemoveDataFromStateAndDataList(string inName, string inChildName)
	{
		ContextData contextData = GetContextData(inName);
		if (contextData != null)
		{
			RemoveFromDataList(inChildName);
			contextData.RemoveChildName(inChildName);
			UpdateChildrenData();
		}
	}

	protected override void Update()
	{
		if (base.pUI != null && ((SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted && base.pCurrentPriority == ContextSensitiveStateType.ONCLICK) || RsResourceManager.pLevelLoading || (AvAvatar.pState == AvAvatarState.PAUSED && GetType() == typeof(PetCSM) && (InteractiveTutManager._CurrentActiveTutorialObject == null || !InteractiveTutManager._CurrentActiveTutorialObject.name.Equals(_MultipleDragonTutorialName)))))
		{
			DestroyMenu(checkProximity: false);
		}
		base.Update();
	}

	protected override void OnActivate()
	{
		if (!(SanctuaryManager.pCurPetInstance == null) && !SanctuaryManager.pCurPetInstance.pIsMounted)
		{
			bool flag = true;
			if (AvAvatar.pObject != null && AvAvatar.pObject.GetComponent<AvAvatarController>().pPlayerCarrying)
			{
				flag = false;
			}
			if (flag)
			{
				base.OnActivate();
			}
		}
	}

	protected override void DestroyMenu(bool checkProximity)
	{
		if (base.pUI != null)
		{
			KAUI.RemoveExclusive(base.pUI);
		}
		mIsFeedCSMOpened = false;
		base.DestroyMenu(checkProximity);
		AvAvatar.pInputEnabled = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		switch (inEvent)
		{
		case MissionEvent.TASK_COMPLETE:
			CloseMenu(checkProximity: true);
			break;
		case MissionEvent.MISSION_COMPLETE:
			CloseMenu(checkProximity: true);
			break;
		}
	}

	public void OnStableUIOpened(UiDragonsStable UiStable)
	{
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "StableClicked");
		}
	}
}
