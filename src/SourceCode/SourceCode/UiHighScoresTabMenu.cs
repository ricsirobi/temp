public class UiHighScoresTabMenu : KAUIMenu
{
	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (_ParentUi != null)
		{
			((UiHighScoresBox)_ParentUi).ChangeTab(inWidget, setGameData: false);
		}
	}
}
