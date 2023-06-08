using System;
using System.Collections.Generic;
using UnityEngine;

public class UiStableQuestDetail : KAUI
{
	public UiStableQuestMain _StableQuestMainUI;

	public LocaleString _ForceCompleteMissionFailText = new LocaleString("[REVIEW] Failed to complete mission instantly");

	public LocaleString _ForceCompleteMissionText = new LocaleString("Pay {{GEMS}} gems complete mission instantly?");

	public LocaleString _EnergyRequiredText = new LocaleString("{{Result}} energy required pre dragon");

	public LocaleString _NotEnoughFeeText = new LocaleString("You do not have enough gems to pay, Please buy more!");

	public LocaleString _MinDragonSelectedText = new LocaleString("You have to pick {{COUNT}} dragon to start the mission.");

	public LocaleString _MinDragonSelectedPluralText = new LocaleString("You have to pick {{COUNT}} dragons to start the mission.");

	public LocaleString _TrashMissionAlertText = new LocaleString("Do you want to trash this mission");

	public LocaleString _CompleteNowMissionAlertText = new LocaleString("Do you want to force complete this mission");

	private KAWidget mMissionTitle;

	private KAWidget mMissionTime;

	private KAWidget mMissionDifficultyStars;

	private KAWidget mMissionInfo;

	private KAWidget mMissionQuestIcon;

	private KAWidget mMissionTipsIcon;

	private KAWidget mMissionTipsDetails;

	private KAWidget mMissionDetailScreen;

	public RewardWidget mRewardWidget;

	private KAWidget mMissionStartMission;

	private KAWidget mMissionInprogress;

	private KAWidget mBackButton;

	private KAWidget mMissionCompleteBtn;

	private KAWidget mMissionForceCompleteBtn;

	private KAWidget mMissionStartBtn;

	private KAWidget mMissionTrashBtn;

	private KAWidget mDragonProgressBar;

	private KAWidget mDragonProgressPercentage;

	private KAWidget mDragonsSelectedCount;

	private KAWidget mDragonsEnergyCost;

	private bool mIsForceCompletingMission;

	private TimedMissionState mPreviousMissionState;

	private TimedMissionSlotData mSlotData;

	private TimedMission mMissionData;

	private List<int> mLocalPetIDs = new List<int>();

	public UiStableQuestDragonSelect _PopUpDragonSelectionUI;

	public UiStableQuestMissionStart _PopUpStartMissionUI;

	private UiStableQuestDragonsSelectedMenu mDragonsSelectedMenu;

	private UiStableQuestLogMenu mLogMenu;

	private int mPrevTimer = -1;

	private int mLastLogReached = -1;

	public Color _QuestProgressFromColor = Color.red;

	public Color _QuestProgressToColor = Color.green;

	private AdEventType mAdEventType;

	public KAWidget pBtnAds { get; set; }

	public TimedMissionSlotData pCurrentSlotData => mSlotData;

	public TimedMission pCurrentMissionData => mMissionData;

	public List<int> pLocalPetIDs
	{
		get
		{
			return mLocalPetIDs;
		}
		set
		{
			mLocalPetIDs = value;
		}
	}

	public UiStableQuestDragonsSelectedMenu pDragonsSelectedMenu => mDragonsSelectedMenu;

	protected override void Start()
	{
		base.Start();
		mMissionTitle = FindItem("TxtHeading");
		mMissionTime = FindItem("TxtMissionTime");
		mMissionDifficultyStars = FindItem("Difficulty");
		mMissionInfo = FindItem("TxtMissionInfo");
		mMissionQuestIcon = FindItem("IcoQuest");
		mMissionTipsIcon = FindItem("IcoHint");
		mMissionTipsDetails = FindItem("TxtMissionTips");
		mMissionDetailScreen = FindItem("MissionDetailScreen");
		mRewardWidget = (RewardWidget)mMissionDetailScreen.FindChildItem("XPReward");
		mMissionStartMission = FindItem("StartMissionDetailScreen");
		mMissionInprogress = FindItem("InProgressDetailScreen");
		mMissionCompleteBtn = FindItem("BtnComplete");
		mMissionForceCompleteBtn = FindItem("BtnCompleteNow");
		mBackButton = FindItem("BackBtn");
		mMissionStartBtn = FindItem("BtnStart");
		mMissionTrashBtn = FindItem("BtnTrash");
		mDragonProgressBar = FindItem("AniWinPercentageMeter");
		mDragonProgressPercentage = FindItem("TxtWinPercentage");
		mDragonsSelectedCount = FindItem("TxtDragonCount");
		mDragonsEnergyCost = FindItem("TxtDragonEnergyCost");
		pBtnAds = FindItem("BtnAds");
		mDragonsSelectedMenu = (UiStableQuestDragonsSelectedMenu)_MenuList[0];
		mLogMenu = (UiStableQuestLogMenu)_MenuList[1];
		mAdEventType = _StableQuestMainUI._StableQuestSlotsUI._AdEventType;
	}

