using System.Collections.Generic;

public class UiStableQuestResult : KAUI
{
	public UiStableQuestLog _LogPopUpUI;

	public UiStableQuestMain _StableQuestMainUI;

	private KAWidget mMissionTitle;

	private KAWidget mMissionDifficultyStars;

	private KAWidget mVictoryWidget;

	private KAWidget mDefeatWidget;

	private KAWidget mResultDetail;

	public RewardWidget mRewardWidget;

	private KAWidget mBtnQuestLog;

	private KAWidget mBtnComplete;

	private TimedMissionSlotData mSlotData;

	private TimedMission mMissionData;

	private UiStableQuestResultMenu mDragonsMenu;

	private List<AchievementReward> mRewardList;

	public List<AchievementReward> pRewardList => mRewardList;

	protected override void Start()
	{
		base.Start();
		mMissionTitle = FindItem("TxtMissionTitle");
		mMissionDifficultyStars = FindItem("Difficulty");
		mVictoryWidget = FindItem("BkgVictory");
		mDefeatWidget = FindItem("BkgDefeat");
		mResultDetail = FindItem("ResultScreen");
		mRewardWidget = (RewardWidget)mResultDetail.FindChildItem("XPReward");
		mBtnQuestLog = FindItem("BtnViewLog");
		mBtnComplete = FindItem("BtnClaim");
		mDragonsMenu = (UiStableQuestResultMenu)_MenuList[0];
	}

	public void SetSlotData(TimedMissionSlotData slotData, AchievementReward[] reward, bool won)
	{
		mSlotData = slotData;
		mMissionData = mSlotData.pMission;
		if (mSlotData == null || mMissionData == null)
		{
			return;
		}
		mMissionTitle.SetText(mMissionData.Title.GetLocalizedString());
		_StableQuestMainUI.SetSlotDifficulty(mMissionData.Difficulty, mMissionDifficultyStars);
		_LogPopUpUI.Init(mSlotData.SlotID);
		if (mRewardList == null)
		{
			mRewardList = new List<AchievementReward>();
		}
		mRewardList.Clear();
		if (reward != null && reward.Length != 0)
		{
			mRewardList.AddRange(reward);
		}
		List<AchievementReward> list = new List<AchievementReward>();
		foreach (AchievementReward mReward in mRewardList)
		{
			if (mReward.PointTypeID.Value != 8)
			{
				list.Add(mReward);
			}
		}
		if (list.Count > 0)
		{
			mRewardWidget.SetVisibility(inVisible: true);
			mRewardWidget.SetRewards(list.ToArray(), MissionManager.pInstance._RewardData);
		}
		else
		{
			mRewardWidget.SetVisibility(inVisible: false);
		}
		mDragonsMenu.PopulateItems(mSlotData);
		mVictoryWidget.SetVisibility(won);
		mDefeatWidget.SetVisibility(!won);
		SetVisibility(inVisible: true);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mSlotData == null || mMissionData == null)
		{
			return;
		}
		if (inWidget == mBtnComplete)
		{
			SetVisibility(inVisible: false);
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.CheckForTaskCompletion("Action", "StableQuest");
			}
			_StableQuestMainUI._StableQuestSlotsUI.ResetUISlot(mSlotData.SlotID);
			_StableQuestMainUI._StableQuestSlotsUI.SetVisibility(inVisible: true);
		}
		else if (inWidget == mBtnQuestLog)
		{
			_LogPopUpUI.SetVisibility(inVisible: true);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (_LogPopUpUI.GetVisibility())
		{
			if (GetState() == KAUIState.INTERACTIVE)
			{
				SetState(KAUIState.DISABLED);
			}
			if (mBtnQuestLog.GetVisibility())
			{
				mBtnQuestLog.SetVisibility(inVisible: false);
			}
		}
		else
		{
			if (GetState() == KAUIState.DISABLED)
			{
				SetState(KAUIState.INTERACTIVE);
			}
			if (!mBtnQuestLog.GetVisibility())
			{
				mBtnQuestLog.SetVisibility(inVisible: true);
			}
		}
	}
}
