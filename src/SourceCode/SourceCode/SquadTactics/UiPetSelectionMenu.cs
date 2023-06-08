namespace SquadTactics;

public class UiPetSelectionMenu : KAUIMenu
{
	public override void SetSelectedItem(KAWidget inWidget)
	{
		base.SetSelectedItem(inWidget);
		if (inWidget != null)
		{
			LevelManager.pInstance._TeamSelection.RefreshButtons(((SquadData)inWidget.GetUserData()).unitData._RaisedPetID);
		}
	}

	public override void OnHover(KAWidget inWidget, bool inIsHover)
	{
		if (!UtPlatform.IsMobile())
		{
			base.OnHover(inWidget, inIsHover);
		}
	}
}
