using UnityEngine;

public class UiClansSearchMenu : KAUIMenu
{
	public override void LoadItem(KAWidget inWidget)
	{
		base.LoadItem(inWidget);
		inWidget.SetVisibility(inVisible: true);
		ClanData clanData = (ClanData)inWidget.GetUserData();
		if (clanData == null)
		{
			return;
		}
		clanData.Load(forceLoad: true);
		Group group = clanData._Group;
		inWidget.FindChildItem("TxtClanName").SetText(group.Name);
		inWidget.FindChildItem("TxtClanType").SetText(group.Type.ToString());
		int type = (int)group.Type;
		if (UiClans.pInstance._ClanTypeText != null && UiClans.pInstance._ClanTypeText.Length > type)
		{
			inWidget.FindChildItem("TxtClanType").SetText(UiClans.pInstance._ClanTypeText[type].GetLocalizedString());
		}
		if (group.TotalMemberCount.HasValue)
		{
			inWidget.FindChildItem("TxtClanMembers").SetText(group.TotalMemberCount.Value + (group.MemberLimit.HasValue ? ("/" + group.MemberLimit.Value) : ""));
		}
		else
		{
			inWidget.FindChildItem("TxtClanMembers").SetText("--");
		}
		inWidget.FindChildItem("TxtClanRank").SetText(group.Rank.HasValue ? group.Rank.Value.ToString() : "--");
		inWidget.FindChildItem("TxtClanTrophyCount").SetText(group.Points.HasValue ? group.Points.ToString() : "0");
		if (group.RankTrend.HasValue)
		{
			int value = group.RankTrend.Value;
			KAWidget kAWidget = inWidget.FindChildItem("AniClanRankDown");
			kAWidget.SetVisibility(value < 0);
			if (value < 0)
			{
				kAWidget.SetText(Mathf.Abs(value).ToString());
			}
			KAWidget kAWidget2 = inWidget.FindChildItem("AniClanRankUp");
			kAWidget2.SetVisibility(value > 0);
			if (value > 0)
			{
				kAWidget2.SetText(value.ToString());
			}
			inWidget.FindChildItem("AniClanRankEqual").SetVisibility(value == 0);
		}
		else
		{
			inWidget.FindChildItem("AniClanRankDown").SetVisibility(inVisible: false);
			inWidget.FindChildItem("AniClanRankUp").SetVisibility(inVisible: false);
			inWidget.FindChildItem("AniClanRankEqual").SetVisibility(inVisible: true);
		}
	}
}
