using System;
using System.Collections;
using System.Collections.Generic;
using KA.Framework;
using UnityEngine;

public class AdManager : MonoBehaviour
{
	public class EventObjectData
	{
		public GameObject _GameObject;

		public AdEventType _AdEventType;

		public EventObjectData(GameObject go, AdEventType eventType)
		{
			_GameObject = go;
			_AdEventType = eventType;
		}
	}

	public int _PairDataID = 2017;

	public Action<bool> OnAppPaused;

	private const string DAY_START_KEY = "AdDayStart";

	private const string AD_EVENT_COUNT_KEY = "AdEventCount";

	private const string AD_EVENT_COOLDOWN_KEY = "AdEventCoolDownCount";

	private float[] mAdsDistribution;

	private float mLastAddShown;

	private bool mDoInitialize;

	private bool mUpdateEventData = true;

	private bool mIsReady;

	private bool mIsAdShown;

	private string mPercentage = "%";

	public float _TimeOutPeriod = 60f;

	private float mTimeOut;

	public LocaleString _NoAdsAvailableText = new LocaleString("Ads are not available! Please try again later.");

	public LocaleString _NoVideoAdsAvailableText = new LocaleString("Videos are not available! Please try again later.");

	public LocaleString _VideoFailedToLoadText = new LocaleString("[REVIEW] Video failed to load. Please try again later.");

	public LocaleString _DailyLimitReachedText = new LocaleString("[REVIEW] You have reached your daily limit to watch an Ad.");

	public LocaleString _AdRewardFailedText = new LocaleString("[REVIEW] Reward failed to load. Please try again later.");

	public LocaleString _PercentageText = new LocaleString("-{0}");

	public LocaleString _FlatReductionText = new LocaleString("-{0} min");

	private static AdManager mInstance;

	private Queue<Tuple<AdPlugin, AdParams>> mAdProvidersAvailable = new Queue<Tuple<AdPlugin, AdParams>>();

	private List<AdPlugin> mAdProviders = new List<AdPlugin>(10);

	public static AdManager pInstance => mInstance;

	public List<AdPlugin> pAdProviders => mAdProviders;

	public static bool RegisterPlugin(AdPlugin inAdPlugin)
	{
		if (inAdPlugin == null)
		{
			UtDebug.LogError("AdManager.RegisterPlugin inAdPlugin is null.");
			return false;
		}
		if (mInstance.mAdProviders.Find((AdPlugin p) => p._ProviderName == inAdPlugin._ProviderName) != null)
		{
			UtDebug.LogError("AdManager.RegisterPlugin. Ignoring attempt to register duplicate plugin " + inAdPlugin._ProviderName);
			return false;
		}
		mInstance.AddPluginEventHandlers(inAdPlugin);
		mInstance.mAdProviders.Add(inAdPlugin);
		return true;
	}

	public static void UnregisterPlugin(AdPlugin inAdPlugin)
	{
		if (inAdPlugin == null)
		{
			UtDebug.LogError("AdManager.UnregisterPlugin inAdPlugin is null.");
			return;
		}
		AdPlugin adPlugin = mInstance.mAdProviders.Find((AdPlugin p) => p._ProviderName == inAdPlugin._ProviderName);
		if (adPlugin != null)
		{
			mInstance.RemovePluginEventHandlers(adPlugin);
			mInstance.mAdProviders.Remove(adPlugin);
		}
	}

	public void Reset()
	{
		mUpdateEventData = true;
	}

