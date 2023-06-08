using System;
using SOD.Event;
using UnityEngine;

public class StableQuestSlotWidget : KAWidget
{
	private TimedMissionSlotData mMissionSlotData;

	private bool mInitialized;

	private KAWidget mLogText;

	private KAWidget mMissionUnavailableText;

	private KAWidget mReadyTemplate;

	private KAWidget mInProgressTemplate;

	private KAWidget mCooldownTemplate;

	private KAWidget mCompleteTemplate;

	private KAWidget mReadyTemplateTimer;

	private KAWidget mMissionDetails;

	private KAWidget mSlotReadyTitle;

	private KAWidget mSlotInprogressTitle;

	private KAWidget mSlotCompleteTitle;

	private KAWidget mInProgressTemplateTimer;

	private KAWidget mInProgressBar;

	private KAWidget mCooldownTemplateTimer;

	private KAWidget mSlotCooldownCost;

	private KAWidget mSlotLock;

	private KAWidget mMissionIcon;

	public RewardWidget mRewardWidget;

	private int mSlotUpdateTimer = -1;

	private TimedMissionState mCurrentSlotState = TimedMissionState.Default;

	private AdEventType mAdEventType;

	private SlotType mSlotType;

	private LocaleString[] mStoryLogs;

	public TimedMissionSlotData pMissionSlotData => mMissionSlotData;

	public TimedMissionState pCurrentSlotState => mCurrentSlotState;

	public KAWidget pInProgressTemplate => mInProgressTemplate;

	public void InitSlotWidget(int slotID)
	{
		mMissionSlotData = TimedMissionManager.pInstance.GetSlotData(slotID);
		if (mMissionSlotData.State == TimedMissionState.None || mMissionSlotData.pMission == null)
		{
			TimedMissionManager.pInstance.ResetSlot(mMissionSlotData);
		}
		mInitialized = true;
	}

	public void Start()
	{
		mSlotLock = FindChildItem("Lock");
		mReadyTemplate = FindChildItem("SlotReadyTemplate");
		mInProgressTemplate = FindChildItem("SlotInProgressTemplate");
		mCooldownTemplate = FindChildItem("SlotCoolDownTemplate");
		mCompleteTemplate = FindChildItem("SlotCompleteTemplate");
		mSlotCompleteTitle = mCompleteTemplate.FindChildItem("TxtSlotTitle");
		mSlotReadyTitle = mReadyTemplate.FindChildItem("TxtSlotTitle");
		mReadyTemplateTimer = mReadyTemplate.FindChildItem("TxtTime");
		mSlotInprogressTitle = mInProgressTemplate.FindChildItem("TxtSlotTitle");
		mInProgressTemplateTimer = mInProgressTemplate.FindChildItem("TxtTime");
		mInProgressBar = mInProgressTemplate.FindChildItem("BkgMeter");
		mLogText = mInProgressTemplate.FindChildItem("TxtLog");
		mCooldownTemplateTimer = mCooldownTemplate.FindChildItem("TxtTime");
		mSlotCooldownCost = mCooldownTemplate.FindChildItem("BkgGem");
		mMissionUnavailableText = FindChildItem("TxtNoMissionAvailable");
		mRewardWidget = (RewardWidget)FindChildItem("XPReward");
		mMissionDetails = mReadyTemplate.FindChildItem("MissionDetails");
		mMissionIcon = FindChildItem("IcoSeason");
		HandleAdButtons();
	}

