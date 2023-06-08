using System;
using System.Collections;
using UnityEngine;

public class UniRate : MonoBehaviour
{
	public delegate void OnUniRateFaildDelegate(Error error);

	public delegate void OnDetectAppUpdatedDelegate();

	public delegate bool ShouldUniRatePromptForRatingDelegate();

	public delegate void OnPromptedForRatingDelegate();

	public delegate void OnUserAttemptToRateDelegate();

	public delegate void OnUserDeclinedToRateDelegate();

	public delegate void OnUserWantReminderToRateDelegate();

	public delegate bool ShouldUniRateOpenRatePageDelegate();

	public enum Error
	{
		BundleIdDoesNotMatchAppStore,
		AppNotFoundOnAppStore,
		NotTheLatestVersion,
		NetworkError
	}

	public int appStoreID;

	public string marketPackageName;

	public UniRateMarketType marketType;

	public string appStoreCountry;

	public string applicationName;

	public string applicationVersion;

	public string applicationBundleID;

	public int usesUntilPrompt = 10;

	public int eventsUntilPrompt;

	public float daysUntilPrompt = 3f;

	public float usesPerWeekForPrompt;

	public float remindPeriod = 1f;

	public LocaleString messageTitleText = new LocaleString("");

	public LocaleString messageText = new LocaleString("");

	public LocaleString cancelButtonLabelText = new LocaleString("");

	public LocaleString remindButtonLabelText = new LocaleString("");

	public LocaleString rateButtonLabelText = new LocaleString("");

	public bool onlyPromptIfLatestVersion = true;

	public bool promptAgainForEachNewVersion = true;

	public bool promptAtLaunch = true;

	public bool useCustomizedPromptView;

	public bool previewMode;

	[SerializeField]
	private string _ratingIOSURL;

	[SerializeField]
	private string _ratingAndroidURL;

	private const string kUniRateRatedVersionKey = "UniRateRatedVersionChecked";

	private const string kUniRateDeclinedVersionKey = "UniRateDeclinedVersion";

	private const string kUniRateLastRemindedKey = "UniRateLastReminded";

	private const string kUniRateLastVersionUsedKey = "UniRateLastVersionUsed";

	private const string kUniRateFirstUsedKey = "UniRateFirstUsed";

	private const string kUniRateUseCountKey = "UniRateUseCount";

	private const string kUniRateEventCountKey = "UniRateEventCount";

	private const string kUniRateAppLookupURLFormat = "http://itunes.apple.com/{0}lookup";

	private const string kUniRateiOSAppStoreURLFormat = "itms-apps://itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?type=Purple+Software&id=";

	private const string kUniRateiOS7AppStoreURLFormat = "itms-apps://itunes.apple.com/app/id";

	private const string kUniRateAndroidMarketURLFormat = "market://details?id=";

	private const string kUniRateAmazonAppstoreURLFormat = "amzn://apps/android?p=";

	private const string kDefaultTitle = "Rate {0}";

	private const string kDefaultMessage = "If you enjoy {0}, would you mind taking a moment to rate it? It will not take more than a minute. Thanks for your support!";

	private const string kDefaultRateBtnTitle = "Rate It Now";

	private const string kDefaultCancelBtnTitle = "No, Thanks";

	private const string kDefaultRemindBtnTitle = "Remind Me Later";

	private const float SECONDS_IN_A_DAY = 86400f;

	private const float SECONDS_IN_A_WEEK = 604800f;

	private bool _currentChecking;

	private static UniRate _instance;

	public string MessageTitle
	{
		get
		{
			if (!string.IsNullOrEmpty(messageTitleText._Text))
			{
				return messageTitleText.GetLocalizedString();
			}
			return $"Rate {applicationName}";
		}
	}

	public string Message
	{
		get
		{
			if (!string.IsNullOrEmpty(messageText._Text))
			{
				return messageText.GetLocalizedString();
			}
			return $"If you enjoy {applicationName}, would you mind taking a moment to rate it? It will not take more than a minute. Thanks for your support!";
		}
	}

