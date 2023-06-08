using System;
using UnityEngine;

public class UiUDTLeaderboard : KAUI, IJournal
{
	public UDTLeaderboardFilterData[] _LeaderboardFilterTabs;

	public UDTLeaderboardTabData[] _LeaderboardTabs;

	public UiUDTAchievements _UDTAchievementsScreen;

	public RequestType _LeaderboardDefaultTab = RequestType.All;

	public LocaleString _NoClanMessageText = new LocaleString("You are not a clan member.");

	private bool mInitialized;

	private UiUDTLeaderboardTabMenu mUiTabMenu;

	private UiUDTLeaderboardFilterMenu mUiFilterMenu;

	private UiUDTLeaderboardMenu mLeaderboardMenu;

	private bool mIsBusy;

	private KAWidget mTxtGettingHighScores;

	private UDTLeaderboardFilterData mCurrentFilterTab;

	private UDTLeaderboardTabData mCurrentTab;

	private int mPlayerScore;

	private string mHighScoreKey;

	private bool mAscendingOrder;

	private KAWidget mPreviousTabWidget;

	private KAWidget mPreviousFilterTabWidget;

	private KAUIPages mUiPages;

	public bool pIsBusy
	{
		get
		{
			return mIsBusy;
		}
		set
		{
			mIsBusy = value;
		}
	}

	public KAWidget pTxtGettingHighScores => mTxtGettingHighScores;

	public UDTLeaderboardFilterData pCurrentFilterTab => mCurrentFilterTab;

	public UDTLeaderboardTabData pCurrentTab => mCurrentTab;

	public int pPlayerScore => mPlayerScore;

	public string pHighScoreKey => mHighScoreKey;

	public bool pAscendingOrder => mAscendingOrder;

