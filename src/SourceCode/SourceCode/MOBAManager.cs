using System;
using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using KnowledgeAdventure.Multiplayer.Model;
using UnityEngine;

public class MOBAManager : MonoBehaviour
{
	public List<MOBAEntityType> _MOBAEntities;

	public float mGameStartTimeout = 300f;

	public float mResolveHostTimeout = 30f;

	public float mConnectingTimeout = 10f;

	private const string EVENT_READY = "MESR";

	private const string EVENT_INPLAY = "MEIP";

	private const string EVENT_HOSTUSER = "MEHU";

	private const string EVENT_STARTGAME = "MESG";

	private const string EVENT_REHOST = "MERH";

	private const string EVENT_RESET = "MERE";

	private const string EVENT_VARUPDATE = "MEVU";

	private const string EVENT_SPAWN = "MESP";

	private const string EVENT_DESTROYENTITY = "MEDE";

	private const char MESSAGE_SEPARATOR = ':';

	private const float mReplicationRate = 0.5f;

	private float mReplicationTimer;

	private MOBAPlayer mMyPlayer;

	private bool mForceStart;

	private float mStateTimer;

	private bool mHostByLatency = true;

	private MOBAState mLocalState = MOBAState.MS_WAITING_READY;

	private MOBAMMOClient mClient;

	private string mSelfID = "";

	private Dictionary<string, MOBAPlayer> mPlayerList = new Dictionary<string, MOBAPlayer>();

	private List<GameObject> mPurgeList = new List<GameObject>();

	private float mPurgeTimer;

	[HideInInspector]
	public int mPlayersExpected = 2;

	private float mLocalPing;

	private int mRandomRoll;

	public static bool mIsAuthority;

	private Dictionary<string, float> mEventTimes = new Dictionary<string, float>();

	private void Start()
	{
		Reset();
		AvAvatar.SetActive(inActive: true);
		mClient = MOBAMMOClient.Init(null, this);
		mClient.OnMessageEvent += MOBAMessageHandler;
	}

	private void Reset()
	{
		mPlayerList.Clear();
		mRandomRoll = UnityEngine.Random.Range(0, 10000000);
		mLocalPing = 0f;
		mLocalState = MOBAState.MS_CONNECTING;
		mStateTimer = 0f;
		mHostByLatency = true;
		ResetEntities(forced: true);
	}

	private void ResetHost()
	{
		foreach (KeyValuePair<string, MOBAPlayer> mPlayer in mPlayerList)
		{
			mPlayer.Value.hostUser = "";
		}
		mIsAuthority = false;
		mHostByLatency = true;
	}

