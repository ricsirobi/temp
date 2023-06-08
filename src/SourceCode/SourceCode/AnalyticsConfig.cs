using System.Collections.Generic;
using UnityEngine;

public class AnalyticsConfig : ScriptableObject
{
	private static AnalyticsConfig mInstance;

	public List<FTUEEvent> _ActiveFTUEEvents = new List<FTUEEvent>();

	public static AnalyticsConfig pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = RsResourceManager.LoadAssetFromResources("AnalyticsConfig.asset", isPrefab: false) as AnalyticsConfig;
			}
			return mInstance;
		}
	}
}
