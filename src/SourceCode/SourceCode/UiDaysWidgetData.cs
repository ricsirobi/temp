using System;
using System.Collections.Generic;

public class UiDaysWidgetData : UiRewardsWidgetData
{
	public List<AchievementReward> _Reward;

	public int _Day;

	public Action<KAWidget> pCallbackOnWidgetReady;

	public UiDaysWidgetData(List<AchievementReward> reward, int day)
	{
		_Reward = reward;
		_Day = day;
	}

	public override void OnSetReward(RewardWidget.SetRewardStatus inSetRewardStatus)
	{
		if (inSetRewardStatus == RewardWidget.SetRewardStatus.COMPLETE)
		{
			base.OnSetReward(inSetRewardStatus);
			if (pCallbackOnWidgetReady != null)
			{
				pCallbackOnWidgetReady(GetItem());
			}
		}
	}
}