	public string CancelButtonLabel
	{
		get
		{
			if (!string.IsNullOrEmpty(cancelButtonLabelText._Text))
			{
				return cancelButtonLabelText.GetLocalizedString();
			}
			return "No, Thanks";
		}
	}

	public string RemindButtonLabel
	{
		get
		{
			if (!string.IsNullOrEmpty(remindButtonLabelText._Text))
			{
				return remindButtonLabelText.GetLocalizedString();
			}
			return "Remind Me Later";
		}
	}

	public string RateButtonLabel
	{
		get
		{
			if (!string.IsNullOrEmpty(rateButtonLabelText._Text))
			{
				return rateButtonLabelText.GetLocalizedString();
			}
			return "Rate It Now";
		}
	}

	public string RatingIOSURL
	{
		get
		{
			if (!string.IsNullOrEmpty(_ratingIOSURL))
			{
				return _ratingIOSURL;
			}
			if (appStoreID == 0)
			{
				Debug.LogWarning("UniRate does not find your App Store ID");
			}
			if (!(iOSVersion >= 7f))
			{
				return "itms-apps://itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?type=Purple+Software&id=" + appStoreID;
			}
			return "itms-apps://itunes.apple.com/app/id" + appStoreID;
		}
	}

	public string RatingAndroidURL
	{
		get
		{
			if (!string.IsNullOrEmpty(_ratingAndroidURL))
			{
				return _ratingAndroidURL;
			}
			string text = "";
			switch (marketType)
			{
			case UniRateMarketType.GooglePlay:
				text = "market://details?id=";
				break;
			case UniRateMarketType.AmazonAppstore:
				text = "amzn://apps/android?p=";
				break;
			}
			return text + marketPackageName;
		}
	}

	public DateTime firstUsed
	{
		get
		{
			return UniRatePlayerPrefs.GetDate("UniRateFirstUsed");
		}
		set
		{
			UniRatePlayerPrefs.SetDate("UniRateFirstUsed", value);
			PlayerPrefs.Save();
		}
	}

	public DateTime lastReminded
	{
		get
		{
			return UniRatePlayerPrefs.GetDate("UniRateLastReminded");
		}
		set
		{
			UniRatePlayerPrefs.SetDate("UniRateLastReminded", value);
			PlayerPrefs.Save();
		}
	}

	public int usesCount
	{
		get
		{
			return PlayerPrefs.GetInt("UniRateUseCount");
		}
		set
		{
			PlayerPrefs.SetInt("UniRateUseCount", value);
			PlayerPrefs.Save();
		}
	}

	public int eventCount
	{
		get
		{
			return PlayerPrefs.GetInt("UniRateEventCount");
		}
		set
		{
			PlayerPrefs.SetInt("UniRateEventCount", value);
			PlayerPrefs.Save();
		}
	}

	public float usesPerWeek => (float)((double)usesCount / ((DateTime.Now - firstUsed).TotalSeconds / 604800.0));

	public bool declinedThisVersion
	{
		get
		{
			if (!string.IsNullOrEmpty(applicationVersion))
			{
				return string.Equals(PlayerPrefs.GetString("UniRateDeclinedVersion"), applicationVersion);
			}
			return false;
		}
		set
		{
			PlayerPrefs.SetString("UniRateDeclinedVersion", value ? applicationVersion : "");
			PlayerPrefs.Save();
		}
	}

	public bool declinedAnyVersion => !string.IsNullOrEmpty(PlayerPrefs.GetString("UniRateDeclinedVersion"));

	public bool ratedThisVersion
	{
		get
		{
			return PlayerPrefs.GetInt("UniRateRatedVersionChecked") == 1;
		}
		set
		{
			PlayerPrefs.SetInt("UniRateRatedVersionChecked", value ? 1 : 0);
			PlayerPrefs.Save();
		}
	}

