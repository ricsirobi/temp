using System.Collections.Generic;

public class KAUIStoreSyncPopUp : KAUIPopup
{
	public PurchaseAchievement[] _PurchaseAchievements;

	public PurchaseConfirmationData[] _PurchaseConfirmationData;

	public LocaleString _InitialStatusText = new LocaleString("Please wait while your purchase is being processed.");

	public LocaleString _PurchaseFailedText = new LocaleString("Your purchase failed, please try again later.");

	public LocaleString _PurchaseSuccessText = new LocaleString("Your purchase is completed.");

	public LocaleString _MissionUnlockText = new LocaleString("'Go see {xx} to start your quest!");

	protected KAUIStore mStoreUI;

	protected KAWidget mProgress;

	protected KAWidget mContinue;

	protected KAWidget mStatus;

	private List<CommonInventoryResponseItem> mLastReceivedItems = new List<CommonInventoryResponseItem>();

	private bool mIsPurchaseSuccess;

	private bool mWaitSubscriptionRefresh;

	private bool mMembershipPurchased;

	private string mPurchaseConfirmationMsg = "";

	protected override void Start()
	{
		base.Start();
		mStoreUI = base.transform.parent.GetComponentInChildren<KAUIStore>();
		mProgress = FindItem("aniProgress");
		mContinue = FindItem("btnContinue");
		mStatus = FindItem("txtSyncStatus");
	}

	public virtual void EndPurchase(bool success, string inPurchaseConfirmationMsg = "")
	{
		if (success)
		{
			NotificationData.SetLastPurchaseTime();
			string text = ((!string.IsNullOrEmpty(inPurchaseConfirmationMsg)) ? inPurchaseConfirmationMsg : _PurchaseSuccessText.GetLocalizedString());
			mStatus.SetText(text);
			if (KAUIStore.pInstance.pStoreInfo._CompletedVO != null)
			{
				mStoreUI._SoundChannel = SnChannel.Play(KAUIStore.pInstance.pStoreInfo._CompletedVO, "VO_Pool", inForce: true, base.gameObject);
			}
		}
		else
		{
			mStatus.SetText(_PurchaseFailedText.GetLocalizedString());
		}
		mIsPurchaseSuccess = success;
		mProgress.SetVisibility(inVisible: false);
		mContinue.SetVisibility(inVisible: true);
	}

