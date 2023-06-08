using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JSGames.UI;
using JSGames.UI.Util;
using KA.Framework;
using KnowledgeAdventure.Multiplayer;
using KnowledgeAdventure.Multiplayer.Events;
using KnowledgeAdventure.Multiplayer.Model;
using KnowledgeAdventure.Multiplayer.SmartFox;
using Newtonsoft.Json;
using Sfs2X;
using Sfs2X.Entities;
using Sfs2X.Entities.Variables;
using UnityEngine;

public class MainStreetMMOClient : KAMonoBase
{
	public string _DisableMMOAssetName = "PfGrpQMMO_Off";

	public string _GenericDBAssetName = "PfUIGenericDB";

	public const string USE_MMO = "USE_MMO";

	private const string MESSAGE_ID = "M";

	private const string WRAPPER_EXTENSION = "we";

	private const string PUBLIC_MESSAGES = "PM";

	public const string USERID_USER_VARIABLE = "UID";

	private const string AVATAR_USER_VARIABLE = "A";

	private const string POSITION_USER_VAR_P1 = "P1";

	private const string POSITION_USER_VAR_P2 = "P2";

	private const string POSITION_USER_VAR_P3 = "P3";

	private const string VELOCITY_USER_VAR_R1 = "R1";

	private const string VELOCITY_USER_VAR_R2 = "R2";

	private const string VELOCITY_USER_VAR_R3 = "R3";

	private const string SERVER_TIME = "ST";

	private const string MECANIM_BITFIELD = "MBF";

	private const string NETWORK_TIME = "NT";

	private const string POSITION_USER_VARIABLE = "P";

	private const string ROTATION_USER_VARIABLE = "R";

	private const string MAXSPEED_USER_VARIABLE = "MX";

	private const string UDP_TIME_STAMP = "t";

	private const string USER_EVENT_VARIABLE = "UE";

	private const string FLAGS_USER_VARIABLE = "F";

	private const string LEVEL_USER_VARIABLE = "L";

	private const string RANK_USER_VARIABLE = "RA";

	private const string MEMBER_USER_VARIABLE = "M";

	private const string COUNTRY_USER_VARIABLE = "CU";

	private const string CLAN_USER_VARIABLE = "CLU";

	private const string MOOD_USER_VARIABLE = "MU";

	private const string RIDE_USER_VARIABLE = "RDE";

	private const string JOIN_USER_VARIABLE = "J";

	private const string RAISEDPET_USER_VARIABLE = "FP";

	private const string BUSY_USER_VARIABLE = "BU";

	private const string PET_USER_VARIABLE = "PU";

	private const string UDT_USER_VARIABLE = "UDT";

	private const string AVERAGE_PING = "AVG_PNG";

	private const string PITCH_USER_VARIABLE = "CUP";

	private const string DRAGON_POSITION_USER_VAR_P1 = "DP1";

	private const string DRAGON_POSITION_USER_VAR_P2 = "DP2";

	private const string DRAGON_POSITION_USER_VAR_P3 = "DP3";

	private const string DRAGON_STATE = "DS";

	private const char MESSAGE_SEPARATOR = ':';

	private const string EMOTICON_MESSAGE = "EID";

	private const string ACTION_MESSAGE = "AID";

	private const string SET_DISPLAY_NAME_MESSAGE = "SDN";

	private const string LAUNCHPAD_MESSAGE = "LP";

	private const string SPRINGBOARD_MESSAGE = "SB";

	private const string SEND_BUDDY_MESSAGE = "SBE";

	private const string SLIDE_MESSAGE = "SL";

	private const string ZIPLINE_MESSAGE = "ZL";

	private const string UPDATE_POSITION_MESSAGE = "UP";

	private const string CANNED_CHAT_MESSAGE = "C";

	private const string CHAT_MESSAGE_RECEIVED = "CMR";

	private const string CHAT_MESSAGE_FAILURE = "SCF";

	private const string SEND_MESSAGE_FAILED = "SMF";

	private const string CHAT_MESSAGE_ACCEPTED = "SCA";

	private const string GET_POSITION_MESSAGE = "GPE";

	private const string GET_POSITION_SPLINE_MESSAGE = "GPSM";

	private const string GET_ELAPSEDTIME_MESSAGE = "RTM";

	private const string COUCH_SIT_MESSAGE = "CS";

	private const string SEND_SCORE_MESSAGE = "SS";

	private const string CARRY_OBJECT_MESSAGE = "CO";

	private const string DROP_OBJECT_MESSAGE = "DO";

	private const string SEND_OBJECT_MESSAGE = "SO";

	private const string SPELL_CAST_MESSAGE = "SC";

	private const string ATTACH_CAST_TOOL_MESSAGE = "ASCT";

	private const string DETATCH_CAST_TOOL_MESSAGE = "DSCT";

	private const string NEW_MESSAGE_POST = "NMP";

	private const string SEND_PRIVATE_PARTY_INVITATION = "SPI";

	private const string SEND_MODERATION_MESSAGE = "SMM";

	private const string RESPONSE_USER_CHAT_BANNED = "CB";

	private const string RESPONSE_CHAT_MESSAGE_BLOCKED = "MB";

	private const string TRADE_REQUEST_MESSAGE = "TR";

	private const string SEND_PENDING_MESSAGE_NOTIFICATION = "SPMN";

	private const string SEND_CHALLENGE_BUDDY_EVENT = "SCBE";

	private const string USER_DISCONNECT_LOGOUT = "UDL";

	private const string PET_MOOD_PARTICLE_MESSAGE = "PRT";

	private const string EXT_MSG_TYPE_UNKNOWN = "0";

	private const string EXT_MSG_TYPE_ADD = "1";

	private const string EXT_MSG_TYPE_REMOVE = "2";

	private const string EXT_MSG_TYPE_BLOCK = "3";

	private const string EXT_MSG_TYPE_APPROVE = "4";

	private const string EXT_MSG_TYPE_INVITE = "5";

	private const string EXT_MSG_TYPE_LOGON = "6";

	private const string EXT_MSG_TYPE_LOGOFF = "7";

	private const string EXT_MSG_TYPE_SCENE_CHANGED = "8";

	private const string EXT_MSG_TYPE_NEIGHBOR_CHANGED = "9";

	private const string EXT_MSG_TYPE_HOUSE_CHANGED = "10";

	private const string EXT_MSG_TYPE_MYROOM_CHANGED = "12";

	private const string EXT_MSG_TYPE_PRIVATEPARTY_INVITE = "14";

	private const string ROOT_ZONE = "JumpStart";

	private const int AVATAR_GROW_SIZE = 10;

	private static MainStreetMMOClient mInstance = null;

	public static uint LOG_MASK = 16u;

	public static uint LOG_MASK2 = 64u;

	public static string UserRoomID = string.Empty;

	private GameObject mUiGenericDB;

	private string mIPAddress = "ka-v-devmmo2.adventurena.com";

	private int mPort = 9933;

	private string mZone = "";

	private string mLastZone = "";

	private Hashtable mZoneList;

	private int mRoomID = -1;

	private IMMOClient mClient = MMOClientFactory.Instance;

	private MMOLevelData mMMOLevelData;

	private MMOClientState mState;

	private bool mSaveOwnerIDOnLoad;

	private Transform mBuddyTeleportMarker;

	private bool mConnectToRoom = true;

	private Hashtable mLevelOwnerIDList;

	private bool mWaitingForRoomListUpdated;

	private string mLevelToBeLoaded = "";

	private Hashtable mSpecialZoneList;

	private List<string> mRacingLevelList;

	private static List<MMOClient> mMMOClientList = new List<MMOClient>();

	private List<MMOPlayerToLoad> mPlayersToLoadList = new List<MMOPlayerToLoad>();

	private List<MMOAvatar> mPlayersToDeleteList = new List<MMOAvatar>();

	public static int mMMOAvatarLimit = 50;

	public static bool _DefaultMMOStateSet = false;

	private int mUserVariableCalledFrameNumber;

	private int mMMOSpawnCount;

	private float mMMOWaitTimer;

	private const float mMMOWaitTimeout = 5f;

	private List<MMOBuddyMessage> mBuddyMessageList = new List<MMOBuddyMessage>();

	private MMOStats mMMOStats = new MMOStats();

	private MMOAvatarUserVarData mLocalSentData = new MMOAvatarUserVarData();

	private Vector3[] mPosHistory = new Vector3[5];

	private float mReconnectTime = 2f;

	private float mReconnectTimer;

	private float mDisconnectTime = 240f;

	private float mDisconnectTimer;

	private GameObject mIdleTimeOutGenericDB;

	private GameObject mPausedBy;

	private bool mAllDeactivated;

	private bool mResetMMOStatus;

	public static bool _IgnoreSenderMessage = true;

	private GameObject mNotifyObjOnIdlePopUpClose;

	private bool mPauseRemoteAvatar;

	private bool mMMOEnabled = true;

	private float mJoinTime = 10f;

	private float mJoinTimer;

	[NonSerialized]
	private BuddyLocation mJoinBuddy;

	private string mLoadToPlayer = "";

	private Vector3 mLastPosition = Vector3.zero;

	private float mLastRotation;

	private MMOAvatarFlags mLastFlags;

	private int mForceSendCount;

	private float mMMOUpdateTime;

	private const float mMMOFastUpdateFrequency = 0.1f;

	private const float mMMONormalUpdateFrequency = 0.333f;

	private const float mMMOSlowUpdateFrequency = 1.5f;

	private const float mMMOIdleUpdateFrequency = 10f;

	private Dictionary<string, MMOAvatar> mPlayerList = new Dictionary<string, MMOAvatar>();

	private Dictionary<string, MMOAvatarUserVarData> mRoomPlayersUserVarData = new Dictionary<string, MMOAvatarUserVarData>();

	private MMORoomVariablesChangedEventHandler mUserRoomVariableHandlers;

	private MMOMessageReceivedEventHandler mUserMessageReceivedHandlers;

	private Dictionary<int, GameObject> mGetPositionList = new Dictionary<int, GameObject>();

	private List<GameObject> mGetElapsedTimeList = new List<GameObject>();

	private Dictionary<string, MMOExtensionResponseReceivedEventHandler> mExtensionResponseHandlers = new Dictionary<string, MMOExtensionResponseReceivedEventHandler>();

	private bool mOldClickableState;

	private AvAvatarState mOldAvatarState;

	private bool mIsConnectToBuddy;

	private bool mIsLoginToBuddyZone;

	private string mPreviousIP = "";

	private bool mIsSwitchingBack;

	private MMOJoinStatus mJoinedAllowed = MMOJoinStatus.ALLOWED;

	private string mCurrentLevel = "";

	private bool mBusy;

	private bool mUseUDP;

	private float mLastChatMessageTime = float.MaxValue;

	private int mSpamCount;

	private int mSpamThreshold = 5;

	public bool pIsSilenced;

	private GameObject mSilenceDB;

	private string mWarningMessageText = "";

	private float mSilencingTimer;

	private bool mNeedToShowModeration;

	private string mRoomName = "";

	private MMOLoginRequest mLoginRequest = new MMOLoginRequest();

	private MMOForceVersion mForceVersion = new MMOForceVersion();

	private static bool mBlockModeration = false;

	private static bool mForceShowModeration = false;

	private MMOTimeManager mTimeManager;

	private bool mIgnoreIdleTimeOut;

	private bool mForceUpdateServer;

	public static MainStreetMMOClient pInstance => mInstance;

	public string pZone => mZone;

	public MMOClientState pState => mState;

	public bool pSaveOwnerIDOnLoad
	{
		get
		{
			return mSaveOwnerIDOnLoad;
		}
		set
		{
			mSaveOwnerIDOnLoad = value;
		}
	}

	public Transform pBuddyTeleportMarker
	{
		get
		{
			return mBuddyTeleportMarker;
		}
		set
		{
			mBuddyTeleportMarker = value;
		}
	}

	public List<MMOAvatar> pPlayersToDeleteList => mPlayersToDeleteList;

	public static bool pIsMMOAvatarsReady
	{
		get
		{
			if (mInstance == null || !pIsMMOEnabled)
			{
				return true;
			}
			if (Time.realtimeSinceStartup - mInstance.mMMOWaitTimer > 5f)
			{
				return true;
			}
			if (mInstance.mState != MMOClientState.IN_ROOM || mInstance.mUserVariableCalledFrameNumber == 0)
			{
				return false;
			}
			return mInstance.mMMOSpawnCount <= 0;
		}
	}

	public float pDisconnectTimer => mDisconnectTimer;

	public GameObject pIdleTimeOutGenericDB => mIdleTimeOutGenericDB;

	public bool pAllDeactivated => mAllDeactivated;

	public GameObject pNotifyObjOnIdlePopUpClose
	{
		set
		{
			mNotifyObjOnIdlePopUpClose = value;
		}
	}

	public bool pPauseRemoteAvatar
	{
		get
		{
			return mPauseRemoteAvatar;
		}
		set
		{
			mPauseRemoteAvatar = value;
		}
	}

	public static bool pIsMMOEnabled
	{
		get
		{
			if (mInstance == null)
			{
				return false;
			}
			return mInstance.mMMOEnabled;
		}
		set
		{
			if (mInstance != null && mInstance.mMMOEnabled != value)
			{
				mInstance.mMMOEnabled = value;
				if (mInstance.mMMOEnabled)
				{
					pInstance.Connect();
				}
				else
				{
					pInstance.Disconnect();
				}
			}
		}
	}

	public static bool pIsReady
	{
		get
		{
			if (mInstance != null)
			{
				return mInstance.mState == MMOClientState.IN_ROOM;
			}
			return false;
		}
	}

	public Dictionary<string, MMOAvatar> pPlayerList => mPlayerList;

	public Dictionary<string, MMOAvatarUserVarData> pRoomPlayersUserVarData => mRoomPlayersUserVarData;

	public string pRoomName => mRoomName;

	private bool pCanShowModeration
	{
		get
		{
			if (!mForceShowModeration)
			{
				if (!mBlockModeration && !RsResourceManager.pLevelLoadingScreen && AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pToolbar != null)
				{
					return AvAvatar.pToolbar.activeInHierarchy;
				}
				return false;
			}
			return true;
		}
	}

	public static bool pBlockModeration
	{
		get
		{
			return mBlockModeration;
		}
		set
		{
			mBlockModeration = value;
		}
	}

	public static bool pForceShowModeration
	{
		get
		{
			return mForceShowModeration;
		}
		set
		{
			mForceShowModeration = value;
		}
	}

	public bool pIgnoreIdleTimeOut
	{
		get
		{
			return mIgnoreIdleTimeOut;
		}
		set
		{
			mIgnoreIdleTimeOut = value;
		}
	}

	public static void DestroyPool()
	{
	}

	public static void ExpandPool()
	{
	}

	public static void Init()
	{
		mMMOAvatarLimit = UtPlatform.GetDeviceAvatarLimit();
		if (MainStreetMMOPlugin.mObject == null)
		{
			MainStreetMMOPlugin.mObject = new GameObject("MainStreetMMOClient");
			UnityEngine.Object.DontDestroyOnLoad(MainStreetMMOPlugin.mObject);
			mInstance = MainStreetMMOPlugin.mObject.AddComponent<MainStreetMMOClient>();
			mInstance.mTimeManager = MainStreetMMOPlugin.mObject.AddComponent<MMOTimeManager>();
		}
		else if (mInstance.mState == MMOClientState.DISCONNECTING)
		{
			mInstance.mState = MMOClientState.NOT_CONNECTED;
		}
		if (ProductConfig.pIsReady)
		{
			if (!string.IsNullOrEmpty(ProductConfig.pInstance.MMOServer))
			{
				mInstance.mIPAddress = ProductConfig.pInstance.MMOServer;
			}
			if (ProductConfig.pInstance.MMOServerPort.HasValue)
			{
				mInstance.mPort = ProductConfig.pInstance.MMOServerPort.Value;
			}
			if (ProductConfig.pInstance.MMOIdleTimeout.HasValue)
			{
				mInstance.mDisconnectTime = ProductConfig.pInstance.MMOIdleTimeout.Value;
			}
		}
		if (!_DefaultMMOStateSet)
		{
			_DefaultMMOStateSet = true;
			string key = "USE_MMO" + UserInfo.pInstance.UserID;
			if (!UserInfo.pInstance.MultiplayerEnabled)
			{
				pIsMMOEnabled = false;
			}
			else if (PlayerPrefs.HasKey(key))
			{
				pIsMMOEnabled = PlayerPrefs.GetInt(key, 1) == 1;
			}
			else
			{
				pIsMMOEnabled = UtPlatform.GetMMODefaultState();
			}
		}
		if (pIsMMOEnabled)
		{
			if (!mInstance.mForceUpdateServer)
			{
				mInstance.LoginToZone();
				return;
			}
			mInstance.Disconnect();
			mInstance.Connect();
		}
	}

	public static void AddClient(MMOClient client)
	{
		mMMOClientList.Add(client);
	}

	public static void RemoveClient(MMOClient client)
	{
		if (mMMOClientList.Contains(client))
		{
			mMMOClientList.Remove(client);
		}
	}

	private IEnumerator SetObjectActive(GameObject go, bool isEnabled)
	{
		yield return null;
		if (go != null)
		{
			go.SetActive(isEnabled);
		}
	}

