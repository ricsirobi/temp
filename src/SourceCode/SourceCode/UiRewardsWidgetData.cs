public class UiRewardsWidgetData : KAWidgetUserData
{
	public void ShowLoading(bool inShow)
	{
		KAWidget kAWidget = GetItem().FindChildItem("Loading");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inShow);
			GetItem().SetInteractive(!inShow);
		}
	}

	public virtual void OnSetReward(RewardWidget.SetRewardStatus inSetRewardStatus)
	{
		if (inSetRewardStatus == RewardWidget.SetRewardStatus.COMPLETE)
		{
			ShowLoading(inShow: false);
		}
	}
}
