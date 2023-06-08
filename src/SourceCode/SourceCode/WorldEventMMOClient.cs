using System;
using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using KnowledgeAdventure.Multiplayer.Model;
using UnityEngine;

public class WorldEventMMOClient : MMOClient
{
	public delegate void OnMessage(string name, string value);

	public const string EVENT_VAR_PREFIX = "WE_";

	public const string NEXT_EVENT_VAR_PREFIX = "WEN_";

	public const string HEALTH_VAR_PREFIX = "WEH_";

	public const string AI_CONTROLLER = "WE__AI";

	public const string SERVER_EVENT_END = "WEE_";

	public const string EVENT_SCORE = "WE_SC";

	public const string EVENT_STATUS_RESPONSE = "WESR";

	public const string EVENT_END_RESPONSE = "EvEnd";

	public const string EVENT_STATUS = "wex.WES";

	public const string AI_ACKNOWLEDGEMENT = "wex.AIACK";

	public const string AI_PING = "wex.AIP";

	public const string EVENT_TIME_SPAN = "wex.ETS";

	public const string EVENT_END_SUFFIX = "_End";

	public const string WEAPON_PREFIX = "WE_Weapon_";

	public const string WEAPON_FLARE_PREFIX = "WEF_";

	protected WorldEventManager mWorldEventManager;

	protected Dictionary<string, OnMessage> mMessageListener = new Dictionary<string, OnMessage>();

	protected Action mAction;

	public static WorldEventMMOClient Init(WorldEventManager manager)
	{
		if (MainStreetMMOClient.pInstance == null)
		{
			return null;
		}
		GameObject gameObject = new GameObject("WorldEventMMOClient");
		WorldEventMMOClient client = gameObject.AddComponent<WorldEventMMOClient>();
		client.mWorldEventManager = manager;
		WorldEventMMOClient worldEventMMOClient = client;
		worldEventMMOClient.mAction = (Action)Delegate.Combine(worldEventMMOClient.mAction, (Action)delegate
		{
			MainStreetMMOClient.pInstance.AddRoomVariableEventHandler(client.RoomVariableHandler);
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("WESR", client.ResponseEventHandler);
		});
		MainStreetMMOClient.AddClient(client);
		return client;
	}

	public void AddListener(string message, OnMessage listener)
	{
		mMessageListener.Add(message, listener);
	}

	public void RemoveListener(string message)
	{
		if (mMessageListener.ContainsKey(message))
		{
			mMessageListener.Remove(message);
		}
	}

	protected virtual void Update()
	{
		if (!(MMOTimeManager.pInstance == null) && MMOTimeManager.pInstance.pIsTimeSynced && mAction != null)
		{
			mAction();
			mAction = null;
		}
	}

	private void OnDestroy()
	{
		MainStreetMMOClient.pInstance.RemoveExtensionResponseEventHandler("wex.WES", ResponseEventHandler);
		MainStreetMMOClient.RemoveClient(this);
	}

	public override void OnJoinedRoom(MMOJoinedRoomEventArgs inJoinedRoomArgs)
	{
		base.OnJoinedRoom(inJoinedRoomArgs);
		mAction = (Action)Delegate.Combine(mAction, (Action)delegate
		{
			SendExtensionMessage("wex.WES", new Dictionary<string, object>());
		});
	}

	public override void Disconnected()
	{
		base.Disconnected();
		WorldEventManager.pInstance.ResetEvent(resetPreviousEventAlso: true);
	}

