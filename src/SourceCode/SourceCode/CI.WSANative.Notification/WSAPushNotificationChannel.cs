using System;

namespace CI.WSANative.Notification;

public class WSAPushNotificationChannel
{
	public Action PushNotificationReceived;

	public string Uri => string.Empty;

	public DateTimeOffset ExpirationTime => DateTimeOffset.MinValue;

	public void Close()
	{
	}
}
