using System;

namespace CI.WSANative.Advertising;

public static class WSANativeNativeAd
{
	public static Action<string, string> _Request;

	public static Action<int, int, int, int> _Position;

	public static Action _Destroy;

	private static string _appId = string.Empty;

	private static string _adUnitId = string.Empty;

	public static Action<WSANativeAd> AdReady { get; set; }

	public static Action<string> ErrorOccurred { get; set; }

	public static void Initialise(string appId, string adUnitId)
	{
		_appId = appId;
		_adUnitId = adUnitId;
	}

	public static void RequestAd()
	{
	}

	public static void SetPosition(int x, int y, int width, int height)
	{
	}

	public static void DestroyAd()
	{
	}
}