	private void Awake()
	{
		mInstance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void AddPluginEventHandlers(AdPlugin adPlugin)
	{
		if (!(adPlugin == null))
		{
			adPlugin.ConnectSucceeded += HandlePluginConnectSucceeded;
			adPlugin.ConnectFailed += HandlePluginConnectFailed;
			adPlugin.Destroying += UnregisterPlugin;
			adPlugin.RewardPointsEarned += HandlePluginRewardPointsEarned;
			adPlugin.AdRequested += HandlePluginAdRequested;
			adPlugin.AdRequestCancelled += HandlePluginAdRequestCancelled;
			adPlugin.AdOpened += HandlePluginAdOpened;
			adPlugin.AdClosed += HandlePluginAdClosed;
			adPlugin.AdFinished += HandlePluginAdFinished;
			adPlugin.AdSkipped += HandlePluginAdSkipped;
			adPlugin.ShowAdFailed += HandlePluginShowAdFailed;
		}
	}

	private void RemovePluginEventHandlers(AdPlugin adPlugin)
	{
		if (!(adPlugin == null))
		{
			adPlugin.ConnectSucceeded -= HandlePluginConnectSucceeded;
			adPlugin.ConnectFailed -= HandlePluginConnectFailed;
			adPlugin.Destroying -= UnregisterPlugin;
			adPlugin.RewardPointsEarned -= HandlePluginRewardPointsEarned;
			adPlugin.AdRequested -= HandlePluginAdRequested;
			adPlugin.AdRequestCancelled -= HandlePluginAdRequestCancelled;
			adPlugin.AdOpened -= HandlePluginAdOpened;
			adPlugin.AdClosed -= HandlePluginAdClosed;
			adPlugin.AdFinished -= HandlePluginAdFinished;
			adPlugin.AdSkipped -= HandlePluginAdSkipped;
			adPlugin.ShowAdFailed -= HandlePluginShowAdFailed;
		}
	}

	public void Init()
	{
		mIsReady = false;
		mDoInitialize = true;
	}

	private void Update()
	{
		if (mDoInitialize && AdData.pIsReady && AdData.pAdSettings != null && ProductConfig.pIsReady)
		{
			mDoInitialize = false;
			AdsWeight[] adsWeight = AdData.pAdSettings.adsWeight;
			mAdsDistribution = new float[adsWeight.Length];
			for (int i = 0; i < adsWeight.Length; i++)
			{
				mAdsDistribution[i] = adsWeight[i].Percentage;
			}
			ProductPlatform currentPlatform = ProductSettings.GetPlatform();
			foreach (AdPlugin mAdProvider in mAdProviders)
			{
				if (Array.Exists(mAdProvider._SupportedPlatforms, (ProductPlatform p) => p == currentPlatform))
				{
					mAdProvider.Init();
				}
			}
			mIsReady = true;
		}
		if (mTimeOut > 0f)
		{
			mTimeOut -= Time.deltaTime;
			if (mTimeOut <= 0f)
			{
				OnTimeOutExpired();
			}
		}
		if (!mUpdateEventData || mAdProviders.Count <= 0 || !ParentData.pIsReady || !ServerTime.pIsReady)
		{
			return;
		}
		mUpdateEventData = false;
		ParentData.pInstance.LoadPairData(_PairDataID, LoadParentPairDataEventHandler);
		foreach (AdPlugin mAdProvider2 in mAdProviders)
		{
			mAdProvider2.SetAge(ParentData.pInstance.pUserInfo.Age ?? 1);
		}
	}

	private void LoadParentPairDataEventHandler(bool success, PairData pData, object inUserData)
	{
		UpdateEventsData();
	}

	private void SaveParentPairDataEventHandler(bool success, PairData pData, object inUserData)
	{
		if (success)
		{
			if (inUserData != null)
			{
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				((EventObjectData)inUserData)?._GameObject.GetComponent<IAdResult>()?.OnAdWatched();
			}
			return;
		}
		UtDebug.LogError("-----WEB SERVICE CALL SetPairData FAILED!!!");
		if (inUserData != null)
		{
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			EventObjectData eventObjectData = (EventObjectData)inUserData;
			if (eventObjectData != null)
			{
				SyncAdAvailableCount(eventObjectData._AdEventType, isConsumed: false, updateOnServer: false);
				eventObjectData._GameObject.GetComponent<IAdResult>()?.OnAdFailed();
				DisplayOKMessage(_AdRewardFailedText.GetLocalizedString());
			}
		}
	}

	private void UpdateEventsData()
	{
		PairData pairDataByID = ParentData.pInstance.GetPairDataByID(_PairDataID);
		if (pairDataByID == null || !ParentData.pIsReady || !ServerTime.pIsReady)
		{
			return;
		}
		string value = pairDataByID.GetValue("AdDayStart");
		DateTime dateTime = ServerTime.pCurrentTime.ToLocalTime();
		int num = 0;
		if (!string.IsNullOrEmpty(value) && value != "___VALUE_NOT_FOUND___")
		{
			try
			{
				DateTime dateTime2 = DateTime.Parse(value, UtUtilities.GetCultureInfo("en-US"));
				num = (dateTime - dateTime2).Days;
				if (num >= 1)
				{
					ParentData.pInstance.UpdatePairData(_PairDataID, "AdDayStart", GetStartDateTime(dateTime));
				}
			}
			catch (Exception exception)
			{
				UtDebug.Log("Ad Manager - Unable to parse start date time : ");
				ParentData.pInstance.UpdatePairData(_PairDataID, "AdDayStart", GetStartDateTime(dateTime));
				Debug.LogException(exception);
			}
		}
		else
		{
			ParentData.pInstance.UpdatePairData(_PairDataID, "AdDayStart", GetStartDateTime(dateTime));
		}
		EventsData[] eventsData = AdData.pAdSettings.eventsData;
		foreach (EventsData eventsData2 in eventsData)
		{
			if (eventsData2.LimitsPerDay <= 0)
			{
				if (pairDataByID.KeyExists("AdEventCount" + eventsData2.AdEventType))
				{
					UtDebug.Log("Ad Manager - Ad Event count removed : " + eventsData2.AdEventType);
					pairDataByID.RemoveByKey("AdEventCount" + eventsData2.AdEventType);
				}
				if (eventsData2.LimitsPerDay == 0)
				{
					if (pairDataByID.KeyExists("AdEventCoolDownCount" + eventsData2.AdEventType))
					{
						UtDebug.Log("Ad Manager - Ad Event cooldown removed : " + eventsData2.AdEventType);
						pairDataByID.RemoveByKey("AdEventCoolDownCount" + eventsData2.AdEventType);
					}
					continue;
				}
			}
			else if (num <= 0)
			{
				eventsData2.pAvailableCount = pairDataByID.GetIntValue("AdEventCount" + eventsData2.AdEventType, -1);
				if (eventsData2.pAvailableCount == -1)
				{
					eventsData2.pAvailableCount = eventsData2.LimitsPerDay;
					ParentData.pInstance.UpdatePairData(_PairDataID, "AdEventCount" + eventsData2.AdEventType, eventsData2.pAvailableCount.ToString());
				}
				UtDebug.Log("Ad Manager - " + eventsData2.AdEventType.ToString() + " : Same day, Display count : " + eventsData2.pAvailableCount, 100);
			}
			else if (num >= 1)
			{
				UtDebug.Log("Ad Manager - " + eventsData2.AdEventType.ToString() + " : Reset day, Display count : " + eventsData2.pAvailableCount, 100);
				eventsData2.pAvailableCount = eventsData2.LimitsPerDay;
				ParentData.pInstance.UpdatePairData(_PairDataID, "AdEventCount" + eventsData2.AdEventType, eventsData2.pAvailableCount.ToString());
			}
			if (eventsData2.CoolDown > 0f)
			{
				try
				{
					eventsData2.pCoolDownEndTime = DateTime.Parse(pairDataByID.GetStringValue("AdEventCoolDownCount" + eventsData2.AdEventType, ""));
				}
				catch (Exception)
				{
					eventsData2.pCoolDownEndTime = DateTime.MinValue;
					ParentData.pInstance.UpdatePairData(_PairDataID, "AdEventCoolDownCount" + eventsData2.AdEventType, eventsData2.pCoolDownEndTime.ToString());
				}
			}
			else if (pairDataByID.KeyExists("AdEventCoolDownCount" + eventsData2.AdEventType))
			{
				UtDebug.Log("Ad Manager - Ad Event cooldown removed : " + eventsData2.AdEventType);
				pairDataByID.RemoveByKey("AdEventCoolDownCount" + eventsData2.AdEventType);
			}
		}
		SavePairData();
	}

	private void SavePairData(EventObjectData eventData = null)
	{
		PairData pairDataByID = ParentData.pInstance.GetPairDataByID(_PairDataID);
		if (pairDataByID != null && pairDataByID._IsDirty)
		{
			if (eventData != null)
			{
				UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			}
			ParentData.pInstance.SavePairData(_PairDataID, SaveParentPairDataEventHandler, eventData);
		}
	}

	private string GetStartDateTime(DateTime dateTime)
	{
		return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).ToString(UtUtilities.GetCultureInfo("en-US"));
	}

