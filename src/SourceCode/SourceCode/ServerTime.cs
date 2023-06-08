using System;
using UnityEngine;

public class ServerTime
{
	public static Action<WsServiceEvent> OnServerTimeReady;

	private static DateTime? mServerTime;

	private static float mLocalTime;

	private static double mOffsetSeconds;

	public static bool pIsReady => mServerTime.HasValue;

	public static DateTime? pServerTime => mServerTime;

	public static DateTime pCurrentTime
	{
		get
		{
			if (!mServerTime.HasValue)
			{
				return DateTime.UtcNow.AddSeconds((double)(Time.realtimeSinceStartup - mLocalTime) + mOffsetSeconds);
			}
			return mServerTime.Value.AddSeconds((double)(Time.realtimeSinceStartup - mLocalTime) + mOffsetSeconds);
		}
	}

	public static void Init(bool inForceInit = false)
	{
		if (!pIsReady || inForceInit)
		{
			WsWebService.GetAuthoritativeTime(AuthoritativeTimeEventHandler, null);
		}
	}

	private static void AuthoritativeTimeEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			mServerTime = (DateTime)inObject;
			mLocalTime = Time.realtimeSinceStartup;
			if (OnServerTimeReady != null)
			{
				OnServerTimeReady(inEvent);
			}
			break;
		case WsServiceEvent.ERROR:
			if (!UtUtilities.IsConnectedToWWW())
			{
				mServerTime = DateTime.UtcNow;
				mLocalTime = Time.realtimeSinceStartup;
			}
			if (OnServerTimeReady != null)
			{
				OnServerTimeReady(inEvent);
			}
			Debug.LogError("GetAuthoritaveTime Failed!");
			break;
		}
	}

	public static void ResetOffsetTime()
	{
		mOffsetSeconds = 0.0;
	}

	public static void AddOffsetTime(double seconds)
	{
		mOffsetSeconds += seconds;
	}

	public static void AddOffsetTime(string duration)
	{
		mOffsetSeconds += pCurrentTime.GetOffsetTime(duration);
	}
}
