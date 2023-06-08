using System;
using System.Collections.Generic;
using UnityEngine;

namespace SOD.Event;

public class UiRedeemMenu : KAUIMenu
{
	[Header("Widgets")]
	[SerializeField]
	private KAWidget m_SingleRewardTemplate;

	public void PopulateUi(RewardIconMap[] rewardIconMap, List<AchievementTaskInfo> redeemableRewardsList)
	{
		if (redeemableRewardsList.Count > 1)
		{
			if (m_SingleRewardTemplate != null)
			{
				m_SingleRewardTemplate.SetVisibility(inVisible: false);
			}
			int i;
			for (i = 0; i < redeemableRewardsList.Count; i++)
			{
				RewardIconMap rewardIconMap2 = Array.Find(rewardIconMap, (RewardIconMap x) => x._AchievementInfoID == redeemableRewardsList[i].AchievementInfoID);
				KAWidget kAWidget = DuplicateWidget(_Template);
				AddWidget(kAWidget);
				if (rewardIconMap2 != null)
				{
					CoBundleItemData coBundleItemData = new CoBundleItemData(rewardIconMap2._IconName, null);
					KAWidget kAWidget2 = kAWidget.FindChildItem("BkgIcon");
					if (kAWidget2 != null)
					{
						coBundleItemData._Item = kAWidget2;
						coBundleItemData.LoadResource();
					}
				}
			}
			return;
		}
		RewardIconMap rewardIconMap3 = Array.Find(rewardIconMap, (RewardIconMap x) => x._AchievementInfoID == redeemableRewardsList[0].AchievementInfoID);
		if (rewardIconMap3 != null)
		{
			CoBundleItemData coBundleItemData2 = new CoBundleItemData(rewardIconMap3._IconName, null);
			if (m_SingleRewardTemplate != null)
			{
				coBundleItemData2._Item = m_SingleRewardTemplate;
				coBundleItemData2.LoadResource();
			}
		}
	}
}