	public static GameObject GetAvatar()
	{
		GameObject gameObject = null;
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfAvatar"));
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
		AvAvatarController component = gameObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component.SoftReset();
		}
		return gameObject;
	}

	private void Awake()
	{
		mMMOLevelData = GameDataConfig.pInstance.MMOLevelData;
		mZoneList = new Hashtable();
		for (int i = 0; i < mMMOLevelData.Zones.Length; i++)
		{
			mZoneList.Add(mMMOLevelData.Zones[i], mMMOLevelData.Zones[i]);
		}
		mLevelOwnerIDList = new Hashtable();
		string[] playerOwnedLevels = mMMOLevelData.PlayerOwnedLevels;
		foreach (string key in playerOwnedLevels)
		{
			mLevelOwnerIDList.Add(key, "");
		}
		mSpecialZoneList = new Hashtable();
		SpecialZoneData[] specialZoneData = mMMOLevelData.SpecialZoneData;
		foreach (SpecialZoneData specialZoneData2 in specialZoneData)
		{
			mSpecialZoneList.Add(specialZoneData2.SpecialZone, specialZoneData2);
		}
		mRacingLevelList = new List<string>();
		playerOwnedLevels = mMMOLevelData.RacingLevels;
		foreach (string item in playerOwnedLevels)
		{
			mRacingLevelList.Add(item);
		}
		mClient.IsDebugMode = false;
		mWaitingForRoomListUpdated = false;
		mLevelToBeLoaded = "";
		if (mClient != null)
		{
			mClient.IsPrefetchSocketPolicyAllowed = false;
			mClient.IsQueueMode = true;
			StartCoroutine(NetworkUpdate());
			mClient.Error += Error;
			mClient.Connected += Connected;
			mClient.ExtensionResponseReceived += ExtensionResponseReceived;
			mClient.LoggedIn += LoggedIn;
			mClient.LoggedOut += LoggedOut;
			mClient.JoinedRoom += JoinedRoom;
			mClient.LeftRoom += LeftRoom;
			mClient.MMOMessageReceived += MessageReceived;
			mClient.RoomVariablesChanged += RoomVariablesChanged;
			mClient.RoomListUpdated += RoomListUpdated;
			mClient.DebugMessageReceived += DebugMessageReceived;
			mClient.UserJoinedRoom += UserJoinedRoom;
			mClient.UserLeftRoom += UserLeftRoom;
			mClient.UserVariablesChanged += UserVariablesChanged;
		}
	}

	private IEnumerator NetworkUpdate()
	{
		WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
		while (true)
		{
			yield return waitForEndOfFrame;
			try
			{
				if (mClient != null)
				{
					mClient.ProcessQueue();
					continue;
				}
				break;
			}
			catch (Exception ex)
			{
				UtDebug.LogError("Exception when Processing Network " + ex.ToString());
			}
		}
	}

	private void Update()
	{
		if (mClient == null)
		{
			return;
		}
		bool flag = false;
		while (mPlayersToDeleteList.Count > 0)
		{
			UtDebug.Log("MMO: Deleting " + mPlayersToDeleteList[0].pUserID, LOG_MASK);
			mPlayersToDeleteList[0].Unload();
			UnityEngine.Object.Destroy(mPlayersToDeleteList[0].gameObject);
			mPlayersToDeleteList.RemoveAt(0);
			flag = true;
		}
		if (flag)
		{
			RsResourceManager.UnloadUnusedAssets();
		}
		if (pIsSilenced)
		{
			if (mSilencingTimer > 0f)
			{
				mSilencingTimer -= Time.deltaTime;
			}
			else
			{
				pIsSilenced = false;
				mSilencingTimer = 0f;
				mWarningMessageText = string.Empty;
			}
		}
		if (mNeedToShowModeration && pCanShowModeration)
		{
			mNeedToShowModeration = false;
			DisplaySilenceMessage();
		}
		switch (mState)
		{
		case MMOClientState.NOT_CONNECTED:
			if (ProductConfig.pIsReady && UserInfo.pIsReady && UserInfo.pInstance.MultiplayerEnabled && AvatarData.pInstance != null && AvatarData.pIsReady && !string.IsNullOrEmpty(AvatarData.pInstance.DisplayName))
			{
				if (mIsSwitchingBack)
				{
					mIsSwitchingBack = false;
				}
				Connect();
			}
			break;
		case MMOClientState.LOGGED_IN:
		{
			if (mJoinBuddy == null)
			{
				break;
			}
			mJoinTimer -= Time.deltaTime;
			if (!(mJoinTimer <= 0f))
			{
				break;
			}
			string zone = mJoinBuddy.Zone;
			mJoinBuddy = null;
			TeleportToBuddy(null, null);
			if (mLastZone != "")
			{
				if (!string.IsNullOrEmpty(mPreviousIP) && mPreviousIP != mClient.MMOServerIP)
				{
					ResetConnection();
					break;
				}
				if (zone != mLastZone)
				{
					LoginToZone(mLastZone);
				}
			}
			mLastZone = "";
			break;
		}
		case MMOClientState.IN_ROOM:
		{
			BuddyList.Init();
			if (BuddyList.pIsReady && mBuddyMessageList.Count > 0)
			{
				foreach (MMOBuddyMessage mBuddyMessage in mBuddyMessageList)
				{
					BuddyMessage(mBuddyMessage);
				}
				mBuddyMessageList.Clear();
			}
			if (mPlayersToLoadList.Count > 0)
			{
				MMOPlayerToLoad value = mPlayersToLoadList[0];
				if (!mPlayerList.ContainsKey(value._Username))
				{
					mMMOSpawnCount--;
					mPlayersToLoadList.RemoveAt(0);
					break;
				}
				MMOAvatar mMOAvatar = mPlayerList[value._Username];
				if (value._AvatarData != null)
				{
					if (!mMOAvatar.pReloading)
					{
						UtDebug.Log("MMO: Loading Avatar for " + mMOAvatar.pUserID, LOG_MASK);
						mMOAvatar.PrepareForLoading(value._AvatarData, reload: false);
					}
					value._AvatarData = null;
					mPlayersToLoadList[0] = value;
					foreach (MMOClient mMMOClient in mMMOClientList)
					{
						mMMOClient.AddPlayer(mMOAvatar);
					}
				}
				if (mMOAvatar.pIsReady)
				{
					mMMOSpawnCount--;
					UtDebug.Log("MMO: " + mMOAvatar.pUserID + " Avatar is ready", LOG_MASK);
					mPlayersToLoadList.RemoveAt(0);
					UtUtilities.SetLayerRecursively(mMOAvatar.pObject, LayerMask.NameToLayer("MMOAvatar"));
					if (mJoinBuddy != null && mJoinBuddy.UserID == value._UserID)
					{
						mJoinBuddy = null;
						mMOAvatar.TeleportAvatar();
					}
					mMOAvatar.Activate(!mAllDeactivated);
				}
			}
			if (mJoinBuddy != null)
			{
				mJoinTimer -= Time.deltaTime;
				if (mJoinTimer <= 0f)
				{
					string zone2 = mJoinBuddy.Zone;
					mJoinBuddy = null;
					TeleportToBuddy(null, null);
					if (mLastZone != "")
					{
						if (!string.IsNullOrEmpty(mPreviousIP) && mPreviousIP != mClient.MMOServerIP)
						{
							ResetConnection();
							break;
						}
						if (zone2 != mLastZone)
						{
							LoginToZone(mLastZone);
						}
					}
					mLastZone = "";
				}
			}
			mMMOUpdateTime += Time.deltaTime;
			bool flag2 = true;
			if (AvAvatar.pState != 0 && AvAvatar.pState != AvAvatarState.PAUSED)
			{
				flag2 = MMOEvalUpdate();
			}
			if (Input.anyKey)
			{
				mDisconnectTimer = mDisconnectTime;
			}
			if (!flag2)
			{
				break;
			}
			mDisconnectTimer -= Time.deltaTime;
			if (ProductConfig.pDevFlag || mIgnoreIdleTimeOut || !(mDisconnectTimer <= 0f) || mSpecialZoneList.Contains(mZone))
			{
				break;
			}
			UtDebug.Log("MMO: Disconnecting due to timeout.", LOG_MASK);
			Disconnect(clearZone: true);
			SnChannel.StopPool("VO_Pool");
			if (!RsResourceManager.pLevelLoading && UiErrorHandler.pInstance == null)
			{
				UIGenericDB uIGenericDB = UIUtil.DisplayGenericDB(_GenericDBAssetName, ProductConfig.pInstance.DisconnectText.GetLocalizedString(), ProductConfig.pInstance.DisconnectTitleText.GetLocalizedString(), base.gameObject, "OnClose");
				mIdleTimeOutGenericDB = uIGenericDB.gameObject;
				if (AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pState != 0)
				{
					mOldAvatarState = AvAvatar.pState;
					mOldClickableState = ObClickable.pGlobalActive;
					AvAvatar.pState = AvAvatarState.PAUSED;
					ObClickable.pGlobalActive = false;
					mPausedBy = mIdleTimeOutGenericDB;
				}
			}
			else
			{
				mState = MMOClientState.NOT_CONNECTED;
			}
			break;
		}
		case MMOClientState.CONNECTION_FAILED:
			mReconnectTimer -= Time.deltaTime;
			if (mReconnectTimer <= 0f)
			{
				Connect();
			}
			break;
		}
	}

	public bool MMOEvalUpdate()
	{
		Transform mTransform = AvAvatar.mTransform;
		float num = 0f;
		float num2 = ((AvAvatar.mNetworkVelocity.magnitude > Mathf.Epsilon) ? 0.5f : 0.1f);
		float num3 = 0.25f;
		if (mLocalSentData._TimeStamp > 0.0)
		{
			Vector3 position = mTransform.position;
			float num4 = (float)((double)Time.time - mLocalSentData._TimeStamp) + num3;
			Vector3 vector = mLocalSentData._Position + mLocalSentData._Velocity * num4;
			num = (position + AvAvatar.mNetworkVelocity * num3 - vector).magnitude;
		}
		float num5 = ((num < num2) ? 1.5f : 0.333f);
		if ((double)Mathf.Abs(Mathf.Round(AvAvatar.mTransform.eulerAngles.y) - mLastRotation) >= 1.0)
		{
			num5 = 0.333f;
		}
		MMOAvatarFlags pSubState = (MMOAvatarFlags)AvAvatar.pSubState;
		if (mLastFlags != pSubState)
		{
			num5 = 0.1f;
		}
		if (mForceSendCount > 0)
		{
			num5 = 0.1f;
		}
		if (mMMOUpdateTime > 0.1f && AvAvatar.mNetworkVelocity.sqrMagnitude + 0.1f < mLocalSentData._Velocity.sqrMagnitude)
		{
			num5 = 0.1f;
		}
		if (AvAvatar.mNetworkVelocity.magnitude < Mathf.Epsilon && mLocalSentData._Velocity.magnitude > Mathf.Epsilon)
		{
			num5 = -1f;
		}
		if (AvAvatar.HasUserEventMsg())
		{
			num5 = -1f;
		}
		if (num5 == 1.5f && AvAvatar.mNetworkVelocity.magnitude < Mathf.Epsilon && AvAvatarAnim.mLocalIdleTime > 1.5f)
		{
			num5 = 10f;
		}
		if (mMMOUpdateTime > num5)
		{
			if (mForceSendCount > 0)
			{
				mForceSendCount--;
			}
			SendUpdate(pSubState);
			mMMOUpdateTime = 0f;
		}
		return num5 == 10f;
	}

	public void InitSmartFoxClient()
	{
		if (mInstance != null && mState > MMOClientState.CONNECTING && ProductConfig.pInstance != null && ProductConfig.pIsReady)
		{
			SmartFoxClientConfig smartFoxClientConfig = new SmartFoxClientConfig();
			smartFoxClientConfig.useBlueBox = ProductConfig.pInstance.MMOUseBlueBox;
			smartFoxClientConfig.debug = ProductConfig.pInstance.MMODebug;
			smartFoxClientConfig.mmoZoneInfoUpdateInterval = ProductConfig.pInstance.MMOZoneInfoUpdateInterval;
			if (ProductConfig.pInstance.MMOHttpPort.HasValue)
			{
				smartFoxClientConfig.httpPort = ProductConfig.pInstance.MMOHttpPort.Value;
			}
			if (ProductConfig.pInstance.MMOBlueBoxPollingRate.HasValue)
			{
				smartFoxClientConfig.blueBoxPollingRate = ProductConfig.pInstance.MMOBlueBoxPollingRate.Value;
			}
			mClient.LoadConfig(smartFoxClientConfig);
			if (ProductConfig.pInstance.MMOUDPPollingRate.HasValue)
			{
				mClient.SetUDPTimerValue(ProductConfig.pInstance.MMOUDPPollingRate.Value);
				return;
			}
			UtDebug.Log("Setting UDP Polling Rate to default value of 500");
			mClient.SetUDPTimerValue(500.0);
		}
	}

	private void ConnectToBuddy(string buddyIP, int buddyPort)
	{
		UtDebug.Log("MMO: Disconnecting to connect to buddy", LOG_MASK);
		mPreviousIP = mClient.MMOServerIP;
		mLastZone = mZone;
		mState = MMOClientState.DISCONNECTING;
		mClient.Disconnect();
		mIPAddress = buddyIP;
		mPort = buddyPort;
		mIsConnectToBuddy = true;
	}

	private void ConnectToBuddy()
	{
		mState = MMOClientState.CONNECTING;
		mForceVersion.ForceVersionIP = mIPAddress;
		mForceVersion.ForceVersionPort = mPort;
		mForceVersion.ForceVersionServerVersion = mClient.MMOServerVersion;
		mForceVersion.ForceVersionRootZone = "JumpStart";
		mLoginRequest.ForceVersion = mForceVersion;
		UtDebug.Log("MMO: Connecting to the buddy server " + mIPAddress + " : " + mPort + " : " + mClient.MMOServerVersion, LOG_MASK);
		if (mClient.MMOServerVersion == "S2X")
		{
			LoginToZone();
		}
	}

	private void SetupLoginRequest()
	{
		string configurationServerURL = ProductConfig.pInstance.ConfigurationServerURL;
		configurationServerURL += "GetMMOServerInfoWithZone";
		mLoginRequest.ApiKey = ProductConfig.pApiKey;
		mLoginRequest.ApiToken = WsWebService.pUserToken;
		if (string.IsNullOrEmpty(ProductConfig.pInstance.MMOServer))
		{
			ProductConfig.pInstance.MMOServer = mIPAddress;
		}
		mLoginRequest.FallBackIP = ProductConfig.pInstance.MMOServer;
		if (!ProductConfig.pInstance.MMOServerPort.HasValue)
		{
			ProductConfig.pInstance.MMOServerPort = mPort;
		}
		mLoginRequest.FallBackPort = ProductConfig.pInstance.MMOServerPort;
		mLoginRequest.ConfigurationUrl = configurationServerURL;
		if (string.IsNullOrEmpty(ProductConfig.pInstance.MMOServerVersion))
		{
			ProductConfig.pInstance.MMOServerVersion = "S2X";
		}
		mLoginRequest.FallBackVersion = ProductConfig.pInstance.MMOServerVersion;
		mLoginRequest.FallBackRootZone = "JumpStart";
		mLoginRequest.UserName = WsWebService.pUserToken;
		mLoginRequest.Password = ProductConfig.pToken;
		mLoginRequest.ForceVersion = null;
	}

	private void SetupForceLogin()
	{
		mForceVersion.ForceVersionIP = mIPAddress;
		mForceVersion.ForceVersionPort = mPort;
		mForceVersion.ForceVersionRootZone = "JumpStart";
		mForceVersion.ForceVersionServerVersion = ((mJoinBuddy != null) ? mJoinBuddy.ServerVersion : ProductConfig.pInstance.MMOServerVersion);
		mLoginRequest.ForceVersion = mForceVersion;
	}

	public void Connect()
	{
		if (mConnectToRoom && pIsMMOEnabled)
		{
			mState = MMOClientState.CONNECTING;
			LoginToZone();
			UtDebug.Log("MMO: Connecting to " + mIPAddress + ":" + mPort, LOG_MASK);
		}
	}

	private void LoginToZone()
	{
		if (mState == MMOClientState.DISCONNECTING || mState == MMOClientState.CONNECTING || mState == MMOClientState.CONNECTED)
		{
			mUserVariableCalledFrameNumber = 0;
			mMMOSpawnCount = 0;
			mMMOWaitTimer = Time.realtimeSinceStartup;
			if (mIsLoginToBuddyZone)
			{
				UtDebug.Log("MMO: Logging in to the buddy zone " + mZone, LOG_MASK);
				mIsLoginToBuddyZone = false;
				if (mJoinBuddy != null)
				{
					mState = MMOClientState.LOGGING_IN;
					DeleteAllPlayers();
					mZone = mJoinBuddy.Zone;
					SendLoginRequest(mJoinBuddy.Zone);
					mJoinTimer = mJoinTime;
				}
			}
			else if (mZoneList.Contains(RsResourceManager.pCurrentLevel))
			{
				mZone = (string)mZoneList[RsResourceManager.pCurrentLevel];
				if (!string.IsNullOrEmpty(mZone))
				{
					mState = MMOClientState.LOGGING_IN;
					SendLoginRequest(mZone);
					UtDebug.Log("MMO: Logging in to " + mZone, LOG_MASK);
				}
			}
		}
		else if (mState == MMOClientState.IN_ROOM && mRoomID == -1)
		{
			if (mLevelOwnerIDList.Contains(RsResourceManager.pCurrentLevel) && ((string)mLevelOwnerIDList[RsResourceManager.pCurrentLevel]).Length > 0)
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary["0"] = (string)mLevelOwnerIDList[RsResourceManager.pCurrentLevel];
				mClient.SendExtensionMessage("le", "JO", dictionary);
			}
			else if (mLevelOwnerIDList.Contains(RsResourceManager.pCurrentLevel) && ((string)mLevelOwnerIDList[RsResourceManager.pCurrentLevel]).Length == 0)
			{
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				dictionary2["0"] = UserInfo.pInstance.UserID;
				mLevelOwnerIDList[RsResourceManager.pCurrentLevel] = UserInfo.pInstance.UserID;
				mClient.SendExtensionMessage("le", "JO", dictionary2);
			}
			else if (mZoneList.Contains(RsResourceManager.pCurrentLevel))
			{
				mClient.SendExtensionMessage("le", "JA", null);
			}
		}
	}

	public void SendLoginRequest(string zone)
	{
		if (mConnectToRoom && ProductConfig.pIsReady && mClient != null)
		{
			bool num = ProductConfig.pInstance.MMOForceVersion | mForceUpdateServer;
			SetupLoginRequest();
			if (num || mJoinBuddy != null)
			{
				SetupForceLogin();
			}
			mLoginRequest.ZoneName = zone;
			string mmoLoginRequest = UtUtilities.ProcessSendObject(mLoginRequest);
			mClient.Login(mmoLoginRequest);
		}
	}

	public void LoginToZone(string zone)
	{
		mLastZone = mZone;
		mZone = zone;
		if (mState == MMOClientState.CONNECTED || string.IsNullOrEmpty(mLastZone))
		{
			UtDebug.Log("MMO: Logging in to " + mZone, LOG_MASK);
			mState = MMOClientState.LOGGING_IN;
			SendLoginRequest(mZone);
		}
		else
		{
			UtDebug.Log("MMO: Logging out from " + mLastZone, LOG_MASK);
			mClient.Logout(forcelogout: false);
			DeleteAllPlayers();
		}
	}

	public void Logout()
	{
		mClient.Logout(forcelogout: false);
	}

	public void Disconnect(bool clearZone = false)
	{
		BuddyList.Reset();
		if (clearZone)
		{
			mZone = "";
		}
		mState = MMOClientState.DISCONNECTING;
		mClient.Disconnect();
		foreach (MMOClient mMMOClient in mMMOClientList)
		{
			mMMOClient.Disconnected();
		}
	}

	public void SendUpdate(MMOAvatarFlags flags)
	{
		if (mState != MMOClientState.IN_ROOM || mRoomID == -1 || (mPauseRemoteAvatar && flags != MMOAvatarFlags.SETPOSITION))
		{
			return;
		}
		Transform mTransform = AvAvatar.mTransform;
		if (mMMOStats.TimeSinceFirstUpdate == 0f)
		{
			mMMOStats.TimeSinceFirstUpdate = Time.fixedTime;
		}
		mMMOStats.TotalSentUpdates++;
		float num = Mathf.Max(0.1f, Time.fixedTime - mMMOStats.TimeSinceFirstUpdate);
		mMMOStats.UpdatesPerSecond = (float)mMMOStats.TotalSentUpdates / num;
		Vector3 position = mTransform.position;
		float y = mTransform.eulerAngles.y;
		Vector3 velocity = ((!AvAvatar.pObject.activeInHierarchy || AvAvatar.pState == AvAvatarState.PAUSED) ? Vector3.zero : AvAvatar.mNetworkVelocity);
		mLastFlags = flags;
		if (mPauseRemoteAvatar)
		{
			position = mLastPosition;
			y = mLastRotation;
		}
		else
		{
			mLastRotation = Mathf.Round(y);
			mLastPosition = new Vector3((float)Math.Round(position.x, 2), (float)Math.Round(position.y, 2), (float)Math.Round(position.z, 2));
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["P1"] = position.x;
		dictionary["P2"] = position.y;
		dictionary["P3"] = position.z;
		dictionary["R1"] = velocity.x;
		dictionary["R2"] = velocity.y;
		dictionary["R3"] = velocity.z;
		dictionary["R"] = mTransform.eulerAngles.y;
		if (AvAvatar.HasUserEventMsg())
		{
			dictionary["UE"] = AvAvatar.GetUserEventMsg(bClear: true);
		}
		dictionary["F"] = (int)flags;
		Transform transform = ((flags == MMOAvatarFlags.GLIDING) ? UtUtilities.FindChildTransform(AvAvatar.pObject, AvatarSettings.pInstance._AvatarName) : AvAvatar.mTransform.Find(AvatarSettings.pInstance._AvatarName));
		if (transform != null)
		{
			AvAvatarAnim component = transform.GetComponent<AvAvatarAnim>();
			if (component != null)
			{
				dictionary["MBF"] = component.mPackState;
			}
		}
		AvAvatarController component2 = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (flags == MMOAvatarFlags.GLIDING && component2 != null)
		{
			dictionary["CUP"] = component2.pFlyingPitch;
		}
		if (component2 != null)
		{
			float num2 = component2.pMaxForwardSpeed;
			if (component2.IsFlyingOrGliding())
			{
				num2 = component2.pMaxFlightSpeed;
			}
			else if (component2.pSubState == AvAvatarSubState.UWSWIMMING)
			{
				num2 = component2.pMaxUWSwimmingSpeed;
			}
			dictionary["MX"] = num2;
		}
		if (MMOConsole._Capture)
		{
			StreamWriter file = MMOConsole._File;
			Vector3 vector = position;
			file.WriteLine("P=" + vector.ToString());
			MMOConsole._File.WriteLine("R=" + mTransform.eulerAngles.ToString());
			MMOConsole._File.WriteLine("F=" + flags.ToString("X"));
		}
		dictionary["t"] = Time.frameCount;
		dictionary["NT"] = MMOTimeManager.pInstance.GetServerTime();
		mClient.UpdatePositionVariables(dictionary, mUseUDP);
		mLocalSentData._Velocity = velocity;
		mLocalSentData._Rotation = mTransform.rotation;
		mLocalSentData._Position = position;
		mLocalSentData._TimeStamp = Time.time;
		mPosHistory[4] = mPosHistory[3];
		mPosHistory[3] = mPosHistory[2];
		mPosHistory[2] = mPosHistory[1];
		mPosHistory[1] = mPosHistory[0];
		mPosHistory[0] = position;
	}

	private void OnApplicationQuit()
	{
		if (mClient != null && mState >= MMOClientState.CONNECTING)
		{
			Disconnect(clearZone: true);
			StopCoroutine("NetworkUpdate");
			mClient.Error -= Error;
			mClient.Connected -= Connected;
			mClient.ExtensionResponseReceived -= ExtensionResponseReceived;
			mClient.LoggedIn -= LoggedIn;
			mClient.LoggedOut -= LoggedOut;
			mClient.JoinedRoom -= JoinedRoom;
			mClient.LeftRoom -= LeftRoom;
			mClient.MMOMessageReceived -= MessageReceived;
			mClient.RoomVariablesChanged -= RoomVariablesChanged;
			mClient.UserJoinedRoom -= UserJoinedRoom;
			mClient.RoomListUpdated -= RoomListUpdated;
			mClient.DebugMessageReceived -= DebugMessageReceived;
			mClient.UserLeftRoom -= UserLeftRoom;
			mClient.UserVariablesChanged -= UserVariablesChanged;
			mClient = null;
		}
	}

	public void SendEmoticon(int id)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			mClient.SendPublicMessage("EID:" + id);
			if (MMOConsole._Capture)
			{
				MMOConsole._File.WriteLine("MSG" + "=" + "EID" + ':' + id);
			}
		}
	}

	public void SendAction(int id)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			mClient.SendPublicMessage("AID:" + id);
			if (MMOConsole._Capture)
			{
				MMOConsole._File.WriteLine("MSG" + "=" + "AID" + ':' + id);
			}
		}
	}

	public void SendChat(int chat)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			mClient.SendPublicMessage("C:" + chat);
		}
	}

	public bool SendChat(string chat, string groupID, int chatType)
	{
		bool result = false;
		mDisconnectTimer = mDisconnectTime;
		if (pIsSilenced && mSilencingTimer > 0f)
		{
			DisplaySilenceMessage();
			return true;
		}
		SpamCheck();
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("chm", chat);
			dictionary.Add("cty", chatType);
			if (ChatData.pCurrentChatOption == ChatOptions.CHAT_CLANS)
			{
				dictionary.Add("tgid", groupID);
			}
			UtDebug.Log("Sending the message", LOG_MASK2);
			mClient.SendExtensionMessage("che", "SCM", dictionary);
			result = true;
		}
		return result;
	}

	public bool SendChat(string chat)
	{
		bool result = false;
		mDisconnectTimer = mDisconnectTime;
		if (pIsSilenced && mSilencingTimer > 0f)
		{
			DisplaySilenceMessage();
			return true;
		}
		SpamCheck();
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("chm", chat);
			dictionary.Add("cty", 1);
			mClient.SendExtensionMessage("che", "SCM", dictionary);
			result = true;
		}
		return result;
	}

	public void SpamCheck()
	{
		float num = Time.time - mLastChatMessageTime;
		mLastChatMessageTime = Time.time;
		if (num < 3f)
		{
			mSpamCount++;
		}
		else
		{
			mSpamCount = 0;
		}
		if (mSpamCount >= mSpamThreshold)
		{
			mSpamCount = 0;
			SetSilenceParams(inAction: true, "You have sent too many chat messages recently", 60f);
		}
	}

	public void SetDisplayName(string name)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			mClient.SendPublicMessage("SDN:" + name);
		}
	}

	public void SetOtherDisplayNamesVisible(bool visible)
	{
		if (mState != MMOClientState.IN_ROOM)
		{
			return;
		}
		foreach (KeyValuePair<string, MMOAvatar> mPlayer in mPlayerList)
		{
			MMOAvatar value = mPlayer.Value;
			if (value != null)
			{
				AvatarData.SetDisplayNameVisible(value.gameObject, visible, value.pIsMember);
			}
		}
	}

	public void SetCountry(int id)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["CU"] = id;
			mClient.SetUserVariables(dictionary);
		}
	}

	public void SetMood(int id)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["MU"] = id;
			mClient.SetUserVariables(dictionary);
		}
	}

	public void UnSetGroup()
	{
		SetGroup(null);
	}

	public void SetGroup(Group inGroup)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (inGroup != null)
			{
				dictionary["CLU"] = inGroup.Name + "|" + inGroup.Logo + "|" + inGroup.Color;
			}
			else
			{
				dictionary["CLU"] = "";
			}
			mClient.SetUserVariables(dictionary);
		}
	}

	public static void ConnectToRoom(bool isConnect)
	{
		if (!(mInstance == null))
		{
			if (!isConnect)
			{
				mInstance.mConnectToRoom = false;
				mInstance.Disconnect();
			}
			else
			{
				mInstance.mConnectToRoom = true;
				mInstance.Connect();
			}
		}
	}

	public void ActivateAll(bool active)
	{
		mAllDeactivated = !active;
		if (mState != MMOClientState.IN_ROOM)
		{
			return;
		}
		foreach (KeyValuePair<string, MMOAvatar> mPlayer in mPlayerList)
		{
			MMOAvatar value = mPlayer.Value;
			if (value != null)
			{
				value.Activate(active);
			}
		}
	}

	public void SetLevel(string level)
	{
		mCurrentLevel = level;
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["L"] = level;
			mClient.SetUserVariables(dictionary);
			mMMOUpdateTime = 0f;
		}
	}

	public void SetMember(bool isMember)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["M"] = isMember;
			mClient.SetUserVariables(dictionary);
		}
	}

	public void SetJoinAllowed(MMOJoinStatus canBeJoined, bool forceUpdate = false)
	{
		if ((!IsLevelRacing() && !mSpecialZoneList.Contains(RsResourceManager.pCurrentLevel)) || forceUpdate)
		{
			mJoinedAllowed = canBeJoined;
			if (mState == MMOClientState.IN_ROOM)
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary["J"] = (int)canBeJoined;
				mClient.SetUserVariables(dictionary);
			}
		}
	}

	public void SetRide()
	{
		SetRide(0);
	}

	public void SetRaisedPetState(AISanctuaryPetFSM petState)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["DS"] = petState;
			mClient.SetUserVariables(dictionary);
		}
	}

	public void SetRaisedPetPosition(Vector3 petPosition)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["DP1"] = petPosition.x;
			dictionary["DP2"] = petPosition.y;
			dictionary["DP3"] = petPosition.z;
			mClient.SetUserVariables(dictionary);
		}
	}

	public void SetRaisedPet(RaisedPetData pData, int mount)
	{
		if (SanctuaryManager.IsPetHatched(pData) || pData.pStage == RaisedPetStage.EGGINHAND)
		{
			SetRaisedPetString(pData.SaveToResStringEx(mount.ToString()));
		}
	}

	public void SetRaisedPetString(string petData)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["FP"] = petData;
			mClient.SetUserVariables(dictionary);
		}
	}

	public void SetRide(int rideItemID)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["RDE"] = rideItemID;
			mClient.SetUserVariables(dictionary);
		}
	}

	public void SetBusy(bool busy)
	{
		mBusy = busy;
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["BU"] = mBusy;
			mClient.SetUserVariables(dictionary);
		}
	}

	public void SetPet(PetDataPet petData)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (petData != null)
			{
				MMOPetData mMOPetData = new MMOPetData();
				mMOPetData.G = petData.Geometry;
				mMOPetData.T = petData.Texture;
				dictionary["PU"] = UtUtilities.JsonSerializeObject(mMOPetData);
			}
			else
			{
				dictionary["PU"] = "";
			}
			mClient.SetUserVariables(dictionary);
		}
	}

	public void Reset()
	{
		string[] playerOwnedLevels = mMMOLevelData.PlayerOwnedLevels;
		foreach (string key in playerOwnedLevels)
		{
			mLevelOwnerIDList[key] = "";
		}
	}

	public void ResetDisconnectTimer()
	{
		mDisconnectTimer = mDisconnectTime;
	}

	public void SendMessageToMMOConsole(string message)
	{
		if (MMOConsole._Capture)
		{
			MMOConsole._File.WriteLine("MSG" + "=" + message);
		}
	}

	public void SendPublicMMOMessage(string message)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			mClient.SendPublicMessage(message);
			SendMessageToMMOConsole(message);
		}
	}

	public void SendPublicExtensionMessage(string message, bool reliable = false)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("M", message);
		if (reliable)
		{
			mClient.SendExtensionMessage("we", "PM", dictionary, useUDP: false);
		}
		else
		{
			mClient.SendExtensionMessage("we", "PM", dictionary);
		}
	}

	public void SendScore(int score, string UIName)
	{
		string message = "SS:" + score + ":" + UIName;
		SendPublicMMOMessage(message);
	}

	public void SendScore(int score, string UIName, string dataString)
	{
		string message = "SS:" + score + ":" + UIName + ":" + dataString;
		SendPublicMMOMessage(message);
	}

	public void SendCarryObject(string name)
	{
		string message = "CO:" + name;
		SendPublicMMOMessage(message);
	}

	public void SendDropObject()
	{
		SendPublicMMOMessage("DO");
	}

	public void SendLaunchPad(string name)
	{
		string text = name.Substring(name.Length - 2);
		string message = "LP:" + text;
		SendPublicMMOMessage(message);
		mMMOUpdateTime = 0f;
	}

	public void SendObjectMessage(string objName, string msgName)
	{
		string message = "SO:" + objName + ":" + msgName;
		SendPublicMMOMessage(message);
		mMMOUpdateTime = 0f;
	}

	public void SendCouchSitMessage(string id)
	{
		string message = "CS:" + id;
		SendPublicMMOMessage(message);
		mMMOUpdateTime = 0f;
	}

	public void SendSpringboard(string name)
	{
		string text = name.Substring(name.Length - 2);
		string message = "SB:" + text;
		SendPublicMMOMessage(message);
		mMMOUpdateTime = 0f;
	}

	public void SendSlide(string name)
	{
		string message = "SL:" + name;
		SendPublicMMOMessage(message);
		mMMOUpdateTime = 0f;
	}

	public void SendZipline(string name)
	{
		string message = "ZL:" + name;
		SendPublicMMOMessage(message);
		mMMOUpdateTime = 0f;
	}

	public void SendExtensionMessage(string inMessage, Dictionary<string, object> inParams)
	{
		SendExtensionMessage("", inMessage, inParams);
	}

	public void SendExtensionMessageToRoom(string inMessage, Dictionary<string, object> inParams)
	{
		SendExtensionMessageToRoom("", inMessage, inParams);
	}

	public void SendExtensionMessageToRoom(string inExtension, string inMessage, Dictionary<string, object> inParams)
	{
		if (inParams == null)
		{
			inParams = new Dictionary<string, object>();
		}
		mClient.SendExtensionMessage(inExtension, inMessage, inParams, mClient.LastJoinedRoom, useUDP: false);
	}

	public void SendExtensionMessage(string inExtension, string inMessage, Dictionary<string, object> inParams)
	{
		if (inParams == null)
		{
			inParams = new Dictionary<string, object>();
		}
		mClient.SendExtensionMessage(inExtension, inMessage, inParams);
	}

	public void SendCounterEvent(string name)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("NAME", name);
			if (mClient.MMOServerVersion == "S2X")
			{
				mClient.SendExtensionMessage("cte", "SCE", dictionary, mClient.LastJoinedRoom, useUDP: false);
			}
			else
			{
				mClient.SendExtensionMessage("cte", "SCE", dictionary);
			}
		}
	}

	public void SendGetPositionEvent(int id, GameObject go)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("ID", id);
			mClient.SendExtensionMessage("pe", "GPE", dictionary);
			mGetPositionList.Add(id, go);
		}
	}

	public void SendGetElapsedTimeEvent(GameObject inGameObject)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			mClient.SendExtensionMessage("cme", "RTM", new Dictionary<string, object>());
			mGetElapsedTimeList.Add(inGameObject);
		}
	}

	public string GetOwnerIDForCurrentZone()
	{
		return GetOwnerIDForZone(mZone);
	}

	public string GetOwnerIDForZone(string zone)
	{
		string result = "";
		string key = "";
		foreach (DictionaryEntry mZone in mZoneList)
		{
			if (zone == (string)mZone.Value)
			{
				key = (string)mZone.Key;
			}
		}
		if (mLevelOwnerIDList.Contains(key))
		{
			result = (string)mLevelOwnerIDList[key];
		}
		return result;
	}

	public string GetOwnerIDForCurrentLevel()
	{
		return GetOwnerIDForLevel(RsResourceManager.pCurrentLevel);
	}

	public string GetOwnerIDForLevel(string level)
	{
		string result = "";
		if (mLevelOwnerIDList.Contains(level))
		{
			result = (string)mLevelOwnerIDList[level];
		}
		return result;
	}

	public bool IsLevelInPlayerOwnedList(string level)
	{
		if (mLevelOwnerIDList.Contains(level))
		{
			return true;
		}
		return false;
	}

	public void SetOwnerIDForLevel(string level, string ownerID)
	{
		mLevelOwnerIDList[level] = ownerID;
	}

	public void RemoveOwnerIDForLevel(string level)
	{
		mLevelOwnerIDList.Remove(level);
	}

	public KnowledgeAdventure.Multiplayer.Model.MMORoom GetRoom()
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			return mClient.GetActiveRoom();
		}
		return null;
	}

	public void SetRoomVariable(string key, string value)
	{
		List<MMORoomVariable> list = new List<MMORoomVariable>();
		list.Add(new MMORoomVariable(key, value, isPrivate: false, isPersistent: true));
		mClient.SetRoomVariables(list);
	}

	public object GetRoomVariable(string varName)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			KnowledgeAdventure.Multiplayer.Model.MMORoom activeRoom = mClient.GetActiveRoom();
			if (activeRoom != null && activeRoom.RoomVariables != null)
			{
				foreach (MMORoomVariable roomVariable in activeRoom.RoomVariables)
				{
					if (roomVariable.Name == varName)
					{
						return roomVariable.Value;
					}
				}
			}
		}
		return null;
	}

	public void UpdateAvatar()
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["A"] = UtUtilities.JsonSerializeObject(AvatarData.pInstance);
			mClient.SetUserVariables(dictionary);
		}
	}

	public void AddMessageReceivedEventHandler(MMOMessageReceivedEventHandler handler)
	{
		UtDebug.Log("MMO: AddMessageReceivedEventHandler " + handler.ToString(), LOG_MASK);
		mUserMessageReceivedHandlers = (MMOMessageReceivedEventHandler)Delegate.Combine(mUserMessageReceivedHandlers, handler);
	}

	public void RemoveMessageReceivedEventHandler(MMOMessageReceivedEventHandler handler)
	{
		UtDebug.Log("MMO: RemoveMessageReceivedEventHandler " + handler.ToString(), LOG_MASK);
		mUserMessageReceivedHandlers = (MMOMessageReceivedEventHandler)Delegate.Remove(mUserMessageReceivedHandlers, handler);
	}

	public void AddRoomVariableEventHandler(MMORoomVariablesChangedEventHandler handler)
	{
		UtDebug.Log("MMO: AddRoomVariableEventHandler " + handler.ToString(), LOG_MASK);
		mUserRoomVariableHandlers = (MMORoomVariablesChangedEventHandler)Delegate.Combine(mUserRoomVariableHandlers, handler);
	}

	public void AddExtensionResponseEventHandler(string Cmd, MMOExtensionResponseReceivedEventHandler handler)
	{
		UtDebug.Log("MMO: AddExtensionResponseEventHandler " + Cmd, LOG_MASK);
		if (mExtensionResponseHandlers.ContainsKey(Cmd))
		{
			Dictionary<string, MMOExtensionResponseReceivedEventHandler> dictionary = mExtensionResponseHandlers;
			dictionary[Cmd] = (MMOExtensionResponseReceivedEventHandler)Delegate.Combine(dictionary[Cmd], handler);
		}
		else
		{
			mExtensionResponseHandlers.Add(Cmd, handler);
		}
	}

	public void RemoveExtensionResponseEventHandler(string Cmd, MMOExtensionResponseReceivedEventHandler handler)
	{
		UtDebug.Log("MMO: RemoveExtensionResponseEventHandler " + Cmd, LOG_MASK);
		if (mExtensionResponseHandlers.ContainsKey(Cmd))
		{
			Dictionary<string, MMOExtensionResponseReceivedEventHandler> dictionary = mExtensionResponseHandlers;
			dictionary[Cmd] = (MMOExtensionResponseReceivedEventHandler)Delegate.Remove(dictionary[Cmd], handler);
		}
	}

	public bool IsJoinAllowed(string inLevel)
	{
		string[] joinNotAllowedLevels = mMMOLevelData.JoinNotAllowedLevels;
		for (int i = 0; i < joinNotAllowedLevels.Length; i++)
		{
			if (joinNotAllowedLevels[i].Equals(inLevel))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsJoinAllowedInActiveMission(string inLevel)
	{
		if (MissionManager.pInstance != null && MissionManager.pInstance.pActiveTasks != null && MissionManager.pInstance.pActiveTasks.Count > 0)
		{
			foreach (Task pActiveTask in MissionManager.pInstance.pActiveTasks)
			{
				List<TaskSetup> setups = pActiveTask.GetSetups();
				if (pActiveTask.pData != null && setups != null && setups.Count > 0)
				{
					foreach (TaskSetup item in setups)
					{
						if (!string.IsNullOrEmpty(item.Scene) && item.Scene.Equals(inLevel) && !string.IsNullOrEmpty(item.Asset) && item.Asset.Contains(_DisableMMOAssetName))
						{
							return false;
						}
					}
				}
				for (Mission mission = pActiveTask._Mission; mission != null; mission = mission._Parent)
				{
					if (mission.pData != null && mission.pData.Setups != null && mission.pData.Setups.Count > 0)
					{
						foreach (MissionSetup setup in mission.pData.Setups)
						{
							if (!string.IsNullOrEmpty(setup.Scene) && setup.Scene.Equals(inLevel) && !string.IsNullOrEmpty(setup.Asset) && setup.Asset.Contains(_DisableMMOAssetName))
							{
								return false;
							}
						}
					}
				}
			}
		}
		return true;
	}

	public bool IsMMOLiteAllowed(string inLevel)
	{
		string[] mMOFullLevels = mMMOLevelData.MMOFullLevels;
		for (int i = 0; i < mMOFullLevels.Length; i++)
		{
			if (mMOFullLevels[i].Equals(inLevel))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsLevelRacing()
	{
		if (mRacingLevelList.Contains(RsResourceManager.pCurrentLevel))
		{
			return true;
		}
		return false;
	}

	public bool IsLevelMMO(string level)
	{
		if (mRacingLevelList.Contains(level))
		{
			return true;
		}
		bool result = false;
		if (mZoneList.Contains(level))
		{
			result = true;
		}
		else
		{
			string[] multiRoomZones = mMMOLevelData.MultiRoomZones;
			foreach (string value in multiRoomZones)
			{
				if (level.Equals(value))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private void LoadLevel(string level)
	{
		UtDebug.Log("MMO: User event handlers cleared!!", LOG_MASK);
		mUserRoomVariableHandlers = null;
		mUserMessageReceivedHandlers = null;
		mExtensionResponseHandlers.Clear();
		mInstance.mTimeManager.ReinitializeTimeManager();
		mNotifyObjOnIdlePopUpClose = null;
		if (mRacingLevelList.Contains(level))
		{
			foreach (MMOClient mMMOClient in mMMOClientList)
			{
				mMMOClient.Reset();
			}
			mZone = "";
		}
		else
		{
			MMOClient[] array = mMMOClientList.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Destroy();
			}
		}
		if (mState < MMOClientState.LOGGING_IN)
		{
			return;
		}
		if (mZoneList.Contains(level))
		{
			if (mLevelOwnerIDList.Contains(RsResourceManager.pLastLevel) && !mSaveOwnerIDOnLoad && level != RsResourceManager.pLastLevel)
			{
				mLevelOwnerIDList[RsResourceManager.pLastLevel] = "";
			}
			mSaveOwnerIDOnLoad = false;
			mBuddyTeleportMarker = null;
			if ((string)mZoneList[level] != mZone)
			{
				UtDebug.Log("MMO: Logging out from " + mZone, LOG_MASK);
				mZone = "";
				mState = MMOClientState.CONNECTED;
				mClient.Logout(forcelogout: false);
				DeleteAllPlayers();
			}
		}
		else if (mRoomID != -1 && !mRacingLevelList.Contains(level))
		{
			DeleteAllPlayers();
			mClient.SendExtensionMessage("le", "JL", null);
		}
	}

	public void LoadLevelComplete(string level)
	{
		mCurrentLevel = "";
		if (mState < MMOClientState.LOGGING_IN || (mZoneList.Contains(level) && !((string)mZoneList[level] == mZone)))
		{
			return;
		}
		SetLevel(level);
		foreach (KeyValuePair<string, MMOAvatar> mPlayer in mPlayerList)
		{
			MMOAvatar value = mPlayer.Value;
			if (value != null)
			{
				value.Activate(value.GetLevel() == level && !mAllDeactivated);
			}
		}
	}

	private AvatarData CreateAvatarData(MMOAvatarData mmoAvatarData)
	{
		return new AvatarData
		{
			Id = mmoAvatarData.Id,
			DisplayName = mmoAvatarData.DisplayName,
			Part = mmoAvatarData.Part,
			GenderType = (Gender)Enum.Parse(typeof(Gender), mmoAvatarData.GenderType, ignoreCase: true)
		};
	}

	private UserAchievementInfo CreateAchievementInfo(MMOAchievementInfo info)
	{
		return new UserAchievementInfo
		{
			AchievementPointTotal = info.P,
			RankID = info.R
		};
	}

	private PetDataPet CreatePetData(MMOPetData mmoPetData)
	{
		return new PetDataPet
		{
			Geometry = mmoPetData.G,
			Texture = mmoPetData.T
		};
	}

	private void CreatePlayer(MMOUser user, bool inSameClan)
	{
		if (!user.UserVariables.ContainsKey("UID") || !user.UserVariables.ContainsKey("A") || user.UserVariables["UID"].ToString().Equals(UserInfo.pInstance.UserID, StringComparison.OrdinalIgnoreCase) || mRoomPlayersUserVarData.ContainsKey(user.Username))
		{
			return;
		}
		MMOAvatarUserVarData mMOAvatarUserVarData = new MMOAvatarUserVarData(user, inSameClan);
		mRoomPlayersUserVarData.Add(user.Username, mMOAvatarUserVarData);
		if (mSpecialZoneList.Contains(mZone) || mRoomID == -1)
		{
			return;
		}
		if (mPlayerList.Count >= mMMOAvatarLimit)
		{
			if (mMOAvatarUserVarData._Priority <= 0)
			{
				return;
			}
			{
				foreach (KeyValuePair<string, MMOAvatarUserVarData> mRoomPlayersUserVarDatum in mRoomPlayersUserVarData)
				{
					string pkey = mRoomPlayersUserVarDatum.Key;
					MMOAvatarUserVarData value = mRoomPlayersUserVarDatum.Value;
					MMOAvatar value2 = null;
					pPlayerList.TryGetValue(pkey, out value2);
					if (value != null && !string.IsNullOrEmpty(pkey) && !(pkey == user.Username) && !(value2 == null) && mMOAvatarUserVarData._Priority >= value._Priority)
					{
						mPlayerList.Remove(pkey);
						mPlayersToDeleteList.Add(value2);
						mPlayersToLoadList.RemoveAll((MMOPlayerToLoad x) => x._Username == pkey);
						CreateMMOUser(user);
						break;
					}
				}
				return;
			}
		}
		CreateMMOUser(user);
	}

	private void CreateMMOUser(MMOUser user)
	{
		foreach (MMOPlayerToLoad mPlayersToLoad in mPlayersToLoadList)
		{
			if (mPlayersToLoad._Username == user.Username)
			{
				return;
			}
		}
		MMOPlayerToLoad item = default(MMOPlayerToLoad);
		item._Username = user.Username;
		item._UserID = user.UserVariables["UID"].ToString();
		MMOAvatarData mMOAvatarData = JsonConvert.DeserializeObject<MMOAvatarData>(user.UserVariables["A"].ToString());
		item._AvatarData = CreateAvatarData(mMOAvatarData);
		UtDebug.Log("MMO: Creating Avatar for " + item._UserID, LOG_MASK);
		Gender gender = (Gender)Enum.Parse(typeof(Gender), mMOAvatarData.GenderType, ignoreCase: true);
		MMOAvatar mMOAvatar = MMOAvatar.CreateAvatar(item._UserID, item._Username, gender);
		if (mMOAvatar != null)
		{
			mMOAvatar.SetIgnoreCollisionToAllAvatars();
			mPlayerList[item._Username] = mMOAvatar;
			mPlayersToLoadList.Add(item);
		}
	}

	public MMOAvatar GetPlayer(string userID)
	{
		foreach (KeyValuePair<string, MMOAvatar> mPlayer in mPlayerList)
		{
			MMOAvatar value = mPlayer.Value;
			if (value != null && value.pUserID == userID)
			{
				return value;
			}
		}
		return null;
	}

	public void DeletePlayer(string userID)
	{
		foreach (KeyValuePair<string, MMOAvatar> mPlayer in mPlayerList)
		{
			MMOAvatar value = mPlayer.Value;
			if (value != null && value.pUserID == userID)
			{
				mPlayerList.Remove(value.pUserID);
				mPlayersToDeleteList.Add(value);
				break;
			}
		}
	}

	public void DeleteAllPlayers()
	{
		mPlayersToLoadList.Clear();
		foreach (KeyValuePair<string, MMOAvatar> mPlayer in mPlayerList)
		{
			MMOAvatar value = mPlayer.Value;
			if (value != null)
			{
				mPlayersToDeleteList.Add(value);
			}
		}
		mPlayerList.Clear();
		mRoomPlayersUserVarData.Clear();
	}

	public void DeleteAllExtraPlayers()
	{
		mPlayersToLoadList.Clear();
		foreach (KeyValuePair<string, MMOAvatar> mPlayer in mPlayerList)
		{
			if (mPlayerList.Count <= mMMOAvatarLimit)
			{
				break;
			}
			MMOAvatar value = mPlayer.Value;
			if (value != null && !mPlayersToDeleteList.Contains(value))
			{
				mPlayerList.Remove(value.pUserID);
				mPlayersToDeleteList.Add(value);
			}
		}
	}

	private void BuddyMessage(MMOBuddyMessage bm)
	{
		string cmdType = bm.CmdType;
		string fromID = bm.FromID;
		switch (cmdType)
		{
		case "1":
		{
			UtDebug.Log("MMO: EXT_MSG_TYPE_ADD!", LOG_MASK2);
			BuddyList.InsertNewBuddy(fromID, "", BuddyStatus.PendingApprovalFromSelf, onlineStatus: true);
			GameObject gameObject = GameObject.Find("PfUiBuddyMessage");
			if (gameObject != null)
			{
				gameObject.SendMessage("ReCheckUserMessage");
				break;
			}
			gameObject = GameObject.Find("PfCheckUserMessages");
			if (gameObject != null)
			{
				gameObject.SendMessage("ReCheckUserMessage");
			}
			break;
		}
		case "2":
			UtDebug.Log("MMO: EXT_MSG_TYPE_REMOVE!", LOG_MASK2);
			BuddyList.RemoveBuddy(fromID);
			RemoveFromParty();
			break;
		case "3":
			UtDebug.Log("MMO: EXT_MSG_TYPE_BLOCK!", LOG_MASK2);
			BuddyList.SetBuddyStatus(fromID, BuddyStatus.BlockedByOther, bestBuddy: false);
			RemoveFromParty();
			break;
		case "4":
			UtDebug.Log("MMO: EXT_MSG_TYPE_APPROVE!", LOG_MASK2);
			BuddyList.SetBuddyStatus(fromID, BuddyStatus.Approved, bestBuddy: false);
			break;
		case "5":
		{
			UtDebug.Log("MMO: EXT_MSG_TYPE_INVITE!", LOG_MASK2);
			string fromZone2 = bm.FromZone;
			BuddyList.AddInvite(fromID, fromZone2, BuddyMessageType.INVITE);
			break;
		}
		case "14":
		{
			string fromZone = bm.FromZone;
			BuddyList.AddInvite(fromID, fromZone, BuddyMessageType.PRIVATE_PARTY_INVITE);
			break;
		}
		case "0":
			UtDebug.Log("MMO: EXT_MSG_TYPE_UNKNOW!", LOG_MASK2);
			break;
		}
	}

	private void RemoveFromParty()
	{
		GameObject gameObject = GameObject.Find("PfPartyManager");
		if (gameObject != null)
		{
			gameObject.SendMessage("CheckForBuddy", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void JoinBuddy(BuddyLocation location)
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		MMOAvatar player = GetPlayer(location.UserID);
		string text = ((player != null) ? player.pLevel : location.Zone);
		if (mState <= MMOClientState.CONNECTION_FAILED || (IsMemberOnlyZone(text) && !SubscriptionInfo.pIsMember) || IsLevelRacing() || !IsJoinAllowed(text) || !IsJoinAllowedInActiveMission(text) || (component != null && component.pPlayerCarrying))
		{
			if (BuddyList.pInstance.pEventDelegate != null)
			{
				BuddyList.pInstance.pEventDelegate(WsServiceType.GET_BUDDY_LOCATION, JoinBuddyResultType.JoinFailedCommon);
			}
			BuddyList.pInstance.pEventDelegate = null;
			return;
		}
		if (!UnlockManager.IsSceneUnlocked(text, inShowUi: false, delegate(bool success)
		{
			if (success)
			{
				JoinBuddy(location);
			}
		}))
		{
			if (BuddyList.pInstance.pEventDelegate != null)
			{
				BuddyList.pInstance.pEventDelegate(WsServiceType.GET_BUDDY_LOCATION, JoinBuddyResultType.JoinFailedHandled);
			}
			BuddyList.pInstance.pEventDelegate = null;
			return;
		}
		LocaleString text2 = null;
		if (!EquipmentCheck.pInstance.CheckEquippedItemsCriteria(text, ref text2))
		{
			DisplayTextBox(text2._ID, text2._Text);
			if (BuddyList.pInstance.pEventDelegate != null)
			{
				BuddyList.pInstance.pEventDelegate(WsServiceType.GET_BUDDY_LOCATION, JoinBuddyResultType.JoinFailedHandled);
			}
			BuddyList.pInstance.pEventDelegate = null;
		}
		else if (location.Server != mClient.MMOServerIP)
		{
			mJoinBuddy = location;
			ConnectToBuddy(location.Server, mClient.MMOServerPort);
		}
		else if (mSpecialZoneList.Contains(text))
		{
			AvAvatar.SetUIActive(inActive: false);
			SpecialZoneData specialZoneData = (SpecialZoneData)mSpecialZoneList[text];
			if (specialZoneData != null)
			{
				JoinOwnerSpace(text, location.UserID, specialZoneData.ForceJoin, specialZoneData.ReloadScene);
			}
		}
		else if (text != mZone)
		{
			mJoinTimer = mJoinTime;
			mJoinBuddy = location;
			LoginToZone(text);
		}
		else if (Convert.ToInt32(location.Room) != mRoomID)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["0"] = location.MultiplayerID;
			mClient.SendExtensionMessage("le", "JU", dictionary);
			mJoinTimer = mJoinTime;
			mJoinBuddy = location;
			if (mLevelOwnerIDList.Contains(RsResourceManager.pCurrentLevel) && text == (string)mZoneList[RsResourceManager.pCurrentLevel])
			{
				if (BuddyList.pInstance.pEventDelegate != null)
				{
					BuddyList.pInstance.pEventDelegate(WsServiceType.GET_BUDDY_LOCATION, JoinBuddyResultType.JoinSuccess);
				}
				BuddyList.pInstance.pEventDelegate = null;
				mLevelToBeLoaded = RsResourceManager.pCurrentLevel;
			}
		}
		else
		{
			TeleportToBuddy(location.UserID);
		}
	}

	public bool LoadToPlayer(ref Vector3 startPos)
	{
		bool result = false;
		if (mPlayerList.ContainsKey(mLoadToPlayer))
		{
			MMOAvatar mMOAvatar = mPlayerList[mLoadToPlayer];
			if (RsResourceManager.pCurrentLevel == mMOAvatar.pLevel && mMOAvatar.pObject.transform.position.y != -5000f && (mMOAvatar.pJoinAllowed == MMOJoinStatus.ALLOWED || (mMOAvatar.pJoinAllowed == MMOJoinStatus.MEMBERS_ONLY && SubscriptionInfo.pIsMember)))
			{
				if (mBuddyTeleportMarker != null)
				{
					AvAvatar.mTransform.forward = mBuddyTeleportMarker.forward;
					startPos = mBuddyTeleportMarker.position;
				}
				else
				{
					AvAvatar.mTransform.rotation = mMOAvatar.pObject.transform.rotation;
					Vector3 outPos = Vector3.zero;
					float ioStartOffset = 0f;
					if (UtUtilities.FindPosNextToObject(out outPos, mMOAvatar.pObject, 1f, 4f, ref ioStartOffset, 15f, 2f, 0f))
					{
						startPos = outPos;
						Vector3 forward = mMOAvatar.pObject.transform.position - outPos;
						forward.y = 0f;
						forward.Normalize();
						AvAvatar.mTransform.forward = forward;
					}
					else
					{
						startPos = mMOAvatar.pObject.transform.position;
					}
				}
				result = true;
			}
		}
		mLoadToPlayer = "";
		return result;
	}

	public void TeleportToBuddy(string userID)
	{
		string userName = "";
		MMOAvatar player = null;
		foreach (KeyValuePair<string, MMOAvatar> mPlayer in mPlayerList)
		{
			MMOAvatar value = mPlayer.Value;
			if (value != null && value.pUserID == userID)
			{
				userName = mPlayer.Key;
				player = value;
				break;
			}
		}
		TeleportToBuddy(player, userName);
	}

	public void TeleportToBuddy(MMOAvatar player, string userName)
	{
		if (player == null)
		{
			if (BuddyList.pInstance != null && BuddyList.pInstance.pEventDelegate != null)
			{
				BuddyList.pInstance.pEventDelegate(WsServiceType.GET_BUDDY_LOCATION, JoinBuddyResultType.JoinFailedCommon);
			}
			if (BuddyList.pInstance != null)
			{
				BuddyList.pInstance.pEventDelegate = null;
			}
		}
		else if ((player.pJoinAllowed == MMOJoinStatus.ALLOWED || (player.pJoinAllowed == MMOJoinStatus.MEMBERS_ONLY && SubscriptionInfo.pIsMember)) && mZoneList.Contains(player.pLevel))
		{
			if (BuddyList.pInstance.pEventDelegate != null)
			{
				BuddyList.pInstance.pEventDelegate(WsServiceType.GET_BUDDY_LOCATION, JoinBuddyResultType.JoinSuccess);
			}
			BuddyList.pInstance.pEventDelegate = null;
			mLastZone = "";
			UserAchievementTask.Set(165, player.pUserID);
			if (RsResourceManager.pCurrentLevel != player.pLevel)
			{
				mLoadToPlayer = userName;
				AvAvatar.SetActive(inActive: false);
				RsResourceManager.LoadLevel(player.pLevel);
			}
			else if (mBuddyTeleportMarker != null)
			{
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.pSubState = AvAvatarSubState.NORMAL;
				AvAvatar.TeleportTo(mBuddyTeleportMarker.position, mBuddyTeleportMarker.forward);
			}
			else if (player.pObject.transform.position.y != -5000f)
			{
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.pSubState = AvAvatarSubState.NORMAL;
				Vector3 outPos = Vector3.zero;
				float ioStartOffset = 0f;
				if (UtUtilities.FindPosNextToObject(out outPos, player.pObject, 1f, 4f, ref ioStartOffset, 15f, 2f, 0f))
				{
					Vector3 inDirection = player.pObject.transform.position - outPos;
					inDirection.y = 0f;
					inDirection.Normalize();
					AvAvatar.TeleportTo(outPos, inDirection);
				}
				else
				{
					AvAvatar.TeleportTo(player.pObject.transform.position, player.pObject.transform.forward);
				}
				AvAvatar.pObject.BroadcastMessage("OnTelportedToPlayer", SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			if (mLastZone != "")
			{
				LoginToZone(mLastZone);
				mLastZone = "";
			}
			if (BuddyList.pInstance.pEventDelegate != null)
			{
				BuddyList.pInstance.pEventDelegate(WsServiceType.GET_BUDDY_LOCATION, JoinBuddyResultType.JoinFailedCommon);
			}
			BuddyList.pInstance.pEventDelegate = null;
		}
	}

	private void ResetConnection()
	{
		if (mJoinBuddy != null && !string.IsNullOrEmpty(mPreviousIP))
		{
			mJoinBuddy = null;
			mIsLoginToBuddyZone = false;
			mIPAddress = mPreviousIP;
			mZone = mLastZone;
			mLastZone = "";
			mPreviousIP = "";
			mIsSwitchingBack = true;
			mState = MMOClientState.NOT_CONNECTED;
			UtDebug.Log("MMO: Reconnecting...", LOG_MASK);
		}
		else
		{
			Debug.LogError("Reconnection failed. Server IP is either null or empty");
		}
	}

	private void Error(object sender, MMOErrorEventArgs args)
	{
		UtDebug.Log("MMO: Error " + args.ErrorMessage, LOG_MASK);
		switch (args.ErrorType)
		{
		case MMOErrorType.ConnectionError:
			if (mState >= MMOClientState.CONNECTED || mState == MMOClientState.DISCONNECTING)
			{
				DeleteAllPlayers();
			}
			if (mState >= MMOClientState.CONNECTING)
			{
				BuddyList.Reset();
				mState = MMOClientState.CONNECTION_FAILED;
				mReconnectTimer = mReconnectTime;
			}
			if (mJoinBuddy != null)
			{
				if (mIsConnectToBuddy)
				{
					mIsConnectToBuddy = false;
					mIsLoginToBuddyZone = true;
					ConnectToBuddy();
					return;
				}
				ResetConnection();
			}
			break;
		case MMOErrorType.LoginError:
			if (mJoinBuddy != null && !string.IsNullOrEmpty(mPreviousIP) && mPreviousIP != mClient.MMOServerIP)
			{
				mState = MMOClientState.DISCONNECTING;
				mClient.Disconnect();
			}
			break;
		case MMOErrorType.JoinRoomError:
			if (mSpecialZoneList.Contains(mZone))
			{
				break;
			}
			if (mJoinBuddy != null && mLastZone != "")
			{
				if (!string.IsNullOrEmpty(mPreviousIP) && mPreviousIP != mClient.MMOServerIP)
				{
					ResetConnection();
					return;
				}
				if (mJoinBuddy.Zone != mLastZone)
				{
					LoginToZone(mLastZone);
				}
			}
			mLastZone = "";
			mJoinBuddy = null;
			TeleportToBuddy(null, null);
			break;
		}
		foreach (MMOClient mMMOClient in mMMOClientList)
		{
			mMMOClient.OnError(args);
		}
	}

	private void Connected(object sender, MMOConnectedEventArgs args)
	{
		UtDebug.Log("MMO: Connected!", LOG_MASK);
		mState = MMOClientState.CONNECTED;
		mIPAddress = mClient.MMOServerIP;
		mPort = mClient.MMOServerPort;
		mUseUDP = mClient.MMOServerVersion == "S2X";
		InitSmartFoxClient();
		foreach (MMOClient mMMOClient in mMMOClientList)
		{
			mMMOClient.OnConnected(args);
		}
	}

	private void ExtensionResponseReceived(object sender, MMOExtensionResponseReceivedEventArgs args)
	{
		string text = args.ResponseDataObject["0"].ToString();
		for (int i = 0; i < args.ResponseDataObject.Count; i++)
		{
			if (text != "PNG")
			{
				UtDebug.Log("MMO: ExtensionResponseReceived! Command[" + i + "]: " + args.ResponseDataObject[i.ToString()], LOG_MASK);
			}
		}
		if (mExtensionResponseHandlers.ContainsKey(text))
		{
			mExtensionResponseHandlers[text](sender, args);
		}
		if (text == "UDL")
		{
			if (args.ResponseDataObject.ContainsKey("2"))
			{
				RemoveUser(args.ResponseDataObject["2"].ToString());
			}
			return;
		}
		if (text == "SBE" && UserInfo.pInstance.UserID == args.ResponseDataObject["3"].ToString())
		{
			MMOBuddyMessage item = default(MMOBuddyMessage);
			item.CmdType = args.ResponseDataObject["4"].ToString();
			item.FromID = args.ResponseDataObject["2"].ToString();
			if (args.ResponseDataObject.Count >= 6)
			{
				item.FromZone = args.ResponseDataObject["5"].ToString();
			}
			mBuddyMessageList.Add(item);
			return;
		}
		switch (text)
		{
		case "SPI":
		{
			MMOBuddyMessage item2 = default(MMOBuddyMessage);
			item2.CmdType = args.ResponseDataObject["4"].ToString();
			item2.FromID = args.ResponseDataObject["2"].ToString();
			if (args.ResponseDataObject.Count >= 6)
			{
				item2.FromZone = args.ResponseDataObject["5"].ToString();
			}
			mBuddyMessageList.Add(item2);
			return;
		}
		case "GPE":
		{
			int key = Convert.ToInt32(args.ResponseDataObject["2"].ToString());
			if (mGetPositionList.ContainsKey(key))
			{
				mGetPositionList[key].SendMessage("OnGetPositionEvent", Convert.ToSingle(args.ResponseDataObject["3"]), SendMessageOptions.DontRequireReceiver);
				mGetPositionList.Remove(key);
			}
			return;
		}
		case "RTM":
		{
			float num = Convert.ToSingle(args.ResponseDataObject["2"].ToString());
			foreach (GameObject mGetElapsedTime in mGetElapsedTimeList)
			{
				mGetElapsedTime.SendMessage("OnGetElapsedTimeEvent", num * 0.001f, SendMessageOptions.DontRequireReceiver);
			}
			mGetElapsedTimeList.Clear();
			return;
		}
		case "CMR":
		{
			string userID = args.ResponseDataObject["2"].ToString();
			string text2 = args.ResponseDataObject["4"].ToString();
			ChatOptions channel = ChatOptions.CHAT_ROOM;
			if (args.ResponseDataObject.ContainsKey("5") && !string.IsNullOrEmpty(args.ResponseDataObject["5"] as string))
			{
				channel = ChatOptions.CHAT_CLANS;
			}
			else if (args.ResponseDataObject.ContainsKey("6") && args.ResponseDataObject["6"] as string == "2")
			{
				channel = ChatOptions.CHAT_FRIENDS;
			}
			if (BuddyList.pInstance != null)
			{
				BuddyStatus buddyStatus = BuddyList.pInstance.GetBuddyStatus(userID);
				if (buddyStatus == BuddyStatus.BlockedBySelf || buddyStatus == BuddyStatus.BlockedByBoth)
				{
					return;
				}
			}
			MMOAvatar player = GetPlayer(userID);
			if (player != null)
			{
				player.Chat(text2, channel);
			}
			else if (args.ResponseDataObject.ContainsKey("7"))
			{
				string text3 = args.ResponseDataObject["7"] as string;
				if (!string.IsNullOrEmpty(text3))
				{
					UiChatHistory.WriteLine(text2, text3, channel);
				}
			}
			return;
		}
		case "SCA":
		{
			string[] array = new string[2]
			{
				args.ResponseDataObject["3"].ToString(),
				ChatOptions.CHAT_ROOM.ToString()
			};
			ChatOptions channel2 = ChatOptions.CHAT_ROOM;
			if (args.ResponseDataObject.ContainsKey("4") && !string.IsNullOrEmpty(args.ResponseDataObject["4"] as string))
			{
				channel2 = ChatOptions.CHAT_CLANS;
			}
			else if (args.ResponseDataObject.ContainsKey("5") && args.ResponseDataObject["5"] as string == "2")
			{
				channel2 = ChatOptions.CHAT_FRIENDS;
			}
			array[1] = channel2.ToString();
			if (AvAvatar.pObject.activeInHierarchy)
			{
				AvAvatar.pObject.SendMessage("OnChat", array);
			}
			else if (AvatarData.pInstance != null)
			{
				UiChatHistory.WriteLine(array[0], AvatarData.pInstance.DisplayName, channel2);
			}
			return;
		}
		case "NMP":
			if (!args.ResponseDataObject.ContainsKey("4") || args.ResponseDataObject["4"].ToString() == "null")
			{
				UiToolbar.pMessageCount = Convert.ToInt32(args.ResponseDataObject["3"].ToString());
				return;
			}
			break;
		}
		switch (text)
		{
		case "SCF":
		{
			string text4 = args.ResponseDataObject["2"].ToString();
			if (text4 == "MB")
			{
				_ = string.Empty;
				if (args.ResponseDataObject.Count > 3)
				{
					string[] array2 = args.ResponseDataObject["3"].ToString().Split(' ');
					string text5 = "";
					for (int j = 0; j < array2.Length; j++)
					{
						if (array2[j].Contains("#"))
						{
							array2[j] = "[FF0000]" + array2[j] + "[-]";
						}
						text5 = text5 + " " + array2[j];
					}
					UiChatHistory.WriteLine(text5, AvatarData.pInstance.DisplayName);
					UiChatHistory.DisplayBlockedTextWarningMessage();
				}
			}
			if (text4 == "CB")
			{
				string inMessage3 = string.Empty;
				string inTime3 = "0";
				if (args.ResponseDataObject.Count > 5)
				{
					inMessage3 = args.ResponseDataObject["4"].ToString();
					inTime3 = args.ResponseDataObject["5"].ToString();
				}
				SetSilenceParams(inAction: true, inMessage3, inTime3);
				DisplaySilenceMessage();
			}
			break;
		}
		case "SMF":
			if (args.ResponseDataObject["2"].ToString() == "CB")
			{
				string inMessage = string.Empty;
				string inTime = "0";
				if (args.ResponseDataObject.Count > 5)
				{
					inMessage = args.ResponseDataObject["4"].ToString();
					inTime = args.ResponseDataObject["5"].ToString();
				}
				SetSilenceParams(inAction: true, inMessage, inTime);
				DisplaySilenceMessage();
			}
			break;
		case "SMM":
		{
			string strA = args.ResponseDataObject["2"].ToString();
			string inMessage2 = args.ResponseDataObject["3"].ToString();
			string inTime2 = "0";
			if (args.ResponseDataObject.Count > 4)
			{
				inTime2 = args.ResponseDataObject["4"].ToString();
			}
			SetSilenceParams(string.Compare(strA, "SILENCE", StringComparison.OrdinalIgnoreCase) == 0, inMessage2, inTime2);
			if (pCanShowModeration)
			{
				DisplaySilenceMessage();
			}
			else if (mSilenceDB != null)
			{
				mSilenceDB.GetComponent<KAUIGenericDB>().SetText(UpdatedWarningMessage(), interactive: false);
			}
			else
			{
				mNeedToShowModeration = true;
			}
			break;
		}
		case "SCBE":
		{
			string fromUser = args.ResponseDataObject["2"].ToString();
			int challengeID = Convert.ToInt32(args.ResponseDataObject["4"].ToString());
			int gameID = Convert.ToInt32(args.ResponseDataObject["5"].ToString());
			int gameLevelID = Convert.ToInt32(args.ResponseDataObject["6"].ToString());
			int gameDiffID = Convert.ToInt32(args.ResponseDataObject["7"].ToString());
			int challengePoints = Convert.ToInt32(args.ResponseDataObject["8"].ToString());
			ChallengeInfo.AddMessage(fromUser, challengeID, gameID, gameLevelID, gameDiffID, challengePoints);
			break;
		}
		case "SPMN":
		{
			GameObject gameObject = GameObject.Find("PfCheckUserMessages");
			if (gameObject != null)
			{
				gameObject.SendMessage("ForceUserMessageUpdate");
			}
			break;
		}
		}
	}

	public void RemoveUser(string userName)
	{
		if (mPlayerList.ContainsKey(userName))
		{
			MMOAvatar mMOAvatar = mPlayerList[userName];
			if ((bool)mMOAvatar)
			{
				mMOAvatar.Activate(active: false);
			}
			foreach (MMOClient mMMOClient in mMMOClientList)
			{
				mMMOClient.RemovePlayer(mMOAvatar);
			}
			mPlayerList.Remove(userName);
			if (mMOAvatar != null)
			{
				mPlayersToDeleteList.Add(mMOAvatar);
			}
			foreach (MMOPlayerToLoad mPlayersToLoad in mPlayersToLoadList)
			{
				if (mPlayersToLoad._Username == userName)
				{
					mPlayersToLoadList.Remove(mPlayersToLoad);
					break;
				}
			}
		}
		if (mRoomPlayersUserVarData.ContainsKey(userName))
		{
			mRoomPlayersUserVarData.Remove(userName);
		}
	}

	private void LoggedIn(object sender, MMOLoggedInEventArgs args)
	{
		if (args == null)
		{
			return;
		}
		UtDebug.Log("MMO: LoggedIn!", LOG_MASK);
		mState = MMOClientState.LOGGED_IN;
		mRoomID = -1;
		mWaitingForRoomListUpdated = true;
		foreach (MMOClient mMMOClient in mMMOClientList)
		{
			mMMOClient.OnLoggedIn(args);
		}
	}

	public bool JoinOwnerSpace(string level, string inOID, bool force = false, bool reloadScene = true)
	{
		if (!UnlockManager.IsSceneUnlocked(level))
		{
			return false;
		}
		if (RsResourceManager.pCurrentLevel != level)
		{
			RsResourceManager.LoadLevel(level);
			if (mZone != level)
			{
				mLevelOwnerIDList[level] = inOID;
				return true;
			}
		}
		else if (RsResourceManager.pLevelLoading)
		{
			mLevelOwnerIDList[level] = inOID;
			if (RsResourceManager.pLevelToLoad != level)
			{
				Debug.LogError("MMO: JoinOwnerSpace called to load a level while a different level is already loading!!!!");
			}
			return false;
		}
		if (force || inOID != (string)mLevelOwnerIDList[level])
		{
			mLevelOwnerIDList[level] = inOID;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["0"] = inOID;
			dictionary["rid"] = UserRoomID;
			mClient.SendExtensionMessage("le", "JO", dictionary);
			if (reloadScene)
			{
				mLevelToBeLoaded = level;
			}
			return true;
		}
		AvAvatar.SetActive(inActive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
		return false;
	}

	private void RoomListUpdated(object sender, MMORoomListUpdatedEventArgs args)
	{
		if (args == null || (!mWaitingForRoomListUpdated && !args.RecoverState))
		{
			return;
		}
		if (mJoinBuddy != null)
		{
			JoinBuddy(mJoinBuddy);
		}
		else if (mLevelToBeLoaded.Length <= 0)
		{
			if (mLevelOwnerIDList.Contains(RsResourceManager.pCurrentLevel) && ((string)mLevelOwnerIDList[RsResourceManager.pCurrentLevel]).Length > 0)
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary["0"] = (string)mLevelOwnerIDList[RsResourceManager.pCurrentLevel];
				dictionary["rid"] = UserRoomID;
				mClient.SendExtensionMessage("le", "JO", dictionary);
			}
			else if (mLevelOwnerIDList.Contains(RsResourceManager.pCurrentLevel) && ((string)mLevelOwnerIDList[RsResourceManager.pCurrentLevel]).Length == 0)
			{
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				dictionary2["0"] = UserInfo.pInstance.UserID;
				dictionary2["rid"] = UserRoomID;
				mLevelOwnerIDList[RsResourceManager.pCurrentLevel] = UserInfo.pInstance.UserID;
				mClient.SendExtensionMessage("le", "JO", dictionary2);
			}
			else if (IsLevelMMO(RsResourceManager.pCurrentLevel))
			{
				mClient.SendExtensionMessage("le", "JA", null);
			}
			else
			{
				mClient.SendExtensionMessage("le", "JL", null);
			}
		}
		mWaitingForRoomListUpdated = false;
	}

	private void DebugMessageReceived(object sender, MMODebugMessageEventArgs args)
	{
		if (args != null)
		{
			UtDebug.Log("MMO: DebugMessageReceived!" + args.Message, LOG_MASK2);
		}
	}

	private void LoggedOut(object sender, MMOLoggedOutEventArgs args)
	{
		if (args == null)
		{
			return;
		}
		UtDebug.Log("MMO: LoggedOut!", LOG_MASK);
		mState = MMOClientState.CONNECTED;
		if (!string.IsNullOrEmpty(mZone))
		{
			UtDebug.Log("Attempting to login to zone : " + mZone, LOG_MASK);
			SendLoginRequest(mZone);
		}
		foreach (MMOClient mMMOClient in mMMOClientList)
		{
			mMMOClient.OnLoggedOut(args);
		}
	}

	private void JoinedRoom(object sender, MMOJoinedRoomEventArgs args)
	{
		UtDebug.Log("MMO: GetPositionList cleared!!", LOG_MASK);
		mGetPositionList.Clear();
		if (args == null || args.RoomJoined == null || args.RoomJoined.RoomName == null)
		{
			return;
		}
		UserRoomID = string.Empty;
		string text = string.Empty;
		string text2 = string.Empty;
		UtDebug.Log("MMO: JoinedRoom = " + args.RoomJoined.RoomName, LOG_MASK);
		mRoomName = args.RoomJoined.RoomName;
		foreach (MMORoomVariable roomVariable in args.RoomJoined.RoomVariables)
		{
			if (roomVariable.Name == "OID")
			{
				string text3 = "";
				foreach (DictionaryEntry mZone in mZoneList)
				{
					if (this.mZone == (string)mZone.Value)
					{
						text3 = (string)mZone.Key;
					}
				}
				if (mLevelOwnerIDList.Contains(text3))
				{
					text = (string)roomVariable.Value;
					mLevelOwnerIDList[text3] = text;
				}
				else
				{
					Debug.LogError("MMO: Error, " + text3 + " is not in owned level list!!");
				}
			}
			else if (roomVariable.Name == "RID")
			{
				text2 = (string)roomVariable.Value;
			}
		}
		if (!text.Equals(UserInfo.pInstance.UserID) && !string.IsNullOrEmpty(text2))
		{
			UserRoomID = text2;
		}
		DeleteAllPlayers();
		foreach (MMOClient mMMOClient in mMMOClientList)
		{
			mMMOClient.RemoveAll();
		}
		if (args.RoomJoined.RoomName.ToLower().Contains("limbo"))
		{
			mRoomID = -1;
		}
		else if (args.RoomJoined.RoomID.HasValue)
		{
			mRoomID = args.RoomJoined.RoomID.Value;
		}
		mState = MMOClientState.IN_ROOM;
		mDisconnectTimer = mDisconnectTime;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["UID"] = UserInfo.pInstance.UserID;
		dictionary["A"] = UtUtilities.JsonSerializeObject(AvatarData.pInstance);
		dictionary["CU"] = UserProfile.pProfileData.GetCountryAnswerID();
		if (UserProfile.pProfileData.HasGroup())
		{
			Group group = Group.GetGroup(UserProfile.pProfileData.GetGroupID());
			if (group != null)
			{
				dictionary["CLU"] = group.Name + "|" + group.Logo + "|" + group.Color;
			}
		}
		dictionary["M"] = SubscriptionInfo.pIsMember;
		if (string.IsNullOrEmpty(mCurrentLevel))
		{
			dictionary["L"] = RsResourceManager.pCurrentLevel;
		}
		else
		{
			dictionary["L"] = mCurrentLevel;
		}
		if (UserRankData.pInstance != null)
		{
			MMOAchievementInfo mMOAchievementInfo = new MMOAchievementInfo();
			mMOAchievementInfo.P = (UserRankData.pInstance.AchievementPointTotal.HasValue ? UserRankData.pInstance.AchievementPointTotal.Value : 0);
			mMOAchievementInfo.R = UserRankData.pInstance.RankID;
			dictionary["RA"] = UtUtilities.JsonSerializeObject(mMOAchievementInfo);
			UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(12);
			if (userAchievementInfoByType != null)
			{
				mMOAchievementInfo.P = (userAchievementInfoByType.AchievementPointTotal.HasValue ? userAchievementInfoByType.AchievementPointTotal.Value : 0);
				mMOAchievementInfo.R = ((userAchievementInfoByType.RankID <= 0) ? 1 : userAchievementInfoByType.RankID);
				dictionary["UDT"] = UtUtilities.JsonSerializeObject(mMOAchievementInfo);
			}
		}
		mMMOUpdateTime = 0f;
		if (AvAvatar.mTransform == null)
		{
			Debug.LogError(" Avatar transform is not set !!!");
			return;
		}
		Vector3 position = AvAvatar.mTransform.position;
		dictionary["P1"] = position.x;
		dictionary["P2"] = position.y;
		dictionary["P3"] = position.z;
		dictionary["R1"] = 0f;
		dictionary["R2"] = 0f;
		dictionary["R3"] = 0f;
		dictionary["R"] = AvAvatar.mTransform.eulerAngles.y;
		Transform transform = AvAvatar.mTransform.Find(AvatarSettings.pInstance._AvatarName);
		if (transform != null)
		{
			AvAvatarAnim component = transform.GetComponent<AvAvatarAnim>();
			if (component != null)
			{
				dictionary["MBF"] = component.PackState();
			}
		}
		dictionary["F"] = 9;
		dictionary["J"] = (int)mJoinedAllowed;
		SanctuaryManager.pPendingMMOPetCheck = true;
		dictionary["BU"] = mBusy;
		AvAvatarController component2 = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component2 != null && component2.pPetObject != null && component2.pPetObject.mData != null)
		{
			MMOPetData mMOPetData = new MMOPetData();
			mMOPetData.G = component2.pPetObject.mData.Geometry;
			mMOPetData.T = component2.pPetObject.mData.Texture;
			dictionary["PU"] = UtUtilities.JsonSerializeObject(mMOPetData);
		}
		dictionary["FP"] = "";
		mClient.SetUserVariables(dictionary);
		foreach (MMOClient mMMOClient2 in mMMOClientList)
		{
			mMMOClient2.OnJoinedRoom(args);
		}
		if (mJoinBuddy != null && Convert.ToInt32(mJoinBuddy.Room) != mRoomID)
		{
			JoinBuddy(mJoinBuddy);
		}
		if (mLevelToBeLoaded.Length > 0)
		{
			AvAvatar.SetActive(inActive: false);
			RsResourceManager.LoadLevel(mLevelToBeLoaded);
			mLevelToBeLoaded = "";
		}
	}

	private void LeftRoom(object sender, MMOLeftRoomEventArgs args)
	{
		if (args == null || args.RoomLeft == null || args.RoomLeft.RoomName == null)
		{
			return;
		}
		UtDebug.Log("MMO: LeftRoom = " + args.RoomLeft.RoomName, LOG_MASK);
		foreach (MMOClient mMMOClient in mMMOClientList)
		{
			mMMOClient.OnLeftRoom(args);
		}
	}

	private void MessageReceived(object sender, MMOMessageReceivedEventArgs args)
	{
		if (args == null || args.MMOMessage == null || args.MMOMessage.Sender == null || args.MMOMessage.Sender.Username == null || (_IgnoreSenderMessage && (args.MMOMessage.Sender.Username == ProductConfig.pToken || args.MMOMessage.Sender.Username == WsWebService.pUserToken)))
		{
			return;
		}
		if (mUserMessageReceivedHandlers != null)
		{
			mUserMessageReceivedHandlers(sender, args);
		}
		UtDebug.Log("MMO: MessageReceived = " + args.MMOMessage.MessageText, LOG_MASK2);
		if (args.MMOMessage.MessageType != MMOMessageType.User)
		{
			return;
		}
		if (mPlayerList.ContainsKey(args.MMOMessage.Sender.Username))
		{
			string[] array = args.MMOMessage.MessageText.Split(':');
			if (array[0] == "UP")
			{
				if (array[1] == ProductConfig.pToken)
				{
					mForceSendCount = 1;
				}
				return;
			}
			if (array[0] == "SC")
			{
				string[] array2 = array[1].Split(',');
				string text = array2[0];
				int num = Convert.ToInt32(array2[1]);
				for (int i = 2; i < array2.Length; i++)
				{
					if (UserInfo.pInstance.UserID == array2[i])
					{
						AvAvatar.pObject.SendMessage("Morph", num, SendMessageOptions.DontRequireReceiver);
						continue;
					}
					foreach (MMOAvatar value in mPlayerList.Values)
					{
						if (value.pUserID == array2[i])
						{
							value.SendMessage("Morph", num, SendMessageOptions.DontRequireReceiver);
						}
						if (value.pUserID == text)
						{
							value.SendMessage("PlaySpellCastAnim", null, SendMessageOptions.DontRequireReceiver);
						}
					}
				}
				return;
			}
			if (array[0] == "TR")
			{
				if (array[1] == "Request")
				{
					AvAvatar.pObject.SendMessage("ReceiveTradingRequest", array[2], SendMessageOptions.DontRequireReceiver);
				}
				else if (array[1] == "RequestDeclined")
				{
					AvAvatar.pObject.SendMessage("TradingRequestDeclined", SendMessageOptions.DontRequireReceiver);
				}
				else if (array[1] == "RequestAccepted")
				{
					AvAvatar.pObject.SendMessage("TradingRequestAccepted", array[2], SendMessageOptions.DontRequireReceiver);
				}
				else if (array[1] == "Cancel")
				{
					AvAvatar.pObject.SendMessage("TradingCanceled", SendMessageOptions.DontRequireReceiver);
				}
				else if (array[1] == "Offer")
				{
					AvAvatar.pObject.SendMessage("TradeOffered", array[2], SendMessageOptions.DontRequireReceiver);
				}
				else if (array[1] == "TradeDeclined")
				{
					AvAvatar.pObject.SendMessage("TradeDeclined", SendMessageOptions.DontRequireReceiver);
				}
				else if (array[1] == "TradeSuccess")
				{
					AvAvatar.pObject.SendMessage("TradeSuccessfull", array[2], SendMessageOptions.DontRequireReceiver);
				}
				return;
			}
			MMOAvatar mMOAvatar = mPlayerList[args.MMOMessage.Sender.Username];
			if (mMOAvatar == null)
			{
				return;
			}
			switch (array[0])
			{
			case "EID":
				mMOAvatar.Emote(Convert.ToInt32(array[1]));
				break;
			case "AID":
				mMOAvatar.Action(Convert.ToInt32(array[1]));
				break;
			case "SDN":
				if (mMOAvatar.pAvatarData.mInstance != null)
				{
					mMOAvatar.pAvatarData.mInstance.DisplayName = array[1];
				}
				AvatarData.SetDisplayName(mMOAvatar.pObject, array[1]);
				break;
			case "SB":
			{
				GameObject gameObject4 = GameObject.Find("PfSpringBoard" + array[1]);
				if (gameObject4 != null)
				{
					MMOAvatarUserVarData mMOAvatarUserVarData4 = new MMOAvatarUserVarData();
					mMOAvatarUserVarData4._GameObject = gameObject4;
					mMOAvatarUserVarData4._Position = gameObject4.transform.position;
					mMOAvatarUserVarData4._Rotation = gameObject4.transform.rotation;
					mMOAvatarUserVarData4._Flags = MMOAvatarFlags.SPRINGBOARD;
					mMOAvatar.Add(mMOAvatarUserVarData4);
				}
				break;
			}
			case "SL":
			{
				GameObject gameObject7 = GameObject.Find(array[1]);
				if (gameObject7 != null)
				{
					MMOAvatarUserVarData mMOAvatarUserVarData7 = new MMOAvatarUserVarData();
					mMOAvatarUserVarData7._GameObject = gameObject7;
					mMOAvatarUserVarData7._Position = gameObject7.transform.position;
					mMOAvatarUserVarData7._Rotation = gameObject7.transform.rotation;
					mMOAvatarUserVarData7._Flags = MMOAvatarFlags.SLIDE;
					mMOAvatar.Add(mMOAvatarUserVarData7);
				}
				break;
			}
			case "ZL":
			{
				GameObject gameObject6 = GameObject.Find(array[1]);
				if (gameObject6 != null)
				{
					MMOAvatarUserVarData mMOAvatarUserVarData6 = new MMOAvatarUserVarData();
					mMOAvatarUserVarData6._GameObject = gameObject6;
					mMOAvatarUserVarData6._Position = gameObject6.transform.position;
					mMOAvatarUserVarData6._Rotation = gameObject6.transform.rotation;
					mMOAvatarUserVarData6._Flags = MMOAvatarFlags.ZIPLINE;
					mMOAvatar.Add(mMOAvatarUserVarData6);
				}
				break;
			}
			case "LP":
			{
				GameObject gameObject5 = GameObject.Find("PfLaunchPad" + array[1]);
				if (gameObject5 != null)
				{
					MMOAvatarUserVarData mMOAvatarUserVarData5 = new MMOAvatarUserVarData();
					mMOAvatarUserVarData5._GameObject = gameObject5;
					mMOAvatarUserVarData5._Position = gameObject5.transform.position;
					mMOAvatarUserVarData5._Rotation = mMOAvatar.pObject.transform.rotation;
					mMOAvatarUserVarData5._Flags = MMOAvatarFlags.LAUNCHPAD;
					mMOAvatar.Add(mMOAvatarUserVarData5);
				}
				break;
			}
			case "SO":
				if (array.Length >= 3)
				{
					GameObject gameObject = GameObject.Find(array[1]);
					if (gameObject != null)
					{
						MMOAvatarUserVarData mMOAvatarUserVarData = new MMOAvatarUserVarData();
						mMOAvatarUserVarData._GameObject = gameObject;
						mMOAvatarUserVarData._Flags = MMOAvatarFlags.SENDOBJECTMESSAGE;
						mMOAvatarUserVarData._Data = array[2];
						mMOAvatar.Add(mMOAvatarUserVarData);
					}
				}
				break;
			case "CS":
			{
				MMOAvatarUserVarData mMOAvatarUserVarData8 = new MMOAvatarUserVarData();
				mMOAvatarUserVarData8._ID = Convert.ToInt32(array[1]);
				mMOAvatarUserVarData8._Flags = MMOAvatarFlags.COUCHSIT;
				mMOAvatarUserVarData8._Position = mMOAvatar.pObject.transform.position;
				mMOAvatarUserVarData8._Rotation = mMOAvatar.pObject.transform.rotation;
				mMOAvatar.Add(mMOAvatarUserVarData8);
				break;
			}
			case "C":
			{
				BuddyStatus buddyStatus = BuddyList.pInstance.GetBuddyStatus(mMOAvatar.pUserID);
				if (buddyStatus != BuddyStatus.BlockedBySelf && buddyStatus != BuddyStatus.BlockedByBoth)
				{
					ChatOptions channel = (ChatOptions)Convert.ToInt32(array[2]);
					mMOAvatar.Chat(Convert.ToInt32(array[1]), channel);
				}
				break;
			}
			case "SS":
			{
				if (array.Length < 3)
				{
					break;
				}
				GameObject gameObject3 = GameObject.Find(array[2]);
				if (gameObject3 != null)
				{
					int score = UtStringUtil.Parse(array[1], 0);
					MMOScoreInfo mMOScoreInfo = new MMOScoreInfo();
					mMOScoreInfo._Score = score;
					mMOScoreInfo._UserName = mMOAvatar.pAvatarData.mInstance.DisplayName;
					mMOScoreInfo._IsMember = mMOAvatar.pIsMember;
					if (array.Length > 3)
					{
						mMOScoreInfo._Message = array[3];
					}
					gameObject3.SendMessage("AddMMOScore", mMOScoreInfo);
				}
				break;
			}
			case "CO":
			{
				GameObject gameObject2 = GameObject.Find(array[1]);
				if (gameObject2 != null)
				{
					MMOAvatarUserVarData mMOAvatarUserVarData3 = new MMOAvatarUserVarData();
					mMOAvatarUserVarData3._GameObject = gameObject2;
					mMOAvatarUserVarData3._Flags = MMOAvatarFlags.CARRYOBJECT;
					mMOAvatarUserVarData3._Position = mMOAvatar.pObject.transform.position;
					mMOAvatarUserVarData3._Rotation = mMOAvatar.pObject.transform.rotation;
					mMOAvatar.Add(mMOAvatarUserVarData3);
				}
				break;
			}
			case "DO":
			{
				MMOAvatarUserVarData mMOAvatarUserVarData2 = new MMOAvatarUserVarData();
				mMOAvatarUserVarData2._Flags = MMOAvatarFlags.DROPOBJECT;
				mMOAvatarUserVarData2._Position = mMOAvatar.pObject.transform.position;
				mMOAvatarUserVarData2._Rotation = mMOAvatar.pObject.transform.rotation;
				mMOAvatar.Add(mMOAvatarUserVarData2);
				break;
			}
			case "ASCT":
				mMOAvatar.SendMessage("AttachGauntlet", null, SendMessageOptions.DontRequireReceiver);
				break;
			case "DSCT":
				mMOAvatar.SendMessage("DetatchGauntlet", null, SendMessageOptions.DontRequireReceiver);
				break;
			case "PRT":
				if (mMOAvatar.pSanctuaryPet != null)
				{
					mMOAvatar.pSanctuaryPet.PlayPetMoodParticle(array[1], isForcePlay: true);
				}
				break;
			}
			return;
		}
		bool inSameClan = false;
		if (args.MMOMessage.Sender.UserVariables.ContainsKey("CLU"))
		{
			string[] array3 = args.MMOMessage.Sender.UserVariables["CLU"].ToString().Split('|');
			Group group = Group.GetGroup(UserProfile.pProfileData.GetGroupID());
			if (group != null && array3.Length == 3 && group.Name.Equals(array3[0]))
			{
				inSameClan = true;
			}
		}
		CreatePlayer(args.MMOMessage.Sender, inSameClan);
	}

	private void RoomVariablesChanged(object sender, MMORoomVariablesChangedEventArgs args)
	{
		if (mUserRoomVariableHandlers != null)
		{
			mUserRoomVariableHandlers(sender, args);
		}
	}

	private void UserJoinedRoom(object sender, MMOUserJoinedRoomEventArgs args)
	{
		if (args != null && args.UserJoined != null && args.UserJoined.Username != null)
		{
			UtDebug.Log("MMO: UserJoinedRoom = " + args.UserJoined.Username, LOG_MASK);
		}
	}

	private void UserLeftRoom(object sender, MMOUserLeftRoomEventArgs args)
	{
		if (args != null && args.UserLeft != null && args.UserLeft.Username != null)
		{
			UtDebug.Log("MMO: UserLeftRoom = " + args.UserLeft.Username, LOG_MASK);
			RemoveUser(args.UserLeft.Username);
		}
	}

	private void UserVariablesChanged(object sender, MMOUserVariablesChangedEventArgs args)
	{
		if (args == null || args.UserChanged == null || args.UserChanged.Username == null || mState != MMOClientState.IN_ROOM)
		{
			return;
		}
		UtDebug.Log("MMO: UserVariablesChanged for " + args.UserChanged.Username, LOG_MASK2);
		if (!mPlayerList.ContainsKey(args.UserChanged.Username))
		{
			bool inSameClan = false;
			if (args.UserChanged.UserVariables.ContainsKey("CLU"))
			{
				string[] array = args.UserChanged.UserVariables["CLU"].ToString().Split('|');
				Group group = Group.GetGroup(UserProfile.pProfileData.GetGroupID());
				if (group != null && array.Length == 3 && group.Name.Equals(array[0]))
				{
					inSameClan = true;
				}
			}
			CreatePlayer(args.UserChanged, inSameClan);
			if (mUserVariableCalledFrameNumber == 0)
			{
				mUserVariableCalledFrameNumber = Time.frameCount;
			}
			if (mPlayerList.Count > 0 && mUserVariableCalledFrameNumber == Time.frameCount)
			{
				mMMOSpawnCount = mPlayerList.Count;
			}
		}
		bool flag = false;
		if (mPlayerList.ContainsKey(args.UserChanged.Username))
		{
			MMOAvatar mMOAvatar = mPlayerList[args.UserChanged.Username];
			if (mMOAvatar == null)
			{
				return;
			}
			foreach (string changedVariableKey in args.ChangedVariableKeys)
			{
				mMOAvatar.SetKey(changedVariableKey, args.UserChanged.UserVariables[changedVariableKey].ToString());
				switch (changedVariableKey)
				{
				case "P1":
					flag = true;
					break;
				case "A":
				{
					MMOAvatarData mmoAvatarData = JsonConvert.DeserializeObject<MMOAvatarData>(args.UserChanged.UserVariables[changedVariableKey].ToString());
					mMOAvatar.PrepareForLoading(CreateAvatarData(mmoAvatarData), reload: true);
					break;
				}
				case "L":
					mMOAvatar.SetLevel(args.UserChanged.UserVariables[changedVariableKey].ToString());
					break;
				case "RA":
				{
					MMOAchievementInfo info = JsonConvert.DeserializeObject<MMOAchievementInfo>(args.UserChanged.UserVariables[changedVariableKey].ToString());
					mMOAvatar.pRankData = CreateAchievementInfo(info);
					break;
				}
				case "M":
				{
					bool member = Convert.ToBoolean(args.UserChanged.UserVariables[changedVariableKey]);
					mMOAvatar.SetMember(member);
					break;
				}
				case "CU":
				{
					int id = Convert.ToInt32(args.UserChanged.UserVariables[changedVariableKey]);
					mMOAvatar.SetCountry(UserProfile.GetCountryURL(mMOAvatar.gameObject, id));
					break;
				}
				case "MU":
				{
					int id2 = Convert.ToInt32(args.UserChanged.UserVariables[changedVariableKey]);
					mMOAvatar.SetMood(UserProfile.GetMoodURL(id2));
					break;
				}
				case "CLU":
				{
					string[] array2 = args.UserChanged.UserVariables[changedVariableKey].ToString().Split('|');
					if (array2.Length == 3)
					{
						Group group2 = new Group();
						group2.Name = array2[0];
						group2.Logo = array2[1];
						group2.Color = array2[2];
						mMOAvatar.SetGroup(group2);
					}
					else
					{
						mMOAvatar.SetGroup(null);
					}
					break;
				}
				case "J":
					mMOAvatar.SetJoinAllowed((MMOJoinStatus)Convert.ToInt32(args.UserChanged.UserVariables[changedVariableKey]));
					break;
				case "RDE":
					mMOAvatar.SetRide(Convert.ToInt32(args.UserChanged.UserVariables[changedVariableKey]));
					break;
				case "FP":
					mMOAvatar.SetRaisedPet(args.UserChanged.UserVariables[changedVariableKey].ToString());
					break;
				case "BU":
					mMOAvatar.SetBusy(Convert.ToBoolean(args.UserChanged.UserVariables[changedVariableKey]));
					break;
				case "PU":
				{
					string value = args.UserChanged.UserVariables[changedVariableKey].ToString();
					if (string.IsNullOrEmpty(value))
					{
						mMOAvatar.SetPet(null);
						break;
					}
					MMOPetData mmoPetData = JsonConvert.DeserializeObject<MMOPetData>(value);
					mMOAvatar.SetPet(CreatePetData(mmoPetData));
					break;
				}
				case "MBF":
				{
					AvAvatarAnim component = mMOAvatar.transform.Find(AvatarSettings.pInstance._AvatarName).GetComponent<AvAvatarAnim>();
					if (component != null)
					{
						component.UnpackState(Convert.ToInt32(args.UserChanged.UserVariables["MBF"]));
					}
					break;
				}
				case "UDT":
				{
					MMOAchievementInfo mMOAchievementInfo = JsonConvert.DeserializeObject<MMOAchievementInfo>(args.UserChanged.UserVariables[changedVariableKey].ToString());
					mMOAvatar.SetUDTPoints(mMOAchievementInfo.P);
					break;
				}
				case "DP1":
				{
					Vector3 raisedPetPosition = new Vector3(Convert.ToSingle(args.UserChanged.UserVariables["DP1"]), Convert.ToSingle(args.UserChanged.UserVariables["DP2"]), Convert.ToSingle(args.UserChanged.UserVariables["DP3"]));
					mMOAvatar.SetRaisedPetPosition(raisedPetPosition);
					break;
				}
				case "DS":
				{
					AISanctuaryPetFSM aISanctuaryPetFSM = (AISanctuaryPetFSM)Enum.Parse(typeof(AISanctuaryPetFSM), args.UserChanged.UserVariables["DS"].ToString(), ignoreCase: true);
					if (Enum.IsDefined(typeof(AISanctuaryPetFSM), aISanctuaryPetFSM))
					{
						mMOAvatar.SetRaisedPetState(aISanctuaryPetFSM);
					}
					break;
				}
				}
			}
			if (!mMOAvatar.pMMOInit)
			{
				if (args.UserChanged.UserVariables.ContainsKey("L"))
				{
					mMOAvatar.SetKey("L", args.UserChanged.UserVariables["L"].ToString());
					mMOAvatar.SetLevel(args.UserChanged.UserVariables["L"].ToString());
				}
				if (args.UserChanged.UserVariables.ContainsKey("RA"))
				{
					string text2 = args.UserChanged.UserVariables["RA"].ToString();
					mMOAvatar.SetKey("RA", text2);
					MMOAchievementInfo info2 = JsonConvert.DeserializeObject<MMOAchievementInfo>(text2);
					mMOAvatar.pRankData = CreateAchievementInfo(info2);
				}
				if (args.UserChanged.UserVariables.ContainsKey("RDE"))
				{
					mMOAvatar.SetKey("RDE", args.UserChanged.UserVariables["RDE"].ToString());
					int num = UtStringUtil.Parse(args.UserChanged.UserVariables["RDE"].ToString(), 0);
					if (num > 0)
					{
						mMOAvatar.SetRide(num);
					}
				}
				if (args.UserChanged.UserVariables.ContainsKey("FP"))
				{
					mMOAvatar.SetKey("FP", args.UserChanged.UserVariables["FP"].ToString());
					string text3 = args.UserChanged.UserVariables["FP"].ToString();
					if (!string.IsNullOrEmpty(text3))
					{
						mMOAvatar.SetRaisedPet(text3);
					}
				}
				if (args.UserChanged.UserVariables.ContainsKey("M"))
				{
					mMOAvatar.SetKey("M", args.UserChanged.UserVariables["M"].ToString());
					if (bool.TryParse(args.UserChanged.UserVariables["M"].ToString(), out var result))
					{
						mMOAvatar.SetMember(result);
					}
				}
				if (args.UserChanged.UserVariables.ContainsKey("CU"))
				{
					string text4 = args.UserChanged.UserVariables["CU"].ToString();
					mMOAvatar.SetKey("CU", text4);
					mMOAvatar.SetCountry(UserProfile.GetCountryURL(mMOAvatar.gameObject, UtStringUtil.Parse(text4, -1)));
				}
				if (args.UserChanged.UserVariables.ContainsKey("MU"))
				{
					string text5 = args.UserChanged.UserVariables["MU"].ToString();
					mMOAvatar.SetKey("MU", text5);
					mMOAvatar.SetMood(UserProfile.GetMoodURL(UtStringUtil.Parse(text5, -1)));
				}
				if (args.UserChanged.UserVariables.ContainsKey("CLU"))
				{
					string text6 = args.UserChanged.UserVariables["CLU"].ToString();
					mMOAvatar.SetKey("CLU", text6);
					string[] array3 = text6.Split('|');
					if (array3.Length == 3)
					{
						Group group3 = new Group();
						group3.Name = array3[0];
						group3.Logo = array3[1];
						group3.Color = array3[2];
						mMOAvatar.SetGroup(group3);
					}
					else
					{
						mMOAvatar.SetGroup(null);
					}
				}
				else
				{
					mMOAvatar.SetGroup(null);
				}
				if (args.UserChanged.UserVariables.ContainsKey("J"))
				{
					string text7 = args.UserChanged.UserVariables["J"].ToString();
					mMOAvatar.SetKey("J", text7);
					int joinAllowed = UtStringUtil.Parse(text7, 0);
					mMOAvatar.SetJoinAllowed((MMOJoinStatus)joinAllowed);
				}
				if (args.UserChanged.UserVariables.ContainsKey("BU"))
				{
					mMOAvatar.SetKey("BU", args.UserChanged.UserVariables["BU"].ToString());
					if (bool.TryParse(args.UserChanged.UserVariables["BU"].ToString(), out var result2))
					{
						mMOAvatar.SetBusy(result2);
					}
				}
				if (args.UserChanged.UserVariables.ContainsKey("PU"))
				{
					string text8 = args.UserChanged.UserVariables["PU"].ToString();
					mMOAvatar.SetKey("PU", text8);
					if (string.IsNullOrEmpty(text8))
					{
						mMOAvatar.SetPet(null);
					}
					else
					{
						MMOPetData mmoPetData2 = JsonConvert.DeserializeObject<MMOPetData>(text8);
						mMOAvatar.SetPet(CreatePetData(mmoPetData2));
					}
				}
				if (args.UserChanged.UserVariables.ContainsKey("DP1"))
				{
					Vector3 raisedPetPosition2 = new Vector3(Convert.ToSingle(args.UserChanged.UserVariables["DP1"]), Convert.ToSingle(args.UserChanged.UserVariables["DP2"]), Convert.ToSingle(args.UserChanged.UserVariables["DP3"]));
					mMOAvatar.SetRaisedPetPosition(raisedPetPosition2);
				}
				if (args.UserChanged.UserVariables.ContainsKey("DS"))
				{
					string text9 = args.UserChanged.UserVariables["DS"].ToString();
					mMOAvatar.SetKey("DS", text9);
					if (!string.IsNullOrEmpty(text9) && Enum.IsDefined(typeof(AISanctuaryPetFSM), text9))
					{
						AISanctuaryPetFSM raisedPetState = (AISanctuaryPetFSM)Enum.Parse(typeof(AISanctuaryPetFSM), text9, ignoreCase: true);
						mMOAvatar.SetRaisedPetState(raisedPetState);
					}
				}
				if (args.UserChanged.UserVariables.ContainsKey("UDT"))
				{
					string text10 = args.UserChanged.UserVariables["UDT"].ToString();
					mMOAvatar.SetKey("UDT", text10);
					MMOAchievementInfo mMOAchievementInfo2 = JsonConvert.DeserializeObject<MMOAchievementInfo>(text10);
					mMOAvatar.SetUDTPoints(mMOAchievementInfo2.P);
				}
			}
			if (args.ChangedVariableKeys.Contains("UE"))
			{
				string text11 = args.UserChanged.UserVariables["UE"].ToString();
				mMOAvatar.SetKey("UE", text11);
				mMOAvatar.HandleUserEvent(text11);
			}
			if (flag || !mMOAvatar.pMMOInit)
			{
				int num2 = -1;
				if (args.UserChanged.UserVariables.ContainsKey("t"))
				{
					num2 = Convert.ToInt32(args.UserChanged.UserVariables["t"]);
				}
				if (num2 < 0 || num2 >= mMOAvatar.pTimeReceived || !mMOAvatar.pMMOInit)
				{
					mMOAvatar.pTimeReceived = num2;
					MMOAvatarUserVarData mMOAvatarUserVarData = new MMOAvatarUserVarData();
					Vector3 position = new Vector3(Convert.ToSingle(args.UserChanged.UserVariables["P1"]), Convert.ToSingle(args.UserChanged.UserVariables["P2"]), Convert.ToSingle(args.UserChanged.UserVariables["P3"]));
					Vector3 velocity = new Vector3(Convert.ToSingle(args.UserChanged.UserVariables["R1"]), Convert.ToSingle(args.UserChanged.UserVariables["R2"]), Convert.ToSingle(args.UserChanged.UserVariables["R3"]));
					Vector3 zero = Vector3.zero;
					if (args.UserChanged.UserVariables.ContainsKey("R"))
					{
						zero.y = Convert.ToSingle(args.UserChanged.UserVariables["R"]);
					}
					float maxSpeed = 0f;
					if (args.UserChanged.UserVariables.ContainsKey("MX"))
					{
						maxSpeed = Convert.ToSingle(args.UserChanged.UserVariables["MX"]);
					}
					if (args.UserChanged.UserVariables.ContainsKey("NT"))
					{
						mMOAvatarUserVarData._ServerTimeStamp = Convert.ToDouble(args.UserChanged.UserVariables["NT"]);
					}
					if (args.UserChanged.UserVariables.ContainsKey("F"))
					{
						string value2 = args.UserChanged.UserVariables["F"].ToString();
						mMOAvatarUserVarData._Flags = (MMOAvatarFlags)Convert.ToInt32(value2, 10);
					}
					if (mMOAvatarUserVarData._Flags == MMOAvatarFlags.GLIDING && mMOAvatar.pController != null && args.UserChanged.UserVariables.ContainsKey("CUP"))
					{
						float num3 = Convert.ToSingle(args.UserChanged.UserVariables["CUP"].ToString());
						zero.x = num3 * 360f + (float)mMOAvatar.pController._GlidingAngle;
						mMOAvatar.pController.pFlyingPitch = num3;
					}
					mMOAvatarUserVarData._Position = position;
					mMOAvatarUserVarData._Rotation = Quaternion.Euler(zero);
					mMOAvatarUserVarData._Velocity = velocity;
					mMOAvatarUserVarData._MaxSpeed = maxSpeed;
					mMOAvatar.Add(mMOAvatarUserVarData);
				}
			}
			mMOAvatar.pMMOInit = true;
		}
		if (mRoomPlayersUserVarData.ContainsKey(args.UserChanged.Username) && args.ChangedVariableKeys.Contains("P1"))
		{
			MMOAvatarUserVarData mMOAvatarUserVarData2 = new MMOAvatarUserVarData();
			Vector3 position2 = new Vector3(Convert.ToSingle(args.UserChanged.UserVariables["P1"]), Convert.ToSingle(args.UserChanged.UserVariables["P2"]), Convert.ToSingle(args.UserChanged.UserVariables["P3"]));
			Vector3 velocity2 = new Vector3(Convert.ToSingle(args.UserChanged.UserVariables["R1"]), Convert.ToSingle(args.UserChanged.UserVariables["R2"]), Convert.ToSingle(args.UserChanged.UserVariables["R3"]));
			Vector3 zero2 = Vector3.zero;
			if (args.UserChanged.UserVariables.ContainsKey("R"))
			{
				zero2.y = Convert.ToSingle(args.UserChanged.UserVariables["R"]);
			}
			float maxSpeed2 = 0f;
			if (args.UserChanged.UserVariables.ContainsKey("MX"))
			{
				maxSpeed2 = Convert.ToSingle(args.UserChanged.UserVariables["MX"]);
			}
			if (args.UserChanged.UserVariables.ContainsKey("NT"))
			{
				mMOAvatarUserVarData2._ServerTimeStamp = Convert.ToDouble(args.UserChanged.UserVariables["NT"]);
			}
			if (args.UserChanged.UserVariables.ContainsKey("F"))
			{
				string value3 = args.UserChanged.UserVariables["F"].ToString();
				mMOAvatarUserVarData2._Flags = (MMOAvatarFlags)Convert.ToInt32(value3, 10);
			}
			mMOAvatarUserVarData2._Position = position2;
			mMOAvatarUserVarData2._Rotation = Quaternion.Euler(zero2);
			mMOAvatarUserVarData2._Velocity = velocity2;
			mMOAvatarUserVarData2._MaxSpeed = maxSpeed2;
			mMOAvatarUserVarData2._Priority = mRoomPlayersUserVarData[args.UserChanged.Username]._Priority;
			mRoomPlayersUserVarData[args.UserChanged.Username] = mMOAvatarUserVarData2;
		}
	}

	public bool IsMemberOnlyZone(string inZone)
	{
		string[] memberOnlyZones = mMMOLevelData.MemberOnlyZones;
		for (int i = 0; i < memberOnlyZones.Length; i++)
		{
			if (memberOnlyZones[i] == inZone)
			{
				return true;
			}
		}
		return false;
	}

	public void AddSpecialZone(string inZone)
	{
		if (!mSpecialZoneList.Contains(inZone))
		{
			mSpecialZoneList.Add(inZone, "");
		}
	}

	public void RemoveSpecialZone(string inZone)
	{
		mSpecialZoneList.Remove(inZone);
	}

	private void OnClose()
	{
		if (mPausedBy == mIdleTimeOutGenericDB)
		{
			AvAvatar.pState = mOldAvatarState;
			ObClickable.pGlobalActive = mOldClickableState;
			mPausedBy = null;
		}
		if (mNotifyObjOnIdlePopUpClose != null)
		{
			mNotifyObjOnIdlePopUpClose.SendMessage("OnCloseIdlePopUp", SendMessageOptions.DontRequireReceiver);
		}
		UnityEngine.Object.Destroy(mIdleTimeOutGenericDB);
		Input.ResetInputAxes();
		Connect();
	}

	private void ResendAvatarInfo()
	{
		if (AvAvatar.pState == AvAvatarState.NONE && ObCouch.pCurrentAvatarCouch != -1)
		{
			ObCouch.SendAvatarCouchID();
		}
	}

	public void SetSilenceParams(bool inAction, string inMessage, string inTime)
	{
		pIsSilenced = inAction;
		mWarningMessageText = inMessage;
		mSilencingTimer = (float)int.Parse(inTime) * 60f;
	}

	public void SetSilenceParams(bool inAction, string inMessage, float time)
	{
		pIsSilenced = inAction;
		mWarningMessageText = inMessage;
		mSilencingTimer = time;
	}

	private string UpdatedWarningMessage()
	{
		string result = mWarningMessageText;
		if (pIsSilenced && mSilencingTimer > 0f && mWarningMessageText.Contains("{{bannedtime}}"))
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			string empty3 = string.Empty;
			TimeSpan timeSpan = TimeSpan.FromSeconds(mSilencingTimer);
			if (timeSpan.Days > 0)
			{
				result = string.Concat(str2: (timeSpan.Days <= 1) ? StringTable.GetStringData(0, "Day") : StringTable.GetStringData(0, "Days"), str0: timeSpan.Days.ToString(), str1: " ");
			}
			else if (timeSpan.Hours <= 0)
			{
				empty3 = ((timeSpan.Minutes <= 1) ? StringTable.GetStringData(0, "Minute") : StringTable.GetStringData(0, "Minutes"));
				result = ((timeSpan.Minutes <= 0) ? ("1 " + empty3) : (timeSpan.Minutes + " " + empty3));
			}
			else
			{
				result = string.Concat(str2: (timeSpan.Hours <= 1) ? StringTable.GetStringData(0, "Hour") : StringTable.GetStringData(0, "Hours"), str0: timeSpan.Hours.ToString(), str1: " ");
			}
			result = mWarningMessageText.Replace("{{bannedtime}}", result);
		}
		return result;
	}

	public void DisplaySilenceMessage()
	{
		if (mSilenceDB == null)
		{
			mSilenceDB = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
			KAUIGenericDB component = mSilenceDB.GetComponent<KAUIGenericDB>();
			component._MessageObject = base.gameObject;
			component._OKMessage = "OnCloseSilenceMessage";
			component.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			component.SetText(UpdatedWarningMessage(), interactive: false);
			if (AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pState != 0)
			{
				mOldAvatarState = AvAvatar.pState;
				mOldClickableState = ObClickable.pGlobalActive;
				AvAvatar.pState = AvAvatarState.PAUSED;
				ObClickable.pGlobalActive = false;
				mPausedBy = mSilenceDB;
			}
			KAUI.SetExclusive(component, new Color(1f, 1f, 1f, 0.5f));
		}
	}

	private void OnCloseSilenceMessage()
	{
		if (mPausedBy == mSilenceDB)
		{
			AvAvatar.pState = mOldAvatarState;
			ObClickable.pGlobalActive = mOldClickableState;
			mPausedBy = null;
		}
		UnityEngine.Object.Destroy(mSilenceDB);
		Input.ResetInputAxes();
	}

	public string GetMMOServerIP()
	{
		if (mClient != null)
		{
			return mClient.MMOServerIP;
		}
		return "";
	}

	public void UpdateServerInfo(string ip, int port)
	{
		mIPAddress = ip;
		mPort = port;
		mForceUpdateServer = true;
	}

	public void SetUDTPoints()
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(12);
			if (userAchievementInfoByType != null)
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				MMOAchievementInfo mMOAchievementInfo = new MMOAchievementInfo();
				mMOAchievementInfo.P = (userAchievementInfoByType.AchievementPointTotal.HasValue ? userAchievementInfoByType.AchievementPointTotal.Value : 0);
				mMOAchievementInfo.R = userAchievementInfoByType.RankID;
				dictionary["UDT"] = UtUtilities.JsonSerializeObject(mMOAchievementInfo);
				mClient.SetUserVariables(dictionary);
			}
		}
	}

	public void SendPetMoodParticle(string inMoodType)
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			mClient.SendPublicMessage("PRT:" + inMoodType);
			if (MMOConsole._Capture)
			{
				MMOConsole._File.WriteLine("MSG" + "=" + "PRT" + ':' + inMoodType);
			}
		}
	}

	private void DisplayTextBox(int msgid, string msgtxt)
	{
		mUiGenericDB = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
		KAUIGenericDB component = mUiGenericDB.GetComponent<KAUIGenericDB>();
		component._MessageObject = base.gameObject;
		component.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		component.SetTextByID(msgid, msgtxt, interactive: false);
		if (AvAvatar.GetUIActive())
		{
			component._OKMessage = "CloseDialogBoxRestoreAvatar";
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
		}
		else
		{
			component._OKMessage = "CloseDialogBox";
		}
	}

	private void CloseDialogBox()
	{
		if (mUiGenericDB != null)
		{
			UnityEngine.Object.Destroy(mUiGenericDB);
		}
	}

	private void CloseDialogBoxRestoreAvatar()
	{
		if (mUiGenericDB != null)
		{
			UnityEngine.Object.Destroy(mUiGenericDB);
		}
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
	}

	public List<MMORoomVariable> GetRoomVariables()
	{
		if (mState == MMOClientState.IN_ROOM)
		{
			KnowledgeAdventure.Multiplayer.Model.MMORoom activeRoom = mClient.GetActiveRoom();
			if (activeRoom != null)
			{
				return activeRoom.RoomVariables;
			}
		}
		return null;
	}

	public List<MMORoomVariable> GetSubscribedRoomVariables()
	{
		SmartFox sFC = (MMOClientFactory.Instance as SmartFoxMMOClient2X).SFC;
		if (sFC == null)
		{
			return null;
		}
		List<Room> roomList = sFC.RoomManager.GetRoomList();
		List<MMORoomVariable> list = new List<MMORoomVariable>();
		foreach (SFSRoom item2 in roomList)
		{
			foreach (SFSRoomVariable variable in item2.GetVariables())
			{
				MMORoomVariable item = new MMORoomVariable(variable.Name, variable.Value, variable.IsPrivate, variable.IsPersistent);
				list.Add(item);
			}
		}
		return list;
	}

	public string GetMMOUserName()
	{
		if (mClient != null)
		{
			return mClient.MMOUsername;
		}
		return string.Empty;
	}

	public void SendPingMessage(double ping)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["AVG_PNG"] = ping;
		mClient.SetUserVariables(dictionary);
	}

	public void ForceEnableMultiplayer(bool enable)
	{
		if (enable && !pIsMMOEnabled)
		{
			mResetMMOStatus = true;
			pIsMMOEnabled = true;
		}
		else if (!enable && mResetMMOStatus)
		{
			mResetMMOStatus = false;
			pIsMMOEnabled = false;
		}
	}
}
