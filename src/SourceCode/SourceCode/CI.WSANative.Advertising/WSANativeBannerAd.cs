using System;

namespace CI.WSANative.Advertising;

public static class WSANativeBannerAd
{
	public static Action<WSABannerAdSettings> Create;

	public static Action<WSABannerAdType, bool> SetVisiblity;

	public static Action<WSABannerAdSettings> Reconfigure;

	public static Action<WSABannerAdType> Destroy;

	private static string _adDuplexAppId;

	private static string _adDuplexAdUnitId;

	private static string _msAppId;

	private static string _msAdUnitId;

	public static Action<WSABannerAdType> AdRefreshed { get; set; }

	public static Action<WSABannerAdType, string> ErrorOccurred { get; set; }

	public static void Initialise(WSABannerAdType adType, string appId, string adUnitId)
	{
		switch (adType)
		{
		case WSABannerAdType.AdDuplex:
			_adDuplexAppId = appId;
			_adDuplexAdUnitId = adUnitId;
			break;
		case WSABannerAdType.Microsoft:
			_msAppId = appId;
			_msAdUnitId = adUnitId;
			break;
		}
	}

	public static void CreatAd(WSABannerAdType adType, int width, int height, WSAAdVerticalPlacement verticalPlacement, WSAAdHorizontalPlacement horizontalPlacement)
	{
	}

	public static void SetAdVisibility(WSABannerAdType adType, bool visible)
	{
	}

	public static void ReconfigureAd(WSABannerAdType adType, int width, int height, WSAAdVerticalPlacement verticalPlacement, WSAAdHorizontalPlacement horizontalPlacement)
	{
	}

	public static void DestroyAd(WSABannerAdType adType)
	{
	}
}
