using System;
using System.Xml.Serialization;
using UnityEngine;

public class UtPlatform
{
	[Serializable]
	public enum PlatformType
	{
		[XmlEnum("0")]
		Unknown,
		[XmlEnum("1")]
		WebGL,
		[XmlEnum("2")]
		iOS,
		[XmlEnum("3")]
		Android,
		[XmlEnum("4")]
		WSA,
		[XmlEnum("5")]
		Standalone,
		[XmlEnum("6")]
		Steam,
		[XmlEnum("7")]
		XBox
	}

	[Serializable]
	public enum DeviceType
	{
		[XmlEnum("0")]
		Unknown,
		[XmlEnum("1")]
		iPhone,
		[XmlEnum("2")]
		iPad
	}

	public static int ForcedAvatarLimit = -1;

	public static int MaxFullMMO = -1;

	public static int pDeviceTypeID => (int)GetDeviceType();

	public static int pPlatformID => (int)GetPlatformType();

	private static DeviceType GetDeviceType()
	{
		return DeviceType.Unknown;
	}

	public static PlatformType GetPlatformType()
	{
		if (IsiOS())
		{
			return PlatformType.iOS;
		}
		if (IsAndroid())
		{
			return PlatformType.Android;
		}
		if (IsWSA())
		{
			return PlatformType.WSA;
		}
		if (IsSteam())
		{
			return PlatformType.Steam;
		}
		if (IsStandAlone())
		{
			return PlatformType.Standalone;
		}
		if (IsXBox())
		{
			return PlatformType.XBox;
		}
		if (IsWebGL())
		{
			return PlatformType.WebGL;
		}
		return PlatformType.Unknown;
	}

	public static bool IsMobile()
	{
		if (IsiOS() || IsAndroid())
		{
			return true;
		}
		return false;
	}

	public static bool IsiOS()
	{
		return false;
	}

	public static bool IsAndroid()
	{
		return false;
	}

	public static bool IsAmazon()
	{
		return false;
	}

	public static bool IsHuawei()
	{
		return false;
	}

	public static bool IsWSA()
	{
		return false;
	}

	public static bool IsEditor()
	{
		return false;
	}

	public static bool IsStandAlone()
	{
		return true;
	}

	public static bool IsStandaloneOSX()
	{
		return false;
	}

	public static bool IsStandaloneWindows()
	{
		return true;
	}

	public static bool IsSteam()
	{
		return false;
	}

	public static bool IsSteamWindows()
	{
		return false;
	}

	public static bool IsSteamMac()
	{
		return false;
	}

	public static bool IsXBox()
	{
		return false;
	}

	public static bool IsWebGL()
	{
		return false;
	}

	public static bool IsHandheldDevice()
	{
		return true;
	}

	public static bool IsTouchInput()
	{
		if (KAInput.pInstance.pInputMode == KAInputMode.TOUCH)
		{
			return true;
		}
		return false;
	}

	public static bool IsRealTimeShadowEnabled()
	{
		if (ProductConfig.GetShadowQuality() == "High")
		{
			return SystemInfo.supportsShadows;
		}
		return false;
	}

	public static bool GetMMODefaultState()
	{
		return ProductConfig.GetPlatformSettings()?.MMODefaultState ?? true;
	}

	public static int GetDeviceAvatarLimit()
	{
		if (ForcedAvatarLimit >= 0)
		{
			return ForcedAvatarLimit;
		}
		PlatformSettings platformSettings = ProductConfig.GetPlatformSettings();
		if (platformSettings != null && platformSettings.MaxMMOData != null)
		{
			if (platformSettings.MaxMMOData.SceneData != null && !string.IsNullOrEmpty(RsResourceManager.pCurrentLevel))
			{
				MaxMMOData.MMOScene[] sceneData = platformSettings.MaxMMOData.SceneData;
				for (int i = 0; i < sceneData.Length; i++)
				{
					if (sceneData[i].Scene.Equals(RsResourceManager.pCurrentLevel, StringComparison.OrdinalIgnoreCase))
					{
						return sceneData[i].MaxMMO;
					}
				}
			}
			return platformSettings.MaxMMOData.DefaultMaxMMO;
		}
		return 5;
	}

	public static int GetDeviceMaxFullMMO()
	{
		if (MaxFullMMO > 0)
		{
			return MaxFullMMO;
		}
		PlatformSettings platformSettings = ProductConfig.GetPlatformSettings();
		if (platformSettings != null && platformSettings.MaxMMOData != null)
		{
			if (platformSettings.MaxMMOData.SceneData != null && !string.IsNullOrEmpty(RsResourceManager.pCurrentLevel))
			{
				MaxMMOData.MMOScene[] sceneData = platformSettings.MaxMMOData.SceneData;
				for (int i = 0; i < sceneData.Length; i++)
				{
					if (sceneData[i].Scene.Equals(RsResourceManager.pCurrentLevel, StringComparison.OrdinalIgnoreCase))
					{
						return sceneData[i].MaxFullMMO;
					}
				}
			}
			return platformSettings.MaxMMOData.DefaultMaxFullMMO;
		}
		return 0;
	}

	public static string GetPlatformName()
	{
		return "PC";
	}

	public static string GetDeviceModel()
	{
		return SystemInfo.deviceModel;
	}

	public static bool PlatformCanBeUsed(string[] platform, string name)
	{
		if (platform == null || platform.Length == 0)
		{
			return true;
		}
		int i = 0;
		for (int num = platform.Length; i < num; i++)
		{
			if (!string.IsNullOrEmpty(platform[i]) && name.Contains(platform[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static string GetCurrentPlatformFolderSuffix()
	{
		if (IsiOS())
		{
			return "iOS";
		}
		if (IsAndroid())
		{
			return "Android";
		}
		if (IsWSA())
		{
			return "WSA";
		}
		if (IsStandAlone())
		{
			if (IsSteamWindows())
			{
				Debug.Log("Returning Steam");
				return "Steam";
			}
			if (IsSteamMac())
			{
				return "SteamMac";
			}
			if (IsStandaloneOSX())
			{
				return "Mac";
			}
			if (IsStandaloneWindows())
			{
				return "Win";
			}
			return "StandaloneUnknown";
		}
		if (IsXBox())
		{
			return "XBox";
		}
		if (IsWebGL())
		{
			return "WebGL";
		}
		return "Unknown";
	}

	public static bool IsImageEffectSupported()
	{
		return SystemInfo.supportsImageEffects;
	}

	public static string GetGPUName()
	{
		return SystemInfo.graphicsDeviceName;
	}
}
