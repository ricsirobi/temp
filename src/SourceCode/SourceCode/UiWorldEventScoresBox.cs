using System;
using System.Collections.Generic;
using UnityEngine;

public class UiWorldEventScoresBox : KAUI
{
	[Serializable]
	public enum ScoreTab
	{
		Results,
		ClanLeaderboard
	}

	[Serializable]
	public class ScoresBoxTab
	{
		public LocaleString _Name;

		public KAUIMenu _Menu;

		public KAWidget _HeaderWidget;

		[HideInInspector]
		public KAWidget _Widget;

		public ScoreTab _Type;
	}

	private class PlayerScoreInfo
	{
		public int _Rank;

		public string _Name;

		public int _Score;

		public string _RewardTier;

		public PlayerScoreInfo(int inRank, string inName, int inScore, string inRewardTier)
		{
			_Rank = inRank;
			_Name = inName;
			_Score = inScore;
			_RewardTier = inRewardTier;
		}
	}

	public ScoresBoxTab[] _ScoresBoxTabs;

	public string _DefaultTab;

	public KAUIMenu _PlayerScoresMenu;

	public KAUIMenu _ClanLeaderboardMenu;

	public KAUIMenu _TabMenu;

	public OnScoreTabChange onScoreTabChange;

	private ScoresBoxTab mCurrentSelectedTab;

	private List<PlayerScoreInfo> mPlayersScore = new List<PlayerScoreInfo>();

	private List<Group> mTopGroups = new List<Group>();

	private bool mIsLoadingClansPoints;

	private bool mIsClansPointsReady;

	private KAWidget mWaitingForScoresTxt;

	protected override void Start()
	{
		base.Start();
		mWaitingForScoresTxt = FindItem("TxtGettingHighscores");
		ScoresBoxTab[] scoresBoxTabs = _ScoresBoxTabs;
		foreach (ScoresBoxTab scoresBoxTab in scoresBoxTabs)
		{
			KAWidget kAWidget = _TabMenu.AddWidget(scoresBoxTab._Name._Text, null);
			kAWidget.SetTextByID(scoresBoxTab._Name._ID, scoresBoxTab._Name._Text);
			kAWidget.SetVisibility(inVisible: true);
			scoresBoxTab._Widget = kAWidget;
		}
		SelectDefaultTab();
	}

	private void SelectDefaultTab()
	{
		KAToggleButton kAToggleButton = _TabMenu.FindItem(_DefaultTab) as KAToggleButton;
		OnClick(kAToggleButton);
		kAToggleButton.SetChecked(isChecked: true);
		_TabMenu.SetSelectedItem(kAToggleButton);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		ScoresBoxTab[] scoresBoxTabs = _ScoresBoxTabs;
		foreach (ScoresBoxTab scoresBoxTab in scoresBoxTabs)
		{
			if (!(scoresBoxTab._Widget == inWidget))
			{
				continue;
			}
			if (mCurrentSelectedTab != null)
			{
				if (mCurrentSelectedTab._HeaderWidget != null)
				{
					mCurrentSelectedTab._HeaderWidget.SetVisibility(inVisible: false);
				}
				if (mCurrentSelectedTab._Menu != null)
				{
					mCurrentSelectedTab._Menu.SetVisibility(inVisible: false);
				}
				if (mCurrentSelectedTab._Widget != null)
				{
					mCurrentSelectedTab._Widget.SetDisabled(isDisabled: false);
				}
			}
			if (scoresBoxTab._Menu != null)
			{
				scoresBoxTab._Menu.SetVisibility(inVisible: true);
			}
			if (scoresBoxTab._HeaderWidget != null)
			{
				scoresBoxTab._HeaderWidget.SetVisibility(inVisible: true);
			}
			inWidget.SetDisabled(isDisabled: true);
			mCurrentSelectedTab = scoresBoxTab;
			if (scoresBoxTab._Type == ScoreTab.ClanLeaderboard)
			{
				if (!mIsClansPointsReady)
				{
					if (!mIsLoadingClansPoints)
					{
						LoadClanLeaderboard();
					}
					mWaitingForScoresTxt.SetVisibility(inVisible: true);
				}
			}
			else
			{
				mWaitingForScoresTxt.SetVisibility(inVisible: false);
			}
			if (onScoreTabChange != null)
			{
				onScoreTabChange(scoresBoxTab._Type);
			}
			break;
		}
	}

