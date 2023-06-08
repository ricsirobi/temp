using System.Collections.Generic;
using UnityEngine;

namespace KA.Framework;

public class ProductSettings : ScriptableObject
{
	public int _ProductGroupID = 14;

	public int _SubscriptionID = -3;

	public int _PairDataID;

	public string _AppName = "";

	public string _Token = "6e9b6ab7-6e10-4b82-8daf-5f1073dbf1ed";

	public string _Resource = "";

	public string _AssetVersionFileName;

	public string _LastP4CLFileName = "LastP4Changelist";

	public string[] _BundledScenes = new string[1] { "Assets/Startup/Startup.unity" };

	public List<EnvironmentDetails> _EnvironmentDetails = new List<EnvironmentDetails>();

	public List<ProductDetails> _ProductDetails = new List<ProductDetails>();

	public string[] _NonBundledLevels;

	public string[] _PreloadBundles;

	public List<KeyDataPair> _CustomKeys = new List<KeyDataPair>();

	public bool _EnableEnvironmentSelectionInEditor;

	private bool mInitialized;

	public int _MetroFrameDelay = 80;

	public string[] _BundlesToDelay;

	private static ProductSettings mInstance;

	public Environment pEnvironment
	{
		get
		{
			if (ProductConfig.pServerType == "D")
			{
				return Environment.DEV;
			}
			if (ProductConfig.pServerType == "Q")
			{
				return Environment.QA;
			}
			if (ProductConfig.pServerType == "S")
			{
				return Environment.STAGING;
			}
			return Environment.LIVE;
		}
	}

	public string pXMLPath
	{
		get
		{
			EnvironmentDetails environmentDetails = GetEnvironmentDetails();
			if (environmentDetails != null)
			{
				return environmentDetails._MainXMLPath;
			}
			return string.Empty;
		}
	}

	public string pXMLSecret
	{
		get
		{
			EnvironmentDetails environmentDetails = GetEnvironmentDetails();
			if (environmentDetails != null)
			{
				return environmentDetails._Secret;
			}
			return string.Empty;
		}
	}

	public static ProductSettings pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = Resources.Load("ProductSettings") as ProductSettings;
				if (mInstance == null)
				{
					mInstance = ScriptableObject.CreateInstance<ProductSettings>();
				}
			}
			return mInstance;
		}
	}

	public string GetCustomKeyValue(string inKey)
	{
		return _CustomKeys.Find((KeyDataPair v) => v._Key == inKey)?._Value;
	}

	public static ProductPlatform GetPlatform()
	{
		if (UtPlatform.IsiOS())
		{
			return ProductPlatform.IOS;
		}
		if (UtPlatform.IsAmazon())
		{
			return ProductPlatform.ANDROID_AMAZON;
		}
		if (UtPlatform.IsHuawei())
		{
			return ProductPlatform.ANDROID_HUAWEI;
		}
		if (UtPlatform.IsAndroid())
		{
			return ProductPlatform.ANDROID_GOOGLE;
		}
		if (UtPlatform.IsWSA())
		{
			return ProductPlatform.WSA;
		}
		if (UtPlatform.IsSteamMac())
		{
			return ProductPlatform.STANDALONE_STEAM_OSX;
		}
		if (UtPlatform.IsSteam())
		{
			return ProductPlatform.STANDALONE_STEAM_WIN;
		}
		if (UtPlatform.IsStandaloneOSX())
		{
			return ProductPlatform.STANDALONE_OSX;
		}
		if (UtPlatform.IsStandaloneWindows())
		{
			return ProductPlatform.STANDALONE_WIN;
		}
		if (UtPlatform.IsXBox())
		{
			return ProductPlatform.XBOXONE;
		}
		return ProductPlatform.UNKNOWN;
	}

	public static Environment GetEnvironment()
	{
		Environment environment = Environment.UNKNOWN;
		string pServerType = ProductConfig.pServerType;
		switch (pServerType)
		{
		default:
			if (pServerType.Length != 0)
			{
				break;
			}
			return Environment.LIVE;
		case "D":
			return Environment.DEV;
		case "Q":
			return Environment.QA;
		case "SOD":
			return Environment.SODSTAGING;
		case "S":
			return Environment.STAGING;
		case null:
			break;
		}
		return Environment.UNKNOWN;
	}

	public EnvironmentDetails GetEnvironmentDetails()
	{
		Environment environment = GetEnvironment();
		return _EnvironmentDetails.Find((EnvironmentDetails e) => e._Environment == environment);
	}

	public EnvironmentDetails GetEnvironmentDetails(Environment environment)
	{
		return _EnvironmentDetails.Find((EnvironmentDetails e) => e._Environment == environment);
	}

	public ProductDetails GetProductDetails()
	{
		return GetProductDetails(GetPlatform());
	}

	public ProductDetails GetProductDetails(ProductPlatform platform)
	{
		return _ProductDetails.Find((ProductDetails p) => p._Platform == platform);
	}

	public ProductDetails GetProductDetails(int productID)
	{
		return _ProductDetails.Find((ProductDetails p) => p._ProductID == productID);
	}

	public string GetChangelistNumber()
	{
		string text = string.Empty;
		TextAsset textAsset = (TextAsset)Resources.Load(_LastP4CLFileName, typeof(TextAsset));
		if (textAsset != null)
		{
			text = text + "CL" + textAsset.text;
		}
		return text;
	}
}
