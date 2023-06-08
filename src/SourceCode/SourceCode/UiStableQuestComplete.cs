using System.Collections.Generic;
using UnityEngine;

public class UiStableQuestComplete : KAUI
{
	private KAWidget mMissionTitle;

	private KAWidget mMissionDifficultyStars;

	private KAWidget mVictoryWidget;

	private KAWidget mDefeatWidget;

	private KAWidget mResultDetail;

	private RewardWidget mRewardWidget;

	private KAWidget mBtnComplete;

	private UiStableQuestCompleteMenu mQuestCompleteMenu;

	private TimedMissionSlotData mSlotData;

	private TimedMission mMissionData;

	protected override void Start()
	{
		base.Start();
		KAUI.SetExclusive(this);
		mMissionTitle = FindItem("TxtMissionTitle");
		mMissionDifficultyStars = FindItem("Difficulty");
		mVictoryWidget = FindItem("BkgVictory");
		mDefeatWidget = FindItem("BkgDefeat");
		mResultDetail = FindItem("ResultScreen");
		mRewardWidget = (RewardWidget)mResultDetail.FindChildItem("XPReward");
		mBtnComplete = FindItem("BtnClaim");
		mQuestCompleteMenu = (UiStableQuestCompleteMenu)_MenuList[0];
		mSlotData = FreeStableQuestDragon.pInstance.pCachedTimedMissionSlotData;
		SetSlotData();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mSlotData != null && mMissionData != null && inWidget == mBtnComplete)
		{
			KAUI.RemoveExclusive(this);
			Object.DestroyObject(base.gameObject);
			TimedMissionManager.pInstance.ResetSlot(mSlotData);
			FreeStableQuestDragon.pInstance.UpdateAndClose();
		}
	}

	public void SetSlotData()
	{
		mMissionData = mSlotData.pMission;
		if (mSlotData == null || mMissionData == null)
		{
			return;
		}
		mMissionTitle.SetText(mMissionData.Title.GetLocalizedString());
		SetSlotDifficulty(mMissionData.Difficulty, mMissionDifficultyStars);
		List<AchievementReward> list = new List<AchievementReward>();
		foreach (AchievementReward pReward in FreeStableQuestDragon.pInstance.pRewards)
		{
			if (pReward.PointTypeID.Value != 8)
			{
				list.Add(pReward);
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
		mQuestCompleteMenu.PopulateItems(mSlotData);
		mVictoryWidget.SetVisibility(FreeStableQuestDragon.pInstance.pWinLostStatus);
		mDefeatWidget.SetVisibility(!FreeStableQuestDragon.pInstance.pWinLostStatus);
		SetVisibility(inVisible: true);
	}

	public void SetSlotDifficulty(int DifficultyValue, KAWidget Widget)
	{
		KAWidget[] componentsInChildren = Widget.GetComponentsInChildren<KAWidget>();
		foreach (KAWidget kAWidget in componentsInChildren)
		{
			string text = kAWidget.name;
			if (text.Contains("Star"))
			{
				int num = int.Parse(text.Split('_')[1]);
				kAWidget.SetVisibility(num <= DifficultyValue);
			}
		}
	}
}
