using System;
using UnityEngine;

public class UserNotifySnoggletogUI : UserNotify
{
	[Header("Job Board")]
	public string _AssetName;

	public const string SNOGGLETOG_LAST_PLAYED_KEY = "SLP";

	public const int _PairDataID = 2017;

	public override void OnWaitBeginImpl()
	{
		if (FUEManager.pIsFUERunning || SnoggletogManager.pInstance == null || (!SnoggletogManager.pInstance.IsEventInProgress() && !SnoggletogManager.pInstance.IsGracePeriodInProgress()))
		{
			OnWaitEnd();
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
		string value = inData.GetValue("SLP");
		if (!string.IsNullOrEmpty(value) && value != "___VALUE_NOT_FOUND___")
		{
			DateTime result = DateTime.MinValue;
			if (!DateTime.TryParse(value, out result))
			{
				result = UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime);
			}
			num = Math.Abs((UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime).Date - result.Date).Days);
		}
		if (SnoggletogManager.pInstance.CanForceOpenMysteryBox() || num >= 1)
		{
			inData.SetValue("SLP", UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime).Date.ToString(UtUtilities.GetCultureInfo("en-US")));
			PairData.Save(2017, ParentData.pInstance.pUserInfo.UserID);
			LoadJobBoard();
		}
		else
		{
			MarkUserNotifyDone();
		}
	}

	private void LoadJobBoard()
	{
		if (!string.IsNullOrEmpty(_AssetName))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = _AssetName.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], LoadObjectEvent, typeof(GameObject));
		}
	}

	private void LoadObjectEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			UnityEngine.Object.Instantiate((GameObject)inObject);
			UiSnoggletogBoard.OnExitPressed += OnSnoggletogUIClose;
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			break;
		}
	}

	public void OnSnoggletogUIClose()
	{
		UiSnoggletogBoard.OnExitPressed -= OnSnoggletogUIClose;
		MarkUserNotifyDone();
	}

	private void MarkUserNotifyDone()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		OnWaitEnd();
	}
}
