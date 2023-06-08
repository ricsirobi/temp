using System;

public class UiHUDBattleEventTimer : KAUI
{
	private KAWidget mTimer;

	protected override void Start()
	{
		base.Start();
		mTimer = FindItem("TxtTimer");
		SetVisibility(inVisible: false);
	}

	protected override void Update()
	{
		base.Update();
		UpdateTimer();
	}

	private void UpdateTimer()
	{
		if (mTimer != null && WorldEventNotification.pInstance != null && !FUEManager.pIsFUERunning && MMOTimeManager.pInstance != null && MMOTimeManager.pInstance.pIsTimeSynced)
		{
			DateTime serverDateTimeMilliseconds = MMOTimeManager.pInstance.GetServerDateTimeMilliseconds();
			if (WorldEventNotification.pInstance.TimeToNextEvent > serverDateTimeMilliseconds && (serverDateTimeMilliseconds < WorldEventNotification.pInstance.TimeToStart || serverDateTimeMilliseconds > WorldEventNotification.pInstance.EventEndTime))
			{
				if (!GetVisibility())
				{
					SetVisibility(inVisible: true);
				}
				DateTime dateTime = ((serverDateTimeMilliseconds < WorldEventNotification.pInstance.TimeToStart) ? WorldEventNotification.pInstance.TimeToStart : WorldEventNotification.pInstance.TimeToNextEvent);
				mTimer.SetText(UtUtilities.GetFormattedTime(dateTime - serverDateTimeMilliseconds, "D ", "H ", "M ", "S"));
				return;
			}
		}
		if (GetVisibility())
		{
			SetVisibility(inVisible: false);
		}
	}
}
