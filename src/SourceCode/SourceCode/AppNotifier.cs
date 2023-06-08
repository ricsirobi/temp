using System;
using UnityEngine;

public class AppNotifier : MonoBehaviour
{
	[Serializable]
	public class Notification
	{
		public LocaleString _Message = new LocaleString("");

		public LocaleString _BtnLabel = new LocaleString("OK");
	}

	[Serializable]
	public class Notification_Metro_Interval
	{
		public string _NotificationID;

		public float _IntervalHours;
	}

	[Serializable]
	public class Notification_Metro
	{
		public LocaleString _Message = new LocaleString("");

		public Notification_Metro_Interval[] _IntervalHours;
	}

	public static Action OnScheduleNotifications;

	private static AppNotifier mNotificationInstance;

	private static bool mEnableCheatForAppIdleNotification;

	private static bool mEnableCheatForPurchaseNotification;

	private static bool mEnableCheatForRegisterDataNotification;

	private const int APPIDLE_NOTIFICATION_TIMER = 900;

	private const int PURCHASE_NOTIFICATION_TIMER = 1200;

	private const int REGISTERDATA_NOTIFICATION_TIMER = 600;

	public Notification _ApplicationNotPlayed;

	public Notification_Metro _ApplicationNotPlayed_Metro;

	public Notification _ItemNotPurchased;

	public Notification_Metro _ItemNotPurchased_Metro;

	public int[] _ApplicationIdleNotificationDays;

	public int _PurchaseNotificationDays;

	public LocaleString _BtnLabel = new LocaleString("OK");

	public LocaleString _NotificationTitle = new LocaleString("School Of Dragons");

	public static AppNotifier pInstance => mNotificationInstance;

	private void Awake()
	{
		if (mNotificationInstance == null)
		{
			mNotificationInstance = this;
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		Init();
		OnApplicationPause(pause: false);
	}

	public void Init()
	{
	}

	private void Notify(Notification notification, int secondsFromNow)
	{
		ScheduleLocalNotification(secondsFromNow, notification._Message.GetLocalizedString(), notification._BtnLabel.GetLocalizedString());
	}

	private void ScheduleLocalNotification(int secondsFromNow, string message, string btnLabel)
	{
	}

	private void Notify(int secondsFromNow, string message, string btnLabel, string notificationID = "")
	{
	}

	private void CancelNotifications(string notificationID = "")
	{
	}

	public void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			if (OnScheduleNotifications != null)
			{
				OnScheduleNotifications();
			}
			NotificationData.SaveData();
			NotifyAppIdle();
			NotifyPurchase();
			NotifyAllRegisteredData();
		}
		else
		{
			NotificationData.LoadData();
			CancelNotifications();
		}
	}

	private void NotifyAppIdle()
	{
		for (int i = 0; i < _ApplicationIdleNotificationDays.GetLength(0); i++)
		{
			if (mEnableCheatForAppIdleNotification)
			{
				Notify(_ApplicationNotPlayed, 900);
				continue;
			}
			TimeSpan timeSpan = new TimeSpan(_ApplicationIdleNotificationDays[i], 0, 0, 0);
			Notify(_ApplicationNotPlayed, Convert.ToInt32(timeSpan.TotalSeconds));
		}
	}

	private void NotifyAllRegisteredData()
	{
		string[,] userData = NotificationData.GetUserData();
		if (userData == null)
		{
			return;
		}
		for (int i = 0; i < userData.GetLength(0); i++)
		{
			if (mEnableCheatForRegisterDataNotification)
			{
				Notify(600, userData[i, NotificationData.idxUserDataName] + " " + userData[i, NotificationData.idxUserDataMessage], _BtnLabel.GetLocalizedString());
				continue;
			}
			int num = Convert.ToInt32(DateTime.Parse(userData[i, NotificationData.idxUserDataDate]).Subtract(DateTime.Now).TotalSeconds);
			if (num > 0)
			{
				Notify(num, userData[i, NotificationData.idxUserDataName] + " " + userData[i, NotificationData.idxUserDataMessage], _BtnLabel.GetLocalizedString());
			}
		}
	}

	private void NotifyPurchase()
	{
		if (mEnableCheatForPurchaseNotification)
		{
			Notify(_ItemNotPurchased, 1200);
			return;
		}
		int lastPurchaseDuration = NotificationData.GetLastPurchaseDuration();
		int num = Convert.ToInt32(new TimeSpan(_PurchaseNotificationDays, 0, 0, 0).TotalSeconds) - lastPurchaseDuration;
		if (num > 0)
		{
			Notify(_ItemNotPurchased, num);
		}
	}

	public void OnApplicationQuit()
	{
	}

	public void EnableCheatForAppIdleNotification(bool enable)
	{
		mEnableCheatForAppIdleNotification = enable;
	}

	public void EnableCheatForPurchaseNotification(bool enable)
	{
		mEnableCheatForPurchaseNotification = enable;
	}

	public void EnableCheatForRegisterDataNotification(bool enable)
	{
		mEnableCheatForRegisterDataNotification = enable;
	}

	public void EnableCheatForAllNotifications(bool enable)
	{
		EnableCheatForAppIdleNotification(enable);
		EnableCheatForPurchaseNotification(enable);
		EnableCheatForRegisterDataNotification(enable);
	}
}
