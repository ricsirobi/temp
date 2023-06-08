using System;

namespace CI.WSANative.Notification;

public static class WSANativeNotification
{
	public static void ShowToastNotification(string title, string text, string tag, Uri image = null)
	{
	}

	public static void ShowScheduledToastNotification(string title, string text, DateTime deliveryTime, string tag, Uri image = null)
	{
	}

	public static void RemoveToastNotification(string tag)
	{
	}

	public static void CreatePushNotificationChannel(Action<WSAPushNotificationChannel> response)
	{
	}

	public static void SendPushNotificationUriToServer(string serverUrl, string channelUri, string authorisation, Action<bool, string> response)
	{
	}
}
