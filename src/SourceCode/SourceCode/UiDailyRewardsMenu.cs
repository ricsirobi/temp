using System.Collections.Generic;

public class UiDailyRewardsMenu : KAUIMenu
{
	public void Populate(List<AchievementReward> rewards)
	{
		ClearMenu();
		foreach (AchievementReward reward in rewards)
		{
			KAWidget kAWidget = DuplicateWidget(_Template);
			UiRewardsWidgetData uiRewardsWidgetData = new UiRewardsWidgetData();
			kAWidget.SetUserData(uiRewardsWidgetData);
			RewardWidget componentInChildren = kAWidget.GetComponentInChildren<RewardWidget>();
			AchievementReward[] inRewards = new AchievementReward[1] { reward };
			uiRewardsWidgetData.ShowLoading(inShow: true);
			componentInChildren.SetRewards(inRewards, MissionManager.pInstance._RewardData, null, uiRewardsWidgetData.OnSetReward);
			kAWidget.SetVisibility(inVisible: true);
			AddWidget(kAWidget);
		}
	}
}
