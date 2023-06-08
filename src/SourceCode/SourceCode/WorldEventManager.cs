using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using KnowledgeAdventure.Multiplayer.Events;
using KnowledgeAdventure.Multiplayer.Model;
using SOD.Event;
using SWS;
using UnityEngine;

public abstract class WorldEventManager : MonoBehaviour
{
	[Serializable]
	public class WorldEventAchievementRewardInfo
	{
		public LocaleString _RewardNameText;

		public int _RewardTier;

		public int _AchievementID;

		public int _AdRewardAchievementID;
	}

	public delegate void WorldEventStatus(bool isEventStarted);

	[Serializable]
	public class EventObject
	{
		public string _ObjectName;

		public GameObject _Marker;

		public Transform[] _SpawnMarkers;

		public SWS.PathManager _SplinePath;

		public Transform _ExitMarkers;

		public float _Scale;

		public bool _Targetable;

		public int _HealthMax;

		public float _Speed = 3f;

		public LocaleString _NameText = new LocaleString("Scout unit");

		public WorldEventAchievementRewardInfo[] _RewardInfo;

		[HideInInspector]
		public GameObject LiveObject;

		[HideInInspector]
		public int LiveHealth;

		[HideInInspector]
		public DateTime LastUpdate;

		[HideInInspector]
		public splineMove SplineMoveRef;

		[HideInInspector]
		public string UID;
	}

	[Serializable]
	public class SubEvent
	{
		public string _Name = "";

		public float _Interval;
	}

	[Serializable]
	public class WorldEvent
	{
		public string _Name;

		public string _EventKey;

		public string _EventObjectResourceURL = "RS_DATA/BattleShipsDO";

		public EventObject[] _Objects;

		public int _RandomSeed = 123456;

		public float _DurationSeconds = 5f;

		public SubEvent[] _SubEvent;

		public bool _SelectRandomly;

		[HideInInspector]
		public DateTime mStartTime;

		[HideInInspector]
		public DateTime mEndTime;

		[HideInInspector]
		public string mUID;

		[HideInInspector]
		public bool mClientSimulated;

		[HideInInspector]
		public WorldEventState _State;

		[HideInInspector]
		public bool mLoading;

		private int mObjectSequenceID;

		public string _SeasonalEventName;

		public string GetObjectUID()
		{
			mObjectSequenceID++;
			return mUID + mObjectSequenceID;
		}

		public void Reset()
		{
			mLoading = false;
			mStartTime = DateTime.MinValue;
			mEndTime = DateTime.MinValue;
			ResetObjectID();
		}

		public void ResetObjectID()
		{
			mObjectSequenceID = 0;
		}
	}

	[HideInInspector]
	private class ObjectSpawnData
	{
		public float inCountDownTime;

		public int mIndex;

		public WorldEvent mWorldEvent;
	}

	public WorldEvent[] _WorldEvent;

	public float _EventObjectExitTime = 60f;

	public GameObject _Hit3DScorePrefab;

	public float _PingSendDelay = 20f;

	public float _AIPingSendDelay = 1f;

	public float _PingMaxAllowedDiff = 3f;

	public CampSite[] _SafeZones;

	public float _ObjectPositionSyncMinDelay = 3f;

	public LocaleString _CriticalText = new LocaleString("CRITICAL HIT");

	public LocaleString _EventOverText = new LocaleString("The event was just over!");

	protected bool mInitialized;

	protected WorldEventTarget mWorldEventTarget;

	protected bool mIsAIController;

	protected int mPlayerScore;

	protected bool mEventWon;

	protected bool mPlayerParticipated;

	protected WorldEvent mWorldEvent;

	protected List<EventObject> mParticipatedEventObjs = new List<EventObject>();

	protected List<GameObject> mPreviousEventObjects = new List<GameObject>();

	protected float mPreviousObjectExitTimer;

	protected WorldEventMMOClient mMMOClient;

	private double mLastPingValue;

	private double mLastPingTime;

	private double mAILastPingTime;

	private WorldEventNotification mEventNotifier;

