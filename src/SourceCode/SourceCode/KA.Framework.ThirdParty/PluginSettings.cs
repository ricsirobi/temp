using UnityEngine;

namespace KA.Framework.ThirdParty;

public class PluginSettings : ScriptableObject
{
	public PluginData[] _Plugins;

	private bool mInitialized;

	private static PluginSettings mInstance;

	private static PluginSettings pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = (PluginSettings)RsResourceManager.LoadAssetFromResources("PluginSettings.asset", isPrefab: false);
				if (mInstance == null)
				{
					UtDebug.Log("PluginSettings.asset is null");
					mInstance = ScriptableObject.CreateInstance<PluginSettings>();
				}
			}
			return mInstance;
		}
	}

	private BuildType GetCurrentType()
	{
		return BuildType.FULL;
	}

	public static KeyData GetKeyData(Plugin plugin)
	{
		PluginData[] plugins = pInstance._Plugins;
		foreach (PluginData pluginData in plugins)
		{
			if (pluginData._Plugin != plugin)
			{
				continue;
			}
			PluginKey[] keys = pluginData._Keys;
			foreach (PluginKey pluginKey in keys)
			{
				if (ProductSettings.GetPlatform() == pluginKey._Platform && pInstance.GetCurrentType() == pluginKey._Type && (pluginKey._Environment == Environment.ALL || ProductSettings.GetEnvironment() == pluginKey._Environment))
				{
					return pluginKey._KeyData;
				}
			}
		}
		return null;
	}

	public static KeyData GetKeyData(Plugin inPlugin, int inMaxAge)
	{
		PluginData[] plugins = pInstance._Plugins;
		foreach (PluginData pluginData in plugins)
		{
			if (pluginData._Plugin != inPlugin)
			{
				continue;
			}
			PluginKey[] keys = pluginData._Keys;
			foreach (PluginKey pluginKey in keys)
			{
				if (ProductSettings.GetPlatform() == pluginKey._Platform && pInstance.GetCurrentType() == pluginKey._Type && (pluginKey._Environment == Environment.ALL || ProductSettings.GetEnvironment() == pluginKey._Environment) && inMaxAge <= pluginKey._MaxAge)
				{
					return pluginKey._KeyData;
				}
			}
		}
		return null;
	}

	public static PluginParam[] GetPluginParams(Plugin inPlugin)
	{
		PluginData[] plugins = pInstance._Plugins;
		foreach (PluginData pluginData in plugins)
		{
			if (pluginData._Plugin != inPlugin)
			{
				continue;
			}
			PluginKey[] keys = pluginData._Keys;
			foreach (PluginKey pluginKey in keys)
			{
				if (ProductSettings.GetPlatform() == pluginKey._Platform && pInstance.GetCurrentType() == pluginKey._Type && (pluginKey._Environment == Environment.ALL || ProductSettings.GetEnvironment() == pluginKey._Environment))
				{
					return pluginKey._Params;
				}
			}
		}
		return null;
	}

	public static string GetPluginParamValue(Plugin inPlugin, PluginParamType paramType)
	{
		PluginParam[] pluginParams = GetPluginParams(inPlugin);
		if (pluginParams != null)
		{
			PluginParam[] array = pluginParams;
			foreach (PluginParam pluginParam in array)
			{
				if (pluginParam._Name == paramType)
				{
					return pluginParam._Value;
				}
			}
		}
		return string.Empty;
	}
}
