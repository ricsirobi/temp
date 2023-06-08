using System;
using UnityEngine;

public class RacingManager : MonoBehaviour
{
	private static RacingManager mInstance;

	public const uint RACING_LOG_MASK = 768u;

	public int _GamePauseTimeOutInSec = 45;

	public float _PlayerIdleTimeOut = 60f;

	public float _ShowIdleWarningIn = 30f;

	public LocaleString _PlayerIdleWarningText = new LocaleString("[Review] Timeout warning due to idle. You will exit racing in:");

	public PenaltyDurationGroup[] _PenaltyDurationGroup;

	public int _PenaltyPairDataID = 2017;

	public string _PenaltyDataKey = "PenaltyData";

	public bool mNetworkConnected = true;

	public bool mPlayerPrefDataSync;

	private DNFType mRaceFinishReason;

	private PairData mPairData;

	private PenaltySaveData mPenaltyDataValue;

	private DateTime mTimeGamePause;

	private bool mIsApplicationQuit;

	private static bool mIsSinglePlayer;

	private static float mTransitionTime;

	private string mUserId;

	public static RacingManager Instance => mInstance;

	public int TimeElapsedGamePause { get; private set; }

	public RacingManagerState State { get; set; }

	public bool Ready { get; private set; }

	public bool PenaltyApplied { get; set; }

	public static bool pIsSinglePlayer
	{
		get
		{
			return mIsSinglePlayer;
		}
		set
		{
			mIsSinglePlayer = value;
		}
	}

