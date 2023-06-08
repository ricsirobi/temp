public class UiUDTLeaderboardFilterMenu : KAUIMenu
{
	public KAWidget _TemplateItemWithTimer;

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (_ParentUi != null)
		{
			((UiUDTLeaderboard)_ParentUi).ChangeFilterTab(inWidget);
		}
	}
}