	protected override void Start()
	{
		base.Start();
		mUiTabMenu = (UiUDTLeaderboardTabMenu)GetMenu("UiUDTLeaderboardTabMenu");
		mUiFilterMenu = (UiUDTLeaderboardFilterMenu)GetMenu("UiUDTLeaderboardFilterMenu");
		mLeaderboardMenu = (UiUDTLeaderboardMenu)GetMenu("UiUDTLeaderboardMenu");
		mTxtGettingHighScores = FindItem("TxtGettingHighscores");
		mUiPages = base.gameObject.GetComponent<KAUIPages>();
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible && !mInitialized)
		{
			Init();
		}
		if (mUiTabMenu != null)
		{
			mUiTabMenu.SetVisibility(inVisible);
		}
		if (_UDTAchievementsScreen != null)
		{
			_UDTAchievementsScreen.SetVisibility(inVisible);
		}
	}

	public void Clear()
	{
	}

	public void ProcessClose()
	{
	}

	public bool IsBusy()
	{
		return pIsBusy;
	}

	public void Exit()
	{
	}

	public void ActivateUI(int uiIndex, bool addToList = true)
	{
		Init();
	}

	public override void SetInteractive(bool inInteractive)
	{
		base.SetInteractive(inInteractive);
		if (mUiTabMenu != null)
		{
			mUiTabMenu.SetInteractive(inInteractive);
		}
		if (mLeaderboardMenu != null)
		{
			mLeaderboardMenu.SetInteractive(inInteractive);
		}
	}

	public void Init()
	{
		mInitialized = true;
		SetupTabMenu();
		SetupFilterTabMenu();
		if (_LeaderboardTabs != null && _LeaderboardTabs.Length != 0)
		{
			int num = 0;
			for (int i = 0; i < _LeaderboardTabs.Length; i++)
			{
				if (_LeaderboardTabs[i]._Type == _LeaderboardDefaultTab)
				{
					num = i;
				}
			}
			KAWidget widget = mUiTabMenu.FindItem(_LeaderboardTabs[num]._NameText._Text);
			ChangeTab(widget, updateGameData: false);
		}
		if (_LeaderboardFilterTabs != null && _LeaderboardFilterTabs.Length != 0)
		{
			KAWidget widget2 = mUiFilterMenu.FindItem(_LeaderboardFilterTabs[0]._NameText._Text);
			ChangeFilterTab(widget2);
		}
	}

	public void SetupFilterTabMenu()
	{
		if (_LeaderboardFilterTabs == null || _LeaderboardFilterTabs.Length == 0)
		{
			return;
		}
		if (mUiFilterMenu != null)
		{
			mUiFilterMenu.ClearItems();
		}
		mUiFilterMenu.SetVisibility(inVisible: true);
		for (int i = 0; i < _LeaderboardFilterTabs.Length; i++)
		{
			UDTLeaderboardFilterData uDTLeaderboardFilterData = _LeaderboardFilterTabs[i];
			KAWidget kAWidget = ((!uDTLeaderboardFilterData._ShowProgressBar) ? mUiFilterMenu.DuplicateWidget(mUiFilterMenu._Template) : mUiFilterMenu.DuplicateWidget(mUiFilterMenu._TemplateItemWithTimer));
			if (kAWidget != null)
			{
				kAWidget.name = uDTLeaderboardFilterData._NameText._Text;
				if (uDTLeaderboardFilterData._NameText != null && !string.IsNullOrEmpty(uDTLeaderboardFilterData._NameText._Text))
				{
					kAWidget.SetText(uDTLeaderboardFilterData._NameText.GetLocalizedString());
				}
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.SetState(uDTLeaderboardFilterData._DefaultState);
				mUiFilterMenu.AddWidget(kAWidget);
			}
		}
	}

	public void SetupTabMenu()
	{
		if (_LeaderboardTabs == null || _LeaderboardTabs.Length == 0)
		{
			return;
		}
		if (mUiTabMenu != null)
		{
			mUiTabMenu.ClearItems();
		}
		mUiTabMenu.SetVisibility(inVisible: true);
		for (int i = 0; i < _LeaderboardTabs.Length; i++)
		{
			UDTLeaderboardTabData uDTLeaderboardTabData = _LeaderboardTabs[i];
			KAWidget kAWidget = mUiTabMenu.DuplicateWidget(mUiTabMenu._Template);
			if (kAWidget != null)
			{
				kAWidget.name = uDTLeaderboardTabData._NameText._Text;
				if (uDTLeaderboardTabData._NameText != null && !string.IsNullOrEmpty(uDTLeaderboardTabData._NameText._Text))
				{
					kAWidget.SetText(uDTLeaderboardTabData._NameText.GetLocalizedString());
				}
				kAWidget.SetVisibility(inVisible: true);
				mUiTabMenu.AddWidget(kAWidget);
			}
		}
	}

	public void ChangeFilterTab(KAWidget widget, bool updateGameData = true)
	{
		if (mUiFilterMenu != null)
		{
			UDTLeaderboardFilterData[] leaderboardFilterTabs = _LeaderboardFilterTabs;
			foreach (UDTLeaderboardFilterData uDTLeaderboardFilterData in leaderboardFilterTabs)
			{
				if (!(uDTLeaderboardFilterData._NameText._Text != widget.name))
				{
					mCurrentFilterTab = uDTLeaderboardFilterData;
					break;
				}
			}
		}
		if (mPreviousFilterTabWidget != null)
		{
			mPreviousFilterTabWidget.SetInteractive(isInteractive: true);
		}
		widget.SetInteractive(isInteractive: false);
		KAToggleButton kAToggleButton = (KAToggleButton)widget;
		if ((bool)kAToggleButton && !kAToggleButton.IsChecked())
		{
			kAToggleButton.SetChecked(isChecked: true);
		}
		mPreviousFilterTabWidget = widget;
		if (updateGameData)
		{
			GetHighScoreData(mCurrentTab._Type, mCurrentFilterTab._Type);
		}
	}

	public void ChangeTab(KAWidget widget, bool updateGameData = true)
	{
		if (mUiTabMenu != null)
		{
			UDTLeaderboardTabData[] leaderboardTabs = _LeaderboardTabs;
			foreach (UDTLeaderboardTabData uDTLeaderboardTabData in leaderboardTabs)
			{
				if (!(uDTLeaderboardTabData._NameText._Text != widget.name))
				{
					mCurrentTab = uDTLeaderboardTabData;
					break;
				}
			}
		}
		widget.SetDisabled(isDisabled: true);
		if (mPreviousTabWidget != null)
		{
			mPreviousTabWidget.SetDisabled(isDisabled: false);
		}
		mPreviousTabWidget = widget;
		if (mCurrentFilterTab == null)
		{
			mCurrentFilterTab = _LeaderboardFilterTabs[0];
		}
		if (updateGameData)
		{
			GetHighScoreData(mCurrentTab._Type, mCurrentFilterTab._Type);
		}
	}

	public void ShowProgressInfo(Range dateRange)
	{
		KAWidget kAWidget = mUiFilterMenu.FindItem(mCurrentFilterTab._NameText._Text);
		if (!mCurrentFilterTab._ShowProgressBar)
		{
			return;
		}
		KAWidget kAWidget2 = kAWidget.FindChildItem("Progress");
		if (!(kAWidget2 == null) && dateRange.EndDate.HasValue)
		{
			kAWidget2.SetVisibility(inVisible: true);
			DateTime value = dateRange.EndDate.Value;
			if ((value - ServerTime.pCurrentTime).Days >= 1)
			{
				kAWidget2.SetText(value.Month + "/" + value.Day + "/" + value.Year);
			}
			else
			{
				kAWidget2.SetText(value.Hour + ":" + value.Minute + ":" + value.Second);
			}
		}
	}

	private void GetHighScoreData(RequestType dispType, ModeType filter)
	{
		string text = null;
		if (UserInfo.pInstance != null)
		{
			text = UserInfo.pInstance.UserID;
		}
		if (text == null)
		{
			Debug.LogError("UserID is null");
		}
		else if (!(mLeaderboardMenu == null))
		{
			mLeaderboardMenu.ClearItems();
			mLeaderboardMenu.SetVisibility(isVisible: false);
			if (dispType != RequestType.Facebook)
			{
				mLeaderboardMenu.LoadGameDataSummary(dispType, filter);
			}
		}
	}

	public void SetPlayerData(int rank, int score)
	{
		KAWidget kAWidget = FindItem("TxtMyRank");
		if (kAWidget != null)
		{
			if (rank > 0)
			{
				kAWidget.SetText(rank.ToString());
				kAWidget.SetVisibility(inVisible: true);
			}
			else
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
		kAWidget = FindItem("TxtMyName");
		if (kAWidget != null)
		{
			kAWidget.SetText(AvatarData.pInstance.DisplayName);
			kAWidget.SetVisibility(inVisible: true);
		}
		string inWidgetName = "TxtMyScore";
		string text = score.ToString();
		kAWidget = FindItem(inWidgetName);
		if (kAWidget != null)
		{
			kAWidget.SetText(text);
			kAWidget.SetVisibility(inVisible: true);
		}
		KAWidget kAWidget2 = FindItem("UDTStarsIcon");
		if (!(kAWidget2 == null))
		{
			kAWidget2.SetVisibility(inVisible: true);
			UDTUtilities.UpdateUDTStars(kAWidget2.transform, "ProfileStarsIconBkg", score);
		}
	}

	public void SetHighScoreData(int inScore, string inKey, bool inOrder)
	{
		mPlayerScore = inScore;
		mHighScoreKey = inKey;
		mAscendingOrder = inOrder;
	}

	public override void OnClick(KAWidget item)
	{
		string text = item.name;
		if (!(text == "BtnUDTAchievementsExpand"))
		{
			if (text == "BtnUDTAchievementsRetract" && (bool)mUiPages)
			{
				mUiPages.ShowNextPage(-1);
			}
		}
		else
		{
			OpenUDTPage();
		}
	}

	public void OpenUDTPage()
	{
		if ((bool)mUiPages)
		{
			mUiPages.ShowNextPage(1);
		}
	}
}
