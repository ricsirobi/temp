using System;

public class UserNotifyDreadfallUI : UserNotify
{
	public static bool mDoneOnce;

	public const string DREADFALL_LAST_PLAYED_KEY = "DLP";

	public const int _PairDataID = 2017;

	public override void OnWaitBeginImpl()
	{
		if (FUEManager.pIsFUERunning || DreadfallAchievementManager.pInstance == null || (!DreadfallAchievementManager.pInstance.EventInProgress() && !DreadfallAchievementManager.pInstance.GracePeriodInProgress()))
		{
			OnWaitEnd();
			return;
		}
		if (mDoneOnce)
		{
			MarkUserNotifyDone();
			return;
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		PairData.Load(2017, OnGamePlayDataReady, null, forceLoad: false, ParentData.pInstance.pUserInfo.UserID);
	}

	private void OnGamePlayDataReady(bool success, PairData inData, object inUserData)
	{
		if (inData == null)
		{
			return;
		}
		int num = 1;
		string value = inData.GetValue("DLP");
		if (!string.IsNullOrEmpty(value) && value != "___VALUE_NOT_FOUND___")
		{
			DateTime result = DateTime.MinValue;
			if (!DateTime.TryParse(value, out result))
			{
				result = UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime);
			}
			num = (UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime).Date - result.Date).Days;
		}
		if (num >= 1)
		{
			inData.SetValue("DLP", UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime).Date.ToString(UtUtilities.GetCultureInfo("en-US")));
			PairData.Save(2017, ParentData.pInstance.pUserInfo.UserID);
			UiDreadfall.Load();
			UiDreadfall.OnExitPressed += OnDreadfallUIClose;
		}
	}

	public void OnDreadfallUIClose()
	{
		UiDreadfall.OnExitPressed -= OnDreadfallUIClose;
		MarkUserNotifyDone();
	}

	private void MarkUserNotifyDone()
	{
		mDoneOnce = true;
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		OnWaitEnd();
	}
}
