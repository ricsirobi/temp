using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiDaysMenu : KAUIMenu
{
	public UiDailyRewardsMenu _UiDailyRewardsMenu;

	public LocaleString _DayText = new LocaleString("Day");

	public void Populate(List<UserTimedAchievement> userTimedAchievements, int currentDay)
	{
		ClearMenu();
		int num = 0;
		KAWidget kAWidget = null;
		foreach (UserTimedAchievement userTimedAchievement in userTimedAchievements)
		{
			KAWidget kAWidget2 = DuplicateWidget(_Template);
			AddData(kAWidget2, userTimedAchievement, currentDay);
			if (userTimedAchievement.Sequence == currentDay)
			{
				kAWidget = kAWidget2;
				if (userTimedAchievement.StatusID <= 1)
				{
					MissionGroup missionGroup = MissionManager.pInstance.pDailyMissionStateResult.MissionGroup.Find((MissionGroup item) => item.MissionGroupID == UiDailyQuests.pMissionGroup);
					List<Mission> allMissions = MissionManager.pInstance.GetAllMissions(UiDailyQuests.pMissionGroup);
					allMissions = allMissions.FindAll((Mission mission) => mission.Completed > 0);
					if (allMissions != null && allMissions.Count >= missionGroup.CompletionCount)
					{
						userTimedAchievement.StatusID = 2;
					}
				}
			}
			if (userTimedAchievement.StatusID > 1)
			{
				kAWidget2.FindChildItem("MissionStatus").SetVisibility(inVisible: true);
			}
			AddWidget(kAWidget2);
			num++;
		}
		if (kAWidget != null)
		{
			UiDaysWidgetData uiDaysWidgetData = (UiDaysWidgetData)kAWidget.GetUserData();
			if (uiDaysWidgetData != null && uiDaysWidgetData._Reward[0].PointTypeID != 6)
			{
				StartCoroutine(SetCurrentDaySelected(kAWidget));
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		ProcessClick(inWidget);
	}

	public void AddData(KAWidget inWidget, UserTimedAchievement inAchievement, int currentDay)
	{
		UiDaysWidgetData uiDaysWidgetData = new UiDaysWidgetData(inAchievement.AchievementReward, inAchievement.Sequence);
		inWidget.SetUserData(uiDaysWidgetData);
		DailyQuestRewardWidget componentInChildren = inWidget.GetComponentInChildren<DailyQuestRewardWidget>();
		AchievementReward[] inRewards = new AchievementReward[1] { inAchievement.AchievementReward[0] };
		uiDaysWidgetData.ShowLoading(inShow: true);
		if (currentDay == inAchievement.Sequence && inAchievement.AchievementReward[0].PointTypeID == 6)
		{
			uiDaysWidgetData.pCallbackOnWidgetReady = OnWidgetReady;
		}
		componentInChildren.SetRewards(inRewards, MissionManager.pInstance._RewardData, null, uiDaysWidgetData.OnSetReward);
		inWidget.FindChildItem("Day").SetText(_DayText.GetLocalizedString() + " " + inAchievement.Sequence);
		inWidget.SetVisibility(inVisible: true);
	}

	public void OnWidgetReady(KAWidget widget)
	{
		StartCoroutine(SetCurrentDaySelected(widget));
	}

	public void ProcessClick(KAWidget inWidget)
	{
		UiDaysWidgetData uiDaysWidgetData = (UiDaysWidgetData)inWidget.GetUserData();
		if (uiDaysWidgetData == null)
		{
			return;
		}
		string rewardName = "";
		List<AchievementReward> list = new List<AchievementReward>();
		if (uiDaysWidgetData._Reward[0].PointTypeID == 6)
		{
			DailyQuestRewardWidget componentInChildren = inWidget.GetComponentInChildren<DailyQuestRewardWidget>();
			if (componentInChildren != null && componentInChildren.pItemData != null)
			{
				rewardName = componentInChildren.pItemData.ItemName;
				if (componentInChildren.pItemData.IsBundleItem())
				{
					List<int> bundledItems = componentInChildren.pItemData.GetBundledItems();
					if (bundledItems != null && bundledItems.Count > 0)
					{
						foreach (int item in bundledItems)
						{
							AchievementReward achievementReward = new AchievementReward();
							achievementReward.ItemID = item;
							achievementReward.PointTypeID = 6;
							achievementReward.Amount = 1;
							list.Add(achievementReward);
						}
					}
				}
				else
				{
					list.Add(uiDaysWidgetData._Reward[0]);
				}
			}
		}
		else
		{
			list.Add(uiDaysWidgetData._Reward[0]);
		}
		MissionGroup missionGroup = MissionManager.pInstance.pDailyMissionStateResult.MissionGroup.Find((MissionGroup item) => item.MissionGroupID == UiDailyQuests.pMissionGroup);
		UiDailyQuests uiDailyQuests = (UiDailyQuests)_ParentUi;
		if (uiDailyQuests != null)
		{
			uiDailyQuests.OnDayItemClicked(uiDaysWidgetData, rewardName, missionGroup.Day == uiDaysWidgetData._Day);
		}
		_UiDailyRewardsMenu.Populate(list);
	}

	private IEnumerator SetCurrentDaySelected(KAWidget inWidget)
	{
		yield return new WaitForEndOfFrame();
		ProcessClick(inWidget);
		SetSelectedItem(inWidget);
		ScrollToSelectedItem();
	}

	private void ScrollToSelectedItem()
	{
		if (mSelectedItem != null && GetNumItems() > 0)
		{
			float num = GetSelectedItemIndex();
			if (num > 0f)
			{
				num += 1f;
			}
			if (base.pHortizontalScrollbar != null)
			{
				base.pHortizontalScrollbar.value = num / (float)GetNumItems();
			}
		}
	}

	protected override void UpdateVisibility(bool inVisible)
	{
		base.UpdateVisibility(inVisible);
		if (mSelectedItem != null && inVisible)
		{
			mSelectedItem.OnSelected(inSelected: true);
		}
	}
}
