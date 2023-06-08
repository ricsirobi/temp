using System.Collections.Generic;
using UnityEngine;

public class UiGameResults : KAUI, IAdResult
{
	protected UiDragonsEndDB mParentUI;

	private KAWidget mChallengeBtn;

	private KAWidget mReplayBtn;

	private KAWidget mAniGrade;

	private KAWidget mRewardParent;

	private KAWidget mUITitle;

	private KAWidget mAdsBtn;

	private int mLevelScore;

	private string mGameModule;

	protected override void Start()
	{
		base.Start();
		mChallengeBtn = FindItem("BtnChallenge");
		mReplayBtn = FindItem("ReplayBtn");
		mAniGrade = FindItem("AniGradeFlag");
		mAdsBtn = FindItem("BtnAds");
	}

	public void Init(UiDragonsEndDB inParent, string grade = null, string gradeBgColor = null)
	{
		mParentUI = inParent;
		if (mParentUI == null || mParentUI.pCurrentGameData == null)
		{
			UtDebug.LogError("NO PARENT FOUND!!!");
			return;
		}
		if (mChallengeBtn != null)
		{
			mChallengeBtn.SetVisibility(mParentUI.pCurrentGameData._IsChallengeAllowed);
		}
		if (mReplayBtn != null)
		{
			mReplayBtn.SetVisibility(mParentUI.pCurrentGameData._IsReplayAllowed);
		}
		if (mAniGrade != null)
		{
			mAniGrade.SetVisibility(mParentUI.pCurrentGameData._IsGrade);
			if (mParentUI.pCurrentGameData._IsGrade)
			{
				SetGrade(grade, gradeBgColor);
			}
		}
	}

