using System;

namespace CI.WSANative.Advertising;

public static class WSANativeInterstitialAd
{
	public static Action<WSAInterstitialAdType, WSAInterstitialAdVariant, string, string> _Request;

	public static Action<WSAInterstitialAdType> _Show;

	private static string _adDuplexAppId;

	private static string _adDuplexAdUnitId;

	private static string _msAppId;

	private static string _msAdUnitId;

	private static string _vungleAppId;

	private static string _vunglePlacementId;

	public static Action<WSAInterstitialAdType> AdReady { get; set; }

	public static Action<WSAInterstitialAdType> Cancelled { get; set; }

	public static Action<WSAInterstitialAdType> Completed { get; set; }

	public static Action<WSAInterstitialAdType, string> ErrorOccurred { get; set; }

	public static void Initialise(WSAInterstitialAdType adType, string appId, string adUnitId)
	{
		switch (adType)
		{
		case WSAInterstitialAdType.AdDuplex:
			_adDuplexAppId = appId;
			_adDuplexAdUnitId = adUnitId;
			break;
		case WSAInterstitialAdType.Microsoft:
			_msAppId = appId;
			_msAdUnitId = adUnitId;
			break;
		case WSAInterstitialAdType.Vungle:
			_vungleAppId = appId;
			_vunglePlacementId = adUnitId;
			break;
		}
	}

	public static void RequestAd(WSAInterstitialAdType adType, WSAInterstitialAdVariant adVariant)
	{
		if (_Request != null)
		{
			switch (adType)
			{
			case WSAInterstitialAdType.AdDuplex:
				_Request(adType, adVariant, _adDuplexAppId, _adDuplexAdUnitId);
				break;
			case WSAInterstitialAdType.Microsoft:
				_Request(adType, adVariant, _msAppId, _msAdUnitId);
				break;
			case WSAInterstitialAdType.Vungle:
				_Request(adType, adVariant, _vungleAppId, _vunglePlacementId);
				break;
			}
		}
	}

	public static void ShowAd(WSAInterstitialAdType adType)
	{
		if (_Show != null)
		{
			_Show(adType);
		}
	}
}