	public void SendExtensionMessage(string inMessage, Dictionary<string, object> inParams)
	{
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SendExtensionMessage(inMessage, inParams);
		}
	}

	public void SendPingMessage(double ping)
	{
		MainStreetMMOClient.pInstance.SendPingMessage(ping);
	}

	private void RoomVariableHandler(object sender, MMORoomVariablesChangedEventArgs args)
	{
		if (args == null || args.RoomChanged == null || args.RoomChanged.RoomVariables == null)
		{
			return;
		}
		if (KAConsole.pUnlocked)
		{
			for (int i = 0; i < args.ChangedVariableKeys.Count; i++)
			{
				KAConsole.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + ": Changed = " + args.ChangedVariableKeys[i].ToString());
			}
		}
		ParseRoomVariables(args.RoomChanged.RoomVariables, args.ChangedVariableKeys);
		CheckForEnd(args);
	}

	public virtual void ParseRoomVariables(List<MMORoomVariable> roomVars, List<object> changedKeys)
	{
		if (roomVars == null)
		{
			return;
		}
		for (int i = 0; i < roomVars.Count; i++)
		{
			MMORoomVariable mMORoomVariable = roomVars[i];
			if (mMORoomVariable == null)
			{
				continue;
			}
			try
			{
				string text = mMORoomVariable.Name.ToString();
				string value = mMORoomVariable.Value.ToString();
				string key = "";
				if (mMessageListener.ContainsKey(text))
				{
					key = text;
				}
				else if (text.StartsWith("WE_"))
				{
					key = "WE_";
				}
				else if (text.StartsWith("WEH_"))
				{
					key = "WEH_";
				}
				else if (text.StartsWith("WEF_"))
				{
					key = "WEF_";
				}
				if (mMessageListener.ContainsKey(key))
				{
					mMessageListener[key](text, value);
				}
			}
			catch (Exception ex)
			{
				UtDebug.LogError(ex.Message);
			}
		}
	}

	public void CheckForEnd(MMORoomVariablesChangedEventArgs args)
	{
		if (args == null || args.ChangedVariableKeys == null)
		{
			return;
		}
		for (int i = 0; i < args.ChangedVariableKeys.Count; i++)
		{
			string text = args.ChangedVariableKeys[i].ToString();
			if (text == null || !text.EndsWith("_End"))
			{
				continue;
			}
			for (int j = 0; j < args.RoomChanged.RoomVariables.Count; j++)
			{
				if (args.RoomChanged.RoomVariables[j] == null)
				{
					continue;
				}
				string text2 = args.RoomChanged.RoomVariables[j].Name.ToString();
				if (text2.EndsWith("_End"))
				{
					string value = args.RoomChanged.RoomVariables[j].Value.ToString();
					if (mMessageListener.ContainsKey("_End"))
					{
						mMessageListener["_End"](text2, value);
					}
					break;
				}
			}
		}
	}

	protected virtual void ResponseEventHandler(object sender, MMOExtensionResponseReceivedEventArgs args)
	{
		if (args == null || sender == null)
		{
			return;
		}
		Dictionary<string, object> responseDataObject = args.ResponseDataObject;
		if (responseDataObject == null || !responseDataObject.ContainsKey("0"))
		{
			return;
		}
		string text = responseDataObject["0"].ToString();
		UtDebug.Log("WE_ cmd : " + text);
		if (!(text == "WESR"))
		{
			return;
		}
		string text2 = string.Empty;
		string text3 = string.Empty;
		for (int i = 1; i < responseDataObject.Count; i++)
		{
			string text4 = responseDataObject[i.ToString()].ToString();
			if (text4.Contains("EvEnd"))
			{
				text3 = text4;
			}
			else
			{
				text2 = text4;
			}
		}
		if (string.IsNullOrEmpty(text2) && string.IsNullOrEmpty(text3))
		{
			List<MMORoomVariable> subscribedRoomVariables = MainStreetMMOClient.pInstance.GetSubscribedRoomVariables();
			ParseRoomVariables(subscribedRoomVariables, null);
		}
		else
		{
			WorldEventManager.pInstance.InitEventFromResponse(text2, text3);
		}
	}

	public static void TriggerEvent(string eventName, int time)
	{
		string key = "WE_" + eventName;
		DateTime dateTime = MMOTimeManager.pInstance.GetServerDateTimeMilliseconds().AddSeconds(time);
		if (MainStreetMMOClient.pInstance != null)
		{
			string value = dateTime.ToString() + "," + MakeUID() + "," + true;
			MainStreetMMOClient.pInstance.SetRoomVariable(key, value);
		}
	}

	public static void StopEvents()
	{
		if (!(WorldEventManager.pInstance != null))
		{
			return;
		}
		for (int i = 0; i < WorldEventManager.pInstance._WorldEvent.Length; i++)
		{
			if (WorldEventManager.pInstance._WorldEvent[i]._State == WorldEventState.ACTIVE)
			{
				WorldEventManager.pInstance.OnEventEnd(WorldEventManager.pInstance._WorldEvent[i], destroyObjects: true, null);
			}
		}
	}

	public static string MakeUID()
	{
		return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8);
	}
}
