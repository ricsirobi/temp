public class UiFarmLeaderboardTabMenu : KAUIMenu
{
	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (_ParentUi != null)
		{
			((UiFarmLeaderboard)_ParentUi).ChangeTab(inWidget);
		}
	}
}
