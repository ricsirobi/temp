namespace SquadTactics;

public class UiRealmMenu : KAUIMenu
{
	public override void OnClick(KAWidget item)
	{
		KAWidgetUserData userData = item.GetUserData();
		if (userData != null)
		{
			base.OnClick(item);
			SetSelectedItem(item);
			LevelManager.pInstance._LevelSelection.SelectAndShowLevelsForRealm(userData._Index);
		}
	}
}