	private void Start()
	{
		MainStreetMMOClient.pInstance.ForceEnableMultiplayer(enable: true);
		if (mInstance == null)
		{
			mInstance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		mUserId = UserInfo.pInstance.UserID;
		Reset();
		Ready = true;
		ReadPairData();
		ConnectivityMonitor.AddDisconnectionHandler(OnNetworkDisconnected);
		ConnectivityMonitor.AddConnectionHandler(OnNetworkConnected);
	}

	public void DestroyInstance()
	{
		mInstance = null;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		ConnectivityMonitor.RemoveDisconnectionHandler(OnNetworkDisconnected);
		ConnectivityMonitor.RemoveConnectionHandler(OnNetworkConnected);
	}

	private void OnApplicationQuit()
	{
		if (!PenaltyApplied && State != 0)
		{
			mIsApplicationQuit = true;
			if (State == RacingManagerState.InLobby)
			{
				ImmediateAddPenalty(DNFType.LobbyForceQuitGame, force: false);
			}
			else if (State == RacingManagerState.Racing || State == RacingManagerState.RaceCountdown || State == RacingManagerState.WaitingForPlayers)
			{
				ImmediateAddPenalty(DNFType.RaceForceQuitGame, force: false);
			}
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (pIsSinglePlayer || State == RacingManagerState.None)
		{
			return;
		}
		if (pause)
		{
			mTimeGamePause = ServerTime.pCurrentTime;
			if (State == RacingManagerState.InLobby)
			{
				ImmediateAddPenalty(DNFType.LobbyForceQuitGame, force: false);
			}
			else if (State == RacingManagerState.Racing || State == RacingManagerState.RaceCountdown || State == RacingManagerState.WaitingForPlayers)
			{
				ImmediateAddPenalty(DNFType.RaceForceQuitGame, force: false);
			}
		}
		else
		{
			ImmediateRemovePenalty(mRaceFinishReason, force: true);
			TimeElapsedGamePause = (ServerTime.pCurrentTime - mTimeGamePause).Seconds;
		}
	}

	public void OnNetworkConnected()
	{
		mNetworkConnected = true;
		if (!pIsSinglePlayer && State != 0 && !PenaltyApplied && mNetworkConnected && MainStreetMMOClient.pInstance.pState == MMOClientState.IN_ROOM)
		{
			ImmediateRemovePenalty(mRaceFinishReason, force: true);
		}
	}

	public void OnNetworkDisconnected()
	{
		mNetworkConnected = false;
		if (!pIsSinglePlayer && State != 0 && !PenaltyApplied)
		{
			if (State == RacingManagerState.InLobby)
			{
				ImmediateAddPenalty(DNFType.LobbyNetworkDisconnect, force: false);
			}
			else if (State == RacingManagerState.Racing || State == RacingManagerState.RaceCountdown || State == RacingManagerState.WaitingForPlayers)
			{
				ImmediateAddPenalty(DNFType.RaceNetworkDisconnect, force: false);
			}
		}
	}

	public void ImmediateAddPenalty(DNFType type, bool force)
	{
		if (!pIsSinglePlayer)
		{
			TrySetFinishReason(type, force);
			AddPenalty(type);
		}
	}

	public void ImmediateRemovePenalty(DNFType type, bool force)
	{
		if (!pIsSinglePlayer)
		{
			RemovePenalty(mRaceFinishReason);
			TrySetFinishReason(DNFType.Default, force);
		}
	}

	public DNFType GetFinishReason()
	{
		return mRaceFinishReason;
	}

	public bool TrySetFinishReason(DNFType reason, bool force = false)
	{
		if ((!PenaltyApplied && (mRaceFinishReason != DNFType.RaceNetworkDisconnect || mRaceFinishReason != DNFType.LobbyNetworkDisconnect)) || force)
		{
			mRaceFinishReason = reason;
			return true;
		}
		return false;
	}

	public void Reset()
	{
		TimeElapsedGamePause = 0;
		PenaltyApplied = false;
		TrySetFinishReason(DNFType.Default, force: true);
	}

	public void ExitLobby()
	{
		if (!pIsSinglePlayer && !PenaltyApplied)
		{
			AddPenalty(mRaceFinishReason);
		}
	}

	public void ExitRacing()
	{
		if (!pIsSinglePlayer && !PenaltyApplied)
		{
			AddPenalty(mRaceFinishReason);
		}
		ProcessExitRacing();
		RsResourceManager.LoadLevel(RsResourceManager.pLastLevel);
	}

	public void RemovePreviousPenalty(bool canSave = false)
	{
		mPenaltyDataValue.UpdatePenaltyCount(mRaceFinishReason, -1);
		PenaltyApplied = false;
		TrySetFinishReason(DNFType.Default);
		if (canSave)
		{
			SavePairData();
		}
	}

	private void AddPenalty(DNFType type)
	{
		if (type != 0)
		{
			mPenaltyDataValue.UpdatePenaltyCount(type, 1);
			CheckForPenalty(type);
			SavePairData();
			PenaltyApplied = true;
		}
	}

	public void RemovePenalty(DNFType type)
	{
		mPenaltyDataValue.UpdatePenaltyCount(mRaceFinishReason, -1);
		PenaltyApplied = false;
		CheckForPenalty(type);
		SavePairData();
	}

	private void CheckForPenalty(DNFType type)
	{
		int penaltyCount = mPenaltyDataValue.GetPenaltyCount(type);
		mPenaltyDataValue.EndTime = DateTime.MinValue;
		PenaltyDurationGroup penaltyDurationGroup = null;
		PenaltyDurationGroup[] penaltyDurationGroup2 = _PenaltyDurationGroup;
		foreach (PenaltyDurationGroup penaltyDurationGroup3 in penaltyDurationGroup2)
		{
			if (penaltyDurationGroup3._GroupType == type)
			{
				for (int num = penaltyDurationGroup3._PenaltyDuration.Length - 1; num >= 0; num--)
				{
					if (penaltyCount >= penaltyDurationGroup3._PenaltyDuration[num]._Count)
					{
						mPenaltyDataValue.PenaltyType = State;
						mPenaltyDataValue.EndTime = ServerTime.pCurrentTime.AddMinutes(penaltyDurationGroup3._PenaltyDuration[num]._DurationInMinute);
						break;
					}
				}
				return;
			}
			if (penaltyDurationGroup3._GroupType == DNFType.Default)
			{
				penaltyDurationGroup = penaltyDurationGroup3;
			}
		}
		if (penaltyDurationGroup == null)
		{
			return;
		}
		for (int num2 = penaltyDurationGroup._PenaltyDuration.Length - 1; num2 >= 0; num2--)
		{
			if (penaltyCount >= penaltyDurationGroup._PenaltyDuration[num2]._Count)
			{
				mPenaltyDataValue.PenaltyType = State;
				mPenaltyDataValue.EndTime = ServerTime.pCurrentTime.AddMinutes(penaltyDurationGroup._PenaltyDuration[num2]._DurationInMinute);
				break;
			}
		}
	}

	public DateTime GetPenaltyEndTime()
	{
		return mPenaltyDataValue.EndTime;
	}

	public TimeSpan GetPenaltyBlockDuration()
	{
		if (mPenaltyDataValue != null)
		{
			return mPenaltyDataValue.EndTime - ServerTime.pCurrentTime;
		}
		return TimeSpan.Zero;
	}

	public RacingManagerState GetPenaltyType()
	{
		if (mPenaltyDataValue != null)
		{
			return mPenaltyDataValue.PenaltyType;
		}
		return RacingManagerState.None;
	}

	public static void MarkTransitionTime()
	{
		mTransitionTime = Time.time;
	}

	public static float TimeSinceTransition()
	{
		return Time.time - mTransitionTime;
	}

	public static void ProcessExitRacing()
	{
		TimeHackPrevent.Reset();
		AvAvatar.pToolbar = null;
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
		}
		AvatarRacing component = AvAvatar.pObject.GetComponent<AvatarRacing>();
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
		AvAvatar.pAvatarCam.SetActive(value: true);
		PathManager.Destroy();
		MainStreetMMOClient.pInstance.pIgnoreIdleTimeOut = false;
		pIsSinglePlayer = false;
	}

	public void CheckResetRequired()
	{
		DateTime dateTime = new DateTime(ServerTime.pCurrentTime.Year, ServerTime.pCurrentTime.Month, ServerTime.pCurrentTime.Day);
		if (mPenaltyDataValue != null && mPenaltyDataValue.TimeStamp < dateTime)
		{
			ResetPenalty();
		}
	}

	public void ResetPenalty()
	{
		mPenaltyDataValue.Reset();
		SavePairData();
		if (PlayerPrefs.HasKey(_PenaltyDataKey + mUserId))
		{
			PlayerPrefs.DeleteKey(_PenaltyDataKey + mUserId);
		}
	}

	private void SavePairData()
	{
		if (mNetworkConnected && !mIsApplicationQuit)
		{
			if (mPairData == null)
			{
				mPairData = new PairData();
			}
			mPairData.SetValue(_PenaltyDataKey, mPenaltyDataValue.Serialize());
			UtDebug.Log("RacingManager: SavePairData: " + mPairData.GetValue(_PenaltyDataKey), 768u);
			if (mPairData._IsDirty)
			{
				UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
				mPairData.PrepareArray();
				WsWebService.SetKeyValuePairByUserID(mUserId, _PenaltyPairDataID, mPairData, ServiceEventHandler, null);
			}
		}
		else
		{
			SavePlayerPref();
		}
	}

	private void ReadPairData()
	{
		if (mNetworkConnected)
		{
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			WsWebService.GetKeyValuePairByUserID(mUserId, _PenaltyPairDataID, ServiceEventHandler, null);
			Ready = false;
		}
		else
		{
			ReadAndSyncPlayerPref();
		}
	}

	private void SavePlayerPref()
	{
		PlayerPrefs.SetString(_PenaltyDataKey + mUserId, mPenaltyDataValue.Serialize());
		mPlayerPrefDataSync = false;
	}

	private void ReadAndSyncPlayerPref()
	{
		if (!PlayerPrefs.HasKey(_PenaltyDataKey + mUserId))
		{
			return;
		}
		string @string = PlayerPrefs.GetString(_PenaltyDataKey + mUserId);
		if (@string != null && @string.Length < 1)
		{
			PlayerPrefs.DeleteKey(_PenaltyDataKey + mUserId);
			mPlayerPrefDataSync = true;
			return;
		}
		PenaltySaveData penaltySaveData = PenaltySaveData.Deserialize(@string);
		DateTime dateTime = new DateTime(ServerTime.pCurrentTime.Year, ServerTime.pCurrentTime.Month, ServerTime.pCurrentTime.Day);
		if (mPenaltyDataValue != null && mPenaltyDataValue.TimeStamp < dateTime)
		{
			PlayerPrefs.DeleteKey(_PenaltyDataKey + mUserId);
			mPlayerPrefDataSync = true;
		}
		else if (penaltySaveData.TimeStamp > mPenaltyDataValue.TimeStamp)
		{
			mPenaltyDataValue = PenaltySaveData.Deserialize(@string);
			SavePairData();
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_KEY_VALUE_PAIR_BY_USER_ID:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inObject != null)
				{
					mPairData = (PairData)inObject;
					mPairData.Init();
					mPairData._IsDirty = false;
					if (mPairData.KeyExists(_PenaltyDataKey))
					{
						mPenaltyDataValue = PenaltySaveData.Deserialize(mPairData.GetValue(_PenaltyDataKey));
					}
					else
					{
						mPenaltyDataValue = new PenaltySaveData();
					}
					if (!mPlayerPrefDataSync)
					{
						ReadAndSyncPlayerPref();
					}
					UtDebug.Log("RacingManager: ReadData: " + mPairData.GetValue(_PenaltyDataKey), 768u);
					Ready = true;
					UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				}
				break;
			case WsServiceEvent.ERROR:
				mPenaltyDataValue = new PenaltySaveData();
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				Ready = true;
				break;
			}
			break;
		case WsServiceType.SET_KEY_VALUE_PAIR_BY_USER_ID:
			switch (inEvent)
			{
			case WsServiceEvent.ERROR:
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				break;
			case WsServiceEvent.COMPLETE:
				if (mPairData != null)
				{
					mPairData._IsDirty = false;
				}
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				if (!mPlayerPrefDataSync)
				{
					PlayerPrefs.DeleteKey(_PenaltyDataKey + mUserId);
					mPlayerPrefDataSync = true;
				}
				break;
			}
			break;
		}
	}
}
