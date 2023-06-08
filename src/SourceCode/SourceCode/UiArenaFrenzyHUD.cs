using System.Collections.Generic;
using UnityEngine;

public class UiArenaFrenzyHUD : KAUI
{
	private KAWidget mWaitMessageBkg;

	private KAWidget mWaitMessageText;

	private KAWidget mTxtTimeCount;

	private KAWidget mMatchStartingBkg;

	private KAWidget mDragonOrnament;

	private KAWidget mTimerBkg;

	private KAWidget mTimerBkgFlash;

	public KAWidget[] _ScoreWidgets;

	public KAWidget[] _PlayerInfos;

	public LocaleString _QuitConfirmationText = new LocaleString("Are you sure you want to go to Main menu?");

	public LocaleString _QuitConfirmationTitleText = new LocaleString("Quit Arena Frenzy?");

	public LocaleString _WaitingForPlayerText = new LocaleString("Waiting for opponent");

	public int _FlashTimerForSeconds = 10;

	public float _FlashRate = 0.25f;

	private KAWidget mBtnBack;

	private KAUIGenericDB mKAUIGenericDB;

	private List<Transform> mHUDElements = new List<Transform>();

	private float mGameTime;

	public float pGameTime => mGameTime;

	protected override void Start()
	{
		base.Start();
		mWaitMessageBkg = FindItem("WaitMessageBkg");
		mWaitMessageText = FindItem("TxtWaitMessage");
		mTxtTimeCount = FindItem("TxtTimeCount");
		mMatchStartingBkg = FindItem("MatchStartingBkg");
		mDragonOrnament = FindItem("DragonOrnament");
		mTimerBkg = FindItem("TimerBkg");
		mTimerBkgFlash = FindItem("TimerBkgFlash");
		mBtnBack = FindItem("BtnBack");
		if (ArenaFrenzyGame.pInstance != null)
		{
			mGameTime = ArenaFrenzyGame.pInstance._GameTimeInSecs;
		}
		if (mTxtTimeCount != null)
		{
			mTxtTimeCount.SetVisibility(inVisible: false);
		}
	}

	public void WaitingForPlayers(bool waiting)
	{
		if (waiting)
		{
			ShowMessage(_WaitingForPlayerText.GetLocalizedString());
		}
		else
		{
			HideMessage();
		}
	}

	public void SetMMOStatusMessage(LocaleString message)
	{
		mWaitMessageText.SetText(message.GetLocalizedString());
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		DestroyKAUIDB();
		if (inWidget == mBtnBack)
		{
			mKAUIGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDBSm", _QuitConfirmationText.GetLocalizedString(), _QuitConfirmationTitleText.GetLocalizedString(), base.gameObject, "ResetGame", "CancelQuitArenaFrenzy", "", "", inDestroyOnClick: true);
			if (ArenaFrenzyGame.pInstance != null)
			{
				ArenaFrenzyGame.pInstance.OnBackBtnClick();
			}
		}
	}

	private void QuitArenaFrenzy()
	{
		if (ArenaFrenzyGame.pInstance != null)
		{
			ArenaFrenzyGame.pInstance.ExitGame();
		}
	}

	private void ResetGame()
	{
		if (ArenaFrenzyGame.pInstance != null)
		{
			ArenaFrenzyGame.pInstance.ResetGame();
			mMatchStartingBkg.SetVisibility(inVisible: false);
		}
	}

	private void CancelQuitArenaFrenzy()
	{
		if (ArenaFrenzyGame.pInstance != null)
		{
			ArenaFrenzyGame.pInstance.OnClosingQuitDB();
		}
	}