	public bool ratedAnyVersion => !string.IsNullOrEmpty(PlayerPrefs.GetString("UniRateRatedVersionChecked"));

	public float usedDays => (float)(DateTime.Now - firstUsed).TotalSeconds / 86400f;

	public bool waitingByRemindLater => lastReminded != DateTime.MaxValue;

	public float leftRemindDays => (float)((double)remindPeriod - (DateTime.Now - lastReminded).TotalSeconds / 86400.0);

	public static UniRate Instance
	{
		get
		{
			if (!_instance)
			{
				_instance = UnityEngine.Object.FindObjectOfType(typeof(UniRate)) as UniRate;
				if (!_instance)
				{
					_instance = new GameObject("UniRateManager").AddComponent<UniRate>();
				}
				else
				{
					_instance.gameObject.name = "UniRateManager";
				}
				UnityEngine.Object.DontDestroyOnLoad(_instance.gameObject);
				UniRatePlugin.InitUniRateAndroid();
			}
			return _instance;
		}
	}

	private float iOSVersion
	{
		get
		{
			float result = -1f;
			float.TryParse(SystemInfo.operatingSystem.Replace("iPhone OS ", "").Substring(0, 1), out result);
			return result;
		}
	}

	public event OnUniRateFaildDelegate OnUniRateFaild;

	public event OnDetectAppUpdatedDelegate OnDetectAppUpdated;

	public event ShouldUniRatePromptForRatingDelegate ShouldUniRatePromptForRating;

	public event OnPromptedForRatingDelegate OnPromptedForRating;

	public event OnUserAttemptToRateDelegate OnUserAttemptToRate;

	public event OnUserDeclinedToRateDelegate OnUserDeclinedToRate;

	public event OnUserWantReminderToRateDelegate OnUserWantReminderToRate;

	public event ShouldUniRateOpenRatePageDelegate ShouldUniRateOpenRatePage;

	public bool ShouldPromptForRating()
	{
		if (previewMode)
		{
			Debug.Log("UniRate is in preview mode. Make sure you set previewMode to false when release.");
			return true;
		}
		if (ratedThisVersion)
		{
			UniRateDebug.Log("Not prompt. The user has already rated this version");
			return false;
		}
		if (!promptAgainForEachNewVersion && ratedAnyVersion)
		{
			UniRateDebug.Log("Not prompt. The user has already rated for some version and promptAgainForEachNewVersion is disabled");
			return false;
		}
		if (declinedThisVersion)
		{
			UniRateDebug.Log("Not prompt. The user refused to rate this version");
			return false;
		}
		if ((daysUntilPrompt > 0f || usesPerWeekForPrompt != 0f) && firstUsed == DateTime.MaxValue)
		{
			UniRateDebug.Log("Not prompt. First launch");
			return false;
		}
		if ((DateTime.Now - firstUsed).TotalSeconds < (double)(daysUntilPrompt * 86400f))
		{
			UniRateDebug.Log("Not prompt. App was used less than " + daysUntilPrompt + " days ago");
			return false;
		}
		if (usesCount < usesUntilPrompt)
		{
			UniRateDebug.Log("Not prompt. App was only used " + usesCount + " times");
			return false;
		}
		if (eventCount < eventsUntilPrompt)
		{
			UniRateDebug.Log("Not prompt. Only " + eventCount + " times of events logged");
			return false;
		}
		if (usesPerWeek < usesPerWeekForPrompt)
		{
			UniRateDebug.Log("Not prompt. Only used " + usesPerWeek + " times per week");
			return false;
		}
		if (lastReminded != DateTime.MaxValue && (DateTime.Now - lastReminded).TotalSeconds < (double)(remindPeriod * 86400f))
		{
			UniRateDebug.Log("Not prompt. The user askd to be reminded and it is not the time now");
			return false;
		}
		return true;
	}

