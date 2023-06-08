using UnityEngine;

public class UserNotifyDailyBonus : UserNotify
{
	public string _DailyBonusURL;

	public int _UnlockMissionID = 1000;

	public AdEventType _AdEventType;

	private bool mUIInitiated;

	private bool mUIStartDone;

	private GameObject mLoadedUI;

	public static bool pDoneOnce;

	public override void OnWaitBeginImpl()
	{
		if (_UnlockMissionID > 0 && MissionManager.pInstance != null)
		{
			Mission mission = MissionManager.pInstance.GetMission(_UnlockMissionID);
			if (mission != null && !mission.pCompleted)
			{
				pDoneOnce = true;
				DailyBonusAndPromo.Init();
			}
		}
		if (string.IsNullOrEmpty(_DailyBonusURL) || pDoneOnce)
		{
			MarkUserNotifyDone();
			return;
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		DailyBonusAndPromo.Init();
	}

	public void Update()
	{
		if (pDoneOnce)
		{
			return;
		}
		if (DailyBonusAndPromo.pIsReady && !mUIInitiated)
		{
			if (!FUEManager.pIsFUERunning && (DailyBonusAndPromo.pInstance.CanAward() || (AdManager.pInstance.AdSupported(_AdEventType, AdType.REWARDED_VIDEO) && AdManager.pInstance.AdAvailable(_AdEventType, AdType.REWARDED_VIDEO, showErrorMessage: false))) && !string.IsNullOrEmpty(_DailyBonusURL))
			{
				string[] array = _DailyBonusURL.Split('/');
				if (array.Length == 3)
				{
					mUIInitiated = true;
					RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnUILoaded, typeof(GameObject));
				}
			}
			else
			{
				MarkUserNotifyDone();
			}
		}
		if (!mUIStartDone && mLoadedUI != null && ServerTime.pIsReady)
		{
			StartUI();
		}
	}

	private void OnUILoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				mLoadedUI = inObject as GameObject;
			}
			else
			{
				MarkUserNotifyDone();
			}
			break;
		case RsResourceLoadEvent.ERROR:
			MarkUserNotifyDone();
			break;
		}
	}

	private bool StartUI()
	{
		if (mLoadedUI == null)
		{
			return false;
		}
		mUIStartDone = true;
		GameObject gameObject = Object.Instantiate(mLoadedUI);
		if (gameObject == null)
		{
			return false;
		}
		UiDailyBonus component = gameObject.GetComponent<UiDailyBonus>();
		if (component == null)
		{
			Object.Destroy(gameObject);
			gameObject = null;
			return false;
		}
		component._MessageObject = base.gameObject;
		if (AdManager.pInstance.AdSupported(_AdEventType, AdType.REWARDED_VIDEO))
		{
			component.SetAdData(_AdEventType);
		}
		return true;
	}

	public void OnDailyBonusUIExit(bool callWaitEnd)
	{
		if (callWaitEnd)
		{
			MarkUserNotifyDone();
		}
		else
		{
			pDoneOnce = true;
		}
	}

	public void OnStoreClosed()
	{
		MarkUserNotifyDone();
	}

	private void MarkUserNotifyDone()
	{
		pDoneOnce = true;
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		OnWaitEnd();
	}
}
