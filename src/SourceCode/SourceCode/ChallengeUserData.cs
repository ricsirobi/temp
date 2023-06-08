using System;
using UnityEngine;

public class ChallengeUserData : KAWidgetUserData
{
	public ChallengeInfo _Challenge;

	public int _DaysLeft;

	public int _HoursLeft;

	public int _MinutesLeft;

	public int _SecondsLeft;

	private bool mLoaded;

	private string mChallengeTxt;

	private KAWidget mTxtChallenge;

	private KAWidget mTxtChallengeTime;

	private AvPhotoManager mPhotoManager;

	private LocaleString mNoDisplayNameText;

	public ChallengeUserData(ChallengeInfo challengeInfo, string challengeTxt, AvPhotoManager inPhotoManager, LocaleString noDisplayNameText)
	{
		mChallengeTxt = challengeTxt;
		_Challenge = challengeInfo;
		mPhotoManager = inPhotoManager;
		mNoDisplayNameText = noDisplayNameText;
	}

	public void Load()
	{
		if (mLoaded)
		{
			return;
		}
		mLoaded = true;
		mTxtChallenge = _Item.FindChildItem("TxtMessage");
		mTxtChallengeTime = _Item.FindChildItem("TxtDate");
		string text = _Challenge.UserID.ToString();
		bool flag = false;
		if (BuddyList.pIsReady)
		{
			Buddy buddy = BuddyList.pInstance.GetBuddy(text);
			if (buddy != null && !string.IsNullOrEmpty(buddy.DisplayName))
			{
				flag = true;
				if (mTxtChallenge != null)
				{
					mTxtChallenge.SetText(buddy.DisplayName + " " + mChallengeTxt);
				}
			}
		}
		if (!flag)
		{
			mTxtChallenge.SetText(mNoDisplayNameText.GetLocalizedString() + " " + mChallengeTxt);
			WsWebService.GetDisplayNameByUserID(text, ServiceEventHandler, text);
		}
		KAWidget kAWidget = _Item.FindChildItem("Picture");
		if (kAWidget != null && mPhotoManager != null)
		{
			mPhotoManager.TakePhotoUI(text, (Texture2D)kAWidget.GetTexture(), OnChallengerPhotoLoaded, kAWidget);
		}
	}

	private void OnChallengerPhotoLoaded(Texture tex, object inUserData)
	{
		KAWidget kAWidget = (KAWidget)inUserData;
		if (kAWidget != null)
		{
			kAWidget.SetTexture(tex);
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && !string.IsNullOrEmpty((string)inObject) && mTxtChallenge != null)
		{
			mTxtChallenge.SetText((string)inObject + mChallengeTxt);
		}
	}

	public bool UpdateChallengeTimer()
	{
		TimeSpan timeSpan = _Challenge.ExpirationDate - ServerTime.pCurrentTime;
		_DaysLeft = (int)timeSpan.TotalDays;
		_HoursLeft = (int)timeSpan.TotalHours;
		_MinutesLeft = (int)timeSpan.TotalMinutes;
		_SecondsLeft = (int)timeSpan.TotalSeconds;
		if (_SecondsLeft > 60)
		{
			_MinutesLeft++;
		}
		if (_MinutesLeft > 60)
		{
			_HoursLeft++;
		}
		if (_HoursLeft > 24)
		{
			_DaysLeft++;
		}
		return timeSpan.TotalSeconds > 0.0;
	}

	public void DisplayTime(string remainingTime)
	{
		if (mTxtChallengeTime != null)
		{
			mTxtChallengeTime.SetText(remainingTime);
		}
	}
}
