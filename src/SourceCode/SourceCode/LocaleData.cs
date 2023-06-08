using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "LocaleData", Namespace = "")]
public class LocaleData
{
	[XmlElement(ElementName = "ls")]
	public StringData[] StringData;

	[XmlElement(ElementName = "fi")]
	public FontData[] FontData;

	public static LocaleData pInstance = null;

	public static bool pIsReady = false;

	public static int pNumBundleLoading = 0;

	private static List<FontInstance> mLoadingList = new List<FontInstance>();

	public static void Init()
	{
		if (!pIsReady)
		{
			pNumBundleLoading = 0;
			pIsReady = false;
			pInstance = null;
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			if (!UtUtilities.GetLocaleLanguage().Equals("en-US", StringComparison.OrdinalIgnoreCase))
			{
				string keyData = GameConfig.GetKeyData("LocaleDataAsset");
				RsResourceManager.Unload(keyData, splitURL: false, force: true);
				RsResourceManager.Load(keyData, XmlLoadEventHandler, RsResourceType.XML);
			}
			else
			{
				pInstance = new LocaleData();
				CreateTables(pInstance);
			}
		}
	}

	private static void XmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				pInstance = UtUtilities.DeserializeFromXml<LocaleData>((string)inObject);
			}
			if (pInstance == null)
			{
				Debug.Log("@@@@ Failed to get create local data object for ::: " + UtUtilities.GetLocaleLanguage());
				pIsReady = true;
			}
			else
			{
				CreateTables(pInstance);
			}
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("!!! Local data XML download failed!!!! " + UtUtilities.GetLocaleLanguage());
			pIsReady = true;
			break;
		}
	}

	public static void Reset()
	{
		pIsReady = false;
		pInstance = null;
	}

	private static void CreateTables(LocaleData ld)
	{
		mLoadingList.Clear();
		FontTable.pInstance = FontTable.CreateFontTable(ld.FontData);
		StringTable.pInstance = StringTable.CreateStringTable(ld.StringData);
		int count = mLoadingList.Count;
		int num = 0;
		for (num = 0; num < count; num++)
		{
			if (!mLoadingList[num].pIsReady)
			{
				pNumBundleLoading++;
			}
		}
		if (pNumBundleLoading <= 0)
		{
			pIsReady = true;
			return;
		}
		for (num = 0; num < count; num++)
		{
			if (!mLoadingList[num].pIsReady)
			{
				mLoadingList[num].LoadRes();
			}
		}
	}

	public static void AddFont(FontInstance fi)
	{
		mLoadingList.Add(fi);
	}

	public static void ReportLoaded()
	{
		pNumBundleLoading--;
		if (pNumBundleLoading <= 0)
		{
			pIsReady = true;
		}
	}
}
