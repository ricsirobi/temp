public class UiJournalAchievementsCatMenu : KAUIMenu
{
	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		((UiAchievements)_ParentUi).ProcessCategorySelected(inWidget);
	}
}