	public void UpdateAdAvailableCount(AdEventType eventType, GameObject messageObject)
	{
		EventsData eventData = GetEventData(eventType);
		PairData pairDataByID = ParentData.pInstance.GetPairDataByID(_PairDataID);
		if (eventData == null || !ParentData.pIsReady || pairDataByID == null)
		{
			return;
		}
		if (eventData.LimitsPerDay == -1 && !pairDataByID.KeyExists("AdEventCount" + eventData.AdEventType))
		{
			new EventObjectData(messageObject, eventType)._GameObject.GetComponent<IAdResult>()?.OnAdWatched();
		}
		else if (pairDataByID.KeyExists("AdEventCount" + eventData.AdEventType))
		{
			int num = eventData.pAvailableCount - 1;
			if (num < 0)
			{
				num = 0;
			}
			UtDebug.Log("Ad Manager - " + eventData.AdEventType.ToString() + " : Update Display count : " + num);
			ParentData.pInstance.UpdatePairData(_PairDataID, "AdEventCount" + eventData.AdEventType, num.ToString());
			if (pairDataByID.KeyExists("AdEventCoolDownCount" + eventData.AdEventType))
			{
				eventData.pCoolDownEndTime = ServerTime.pCurrentTime.AddHours(eventData.CoolDown);
				UtDebug.Log("Ad Manager - " + eventData.AdEventType.ToString() + " : Update cooldown : " + eventData.pCoolDownEndTime);
				ParentData.pInstance.UpdatePairData(_PairDataID, "AdEventCoolDownCount" + eventData.AdEventType, eventData.pCoolDownEndTime.ToString());
			}
			SavePairData(new EventObjectData(messageObject, eventType));
		}
	}