	public void PromptIfNetworkAvailable()
	{
		if (!_currentChecking)
		{
			_currentChecking = true;
		}
	}

	public void ShowPrompt()
	{
		UniRateDebug.Log("It's time to show prompt");
		if (this.OnPromptedForRating != null)
		{
			this.OnPromptedForRating();
		}
		if (!useCustomizedPromptView)
		{
			UniRatePlugin.ShowPrompt(MessageTitle, Message, RateButtonLabel, CancelButtonLabel, RemindButtonLabel);
		}
	}

	public void LogEvent(bool withPrompt)
	{
		IncreaseEventCount();
		if (withPrompt && ShouldPromptForRating())
		{
			PromptIfNetworkAvailable();
		}
	}

	public void Reset()
	{
		PlayerPrefs.DeleteKey("UniRateRatedVersionChecked");
		PlayerPrefs.DeleteKey("UniRateDeclinedVersion");
		PlayerPrefs.DeleteKey("UniRateLastReminded");
		PlayerPrefs.DeleteKey("UniRateLastVersionUsed");
		PlayerPrefs.DeleteKey("UniRateFirstUsed");
		PlayerPrefs.DeleteKey("UniRateUseCount");
		PlayerPrefs.DeleteKey("UniRateEventCount");
		PlayerPrefs.Save();
		Instance.Init();
	}

	private void Start()
	{
		Instance.Init();
	}

	private void Init()
	{
		if (string.IsNullOrEmpty(appStoreCountry))
		{
			appStoreCountry = UniRatePlugin.GetAppStoreCountry();
			UniRateDebug.Log("Get Country Code: " + appStoreCountry);
		}
		if (string.IsNullOrEmpty(applicationVersion))
		{
			applicationVersion = UniRatePlugin.GetApplicationVersion();
			UniRateDebug.Log("Get App Version: " + applicationVersion);
		}
		if (string.IsNullOrEmpty(applicationName))
		{
			applicationName = UniRatePlugin.GetApplicationName();
			UniRateDebug.Log("Get App Name: " + applicationName);
		}
		if (string.IsNullOrEmpty(applicationBundleID))
		{
			applicationBundleID = UniRatePlugin.GetApplicationBundleID();
			UniRateDebug.Log("Get Bundle ID: " + applicationBundleID);
		}
		if (string.IsNullOrEmpty(marketPackageName))
		{
			marketPackageName = UniRatePlugin.GetPackageName();
			UniRateDebug.Log("Get Android package name: " + marketPackageName);
		}
		UniRateLauched();
	}

	private void UniRateLauched()
	{
		if (!IsSameVersion())
		{
			PlayerPrefs.SetString("UniRateLastVersionUsed", applicationVersion);
			UniRatePlayerPrefs.SetDate("UniRateFirstUsed", DateTime.Now);
			PlayerPrefs.SetInt("UniRateUseCount", 0);
			PlayerPrefs.SetInt("UniRateEventCount", 0);
			PlayerPrefs.DeleteKey("UniRateLastReminded");
			PlayerPrefs.Save();
			if (this.OnDetectAppUpdated != null)
			{
				this.OnDetectAppUpdated();
			}
		}
		IncreaseUseCount();
		if (promptAtLaunch && ShouldPromptForRating())
		{
			PromptIfNetworkAvailable();
		}
	}

	private bool IsSameVersion()
	{
		if (!string.IsNullOrEmpty(applicationVersion))
		{
			return string.Equals(PlayerPrefs.GetString("UniRateLastVersionUsed"), applicationVersion);
		}
		return false;
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (!pauseStatus && _instance != null)
		{
			IncreaseUseCount();
			if (promptAtLaunch && ShouldPromptForRating())
			{
				PromptIfNetworkAvailable();
			}
		}
	}

	private void IncreaseUseCount()
	{
		usesCount++;
	}

	private void IncreaseEventCount()
	{
		eventCount++;
	}