	public static WorldEventManager pInstance;

	public bool pIsAIController => mIsAIController;

	public bool pIsEventActive
	{
		get
		{
			if (mWorldEvent != null)
			{
				return mWorldEvent._State == WorldEventState.ACTIVE;
			}
			return false;
		}
	}

	public static event WorldEventStatus OnWEStatusChanged;

	protected virtual void Awake()
	{
		pInstance = this;
		SetDefaults();
		mEventNotifier = WorldEventNotification.pInstance;
	}

	protected virtual void Start()
	{
	}

	protected virtual void InitializeMMO()
	{
		if (!(mMMOClient != null))
		{
			mMMOClient = WorldEventMMOClient.Init(this);
			if (mMMOClient != null)
			{
				mMMOClient.AddListener("WE_", InitEvent);
				mMMOClient.AddListener("WE__AI", SetAIController);
				mMMOClient.AddListener("WEH_", ProcessHealth);
				mMMOClient.AddListener("_End", EndEvent);
				mInitialized = true;
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (mMMOClient != null)
		{
			mMMOClient.RemoveListener("WE__AI");
			mMMOClient.RemoveListener("WE_");
			mMMOClient.RemoveListener("WEH_");
			mMMOClient.RemoveListener("_End");
		}
	}

	protected virtual void ShowMessage(LocaleString message)
	{
	}

	private void SetAIController(string name, string value)
	{
		if (value.Equals(UserInfo.pInstance.UserID))
		{
			mIsAIController = true;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("uid", mWorldEvent.mUID);
			dictionary.Add("id", UserInfo.pInstance.UserID);
			mMMOClient.SendExtensionMessage("wex.AIACK", dictionary);
		}
		else
		{
			mIsAIController = false;
		}
	}

	public void InitEventFromResponse(string startValue, string endValue)
	{
		if (!string.IsNullOrEmpty(startValue))
		{
			string[] array = startValue.Split('|');
			if (array.Length > 1)
			{
				InitEvent(array[0], array[1], endValue);
			}
		}
	}

	private void InitEvent(string name, string value)
	{
		InitEvent(name, value, string.Empty);
	}

	private void InitEvent(string name, string value, string lastEventEndValue)
	{
		KAConsole.WriteLine("InitEvent = " + name + " :: " + value + " :: " + lastEventEndValue);
		WorldEvent worldEvent = GetWorldEvent(name.Substring("WE_".Length));
		string[] array = value.Split(',');
		if (worldEvent == null || array == null || array.Length == 0 || (worldEvent._State != 0 && array[1] == worldEvent.mUID))
		{
			return;
		}
		if (!string.IsNullOrEmpty(lastEventEndValue) && lastEventEndValue.Contains(array[1]))
		{
			ShowMessage(_EventOverText);
			return;
		}
		DateTime mStartTime = DateTime.Parse(array[0], UtUtilities.GetCultureInfo("en-US"));
		DateTime dateTime = mStartTime.AddSeconds(worldEvent._DurationSeconds);
		DateTime serverDateTimeMilliseconds = MMOTimeManager.pInstance.GetServerDateTimeMilliseconds();
		if (array.Length >= 3 && serverDateTimeMilliseconds < dateTime)
		{
			worldEvent.mStartTime = mStartTime;
			worldEvent.mEndTime = dateTime;
			worldEvent.mUID = array[1];
			worldEvent.mClientSimulated = bool.Parse(array[2]);
			SetEventState(worldEvent, WorldEventState.INIT);
		}
	}

	private void EndEvent(string name, string value)
	{
		KAConsole.WriteLine("EndEvent = " + name + " :: " + value);
		string[] array = value.Split(';');
		if (pIsEventActive && mWorldEvent.mUID != null && array.Length > 3 && mWorldEvent.mUID.Equals(array[0]))
		{
			bool result = false;
			if (bool.TryParse(array[1], out result))
			{
				OnEventEndFromServer(result, array[2], array[3]);
			}
			else
			{
				UtDebug.Log("Could not parse END variable values");
			}
		}
	}

	private void ProcessHealth(string name, string value)
	{
		string text = name.Substring("WEH_".Length);
		EventObject eventObject = GetEventObject(text);
		if (eventObject != null && value != null)
		{
			string[] array = value.Split(',');
			int liveHealth = eventObject.LiveHealth;
			eventObject.LiveHealth = (int)(float.Parse(array[0]) * (float)eventObject._HealthMax);
			if (liveHealth != eventObject.LiveHealth || eventObject.LiveHealth == eventObject._HealthMax)
			{
				eventObject.LiveObject.SendMessage("HealthUpdateFromServer", eventObject.LiveHealth);
				OnEventObjectHealthUpdate(text, eventObject._HealthMax, float.Parse(array[0]));
			}
		}
	}

	protected virtual void Update()
	{
		if (!mInitialized)
		{
			InitializeMMO();
			return;
		}
		DateTime serverDateTimeMilliseconds = MMOTimeManager.pInstance.GetServerDateTimeMilliseconds();
		for (int i = 0; i < _WorldEvent.Length; i++)
		{
			if (!_WorldEvent[i].mLoading && _WorldEvent[i]._State != 0)
			{
				UpdateEvent(_WorldEvent[i], serverDateTimeMilliseconds);
			}
		}
		if (mPreviousEventObjects.Count > 0)
		{
			mPreviousObjectExitTimer -= Time.deltaTime;
			if (mPreviousObjectExitTimer <= 0f)
			{
				DestroyPreviousEventObjects();
				mPreviousObjectExitTimer = 0f;
			}
		}
	}

	protected virtual void OnEventObjectHealthUpdate(string inObjID, int inMaxHealth, float inCurrentHealthPercent)
	{
	}

	protected virtual void OnPlayersScoreReady(string[] playersScore)
	{
	}

	public WorldEvent GetWorldEvent(string eventName)
	{
		if (_WorldEvent == null)
		{
			return null;
		}
		WorldEvent worldEvent = Array.Find(_WorldEvent, delegate(WorldEvent t)
		{
			EventManager eventManager = EventManager.Get(t._SeasonalEventName);
			return t._Name == eventName && eventManager != null && eventManager.EventInProgress() && !eventManager.GracePeriodInProgress();
		});
		if (worldEvent != null)
		{
			return worldEvent;
		}
		return Array.Find(_WorldEvent, (WorldEvent t) => t._Name == eventName);
	}

	public EventObject GetEventObject(string id)
	{
		if (_WorldEvent != null)
		{
			for (int i = 0; i < _WorldEvent.Length; i++)
			{
				WorldEvent worldEvent = _WorldEvent[i];
				if (worldEvent._Objects == null || worldEvent._Objects.Length == 0)
				{
					continue;
				}
				for (int j = 0; j < worldEvent._Objects.Length; j++)
				{
					EventObject eventObject = worldEvent._Objects[j];
					if (eventObject.LiveObject != null && eventObject.UID == id)
					{
						return eventObject;
					}
				}
			}
		}
		return null;
	}

	public void UpdateEvent(WorldEvent we, DateTime serverTimeInMS)
	{
		switch (we._State)
		{
		case WorldEventState.INIT:
		{
			TimeSpan timeSpan = we.mStartTime.Subtract(serverTimeInMS);
			WorldEventNotification.WorldEventInfo eventNotificaiton = mEventNotifier.GetEventNotificaiton(we._EventKey);
			if (timeSpan.TotalSeconds < (double)eventNotificaiton._EventNotificationStartTime)
			{
				SetEventState(we, WorldEventState.WARN);
			}
			break;
		}
		case WorldEventState.WARN:
			if (serverTimeInMS >= we.mStartTime)
			{
				if (serverTimeInMS < we.mEndTime)
				{
					SetEventState(we, WorldEventState.ACTIVE);
				}
				else
				{
					SetEventState(we, WorldEventState.NONE);
				}
			}
			break;
		case WorldEventState.ACTIVE:
		{
			if (mIsAIController)
			{
				if ((double)Time.time - mAILastPingTime > (double)_AIPingSendDelay)
				{
					mMMOClient.SendExtensionMessage("wex.AIP", null);
					mAILastPingTime = Time.time;
				}
			}
			else if ((double)Time.time - mLastPingTime > (double)_PingSendDelay && Mathf.Abs((float)(MMOTimeManager.pInstance.pAveragePing - mLastPingValue)) > _PingMaxAllowedDiff)
			{
				mMMOClient.SendPingMessage(MMOTimeManager.pInstance.pAveragePing);
				mLastPingValue = MMOTimeManager.pInstance.pAveragePing;
				mLastPingTime = Time.time;
			}
			if (we._Objects == null)
			{
				break;
			}
			for (int i = 0; i < mWorldEvent._Objects.Length; i++)
			{
				splineMove splineMoveRef = mWorldEvent._Objects[i].SplineMoveRef;
				if (!(mWorldEvent._Objects[i].LiveObject == null) && !(splineMoveRef == null) && splineMoveRef.tween != null)
				{
					float num = (float)(serverTimeInMS - mWorldEvent.mStartTime).TotalSeconds;
					if (num - splineMoveRef.tween.fullPosition > _ObjectPositionSyncMinDelay)
					{
						float fullPosition = Mathf.Clamp(num, 0f, splineMoveRef.tween.Duration());
						splineMoveRef.tween.fullPosition = fullPosition;
					}
				}
			}
			break;
		}
		}
	}

	public void SetEventState(WorldEvent we, WorldEventState newState)
	{
		if (we._State == newState)
		{
			return;
		}
		we._State = newState;
		switch (newState)
		{
		case WorldEventState.NONE:
			if (mEventNotifier != null)
			{
				mEventNotifier.Reset();
			}
			KAConsole.WriteLine("Reset event = " + we.mUID);
			DestroyEventObjects(we);
			we.Reset();
			break;
		case WorldEventState.INIT:
			UtDebug.Log("Making event active , uid = " + we.mUID);
			KAConsole.WriteLine("Making event active = " + we.mUID);
			break;
		case WorldEventState.WARN:
			KAConsole.WriteLine("Event object spawn= " + we.mUID);
			OnEventWarning(we);
			break;
		case WorldEventState.ACTIVE:
		{
			KAConsole.WriteLine("Begin event = " + we.mUID);
			mWorldEvent = we;
			OnEventBegin(we);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("timeSpan", we._DurationSeconds);
			mMMOClient.SendExtensionMessage("wex.ETS", dictionary);
			UtDebug.Log("World Event " + we._Name + " ends at " + we.mEndTime.ToString() + " TimeSpan : " + we._DurationSeconds);
			break;
		}
		}
	}

	public virtual void OnEventWarning(WorldEvent we)
	{
		UtDebug.LogWarning("World Event " + we._Name + " is about to begin!");
		DateTime serverDateTimeMilliseconds = MMOTimeManager.pInstance.GetServerDateTimeMilliseconds();
		InstantiateEventObjects(we, (float)we.mStartTime.Subtract(serverDateTimeMilliseconds).TotalSeconds);
	}

	private void LoadWorldEventObject(WorldEvent worldEvent, int index, float inCountDownTime)
	{
		worldEvent.mLoading = true;
		ObjectSpawnData objectSpawnData = new ObjectSpawnData();
		objectSpawnData.inCountDownTime = inCountDownTime;
		objectSpawnData.mIndex = index;
		objectSpawnData.mWorldEvent = worldEvent;
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.LoadAssetFromBundle(worldEvent._EventObjectResourceURL, worldEvent._Objects[index]._ObjectName, OnWorldEventObjectLoaded, typeof(GameObject), inDontDestroy: false, objectSpawnData);
	}

	public void OnWorldEventObjectLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			ObjectSpawnData objectSpawnData = (ObjectSpawnData)inUserData;
			objectSpawnData.mWorldEvent.mLoading = false;
			SpawnObject(objectSpawnData.mWorldEvent, (GameObject)inObject, objectSpawnData.mIndex, objectSpawnData.inCountDownTime);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			UtDebug.LogError("Failed to load World Event Battle Ship Bundle");
			break;
		}
	}