	public void SetGrade(string inGrade, string inGradeColor)
	{
		KAWidget kAWidget = mAniGrade.FindChildItem("AniGradeGreen");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		kAWidget = mAniGrade.FindChildItem("AniGradeRed");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		kAWidget = mAniGrade.FindChildItem("AniGradeYellow");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		if (inGrade != null && inGradeColor != null)
		{
			string widgetName = "AniGrade" + inGradeColor;
			kAWidget = mAniGrade.FindChildItem(widgetName);
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.SetText(inGrade);
			}
		}
	}

	public void SetRewardDisplay(AchievementReward[] inRewards, bool updateRewardsForAds = false)
	{
		if (mAdsBtn != null && inRewards != null && inRewards.Length != 0)
		{
			if (updateRewardsForAds)
			{
				UpdateRewardsForAds(inRewards);
			}
			else if (AdManager.pInstance.AdSupported(mParentUI._AdEventType, AdType.REWARDED_VIDEO) && mLevelScore > 0)
			{
				EnableAdWidgets(inVisible: true);
			}
		}
		RewardWidget rewardWidget = (RewardWidget)FindItem("XPReward");
		if (rewardWidget != null)
		{
			if (inRewards != null && inRewards.Length != 0)
			{
				rewardWidget.SetVisibility(inVisible: true);
				rewardWidget.SetRewards(inRewards, MissionManager.pInstance._RewardData);
			}
			else
			{
				rewardWidget.SetVisibility(inVisible: false);
			}
		}
		else
		{
			UtDebug.LogWarning("Change to new RewardWidget");
			SetRewardDisplayOld(inRewards);
		}
	}

	public void UpdateRewardsForAds(AchievementReward[] inRewards)
	{
		foreach (AchievementReward achievementReward in inRewards)
		{
			int value = achievementReward.PointTypeID.Value;
			if ((uint)(value - 1) <= 1u || (uint)(value - 8) <= 2u || value == 12)
			{
				achievementReward.Amount *= 2;
			}
		}
	}

	public void SetRewardDisplayOld(AchievementReward[] inRewards)
	{
		int num = 0;
		mRewardParent = FindItem("Reward0" + (num + 1));
		while (mRewardParent != null)
		{
			mRewardParent.SetVisibility(inVisible: false);
			num++;
			mRewardParent = FindItem("Reward0" + (num + 1));
		}
		if (inRewards == null)
		{
			return;
		}
		KAWidget kAWidget = FindItem("GrpRewards");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: true);
		}
		Dictionary<int, AchievementReward> dictionary = new Dictionary<int, AchievementReward>();
		foreach (AchievementReward achievementReward in inRewards)
		{
			if (achievementReward.PointTypeID.Value == 6 || !dictionary.ContainsKey(achievementReward.PointTypeID.Value))
			{
				dictionary.Add(achievementReward.PointTypeID.Value, achievementReward);
			}
			else
			{
				dictionary[achievementReward.PointTypeID.Value].Amount += achievementReward.Amount.Value;
			}
		}
		mRewardParent = FindItem("Reward0" + ((dictionary.Keys.Count < num) ? dictionary.Keys.Count.ToString() : num.ToString()));
		if (mRewardParent != null)
		{
			mRewardParent.SetVisibility(inVisible: true);
		}
		int num2 = 1;
		foreach (KeyValuePair<int, AchievementReward> item in dictionary)
		{
			KAWidget kAWidget2 = mRewardParent.FindChildItem("RewardIcon0" + num2);
			if (kAWidget2 == null)
			{
				UtDebug.LogError("NO REWARD ICON PARENT FOUND!!!");
				continue;
			}
			kAWidget2.SetVisibility(inVisible: true);
			KAWidget kAWidget3 = mRewardParent.FindChildItem("TxtReward0" + num2);
			if (kAWidget3 == null)
			{
				UtDebug.LogError("NO REWARD ICON PARENT FOUND!!!");
				continue;
			}
			kAWidget3.SetText(item.Value.Amount.Value.ToString());
			kAWidget3.SetVisibility(inVisible: true);
			num2++;
			string text = string.Empty;
			switch (item.Key)
			{
			case 1:
				text = "IcoXPPlayer";
				break;
			case 2:
				text = "IcoCoins";
				break;
			case 5:
				text = "IcoGems";
				break;
			case 6:
				text = "IcoItem";
				break;
			case 8:
				text = "IcoXPDragon";
				break;
			case 9:
				text = "IcoXPFarming";
				break;
			case 10:
				text = "IcoXPFishing";
				break;
			case 11:
				text = "IcoTrophies";
				break;
			case 12:
				text = "IcoXPUDT";
				break;
			}
			for (int j = 0; j < kAWidget2.pChildWidgets.Count; j++)
			{
				kAWidget2.pChildWidgets[j].SetVisibility(inVisible: false);
			}
			if (text != string.Empty)
			{
				KAWidget kAWidget4 = kAWidget2.FindChildItem(text);
				if (kAWidget4 != null)
				{
					kAWidget4.SetVisibility(inVisible: true);
				}
			}
		}
	}

	public void AllowChallenge(bool allow)
	{
		if (mChallengeBtn != null)
		{
			mChallengeBtn.SetDisabled(!allow);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "DeclineBtn" || inWidget.name == "CloseBtn")
		{
			mParentUI.pMessageObject.SendMessage("OnEndDBClose", SendMessageOptions.RequireReceiver);
			mParentUI.SetVisibility(Visibility: false);
		}
		else if (inWidget.name == "ReplayBtn")
		{
			mParentUI.pMessageObject.SendMessage("OnReplayGame", SendMessageOptions.RequireReceiver);
			mParentUI.SetVisibility(Visibility: false);
		}
		else if (inWidget.name == "BtnChallenge")
		{
			UiChallengeInvite.Show(mParentUI.gameObject, "OnChallengeDone");
			mParentUI.SetVisibility(Visibility: false);
		}
		else if (inWidget == mAdsBtn && AdManager.pInstance.AdAvailable(mParentUI._AdEventType, AdType.REWARDED_VIDEO))
		{
			mParentUI.SetInteractive(interactive: false);
			AdManager.DisplayAd(mParentUI._AdEventType, AdType.REWARDED_VIDEO, base.gameObject);
		}
	}

	public void OnAdWatched()
	{
		EnableAdWidgets(inVisible: false);
		if (string.IsNullOrEmpty(mGameModule))
		{
			mParentUI.SetInteractive(interactive: true);
			mParentUI.pMessageObject.GetComponent<IAdResult>()?.OnAdWatched();
		}
		else
		{
			AdManager.pInstance.LogAdWatchedEvent(mParentUI._AdEventType, "DoubleRewards");
			UpdateRewards();
			mParentUI.SetInteractive(interactive: true);
		}
	}

	public void OnAdFailed()
	{
		mParentUI.SetInteractive(interactive: true);
		UtDebug.LogError("OnAdFailed for event:- " + mParentUI._AdEventType);
	}

	public void OnAdSkipped()
	{
		mParentUI.SetInteractive(interactive: true);
	}

	public void OnAdClosed()
	{
	}

	public void OnAdFinished(string eventDataRewardString)
	{
	}

	public void OnAdCancelled()
	{
	}

	private void UpdateRewards()
	{
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		WsWebService.ApplyPayout(mGameModule, mLevelScore, ServiceEventHandler, null);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.APPLY_PAYOUT)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			mParentUI.SetInteractive(interactive: true);
			if (inObject != null)
			{
				AchievementReward[] array = (AchievementReward[])inObject;
				if (array != null)
				{
					GameUtilities.AddRewards(array, inUseRewardManager: false, inImmediateShow: false);
					SetRewardDisplay(array, updateRewardsForAds: true);
					AdManager.pInstance.SyncAdAvailableCount(mParentUI._AdEventType, isConsumed: true);
					break;
				}
			}
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", mParentUI._AdRewardFailedText.GetLocalizedString(), null, "");
			AdManager.pInstance.SyncAdAvailableCount(mParentUI._AdEventType, isConsumed: false);
			break;
		case WsServiceEvent.ERROR:
			mParentUI.SetInteractive(interactive: true);
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", mParentUI._AdRewardFailedText.GetLocalizedString(), null, "");
			AdManager.pInstance.SyncAdAvailableCount(mParentUI._AdEventType, isConsumed: false);
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			break;
		}
	}

	public void OnChallengeDone(bool disableBtn = true)
	{
		mChallengeBtn.SetDisabled(disableBtn);
		SetVisibility(inVisible: true);
	}

	public void SetRewardMessage(string inText)
	{
		KAWidget kAWidget = FindItem("XPReward");
		if (kAWidget == null)
		{
			kAWidget = FindItem("GrpRewards");
		}
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: true);
		}
		KAWidget kAWidget2 = FindItem("TxtRewardMessage");
		if (kAWidget2 != null)
		{
			if (!string.IsNullOrEmpty(inText))
			{
				kAWidget2.SetVisibility(inVisible: true);
				kAWidget2.SetText(inText);
			}
			else
			{
				kAWidget2.SetText(string.Empty);
				kAWidget2.SetVisibility(inVisible: false);
			}
		}
	}

	public void SetAdRewardData(string moduleName, int levelScore)
	{
		mLevelScore = levelScore;
		mGameModule = moduleName;
	}

	public void EnableAdWidgets(bool inVisible)
	{
		if (mAdsBtn == null)
		{
			mAdsBtn = FindItem("BtnAds");
		}
		if (mAdsBtn != null)
		{
			mAdsBtn.SetVisibility(inVisible);
		}
	}
}
