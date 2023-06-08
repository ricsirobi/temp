using System;
using System.Collections.Generic;
using UnityEngine;

namespace SOD.Event;

public class UiPrizeProgressionMenu : KAUIMenu
{
	[SerializeField]
	private LocaleString m_PointsText = new LocaleString("Candies");

	[Header("Widgets")]
	[SerializeField]
	private KAWidget m_FinalRewardIcon;

	[SerializeField]
	private KAWidget m_FinalRewardName;

	[SerializeField]
	private KAWidget m_FinalRewardValue;

	private int mLastInfoId;

	private KAWidget mProgressRight;

	private UiPrizeProgression mUiPrizeProgression;

	public void Populate(UserAchievementTask achievementTask, List<int> redeemFailedList = null)
	{
		mUiPrizeProgression = GetComponentInParent<UiPrizeProgression>();
		mLastInfoId = 0;
		List<AchievementTaskInfo> list = new List<AchievementTaskInfo>();
		EventManager eventManager = EventManager.Get(mUiPrizeProgression._EventName);
		for (int i = 0; i < eventManager.AchievementTaskInfoList.AchievementTaskInfo.Length; i++)
		{
			AchievementTaskInfo achievementTaskInfo = eventManager.AchievementTaskInfoList.AchievementTaskInfo[i];
			if (eventManager.AchievementVisible(achievementTaskInfo))
			{
				list.Add(achievementTaskInfo);
			}
		}
		foreach (AchievementTaskInfo item in list)
		{
			if (list.IndexOf(item) == list.Count - 1)
			{
				ShowRewardInfo(item, m_FinalRewardIcon, redeemFailedList, achievementTask, null, m_FinalRewardName, m_FinalRewardValue);
				continue;
			}
			KAWidget kAWidget = DuplicateWidget(_Template);
			AddWidget(kAWidget);
			KAWidget kAWidget2 = kAWidget.FindChildItem("TxtProgressionCount");
			if (kAWidget2 != null)
			{
				kAWidget2.SetText(item.PointValue + "\n" + m_PointsText.GetLocalizedString());
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
		RewardIconMap rewardIconMap = Array.Find(mUiPrizeProgression._RewardIconMap, (RewardIconMap x) => x._AchievementInfoID == taskInfo.AchievementInfoID);
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
		EventManager eventManager = EventManager.Get(mUiPrizeProgression._EventName);
		if (achTask.NextLevel.HasValue)
		{
			progressWidget.SetProgressLevel(eventManager.GetAchievementProgress(achTask.NextLevel.Value, achTask));
		}
		else
		{
			progressWidget.SetProgressLevel(eventManager.GetAchievementProgress(level, achTask));
		}
	}
}