	private IEnumerator CheckForConnectivityInBackground()
	{
		string text = (string.IsNullOrEmpty(appStoreCountry) ? string.Format("http://itunes.apple.com/{0}lookup", "") : string.Format("http://itunes.apple.com/{0}lookup", appStoreCountry + "/"));
		text = ((appStoreID == 0) ? (text + "?bundleId=" + applicationBundleID) : (text + "?id=" + appStoreID));
		UniRateDebug.Log("Checking app info: " + text);
		bool errorHappened = false;
		WWW www = new WWW(text);
		yield return www;
		if (string.IsNullOrEmpty(www.error))
		{
			UniRateAppInfo uniRateAppInfo = new UniRateAppInfo(www.text);
			if (uniRateAppInfo.validAppInfo)
			{
				if (uniRateAppInfo.bundleId.Equals(applicationBundleID))
				{
					if (appStoreID == 0)
					{
						appStoreID = uniRateAppInfo.appID;
						UniRateDebug.Log("UniRate found the app, app id: " + appStoreID);
					}
					if (onlyPromptIfLatestVersion && !previewMode && !applicationVersion.Equals(uniRateAppInfo.version))
					{
						UniRateDebug.Log("No prompt because it is not the version in app store and you set onlyPromptIfLatestVersion.");
						errorHappened = true;
						UniRateFailWithError(Error.NotTheLatestVersion);
					}
				}
				else
				{
					Debug.LogWarning("The bundle Id is not the same. Appstore: " + uniRateAppInfo.bundleId + " vs AppSetting:" + applicationBundleID);
					errorHappened = true;
					UniRateFailWithError(Error.BundleIdDoesNotMatchAppStore);
				}
			}
			else if (appStoreID != 0)
			{
				Debug.LogWarning("No App info found with this app Id " + appStoreID);
				errorHappened = true;
				UniRateFailWithError(Error.AppNotFoundOnAppStore);
			}
			else
			{
				Debug.Log("No App info found with this bundle Id " + applicationBundleID);
				Debug.Log("Could not find your app on AppStore. It is normal when your app is not released, don't worry about this message.");
			}
		}
		else
		{
			UniRateDebug.Log("Error happend in loading app information. Maybe due to no internet connection.");
			errorHappened = true;
			UniRateFailWithError(Error.NetworkError);
		}
		if (!errorHappened)
		{
			ReadyToPrompt();
		}
	}

	private void ReadyToPrompt()
	{
		_currentChecking = false;
		if (this.ShouldUniRatePromptForRating != null && !this.ShouldUniRatePromptForRating())
		{
			UniRateDebug.Log("Not display prompt because ShouldUniRatePromptForRating returns false.");
		}
		else
		{
			ShowPrompt();
		}
	}

	private void UniRateFailWithError(Error error)
	{
		if (this.OnUniRateFaild != null)
		{
			this.OnUniRateFaild(error);
		}
	}

	private void UniRateUserDeclinedPrompt()
	{
		UniRateDebug.Log("User declined the prompt");
		declinedThisVersion = true;
		if (this.OnUserDeclinedToRate != null)
		{
			this.OnUserDeclinedToRate();
		}
	}

	private void UniRateUserWantRemind()
	{
		UniRateDebug.Log("User wants to be reminded later");
		lastReminded = DateTime.Now;
		if (this.OnUserWantReminderToRate != null)
		{
			this.OnUserWantReminderToRate();
		}
	}

	private void UniRateUserWantToRate()
	{
		UniRateDebug.Log("User wants to rate");
		ratedThisVersion = true;
		if (this.OnUserAttemptToRate != null)
		{
			this.OnUserAttemptToRate();
		}
		if (this.ShouldUniRateOpenRatePage == null || this.ShouldUniRateOpenRatePage())
		{
			OpenRatePage();
		}
	}

	private void OpenRatePage()
	{
	}
}