	public void ShowKAUIDialog(string assetName, string dbName, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, LocaleString localeString, GameObject msgObject = null)
	{
		if (mKAUIGenericDB != null)
		{
			DestroyKAUIDB();
		}
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB(assetName, dbName);
		if (mKAUIGenericDB != null)
		{
			if (msgObject == null)
			{
				msgObject = base.gameObject;
			}
			mKAUIGenericDB.SetMessage(msgObject, yesMessage, noMessage, okMessage, closeMessage);
			mKAUIGenericDB.SetDestroyOnClick(destroyDB);
			mKAUIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
			mKAUIGenericDB.SetTextByID(localeString._ID, localeString._Text, interactive: false);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	public void DestroyKAUIDB()
	{
		if (!(mKAUIGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			Object.DestroyImmediate(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	public void ShowMessage(string message, float duration = 0f)
	{
		mWaitMessageText.SetText(message);
		mWaitMessageBkg.SetVisibility(inVisible: true);
		mWaitMessageText.SetVisibility(inVisible: true);
		CancelInvoke("HideMessage");
		if (duration > 0f)
		{
			Invoke("HideMessage", duration);
		}
	}

	public void HideMessage()
	{
		mWaitMessageBkg.SetVisibility(inVisible: false);
		mWaitMessageText.SetVisibility(inVisible: false);
	}

	protected override void Update()
	{
		if (ArenaFrenzyGame.pInstance != null && AvAvatar.pState != AvAvatarState.PAUSED)
		{
			if (ArenaFrenzyGame.pInstance.pIsInGame)
			{
				mGameTime -= Time.deltaTime;
				if (mGameTime < 0f)
				{
					mGameTime = 0f;
				}
				int num = (int)(mGameTime / 60f);
				int num2 = (int)(mGameTime % 60f);
				string text = $"{num:00}" + ":" + $"{num2:00}";
				mTxtTimeCount.SetText(text);
				if (num == 0 && num2 == _FlashTimerForSeconds && !IsInvoking("FlashTimer"))
				{
					InvokeRepeating("FlashTimer", 1f, _FlashRate);
				}
			}
			if (ArenaFrenzyGame.pInstance.pGameMode != 0)
			{
				int teamCount = ArenaFrenzyGame.pInstance.GetTeamCount();
				for (int i = 0; i < teamCount; i++)
				{
					if (i < _ScoreWidgets.Length)
					{
						_ScoreWidgets[i].SetText(ArenaFrenzyGame.pInstance.GetScore(i).ToString());
					}
				}
			}
		}
		base.Update();
	}

	private void FlashTimer()
	{
		if (mTimerBkgFlash != null)
		{
			mTimerBkgFlash.SetVisibility(!mTimerBkgFlash.GetVisibility());
		}
	}

	private void SetPlayerInfo(int idx, ArenaFrenzyGame.PlayerInfo inInfo)
	{
		if (idx < _PlayerInfos.Length)
		{
			_PlayerInfos[idx].SetVisibility(inVisible: true);
			if (!string.IsNullOrEmpty(inInfo._Name))
			{
				KAWidget kAWidget = _PlayerInfos[idx].FindChildItem("TxtNamePlayer");
				kAWidget.SetText(inInfo._Name);
				kAWidget.SetVisibility(inVisible: true);
			}
			else
			{
				Debug.Log("Player Name is empty");
			}
			KAWidget kAWidget2 = _PlayerInfos[idx].FindChildItem("PlayerAvatar");
			inInfo.SetPhotoWidget(kAWidget2);
			kAWidget2.SetVisibility(inVisible: true);
		}
	}

	public void OnGameStart()
	{
		if (ArenaFrenzyGame.pInstance == null)
		{
			return;
		}
		if (mMatchStartingBkg != null && ArenaFrenzyGame.pInstance.pGameMode != ArenaFrenzyGame.GAME_MODE.SINGLE_PLAYER)
		{
			mMatchStartingBkg.SetVisibility(inVisible: true);
		}
		ArenaFrenzyGame.GAME_MODE pGameMode = ArenaFrenzyGame.pInstance.pGameMode;
		if ((uint)(pGameMode - 2) <= 1u)
		{
			for (int i = 0; i < ArenaFrenzyGame.pInstance.pTeams.Count; i++)
			{
				SetPlayerInfo(i, ArenaFrenzyGame.pInstance.pTeams[i]._Players[0]);
			}
		}
	}

	public void OnGameComplete()
	{
		DestroyKAUIDB();
		if (mTimerBkg != null && mDragonOrnament != null)
		{
			mTimerBkg.SetVisibility(inVisible: true);
		}
		if (mDragonOrnament != null)
		{
			mDragonOrnament.SetVisibility(inVisible: true);
		}
		if (mTxtTimeCount != null)
		{
			mTxtTimeCount.SetVisibility(inVisible: false);
		}
		if (mTimerBkgFlash != null)
		{
			mTimerBkgFlash.SetVisibility(inVisible: false);
			CancelInvoke("FlashTimer");
		}
		ResetHUD();
	}

	public void OnCountDownComplete()
	{
		if (mDragonOrnament != null && mTimerBkg != null)
		{
			mDragonOrnament.SetVisibility(inVisible: false);
		}
		if (mTimerBkg != null)
		{
			mTimerBkg.SetVisibility(inVisible: true);
		}
		if (mTxtTimeCount != null)
		{
			mTxtTimeCount.SetVisibility(inVisible: true);
		}
		if (ArenaFrenzyGame.pInstance != null)
		{
			mGameTime = ArenaFrenzyGame.pInstance._GameTimeInSecs;
		}
		if (mMatchStartingBkg != null)
		{
			mMatchStartingBkg.SetVisibility(inVisible: false);
		}
		mBtnBack.SetVisibility(inVisible: true);
		InitScore();
	}

	public void InitScore()
	{
		if (ArenaFrenzyGame.pInstance == null)
		{
			return;
		}
		int teamCount = ArenaFrenzyGame.pInstance.GetTeamCount();
		for (int i = 0; i < teamCount; i++)
		{
			if (i < _ScoreWidgets.Length)
			{
				_ScoreWidgets[i].SetText("");
				_ScoreWidgets[i].SetVisibility(inVisible: true);
			}
		}
	}

	public void ResetHUD()
	{
		mGameTime = 0f;
		for (int i = 0; i < _ScoreWidgets.Length; i++)
		{
			_ScoreWidgets[i].SetText("");
			_ScoreWidgets[i].SetVisibility(inVisible: false);
		}
	}

	public void AddHUDElement(Transform inHUDElement, Transform inMarker = null)
	{
		if (!mHUDElements.Contains(inHUDElement))
		{
			mHUDElements.Add(inHUDElement);
			if (inHUDElement.parent != base.transform)
			{
				inHUDElement.parent = base.transform;
			}
		}
		if (inMarker != null)
		{
			inHUDElement.localPosition = inMarker.localPosition;
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		foreach (Transform mHUDElement in mHUDElements)
		{
			mHUDElement.gameObject.SetActive(inVisible);
		}
	}

	public void OnJoiningGame()
	{
		DestroyKAUIDB();
		mBtnBack.SetVisibility(inVisible: false);
	}
}