	private KAUIMenu GetMenu(ScoreTab inTabType)
	{
		ScoresBoxTab[] scoresBoxTabs = _ScoresBoxTabs;
		foreach (ScoresBoxTab scoresBoxTab in scoresBoxTabs)
		{
			if (scoresBoxTab._Type == inTabType)
			{
				return scoresBoxTab._Menu;
			}
		}
		return null;
	}

	public void PopulateClanLeaderBoard(List<Group> groups)
	{
		KAUIMenu menu = GetMenu(ScoreTab.ClanLeaderboard);
		if (menu == null)
		{
			return;
		}
		int num = 1;
		foreach (Group group in groups)
		{
			KAWidget kAWidget = menu.AddWidget(group.Name, null);
			kAWidget.FindChildItem("TxtClanRank").SetText(num.ToString());
			kAWidget.FindChildItem("TxtClanName").SetText(group.Name);
			kAWidget.FindChildItem("TxtClanPoints").SetText(group.Points.ToString());
			kAWidget.SetVisibility(inVisible: true);
			num++;
		}
	}

	public void AddPlayerScore(int inRank, string inPlayerName, int inScore, string inRewardTier)
	{
		PlayerScoreInfo item = new PlayerScoreInfo(inRank, inPlayerName, inScore, inRewardTier);
		mPlayersScore.Add(item);
	}

	private void PopulatePlayerScore(List<PlayerScoreInfo> inPlayersScore)
	{
		KAUIMenu menu = GetMenu(ScoreTab.Results);
		if (menu == null)
		{
			return;
		}
		foreach (PlayerScoreInfo item in inPlayersScore)
		{
			KAWidget kAWidget = menu.AddWidget(item._Name, null);
			kAWidget.FindChildItem("TxtPlayerRank").SetText(item._Rank.ToString());
			kAWidget.FindChildItem("TxtPlayerName").SetText(item._Name);
			kAWidget.FindChildItem("TxtPlayerScore").SetText(item._Score.ToString());
			kAWidget.FindChildItem("TxtPlayerReward").SetText(item._RewardTier);
			kAWidget.SetVisibility(inVisible: true);
		}
	}

	private void ClearMenus()
	{
		ScoresBoxTab[] scoresBoxTabs = _ScoresBoxTabs;
		for (int i = 0; i < scoresBoxTabs.Length; i++)
		{
			scoresBoxTabs[i]._Menu.ClearItems();
		}
	}

	public void ClearScores()
	{
		mPlayersScore.Clear();
		mTopGroups.Clear();
		ClearMenus();
	}

	private void GetGroupsEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE || inEvent == WsServiceEvent.ERROR)
		{
			GetGroupsResult getGroupsResult = (GetGroupsResult)inObject;
			if (getGroupsResult != null && getGroupsResult.Groups != null)
			{
				mTopGroups = new List<Group>(getGroupsResult.Groups);
				mWaitingForScoresTxt.SetVisibility(inVisible: false);
				PopulateClanLeaderBoard(mTopGroups);
				mIsClansPointsReady = true;
				mIsLoadingClansPoints = false;
			}
		}
	}

	public void LoadScores()
	{
		PopulatePlayerScore(mPlayersScore);
		mIsLoadingClansPoints = false;
		mIsClansPointsReady = false;
		SelectDefaultTab();
	}

	private void LoadClanLeaderboard()
	{
		GetGroupsRequest getGroupsRequest = new GetGroupsRequest();
		GroupsFilter groupsFilter = new GroupsFilter
		{
			PointTypeID = 21
		};
		DateTime pCurrentTime = ServerTime.pCurrentTime;
		DateTime value = new DateTime(pCurrentTime.Year, pCurrentTime.Month, 1);
		DateTime value2 = value.AddMonths(1).AddDays(-1.0);
		groupsFilter.FromDate = value;
		groupsFilter.ToDate = value2;
		groupsFilter.Refresh = true;
		getGroupsRequest.GroupsFilter = groupsFilter;
		WsWebService.GetGroups(getGroupsRequest, GetGroupsEventHandler, null);
		mIsLoadingClansPoints = true;
	}
}
