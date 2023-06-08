public class UiUDTLeaderboardTabMenu : KAUIMenu
{
	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (_ParentUi != null)
		{
			((UiUDTLeaderboard)_ParentUi).ChangeTab(inWidget);
		}
	}
}
