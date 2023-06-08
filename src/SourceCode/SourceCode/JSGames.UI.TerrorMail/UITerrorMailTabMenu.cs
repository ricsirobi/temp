using System.Collections.Generic;

namespace JSGames.UI.TerrorMail;

public class UITerrorMailTabMenu : UIMenu
{
	public UIToggleButton _GiftTabBtn;

	public UIToggleButton _AchievementTabBtn;

	public List<TabType> _TabBtnTypes = new List<TabType>();
}