	private void NotifyGainAuthority()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("MOBAEntity");
		if (array == null)
		{
			return;
		}
		GameObject[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			MOBAEntity component = array2[i].GetComponent<MOBAEntity>();
			if (component != null)
			{
				component.OnGainAuthority();
			}
		}
	}

	private void ResetEntities(bool forced)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("MOBAEntity");
		if (array == null)
		{
			return;
		}
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			MOBAEntity component = gameObject.GetComponent<MOBAEntity>();
			if (component != null)
			{
				if (component._Resident)
				{
					component.EntityReset(mIsAuthority);
				}
				else if (forced)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
				else
				{
					component.mRequestDestroy = true;
				}
			}
		}
	}

	private void Update()
	{
		mStateTimer += Time.deltaTime;
		bool num = mIsAuthority;
		mIsAuthority = mMyPlayer != null && mMyPlayer.hostUser == mSelfID;
		if (!num && mIsAuthority)
		{
			NotifyGainAuthority();
		}
		mPurgeTimer += Time.deltaTime;
		if (mPurgeTimer > 10f)
		{
			if (mPurgeList.Count > 0)
			{
				foreach (GameObject mPurge in mPurgeList)
				{
					UnityEngine.Object.Destroy(mPurge);
				}
			}
			mPurgeTimer = 0f;
			mPurgeList.Clear();
		}
		switch (mLocalState)
		{
		case MOBAState.MS_CONNECTING:
			if (MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.pState == MMOClientState.IN_ROOM && (mStateTimer > mConnectingTimeout || (MMOTimeManager.pInstance != null && MMOTimeManager.pInstance.pAveragePing > 0.0)))
			{
				mSelfID = WsWebService.pUserToken;
				SetState(MOBAState.MS_WAITING_READY);
				mLocalPing = (float)MMOTimeManager.pInstance.pAveragePing;
				if (mLocalPing == 0f)
				{
					mLocalPing = 999f;
				}
				mMyPlayer = GetMOBAPlayer(mSelfID);
				mMyPlayer.latency = mLocalPing;
				mMyPlayer.roll = mRandomRoll;
				SetState(MOBAState.MS_WAITING_READY);
			}
			break;
		case MOBAState.MS_WAITING_READY:
			MOBAEvent("MESR", 2f, bReliable: false, mLocalPing.ToString(), mRandomRoll.ToString());
			if (mPlayerList.Count >= mPlayersExpected)
			{
				SetState(MOBAState.MS_HOST_ARBITRATION);
			}
			if (mStateTimer > mGameStartTimeout)
			{
				if (mPlayerList.Count > 1)
				{
					SetState(MOBAState.MS_HOST_ARBITRATION);
				}
				else
				{
					SetState(MOBAState.MS_FAIL);
				}
			}
			if (mForceStart)
			{
				SetState(MOBAState.MS_HOST_ARBITRATION);
			}
			break;
		case MOBAState.MS_HOST_ARBITRATION:
			ChooseHost(mHostByLatency);
			MOBAEvent("MESR", 2f, bReliable: false, mLocalPing.ToString(), mRandomRoll.ToString());
			MOBAEvent("MEHU", 2f, bReliable: false, mMyPlayer.hostUser);
			if (ResolveHost() && mIsAuthority)
			{
				MOBAEvent("MESG", 0f, bReliable: true, mMyPlayer.hostUser);
				SetState(MOBAState.MS_GAME_ON);
			}
			if (mStateTimer > mResolveHostTimeout)
			{
				if (mHostByLatency)
				{
					mHostByLatency = false;
					SetState(MOBAState.MS_HOST_ARBITRATION);
				}
				else
				{
					UtDebug.Log("MOBA: Host arbitration has failed.");
					SetState(MOBAState.MS_HOST_ARBITRATION_FAIL);
				}
			}
			break;
		case MOBAState.MS_GAME_ON:
			MOBAEvent("MEIP", 5f, bReliable: false, mMyPlayer.hostUser);
			break;
		case MOBAState.MS_HOST_ARBITRATION_FAIL:
		case MOBAState.MS_FAIL:
			break;
		}
	}

	private void LateUpdate()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("MOBAEntity");
		if (array == null || array.Length < 1)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		if (mIsAuthority)
		{
			mReplicationTimer += Time.deltaTime;
			if (mReplicationTimer > 0.5f)
			{
				mReplicationTimer = 0f;
				string text = "";
				GameObject[] array2 = array;
				foreach (GameObject gameObject in array2)
				{
					MOBAEntity component = gameObject.GetComponent<MOBAEntity>();
					if (component != null)
					{
						if (!component._Resident && component.mRequestDestroy)
						{
							MOBAEvent("MEDE", 0f, bReliable: true, gameObject.name);
							list.Add(gameObject);
						}
						else
						{
							string replicationString = component.GetReplicationString();
							text += replicationString;
						}
					}
				}
				if (!string.IsNullOrEmpty(text))
				{
					MOBAEvent("MEVU", 0f, bReliable: true, text);
				}
				array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					MOBAEntity component2 = array2[i].GetComponent<MOBAEntity>();
					if (!(component2 != null) || !component2.mLimbo || component2.mRequestDestroy)
					{
						continue;
					}
					if (component2._EntityID == -1 && !component2._Resident)
					{
						UtDebug.LogError("MOBA Error: MOBAEntity cannot be hosted due to invalid EntityID");
						continue;
					}
					if (component2._UniqueName == "host_chosen")
					{
						component2._UniqueName = Guid.NewGuid().ToString().Substring(0, 8);
					}
					component2.Init();
					component2.mForceReplication = true;
					component2.mLimbo = false;
				}
			}
		}
		foreach (GameObject item in list)
		{
			UnityEngine.Object.Destroy(item);
		}
		list.Clear();
	}

	private void SetState(MOBAState newState)
	{
		mStateTimer = 0f;
		mLocalState = newState;
	}

	private void MOBAEvent(string eventName, float frequency, bool bReliable)
	{
		MOBAEvent(eventName, frequency, bReliable, "", "");
	}

	private void MOBAEvent(string eventName, float frequency, bool bReliable, string value)
	{
		MOBAEvent(eventName, frequency, bReliable, value, "");
	}

	private void MOBAEvent(string eventName, float frequency, bool bReliable, string valueA, string valueB)
	{
		bool flag = true;
		if (frequency > Mathf.Epsilon && mEventTimes.ContainsKey(eventName) && Time.realtimeSinceStartup - mEventTimes[eventName] < frequency)
		{
			flag = false;
		}
		if (flag)
		{
			if (mEventTimes.ContainsKey(eventName))
			{
				mEventTimes[eventName] = Time.realtimeSinceStartup;
			}
			else
			{
				mEventTimes.Add(eventName, Time.realtimeSinceStartup);
			}
			string text = eventName + ":" + valueA;
			if (!string.IsNullOrEmpty(valueB))
			{
				text = text + ':' + valueB;
			}
			MainStreetMMOClient.pInstance.SendPublicExtensionMessage(text, bReliable);
		}
	}

	private void MOBAMessageHandler(MMOMessageReceivedEventArgs args)
	{
		if (args.MMOMessage.MessageType != MMOMessageType.User)
		{
			return;
		}
		string[] array = args.MMOMessage.MessageText.Split(':');
		MOBAPlayer mOBAPlayer = GetMOBAPlayer(args.MMOMessage.Sender.Username);
		if (mOBAPlayer == null)
		{
			return;
		}
		if (array[0] == "MESR")
		{
			float.TryParse(array[1], out mOBAPlayer.latency);
			int.TryParse(array[2], out mOBAPlayer.roll);
			if (mOBAPlayer.latency == 0f)
			{
				mOBAPlayer.latency = 999f;
			}
		}
		if (array[0] == "MEIP" && mLocalState != 0 && mLocalState != MOBAState.MS_GAME_ON)
		{
			mMyPlayer.hostUser = array[1];
			SetState(MOBAState.MS_GAME_ON);
		}
		if (array[0] == "MEHU")
		{
			mOBAPlayer.hostUser = array[1];
		}
		if (array[0] == "MESG")
		{
			if (string.IsNullOrEmpty(mMyPlayer.hostUser))
			{
				mMyPlayer.hostUser = array[1];
			}
			if (array[1] != mMyPlayer.hostUser)
			{
				MOBAEvent("MERH", 0f, bReliable: true);
				ResetHost();
				SetState(MOBAState.MS_HOST_ARBITRATION);
			}
			else
			{
				SetState(MOBAState.MS_GAME_ON);
			}
		}
		if (array[0] == "MERH")
		{
			ResetHost();
			SetState(MOBAState.MS_HOST_ARBITRATION);
		}
		if (array[0] == "MERE")
		{
			Reset();
		}
		if (array[0] == "MEVU" && mLocalState == MOBAState.MS_GAME_ON)
		{
			HandleReplicationString(array);
		}
		if (!(array[0] == "MEDE"))
		{
			return;
		}
		GameObject gameObject = GameObject.Find(array[1]);
		if (gameObject != null)
		{
			Renderer component = gameObject.GetComponent<Renderer>();
			if (component != null)
			{
				component.enabled = false;
			}
			mPurgeList.Add(gameObject);
		}
	}

	private void HandleReplicationString(string[] message)
	{
		int num = 2;
		int num2 = -1;
		while (message.Length > num && num2 != num)
		{
			num2 = num;
			GameObject gameObject = GameObject.Find(message[num]);
			if (gameObject != null)
			{
				MOBAEntity component = gameObject.GetComponent<MOBAEntity>();
				if (component != null)
				{
					num = component.ApplyReplicationString(message, num);
				}
			}
			else
			{
				if (message.Length <= num + 1 || !int.TryParse(message[num + 1], out var result) || result <= -1 || !(_MOBAEntities[result]._GameObject != null))
				{
					continue;
				}
				gameObject = UnityEngine.Object.Instantiate(_MOBAEntities[result]._GameObject);
				if (gameObject != null)
				{
					MOBAEntity component2 = gameObject.GetComponent<MOBAEntity>();
					if (component2 != null)
					{
						component2._UniqueName = message[num];
						component2.Init();
						num = component2.ApplyReplicationString(message, num);
					}
				}
			}
		}
	}

	private void ChooseHost(bool byLatency)
	{
		MOBAPlayer mOBAPlayer = null;
		int num = 0;
		float num2 = 0f;
		foreach (KeyValuePair<string, MOBAPlayer> mPlayer in mPlayerList)
		{
			if (byLatency)
			{
				if (mOBAPlayer == null || mPlayer.Value.latency < num2)
				{
					num2 = mPlayer.Value.latency;
					mOBAPlayer = mPlayer.Value;
				}
			}
			else if (mOBAPlayer == null || mPlayer.Value.roll < num)
			{
				num = mPlayer.Value.roll;
				mOBAPlayer = mPlayer.Value;
			}
		}
		mMyPlayer.hostUser = mOBAPlayer.name;
	}

	private bool ResolveHost()
	{
		bool result = true;
		string text = null;
		foreach (KeyValuePair<string, MOBAPlayer> mPlayer in mPlayerList)
		{
			if (text == null)
			{
				text = mPlayer.Value.hostUser;
			}
			if (string.IsNullOrEmpty(text) || text != mPlayer.Value.hostUser)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private MOBAPlayer GetMOBAPlayer(string name)
	{
		MOBAPlayer mOBAPlayer = null;
		if (mPlayerList.ContainsKey(name))
		{
			mOBAPlayer = mPlayerList[name];
		}
		else
		{
			mOBAPlayer = new MOBAPlayer();
			mOBAPlayer.name = name;
			mPlayerList.Add(name, mOBAPlayer);
		}
		return mOBAPlayer;
	}

	public MMOAvatar GetMMOPlayer(string name)
	{
		foreach (KeyValuePair<string, MMOAvatar> pPlayer in MainStreetMMOClient.pInstance.pPlayerList)
		{
			MMOAvatar value = pPlayer.Value;
			if (value != null && pPlayer.Key == name)
			{
				return value;
			}
		}
		return null;
	}

	public void RemovePlayer(MMOAvatar avatar)
	{
		if (mPlayerList.ContainsKey(avatar.mUserName))
		{
			mPlayerList.Remove(avatar.mUserName);
			if (mMyPlayer.hostUser == avatar.mUserName)
			{
				MOBAEvent("MERH", 0f, bReliable: true);
				ResetHost();
				SetState(MOBAState.MS_HOST_ARBITRATION);
			}
		}
	}

	public void OnGUI()
	{
		float num = Screen.width / 2 - 128;
		if (GUI.Button(new Rect(num, 32f, 96f, 48f), "Force Start"))
		{
			mForceStart = true;
		}
		if (GUI.Button(new Rect(num + 96f + 8f, 32f, 96f, 48f), "Reset"))
		{
			mForceStart = false;
			MOBAEvent("MERE", 0f, bReliable: true);
			Reset();
		}
		string text = "";
		text = text + "\nIs Authority: " + mIsAuthority;
		text = text + "\nHost Player: " + ((mMyPlayer == null) ? "---" : mMyPlayer.hostUser);
		text = text + "\nPlayer Count: " + mPlayerList.Count + "/" + mPlayersExpected;
		text = text + "\nServer Ping: " + mLocalPing;
		text = text + "\nState Timer: " + mStateTimer;
		text = text + "\nMOBA State: " + mLocalState;
		text = text + "\nRoom State: " + ((MainStreetMMOClient.pInstance == null) ? "Null" : MainStreetMMOClient.pInstance.pState.ToString());
		GUI.Label(new Rect(num, 80f, Screen.width, Screen.height), text);
	}
}
