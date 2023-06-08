namespace SquadTactics;

public class UiLevelSelectionMenu : KAUIMenu
{
	public override void OnHover(KAWidget inWidget, bool inIsHover)
	{
		LevelUserData levelUserData = (LevelUserData)inWidget.GetUserData();
		if (levelUserData != null)
		{
			if (!levelUserData._Locked)
			{
				base.OnHover(inWidget, inIsHover);
			}
			if (inIsHover)
			{
				LevelManager.pInstance._LevelSelection.UpdateSelectedLevelDescription(levelUserData._Index, levelUserData._Locked);
			}
			else
			{
				LevelManager.pInstance._LevelSelection.ResetSelectedLevelDescription();
			}
		}
	}

	public override void OnClick(KAWidget item)
	{
		LevelUserData levelUserData = (LevelUserData)item.GetUserData();
		if (levelUserData != null && !levelUserData._Locked)
		{
			base.OnClick(item);
			SetSelectedItem(item);
			LevelManager.pInstance._LevelSelection.SelectLevel(levelUserData._Index);
		}
	}

	public override void OnPress(KAWidget inWidget, bool inPressed)
	{
		LevelUserData levelUserData = (LevelUserData)inWidget.GetUserData();
		if (levelUserData != null)
		{
			if (!levelUserData._Locked)
			{
				base.OnPress(inWidget, inPressed);
			}
			if (inPressed)
			{
				LevelManager.pInstance._LevelSelection.UpdateSelectedLevelDescription(levelUserData._Index, levelUserData._Locked);
			}
			else
			{
				LevelManager.pInstance._LevelSelection.ResetSelectedLevelDescription();
			}
		}
	}

	public override void OnSelect(KAWidget inWidget, bool inSelected)
	{
		LevelUserData levelUserData = (LevelUserData)inWidget.GetUserData();
		if (levelUserData != null && !levelUserData._Locked)
		{
			base.OnSelect(inWidget, inSelected);
		}
	}
}
