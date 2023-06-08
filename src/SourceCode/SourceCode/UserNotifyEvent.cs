using System;
using SOD.Event;
using UnityEngine;

public class UserNotifyEvent : UserNotify
{
	public int _PairDataID = 2017;

	private EventManager mEventManager;

	private string mEventName = string.Empty;

	private string LastPlayedKey => mEventName + "LP" + mEventManager?.pStartDate.Year.ToString();

	private string IntroPlayedKey => mEventName + "Intro" + mEventManager?.pStartDate.Year.ToString();

	public override void OnWaitBeginImpl()
	{
		mEventManager = EventManager.GetActiveEvent();
		if ((bool)mEventManager)
		{
			mEventName = mEventManager._EventName;
		}
		if (FUEManager.pIsFUERunning || mEventManager == null || (!mEventManager.EventInProgress() && !mEventManager.GracePeriodInProgress()))
		{
			OnWaitEnd();
			return;
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		PairData.Load(_PairDataID, OnGamePlayDataReady, null);
	}

	private void OnGamePlayDataReady(bool success, PairData inData, object inUserData)
	{
		if (inData != null)
		{
			if (!inData.GetBoolValue(IntroPlayedKey, defaultVal: false))
			{
				if (mEventManager.GracePeriodInProgress())
				{
					MarkUserNotifyDone();
					return;
				}
				inData.SetValue(LastPlayedKey, UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime).Date.ToString(UtUtilities.GetCultureInfo("en-US")));
				inData.SetValue(IntroPlayedKey, true.ToString());
				PairData.Save(_PairDataID);
				AvAvatar.SetUIActive(inActive: false);
				KAUICursorManager.SetDefaultCursor("Loading");
				RsResourceManager.LoadAssetFromBundle(mEventManager._IntroAssetName, OnBundleLoaded, typeof(GameObject));
				return;
			}
			int daysCount = 1;
			string value = inData.GetValue(LastPlayedKey);
			if (!string.IsNullOrEmpty(value) && value != "___VALUE_NOT_FOUND___")
			{
				DateTime result = DateTime.MinValue;
				if (!DateTime.TryParse(value, out result))
				{
					result = UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime);
				}
				daysCount = Math.Abs((UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime).Date - result.Date).Days);
			}
			CheckForNotify(daysCount, inData);
		}
		else
		{
			MarkUserNotifyDone();
		}
	}

	public void CheckForNotify(int daysCount, PairData inData)
	{
		if (daysCount >= 1 || (mEventManager._Type == SOD.Event.Type.MISSION && mEventManager.CanForceOpenMysteryBox()))
		{
			inData.SetValue(LastPlayedKey, UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime).Date.ToString(UtUtilities.GetCultureInfo("en-US")));
			PairData.Save(_PairDataID);
			if (mEventManager._Type == SOD.Event.Type.ACHIEVEMENT_TASK)
			{
				UiPrizeProgression.Load(mEventName, OnPrizeProgressionLoaded);
			}
			else if (mEventManager._Type == SOD.Event.Type.MISSION)
			{
				UiEventBoard.Load(mEventName, OnEventBoardLoaded);
			}
		}
		else
		{
			MarkUserNotifyDone();
		}
	}

	public void OnPrizeProgressionLoaded(UiPrizeProgression inUiPrizeProgression)
	{
		if (inUiPrizeProgression != null)
		{
			inUiPrizeProgression.OnClosed = (Action)Delegate.Combine(inUiPrizeProgression.OnClosed, new Action(MarkUserNotifyDone));
		}
	}

	public void OnEventBoardLoaded(UiEventBoard inUiEventBoard)
	{
		if (inUiEventBoard != null)
		{
			inUiEventBoard.OnClosed = (Action)Delegate.Combine(inUiEventBoard.OnClosed, new Action(MarkUserNotifyDone));
		}
	}

	public void OnEventIntroScreenLoaded(UiEventIntro inUiEventIntro)
	{
		if (inUiEventIntro != null)
		{
			inUiEventIntro.m_UserNotifyEvent = this;
			inUiEventIntro.OnClosed = (Action)Delegate.Combine(inUiEventIntro.OnClosed, new Action(MarkUserNotifyDone));
		}
	}

	private void OnBundleLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			gameObject.name = ((GameObject)inObject).name;
			OnEventIntroScreenLoaded(gameObject.GetComponent<UiEventIntro>());
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			Debug.LogError("Error loading Event Intro Prefab!" + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	public void MarkUserNotifyDone()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		try
		{
			PairDataInstance pairDataInstanceByID = PairData.GetPairDataInstanceByID(_PairDataID);
			if (pairDataInstanceByID != null && pairDataInstanceByID.mLoadCallback != null)
			{
				pairDataInstanceByID.mLoadCallback = null;
			}
		}
		catch (Exception ex)
		{
			UtDebug.LogError("Exception caught: " + ex);
		}
		OnWaitEnd();
	}
}
