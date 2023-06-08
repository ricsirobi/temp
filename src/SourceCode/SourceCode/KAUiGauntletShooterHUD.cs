using System.Collections;
using UnityEngine;

public class KAUiGauntletShooterHUD : KAUiGameHUD
{
	public LocaleString _GameEndTimerText;

	public LocaleString _ExitFireBallText = new LocaleString("Do you really want to quit?");

	public LocaleString _ExitChallengeText = new LocaleString("Are you sure you want to lose this challenge?");

	private KAWidget mTxtOpponentScore;

	private KAWidget mTxtOpponentAccuracy;

	private KAWidget mTxtGameEndTimer;

	private KAWidget mTxtYouWinMessage;

	private KAWidget mOpponentScore;

	private KAWidget mOpponentAccuracy;

	private KAUIGenericDB mGenericDB;

	private KAWidget mBkgExtraTime;

	private KAWidget mTxtExtraTime;

	private KAWidget mChallengePoints;

	private float mChallengePointFlashDuration;

	private float mChallengePointFlashTimer;

	private int mChallengePointFlashTimes;

	private GauntletRailShootManager mGameManager;

	private KAWidget mBonusLevelAnim;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		mTxtOpponentScore = FindItem("txtOpponentScore");
		mTxtOpponentAccuracy = FindItem("txtOpponentAccuracy");
		mTxtGameEndTimer = FindItem("txtGameEndTimer");
		mTxtYouWinMessage = FindItem("txtYouWinMessage");
		mOpponentScore = FindItem("OpponentScore");
		mOpponentAccuracy = FindItem("OpponentAccuracy");
		mBkgExtraTime = FindItem("AniExtraTime");
		mTxtExtraTime = mBkgExtraTime.FindChildItem("TxtExtraTime");
		mChallengePoints = FindItem("ChallengePoints");
		mBonusLevelAnim = FindItem("AniBonusLevel");
		mGameManager = GauntletRailShootManager.pInstance;
	}

	public override void OnPressRepeated(KAWidget inWidget, bool inPressed)
	{
		if (inPressed && inWidget == mFireBtn)
		{
			mGameManager._GauntletController.ShootFireBall();
		}
		base.OnPressRepeated(inWidget, inPressed);
	}

	public override void SetVisibility(bool t)
	{
		mTxtOpponentScore = FindItem("txtOpponentScore");
		mTxtOpponentAccuracy = FindItem("txtOpponentAccuracy");
		mTxtGameEndTimer = FindItem("txtGameEndTimer");
		mTxtYouWinMessage = FindItem("txtYouWinMessage");
		mOpponentScore = FindItem("OpponentScore");
		mOpponentAccuracy = FindItem("OpponentAccuracy");
		base.SetVisibility(t);
		if (UtPlatform.IsMobile() && mHelpBtn != null)
		{
			mHelpBtn.SetVisibility(inVisible: true);
		}
		if (t)
		{
			mTxtOpponentScore.SetText("0");
			mTxtOpponentAccuracy.SetText("0");
			SetOpponentName("");
			mTxtYouWinMessage.SetVisibility(inVisible: false);
			mTxtGameEndTimer.SetVisibility(inVisible: false);
		}
		if (mGameManager != null && mGameManager.pIsMultiplayer)
		{
			FindItem("AniScScoreBkg").SetVisibility(inVisible: true);
			FindItem("AniAccuracyBkg").SetVisibility(inVisible: true);
			mOpponentScore.SetVisibility(inVisible: true);
			mOpponentAccuracy.SetVisibility(inVisible: true);
		}
		else
		{
			FindItem("AniScScoreBkg").SetVisibility(inVisible: true);
			FindItem("AniAccuracyBkg").SetVisibility(inVisible: true);
			mOpponentScore.SetVisibility(inVisible: false);
			mOpponentAccuracy.SetVisibility(inVisible: false);
		}
	}

	public void SetOpponentName(string inOpponentName)
	{
		if (!(mOpponentScore == null))
		{
			KAWidget kAWidget = mOpponentScore.FindChildItem("txtOpponentName");
			if (kAWidget != null)
			{
				kAWidget.SetText(inOpponentName);
			}
		}
	}

	public void SetOpponentScore(int inScore, int inAccuracy)
	{
		mTxtOpponentScore.SetTextByID(0, inScore.ToString());
		mTxtOpponentAccuracy.SetTextByID(0, inAccuracy.ToString());
	}

	public void PlayCountdown(int countdown)
	{
		if (4 - countdown == 0)
		{
			mCountDown.SetVisibility(inVisible: true);
			if (mBackBtn != null)
			{
				mBackBtn.SetVisibility(inVisible: false);
			}
			if (mHelpBtn != null)
			{
				mHelpBtn.SetVisibility(inVisible: false);
			}
			if (mFireBtn != null)
			{
				mFireBtn.SetVisibility(inVisible: false);
			}
			mCountDown.PlayAnim("Play", 0);
		}
		if (_CountDownSFX != null)
		{
			SnChannel.Play(_CountDownSFX, "SFX_Pool", 0, inForce: true, null);
		}
		mCountDown.PlayAnim("Play", 0);
	}

	public void StopCountdown()
	{
		OnAnimEnd(mCountDown, 3);
	}

	public void ShowGameEndTimer(bool isShow, int incount)
	{
		mTxtGameEndTimer.SetVisibility(isShow);
		if (isShow)
		{
			string stringData = StringTable.GetStringData(_GameEndTimerText._ID, _GameEndTimerText._Text);
			string stringData2 = StringTable.GetStringData(mGameManager._SecondsText._ID, mGameManager._SecondsText._Text);
			mTxtGameEndTimer.SetText(stringData + incount + " " + stringData2);
		}
	}

	public IEnumerator ShowYouWinMessage()
	{
		mTxtYouWinMessage.SetVisibility(inVisible: true);
		mBackBtn.SetVisibility(inVisible: false);
		mHelpBtn.SetVisibility(inVisible: false);
		if (mFireBtn != null)
		{
			mFireBtn.SetDisabled(isDisabled: true);
		}
		if (mGameManager._GauntletController != null)
		{
			mGameManager._GauntletController._Pause = true;
			mGameManager._GauntletController.SetDisabled(isDisabled: true);
		}
		mGameManager.pIsGameRunning = false;
		yield return new WaitForSeconds(2f);
		mTxtYouWinMessage.SetVisibility(inVisible: false);
		int score = GetScore();
		int score2 = score + mGameManager.GetAccuracyScore((int)mGameManager.pAccuracy, score);
		GauntletMMOPlayer gauntletMMOPlayer = new GauntletMMOPlayer();
		gauntletMMOPlayer._UserID = UserInfo.pInstance.UserID;
		gauntletMMOPlayer._Score = score2;
		gauntletMMOPlayer._Accuracy = (int)mGameManager.pAccuracy;
		mGameManager.SetResults(new GauntletMMOPlayer[1] { gauntletMMOPlayer });
	}

	public IEnumerator ShowOpponentLeftMessage()
	{
		mGameManager.pOpponentLeft = true;
		mTxtYouWinMessage.SetVisibility(inVisible: true);
		yield return new WaitForSeconds(3f);
		mTxtYouWinMessage.SetVisibility(inVisible: false);
	}

	private void ShowMultiplayerMenuScreen()
	{
		SetVisibility(t: false);
		mTxtYouWinMessage.SetVisibility(inVisible: false);
		mBackBtn.SetDisabled(isDisabled: false);
		mHelpBtn.SetDisabled(isDisabled: false);
		if (mFireBtn != null)
		{
			mFireBtn.SetDisabled(isDisabled: false);
		}
		mGameManager.ShowMultiplayerMenu();
	}

	public void SetCountdownVisibility(bool isVisible)
	{
		if (mCountDown != null)
		{
			mCountDown.SetVisibility(isVisible);
		}
	}

	public void SetBonusFlash(float waitSecs)
	{
		if (mBonusLevelAnim != null)
		{
			if (mQuestionGroup != null)
			{
				mQuestionGroup.SetVisibility(inVisible: false);
			}
			mBonusLevelAnim.SetVisibility(inVisible: true);
			mBonusLevelAnim.PlayAnim("Flash", -1);
			Invoke("ResetBonusFlash", waitSecs);
		}
	}

	private void ResetBonusFlash()
	{
		if (mBonusLevelAnim != null)
		{
			mBonusLevelAnim.PlayAnim("Still", -1);
			mBonusLevelAnim.SetVisibility(inVisible: false);
		}
	}

	public void HideQuestion()
	{
		if (mQuestionGroup != null)
		{
			mQuestionGroup.SetVisibility(inVisible: false);
		}
	}

	public void HideFireButton()
	{
		if (mFireBtn != null)
		{
			mFireBtn.SetVisibility(inVisible: false);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget == mBackBtn)
		{
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
			}
			mGameManager._GauntletController._Pause = true;
			mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "GenericDB");
			mGenericDB.SetMessage(base.gameObject, "QuitFireBallFrenzy", "ContinueFireBallFrenzy", "", "");
			mGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			if (ChallengeInfo.pActiveChallenge != null)
			{
				mGenericDB.SetText(_ExitChallengeText.GetLocalizedString(), interactive: false);
			}
			else
			{
				mGenericDB.SetText(_ExitFireBallText.GetLocalizedString(), interactive: false);
			}
			mGenericDB.SetExclusive();
		}
		else
		{
			base.OnClick(inWidget);
		}
	}

	private void OnDisable()
	{
		if (mGenericDB != null)
		{
			mGenericDB.Destroy();
		}
	}

	private void QuitFireBallFrenzy()
	{
		mGameManager.ProcessUserLeft();
		mGenericDB.Destroy();
		base.OnClick(mBackBtn);
	}

	private void ContinueFireBallFrenzy()
	{
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
		}
		mGameManager._GauntletController._Pause = false;
		mGenericDB.Destroy();
	}

	public void UpdateChallengePoints(int points)
	{
		if (mChallengePoints != null)
		{
			mChallengePoints.SetText(points.ToString());
		}
	}

	public void ChallengeItemVisible(bool visible)
	{
		if (mChallengePoints != null && mChallengePoints.GetParentItem() != null)
		{
			mChallengePoints.GetParentItem().SetVisibility(visible);
		}
	}

	public bool GetChallengeItemVisibility()
	{
		if (mChallengePoints != null && mChallengePoints.GetParentItem() != null)
		{
			return mChallengePoints.GetParentItem().GetVisibility();
		}
		return false;
	}

	public void FlashChallengeItem(float interval, int loopTimes)
	{
		if (mChallengePoints != null && mChallengePoints.GetParentItem() != null)
		{
			mChallengePointFlashTimes = loopTimes;
			mChallengePointFlashDuration = interval;
			mChallengePointFlashTimer = interval;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!(mChallengePointFlashTimer > 0f))
		{
			return;
		}
		mChallengePointFlashTimer -= Time.deltaTime;
		if (!(mChallengePointFlashTimer <= 0f))
		{
			return;
		}
		if (mChallengePointFlashTimes > 0)
		{
			bool challengeItemVisibility = GetChallengeItemVisibility();
			ChallengeItemVisible(!challengeItemVisibility);
			mChallengePointFlashTimer = mChallengePointFlashDuration;
			if (!challengeItemVisibility)
			{
				mChallengePointFlashTimes--;
			}
		}
		else
		{
			ChallengeItemVisible(visible: false);
		}
	}

	public void ShowExtraTime(float timeleft)
	{
		mTxtExtraTime.SetText(timeleft.ToString("F2"));
		if (timeleft > 0f)
		{
			mBkgExtraTime.SetVisibility(inVisible: true);
		}
		else
		{
			mBkgExtraTime.SetVisibility(inVisible: false);
		}
	}

	public void ShowWaitingForGameEndMessage(bool Visibility)
	{
		if (Visibility)
		{
			mTxtGameEndTimer.SetText(mGameManager._GameEndWaitText.GetLocalizedString());
		}
		mTxtGameEndTimer.SetVisibility(Visibility);
	}
}
