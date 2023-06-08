using System;
using UnityEngine;

public class UiWorldEventEndDB : UiDragonsEndDB
{
	public UiWorldEventRewards _UiRewards;

	public UiWorldEventScoresBox _UiScoreBox;

	public KAUI _EventCompleteUi;

	private KAWidget mCloseBtn;

	private KAWidget mEventWon;

	private KAWidget mEventLost;

	private bool mIsVictorySplashShown;

	private AvAvatarState mPrevAvatarState;

	private bool mInitialized;

	public bool pInitialized => mInitialized;

	public bool pIsVictorySplashShown => mIsVictorySplashShown;

	protected override void Start()
	{
		base.Start();
		SetGameSettings("WorldEventScoutAttack", base.gameObject, "any");
		mCloseBtn = FindItem("CloseBtn");
		UiWorldEventScoresBox uiScoreBox = _UiScoreBox;
		uiScoreBox.onScoreTabChange = (OnScoreTabChange)Delegate.Combine(uiScoreBox.onScoreTabChange, new OnScoreTabChange(OnScoreTabChanged));
		mEventWon = _EventCompleteUi.FindItem("WorldEventVictory");
		mEventLost = _EventCompleteUi.FindItem("WorldEventDefeat");
		mInitialized = true;
	}

	private void OnScoreTabChanged(UiWorldEventScoresBox.ScoreTab inScoreTabType)
	{
		_UiRewards.OnScoreTabChanged(inScoreTabType);
	}

	public void ClearScores()
	{
		_UiScoreBox.ClearScores();
	}

	public void AddPlayerScore(int rank, string rewardName, string playerName, int score)
	{
		if (_UiScoreBox == null)
		{
			UtDebug.Log("Player score menu is null");
		}
		else
		{
			_UiScoreBox.AddPlayerScore(rank, playerName, score, rewardName);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCloseBtn)
		{
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = mPrevAvatarState;
			KAUI.RemoveExclusive(this);
			SetVisibility(Visibility: false);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void Show(AchievementReward[] rewards = null, string rewardTier = "", bool inEventWon = false)
	{
		if (rewards != null)
		{
			_UiRewards.ShowRewards(rewards, rewardTier, inEventWon);
		}
		else
		{
			_UiRewards.SetState(KAUIState.DISABLED);
		}
		SetVisibility(Visibility: true);
		mUpsellDropDownUI.SlideToTargetPosition();
		KAUI.SetExclusive(this);
		mPrevAvatarState = ((AvAvatar.pState == AvAvatarState.PAUSED) ? AvAvatar.pPrevState : AvAvatar.pState);
		_UiScoreBox.LoadScores();
	}

	public void ShowEventCompletedMsg(bool isWon)
	{
		mIsVictorySplashShown = isWon;
		mEventWon.SetVisibility(isWon);
		mEventLost.SetVisibility(!isWon);
	}

	public void OnStoreOpened()
	{
		AvAvatar.pState = AvAvatarState.PAUSED;
		KAUI.RemoveExclusive(this);
	}

	public void OnStoreClosed()
	{
		KAUI.SetExclusive(this);
	}
}
