public class UiJournalAchievementsIconsMenu : KAUIMenu
{
	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		((UiJournalAchievements)_ParentUi).ProcessCategoryIconClicked(inWidget);
	}
}
