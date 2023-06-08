using System;
using System.Collections.Generic;
using JSGames.Platform.PlayFab;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;

public class PlayFabAnalyticsAgent : MonoBehaviour
{
	[Serializable]
	public class SceneEventsMap
	{
		public string SceneName;

		public List<EventData> EventData;
	}

	[Serializable]
	public class EventData
	{
		public AnalyticEvent Event;

		public string EventParam;
	}

	public List<SceneEventsMap> _SceneEventMap;

	private const string mAgentName = "Playfab";

	private JSGames.Platform.PlayFab.AnalyticAgent mPlayFabAgent = new JSGames.Platform.PlayFab.AnalyticAgent();

	private readonly char mCharToReplace = ' ';

	private readonly char mCharSeparator = '_';

	private bool mRegisterMissionEvent;

	private void Start()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		CommonInventoryData.PurchaseSuccessCallback = (InventoryEventHandler)Delegate.Combine(CommonInventoryData.PurchaseSuccessCallback, new InventoryEventHandler(OnItemPurchase));
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		CommonInventoryData.PurchaseSuccessCallback = (InventoryEventHandler)Delegate.Remove(CommonInventoryData.PurchaseSuccessCallback, new InventoryEventHandler(OnItemPurchase));
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
	}

	private void OnItemPurchase(WsServiceType type, Dictionary<int, CommonInventoryRequest> requests)
	{
		if (type != WsServiceType.PURCHASE_ITEMS)
		{
			return;
		}
		foreach (KeyValuePair<int, CommonInventoryRequest> request in requests)
		{
			_ = request;
		}
	}

	public void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		CharacterEvent("LoadScene", new Dictionary<string, object> { { "SceneName", scene.name } });
		if (_SceneEventMap == null || _SceneEventMap.Count <= 0)
		{
			return;
		}
		SceneEventsMap sceneEventsMap = _SceneEventMap.Find((SceneEventsMap entry) => entry.SceneName.Equals(scene.name));
		if (sceneEventsMap == null || sceneEventsMap.EventData == null || sceneEventsMap.EventData.Count <= 0)
		{
			return;
		}
		foreach (EventData eventDatum in sceneEventsMap.EventData)
		{
			_ = eventDatum;
		}
	}

	private void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		switch (inEvent)
		{
		case MissionEvent.TASK_COMPLETE:
			_ = (Task)inObject;
			break;
		case MissionEvent.MISSION_COMPLETE:
			_ = (Mission)inObject;
			break;
		}
	}

	public void LogEvent(string inEventName, Dictionary<string, object> inParameter)
	{
	}

	private string CorrectEventName(string inEventName)
	{
		return inEventName.Replace(mCharToReplace, mCharSeparator);
	}

	public void LogEvent(string inEventName, Dictionary<string, string> inParameter)
	{
	}

	public void PurchaseEvent(string inEventName, Product product)
	{
	}

	public string GetAgentName()
	{
		return "Playfab";
	}

	public void LogEvent(AnalyticEvent inEventID, Dictionary<string, object> inParameter)
	{
	}

	public void TitleEvent(string inEventName, Dictionary<string, object> inParameter)
	{
		if (inParameter == null)
		{
			UtDebug.Log("Unable to register title event");
		}
		else
		{
			mPlayFabAgent.SendTitleEvent(inEventName, inParameter);
		}
	}

	private void PlayerEvent(string inEventName, Dictionary<string, object> inParameter)
	{
		if (inParameter == null)
		{
			UtDebug.Log("Unable to register player event");
		}
		else
		{
			mPlayFabAgent.SendPlayerEvent(CorrectEventName(inEventName), inParameter);
		}
	}

	private void CharacterEvent(string inEventName, Dictionary<string, object> inParameter)
	{
		if (PlayfabManager<PlayFabManagerDO>.Instance == null || PlayfabManager<PlayFabManagerDO>.Instance.GetCurrentCharacterID() == null || inParameter == null)
		{
			UtDebug.Log("Unable to register character event");
		}
		else
		{
			mPlayFabAgent.SendCharacterEvent(CorrectEventName(inEventName), PlayfabManager<PlayFabManagerDO>.Instance.GetCurrentCharacterID(), inParameter);
		}
	}

	public void LogFTUEEvent(FTUEEvent inEventID, string ID, int stepIndex, Dictionary<string, object> inParameter)
	{
	}
}