	private GameObject SpawnObject(WorldEvent we, GameObject inObject, int index, float countDownTime)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(inObject);
		EventObject eventObject = we._Objects[index];
		if (gameObject != null)
		{
			if (eventObject.LiveObject != null)
			{
				UnityEngine.Object.Destroy(eventObject.LiveObject);
			}
			eventObject.LiveObject = gameObject;
			eventObject.UID = we.GetObjectUID();
			gameObject.transform.parent = base.gameObject.transform.root;
			if (eventObject._Scale > 0f)
			{
				gameObject.transform.localScale += new Vector3(eventObject._Scale, eventObject._Scale, eventObject._Scale);
			}
			if (countDownTime > 0f && eventObject._SpawnMarkers != null)
			{
				if (eventObject._SpawnMarkers.Length != 0)
				{
					int num = UnityEngine.Random.Range(0, eventObject._SpawnMarkers.Length);
					gameObject.transform.position = eventObject._SpawnMarkers[num].transform.position;
				}
				if (eventObject._SplinePath != null)
				{
					Vector3 vector = eventObject._SplinePath.GetPathPoints()[0];
					gameObject.transform.LookAt(vector);
					WorldEventNotification.WorldEventInfo eventNotificaiton = mEventNotifier.GetEventNotificaiton(we._EventKey);
					Vector3 position = Vector3.Lerp(gameObject.transform.position, vector, 1f - countDownTime / eventNotificaiton._EventNotificationStartTime);
					gameObject.transform.position = position;
					TweenPosition.Begin(gameObject, countDownTime, vector);
				}
			}
		}
		StartCoroutine(SetPathForBoat(we));
		return gameObject;
	}

	private IEnumerator SetPathForBoat(WorldEvent we)
	{
		yield return new WaitForEndOfFrame();
		for (int i = 0; i < we._Objects.Length; i++)
		{
			EventObject eventObject = we._Objects[i];
			if (eventObject.LiveObject == null || !(null != eventObject._SplinePath))
			{
				continue;
			}
			splineMove component = eventObject.LiveObject.GetComponent<splineMove>();
			if (null == component)
			{
				component = eventObject.LiveObject.AddComponent<splineMove>();
				if (null != component)
				{
					component.onStart = false;
					component.moveToPath = false;
					component.timeValue = splineMove.TimeValue.time;
					component.pathType = PathType.CatmullRom;
					component.lockRotation = AxisConstraint.X;
					component.ChangeSpeed(we._DurationSeconds);
					component.SetPath(eventObject._SplinePath);
					component.Pause();
					eventObject.SplineMoveRef = component;
				}
			}
		}
	}

	private void OnReachingAttackPoint(WorldEvent we)
	{
		for (int i = 0; i < we._Objects.Length; i++)
		{
			EventObject eventObject = we._Objects[i];
			if (eventObject.LiveObject == null)
			{
				continue;
			}
			GameObject liveObject = eventObject.LiveObject;
			if (null != eventObject._SplinePath)
			{
				splineMove splineMove = eventObject.LiveObject.GetComponent<splineMove>();
				if (null == splineMove)
				{
					splineMove = eventObject.LiveObject.AddComponent<splineMove>();
					splineMove.onStart = false;
					splineMove.moveToPath = false;
					splineMove.timeValue = splineMove.TimeValue.time;
					splineMove.pathType = PathType.CatmullRom;
					splineMove.lockRotation = AxisConstraint.X;
					splineMove.ChangeSpeed(we._DurationSeconds);
					splineMove.SetPath(eventObject._SplinePath);
					splineMove.Pause();
					eventObject.SplineMoveRef = splineMove;
				}
				float num = 0f;
				DateTime serverDateTimeMilliseconds = MMOTimeManager.pInstance.GetServerDateTimeMilliseconds();
				num = ((eventObject.LiveHealth > 0 || !(eventObject.LastUpdate > DateTime.MinValue)) ? ((float)(serverDateTimeMilliseconds - we.mStartTime).TotalSeconds) : ((float)(serverDateTimeMilliseconds - eventObject.LastUpdate).TotalSeconds));
				float max = splineMove.tween.Duration();
				float num2 = 0f;
				num2 = Mathf.Clamp(num, 0f, max);
				splineMove.tween.fullPosition = num2;
				splineMove.Resume();
			}
			if (eventObject._Targetable)
			{
				mWorldEventTarget = liveObject.GetComponent<WorldEventTarget>();
				if (mWorldEventTarget == null)
				{
					mWorldEventTarget = liveObject.AddComponent<WorldEventTarget>();
				}
				if (mWorldEventTarget != null)
				{
					mWorldEventTarget._Health = eventObject.LiveHealth;
					mWorldEventTarget.myEvent = we;
					mWorldEventTarget.myObject = eventObject;
					mWorldEventTarget.Init();
				}
			}
		}
	}

	protected virtual void InstantiateEventObjects(WorldEvent we, float inCountDownTime)
	{
		we.ResetObjectID();
		if (we._Objects == null || we._Objects.Length == 0)
		{
			return;
		}
		if (we._SelectRandomly)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(we.mUID);
			int num = 0;
			for (int i = 0; i < bytes.Length; i++)
			{
				num += bytes[i];
			}
			UnityEngine.Random.InitState(num);
			int index = UnityEngine.Random.Range(0, we._Objects.Length);
			LoadWorldEventObject(we, index, inCountDownTime);
		}
		else
		{
			for (int j = 0; j < we._Objects.Length; j++)
			{
				LoadWorldEventObject(we, j, inCountDownTime);
			}
		}
	}

	public virtual void OnEventBegin(WorldEvent we)
	{
		mParticipatedEventObjs.Clear();
		OnReachingAttackPoint(we);
		if (WorldEventManager.OnWEStatusChanged != null)
		{
			WorldEventManager.OnWEStatusChanged(isEventStarted: true);
		}
		UtDebug.LogWarning("World Event " + base.name + " has begun!");
	}

	public virtual void ResetEvent(bool resetPreviousEventAlso = false)
	{
		WorldEvent[] worldEvent = _WorldEvent;
		foreach (WorldEvent we in worldEvent)
		{
			SetEventState(we, WorldEventState.NONE);
		}
		if (UiWorldEventNotification.pInstance != null)
		{
			UiWorldEventNotification.pInstance.CloseUI();
		}
		if (resetPreviousEventAlso)
		{
			DestroyPreviousEventObjects();
		}
	}

	protected virtual void DestroyEventObjects(WorldEvent we)
	{
		if (we._Objects != null && we._Objects.Length != 0)
		{
			for (int i = 0; i < we._Objects.Length; i++)
			{
				if (we._Objects[i].LiveObject != null)
				{
					UnityEngine.Object.Destroy(we._Objects[i].LiveObject);
				}
				we._Objects[i].LiveObject = null;
				we._Objects[i].SplineMoveRef = null;
			}
		}
		SetDefaults(we);
	}

	public virtual void OnEventEnd(WorldEvent we, bool destroyObjects, string[] scores)
	{
		if (we == null)
		{
			return;
		}
		SetEventState(we, WorldEventState.END);
		if (WorldEventManager.OnWEStatusChanged != null)
		{
			WorldEventManager.OnWEStatusChanged(isEventStarted: false);
		}
		UtDebug.LogWarning("World Event " + base.name + " has ended!  My Score : " + mPlayerScore);
		if (destroyObjects)
		{
			DestroyEventObjects(we);
		}
		if (mEventWon)
		{
			if (scores == null)
			{
				Debug.Log("WE : event won but no scores");
			}
		}
		else if (mParticipatedEventObjs.Count > 0)
		{
			foreach (EventObject mParticipatedEventObj in mParticipatedEventObjs)
			{
				splineMove component = mParticipatedEventObj.LiveObject.GetComponent<splineMove>();
				if (null != component)
				{
					component.Stop();
					UnityEngine.Object.Destroy(component);
				}
				mParticipatedEventObj.LiveObject.transform.LookAt(mParticipatedEventObj._ExitMarkers.position);
				TweenPosition tweenPosition = TweenPosition.Begin(mParticipatedEventObj.LiveObject, _EventObjectExitTime, mParticipatedEventObj._ExitMarkers.position);
				tweenPosition.eventReceiver = base.gameObject;
				tweenPosition.callWhenFinished = "OnReachingExitPoint";
			}
		}
		if (scores != null)
		{
			OnPlayersScoreReady(scores);
		}
	}

	private void OnReachingExitPoint()
	{
		DestroyPreviousEventObjects();
	}

	protected virtual void DestroyPreviousEventObjects()
	{
		for (int i = 0; i < mPreviousEventObjects.Count; i++)
		{
			UnityEngine.Object.Destroy(mPreviousEventObjects[i]);
		}
	}

	private void SetDefaults()
	{
		for (int i = 0; i < _WorldEvent.Length; i++)
		{
			SetDefaults(_WorldEvent[i]);
		}
	}

	private void SetDefaults(WorldEvent we)
	{
		for (int i = 0; i < we._Objects.Length; i++)
		{
			EventObject eventObject = we._Objects[i];
			if (eventObject != null)
			{
				eventObject.LiveHealth = eventObject._HealthMax;
				eventObject.LastUpdate = DateTime.MinValue;
				eventObject.SplineMoveRef = null;
				eventObject.LiveObject = null;
			}
		}
	}

	protected virtual void OnEventEndFromServer(bool isEventWon, string scoreString, string objectString)
	{
		mEventWon = isEventWon;
		mParticipatedEventObjs.Clear();
		string[] scores = scoreString.TrimEnd(',').Split(',');
		string[] array = objectString.TrimEnd(',').Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Split(':')[0];
			if (text.Contains("WEH_"))
			{
				text = text.Substring("WEH_".Length);
			}
			EventObject eventObject = GetEventObject(text);
			if (eventObject != null)
			{
				mParticipatedEventObjs.Add(eventObject);
			}
		}
		if (mParticipatedEventObjs.Count > 0)
		{
			OnEventEnd(mWorldEvent, destroyObjects: false, scores);
		}
		mPreviousObjectExitTimer = _EventObjectExitTime;
		for (int j = 0; j < mWorldEvent._Objects.Length; j++)
		{
			GameObject liveObject = mWorldEvent._Objects[j].LiveObject;
			if (liveObject != null)
			{
				mPreviousEventObjects.Add(liveObject);
			}
			mWorldEvent._Objects[j].LiveObject = null;
			mWorldEvent._Objects[j].SplineMoveRef = null;
		}
		SetEventState(mWorldEvent, WorldEventState.NONE);
	}

	protected GameObject GetActiveLiveObject(string objUID)
	{
		if (mWorldEvent == null)
		{
			return null;
		}
		for (int i = 0; i < mWorldEvent._Objects.Length; i++)
		{
			if (mWorldEvent._Objects[i].UID == objUID)
			{
				return mWorldEvent._Objects[i].LiveObject;
			}
		}
		return null;
	}

	public void Show3DTargetHitScore(Vector3 inPosition, int inScore, bool isCritical)
	{
		TargetHit3DScore.Show3DHitScore(_Hit3DScorePrefab, inPosition, inScore * -1, isCritical);
	}

	public void CheckNotifier(List<MMORoomVariable> roomVars)
	{
		mEventNotifier.ParseRoomVariables(roomVars);
	}

	protected virtual void ResponseEventHandler(object sender, MMOExtensionResponseReceivedEventArgs args)
	{
	}

	public bool IsPlayerInSafeZone(Vector3 position)
	{
		CampSite[] safeZones = _SafeZones;
		for (int i = 0; i < safeZones.Length; i++)
		{
			if (safeZones[i].IsInProximity(position))
			{
				return true;
			}
		}
		return false;
	}
}
