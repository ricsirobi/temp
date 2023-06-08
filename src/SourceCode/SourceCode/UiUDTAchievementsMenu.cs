public class UiUDTAchievementsMenu : KAUIMenu
{
	private static KAWidget mClickedWidget;

	public UiUDTAchievements _UiUDTAchievements;

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnClaimReward")
		{
			mClickedWidget = inWidget;
			_UiUDTAchievements.RedeemReward(mClickedWidget.GetParentItem());
		}
	}
}
