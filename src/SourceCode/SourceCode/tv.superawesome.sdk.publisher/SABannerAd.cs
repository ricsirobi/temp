using System;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;

namespace tv.superawesome.sdk.publisher;

public class SABannerAd : MonoBehaviour
{
	private static uint index;

	private Action<int, SAEvent> callback = delegate
	{
	};

	private bool isParentalGateEnabled = SADefines.defaultParentalGate();

	private bool isBumperPageEnabled = SADefines.defaultBumperPage();

	private SABannerPosition position = SADefines.defaultBannerPosition();

	private int bannerWidth = SADefines.defaultBannerWidth();

	private int bannerHeight = SADefines.defaultBannerHeight();

	private bool color = SADefines.defaultBgColor();

	private SAConfiguration configuration = SADefines.defaultConfiguration();

	private bool isTestingEnabled = SADefines.defaultTestMode();

	public static SABannerAd createInstance()
	{
		GameObject obj = new GameObject();
		SABannerAd sABannerAd = obj.AddComponent<SABannerAd>();
		uint num = ++index;
		sABannerAd.name = "SABannerAd_" + num;
		UnityEngine.Object.DontDestroyOnLoad(obj);
		SAVersion.setVersionInNative();
		Debug.Log(sABannerAd.name + " Create");
		return sABannerAd;
	}

	public void load(int placementId)
	{
		Debug.Log(base.name + " Load");
	}

	public void play()
	{
		Debug.Log(base.name + " Play has not implemented");
	}

	public bool hasAdAvailable()
	{
		Debug.Log(base.name + " HasAdAvailable has not implemented");
		return false;
	}

	public void close()
	{
		Debug.Log(base.name + " Close");
	}

	public void setCallback(Action<int, SAEvent> value)
	{
		callback = ((value != null) ? value : callback);
	}

	public void enableParentalGate()
	{
		isParentalGateEnabled = true;
	}

	public void disableParentalGate()
	{
		isParentalGateEnabled = false;
	}

	public void enableBumperPage()
	{
		isBumperPageEnabled = true;
	}

	public void disableBumperPage()
	{
		isBumperPageEnabled = false;
	}

	public void enableTestMode()
	{
		isTestingEnabled = true;
	}

	public void disableTestMode()
	{
		isTestingEnabled = false;
	}

	public void setConfigurationProduction()
	{
		configuration = SAConfiguration.PRODUCTION;
	}

	public void setConfigurationStaging()
	{
		configuration = SAConfiguration.STAGING;
	}

	public void setPositionTop()
	{
		position = SABannerPosition.TOP;
	}

	public void setPositionBottom()
	{
		position = SABannerPosition.BOTTOM;
	}

	public void setSize_300_50()
	{
		bannerWidth = 300;
		bannerHeight = 50;
	}

	public void setSize_320_50()
	{
		bannerWidth = 320;
		bannerHeight = 50;
	}

	public void setSize_728_90()
	{
		bannerWidth = 728;
		bannerHeight = 90;
	}

	public void setSize_300_250()
	{
		bannerWidth = 300;
		bannerHeight = 250;
	}

	public void setColorGray()
	{
		color = false;
	}

	public void setColorTransparent()
	{
		color = true;
	}

	public void nativeCallback(string payload)
	{
		string text = "";
		int result = 0;
		try
		{
			Dictionary<string, object> obj = Json.Deserialize(payload) as Dictionary<string, object>;
			text = (string)obj["type"];
			int.TryParse((string)obj["placementId"], out result);
		}
		catch
		{
			Debug.Log("Error w/ callback!");
			return;
		}
		switch (text)
		{
		case "sacallback_adLoaded":
			callback(result, SAEvent.adLoaded);
			break;
		case "sacallback_adEmpty":
			callback(result, SAEvent.adEmpty);
			break;
		case "sacallback_adFailedToLoad":
			callback(result, SAEvent.adFailedToLoad);
			break;
		case "sacallback_adAlreadyLoaded":
			callback(result, SAEvent.adAlreadyLoaded);
			break;
		case "sacallback_adShown":
			callback(result, SAEvent.adShown);
			break;
		case "sacallback_adFailedToShow":
			callback(result, SAEvent.adFailedToShow);
			break;
		case "sacallback_adClicked":
			callback(result, SAEvent.adClicked);
			break;
		case "sacallback_adEnded":
			callback(result, SAEvent.adEnded);
			break;
		case "sacallback_adClosed":
			callback(result, SAEvent.adClosed);
			break;
		}
	}
}
