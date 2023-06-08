using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Purchasing;

public class AnalyticAgent : KAMonoBase
{
	private const string CURRENT_LOCATION = "CurrentLocation";

	private static List<IAnalyticAgent> mAgents = new List<IAnalyticAgent>();

	private static AnalyticAgent mInstance;

	public static bool pOptedOut
	{
		get
		{
			for (int i = 0; i < mAgents.Count; i++)
			{
				if (!mAgents[i].pOptedOut)
				{
					return false;
				}
			}
			return true;
		}
	}

	private void Awake()
	{
		if (mInstance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		mInstance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public static void RegisterAgent(IAnalyticAgent agent)
	{
		if (!mAgents.Contains(agent))
		{
			mAgents.Add(agent);
		}
	}

	public static void UnregisterAgent(IAnalyticAgent agent)
	{
		IAnalyticAgent analyticAgent = mAgents.Find((IAnalyticAgent p) => p == agent);
		if (analyticAgent != null)
		{
			mAgents.Remove(analyticAgent);
		}
	}

	public static void LogEvent(AnalyticEvent inEventId, Dictionary<string, object> inParameter = null)
	{
		LogEvent(GetDescription(inEventId), inParameter);
	}

	public static void LogEvent(string inEventName, Dictionary<string, object> inParameter = null)
	{
		for (int i = 0; i < mAgents.Count; i++)
		{
			mAgents[i].LogEvent(inEventName, inParameter);
		}
	}

	public static void LogEvent(string agentName, AnalyticEvent inEventId, Dictionary<string, string> inParameter = null)
	{
		for (int i = 0; i < mAgents.Count; i++)
		{
			if (agentName.Equals(mAgents[i].GetAgentName()))
			{
				mAgents[i].LogEvent(GetDescription(inEventId), inParameter);
			}
		}
	}

	public static void LogFTUEEvent(FTUEEvent inEventID, Dictionary<string, object> inParameter = null)
	{
		if (!AnalyticsConfig.pInstance || AnalyticsConfig.pInstance._ActiveFTUEEvents.Contains(inEventID))
		{
			for (int i = 0; i < mAgents.Count; i++)
			{
				mAgents[i].LogFTUEEvent(inEventID, inParameter);
			}
		}
	}

	public static void PurchaseEvent(string agentName, AnalyticEvent inEventId, Product product)
	{
		for (int i = 0; i < mAgents.Count; i++)
		{
			if (agentName.Equals(mAgents[i].GetAgentName()))
			{
				mAgents[i].PurchaseEvent(GetDescription(inEventId), product);
			}
		}
	}

	public static string GetDescription(Enum value)
	{
		if (Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()), typeof(DescriptionAttribute)) is DescriptionAttribute descriptionAttribute)
		{
			return descriptionAttribute.Description;
		}
		return value.ToString();
	}

	public static void OptOut(Action onSuccess, Action<string> onFailure)
	{
		try
		{
			for (int i = 0; i < mAgents.Count; i++)
			{
				mAgents[i].OptOut();
			}
			onSuccess?.Invoke();
		}
		catch (Exception ex)
		{
			onFailure?.Invoke(ex.Message);
		}
	}

	private void OnApplicationPause(bool pause)
	{
	}
}
