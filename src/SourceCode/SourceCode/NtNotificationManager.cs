using System;
using UnityEngine;

public class NtNotificationManager : SingletonBehaviour<NtNotificationManager>
{
	private bool mAppInstallRegistrationComplete;

	private int mBadgeCount;

	private NtNotificationEventHandler mNotificationReceivedHandler;

	private NtNotificationErrorHandler mNotificationErrorHandler;

	private string mStatus = "No data available.";

	public static string pInstallID => PlayerPrefs.GetString("InstallID", Guid.NewGuid().ToString());

	public static bool pAppInstallRegistrationComplete
	{
		get
		{
			if (!(SingletonBehaviour<NtNotificationManager>.pInstance == null))
			{
				return SingletonBehaviour<NtNotificationManager>.pInstance.mAppInstallRegistrationComplete;
			}
			return false;
		}
	}

	public static int pBadgeCount
	{
		get
		{
			if (!(SingletonBehaviour<NtNotificationManager>.pInstance == null))
			{
				return SingletonBehaviour<NtNotificationManager>.pInstance.mBadgeCount;
			}
			return 0;
		}
	}

	public static NtNotificationEventHandler pNotificationReceievedHandler
	{
		get
		{
			return SingletonBehaviour<NtNotificationManager>.pInstance.mNotificationReceivedHandler;
		}
		set
		{
			SingletonBehaviour<NtNotificationManager>.pInstance.mNotificationReceivedHandler = value;
		}
	}

	public static NtNotificationErrorHandler pNotificationErrorHandler
	{
		get
		{
			return SingletonBehaviour<NtNotificationManager>.pInstance.mNotificationErrorHandler;
		}
		set
		{
			SingletonBehaviour<NtNotificationManager>.pInstance.mNotificationErrorHandler = value;
		}
	}

	private void Awake()
	{
	}

	private void GUIDebug()
	{
		GUI.skin = null;
		GUILayout.Label("Notification status: " + mStatus);
		GUILayout.Label("Badge count: " + mBadgeCount);
	}
}