	public void SyncAdAvailableCount(AdEventType eventType, bool isConsumed, bool updateOnServer = true)
	{
		EventsData eventData = GetEventData(eventType);
		if (isConsumed)
		{
			eventData.pAvailableCount--;
			return;
		}
		ParentData.pInstance.UpdatePairData(_PairDataID, "AdEventCount" + eventData.AdEventType, eventData.pAvailableCount.ToString());
		if (updateOnServer)
		{
			SavePairData();
		}
	}

	public void ResetAdCoolDown(AdEventType eventType)
	{
		EventsData eventData = GetEventData(eventType);
		if (eventData != null)
		{
			eventData.pCoolDownEndTime = DateTime.MinValue;
			ParentData.pInstance.UpdatePairData(_PairDataID, "AdEventCoolDownCount" + eventData.AdEventType, eventData.pCoolDownEndTime.ToString());
			SavePairData();
		}
	}

	public bool AdSupported(AdEventType eventType, AdType type)
	{
		EventsData eventData = GetEventData(eventType);
		if (eventData == null || eventData.LimitsPerDay == 0)
		{
			return false;
		}
		AdsWeight[] providersForAds = GetProvidersForAds(type, eventType);
		foreach (AdsWeight provider in providersForAds)
		{
			if (provider.AdType == type && mInstance.mAdProviders.Find((AdPlugin p) => p._ProviderName == provider.ProviderName) != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool AdAvailable(AdEventType eventType, AdType type, bool showErrorMessage = true)
	{
		if (IsDailyLimitAvailable(eventType, showErrorMessage))
		{
			AdsWeight[] providersForAds = GetProvidersForAds(type, eventType);
			foreach (AdsWeight provider in providersForAds)
			{
				if (provider.AdType == type)
				{
					AdPlugin adPlugin = mInstance.mAdProviders.Find((AdPlugin p) => p._ProviderName == provider.ProviderName);
					if (adPlugin != null && adPlugin.AdAvailable(type))
					{
						return true;
					}
				}
			}
			if (showErrorMessage)
			{
				DisplayOKMessage(_NoVideoAdsAvailableText.GetLocalizedString());
			}
		}
		return false;
	}

	public bool AdProvidersAvailable(AdType type)
	{
		AdsWeight[] providersForAdType = GetProvidersForAdType(type);
		foreach (AdsWeight provider in providersForAdType)
		{
			if (provider.AdType == type && mInstance.mAdProviders.Find((AdPlugin p) => p._ProviderName == provider.ProviderName) != null)
			{
				return true;
			}
		}
		return false;
	}

	public string GetAdReward(AdEventType eventType)
	{
		EventsData eventData = GetEventData(eventType);
		if (eventData == null || eventData.LimitsPerDay == 0)
		{
			return "";
		}
		return eventData.RewardData;
	}

	private void OnFeaturedAppLoaded(Hashtable table)
	{
		mIsAdShown = false;
	}

	public static void DisplayAd(AdEventType evt, AdOption opt)
	{
		if (mInstance != null)
		{
			mInstance.ShowAd(evt, opt);
		}
	}

	public static void DisplayAd(AdType adType, GameObject gameObject, string userId)
	{
		if (!(mInstance == null))
		{
			for (int i = 0; i < mInstance.mAdProviders.Count; i++)
			{
				AdParams adParam = default(AdParams);
				adParam.adProvider = "";
				adParam.adType = adType;
				adParam.gameObject = gameObject;
				adParam.userID = userId;
				mInstance.mAdProviders[i].ShowAd(adParam);
			}
		}
	}

	public static void DisplayAd(AdEventType eventType, AdType adType, GameObject gameObject, string userId)
	{
		if (mInstance == null)
		{
			return;
		}
		AdsWeight[] providersForAds = mInstance.GetProvidersForAds(adType, eventType);
		if (providersForAds == null || mInstance.mAdProvidersAvailable.Count > 0)
		{
			return;
		}
		AdParams item = default(AdParams);
		item.adType = adType;
		item.gameObject = gameObject;
		item.adEvent = eventType;
		AdsWeight[] array = providersForAds;
		foreach (AdsWeight adsWeigh in array)
		{
			AdPlugin adPlugin = mInstance.mAdProviders.Find((AdPlugin p) => p._ProviderName == adsWeigh.ProviderName);
			if (adPlugin != null)
			{
				mInstance.mAdProvidersAvailable.Enqueue(new Tuple<AdPlugin, AdParams>(adPlugin, item));
			}
		}
		mInstance.ShowAd();
	}

	private void ShowAd(bool showErrorMessage = false)
	{
		if (mAdProvidersAvailable.Count <= 0)
		{
			if (showErrorMessage)
			{
				DisplayOKMessage(_VideoFailedToLoadText.GetLocalizedString());
			}
			return;
		}
		Tuple<AdPlugin, AdParams> tuple = mAdProvidersAvailable.Dequeue();
		if (!tuple.Item1.ShowAd(tuple.Item2))
		{
			ShowAd(showErrorMessage: true);
		}
	}

	public static void DisplayAd(AdEventType eventType, AdType adType, GameObject gameObject)
	{
		if (mInstance == null)
		{
			return;
		}
		AdsWeight[] providersForAds = mInstance.GetProvidersForAds(adType, eventType);
		if (providersForAds == null || mInstance.mAdProvidersAvailable.Count > 0)
		{
			return;
		}
		AdParams item = default(AdParams);
		item.adType = adType;
		item.gameObject = gameObject;
		item.adEvent = eventType;
		AdsWeight[] array = providersForAds;
		foreach (AdsWeight adsWeigh in array)
		{
			AdPlugin adPlugin = mInstance.mAdProviders.Find((AdPlugin p) => p._ProviderName == adsWeigh.ProviderName);
			if (adPlugin != null)
			{
				mInstance.mAdProvidersAvailable.Enqueue(new Tuple<AdPlugin, AdParams>(adPlugin, item));
			}
		}
		mInstance.ShowAd();
	}

	private bool ShowAd(AdEventType evt, AdOption opt)
	{
		bool result = false;
		if (!mIsReady || (SubscriptionInfo.pIsMember && opt != AdOption.HIDE))
		{
			return result;
		}
		float num = Time.time - mLastAddShown;
		AdsWeight[] providersForEvent = GetProvidersForEvent(evt);
		if (providersForEvent != null && (num >= (float)AdData.pAdSettings.minDelayBetweenAdds || mLastAddShown == 0f) && (float)UnityEngine.Random.Range(1, 100) <= GetEventProbablity(evt))
		{
			AdType adType = AdType.NONE;
			string adProvider = "None";
			string data = "";
			float num2 = 0f;
			int num3 = UnityEngine.Random.Range(0, 100);
			for (int i = 0; i < providersForEvent.Length; i++)
			{
				if (providersForEvent[i].AdType == AdType.REWARDED_VIDEO)
				{
					continue;
				}
				num2 += mAdsDistribution[i];
				if ((float)num3 < num2)
				{
					adType = providersForEvent[i].AdType;
					adProvider = providersForEvent[i].ProviderName;
					if (!string.IsNullOrEmpty(providersForEvent[i].CustomData))
					{
						data = providersForEvent[i].CustomData;
					}
					break;
				}
			}
			UtDebug.Log("ShowAd: " + adType);
			if (adType != 0)
			{
				mLastAddShown = Time.time;
				AdPlugin adPlugin = mAdProviders.Find((AdPlugin p) => p._ProviderName == adProvider);
				if (adPlugin != null)
				{
					EventsData eventData = GetEventData(evt);
					AdParams adParam = default(AdParams);
					adParam.adProvider = adProvider;
					adParam.adType = adType;
					adParam.adOption = opt;
					adParam.adEvent = eventData.AdEventType;
					if (UserInfo.pInstance != null)
					{
						adParam.userID = ProductConfig.pApiKey + "|" + UserInfo.pInstance.UserID;
					}
					adParam.data = data;
					adPlugin.ShowAd(adParam);
				}
			}
		}
		return result;
	}

	private AdsWeight[] GetProvidersForEvent(AdEventType adEvent)
	{
		List<AdsWeight> list = new List<AdsWeight>();
		for (int i = 0; i < AdData.pAdSettings?.adsWeight.Length; i++)
		{
			AdsWeight adsWeight = AdData.pAdSettings.adsWeight[i];
			if (adsWeight.SupportedEvents != null)
			{
				if (Array.Exists(adsWeight.SupportedEvents, (AdEventType? p) => p.Equals(adEvent)))
				{
					list.Add(adsWeight);
				}
			}
			else if (adsWeight.UnSupportedEvents != null && !Array.Exists(adsWeight.UnSupportedEvents, (AdEventType? p) => p.Equals(adEvent)))
			{
				list.Add(adsWeight);
			}
		}
		return list.ToArray();
	}

	public bool ProviderAvailableForEvent(AdEventType adEvent, string providerName)
	{
		AdsWeight[] providersForEvent = GetProvidersForEvent(adEvent);
		for (int i = 0; i < providersForEvent.Length; i++)
		{
			if (providersForEvent[i].ProviderName == providerName)
			{
				return true;
			}
		}
		return false;
	}

	public AdsWeight[] GetProvidersForAdType(AdType adType)
	{
		List<AdsWeight> list = new List<AdsWeight>();
		for (int i = 0; i < AdData.pAdSettings?.adsWeight.Length; i++)
		{
			AdsWeight adsWeight = AdData.pAdSettings.adsWeight[i];
			if (adsWeight.AdType == adType)
			{
				list.Add(adsWeight);
			}
		}
		return list.ToArray();
	}

	private AdsWeight[] GetProvidersForAds(AdType adType, AdEventType adEventType)
	{
		List<AdsWeight> list = new List<AdsWeight>();
		for (int i = 0; i < AdData.pAdSettings?.adsWeight.Length; i++)
		{
			AdsWeight adsWeight = AdData.pAdSettings.adsWeight[i];
			if (adsWeight.AdType != adType)
			{
				continue;
			}
			if (adsWeight.SupportedEvents != null)
			{
				if (Array.Exists(adsWeight.SupportedEvents, (AdEventType? p) => p.Equals(adEventType)))
				{
					list.Add(adsWeight);
				}
			}
			else if (adsWeight.UnSupportedEvents != null && !Array.Exists(adsWeight.UnSupportedEvents, (AdEventType? p) => p.Equals(adEventType)))
			{
				list.Add(adsWeight);
			}
		}
		return list.ToArray();
	}

	private EventsData GetEventData(AdEventType evt)
	{
		if (AdData.pAdSettings != null)
		{
			EventsData[] eventsData = AdData.pAdSettings.eventsData;
			for (int i = 0; i < eventsData.Length; i++)
			{
				if (evt == eventsData[i].AdEventType)
				{
					return eventsData[i];
				}
			}
		}
		return null;
	}

	private float GetEventProbablity(AdEventType evt)
	{
		float result = 0f;
		EventsData eventData = GetEventData(evt);
		if (eventData != null)
		{
			result = eventData.Percentage;
		}
		return result;
	}

	public void OnError()
	{
		mIsAdShown = false;
	}

	public void OnSuccess()
	{
		mIsAdShown = true;
	}

	public void OnClosed()
	{
		mIsAdShown = false;
	}

	private void StartTimeOut()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		mTimeOut = _TimeOutPeriod;
	}

	private void StopTimeOut()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		mTimeOut = 0f;
		mAdProvidersAvailable.Clear();
	}

	private void OnTimeOutExpired()
	{
		StopTimeOut();
		foreach (AdPlugin mAdProvider in mAdProviders)
		{
			mAdProvider.CancelAdRequest();
		}
		DisplayOKMessage(_NoAdsAvailableText.GetLocalizedString());
	}

	private void OnTapPointsEarnedEvent(int tapPoints)
	{
		UtDebug.Log("OnTapPointsEarnedEventHandler" + tapPoints);
	}

	private void OnApplicationPause(bool isPause)
	{
		if (!isPause && mIsAdShown)
		{
			mIsAdShown = false;
			StopTimeOut();
			AudioListener.volume = 1f;
			if (OnAppPaused != null)
			{
				OnAppPaused(obj: false);
			}
		}
		else if (isPause && OnAppPaused != null)
		{
			OnAppPaused(mIsAdShown);
		}
	}

	private void HandlePluginConnectSucceeded()
	{
	}

	private void HandlePluginConnectFailed()
	{
	}

	private void HandlePluginRewardPointsEarned(int inPoints)
	{
	}

	private void HandlePluginAdRequested(AdPlugin plugin)
	{
		if (plugin.pCurrentAdType == AdType.WALL || plugin.pCurrentAdType == AdType.VIDEO || plugin.pCurrentAdType == AdType.REWARDED_VIDEO)
		{
			StartTimeOut();
		}
	}

	private void HandlePluginAdRequestCancelled(AdPlugin plugin)
	{
		if (plugin.pCurrentAdType == AdType.WALL || plugin.pCurrentAdType == AdType.VIDEO || plugin.pCurrentAdType == AdType.REWARDED_VIDEO)
		{
			if (plugin != null && plugin.pMessageObject != null)
			{
				plugin.pMessageObject.GetComponent<IAdResult>()?.OnAdCancelled();
			}
			StopTimeOut();
		}
	}

	private void HandlePluginAdOpened(AdPlugin plugin)
	{
		StopTimeOut();
		mIsAdShown = true;
		AudioListener.volume = 0f;
	}

	private void HandlePluginAdClosed(AdPlugin plugin)
	{
		StopTimeOut();
		if (mIsAdShown)
		{
			AudioListener.volume = 1f;
		}
		mIsAdShown = false;
		if (plugin != null && plugin.pMessageObject != null)
		{
			plugin.pMessageObject.GetComponent<IAdResult>()?.OnAdClosed();
		}
	}

	private void HandlePluginAdFinished(AdPlugin plugin)
	{
		if (plugin != null && plugin.pMessageObject != null)
		{
			EventsData eventData = GetEventData(plugin.pCurrentAdEvent);
			plugin.pMessageObject.GetComponent<IAdResult>()?.OnAdFinished(eventData.RewardData);
			UpdateAdAvailableCount(plugin.pCurrentAdEvent, plugin.pMessageObject);
		}
		if (mIsAdShown)
		{
			AudioListener.volume = 1f;
		}
		mIsAdShown = false;
	}

	private void HandlePluginAdSkipped(AdPlugin plugin)
	{
		StopTimeOut();
		if (mIsAdShown)
		{
			AudioListener.volume = 1f;
		}
		mIsAdShown = false;
		if (plugin != null && plugin.pMessageObject != null)
		{
			plugin.pMessageObject.GetComponent<IAdResult>()?.OnAdSkipped();
		}
	}

	private void HandlePluginShowAdFailed(AdPlugin plugin, string inStrError)
	{
		if (mAdProvidersAvailable.Count > 0)
		{
			mInstance.ShowAd();
			return;
		}
		StopTimeOut();
		if (mIsAdShown)
		{
			AudioListener.volume = 1f;
		}
		mIsAdShown = false;
		switch (plugin.pCurrentAdType)
		{
		case AdType.WALL:
			DisplayOKMessage(_NoAdsAvailableText.GetLocalizedString());
			break;
		case AdType.VIDEO:
			DisplayOKMessage(_NoVideoAdsAvailableText.GetLocalizedString());
			break;
		case AdType.REWARDED_VIDEO:
			DisplayOKMessage(_VideoFailedToLoadText.GetLocalizedString());
			break;
		}
		if (plugin != null && plugin.pMessageObject != null)
		{
			plugin.pMessageObject.GetComponent<IAdResult>()?.OnAdFailed();
		}
	}

	private void DisplayOKMessage(string inText)
	{
		GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB");
		if (gameObject != null)
		{
			GameObject obj = UnityEngine.Object.Instantiate(gameObject);
			obj.name = "UiGenericDB";
			KAUIGenericDB component = obj.GetComponent<KAUIGenericDB>();
			component.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			component._MessageObject = base.gameObject;
			component._OKMessage = "OnDBClose";
			component.SetText(inText, interactive: false);
			component.SetDestroyOnClick(isDestroy: true);
			KAUI.SetExclusive(component, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		}
	}

	public void OnDBClose()
	{
		KAUI.RemoveExclusive(KAUI._GlobalExclusiveUI);
	}

	public void LogAdWatchedEvent(AdEventType eventType, string reward)
	{
		EventsData eventData = GetEventData(eventType);
		if (eventData?.ModuleNameText != null)
		{
			string text = eventData.ModuleNameText._Text;
			if (!string.IsNullOrEmpty(text))
			{
				new Dictionary<string, object>().Add(text, reward);
			}
		}
	}

	public bool IsReductionTimeGreater(AdEventType adEventType, int remainingTime)
	{
		string adReward = GetAdReward(adEventType);
		if (adReward == "")
		{
			return false;
		}
		if (adReward.Contains(mPercentage))
		{
			return true;
		}
		return remainingTime > int.Parse(adReward) * 60;
	}

	public int GetReductionTime(AdEventType adEventType, int remainingtimeinsecond)
	{
		string adReward = GetAdReward(adEventType);
		int result = 0;
		if (adReward.Contains(mPercentage))
		{
			int.TryParse(adReward.Trim(mPercentage.ToCharArray()), out result);
			return (int)((float)remainingtimeinsecond * ((float)result / 100f));
		}
		return int.Parse(adReward) * 60;
	}

	public string GetReductionTimeText(AdEventType adEventType)
	{
		string adReward = GetAdReward(adEventType);
		if (adReward.Contains(mPercentage))
		{
			return string.Format(_PercentageText.GetLocalizedString(), adReward);
		}
		return string.Format(_FlatReductionText.GetLocalizedString(), adReward);
	}

	public bool IsDailyLimitAvailable(AdEventType eventType, bool showErrorMessage = true)
	{
		EventsData eventData = GetEventData(eventType);
		if (eventData == null || eventData.LimitsPerDay == 0)
		{
			if (showErrorMessage)
			{
				UtDebug.LogError("Ad is not configured properly for " + eventType);
			}
			return false;
		}
		if (eventData.LimitsPerDay < 0)
		{
			return true;
		}
		UpdateEventsData();
		if (eventData.pAvailableCount <= 0)
		{
			if (showErrorMessage)
			{
				string text = _DailyLimitReachedText.GetLocalizedString();
				if (eventData.ModuleNameText != null)
				{
					string localizedString = eventData.ModuleNameText.GetLocalizedString();
					if (!string.IsNullOrEmpty(localizedString))
					{
						text = text.Replace("{module name}", localizedString);
					}
				}
				DisplayOKMessage(text);
			}
			return false;
		}
		return true;
	}

	public void GetAvailableAndDailyLimitCount(AdEventType adEventType, out int dailyLimit, out int available)
	{
		dailyLimit = 0;
		available = 0;
		EventsData eventData = GetEventData(adEventType);
		if (eventData != null)
		{
			dailyLimit = eventData.LimitsPerDay;
			available = eventData.pAvailableCount;
		}
	}

	public bool UnderCoolDown(AdEventType eventType)
	{
		EventsData eventData = GetEventData(eventType);
		if (eventData == null || eventData.CoolDown == 0f)
		{
			return false;
		}
		return eventData.pCoolDownEndTime.Subtract(ServerTime.pCurrentTime).Seconds > 0;
	}

	public DateTime CoolDownEndTime(AdEventType eventType)
	{
		EventsData eventData = GetEventData(eventType);
		if (eventData == null || eventData.CoolDown == 0f)
		{
			return DateTime.MinValue;
		}
		return eventData.pCoolDownEndTime;
	}

	public float CoolDownTime(AdEventType eventType)
	{
		EventsData eventData = GetEventData(eventType);
		if (eventData == null || eventData.CoolDown == 0f)
		{
			return 0f;
		}
		return eventData.CoolDown;
	}

	public bool HideAdEventForMember(AdEventType eventType)
	{
		EventsData eventData = GetEventData(eventType);
		if (eventData == null || eventData.HideForMember)
		{
			return false;
		}
		if (SubscriptionInfo.pIsMember)
		{
			return eventData.HideForMember;
		}
		return false;
	}
}
