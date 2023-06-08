using System;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;

namespace tv.superawesome.sdk.publisher;

public class SAVideoAd : MonoBehaviour
{
	private static SAVideoAd staticInstance = null;

	private static Action<int, SAEvent> callback = delegate
	{
	};

	private static bool isParentalGateEnabled = SADefines.defaultParentalGate();

	private static bool isBumperPageEnabled = SADefines.defaultBumperPage();

	private static bool shouldShowCloseButton = SADefines.defaultCloseButton();

	private static bool shouldShowCloseWarning = SADefines.defaultCloseWarning();

	private static bool shouldShowSmallClickButton = SADefines.defaultSmallClick();

	private static bool shouldAutomaticallyCloseAtEnd = SADefines.defaultCloseAtEnd();

	private static bool isTestingEnabled = SADefines.defaultTestMode();

	private static bool isBackButtonEnabled = SADefines.defaultBackButton();

	private static SAOrientation orientation = SADefines.defaultOrientation();

	private static SAConfiguration configuration = SADefines.defaultConfiguration();

	private static void createInstance()
	{
		if (staticInstance == null)
		{
			staticInstance = new GameObject().AddComponent<SAVideoAd>();
			staticInstance.name = "SAVideoAd";
			UnityEngine.Object.DontDestroyOnLoad(staticInstance);
			SAVersion.setVersionInNative();
			Debug.Log("SAVideoAd Create");
		}
	}

	public static void load(int placementId)
	{
		createInstance();
		Debug.Log("SAVideoAd Load");
	}

	public static void play(int placementId)
	{
		createInstance();
		Debug.Log("SAVideoAd Play");
	}

	public static void applySettings()
	{
		createInstance();
		Debug.Log("SAVideoAd applySettings");
	}

	public static bool hasAdAvailable(int placementId)
	{
		createInstance();
		Debug.Log("SAVideoAd hasAdAvailable has not implemented");
		return false;
	}

	public static void setCallback(Action<int, SAEvent> value)
	{
		callback = ((value != null) ? value : callback);
	}

	public static void setIsParentalGateEnabled(bool value)
	{
		isParentalGateEnabled = value;
	}

	public static void enableParentalGate()
	{
		isParentalGateEnabled = true;
	}

	public static void disableParentalGate()
	{
		isParentalGateEnabled = false;
	}

	public static void enableBumperPage()
	{
		isBumperPageEnabled = true;
	}

	public static void disableBumperPage()
	{
		isBumperPageEnabled = false;
	}

	public static void enableTestMode()
	{
		isTestingEnabled = true;
	}

	public static void disableTestMode()
	{
		isTestingEnabled = false;
	}

	public static void setConfigurationProduction()
	{
		configuration = SAConfiguration.PRODUCTION;
	}

	public static void setConfigurationStaging()
	{
		configuration = SAConfiguration.STAGING;
	}

	public static void setOrientationAny()
	{
		orientation = SAOrientation.ANY;
	}

	public static void setOrientationPortrait()
	{
		orientation = SAOrientation.PORTRAIT;
	}

	public static void setOrientationLandscape()
	{
		orientation = SAOrientation.LANDSCAPE;
	}

	public static void enableCloseButton()
	{
		shouldShowCloseButton = true;
	}

	public static void disableCloseButton()
	{
		shouldShowCloseButton = false;
	}

	public static void enableSmallClickButton()
	{
		shouldShowSmallClickButton = true;
	}

	public static void disableSmallClickButton()
	{
		shouldShowSmallClickButton = false;
	}

	public static void enableCloseAtEnd()
	{
		shouldAutomaticallyCloseAtEnd = true;
	}

	public static void disableCloseAtEnd()
	{
		shouldAutomaticallyCloseAtEnd = false;
	}

	public static void enableBackButton()
	{
		isBackButtonEnabled = true;
	}

	public static void enableCloseButtonWithWarning()
	{
		shouldShowCloseButton = true;
		shouldShowCloseWarning = true;
	}

	public static void disableBackButton()
	{
		isBackButtonEnabled = false;
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