	private void SetBackground(KAWidget stateWidget, SlotType slotType)
	{
		mSlotType = slotType;
		KAWidget kAWidget = stateWidget.FindChildItem("Bkgs");
		if (kAWidget != null)
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem("BkgType_Default");
			int num = (int)slotType;
			KAWidget kAWidget3 = kAWidget.FindChildItem("BkgType_" + num);
			if (kAWidget3 != null)
			{
				kAWidget2.SetVisibility(inVisible: false);
				kAWidget3.SetVisibility(inVisible: true);
			}
			else
			{
				kAWidget2.SetVisibility(inVisible: true);
			}
		}
		if (mSlotType == SlotType.Timed && (bool)mMissionIcon)
		{
			SetEventIcon();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!mInitialized)
		{
			return;
		}
		TimedMissionState timedMissionState = TimedMissionState.None;
		if (mMissionSlotData != null && mMissionSlotData.pMission != null)
		{
			timedMissionState = mMissionSlotData.State;
		}
		StateChangeInit(timedMissionState);
		string empty = string.Empty;
		switch (timedMissionState)
		{
		case TimedMissionState.Started:
		{
			TimedMissionManager.pInstance.CheckMissionCompleted(mMissionSlotData);
			TimeSpan completionTime = TimedMissionManager.pInstance.GetCompletionTime(mMissionSlotData);
			if (mLogText != null)
			{
				float num = mMissionSlotData.pMission.Duration * 60;
				int num2 = Mathf.FloorToInt((float)((double)num - completionTime.TotalSeconds) * (float)mStoryLogs.Length / num);
				if (mStoryLogs.Length != 0 && num2 > -1 && num2 < mStoryLogs.Length)
				{
					mLogText.SetText(mStoryLogs[num2].GetLocalizedString());
				}
			}
			if (mSlotUpdateTimer != (int)completionTime.TotalSeconds)
			{
				mSlotUpdateTimer = (int)completionTime.TotalSeconds;
				empty = GetTimerString(mSlotUpdateTimer);
				mInProgressTemplateTimer.SetText(empty);
				float progressLevel = (float)completionTime.TotalSeconds / (float)(mMissionSlotData.pMission.Duration * 60);
				mInProgressBar.SetProgressLevel(progressLevel);
			}
			break;
		}
		case TimedMissionState.CoolDown:
		{
			TimeSpan coolDownTime = TimedMissionManager.pInstance.GetCoolDownTime(mMissionSlotData);
			if (mSlotUpdateTimer != (int)coolDownTime.TotalSeconds)
			{
				mSlotUpdateTimer = (int)coolDownTime.TotalSeconds;
				empty = GetTimerString(mSlotUpdateTimer);
				mCooldownTemplateTimer.SetText(empty);
				mSlotCooldownCost.SetText(TimedMissionManager.pInstance.GetCostForCoolDown(mMissionSlotData).ToString());
			}
			if (TimedMissionManager.pInstance.GetCoolDownTime(mMissionSlotData) < TimeSpan.Zero)
			{
				TimedMissionManager.pInstance.CheckCoolDownEnd(mMissionSlotData);
			}
			break;
		}
		}
	}

	private void StateChangeInit(TimedMissionState state)
	{
		if (mCurrentSlotState == state)
		{
			return;
		}
		if (mCurrentSlotState != TimedMissionState.Default && (state == TimedMissionState.Started || state == TimedMissionState.CoolDown))
		{
			mMissionSlotData.pAdWatched = false;
			TimedMissionManager.pInstance.SaveSlotData();
		}
		mReadyTemplate.SetVisibility(state == TimedMissionState.Alotted || state == TimedMissionState.None);
		mInProgressTemplate.SetVisibility(state == TimedMissionState.Started);
		mCooldownTemplate.SetVisibility(state == TimedMissionState.CoolDown);
		mCompleteTemplate.SetVisibility(state == TimedMissionState.Ended || state == TimedMissionState.Won || state == TimedMissionState.Lost);
		mMissionUnavailableText.SetVisibility(state == TimedMissionState.None);
		if (!base.name.Contains(mMissionSlotData.Type.ToString()))
		{
			base.name = "Type : " + mMissionSlotData.Type;
		}
		mSlotUpdateTimer = -1;
		switch (state)
		{
		case TimedMissionState.None:
			SetState(KAUIState.DISABLED);
			SetBackground(mReadyTemplate, mMissionSlotData.Type);
			if (mMissionDetails != null)
			{
				mMissionDetails.SetVisibility(inVisible: false);
			}
			break;
		case TimedMissionState.Alotted:
		{
			SetState(KAUIState.INTERACTIVE);
			if (mMissionDetails != null)
			{
				mMissionDetails.SetVisibility(inVisible: true);
			}
			SetBackground(mReadyTemplate, mMissionSlotData.Type);
			SetSlotDifficulty(mMissionSlotData.pMission.Difficulty, mMissionDetails);
			mRewardWidget.SetRewards(mMissionSlotData.pMission.WinRewards.ToArray(), MissionManager.pInstance._RewardData);
			string timerString = GetTimerString(mMissionSlotData.pMission.Duration * 60);
			mReadyTemplateTimer.SetText(timerString);
			mSlotReadyTitle.SetText(mMissionSlotData.pMission.Title.GetLocalizedString());
			bool visibility = false;
			if (mMissionSlotData.Type == SlotType.Toothless)
			{
				if (!RaisedPetData.pActivePets.ContainsKey(17))
				{
					visibility = true;
				}
			}
			else if (mMissionSlotData.Type == SlotType.Member && !SubscriptionInfo.pIsMember)
			{
				visibility = true;
			}
			mSlotLock.SetVisibility(visibility);
			break;
		}
		case TimedMissionState.Started:
			mSlotUpdateTimer = -1;
			mStoryLogs = TimedMissionManager.pInstance.GetMissionLogs(mMissionSlotData.SlotID);
			SetBackground(mInProgressTemplate, mMissionSlotData.Type);
			mSlotInprogressTitle.SetText(mMissionSlotData.pMission.Title.GetLocalizedString());
			break;
		case TimedMissionState.Won:
		case TimedMissionState.Lost:
		case TimedMissionState.Ended:
			mSlotCompleteTitle.SetText(mMissionSlotData.pMission.Title.GetLocalizedString());
			break;
		case TimedMissionState.CoolDown:
			pMissionSlotData.Init();
			break;
		}
		mCurrentSlotState = state;
		HandleAdButtons();
	}

	public void ResetSlotWidget()
	{
		mCurrentSlotState = TimedMissionState.Default;
		TimedMissionManager.pInstance.ResetSlot(mMissionSlotData);
	}

	private string GetTimerString(int timeInSeconds)
	{
		int num = timeInSeconds / 3600;
		int num2 = timeInSeconds / 60 % 60;
		int num3 = timeInSeconds % 60;
		return num.ToString("d2") + ":" + num2.ToString("d2") + ":" + num3.ToString("d2");
	}

	private void SetSlotDifficulty(int DifficultyValue, KAWidget Widget)
	{
		foreach (KAWidget pChildWidget in Widget.pChildWidgets)
		{
			string text = pChildWidget.name;
			if (text.Contains("Star"))
			{
				int num = int.Parse(text.Split('_')[1]);
				pChildWidget.SetVisibility(num <= DifficultyValue);
			}
		}
	}

	private void OnLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		KAWidget kAWidget = (KAWidget)inUserData;
		if (kAWidget != null)
		{
			CoBundleItemData coBundleItemData = new CoBundleItemData(dataItem.IconName, "");
			kAWidget.SetUserData(coBundleItemData);
			coBundleItemData.LoadResource();
		}
	}

	private void HandleAdButtons()
	{
		mAdEventType = GetComponentInParent<UiStableQuestSlots>()._AdEventType;
		if (AdManager.pInstance.AdSupported(mAdEventType, AdType.REWARDED_VIDEO) && (!(StableManager.pInstance._StableQuestTutorial != null) || StableManager.pInstance._StableQuestTutorial.TutorialComplete()) && mMissionSlotData != null)
		{
			if (mMissionSlotData.State == TimedMissionState.Started && AdManager.pInstance.IsReductionTimeGreater(mAdEventType, mMissionSlotData.pMission.Duration * 60))
			{
				ShowAdButton(mInProgressTemplate.FindChildItem("BtnAds"));
			}
			else if (mMissionSlotData.State == TimedMissionState.CoolDown && AdManager.pInstance.IsReductionTimeGreater(mAdEventType, mMissionSlotData.pCoolDownDuration * 60))
			{
				ShowAdButton(mCooldownTemplate.FindChildItem("BtnAds"));
			}
		}
	}

	private void ShowAdButton(KAWidget btnAd)
	{
		if (btnAd != null && !mMissionSlotData.pAdWatched)
		{
			btnAd.SetVisibility(inVisible: true);
			if (btnAd.GetLabel() != null)
			{
				btnAd.GetLabel().text = AdManager.pInstance.GetReductionTimeText(mAdEventType);
			}
		}
	}

	private void SetEventIcon()
	{
		EventManager activeEvent = EventManager.GetActiveEvent();
		if (activeEvent != null && activeEvent.EventInProgress() && !activeEvent.GracePeriodInProgress() && (bool)activeEvent._StableQuestIcon)
		{
			mMissionIcon.SetTexture(activeEvent._StableQuestIcon);
		}
	}
}
