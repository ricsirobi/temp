public class UiGauntletShooterScore : KAUI
{
	private KAWidget mTitle;

	private KAWidget mTopScoreBtn;

	private UiGauntletShooreScoreMenu mGameMenu;

	private KAWidget mAniDragonPoints;

	private KAWidget mAniDragonPlayer;

	private KAWidget mAniCoins;

	private KAWidget mAniGems;

	private KAWidget mAniRewardItem;

	private KAWidget mTxtChallengeResult;

	private KAWidget mBtnChallenge;

	private KAWidget mBtnClose;

	protected override void Start()
	{
		base.Start();
		mTitle = FindItem("Title");
		mTopScoreBtn = FindItem("TopScores");
		mGameMenu = (UiGauntletShooreScoreMenu)GetMenu("UiGauntletShooreScoreMenu");
		mAniDragonPoints = FindItem("AniDragonPoints");
		mAniDragonPlayer = FindItem("AniDragonPlayer");
		mAniCoins = FindItem("AniCoins");
		mAniGems = FindItem("AniGems");
		mAniRewardItem = FindItem("AniRewardItem");
		mTxtChallengeResult = FindItem("TxtChallengeResult");
		mBtnChallenge = FindItem("BtnChallenge");
		mBtnClose = FindItem("CloseBtn");
		SetRewardsInvisible();
	}

	public void SetChallengeBtnDisabled(bool isDisable)
	{
		mBtnChallenge.SetDisabled(isDisable);
	}

	public void SetTitleText(string inTxt)
	{
		mGameMenu.ClearItems();
		if (mTitle != null)
		{
			mTitle.SetText(inTxt);
		}
	}

	public void SetChallengeResult(string inTxtResult)
	{
		if (mTxtChallengeResult != null)
		{
			mTxtChallengeResult.SetText(inTxtResult);
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mTopScoreBtn)
		{
			KAUI.RemoveExclusive(this);
			SetVisibility(inVisible: false);
			SetRewardsInvisible();
			GauntletRailShootManager.pInstance.ShowTodaysTopScore();
		}
		else if (item.name == "BtnExit" || item == mBtnClose)
		{
			KAUI.RemoveExclusive(this);
			SetVisibility(inVisible: false);
			if (SanctuaryManager.pInstance.pPetMeter != null)
			{
				SanctuaryManager.pInstance.pPetMeter.gameObject.SetActive(value: false);
			}
			GauntletRailShootManager.pInstance.ShowLevelSelectionMenu();
		}
		else if (item == mBtnChallenge)
		{
			SetVisibility(inVisible: false);
			UiChallengeInvite.Show(base.gameObject, "OnChallengeDone");
		}
	}

	private void OnChallengeDone()
	{
		if (UiChallengeInvite.pIsChallengeSent)
		{
			mBtnChallenge.SetDisabled(isDisabled: true);
		}
		SetVisibility(inVisible: true);
	}

	public void SetScoreItem(string inName, string inVal)
	{
		KAWidget kAWidget = DuplicateWidget("ScoreTemplate");
		mGameMenu.AddWidget(kAWidget);
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.FindChildItem("ItemName").SetText(inName);
		kAWidget.FindChildItem("ItemValue").SetText(inVal);
	}

	public void SetRewardsInvisible()
	{
		if (mAniDragonPoints != null)
		{
			mAniDragonPoints.SetVisibility(inVisible: false);
		}
		if (mAniDragonPlayer != null)
		{
			mAniDragonPlayer.SetVisibility(inVisible: false);
		}
		if (mAniCoins != null)
		{
			mAniCoins.SetVisibility(inVisible: false);
		}
		if (mAniGems != null)
		{
			mAniGems.SetVisibility(inVisible: false);
		}
		if (mAniRewardItem != null)
		{
			mAniRewardItem.SetVisibility(inVisible: false);
		}
	}

	public void SetRewards(AchievementReward[] inRewards)
	{
		if (inRewards == null)
		{
			return;
		}
		foreach (AchievementReward achievementReward in inRewards)
		{
			KAWidget kAWidget = null;
			switch (achievementReward.PointTypeID)
			{
			case 8:
				if (mAniDragonPoints != null)
				{
					mAniDragonPoints.SetVisibility(inVisible: true);
					kAWidget = mAniDragonPoints.FindChildItem("TxtXPPoints");
				}
				break;
			case 1:
				if (mAniDragonPlayer != null)
				{
					mAniDragonPlayer.SetVisibility(inVisible: true);
					kAWidget = mAniDragonPlayer.FindChildItem("TxtXPPoints");
				}
				break;
			case 2:
				if (mAniCoins != null)
				{
					mAniCoins.SetVisibility(inVisible: true);
					kAWidget = mAniCoins.FindChildItem("TxtCoins");
				}
				break;
			case 5:
				if (mAniGems != null)
				{
					mAniGems.SetVisibility(inVisible: true);
					kAWidget = mAniGems.FindChildItem("TxtGems");
				}
				break;
			case 6:
				if (mAniRewardItem != null)
				{
					mAniRewardItem.SetVisibility(inVisible: true);
					kAWidget = mAniRewardItem.FindChildItem("TxtRewardItems");
				}
				break;
			}
			if (kAWidget != null)
			{
				kAWidget.SetText(achievementReward.Amount.Value.ToString());
			}
		}
	}
}
