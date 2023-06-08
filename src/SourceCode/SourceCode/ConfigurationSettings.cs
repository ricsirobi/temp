using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "cs", Namespace = "")]
public class ConfigurationSettings
{
	[XmlElement(ElementName = "kvp")]
	public List<ConfigurationSetting> Settings;

	private static ConfigurationSettings mInstance;

	private static bool mInitialized;

	public static ConfigurationSettings pInstance => mInstance;

	public static bool pIsReady => mInstance != null;

	public ConfigurationSettings()
	{
		Settings = new List<ConfigurationSetting>();
	}

	public static void Init()
	{
		if (!mInitialized && ProductConfig.pIsReady)
		{
			mInitialized = true;
			WsWebService.GetConfigurationSettings(WsGetEventHandler, null);
		}
	}

	public static void InitDefault()
	{
		mInstance = new ConfigurationSettings();
		mInstance.Settings = new List<ConfigurationSetting>();
	}

	public static TYPE GetAttribute<TYPE>(string attribute, TYPE defaultValue)
	{
		if (mInstance.Settings == null)
		{
			return defaultValue;
		}
		foreach (ConfigurationSetting setting in mInstance.Settings)
		{
			if (!(setting.SettingKey == attribute))
			{
				continue;
			}
			Type typeFromHandle = typeof(TYPE);
			if (typeFromHandle.Equals(typeof(int)))
			{
				return (TYPE)(object)int.Parse(setting.SettingValue);
			}
			if (typeFromHandle.Equals(typeof(float)))
			{
				return (TYPE)(object)float.Parse(setting.SettingValue);
			}
			if (typeFromHandle.Equals(typeof(bool)))
			{
				if (setting.SettingValue.Equals("t", StringComparison.OrdinalIgnoreCase) || setting.SettingValue.Equals("1", StringComparison.OrdinalIgnoreCase) || setting.SettingValue.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					return (TYPE)(object)true;
				}
				if (setting.SettingValue.Equals("f", StringComparison.OrdinalIgnoreCase) || setting.SettingValue.Equals("0", StringComparison.OrdinalIgnoreCase) || setting.SettingValue.Equals("false", StringComparison.OrdinalIgnoreCase))
				{
					return (TYPE)(object)false;
				}
			}
			else
			{
				if (typeFromHandle.Equals(typeof(string)))
				{
					return (TYPE)(object)setting.SettingValue;
				}
				if (typeFromHandle.Equals(typeof(DateTime)))
				{
					return (TYPE)(object)DateTime.Parse(setting.SettingValue, UtUtilities.GetCultureInfo("en-US"));
				}
			}
		}
		return defaultValue;
	}

	public static void WsGetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			mInstance = (ConfigurationSettings)inObject;
			if (mInstance == null)
			{
				InitDefault();
				UtDebug.LogError("WEB SERVICE CALL GetConfigurationSettings RETURNED NO DATA!!!");
			}
			break;
		case WsServiceEvent.ERROR:
			InitDefault();
			UtDebug.LogError("WEB SERVICE CALL GetConfigurationSettings FAILED!!!");
			break;
		}
	}
}
