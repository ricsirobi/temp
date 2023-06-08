using System;
using System.Collections.Generic;
using UnityEngine;

public class PrefetchWarning
{
	public enum DownloadStateType
	{
		INSTALL,
		DOWNLOAD_REQUIRED,
		LANGUAGE_UPDATE,
		BUNDLE_UPDATE
	}

	public class WarningDataSetting
	{
		public DownloadStateType _Type;

		public string _Value = "";

		public WarningDataSetting(DownloadStateType type, string value)
		{
			_Type = type;
			_Value = value;
		}
	}

	private List<WarningDataSetting> mCurrentWarningData;

	private const string WarningDataKey = "PrefetchWarning";

	private static PrefetchWarning mInstance;

	public static PrefetchWarning pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new PrefetchWarning();
			}
			return mInstance;
		}
	}

	public PrefetchWarning()
	{
		Init();
	}

	private void Init()
	{
		if (mCurrentWarningData != null)
		{
			mCurrentWarningData = null;
		}
		mCurrentWarningData = new List<WarningDataSetting>();
		string @string = PlayerPrefs.GetString("PrefetchWarning");
		if (string.IsNullOrEmpty(@string))
		{
			return;
		}
		string[] array = @string.Split('|');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(':');
			if (array2 != null && Enum.IsDefined(typeof(DownloadStateType), array2[0]))
			{
				DownloadStateType type = (DownloadStateType)Enum.Parse(typeof(DownloadStateType), array2[0]);
				mCurrentWarningData.Add(new WarningDataSetting(type, array2[1]));
			}
		}
	}

	private void Save()
	{
		string text = "";
		for (int i = 0; i < mCurrentWarningData.Count; i++)
		{
			text = text + ((i > 0) ? "|" : "") + mCurrentWarningData[i]._Type.ToString() + ":" + mCurrentWarningData[i]._Value;
		}
		PlayerPrefs.SetString("PrefetchWarning", text);
	}

	public bool CheckWarning(string[] checkList)
	{
		if (checkList == null || PrefetchManager.pInstance.pDownloadBundleCount == 0)
		{
			return false;
		}
		foreach (string value in checkList)
		{
			if (string.IsNullOrEmpty(value) || !Enum.IsDefined(typeof(DownloadStateType), value))
			{
				continue;
			}
			DownloadStateType type = (DownloadStateType)Enum.Parse(typeof(DownloadStateType), value);
			WarningDataSetting warningDataSetting = mCurrentWarningData.Find((WarningDataSetting entry) => entry._Type == type);
			switch (type)
			{
			case DownloadStateType.INSTALL:
				if (warningDataSetting == null)
				{
					return true;
				}
				break;
			case DownloadStateType.DOWNLOAD_REQUIRED:
				if (PrefetchManager.pInstance.pDownloadBundleCount > 0)
				{
					return true;
				}
				break;
			case DownloadStateType.BUNDLE_UPDATE:
				if (warningDataSetting == null)
				{
					return true;
				}
				if (!warningDataSetting._Value.Contains(ProductConfig.GetBundleQuality()))
				{
					return true;
				}
				break;
			case DownloadStateType.LANGUAGE_UPDATE:
				if (warningDataSetting == null)
				{
					return true;
				}
				if (!warningDataSetting._Value.Contains(UtUtilities.GetLocaleLanguage()))
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	public void SaveDefaults(string[] checkList)
	{
		if (checkList == null)
		{
			return;
		}
		foreach (string value in checkList)
		{
			if (string.IsNullOrEmpty(value) || !Enum.IsDefined(typeof(DownloadStateType), value))
			{
				continue;
			}
			DownloadStateType type = (DownloadStateType)Enum.Parse(typeof(DownloadStateType), value);
			WarningDataSetting warningDataSetting = mCurrentWarningData.Find((WarningDataSetting entry) => entry._Type == type);
			switch (type)
			{
			case DownloadStateType.INSTALL:
				if (warningDataSetting == null)
				{
					mCurrentWarningData.Add(new WarningDataSetting(DownloadStateType.INSTALL, ""));
				}
				break;
			case DownloadStateType.BUNDLE_UPDATE:
				if (warningDataSetting == null)
				{
					mCurrentWarningData.Add(new WarningDataSetting(DownloadStateType.BUNDLE_UPDATE, ProductConfig.GetBundleQuality()));
				}
				else if (!warningDataSetting._Value.Contains(ProductConfig.GetBundleQuality()))
				{
					warningDataSetting._Value += ProductConfig.GetBundleQuality();
				}
				break;
			case DownloadStateType.LANGUAGE_UPDATE:
				if (warningDataSetting == null)
				{
					mCurrentWarningData.Add(new WarningDataSetting(DownloadStateType.LANGUAGE_UPDATE, UtUtilities.GetLocaleLanguage()));
				}
				else if (!warningDataSetting._Value.Contains(UtUtilities.GetLocaleLanguage()))
				{
					warningDataSetting._Value += UtUtilities.GetLocaleLanguage();
				}
				break;
			}
		}
		Save();
	}

	public static void Kill()
	{
		mInstance = null;
	}
}
