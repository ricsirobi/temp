using System;
using System.Collections;
using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using KnowledgeAdventure.Multiplayer.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldEventNotification : MonoBehaviour
{
	[Serializable]
	public class WorldEventInfo
	{
		public string _EventKey;

		public float _Duration;

		public Coroutine _NotifyEvent;

		public float _EventNotificationStartTime = 60f;
	}

	[Serializable]
	public class SceneInfo
	{
		public string _SceneName;

		public LocaleString _DisplayName;
	}

	public LocaleString _AboutToBeginMinsText = new LocaleString("A Battle Event starts in [[time]] minutes");

	public LocaleString _AboutToBeginSecsText = new LocaleString("A Battle Event starts in [[time]] seconds");

	public LocaleString _EventRunningText = new LocaleString("A Battle Event is going on");

	public LocaleString _EventLocationText = new LocaleString("go to [[place]]");

	public SceneInfo[] _SceneInfos;

	public static WorldEventNotification pInstance;

	public WorldEventInfo[] _WorldEvents;

	public List<string> _ExcludeSceneList = new List<string>();

	public float _SnoozeBefore = 30f;

	private bool mInitialized;

	private string mLastEventUID;

	private string mLastLevel;

	private MMORoomVariable mEventVar;

	private bool mProcessAfterTimeSync;

	private float mExpireTime;

	private float mTimeToEvent;

	private DateTime mTimeToStart;

	private DateTime mTimeToNextEvent;

	private DateTime mEventEndTime;

	private bool mNotified;

	private bool mWarned;

	private bool mEventStarted;

	private string mLevelToLoad;

	private MessageInfo mMessageInfo;

	private bool mEventNotificationActive;

	public DateTime TimeToStart => mTimeToStart;

	public DateTime TimeToNextEvent => mTimeToNextEvent;

	public DateTime EventEndTime => mEventEndTime;

	private bool pIsEventScene => RsResourceManager.pCurrentLevel == mLevelToLoad;

	private void Start()
	{
		if (pInstance == null)
		{
			pInstance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		mInitialized = false;
		mLastEventUID = string.Empty;
	}

	public void Reset()
	{
		mLastEventUID = string.Empty;
		if (mMessageInfo != null)
		{
			UiChatHistory.SystemMessageAccepted(mMessageInfo, removeAll: true);
		}
	}

	public WorldEventInfo GetEventNotificaiton(string inEventKey)
	{
		return Array.Find(_WorldEvents, (WorldEventInfo t) => t._EventKey.Equals(inEventKey));
	}

	private void Update()
	{
		if (MainStreetMMOClient.pIsReady)
		{
			if (!mInitialized && MainStreetMMOClient.pInstance != null)
			{
				List<MMORoomVariable> subscribedRoomVariables = MainStreetMMOClient.pInstance.GetSubscribedRoomVariables();
				ParseRoomVariables(subscribedRoomVariables);
				MainStreetMMOClient.pInstance.AddRoomVariableEventHandler(RoomVariableHandler);
				mInitialized = true;
			}
		}
		else
		{
			mInitialized = false;
		}
		if (mProcessAfterTimeSync && SanctuaryManager.pCurPetInstance != null && MMOTimeManager.pInstance != null && MMOTimeManager.pInstance.pIsTimeSynced)
		{
			ProcessEventVariable(mEventVar);
			mProcessAfterTimeSync = false;
			mEventVar = null;
		}
		if (null == MMOTimeManager.pInstance || !MMOTimeManager.pInstance.pIsTimeSynced || !mEventNotificationActive)
		{
			return;
		}
		if (!mEventStarted)
		{
			if (mTimeToEvent <= _SnoozeBefore)
			{
				if (mTimeToEvent < 0f)
				{
					ShowSystemMessage(0);
					mEventStarted = true;
				}
				else if (!mWarned)
				{
					mWarned = true;
					ShowSystemMessage((int)_SnoozeBefore);
				}
			}
			else if (!mNotified)
			{
				mNotified = true;
				ShowSystemMessage((int)mTimeToEvent);
			}
		}
		DateTime serverDateTimeMilliseconds = MMOTimeManager.pInstance.GetServerDateTimeMilliseconds();
		mTimeToEvent = (float)(mTimeToStart - serverDateTimeMilliseconds).TotalSeconds;
		mExpireTime = (float)(mEventEndTime - serverDateTimeMilliseconds).TotalSeconds;
		if (mExpireTime <= 0f)
		{
			if (mMessageInfo != null)
			{
				UiChatHistory.SystemMessageAccepted(mMessageInfo, removeAll: true);
				mMessageInfo = null;
			}
			mEventNotificationActive = false;
		}
	}

	private void ShowSystemMessage(int time)
	{
		string text = "";
		if (time > 0)
		{
			string timerString = UtUtilities.GetTimerString(time);
			text = ((time >= 60) ? _AboutToBeginMinsText.GetLocalizedString() : _AboutToBeginSecsText.GetLocalizedString());
			text = text.Replace("[[time]]", timerString);
		}
		else
		{
			text = _EventRunningText.GetLocalizedString();
		}
		if (mMessageInfo == null)
		{
			mMessageInfo = new MessageInfo();
		}
		UiChatHistory.AddSystemNotification(text, mMessageInfo, OnSystemMessageClicked, ignoreDuplicateMessage: false, pIsEventScene ? "" : _EventLocationText.GetLocalizedString().Replace("[[place]]", GetSceneDisplayName(mLevelToLoad)));
	}

	private void OnSystemMessageClicked(object messageObject)
	{
		if (mEventNotificationActive && !pIsEventScene)
		{
			AvAvatar.SetActive(inActive: false);
			RsResourceManager.LoadLevel(mLevelToLoad);
		}
	}

	private string GetSceneDisplayName(string inSceneName)
	{
		SceneInfo[] sceneInfos = _SceneInfos;
		foreach (SceneInfo sceneInfo in sceneInfos)
		{
			if (sceneInfo._SceneName == inSceneName)
			{
				return sceneInfo._DisplayName.GetLocalizedString();
			}
		}
		return null;
	}

	private void RoomVariableHandler(object sender, MMORoomVariablesChangedEventArgs args)
	{
		if (args != null && args.RoomChanged != null && args.RoomChanged.RoomVariables != null)
		{
			ParseRoomVariables(args.RoomChanged.RoomVariables);
		}
	}

	private void ProcessEventVariable(MMORoomVariable inEventVariable)
	{
		if (inEventVariable == null)
		{
			return;
		}
		string text = inEventVariable.Name.ToString();
		string text2 = inEventVariable.Value.ToString();
		if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
		{
			return;
		}
		WorldEventInfo eventNotificaiton = GetEventNotificaiton(text);
		if (eventNotificaiton == null)
		{
			return;
		}
		float num = eventNotificaiton._Duration;
		if (num < 0.01f)
		{
			return;
		}
		string[] array = text2.Split(',');
		if (array.Length < 4)
		{
			return;
		}
		DateTime dateTime = DateTime.Parse(array[0], UtUtilities.GetCultureInfo("en-US"));
		if (mLastEventUID == array[1])
		{
			UtDebug.Log(mLastEventUID + " this event has already been processed");
		}
		else
		{
			if (SanctuaryManager.pCurPetInstance == null || MMOTimeManager.pInstance == null || MainStreetMMOClient.pInstance.pAllDeactivated)
			{
				return;
			}
			DateTime serverDateTimeMilliseconds = MMOTimeManager.pInstance.GetServerDateTimeMilliseconds();
			float num2 = (float)(dateTime - serverDateTimeMilliseconds).TotalSeconds;
			mLastEventUID = array[1];
			DateTime dateTime2 = dateTime.AddSeconds(num);
			if (serverDateTimeMilliseconds < dateTime2)
			{
				if (num2 < 0f)
				{
					num += num2;
				}
				if (!_ExcludeSceneList.Contains(RsResourceManager.pCurrentLevel) && AvAvatar.pLevelState != AvAvatarLevelState.RACING && AvAvatar.pLevelState != AvAvatarLevelState.FLIGHTSCHOOL)
				{
					float num3 = num2 - eventNotificaiton._EventNotificationStartTime;
					Coroutine notifyEvent = eventNotificaiton._NotifyEvent;
					if (notifyEvent != null)
					{
						StopCoroutine(notifyEvent);
						eventNotificaiton._NotifyEvent = null;
					}
					notifyEvent = StartCoroutine(ShowNotification(dateTime, dateTime2, num2, num, text, array[3], (num3 > 0f) ? num3 : 0f));
					eventNotificaiton._NotifyEvent = notifyEvent;
				}
			}
			else
			{
				UtDebug.Log("Start time : " + dateTime);
				UtDebug.Log("Current time : " + serverDateTimeMilliseconds);
				UtDebug.Log("Last event has already finished " + (serverDateTimeMilliseconds - dateTime2).ToString() + " ago\nStart time : " + dateTime.ToString() + "\nEnd Time : " + dateTime2);
			}
		}
	}

	private IEnumerator ShowNotification(DateTime timeToStart, DateTime eventEndTime, float inTimeToEvent, float inEvenDuration, string inEventKey, string inWhichScene, float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		mTimeToStart = timeToStart;
		mEventEndTime = eventEndTime;
		mLevelToLoad = inWhichScene.Trim();
		mTimeToEvent = inTimeToEvent;
		mEventStarted = false;
		mNotified = false;
		mWarned = false;
		mMessageInfo = null;
		mEventNotificationActive = true;
	}

	public void ParseRoomVariables(List<MMORoomVariable> roomVars)
	{
		if (roomVars == null)
		{
			return;
		}
		foreach (MMORoomVariable roomVar in roomVars)
		{
			if (roomVar == null)
			{
				continue;
			}
			try
			{
				string text = roomVar.Name.ToString();
				if (text.StartsWith("WE_"))
				{
					if (MMOTimeManager.pInstance != null && MMOTimeManager.pInstance.pIsTimeSynced)
					{
						ProcessEventVariable(roomVar);
					}
					else if (mEventVar == null)
					{
						mEventVar = roomVar;
						mProcessAfterTimeSync = true;
					}
					else
					{
						UtDebug.Log("@@@@  " + roomVar.Name.ToString() + " / " + roomVar.Value.ToString());
					}
				}
				else if (text.StartsWith("WEN_"))
				{
					mTimeToNextEvent = DateTime.Parse(roomVar.Value.ToString(), UtUtilities.GetCultureInfo("en-US"));
					KAConsole.WriteLine("WE Next event time " + mTimeToNextEvent);
					UtDebug.Log("WE Next event time " + mTimeToNextEvent);
				}
			}
			catch (Exception ex)
			{
				UtDebug.LogError(ex.Message);
			}
		}
	}
}
