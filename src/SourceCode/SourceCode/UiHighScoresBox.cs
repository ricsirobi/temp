using UnityEngine;

public class UiHighScoresBox : KAUI
{
	public HighScoreTabData[] _HighScoreTabs;

	public LocaleString _NoClanMessageText = new LocaleString("You are not a clan member.");

	private UiHighScoresTabMenu mUiTabMenu;

	private UiHighScoresListMenu mHighScoreListMenu;

	private UiDragonsEndDB mParentUI;

	private KAWidget mTxtGettingHighScores;

	private HighScoreTabData mCurrentTab;

	private int mPlayerScore;

	private string mHighScoreKey;

	private bool mAscendingOrder;

	private KAWidget mPreviousTabWidget;

	public UiDragonsEndDB pParentUI => mParentUI;

	public KAWidget pTxtGettingHighScores => mTxtGettingHighScores;

	public HighScoreTabData pCurrentTab => mCurrentTab;

	public int pPlayerScore => mPlayerScore;

	public string pHighScoreKey => mHighScoreKey;

	public bool pAscendingOrder => mAscendingOrder;

	protected override void Start()
	{
		base.Start();
		mUiTabMenu = (UiHighScoresTabMenu)GetMenu("UiHighScoresTabMenu");
		mHighScoreListMenu = (UiHighScoresListMenu)GetMenu("UiHighScoresListMenu");
		mTxtGettingHighScores = FindItem("TxtGettingHighscores");
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (mUiTabMenu != null)
		{
			mUiTabMenu.SetVisibility(inVisible);
		}
		if (mHighScoreListMenu != null)
		{
			mHighScoreListMenu.SetVisibility(inVisible);
		}
	}

	public override void SetInteractive(bool inInteractive)
	{
		base.SetInteractive(inInteractive);
		if (mUiTabMenu != null)
		{
			mUiTabMenu.SetInteractive(inInteractive);
		}
		if (mHighScoreListMenu != null)
		{
			mHighScoreListMenu.SetInteractive(inInteractive);
		}
	}

	public void Init(UiDragonsEndDB inParent)
	{
		mParentUI = inParent;
		SetTabMenu();
		if (_HighScoreTabs != null && _HighScoreTabs.Length != 0)
		{
			KAWidget widget = mUiTabMenu.FindItem(_HighScoreTabs[0]._NameText._Text);
			ChangeTab(widget, setGameData: true);
		}
	}

	public void SetTabMenu()
	{
		if (_HighScoreTabs == null || _HighScoreTabs.Length == 0)
		{
			return;
		}
		if (mUiTabMenu != null)
		{
			mUiTabMenu.ClearItems();
		}
		mUiTabMenu.SetVisibility(inVisible: true);
		for (int i = 0; i < _HighScoreTabs.Length; i++)
		{
			HighScoreTabData highScoreTabData = _HighScoreTabs[i];
			KAWidget kAWidget = mUiTabMenu.DuplicateWidget(mUiTabMenu._Template);
			if (kAWidget != null)
			{
				kAWidget.name = highScoreTabData._NameText._Text;
				if (highScoreTabData._NameText != null && !string.IsNullOrEmpty(highScoreTabData._NameText._Text))
				{
					kAWidget.SetText(highScoreTabData._NameText.GetLocalizedString());
				}
				kAWidget.SetVisibility(inVisible: true);
				mUiTabMenu.AddWidget(kAWidget);
			}
		}
	}

	public void ChangeTab(KAWidget widget, bool setGameData)
	{
		if (mUiTabMenu != null)
		{
			HighScoreTabData[] highScoreTabs = _HighScoreTabs;
			foreach (HighScoreTabData highScoreTabData in highScoreTabs)
			{
				if (highScoreTabData._NameText._Text == widget.name)
				{
					mCurrentTab = highScoreTabData;
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
		GetHighScoreData(mCurrentTab._Type, setGameData);
	}

	private void GetHighScoreData(eLaunchPage dispType, bool setGameData)
	{
		if (HighScores.pInstance == null)
		{
			return;
		}
		string text = null;
		if (UserInfo.pInstance != null)
		{
			text = UserInfo.pInstance.UserID;
		}
		if (text == null)
		{
			Debug.LogError("UserID is null");
			return;
		}
		if (setGameData && HighScores.GetGameData(0) != null)
		{
			string xMLString = HighScores.GetXMLString();
			WsWebService.SetGameData(text, HighScores.pInstance.GameID, HighScores.pInstance.IsMultiPlayer, HighScores.pInstance.Difficulty, HighScores.pInstance.Level, xMLString, HighScores.pInstance.Win, HighScores.pInstance.Loss, HighScores.pInstance.mCallback, null);
		}
		HighScores.pInstance.mMessageObject = base.gameObject;
		HighScores.pInstance.mDisplayType = dispType;
		if (mHighScoreListMenu != null)
		{
			mHighScoreListMenu.ClearItems();
			mHighScoreListMenu.SetVisibility(isVisible: false);
		}
		mHighScoreListMenu.LoadGameDataSummary(dispType);
	}

	public void SetPlayerData(GameData inData, string key)
	{
		if (inData != null)
		{
			KAWidget kAWidget = FindItem("TxtMyName");
			if (kAWidget != null)
			{
				kAWidget.SetText(AvatarData.pInstance.DisplayName);
				kAWidget.SetVisibility(inVisible: true);
			}
			string inWidgetName = "TxtMyScore";
			string text = inData.Value.ToString();
			if (key == "time")
			{
				inWidgetName = "TxtMyTime";
				text = GameUtilities.FormatTime((float)inData.Value / 100f);
			}
			kAWidget = FindItem(inWidgetName);
			if (kAWidget != null)
			{
				kAWidget.SetText(text);
				kAWidget.SetVisibility(inVisible: true);
			}
		}
		else
		{
			KAWidget kAWidget = FindItem("TxtMyName");
			if (kAWidget != null)
			{
				kAWidget.SetText("---Not Ranked---");
				kAWidget.SetVisibility(inVisible: true);
			}
		}
	}

	public void ResetPlayerData()
	{
		KAWidget kAWidget = FindItem("TxtMyRank");
		if (kAWidget != null)
		{
			kAWidget.SetText(string.Empty);
		}
		kAWidget = FindItem("TxtMyName");
		if (kAWidget != null)
		{
			kAWidget.SetText(string.Empty);
		}
		kAWidget = FindItem("TxtMyScore");
		if (kAWidget != null)
		{
			kAWidget.SetText(string.Empty);
		}
		kAWidget = FindItem("TxtMyTime");
		if (kAWidget != null)
		{
			kAWidget.SetText(string.Empty);
		}
	}

	public void SetHighScoreData(int inScore, string inKey, bool inOrder)
	{
		mPlayerScore = inScore;
		mHighScoreKey = inKey;
		mAscendingOrder = inOrder;
	}
}
