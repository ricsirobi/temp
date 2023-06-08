using System;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;

public class UnityAnalyticsAgent : MonoBehaviour, IAnalyticAgent
{
	[Header("Max time in hours allowed between the server and local client for analytics to be valid")]
	public double _ClientHoursAheadOfServerMax = 23.0;

	private const string mAgentName = "Unity";

	private static bool mNewUser;

	private const string UNITY_ANALYTICS_OPT_OUT_KEY = "UnityAnalyticsOptOutStatus";

	public static bool pNewUser
	{
		get
		{
			return mNewUser;
		}
		set
		{
			mNewUser = value;
		}
	}

	private DateTime mActiveSessionTimestamp { get; set; }

	public bool pOptedOut => bool.Parse(PlayerPrefs.GetString("UnityAnalyticsOptOutStatus", false.ToString()));

	private void Awake()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene inScene, LoadSceneMode inLoadSceneMode)
	{
		if (mNewUser)
		{
			if (RsResourceManager.pCurrentLevel == "ProfileSelectionDO" && inScene.name != "Transition")
			{
				LogFTUEEvent(FTUEEvent.PROFILE_SELECTION_LOADED);
			}
			else if (RsResourceManager.pLastLevel == "ProfileSelectionDO" && inScene.name != "Transition" && inScene.name != "LoginDM")
			{
				LogFTUEEvent(FTUEEvent.FIRST_SCENE_LOADED);
			}
			else if (RsResourceManager.pLastLevel == "HubSchoolDO" && inScene.name != "Transition")
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("name", inScene.name);
				LogFTUEEvent(FTUEEvent.SCENE_LOADED, dictionary);
			}
		}
	}

	private void Start()
	{
		AnalyticAgent.RegisterAgent(this);
		if ((!PlayerPrefs.HasKey("GUEST_ACC_CREATED") && !PlayerPrefs.HasKey("REM_PASSWORD") && !PlayerPrefs.HasKey("FTUE_FIRSTLAUNCH")) || PlayerPrefs.HasKey("FTUE_NEWUSER"))
		{
			mNewUser = true;
			if (!PlayerPrefs.HasKey("FTUE_NEWUSER"))
			{
				PlayerPrefs.SetString("FTUE_NEWUSER", UiLogin.pUserName);
			}
			LogFTUEEvent(FTUEEvent.FIRST_LAUNCHED);
		}
	}

	private void OnDestroy()
	{
		AnalyticAgent.UnregisterAgent(this);
	}

	public void LogEvent(string inEventName, Dictionary<string, object> inParameter = null)
	{
		if (CanLogEvent())
		{
			if (inParameter == null)
			{
				inParameter = new Dictionary<string, object>();
			}
			AnalyticsService.Instance.CustomData(inEventName, inParameter);
		}
	}

	public void LogEvent(string inEventName, Dictionary<string, string> inParameter)
	{
	}

	public void PurchaseEvent(string inEventName, UnityEngine.Purchasing.Product product)
	{
	}

	public string GetAgentName()
	{
		return "Unity";
	}

	public void LogEvent(AnalyticEvent inEventID, Dictionary<string, object> inParameter)
	{
	}

	public void LogFTUEEvent(FTUEEvent inEventID, Dictionary<string, object> inParameter = null)
	{
		if (!CanLogEvent())
		{
			return;
		}
		bool flag = inEventID == FTUEEvent.STEP || inEventID == FTUEEvent.CUTSCENE_STARTED || inEventID == FTUEEvent.CUTSCENE_ENDED || inEventID == FTUEEvent.OFFER_CLOSED || inEventID == FTUEEvent.COLLECT_TASK;
		if ((flag || !PlayerPrefs.HasKey(inEventID.ToString())) && pNewUser)
		{
			if (inParameter == null)
			{
				inParameter = new Dictionary<string, object>();
			}
			inParameter.Add("ftueEvent", AnalyticAgent.GetDescription(inEventID));
			inParameter.Add("isGuest", PlayerPrefs.HasKey("GUEST_ACC_CREATED"));
			AnalyticsService.Instance.CustomData("FTUE", inParameter);
			AnalyticsService.Instance.Flush();
			if (!flag)
			{
				PlayerPrefs.SetString(inEventID.ToString(), "");
			}
		}
	}

	private bool CanLogEvent()
	{
		if (!ServerTime.pIsReady)
		{
			return true;
		}
		return !(DateTime.UtcNow - ServerTime.pCurrentTime > TimeSpan.FromHours(_ClientHoursAheadOfServerMax));
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			mActiveSessionTimestamp = DateTime.UtcNow;
			return;
		}
		int seconds = (DateTime.UtcNow - mActiveSessionTimestamp).Seconds;
		Dictionary<string, object> inParameter = new Dictionary<string, object> { { "duration", seconds } };
		LogEvent(AnalyticEvent.SESSION_ENDED, inParameter);
	}

	public void OptOut()
	{
		AnalyticsService.Instance.OptOut();
		PlayerPrefs.SetString("UnityAnalyticsOptOutStatus", true.ToString());
		PlayerPrefs.Save();
	}
}
