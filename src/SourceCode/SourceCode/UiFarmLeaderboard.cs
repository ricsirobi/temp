using UnityEngine;

public class UiFarmLeaderboard : KAUI
{
	private KAWidget mCloseBtn;

	private UiFarmLeaderboardMenu mMenu;

	private UiFarmLeaderboardTabMenu mUiTabMenu;

	private FarmLeaderboardTabData mCurrentTab;

	private KAWidget mPreviousTabWidget;

	private KAWidget mTxtGettingRankings;

	public FarmLeaderboardTabData[] _LeaderboardTabs;

	public RequestType _LeaderboardDefaultTab = RequestType.All;

	public LocaleString _NoClanMessageText = new LocaleString("You are not a clan member.");

	public FarmLeaderboardTabData pCurrentTab => mCurrentTab;

	public KAWidget pTxtGettingRankings => mTxtGettingRankings;

	protected override void Start()
	{
		base.Start();
		AvAvatar.pState = AvAvatarState.PAUSED;
		mCloseBtn = FindItem("CloseBtn");
		mTxtGettingRankings = FindItem("TxtGettingRankings");
		mMenu = (UiFarmLeaderboardMenu)GetMenu("UiFarmLeaderboardMenu");
		mUiTabMenu = (UiFarmLeaderboardTabMenu)GetMenu("UiFarmLeaderboardTabMenu");
		KAUI.SetExclusive(this);
		Init();
	}

	private void Init()
	{
		SetupTabMenu();
		mMenu.Init();
		if (_LeaderboardTabs == null || _LeaderboardTabs.Length == 0)
		{
			return;
		}
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

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mCloseBtn)
		{
			AvAvatar.pState = AvAvatar.pPrevState;
			KAUI.RemoveExclusive(this);
			Object.Destroy(base.gameObject);
		}
	}

	public void ChangeTab(KAWidget widget, bool updateGameData = true)
	{
		if (mUiTabMenu != null)
		{
			FarmLeaderboardTabData[] leaderboardTabs = _LeaderboardTabs;
			foreach (FarmLeaderboardTabData farmLeaderboardTabData in leaderboardTabs)
			{
				if (farmLeaderboardTabData._NameText._Text == widget.name)
				{
					mCurrentTab = farmLeaderboardTabData;
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
		mMenu.GetLeaderboardData(mCurrentTab._Type);
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
			FarmLeaderboardTabData farmLeaderboardTabData = _LeaderboardTabs[i];
			KAWidget kAWidget = mUiTabMenu.DuplicateWidget(mUiTabMenu._Template);
			if (kAWidget != null)
			{
				kAWidget.name = farmLeaderboardTabData._NameText._Text;
				if (farmLeaderboardTabData._NameText != null && !string.IsNullOrEmpty(farmLeaderboardTabData._NameText._Text))
				{
					kAWidget.SetText(farmLeaderboardTabData._NameText.GetLocalizedString());
				}
				kAWidget.FindChildItem("FBIcon").SetVisibility(farmLeaderboardTabData._Type == RequestType.Facebook);
				kAWidget.SetVisibility(inVisible: true);
				mUiTabMenu.AddWidget(kAWidget);
			}
		}
	}
}
