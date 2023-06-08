public class UiMOBALeaderboardMenu : KAUIMenu
{
	public override void LoadItem(KAWidget inWidget)
	{
		base.LoadItem(inWidget);
		inWidget.SetVisibility(inVisible: true);
		ClanData clanData = (ClanData)inWidget.GetUserData();
		clanData?.Load(forceLoad: true);
		Group group = clanData._Group;
		inWidget.FindChildItem("TeamName").SetText(group.Name);
		inWidget.FindChildItem("ClanRank").SetText(group.Rank.HasValue ? group.Rank.Value.ToString() : "--");
		inWidget.FindChildItem("TrophyCount").SetText(group.Points.HasValue ? group.Points.ToString() : "0");
	}
}
