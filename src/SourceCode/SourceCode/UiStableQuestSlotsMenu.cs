using System.Collections.Generic;
using UnityEngine;

public class UiStableQuestSlotsMenu : KAUIMenu, IAdResult
{
	private bool mPopulateWidgets = true;

	private bool mCheckForUpsellResult;

	private KAUIGenericDB mCoolDownGenericDB;

	private UiStableQuestSlots mParentSlotsUi;

	private TimedMissionSlotData mSlotData;

	private KAWidget mClickedAdBtn;

	public KAWidget pInProgressSlotWidget { get; set; }

	protected override void Start()
	{
		base.Start();
		mParentSlotsUi = (UiStableQuestSlots)_ParentUi;
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			PopulateSlots(TimedMissionManager.pInstance.pTimedMissionSlotList);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnSlot" || inWidget.name == "BtnDetails")
		{
			StableQuestSlotWidget stableQuestSlotWidget = (StableQuestSlotWidget)inWidget.GetParentItem().GetParentItem();
			mSlotData = stableQuestSlotWidget.pMissionSlotData;
			if (mSlotData == null)
			{
				return;
			}
			bool flag = false;
			if (mSlotData.Type == SlotType.Toothless)
			{
				if (!RaisedPetData.pActivePets.ContainsKey(17))
				{
					flag = true;
					if (SubscriptionInfo.pIsMember)
					{
						UserNotifyDragonTicket.pInstance.CheckTickets(null, OnUNDragonTicketDone);
					}
					else
					{
						GameUtilities.DisplayGenericDB("PfKAUIGenericDB", mParentSlotsUi._BuyToothlessText.GetLocalizedString(), null, base.gameObject, "ShowMembershipPurchaseDB", "OnDBClose", null, null, inDestroyOnClick: true);
					}
				}
			}
			else if (mSlotData.Type == SlotType.Member && !SubscriptionInfo.pIsMember)
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", mParentSlotsUi._NonMemberText.GetLocalizedString(), null, base.gameObject, "ShowMembershipPurchaseDB", "OnDBClose", null, null, inDestroyOnClick: true);
				flag = true;
			}
			if (!flag)
			{
				mParentSlotsUi._StableQuestMainUI._StableQuestDetailsUI.SetSlotData(mSlotData);
				pInProgressSlotWidget = stableQuestSlotWidget.pInProgressTemplate;
				mParentSlotsUi._StableQuestMainUI._StableQuestDetailsUI.HandleAdButton();
				mParentSlotsUi.SetVisibility(inVisible: false);
			}
		}
		else if (inWidget.name == "BtnComplete")
		{
			StableQuestSlotWidget stableQuestSlotWidget2 = (StableQuestSlotWidget)inWidget.GetParentItem().GetParentItem();
			mSlotData = stableQuestSlotWidget2.pMissionSlotData;
			if (mSlotData != null)
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				SetInteractive(interactive: false);
				TimedMissionManager.pInstance.CompleteMission(mSlotData, TimeMissionCompleteCallBack);
			}
		}
		else if (inWidget.name == "BtnBuy")
		{
			StableQuestSlotWidget stableQuestSlotWidget3 = (StableQuestSlotWidget)inWidget.GetParentItem().GetParentItem();
			mSlotData = stableQuestSlotWidget3.pMissionSlotData;
			if (mSlotData != null)
			{
				if (Money.pCashCurrency >= TimedMissionManager.pInstance.GetCostForCoolDown(mSlotData))
				{
					string localizedString = mParentSlotsUi._CooldownSlotPurchaseText.GetLocalizedString();
					localizedString = localizedString.Replace("{{GEMS}}", TimedMissionManager.pInstance.GetCostForCoolDown(mSlotData).ToString());
					new LocaleString(localizedString)._ID = 0;
					mCoolDownGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", localizedString, null, base.gameObject, "BuyCoolDown", "OnDBClose", null, null, inDestroyOnClick: true);
				}
				else
				{
					GameUtilities.DisplayGenericDB("PfKAUIGenericDB", mParentSlotsUi._NotEnoughFeeText.GetLocalizedString(), null, base.gameObject, "BuyGemsOnline", "OnDBClose", null, null, inDestroyOnClick: true);
				}
			}
		}
		else if (inWidget.name == "BtnAds")
		{
			OnAdButtonClick(inWidget);
		}
	}

	public void OnAdButtonClick(KAWidget inWidget)
	{
		if (inWidget != null && AdManager.pInstance.AdAvailable(mParentSlotsUi._AdEventType, AdType.REWARDED_VIDEO))
		{
			StableQuestSlotWidget stableQuestSlotWidget = (StableQuestSlotWidget)inWidget.GetParentItem().GetParentItem();
			mSlotData = stableQuestSlotWidget.pMissionSlotData;
			mClickedAdBtn = inWidget;
			AdManager.DisplayAd(mParentSlotsUi._AdEventType, AdType.REWARDED_VIDEO, base.gameObject);
		}
	}

	private void ShowMembershipPurchaseDB()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
	}

	private void BuyGemsOnline()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void OnDBClose()
	{
	}

	protected override void Update()
	{
		base.Update();
		if (mCoolDownGenericDB != null && (mSlotData == null || mSlotData.State != TimedMissionState.CoolDown))
		{
			Object.Destroy(mCoolDownGenericDB.gameObject);
			mCoolDownGenericDB = null;
		}
		if (mCheckForUpsellResult && ParentData.pIsReady)
		{
			mCheckForUpsellResult = false;
			if (UserNotifyDragonTicket.pInstance != null)
			{
				mParentSlotsUi.SetVisibility(inVisible: false);
				UserNotifyDragonTicket.pInstance.CheckTickets(null, OnUNDragonTicketDone);
			}
		}
	}

	private void OnUNDragonTicketDone(bool inSuccess)
	{
		if (mParentSlotsUi != null)
		{
			mParentSlotsUi.SetVisibility(inVisible: true);
		}
		if (!inSuccess)
		{
			if (!UserNotifyDragonTicket.pInstance._IsNestAvailable)
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", UserNotifyDragonTicket.pInstance._NestNotAvailableText.GetLocalizedString(), null, base.gameObject, null, null, "OnDBClose", null, inDestroyOnClick: true);
			}
			else
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", mParentSlotsUi._BuyToothlessText.GetLocalizedString(), null, base.gameObject, "ShowMembershipPurchaseDB", "OnDBClose", null, null, inDestroyOnClick: true);
			}
		}
		else
		{
			ResetSlot(mSlotData.SlotID);
		}
	}

	private void OnIAPStoreClosed()
	{
		if (SubscriptionInfo.pIsMember)
		{
			mCheckForUpsellResult = true;
		}
	}

	private void BuyCoolDown()
	{
		mCoolDownGenericDB = null;
		KAUICursorManager.SetDefaultCursor("Loading");
		TimedMissionManager.pInstance.BuyCoolDownTime(mSlotData, OnCoolDownPurchase);
		mParentSlotsUi.SetInteractive(interactive: false);
	}

	private void OnCoolDownPurchase(bool success)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		mParentSlotsUi.SetInteractive(interactive: true);
		if (success)
		{
			ResetSlot(mSlotData.SlotID);
		}
	}

	private void TimeMissionCompleteCallBack(bool success, bool winStatus, AchievementReward[] reward)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		if (success)
		{
			mParentSlotsUi._StableQuestMainUI._StableQuestResultsUI.SetSlotData(mSlotData, reward, winStatus);
			mParentSlotsUi._StableQuestMainUI._StableQuestResultsUI.SetVisibility(inVisible: true);
			mParentSlotsUi.SetVisibility(inVisible: false);
		}
	}

	public void ResetSlot(int slotID)
	{
		foreach (StableQuestSlotWidget item in GetItems())
		{
			if (item.pMissionSlotData.SlotID == slotID)
			{
				item.ResetSlotWidget();
				break;
			}
		}
	}

	private void PopulateSlots(List<TimedMissionSlotData> slotData)
	{
		if (mPopulateWidgets)
		{
			mPopulateWidgets = false;
			ClearItems();
			slotData.Sort((TimedMissionSlotData x, TimedMissionSlotData y) => x.Type - y.Type);
			for (int i = 0; i < slotData.Count; i++)
			{
				StableQuestSlotWidget obj = (StableQuestSlotWidget)AddWidget(GetSlotTemplate(slotData[i].Type).name, null);
				obj.InitSlotWidget(slotData[i].SlotID);
				obj.SetVisibility(inVisible: true);
			}
			mCurrentGrid.repositionNow = true;
		}
	}

	private KAWidget GetSlotTemplate(SlotType slotType)
	{
		return _Template;
	}

	public void OnAdWatched()
	{
		AdManager.pInstance.LogAdWatchedEvent(mParentSlotsUi._AdEventType, "StableQuest");
		AdManager.pInstance.SyncAdAvailableCount(mParentSlotsUi._AdEventType, isConsumed: true);
		StableQuestSlotWidget stableQuestSlotWidget = (StableQuestSlotWidget)mClickedAdBtn.GetParentItem().GetParentItem();
		if (stableQuestSlotWidget.pCurrentSlotState == TimedMissionState.Started)
		{
			TimedMission currentMission = TimedMissionManager.pInstance.GetCurrentMission(mSlotData);
			ReduceDuration(currentMission.Duration);
		}
		else if (stableQuestSlotWidget.pCurrentSlotState == TimedMissionState.CoolDown)
		{
			ReduceDuration(mSlotData.pCoolDownDuration);
		}
	}

	public void OnAdFailed()
	{
		mClickedAdBtn.SetVisibility(inVisible: false);
		UtDebug.LogError("OnAdFailed for event:- " + mParentSlotsUi._AdEventType);
	}

	public void OnAdSkipped()
	{
	}

	public void OnAdClosed()
	{
	}

	public void OnAdFinished(string eventDataRewardString)
	{
	}

	public void OnAdCancelled()
	{
	}

	private void ReduceDuration(int duration)
	{
		int reductionTime = AdManager.pInstance.GetReductionTime(mParentSlotsUi._AdEventType, duration * 60);
		mSlotData.StartDate = mSlotData.StartDate.AddSeconds(-reductionTime);
		mSlotData.pAdWatched = true;
		UiStableQuestDetail stableQuestDetailsUI = mParentSlotsUi._StableQuestMainUI._StableQuestDetailsUI;
		if (stableQuestDetailsUI != null)
		{
			stableQuestDetailsUI.SetGemsText();
			if (stableQuestDetailsUI.pBtnAds != null)
			{
				stableQuestDetailsUI.pBtnAds.SetVisibility(inVisible: false);
			}
		}
		TimedMissionManager.pInstance.SaveSlotData();
	}
}
