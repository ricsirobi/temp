using System;
using System.IO;
using System.Xml.Serialization;
using KA.Framework;

[Serializable]
[XmlRoot(ElementName = "AdData", Namespace = "")]
public class AdData
{
	public delegate void OnAdDataReady();

	[XmlElement(ElementName = "AdsWeight")]
	public AdsWeight[] adsWeight;

	[XmlElement(ElementName = "EventsData")]
	public EventsData[] eventsData;

	[XmlElement(ElementName = "MinDelay")]
	public int minDelayBetweenAdds;

	[XmlElement(ElementName = "NoShowScenes")]
	public string[] noShowScenes;

	[XmlElement(ElementName = "AdsNetworkEnableInfo")]
	public AdsNetworkEnableInfo[] adsNetworkEnableInfo;

	private static AdData mAdSettings;

	public static bool pIsReady;

	public static OnAdDataReady mOnAdDataReady;

	public static AdData pAdSettings => mAdSettings;

	public static void Init()
	{
		if (mAdSettings == null && !string.IsNullOrEmpty(GameConfig.GetKeyData("AdDataFile")))
		{
			RsResourceManager.Load(GameConfig.GetKeyData("AdDataFile"), XmlLoadEventHandler);
		}
	}

	public static void Dump()
	{
		if (mAdSettings != null)
		{
			AdsWeight[] array = mAdSettings.adsWeight;
			foreach (AdsWeight adsWeight in array)
			{
				UtDebug.Log("AdsWeight " + adsWeight.AdType.ToString() + " Percent " + adsWeight.Percentage, 100);
			}
		}
	}

	public static AdsNetworkEnableInfo GetAdsNetworkEnableInfo(ProductPlatform platform, string provider)
	{
		if (mAdSettings != null && mAdSettings.adsNetworkEnableInfo != null)
		{
			AdsNetworkEnableInfo[] array = mAdSettings.adsNetworkEnableInfo;
			foreach (AdsNetworkEnableInfo adsNetworkEnableInfo in array)
			{
				if (adsNetworkEnableInfo.Platform == platform && adsNetworkEnableInfo.ProviderName == provider)
				{
					return adsNetworkEnableInfo;
				}
			}
		}
		return null;
	}

	public static void XmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			pIsReady = true;
			if (inObject != null)
			{
				using StringReader textReader = new StringReader((string)inObject);
				mAdSettings = new XmlSerializer(typeof(AdData)).Deserialize(textReader) as AdData;
				Dump();
			}
			if (mOnAdDataReady != null)
			{
				mOnAdDataReady();
			}
			break;
		case RsResourceLoadEvent.ERROR:
			pIsReady = true;
			mAdSettings = null;
			if (mOnAdDataReady != null)
			{
				mOnAdDataReady();
			}
			UtDebug.LogError("AdData missing!!!");
			break;
		}
	}
}