	protected virtual void ProcessItem(PurchaseItemData currentPurchaseItem)
	{
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Buy", currentPurchaseItem._StoreID, currentPurchaseItem._ItemData);
		}
		mStoreUI._BoughtItemID = currentPurchaseItem._ItemData.ItemID;
		if (_PurchaseAchievements == null)
		{
			return;
		}
		PurchaseAchievement[] purchaseAchievements = _PurchaseAchievements;
		foreach (PurchaseAchievement purchaseAchievement in purchaseAchievements)
		{
			if (!currentPurchaseItem._ItemData.HasCategory(purchaseAchievement._CategoryID))
			{
				continue;
			}
			if (purchaseAchievement._IsAchievementTask)
			{
				AchievementTask achievementTask = null;
				if (purchaseAchievement._AchievementID > 0)
				{
					achievementTask = new AchievementTask(purchaseAchievement._AchievementID, "", 0, currentPurchaseItem._Quantity);
				}
				AchievementTask achievementTask2 = null;
				if (purchaseAchievement._GroupAchievementID > 0)
				{
					achievementTask2 = UserProfile.pProfileData.GetGroupAchievement(purchaseAchievement._GroupAchievementID, "", 0, currentPurchaseItem._Quantity);
				}
				UserAchievementTask.Set(achievementTask, achievementTask2);
			}
			else
			{
				WsWebService.SetUserAchievementAndGetReward(purchaseAchievement._AchievementID, AchievementEventHandler, null);
			}
		}
	}

	private void ProcessForMissionUnlock(ItemData inPurchasedItemData, ref string inPurchaseConfirmationText)
	{
		List<Mission> allMissions = MissionManager.pInstance.GetAllMissions(-1);
		if (allMissions == null)
		{
			return;
		}
		for (int i = 0; i < allMissions.Count; i++)
		{
			if (!IsMissionQualified(allMissions[i], inPurchasedItemData.ItemID) || MissionManager.pInstance.IsLocked(allMissions[i]))
			{
				continue;
			}
			MissionManager.pInstance.RefreshMissions(!allMissions[i].pMustAccept);
			if (allMissions[i].pMustAccept)
			{
				if (string.IsNullOrEmpty(inPurchaseConfirmationText))
				{
					inPurchaseConfirmationText = _PurchaseSuccessText.GetLocalizedString();
				}
				inPurchaseConfirmationText += " ";
				string nPCName = MissionManagerDO.GetNPCName(GetTaskWho(allMissions[i]));
				inPurchaseConfirmationText += _MissionUnlockText.GetLocalizedString().Replace("{xx}", nPCName);
			}
			else if (!allMissions[i].pStarted)
			{
				List<Task> tasks = new List<Task>();
				MissionManager.pInstance.GetNextTask(allMissions[i], ref tasks);
				if (tasks.Count > 0)
				{
					MissionManagerDO.SetCurrentActiveTask(tasks[0].TaskID, waitForRefresh: true);
				}
			}
			break;
		}
	}

	private bool IsMissionQualified(Mission inMission, int inItemID)
	{
		foreach (PrerequisiteItem prerequisite in inMission.MissionRule.Prerequisites)
		{
			if (prerequisite.Type == PrerequisiteRequiredType.Item)
			{
				int num = UtStringUtil.Parse(prerequisite.Value, -1);
				if (num != -1 && num == inItemID && CommonInventoryData.pInstance.GetQuantity(num) + ParentData.pInstance.pInventory.GetQuantity(num) >= prerequisite.Quantity)
				{
					return true;
				}
			}
		}
		return false;
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

	public virtual void OnShowPopup(bool inVisible)
	{
		if (inVisible)
		{
			if (KAUIStore.pInstance.pMainMenu != null)
			{
				KAUIStore.pInstance.pMainMenu.SetState(KAUIState.NOT_INTERACTIVE);
			}
			if (KAUIStoreCategory.pInstance != null)
			{
				KAUIStoreCategory.pInstance.SetState(KAUIState.NOT_INTERACTIVE);
				KAUIStoreCategory.pInstance._Menu.SetState(KAUIState.NOT_INTERACTIVE);
				KAUIStoreCategory.pInstance._IdleMgr.StopIdles();
			}
			mIsPurchaseSuccess = false;
			mLastReceivedItems.Clear();
			mProgress.SetVisibility(inVisible: true);
			mContinue.SetVisibility(inVisible: false);
			mStatus.SetTextByID(_InitialStatusText._ID, _InitialStatusText._Text);
			PurchaseItemData currentPurchaseItem = mStoreUI._CurrentPurchaseItem;
			if (currentPurchaseItem._ItemData.HasAttribute("Parent"))
			{
				ParentData.pInstance.pInventory.AddPurchaseItem(currentPurchaseItem._ItemData.ItemID, currentPurchaseItem._Quantity, ItemPurchaseSource.GENERAL_STORE.ToString());
				ParentData.pInstance.pInventory.DoPurchase(currentPurchaseItem._ItemData.GetPurchaseType(), currentPurchaseItem._StoreID, OnPurchaseComplete);
			}
			else
			{
				CommonInventoryData.pInstance.AddPurchaseItem(currentPurchaseItem._ItemData.ItemID, currentPurchaseItem._Quantity, ItemPurchaseSource.GENERAL_STORE.ToString());
				CommonInventoryData.pInstance.DoPurchase(CommonInventoryDataInstance.LoadByUserID(UserInfo.pInstance.UserID), currentPurchaseItem._ItemData.GetPurchaseType(), currentPurchaseItem._StoreID, currentPurchaseItem._ItemData.HasCategory(462), OnPurchaseComplete);
			}
		}
		else
		{
			if (KAUIStore.pInstance.pMainMenu != null)
			{
				KAUIStore.pInstance.pMainMenu.SetState(KAUIState.INTERACTIVE);
			}
			if (!(KAUIStoreCategory.pInstance == null))
			{
				KAUIStoreCategory.pInstance.SetState(KAUIState.INTERACTIVE);
				KAUIStoreCategory.pInstance._Menu.SetState(KAUIState.INTERACTIVE);
				KAUIStoreCategory.pInstance._IdleMgr.StartIdles();
			}
		}
	}

	public void OnPurchaseComplete(CommonInventoryResponse ret)
	{
		if (ret != null && ret.Success)
		{
			CommonInventoryResponseItem[] commonInventoryIDs = ret.CommonInventoryIDs;
			foreach (CommonInventoryResponseItem item in commonInventoryIDs)
			{
				mLastReceivedItems.Add(item);
			}
			mStoreUI.pChooseUI.PurchaseSuccessful(mLastReceivedItems.ToArray());
			ProcessItem(mStoreUI._CurrentPurchaseItem);
			string inPurchaseConfirmationText = "";
			ItemData itemData = mStoreUI._CurrentPurchaseItem._ItemData;
			if (itemData.HasAttribute("TrialMembershipRedemptionValue"))
			{
				ParentData.Reset();
				ParentData.Init();
				SubscriptionInfo.Reset();
				SubscriptionInfo.Init();
				UserProfile.Reset();
				UserProfile.Init();
				mWaitSubscriptionRefresh = true;
				mMembershipPurchased = true;
			}
			int attribute = itemData.GetAttribute("ExpansionMissionID", -1);
			if (attribute != -1)
			{
				ExpansionUnlock.UpsellDataMissionMap upsellInfo = ExpansionUnlock.pInstance.GetUpsellInfo(attribute);
				if (upsellInfo != null)
				{
					inPurchaseConfirmationText = upsellInfo.GetUpsellConfirmationMsg();
				}
			}
			if (_PurchaseConfirmationData != null)
			{
				for (int j = 0; j < _PurchaseConfirmationData.Length; j++)
				{
					if (itemData.ItemID == _PurchaseConfirmationData[j]._ItemID || (_PurchaseConfirmationData[j]._CategoryID > 0 && itemData.HasCategory(_PurchaseConfirmationData[j]._CategoryID)))
					{
						inPurchaseConfirmationText = _PurchaseConfirmationData[j]._Text.GetLocalizedString();
					}
				}
			}
			ProcessForMissionUnlock(itemData, ref inPurchaseConfirmationText);
			mStoreUI._CurrentPurchaseItem = null;
			if (mWaitSubscriptionRefresh)
			{
				mPurchaseConfirmationMsg = inPurchaseConfirmationText;
			}
			else
			{
				EndPurchase(success: true, inPurchaseConfirmationText);
			}
		}
		else
		{
			EndPurchase(success: false);
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		OnShowPopup(inVisible);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mContinue)
		{
			CloseSyncUI(mIsPurchaseSuccess);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mWaitSubscriptionRefresh && SubscriptionInfo.pIsReady && ParentData.pIsReady)
		{
			mWaitSubscriptionRefresh = false;
			EndPurchase(success: true, mPurchaseConfirmationMsg);
			mPurchaseConfirmationMsg = "";
		}
	}

	protected virtual void CloseSyncUI(bool isPurchaseSuccess)
	{
		SetVisibility(inVisible: false);
		mStoreUI.SetState(KAUIState.INTERACTIVE);
		mStoreUI.pChooseMenu.OnSyncUIClosed(isPurchaseSuccess, mLastReceivedItems.ToArray());
		mStoreUI.SetStoreMode(KAUIStore.StoreMode.Choose, update: true, mStoreUI.pStoreName, mStoreUI.pCategory, mStoreUI.pFilter);
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "ContinueClicked");
		}
		if (mStoreUI.pChooseUI != null)
		{
			mStoreUI.pChooseUI.OnSyncUIClosed(isPurchaseSuccess);
		}
	}

	private string GetTaskWho(Mission inMission)
	{
		string text = "";
		if (inMission != null && inMission.pData != null && inMission.pData.Offers != null && inMission.pData.Offers.Count > 0)
		{
			return inMission.pData.Offers[0].NPC;
		}
		return GetTaskWho(FindFirstTask(inMission));
	}

	private string GetTaskWho(Task inTask)
	{
		string text = "";
		if (inTask == null)
		{
			return text;
		}
		if (inTask.pData != null && inTask.pData.Offers != null && inTask.pData.Offers.Count > 0)
		{
			text = inTask.pData.Offers[0].NPC;
		}
		Mission mission = inTask._Mission;
		while (string.IsNullOrEmpty(text) && mission != null)
		{
			if (mission.pData != null && mission.pData.Offers != null && mission.pData.Offers.Count > 0)
			{
				text = mission.pData.Offers[0].NPC;
			}
			mission = mission._Parent;
		}
		return text;
	}

	private Task FindFirstTask(Mission inMission)
	{
		if (inMission == null)
		{
			return null;
		}
		List<Task> tasks = new List<Task>();
		MissionManager.pInstance.GetNextTask(inMission, ref tasks);
		if (tasks.Count <= 0)
		{
			return null;
		}
		using (List<Task>.Enumerator enumerator = tasks.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return null;
	}
}
