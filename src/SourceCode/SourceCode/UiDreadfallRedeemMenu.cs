using System;
using System.Collections.Generic;

public class UiDreadfallRedeemMenu : KAUIMenu
{
	public KAWidget singleRewardTemplate;

	public void PopulateUi(UiDreadfall.RewardIconMap[] rewardIconMap, List<AchievementTaskInfo> redeemableRewardsList)
	{
		if (redeemableRewardsList.Count > 1)
		{
			if (singleRewardTemplate != null)
			{
				singleRewardTemplate.SetVisibility(inVisible: false);
			}
			int i;
			for (i = 0; i < redeemableRewardsList.Count; i++)
			{
				UiDreadfall.RewardIconMap rewardIconMap2 = Array.Find(rewardIconMap, (UiDreadfall.RewardIconMap x) => x._AchievementInfoID == redeemableRewardsList[i].AchievementInfoID);
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
		UiDreadfall.RewardIconMap rewardIconMap3 = Array.Find(rewardIconMap, (UiDreadfall.RewardIconMap x) => x._AchievementInfoID == redeemableRewardsList[0].AchievementInfoID);
		if (rewardIconMap3 != null)
		{
			CoBundleItemData coBundleItemData2 = new CoBundleItemData(rewardIconMap3._IconName, null);
			if (singleRewardTemplate != null)
			{
				coBundleItemData2._Item = singleRewardTemplate;
				coBundleItemData2.LoadResource();
			}
		}
	}
}
