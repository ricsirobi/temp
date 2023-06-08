using UnityEngine;

public class UiRacingSingleplayer : KAUI
{
	private MainMenu mMainMenu;

	public string _TrackSelectHelpDBRes = "RS_DATA/PfUiHelpTrekJoinLobby.unity3d/PfUiHelpTrekJoinLobby";

	public string _TrackSelectHelpMenuType = "JoinLobby";

	public LocaleString _ExitConfirmText = new LocaleString("Are you sure you want to exit to the main lobby?");

	public LocaleString _MMODisabledExitConfirmText = new LocaleString("Are you sure you want to exit from the game?");

	private KAWidget mCashTotalItem;

	private UiRacingSingleplayerMenu mUiRaceSelectMenu;

	private KAWidget mTrackNameTxt;

	private KAWidget mRaceTypeTxt;

	private KAWidget mBtnTrackLeft;

	private KAWidget mBtnTrackRight;

	public MainMenu pMainMenu => mMainMenu;

	public void EnableUi(MainMenu uiMainMenu)
	{
		SetVisibility(inVisible: true);
		mMainMenu = uiMainMenu;
		if (mUiRaceSelectMenu == null)
		{
			mUiRaceSelectMenu = _MenuList[0] as UiRacingSingleplayerMenu;
		}
		mCashTotalItem = FindItem("txtCashTotal");
		mCashTotalItem?.SetText(Money.pCashCurrency.ToString());
		mCashTotalItem?.SetInteractive(isInteractive: true);
		mTrackNameTxt = FindItem("TrackNameTextBox");
		mRaceTypeTxt = FindItem("RaceTypeTextBox");
		mMainMenu.SetCurrentGameMode(GameModes.SINGLERACE);
		mRaceTypeTxt.SetText(mMainMenu.pCurrentGameModeData._GameModeText._Text);
		mBtnTrackLeft = FindItem("BtnTrackLeft");
		mBtnTrackRight = FindItem("BtnTrackRight");
		SetTrackWithIndex(0);
		SetUiMenu();
	}

	public void SetUiMenu()
	{
		for (int i = 0; i < mMainMenu.pActiveTrackData.Count; i++)
		{
			KAWidget kAWidget = mUiRaceSelectMenu.DuplicateWidget(mUiRaceSelectMenu._Template.FindChildItem("TrackImage"));
			kAWidget.name = mMainMenu.pActiveTrackData[i]._TrackSceneName;
			kAWidget.SetTexture(mMainMenu.pActiveTrackData[i]._TrackIcon);
			kAWidget.SetVisibility(inVisible: true);
			mUiRaceSelectMenu.AddWidget(kAWidget);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnBack")
		{
			if (MainStreetMMOClient.pIsMMOEnabled)
			{
				mMainMenu.ShowKAUIDialog("PfKAUIGenericDBSm", "ExitConfirmDB", "ConfirmYes", "DestroyDB", "", "", destroyDB: true, _ExitConfirmText, base.gameObject);
			}
			else
			{
				mMainMenu.ShowKAUIDialog("PfKAUIGenericDBSm", "ExitConfirmDB", "ExitGame", "DestroyDB", "", "", destroyDB: true, _MMODisabledExitConfirmText, base.gameObject);
			}
		}
		else if (inWidget.name == "BtnStartRace")
		{
			pMainMenu.StartGame();
		}
		else if (inWidget.name == "BtnTrackLeft")
		{
			SetTrackWithIndex(mMainMenu.pActiveTrackData.IndexOf(mMainMenu.pCurrentTrackData) - 1);
		}
		else if (inWidget.name == "BtnTrackRight")
		{
			SetTrackWithIndex(mMainMenu.pActiveTrackData.IndexOf(mMainMenu.pCurrentTrackData) + 1);
		}
	}

	public void SetTrackWithIndex(int inIndex)
	{
		inIndex = Mathf.Clamp(inIndex, 0, mMainMenu.pActiveTrackData.Count - 1);
		mBtnTrackLeft.SetVisibility((inIndex != 0) ? true : false);
		mBtnTrackRight.SetVisibility((inIndex != mMainMenu.pActiveTrackData.Count - 1) ? true : false);
		mMainMenu.SetCurrentSelectionIndex(inIndex);
		mUiRaceSelectMenu.GoToPage(inIndex + 1);
	}

	private void ConfirmYes()
	{
		MainStreetMMOClient.pInstance.Connect();
		DestroyDB();
		RacingManager.pIsSinglePlayer = false;
		SetVisibility(inVisible: false);
		mUiRaceSelectMenu.ClearItems();
		mMainMenu.MoveNext(GameScreen.GAME_MODE_SCREEN);
	}

	private void ExitGame()
	{
		DestroyDB();
		UiChatHistory._IsVisible = false;
		SetVisibility(inVisible: false);
		mMainMenu.ClearPictureCache();
		RacingManager.pIsSinglePlayer = false;
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.pNotifyObjOnIdlePopUpClose = null;
			MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.ALLOWED, forceUpdate: true);
			MainStreetMMOClient.pInstance.ForceEnableMultiplayer(enable: false);
		}
		RsResourceManager.LoadLevel(mMainMenu._LastLevelToLoad);
	}

	private void DestroyDB()
	{
		mMainMenu.DestroyKAUIDB();
	}

	public void UpdateTrackInfo()
	{
		mTrackNameTxt.SetTextByID(mMainMenu.pCurrentTrackData._TrackNameText._ID, mMainMenu.pCurrentTrackData._TrackNameText._Text);
	}
}
