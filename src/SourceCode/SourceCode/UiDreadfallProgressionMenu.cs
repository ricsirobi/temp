using System;
using System.Collections.Generic;
using UnityEngine;

public class UiDreadfallProgressionMenu : KAUIMenu
{
	public KAWidget _FinalRewardIcon;

	public KAWidget _FinalRewardName;

	public KAWidget _FinalRewardValue;

	public LocaleString _PointsName = new LocaleString("Candies");

	private int mLastInfoId;

	private KAWidget mProgressRight;

	private UiDreadfall mUiDreadfall;

	public void Populate(UserAchievementTask achievementTask, List<int> redeemFailedList = null)
	{
		mUiDreadfall = GetComponentInParent<UiDreadfall>();
		mLastInfoId = 0;
		List<AchievementTaskInfo> list = new List<AchievementTaskInfo>();
		for (int i = 0; i < DreadfallAchievementManager.pInstance.AchievementTaskInfoList.AchievementTaskInfo.Length; i++)
		{
			AchievementTaskInfo achievementTaskInfo = DreadfallAchievementManager.pInstance.AchievementTaskInfoList.AchievementTaskInfo[i];
			if (DreadfallAchievementManager.pInstance.AchievementVisible(achievementTaskInfo))
			{
				list.Add(achievementTaskInfo);
			}
		}
		foreach (AchievementTaskInfo item in list)
		{
			if (list.IndexOf(item) == list.Count - 1)
			{
				ShowRewardInfo(item, _FinalRewardIcon, redeemFailedList, achievementTask, null, _FinalRewardName, _FinalRewardValue);
				continue;
			}
			KAWidget kAWidget = DuplicateWidget(_Template);
			AddWidget(kAWidget);
			KAWidget kAWidget2 = kAWidget.FindChildItem("TxtProgressionCount");
			if (kAWidget2 != null)
			{
				kAWidget2.SetText(item.PointValue + "\n" + _PointsName.GetLocalizedString());
			}
			mProgressRight = kAWidget.FindChildItem("ProgressBarRight");
			if (list.IndexOf(item) == list.Count - 2)
			{
				mLastInfoId = item.AchievementInfoID;
			}
			else
			{
				KAWidget kAWidget3 = kAWidget.FindChildItem("ProgressBarBkgRight");
				if (kAWidget3 != null)
				{
					kAWidget3.gameObject.SetActive(value: false);
				}
			}
			KAWidget icon = kAWidget.FindChildItem("BkgIcon");
			ShowRewardInfo(item, icon, redeemFailedList, achievementTask, kAWidget);
		}
		if (achievementTask != null && achievementTask.NextLevelAchievementRewards != null && achievementTask.NextLevelAchievementRewards.Length != 0)
		{
			int nextTaskID = achievementTask.NextLevelAchievementRewards[0].AchievementInfoID;
			SetDefaultFocusIndex(list.FindIndex((AchievementTaskInfo x) => x.AchievementInfoID == nextTaskID));
		}
	}

	private void ShowRewardInfo(AchievementTaskInfo taskInfo, KAWidget icon, List<int> redeemedFailedList, UserAchievementTask achievementTask, KAWidget templateWidget = null, KAWidget name = null, KAWidget number = null)
	{
		UiDreadfall.RewardIconMap rewardIconMap = Array.Find(mUiDreadfall._RewardIconMap, (UiDreadfall.RewardIconMap x) => x._AchievementInfoID == taskInfo.AchievementInfoID);
		if (rewardIconMap != null)
		{
			CoBundleItemData coBundleItemData = new CoBundleItemData(rewardIconMap._IconName, null);
			if (icon != null)
			{
				coBundleItemData._Item = icon;
				coBundleItemData.LoadResource();
			}
			if (name != null)
			{
				name.SetText(rewardIconMap._AchievementRewardName);
			}
		}
		if (number != null)
		{
			number.SetText(taskInfo.PointValue.ToString());
		}
		if (!(templateWidget != null))
		{
			return;
		}
		KAToggleButton componentInChildren = templateWidget.gameObject.GetComponentInChildren<KAToggleButton>();
		Transform transform = templateWidget.gameObject.transform.Find("Selected");
		KAWidget kAWidget = templateWidget.FindChildItem("ProgressBar");
		templateWidget.gameObject.GetComponent<KAButton>().SetToolTipText(rewardIconMap._AchievementRewardName);
		if (kAWidget != null)
		{
			kAWidget.SetProgressLevel(0f);
			kAWidget.SetText(null);
		}
		if (mProgressRight != null)
		{
			mProgressRight.SetProgressLevel(0f);
		}
		if (achievementTask.NextLevel.HasValue)
		{
			if (taskInfo.Level < achievementTask.NextLevel.Value)
			{
				if (kAWidget != null)
				{
					kAWidget.SetProgressLevel(1f);
				}
				if (redeemedFailedList == null || redeemedFailedList.Count == 0 || !redeemedFailedList.Contains(taskInfo.AchievementInfoID))
				{
					if (componentInChildren != null)
					{
						componentInChildren.SetChecked(isChecked: true);
					}
					if (transform != null)
					{
						transform.gameObject.SetActive(value: true);
					}
				}
				if (mLastInfoId == taskInfo.AchievementInfoID)
				{
					ShowProgression(mProgressRight, achievementTask, achievementTask.NextLevel.Value);
				}
			}
			else if (taskInfo.Level == achievementTask.NextLevel.Value)
			{
				if (kAWidget != null)
				{
					ShowProgression(kAWidget, achievementTask, taskInfo.Level);
				}
				SetSelectedItem(templateWidget);
			}
		}
		else
		{
			if (achievementTask.QuantityRequired.HasValue)
			{
				return;
			}
			if (kAWidget != null)
			{
				kAWidget.SetProgressLevel(1f);
			}
			if (redeemedFailedList == null || redeemedFailedList.Count == 0 || !redeemedFailedList.Contains(taskInfo.AchievementInfoID))
			{
				if (componentInChildren != null)
				{
					componentInChildren.SetChecked(isChecked: true);
				}
				if (transform != null)
				{
					transform.gameObject.SetActive(value: true);
				}
			}
			if (mLastInfoId == taskInfo.AchievementInfoID && mProgressRight != null)
			{
				mProgressRight.SetProgressLevel(1f);
			}
		}
	}

	private void ShowProgression(KAWidget progressWidget, UserAchievementTask achTask, int level)
	{
		if (achTask.NextLevel.HasValue)
		{
			progressWidget.SetProgressLevel(DreadfallAchievementManager.pInstance.GetAchievementProgress(achTask.NextLevel.Value, achTask));
		}
		else
		{
			progressWidget.SetProgressLevel(DreadfallAchievementManager.pInstance.GetAchievementProgress(level, achTask));
		}
	}
}