	protected override void Update()
	{
		base.Update();
		UpdateWidgetData();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mSlotData == null || mMissionData == null)
		{
			return;
		}
		if (inWidget == mMissionStartBtn)
		{
			int num = (int)mMissionData.PetCount.Min;
			if (mLocalPetIDs.Count >= num)
			{
				_PopUpStartMissionUI.SetVisibility(inVisible: true);
				return;
			}
			string empty = string.Empty;
			empty = ((num <= 1) ? _MinDragonSelectedText.GetLocalizedString() : _MinDragonSelectedPluralText.GetLocalizedString());
			empty = empty.Replace("{{COUNT}}", num.ToString());
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", empty, base.gameObject, "OnDBClose");
		}
		else if (inWidget == mMissionTrashBtn)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _TrashMissionAlertText.GetLocalizedString(), null, base.gameObject, "TrashMission", "OnDBClose", null, null, inDestroyOnClick: true);
		}
		else if (inWidget == mMissionForceCompleteBtn)
		{
			mIsForceCompletingMission = true;
			string localizedString = _ForceCompleteMissionText.GetLocalizedString();
			localizedString = localizedString.Replace("{{GEMS}}", TimedMissionManager.pInstance.GetCostForCompletion(mSlotData).ToString());
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _CompleteNowMissionAlertText.GetLocalizedString() + " " + localizedString, null, base.gameObject, "CompleteNowMission", "OnForceCompleteDBClose", null, null, inDestroyOnClick: true);
		}
		else if (inWidget == mMissionCompleteBtn)
		{
			SetInteractive(interactive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			TimedMissionManager.pInstance.CompleteMission(mSlotData, TimeMissionCompleteCallBack);
		}
		else if (inWidget == mBackButton)
		{
			SetVisibility(inVisible: false);
			_StableQuestMainUI._StableQuestSlotsUI.SetVisibility(inVisible: true);
			mDragonsSelectedMenu.ClearItems();
		}
		else if (inWidget == pBtnAds && AdManager.pInstance.AdAvailable(mAdEventType, AdType.REWARDED_VIDEO))
		{
			UiStableQuestSlotsMenu uiStableQuestSlotsMenu = (UiStableQuestSlotsMenu)_StableQuestMainUI._StableQuestSlotsUI._MenuList[0];
			if (uiStableQuestSlotsMenu.pInProgressSlotWidget != null)
			{
				uiStableQuestSlotsMenu.OnAdButtonClick(uiStableQuestSlotsMenu.pInProgressSlotWidget.FindChildItem("BtnAds"));
			}
		}
	}

	public void HandleAdButton()
	{
		if (!(pBtnAds != null) || mMissionData == null || !AdManager.pInstance.AdSupported(mAdEventType, AdType.REWARDED_VIDEO) || (StableManager.pInstance._StableQuestTutorial != null && !StableManager.pInstance._StableQuestTutorial.TutorialComplete()))
		{
			return;
		}
		if (!pCurrentSlotData.pAdWatched && AdManager.pInstance.IsReductionTimeGreater(mAdEventType, mMissionData.Duration * 60))
		{
			pBtnAds.SetVisibility(inVisible: true);
			if (pBtnAds.GetLabel() != null)
			{
				pBtnAds.GetLabel().text = AdManager.pInstance.GetReductionTimeText(mAdEventType);
			}
		}
		else
		{
			pBtnAds.SetVisibility(inVisible: false);
		}
	}

	private void BuyGemsOnline()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void CompleteNowMission()
	{
		if (Money.pCashCurrency >= TimedMissionManager.pInstance.GetCostForCompletion(mSlotData))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			TimedMissionManager.pInstance.ForceComplete(mSlotData, TimeMissionCompleteCallBack);
			SetInteractive(interactive: false);
		}
		else
		{
			mIsForceCompletingMission = false;
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _NotEnoughFeeText.GetLocalizedString(), null, base.gameObject, "BuyGemsOnline", "OnDBClose", null, null, inDestroyOnClick: true);
		}
	}

	private void OnDBClose()
	{
	}

	private void OnForceCompleteDBClose()
	{
		mIsForceCompletingMission = false;
	}

	private void TrashMission()
	{
		SetVisibility(inVisible: false);
		_StableQuestMainUI._StableQuestSlotsUI.SetVisibility(inVisible: true);
		TimedMissionManager.pInstance.TrashMission(mSlotData.SlotID);
	}

	private void TimeMissionCompleteCallBack(bool success, bool winStatus, AchievementReward[] reward)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		if (success)
		{
			_StableQuestMainUI._StableQuestResultsUI.SetVisibility(inVisible: true);
			_StableQuestMainUI._StableQuestResultsUI.SetSlotData(mSlotData, reward, winStatus);
			SetVisibility(inVisible: false);
		}
		else if (mIsForceCompletingMission)
		{
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ForceCompleteMissionFailText.GetLocalizedString(), null, "");
		}
		mIsForceCompletingMission = false;
	}

	public void SetSlotData(TimedMissionSlotData slotData)
	{
		mPrevTimer = -1;
		mLastLogReached = -1;
		mSlotData = slotData;
		mMissionData = slotData.pMission;
		mPreviousMissionState = TimedMissionState.None;
		if (mSlotData != null && mMissionData != null)
		{
			mLocalPetIDs.Clear();
			if (mSlotData.PetIDs != null && mSlotData.State == TimedMissionState.Started)
			{
				mLocalPetIDs.AddRange(mSlotData.PetIDs);
			}
			string localizedString = _EnergyRequiredText.GetLocalizedString();
			localizedString = localizedString.Replace("{{Result}}", mMissionData.DragonEnergyCost.ToString());
			if (mDragonsEnergyCost != null)
			{
				mDragonsEnergyCost.SetText(localizedString);
			}
			if (mMissionTitle != null)
			{
				mMissionTitle.SetText(mMissionData.Title.GetLocalizedString());
			}
			_StableQuestMainUI.SetSlotDifficulty(mMissionData.Difficulty, mMissionDifficultyStars);
			mMissionInfo.SetText(mMissionData.Description.GetLocalizedString());
			string[] array = mMissionData.Icon.Split('/');
			string assetPath = array[0] + "/" + array[1];
			mMissionQuestIcon.SetTextureFromBundle(assetPath, array[2]);
			mMissionQuestIcon.SetVisibility(inVisible: true);
			mMissionTipsDetails.SetText(mMissionData.Hint.GetLocalizedString());
			if (mMissionTipsIcon != null)
			{
				string[] array2 = mMissionData.HintIcon.Split('/');
				string assetPath2 = array2[0] + "/" + array2[1];
				mMissionTipsIcon.SetTextureFromBundle(assetPath2, array2[2]);
				mMissionTipsIcon.SetVisibility(inVisible: true);
			}
			mRewardWidget.SetRewards(mMissionData.WinRewards.ToArray(), MissionManager.pInstance._RewardData);
			mDragonsSelectedMenu.Init(mSlotData);
			mDragonsSelectedMenu.SetVisibility(inVisible: true);
			if (mSlotData.State == TimedMissionState.Alotted)
			{
				string timerString = _StableQuestMainUI.GetTimerString(mMissionData.Duration * 60);
				mMissionTime.SetText(timerString);
				mMissionInprogress.SetVisibility(inVisible: false);
				mMissionStartMission.SetVisibility(inVisible: true);
			}
			else if (mSlotData.State == TimedMissionState.Started)
			{
				TimeSpan completionTime = TimedMissionManager.pInstance.GetCompletionTime(mSlotData);
				string timerString2 = _StableQuestMainUI.GetTimerString(completionTime.Seconds);
				mMissionTime.SetText(timerString2);
				mMissionStartMission.SetVisibility(inVisible: false);
				mMissionInprogress.SetVisibility(inVisible: true);
			}
			mMissionCompleteBtn.SetVisibility(inVisible: false);
			mMissionForceCompleteBtn.SetVisibility(inVisible: true);
			mIsForceCompletingMission = false;
			SetGemsText();
			mLogMenu.SetVisibility(inVisible: false);
			RefreshWinProbabilityBar();
			SetVisibility(inVisible: true);
		}
	}

	public void RefreshWinProbabilityBar()
	{
		float winProbability = TimedMissionManager.pInstance.GetWinProbability(mMissionData, mLocalPetIDs);
		float progressLevel = winProbability / 100f;
		mDragonProgressBar.SetProgressLevel(progressLevel);
		mDragonProgressPercentage.SetText(winProbability + "%");
		mDragonsSelectedCount.SetText(mLocalPetIDs.Count + "/" + mSlotData.pMission.PetCount.Max);
	}

	public void SetDragonSelectionMode(bool enable)
	{
		_PopUpDragonSelectionUI.SetVisibility(enable);
		mMissionStartBtn.SetVisibility(!enable);
		mMissionTrashBtn.SetVisibility(!enable);
		mMissionDetailScreen.SetInteractive(!enable);
		mDragonsSelectedMenu.SetVisibility(!enable);
		mBackButton.SetVisibility(!enable);
	}

	private void UpdateWidgetData()
	{
		if (mSlotData == null || mMissionData == null)
		{
			return;
		}
		if (mSlotData.State == TimedMissionState.Started)
		{
			if (mPreviousMissionState != mSlotData.State)
			{
				mMissionStartMission.SetVisibility(inVisible: false);
				mMissionInprogress.SetVisibility(inVisible: true);
				mLogMenu.PopulateItems(mSlotData.SlotID);
				mLogMenu.SetVisibility(inVisible: true);
				mLogMenu.pMenuGrid.repositionNow = true;
			}
			int num = MissionLogIndex();
			if (num != mLastLogReached)
			{
				mLogMenu.ShowLogsTillIndex(num);
				mLastLogReached = num;
			}
			TimeSpan completionTime = TimedMissionManager.pInstance.GetCompletionTime(mSlotData);
			if (mPrevTimer != (int)completionTime.TotalSeconds)
			{
				mPrevTimer = (int)completionTime.TotalSeconds;
				string empty = string.Empty;
				empty = _StableQuestMainUI.GetTimerString(mPrevTimer);
				mMissionTime.SetText(empty);
				SetGemsText();
			}
			if (!mIsForceCompletingMission)
			{
				TimedMissionManager.pInstance.CheckMissionCompleted(mSlotData.SlotID);
			}
		}
		else if (mSlotData.State == TimedMissionState.Ended && mPreviousMissionState != mSlotData.State)
		{
			mMissionForceCompleteBtn.SetVisibility(inVisible: false);
			mMissionCompleteBtn.SetVisibility(inVisible: true);
			if (pBtnAds != null && pBtnAds.GetVisibility())
			{
				pBtnAds.SetVisibility(inVisible: false);
			}
		}
		mPreviousMissionState = mSlotData.State;
	}

	private int MissionLogIndex()
	{
		TimeSpan completionTime = TimedMissionManager.pInstance.GetCompletionTime(mSlotData);
		float num = mSlotData.pMission.Duration * 60;
		return Mathf.FloorToInt((float)((double)num - completionTime.TotalSeconds) * (float)mLogMenu.pLogsLength / num);
	}

	public void SetGemsText()
	{
		mMissionForceCompleteBtn.SetText(TimedMissionManager.pInstance.GetCostForCompletion(mSlotData.SlotID).ToString());
	}
}
