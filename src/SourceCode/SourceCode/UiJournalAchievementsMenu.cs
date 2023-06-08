public class UiJournalAchievementsMenu : KAUIMenu
{
	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		((UiAchievements)_ParentUi).ProcessMenuWidgetClicked(inWidget);
	}
}
