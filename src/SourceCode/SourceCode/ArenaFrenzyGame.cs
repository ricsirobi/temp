using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class ArenaFrenzyGame : MonoBehaviour, IConsumable
{
	private class PlacedElementInfo
	{
		public MapElementInfo _TrackPiece;

		public GameObject _ElementGO;

		public int _CellX;

		public int _CellY;

		public float _Yaw;
	}

	public class PlayerInfo
	{
		public delegate void PlayerLoadedEvent(PlayerInfo inPlayer);

		public string _PlayerUserID;

		public string _Name;

		public Texture _Image;

		public int _AccurateShots;

		public int _TotalShots;

		public PlayerLoadedEvent _OnPlayerLoaded;

		private bool mIsLoading;

		private KAWidget mPhotoWidget;

		public MMOAvatar _MMOAvatar;

		public bool pIsLoaded
		{
			get
			{
				if (!(_MMOAvatar != null) || !(_MMOAvatar.pObject != null) || _MMOAvatar.pReloading || !_MMOAvatar.pIsReady || !_MMOAvatar.pIsSanctuaryPetReady || _MMOAvatar.pProfileData == null)
				{
					return _PlayerUserID == UserInfo.pInstance.UserID;
				}
				return true;
			}
		}

		public void Update()
		{
			if (mIsLoading && pIsLoaded && _OnPlayerLoaded != null)
			{
				mIsLoading = false;
				_OnPlayerLoaded(this);
			}
		}

		public void Load(PlayerLoadedEvent inEvent)
		{
			mIsLoading = true;
			_OnPlayerLoaded = inEvent;
			if (null != _MMOAvatar)
			{
				_MMOAvatar.UpdateProfileData();
			}
		}

		public void SetPhotoWidget(KAWidget inPhotoWidget)
		{
			if (_Image != null)
			{
				inPhotoWidget.SetTexture(_Image);
			}
			else
			{
				mPhotoWidget = inPhotoWidget;
			}
		}

		public void OnPictureLoaded()
		{
			if (mPhotoWidget != null)
			{
				mPhotoWidget.SetTexture(_Image);
			}
		}
	}

	public class TeamInfo
	{
		public List<PlayerInfo> _Players = new List<PlayerInfo>();

		public int _Score;

		public int _AccurateShots;

		public int _TotalShots;

		public float GetAccuracyPercent()
		{
			if (_TotalShots > 0)
			{
				return (float)_AccurateShots / (float)_TotalShots * 100f;
			}
			return 0f;
		}

		public int GetAccuracyScore()
		{
			return (int)GetAccuracyPercent() * pInstance._AccuracyBonusMultiplier;
		}

		public int GetTotaScore()
		{
			return _Score + GetAccuracyScore();
		}
	}

	public enum GAME_MODE
	{
		NONE,
		SINGLE_PLAYER,
		PLAYER_VS_PLAYER,
		CLAN_VS_CLAN
	}

	[Serializable]
	public class TeamData
	{
		public int _ID;

		public Color _Color;

		public Transform[] _Markers;

		public Transform _PortraitHUDMarker;
	}

	[Serializable]
	public class GameData
	{
		public GAME_MODE _Mode;

		public UiDragonsEndDBArenaFrenzy _EndScreen;
	}

	public List<TeamData> _TeamData;

	public List<GameData> _GameData;

	public string _LevelsInfoXmlPath = "RS_DATA/ArenaFrenzyLevelsInfo.xml";

	public UiArenaFrenzyDragonSelect _DragonSelectionUi;

	public UiPowerupSelect _UiPowerupSelect;

	public UiArenaFrenzyHUD _GameHUD;

	public UiCountDown _UiCountdown;

	public UiGSBuddySelect _UiBuddySelect;

	public int _GameTimeInSecs = 180;

	public int _GameID;

	public int _AccuracyBonusMultiplier = 10;

	public float _MessageDuration = 2f;

	public string _GameModuleName = "ArenaFrenzyDO";

	public UiConsumable _UiConsumable;

	public PowerUpManager _PowerUpManager;

	public Transform _PlayerInfoHUDElement;

	public Animation _Door;

	public AudioClip _MainMenuMusic;

	public AudioClip _InGameMusic;

	public AudioClip _GameCompleteMusic;

	public AudioClip _ScoreBoardMusic;

	private KAUIGenericDB mKAUiGenericDB;

	private bool mIsGamePaused;

	public LocaleString _BuddyRoomFailedText = new LocaleString("Sorry! Unabled to find the buddy");

	public LocaleString _MatchAbandonedText = new LocaleString("Your opponents left the Game");

	public LocaleString _MatchLostText = new LocaleString("You lost the Game");

	public LocaleString _MatchWonText = new LocaleString("You Won the Game");

	public LocaleString _MatchTiedText = new LocaleString("Match Tied");

	public LocaleString _HeadToHeadDBText = new LocaleString("Looking for game");

	public LocaleString _HeadToHeadDBTitle = new LocaleString("Looking for game");

	public LocaleString _OpponentJoiningText = new LocaleString("Opponent joining game");

	public LocaleString _GameStartingText = new LocaleString("Game Starting!");

	public LocaleString _JoiningRoomText = new LocaleString("Joining game");

	private ArenaFrenzyMaps mMaps;

	private List<PlacedElementInfo> mTrackPieces = new List<PlacedElementInfo>();

	private ArenaFrenzyMapData mCurrentMapInfo;

	private int mPendingElements;

	private int mPhotoDownloadCount;

	private int mMaxPlayerCount;

	private bool mIsPlayerPetReady;

	private static int mTargetCount;

	private AvPhotoManager mPhotoManager;

	private IEnumerator mGameCompleteCoroutine;

	private Dictionary<int, TeamInfo> mTeams = new Dictionary<int, TeamInfo>();

	private List<PlayerInfo> mUnassignedPlayers = new List<PlayerInfo>();

	public GameObject _Hit3DScorePrefab;

	private GAME_MODE mGameMode;

	private bool mIsInGame;

	private bool mIsInCountDown;

	private static ArenaFrenzyGame mInstance;

	public int mCurrentSeed;

	private bool mLevelReady;

	private PowerUpHelper mPowerUpHelper = new PowerUpHelper();

	private AvAvatarLevelState mPrevLevelState;

	public Action _OnMapLoaded;

	public bool mInviteBuddies;

	public bool mIsGameAbandoned;

	private int mPlayerTeamIdx = -1;

	public Dictionary<int, TeamInfo> pTeams => mTeams;

	public GAME_MODE pGameMode => mGameMode;

	public bool pIsInGame => mIsInGame;

	public bool pIsInCountDown => mIsInCountDown;

	public static ArenaFrenzyGame pInstance => mInstance;

	public int pRandomSeed
	{
		set
		{
			mCurrentSeed = value;
			UnityEngine.Random.InitState(mCurrentSeed);
		}
	}

	private UiDragonsEndDBArenaFrenzy pEndScreen => _GameData.Find((GameData t) => t._Mode == pGameMode)?._EndScreen;

	public bool pIsReady
	{
		get
		{
			if (mLevelReady && mIsPlayerPetReady)
			{
				return mPendingElements == 0;
			}
			return false;
		}
	}

	private void Start()
	{
		if (null != mInstance)
		{
			if (null != mInstance.gameObject)
			{
				UnityEngine.Object.DestroyImmediate(mInstance.gameObject);
			}
			Debug.Log("Error: Multiple instance of ArenaFrenzy game");
		}
		mInstance = this;
		KAUICursorManager.SetDefaultCursor("Loading");
		pRandomSeed = Time.frameCount;
		RsResourceManager.Load(_LevelsInfoXmlPath, XMLDownloaded);
		mIsPlayerPetReady = false;
		mPowerUpHelper._GetParentObject = GetBombParentObject;
		mPowerUpHelper._OnBombExplode = OnBombExplode;
		mPowerUpHelper._FireProjectile = FireProjectile;
		mPowerUpHelper._IsMMO = IsMMO;
		mPrevLevelState = AvAvatar.pLevelState;
		AvAvatar.pLevelState = AvAvatarLevelState.RACING;
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.pIgnoreIdleTimeOut = true;
			MainStreetMMOClient.pInstance.SendExtensionMessage("le", "JL", null);
		}
		InitMainMenu();
	}

	private void SetGameMode(GAME_MODE newMode)
	{
		UtDebug.Log("Set Game Mode to = " + newMode);
		if (mGameMode != newMode)
		{
			GAME_MODE gAME_MODE = mGameMode;
			if ((uint)(gAME_MODE - 2) <= 1u)
			{
				mIsGameAbandoned = false;
				mIsInGame = false;
			}
			if (newMode == GAME_MODE.NONE)
			{
				AvAvatar.pState = AvAvatarState.PAUSED;
			}
			mGameMode = newMode;
		}
	}

	private void OnLevelReady()
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		mLevelReady = true;
	}

	private void OnWeaponFired()
	{
		if (mIsInGame)
		{
			mTeams[mPlayerTeamIdx]._TotalShots++;
		}
	}

	public void StartGame()
	{
		if (IsMMO())
		{
			AnimateDoor(open: false);
		}
		PlaySound(_InGameMusic);
		AvAvatar.pState = AvAvatarState.PAUSED;
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.SetVisibility(inVisible: false);
		}
		_GameHUD.OnGameStart();
		StartCountDown(OnCountDownComplete);
	}

	private void OnCountDownComplete()
	{
		AnimateDoor(open: true);
		_UiCountdown.OnCountdownDone -= OnCountDownComplete;
		_GameHUD.OnCountDownComplete();
		mIsInGame = true;
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.pSubState = AvAvatarSubState.FLYING;
		AvAvatar.pObject.GetComponent<AvAvatarController>().SetFlyingHover(inHover: true);
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.SetVisibility(inVisible: true);
		}
		InitPowerups();
		if (pGameMode != GAME_MODE.SINGLE_PLAYER)
		{
			mGameCompleteCoroutine = GameComplete(_GameTimeInSecs);
			StartCoroutine(mGameCompleteCoroutine);
		}
		mPlayerTeamIdx = GetTeamForPlayer(UserInfo.pInstance.UserID);
		mIsInCountDown = false;
		if (SanctuaryManager.pCurPetInstance != null)
		{
			WeaponManager componentInChildren = SanctuaryManager.pCurPetInstance.gameObject.GetComponentInChildren<WeaponManager>();
			if (componentInChildren != null)
			{
				componentInChildren.pFiredMessageObject = base.gameObject;
			}
		}
	}

	public void ResetGame(bool showMainMenu = true)
	{
		if ((bool)ArenaFrenzyMMOClient.pInstance)
		{
			ArenaFrenzyMMOClient.pInstance.DestroyMMO();
		}
		if (mGameCompleteCoroutine != null)
		{
			StopCoroutine(mGameCompleteCoroutine);
			mGameCompleteCoroutine = null;
		}
		if (!mIsInGame && _UiCountdown != null)
		{
			_UiCountdown.OnCountdownDone -= OnCountDownComplete;
			_UiCountdown.StartCountDown(inStart: false);
		}
		mIsInGame = false;
		mIsGamePaused = false;
		ResetLevel();
		if (_GameHUD != null)
		{
			_GameHUD.OnGameComplete();
			_GameHUD.SetVisibility(inVisible: false);
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.pMeterPaused = false;
		}
		if (_UiCountdown != null)
		{
			_UiCountdown.ApplyPause(pause: false);
		}
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.DisableAllDragonControls();
		}
		if (_UiConsumable != null)
		{
			_UiConsumable.SetVisibility(inVisible: false);
		}
		if (showMainMenu)
		{
			InitMainMenu();
		}
		mUnassignedPlayers.Clear();
	}

	private IEnumerator GameComplete(int gameTime)
	{
		yield return new WaitForSeconds(gameTime);
		SendGameCompleteMessage();
	}

	public void SendGameCompleteMessage()
	{
		if (ArenaFrenzyMMOClient.pInstance != null)
		{
			TeamInfo teamData = mTeams[mPlayerTeamIdx];
			if (pInstance.mIsGameAbandoned)
			{
				pInstance.OnGameComplete();
			}
			else
			{
				ArenaFrenzyMMOClient.pInstance.OnGameComplete(teamData);
			}
		}
	}

	private string GetModuleName(bool isMultiplayer)
	{
		string text = _GameModuleName;
		if (isMultiplayer)
		{
			text += "Multiplayer";
		}
		if (SubscriptionInfo.pIsMember)
		{
			text += "Member";
		}
		return text;
	}

	public void SetScoreData(string playerID, TeamInfo scoreData)
	{
		if (!UserInfo.pInstance.UserID.Equals(playerID) && scoreData != null)
		{
			int teamForPlayer = GetTeamForPlayer(playerID);
			mTeams[teamForPlayer]._Score = scoreData._Score;
			mTeams[teamForPlayer]._AccurateShots = scoreData._AccurateShots;
			mTeams[teamForPlayer]._TotalShots = scoreData._TotalShots;
		}
	}

	private void SetHighScoreData()
	{
		bool isMultiPlayer = IsMMO();
		int totaScore = mTeams[mPlayerTeamIdx].GetTotaScore();
		UiChallengeInvite.SetData(_GameID, 1, 0, totaScore);
		HighScores.SetCurrentGameSettings(_GameModuleName, _GameID, isMultiPlayer, 0, 1);
		HighScores.AddGameData("highscore", totaScore.ToString());
		if (pEndScreen != null)
		{
			pEndScreen.SetHighScoreData(totaScore, "highscore");
			pEndScreen.SetVisibility(Visibility: true);
			pEndScreen.SetGameSettings(_GameModuleName, base.gameObject, "any");
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.APPLY_PAYOUT)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			AchievementReward[] array = null;
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			if (inObject != null)
			{
				array = (AchievementReward[])inObject;
				if (array != null)
				{
					GameUtilities.AddRewards(array, inUseRewardManager: false, inImmediateShow: false);
				}
			}
			if (pEndScreen != null)
			{
				pEndScreen.SetRewardDisplay(array);
			}
			break;
		}
		case WsServiceEvent.ERROR:
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			UtDebug.Log("reward data is null!!!");
			break;
		}
	}

	private void PlaySound(AudioClip clip, bool bgm = true)
	{
		if (clip != null)
		{
			SnChannel.Play(clip, bgm ? "Music_Pool" : "SFX_Pool", inForce: true);
		}
	}

	public void OnGameComplete()
	{
		PlaySound(_ScoreBoardMusic, bgm: false);
		PlaySound(_GameCompleteMusic);
		if (_GameHUD != null)
		{
			_GameHUD.OnGameComplete();
		}
		if (pEndScreen != null)
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			_GameHUD.SetVisibility(inVisible: false);
			UiAvatarControls.pInstance.DisableAllDragonControls();
			if (_UiConsumable != null)
			{
				_UiConsumable.SetVisibility(inVisible: false);
			}
			pEndScreen.SetVisibility(Visibility: true);
			mIsInGame = false;
			switch (mGameMode)
			{
			case GAME_MODE.SINGLE_PLAYER:
			{
				pEndScreen.SetGameSettings(_GameModuleName + "SP", base.gameObject, "");
				pEndScreen.SetPlayerInfo(0, mTeams[0]._Players[0]._Name, mTeams[0]._Players[0]._Image);
				pEndScreen.SetResultData(0, "AccuracyScore", null, mTeams[0].GetAccuracyScore().ToString());
				pEndScreen.SetResultData(0, "GameScore", null, mTeams[0]._Score.ToString());
				pEndScreen.SetResultData(0, "TotalScore", null, mTeams[0].GetTotaScore().ToString());
				pEndScreen.SetResultData(0, "AccuracyPercent", null, mTeams[0].GetAccuracyPercent().ToString("00.00"));
				KAUICursorManager.SetDefaultCursor("Loading");
				string moduleName2 = GetModuleName(isMultiplayer: false);
				int totaScore3 = mTeams[0].GetTotaScore();
				pEndScreen.SetAdRewardData(moduleName2, totaScore3);
				WsWebService.ApplyPayout(moduleName2, totaScore3, ServiceEventHandler, null);
				break;
			}
			case GAME_MODE.PLAYER_VS_PLAYER:
			{
				pEndScreen.SetGameSettings(_GameModuleName + "PvsP", base.gameObject, "");
				for (int i = 0; i < mTeams.Count; i++)
				{
					if (mTeams[i]._Players != null && mTeams[i]._Players.Count > 0)
					{
						pEndScreen.SetPlayerInfo(i, mTeams[i]._Players[0]._Name, mTeams[i]._Players[0]._Image);
						pEndScreen.SetResultData(i, "AccuracyScore", null, mTeams[i].GetAccuracyScore().ToString());
						pEndScreen.SetResultData(i, "GameScore", null, mTeams[i]._Score.ToString());
						pEndScreen.SetResultData(i, "TotalScore", null, mTeams[i].GetTotaScore().ToString());
						pEndScreen.SetResultData(i, "AccuracyPercent", null, mTeams[i].GetAccuracyPercent().ToString("00.00"));
					}
					else
					{
						pEndScreen.SetPlayerInfo(i, null, null);
						pEndScreen.SetResultData(i, "AccuracyScore", null, "0");
						pEndScreen.SetResultData(i, "GameScore", null, "0");
						pEndScreen.SetResultData(i, "TotalScore", null, "0");
						pEndScreen.SetResultData(i, "AccuracyPercent", null, "0");
					}
				}
				int num = -1;
				if (!mIsGameAbandoned)
				{
					int totaScore = mTeams[mPlayerTeamIdx].GetTotaScore();
					for (int j = 0; j < mTeams.Count; j++)
					{
						if (mPlayerTeamIdx != j)
						{
							int totaScore2 = mTeams[j].GetTotaScore();
							if (totaScore2 > totaScore)
							{
								num = -2;
							}
							else if (totaScore2 == totaScore)
							{
								num = 0;
							}
						}
					}
				}
				if (-1 == num)
				{
					pEndScreen.SetWinLossText(_MatchWonText.GetLocalizedString());
				}
				else if (num == 0)
				{
					pEndScreen.SetWinLossText(_MatchTiedText.GetLocalizedString());
				}
				else if (-2 == num)
				{
					pEndScreen.SetWinLossText(_MatchLostText.GetLocalizedString());
				}
				if (num != 0)
				{
					KAUICursorManager.SetDefaultCursor("Loading");
					string moduleName = GetModuleName(isMultiplayer: true);
					pEndScreen.SetAdRewardData(moduleName, num);
					WsWebService.ApplyPayout(moduleName, num, ServiceEventHandler, null);
				}
				break;
			}
			}
			SetHighScoreData();
		}
		ClearTeams();
		if (SanctuaryManager.pCurPetInstance != null)
		{
			WeaponManager componentInChildren = SanctuaryManager.pCurPetInstance.gameObject.GetComponentInChildren<WeaponManager>();
			if (componentInChildren != null)
			{
				componentInChildren.pFiredMessageObject = null;
			}
		}
	}

	public void OnEndDBClose()
	{
		ResetGame();
		InitMainMenu();
	}

	public void OnReplayGame()
	{
		AnimateDoor(open: false);
		switch (pGameMode)
		{
		case GAME_MODE.SINGLE_PLAYER:
			InitSinglePlayer();
			break;
		case GAME_MODE.PLAYER_VS_PLAYER:
			InitPlayerVsPlayer();
			break;
		}
	}

	public void StartCountDown(UiCountDown.CountdownDone onCountDownDone)
	{
		if (_UiCountdown != null)
		{
			_UiCountdown.OnCountdownDone += onCountDownDone;
			_UiCountdown.StartCountDown(inStart: true);
			mIsInCountDown = true;
		}
		PositionPlayers();
	}

	public void PositionPlayers()
	{
		foreach (KeyValuePair<int, TeamInfo> team in mTeams)
		{
			SpawnPointMarkerInfo[] array = Array.FindAll(mCurrentMapInfo.SpawnMarkers, (SpawnPointMarkerInfo sm) => sm.Team - 1 == team.Key);
			if (array == null)
			{
				continue;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (i >= team.Value._Players.Count)
				{
					continue;
				}
				Transform transform = null;
				if (team.Value._Players[i]._PlayerUserID == UserInfo.pInstance.UserID)
				{
					transform = AvAvatar.mTransform;
				}
				else
				{
					MMOAvatar player = MainStreetMMOClient.pInstance.GetPlayer(team.Value._Players[i]._PlayerUserID);
					if (player != null)
					{
						transform = player.transform;
					}
				}
				if (transform != null)
				{
					GameObject gameObject = GameObject.Find(array[i].Name);
					if (gameObject != null)
					{
						transform.position = gameObject.transform.position;
						transform.rotation = gameObject.transform.rotation;
					}
				}
			}
		}
	}

	private void Update()
	{
		if (mMaps == null || mMaps.Maps == null)
		{
			return;
		}
		foreach (PlayerInfo mUnassignedPlayer in mUnassignedPlayers)
		{
			mUnassignedPlayer.Update();
		}
		if (Application.isEditor && Input.GetKeyDown(KeyCode.K))
		{
			ResetLevel();
		}
		if (Application.isEditor && Input.GetKeyDown(KeyCode.O))
		{
			pRandomSeed = Time.frameCount;
			LoadMap(0);
		}
		if (mLevelReady && !mIsPlayerPetReady && SanctuaryManager.pCurPetInstance != null && mPendingElements == 0)
		{
			mIsPlayerPetReady = true;
			if (MainStreetMMOClient.pInstance != null)
			{
				string ownerIDForCurrentLevel = MainStreetMMOClient.pInstance.GetOwnerIDForCurrentLevel();
				if (!string.IsNullOrEmpty(ownerIDForCurrentLevel) && ownerIDForCurrentLevel != UserInfo.pInstance.UserID)
				{
					mIsPlayerPetReady = false;
					if (MainStreetMMOClient.pInstance.pState != MMOClientState.IN_ROOM)
					{
						return;
					}
					if (!MainStreetMMOClient.pInstance.pRoomName.ToLower().Contains("limbo"))
					{
						if (MainStreetMMOClient.pInstance != null)
						{
							MainStreetMMOClient.pInstance.SendExtensionMessage("le", "JL", null);
						}
					}
					else
					{
						mIsPlayerPetReady = true;
						InitPlayerVsPlayer(challengeBuddy: false, ownerIDForCurrentLevel);
					}
					return;
				}
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
		if (mInviteBuddies && MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.pState == MMOClientState.IN_ROOM && mPendingElements == 0)
		{
			foreach (string pSelectedBuddy in _UiBuddySelect.pSelectedBuddies)
			{
				BuddyList.pInstance.InviteBuddy(pSelectedBuddy, null);
			}
			InitPlayerVsPlayer(challengeBuddy: true);
			mInviteBuddies = false;
		}
		if (pGameMode == GAME_MODE.SINGLE_PLAYER && mIsInGame && !mIsGamePaused && _GameHUD.pGameTime <= 0f)
		{
			OnGameComplete();
		}
	}

	public void InitMainMenu()
	{
		SetGameMode(GAME_MODE.NONE);
		_DragonSelectionUi.EnableDragonSelectionMenu();
		_UiPowerupSelect.SetVisibility(visibility: true);
		_DragonSelectionUi.SetVisibility(inVisible: true);
		_DragonSelectionUi.SetInteractive(interactive: true);
		_GameHUD.ResetHUD();
		_GameHUD.SetVisibility(inVisible: false);
		if (_UiConsumable != null)
		{
			_UiConsumable.SetVisibility(inVisible: false);
			_UiConsumable.ResetButtonStates();
		}
		AnimateDoor(open: false);
		PlaySound(_MainMenuMusic);
		int num = string.Compare(MainStreetMMOClient.pInstance.pRoomName, "LIMBO", StringComparison.OrdinalIgnoreCase);
		if (MainStreetMMOClient.pInstance != null && num != 0)
		{
			MainStreetMMOClient.pInstance.SendExtensionMessage("le", "JL", null);
		}
		TeamData teamData = _TeamData.Find((TeamData data) => data._ID == 0);
		if (teamData != null && teamData._Markers.Length != 0)
		{
			AvAvatar.SetPosition(teamData._Markers[0]);
		}
	}

	private void AnimateDoor(bool open)
	{
		string text = "Take 001";
		if (_Door != null)
		{
			Animation component = _Door.GetComponent<Animation>();
			component[text].time = 0f;
			component[text].speed = (open ? 1f : (0f - component[text].length * 30f));
			component.Play();
		}
	}

	public void OnBuddyInvite(string[] inBuddies)
	{
		if (inBuddies == null || inBuddies.Length == 0)
		{
			InitMainMenu();
			return;
		}
		mInviteBuddies = true;
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SendLoginRequest(RsResourceManager.pCurrentLevel);
		}
	}

	public void LoadMap(int mapID, int seed)
	{
		ResetLevel();
		pRandomSeed = seed;
		LoadMap(mapID);
		mTargetCount = 0;
	}

	public int GetNextTargetID()
	{
		mTargetCount++;
		return mTargetCount;
	}

	private void XMLDownloaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		if (inEvent != RsResourceLoadEvent.COMPLETE)
		{
			_ = 3;
			return;
		}
		using StringReader textReader = new StringReader((string)inFile);
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(ArenaFrenzyMaps));
		mMaps = (ArenaFrenzyMaps)xmlSerializer.Deserialize(textReader);
		LoadMap(0);
	}

	public int NextRandom()
	{
		return UnityEngine.Random.Range(0, int.MaxValue);
	}

	public void LoadMap(int inLevelIdx)
	{
		mCurrentMapInfo = mMaps.Maps[inLevelIdx];
		int num = 0;
		if (mCurrentMapInfo.MapElements != null)
		{
			for (int i = 0; i < mCurrentMapInfo.MapElements.Length; i++)
			{
				MapElementInfo mapElementInfo = mCurrentMapInfo.MapElements[i];
				if (mapElementInfo.CellSizeX > num)
				{
					num = mapElementInfo.CellSizeX;
				}
				if (mapElementInfo.CellSizeY > num)
				{
					num = mapElementInfo.CellSizeY;
				}
			}
		}
		int num2 = mCurrentMapInfo.MinElements;
		if (mCurrentMapInfo.MaxElements > mCurrentMapInfo.MinElements)
		{
			num2 += NextRandom() % (mCurrentMapInfo.MaxElements - mCurrentMapInfo.MinElements + 1);
		}
		int num3 = mCurrentMapInfo.MapDimensions.NumCellsX / num;
		int num4 = mCurrentMapInfo.MapDimensions.NumCellsX / num * (mCurrentMapInfo.MapDimensions.NumCellsY / num);
		int num5 = 0;
		List<int> list = new List<int>();
		for (int j = 0; j < num4; j++)
		{
			list.Add(j);
		}
		while (num2 - num5 > 0)
		{
			int index = NextRandom() % list.Count;
			int num6 = list[index];
			list.RemoveAt(index);
			int probableMapElement = GetProbableMapElement(mMaps.Maps[inLevelIdx].MapElements);
			PlacedElementInfo placedElementInfo = new PlacedElementInfo
			{
				_TrackPiece = mCurrentMapInfo.MapElements[probableMapElement]
			};
			int num7 = NextRandom() % 4;
			placedElementInfo._Yaw = 90 * num7;
			int num8 = ((num7 == 0 || num7 == 2) ? placedElementInfo._TrackPiece.CellSizeX : placedElementInfo._TrackPiece.CellSizeY);
			int num9 = ((num7 == 0 || num7 == 2) ? placedElementInfo._TrackPiece.CellSizeY : placedElementInfo._TrackPiece.CellSizeX);
			int num10 = num6 % num3;
			int num11 = num6 / num3;
			placedElementInfo._CellX = num10 * num;
			placedElementInfo._CellY = num11 * num;
			if (num - num8 > 0)
			{
				placedElementInfo._CellX += NextRandom() % (num - num8);
			}
			if (num - num9 > 0)
			{
				placedElementInfo._CellY += NextRandom() % (num - num9);
			}
			mTrackPieces.Add(placedElementInfo);
			num5++;
			if (mTrackPieces.Count >= num2)
			{
				break;
			}
		}
		LoadMapElements();
	}

	public void FireAProjectile(bool isPowerUp = false)
	{
		Ray ray = Camera.main.ScreenPointToRay(KAInput.mousePosition);
		float maxDistance = 200f;
		if (isPowerUp)
		{
			Vector2 vector = new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
			ray = Camera.main.ScreenPointToRay(new Vector3(vector.x, vector.y, 0f));
			maxDistance = 50f;
		}
		if (Physics.Raycast(ray, out var hitInfo, maxDistance, UtUtilities.GetGroundRayCheckLayers()))
		{
			SanctuaryManager.pCurPetInstance.Fire(null, useDirection: true, hitInfo.point, ignoreCoolDown: true);
		}
	}

	private void OnPlayerLoaded(PlayerInfo inPlayer)
	{
		if (CheckAllPlayersLoaded())
		{
			ArenaFrenzyMMOClient.pInstance.OnAllPlayersLoaded();
		}
	}

	private void InitializeNewPlayer(PlayerInfo inPlayer)
	{
		inPlayer.Load(OnPlayerLoaded);
		if (!pIsInGame && ArenaFrenzyMMOClient.pInstance.pIsHost)
		{
			_GameHUD.SetMMOStatusMessage(_OpponentJoiningText);
		}
	}

	public void AssignPlayerToTeam(string inPlayerID, int teamIdx)
	{
		PlayerInfo playerInfo = mUnassignedPlayers.Find((PlayerInfo p) => p._PlayerUserID == inPlayerID);
		if (playerInfo != null && teamIdx >= 0)
		{
			AddPlayer(playerInfo, teamIdx);
		}
		else
		{
			UtDebug.LogWarning("Player not found in unassigned list: " + inPlayerID);
		}
	}

	public bool CheckAllPlayersLoaded()
	{
		if (mUnassignedPlayers.Count >= mMaxPlayerCount)
		{
			return mUnassignedPlayers.Find((PlayerInfo p) => !p.pIsLoaded) == null;
		}
		return false;
	}

	public void AddPlayer(PlayerInfo inPlayer, int inTeamIndex = -1)
	{
		if (inTeamIndex == -1)
		{
			mUnassignedPlayers.Add(inPlayer);
			InitializeNewPlayer(inPlayer);
			if (mUnassignedPlayers.Count < mMaxPlayerCount && pInstance != null)
			{
				pInstance._GameHUD.WaitingForPlayers(waiting: true);
			}
			else
			{
				pInstance.ShowJoiningRoomDB();
			}
			return;
		}
		TeamInfo teamInfo = null;
		if (!mTeams.ContainsKey(inTeamIndex))
		{
			teamInfo = new TeamInfo();
			teamInfo._Players.Add(inPlayer);
			mTeams.Add(inTeamIndex, teamInfo);
		}
		else
		{
			teamInfo = mTeams[inTeamIndex];
			teamInfo._Players.Add(inPlayer);
		}
		if (GAME_MODE.PLAYER_VS_PLAYER == pGameMode)
		{
			TakePicture(inPlayer, inTeamIndex);
		}
		if (mUnassignedPlayers.Exists((PlayerInfo p) => p._PlayerUserID == inPlayer._PlayerUserID))
		{
			mUnassignedPlayers.RemoveAll((PlayerInfo p) => p._PlayerUserID == inPlayer._PlayerUserID);
		}
		if (!(inPlayer._PlayerUserID == UserInfo.pInstance.UserID))
		{
			return;
		}
		TeamData teamData = _TeamData.Find((TeamData data) => data._ID == inTeamIndex);
		if (teamData != null)
		{
			int num = teamInfo._Players.Count - 1;
			if (num < teamData._Markers.Length)
			{
				AvAvatar.SetPosition(teamData._Markers[num]);
			}
			if (_PlayerInfoHUDElement != null)
			{
				_GameHUD.AddHUDElement(_PlayerInfoHUDElement, teamData._PortraitHUDMarker);
			}
		}
	}

	public void ClearTeams()
	{
		mTeams.Clear();
	}

	public void RemovePlayer(string inPlayerUserID)
	{
		bool flag = false;
		foreach (KeyValuePair<int, TeamInfo> mTeam in mTeams)
		{
			TeamInfo value = mTeam.Value;
			PlayerInfo playerInfo = value._Players.Find((PlayerInfo plyr) => plyr._PlayerUserID.Equals(inPlayerUserID));
			if (playerInfo != null)
			{
				value._Players.Remove(playerInfo);
				flag = true;
				break;
			}
		}
		PlayerInfo playerInfo2 = mUnassignedPlayers.Find((PlayerInfo plyr) => plyr._PlayerUserID.Equals(inPlayerUserID));
		if (playerInfo2 != null)
		{
			mUnassignedPlayers.Remove(playerInfo2);
			flag = true;
		}
		if (IsMMO() && flag)
		{
			mIsGameAbandoned = true;
			ShowGameAbandonedMsg();
		}
	}

	private void ShowGameAbandonedMsg()
	{
		_GameHUD.ShowMessage(_MatchAbandonedText.GetLocalizedString(), _MessageDuration);
	}

	public int GetTeamForPlayer(string inUserID)
	{
		foreach (KeyValuePair<int, TeamInfo> mTeam in mTeams)
		{
			if (mTeam.Value._Players.Exists((PlayerInfo p) => p._PlayerUserID == inUserID))
			{
				return mTeam.Key;
			}
		}
		return -1;
	}

	public void AddScore(string inPlayerID, ArenaFrenzyTarget inTarget)
	{
		int teamForPlayer = GetTeamForPlayer(inPlayerID);
		if (teamForPlayer < 0)
		{
			return;
		}
		int num = inTarget._TeamScores[teamForPlayer];
		mTeams[teamForPlayer]._Score += num;
		mTeams[teamForPlayer]._AccurateShots += ((num > 0) ? 1 : 0);
		PlayerInfo playerInfo = mTeams[teamForPlayer]._Players.Find((PlayerInfo p) => p._PlayerUserID == inPlayerID);
		if (playerInfo != null)
		{
			playerInfo._TotalShots++;
			playerInfo._AccurateShots += ((num > 0) ? 1 : 0);
			if (inPlayerID == UserInfo.pInstance.UserID)
			{
				Show3DTargetHitScore(inTarget.transform.position, num);
			}
		}
	}

	public void OnTargetHit(ArenaFrenzyTarget inTarget, string userID)
	{
		if (IsMMO() && mIsInGame)
		{
			if (ArenaFrenzyMMOClient.pInstance != null && UserInfo.pInstance.UserID == userID)
			{
				ArenaFrenzyMMOClient.pInstance.OnTargetHit(inTarget.pTargetID);
			}
		}
		else
		{
			pInstance.AddScore(userID, inTarget);
			inTarget.gameObject.SetActive(value: false);
		}
	}

	public void AddScore(int inTeamIdx, int inScore)
	{
		if (inTeamIdx >= 0 && inTeamIdx < mTeams.Count)
		{
			mTeams[inTeamIdx]._Score += inScore;
		}
	}

	public int GetScore(int inTeamIdx)
	{
		if (inTeamIdx >= 0 && inTeamIdx < mTeams.Count)
		{
			return mTeams[inTeamIdx]._Score;
		}
		return -1;
	}

	public int GetTeamCount()
	{
		return mTeams.Count;
	}

	private string GetPlayerGroupID(string inPlayerUserID)
	{
		MMOAvatar mMOAvatar = null;
		if (MainStreetMMOClient.pInstance != null)
		{
			mMOAvatar = MainStreetMMOClient.pInstance.GetPlayer(inPlayerUserID);
		}
		string text = ((mMOAvatar != null) ? mMOAvatar.pProfileData.GetGroupID() : null);
		if (string.IsNullOrEmpty(text))
		{
			text = (UserProfile.pProfileData.ID.Equals(inPlayerUserID) ? UserProfile.pProfileData.GetGroupID() : null);
		}
		return text;
	}

	public ArenaFrenzyTarget GetTarget(int inTargetID)
	{
		foreach (PlacedElementInfo mTrackPiece in mTrackPieces)
		{
			ArenaFrenzyMapElement component = mTrackPiece._ElementGO.GetComponent<ArenaFrenzyMapElement>();
			if (component != null)
			{
				ArenaFrenzyTarget target = component.GetTarget(inTargetID);
				if (target != null)
				{
					return target;
				}
			}
		}
		return null;
	}

	private IEnumerator TakePictureDelayed(PlayerInfo inPlayer, int inTeamIndex)
	{
		yield return new WaitForSeconds((float)inTeamIndex * 0.333f);
		Texture2D dstTex = new Texture2D(256, 256, TextureFormat.ARGB32, mipChain: false);
		if (mPhotoManager == null)
		{
			mPhotoManager = AvPhotoManager.Init("PfToolbarPhotoMgr");
			mPhotoManager.name = "PfToolbarPhotoMgr";
		}
		mPhotoManager._AvatarCam.GetComponent<Camera>().backgroundColor = pInstance.GetTeamColor(inTeamIndex);
		mPhotoManager.TakePhoto(inPlayer._PlayerUserID, dstTex, ProfileAvPhotoCallback, inPlayer);
	}

	private void TakePicture(PlayerInfo player, int teamIdx)
	{
		if (player != null)
		{
			mPhotoDownloadCount++;
			StartCoroutine(TakePictureDelayed(player, teamIdx));
		}
	}

	public void ProfileAvPhotoCallback(Texture tex, object inUserData)
	{
		PlayerInfo playerInfo = (PlayerInfo)inUserData;
		if (playerInfo != null)
		{
			playerInfo._Image = tex;
			playerInfo.OnPictureLoaded();
		}
		mPhotoDownloadCount--;
	}

	private void LoadMapElements()
	{
		if (mTrackPieces != null && mTrackPieces.Count > 0)
		{
			mPendingElements = mCurrentMapInfo.MapElements.Length;
			MapElementInfo[] mapElements = mCurrentMapInfo.MapElements;
			for (int i = 0; i < mapElements.Length; i++)
			{
				string[] array = mapElements[i].PrefabFilePath.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], MapElementsEventHandler, typeof(GameObject));
			}
		}
	}

	private void MapElementsEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			List<PlacedElementInfo> list = mTrackPieces.FindAll((PlacedElementInfo tp) => tp._TrackPiece.PrefabFilePath == inURL);
			float num = mCurrentMapInfo.MapDimensions.CellWidth * mCurrentMapInfo.MapDimensions.NumCellsX / 2;
			float num2 = mCurrentMapInfo.MapDimensions.CellHeight * mCurrentMapInfo.MapDimensions.NumCellsY / 2;
			float num3 = mCurrentMapInfo.MapDimensions.CellWidth / 2;
			float num4 = mCurrentMapInfo.MapDimensions.CellHeight / 2;
			for (int i = 0; i < list.Count; i++)
			{
				Vector3 position = new Vector3((float)(list[i]._CellX * mCurrentMapInfo.MapDimensions.CellWidth) - num + num3, 0f, (float)(list[i]._CellY * mCurrentMapInfo.MapDimensions.CellHeight) - num2 + num4);
				list[i]._ElementGO = UnityEngine.Object.Instantiate((GameObject)inObject, position, Quaternion.Euler(0f, list[i]._Yaw, 0f));
				list[i]._ElementGO.transform.parent = base.transform;
			}
			mPendingElements--;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mPendingElements--;
			UtDebug.LogError("Could not load track piece: " + inURL);
			break;
		}
		if (mPendingElements <= 0)
		{
			InitMapElements();
			InitTargets();
		}
	}

	private void ResetLevel()
	{
		mCurrentMapInfo = null;
		mPendingElements = 0;
		mTrackPieces.Clear();
		ClearTeams();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			UnityEngine.Object.Destroy(base.transform.GetChild(i).gameObject);
		}
	}

	private void InitMapElements()
	{
		for (int i = 0; i < mTrackPieces.Count; i++)
		{
			ArenaFrenzyMapElement component = mTrackPieces[i]._ElementGO.GetComponent<ArenaFrenzyMapElement>();
			if (component != null)
			{
				component.Init(this);
			}
		}
		if (mGameMode == GAME_MODE.SINGLE_PLAYER)
		{
			PlayerInfo playerInfo = new PlayerInfo();
			playerInfo._PlayerUserID = UserInfo.pInstance.UserID;
			AddPlayer(playerInfo, 0);
			StartGame();
		}
		else if (_OnMapLoaded != null)
		{
			_OnMapLoaded();
		}
	}

	private void InitTargets()
	{
		pRandomSeed = mCurrentSeed;
		int num = mCurrentMapInfo.MinTargets + NextRandom() % (mCurrentMapInfo.MaxTargets - mCurrentMapInfo.MinTargets + 1);
		int num2 = mCurrentMapInfo.MinBonusTargets + NextRandom() % (mCurrentMapInfo.MaxBonusTargets - mCurrentMapInfo.MinBonusTargets + 1);
		int num3 = 0;
		List<ArenaFrenzyMapElement> list = new List<ArenaFrenzyMapElement>();
		foreach (PlacedElementInfo mTrackPiece in mTrackPieces)
		{
			ArenaFrenzyMapElement component = mTrackPiece._ElementGO.GetComponent<ArenaFrenzyMapElement>();
			if (component != null && component.pFreeTargetSlots > 0)
			{
				list.Add(component);
				num3 += component.pFreeTargetSlots;
			}
		}
		num3 -= num2;
		if (num > num3)
		{
			num = num3;
		}
		for (int i = 0; i < num; i++)
		{
			int index = NextRandom() % list.Count;
			ArenaFrenzyTarget.ArenaFrenzyTargetType arenaFrenzyTargetType = (ArenaFrenzyTarget.ArenaFrenzyTargetType)(i % 2 + 1);
			if (pGameMode == GAME_MODE.SINGLE_PLAYER && arenaFrenzyTargetType == ArenaFrenzyTarget.ArenaFrenzyTargetType.TARGET_TEAM_2)
			{
				arenaFrenzyTargetType = ArenaFrenzyTarget.ArenaFrenzyTargetType.TARGET_TEAM_1;
			}
			list[index].SetupTarget(arenaFrenzyTargetType);
			if (list[index].pFreeTargetSlots <= 0)
			{
				list.RemoveAt(index);
			}
		}
		for (int j = 0; j < num2; j++)
		{
			int index2 = NextRandom() % list.Count;
			list[index2].SetupTarget(ArenaFrenzyTarget.ArenaFrenzyTargetType.TARGET_NEUTRAL);
			if (list[index2].pFreeTargetSlots <= 0)
			{
				list.RemoveAt(index2);
			}
		}
	}

	public Color GetTeamColor(int idx)
	{
		return _TeamData.Find((TeamData t) => t._ID == idx)?._Color ?? Color.black;
	}

	private int GetProbableMapElement(MapElementInfo[] inMap)
	{
		int result = 0;
		if (inMap != null && inMap.Length > 1)
		{
			int num = 0;
			foreach (MapElementInfo mapElementInfo in inMap)
			{
				num += mapElementInfo.AppearanceChance;
			}
			int num2 = NextRandom() % num;
			num = 0;
			for (int j = 0; j < inMap.Length; j++)
			{
				num += inMap[j].AppearanceChance;
				if (num > num2)
				{
					result = j;
					break;
				}
			}
		}
		return result;
	}

	public void InitPlayerVsPlayer(bool challengeBuddy = false, string joinBuddyID = "")
	{
		mMaxPlayerCount = 2;
		LoadMap(0, Time.frameCount);
		SetGameMode(GAME_MODE.PLAYER_VS_PLAYER);
		ArenaFrenzyMMOClient.Init(challengeBuddy, joinBuddyID);
		SetupAvatar();
		SetupHUD();
		AnimateDoor(open: true);
	}

	public void InitSinglePlayer()
	{
		mMaxPlayerCount = 1;
		SetGameMode(GAME_MODE.SINGLE_PLAYER);
		SetupAvatar();
		LoadMap(0, Time.frameCount);
		SetupHUD();
	}

	private void SetupHUD()
	{
		_UiPowerupSelect.SetVisibility(visibility: false);
		_GameHUD.SetVisibility(inVisible: true);
		_GameHUD.InitScore();
		if (!IsMMO())
		{
			_GameHUD.WaitingForPlayers(waiting: false);
		}
		if (_PlayerInfoHUDElement != null)
		{
			_GameHUD.AddHUDElement(_PlayerInfoHUDElement);
		}
		if (pEndScreen != null)
		{
			pEndScreen.SetRewardDisplay(null);
		}
		if (pGameMode == GAME_MODE.PLAYER_VS_PLAYER)
		{
			ShowGenericDB("HeadToHeadDB", _HeadToHeadDBText, _HeadToHeadDBTitle);
			Invoke("DestroyDB", _MessageDuration);
		}
		if (SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.pDisablePetSwitch = true;
		}
	}

	public void ShowJoiningRoomDB()
	{
		_GameHUD.OnJoiningGame();
		CancelInvoke("DestroyDB");
		ShowGenericDB("HeadToHeadDB", _JoiningRoomText, null);
		Invoke("DestroyDB", _MessageDuration);
	}

	private void SetupAvatar()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.pSubState = AvAvatarSubState.FLYING;
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		component._NoFlyingLanding = true;
		component.SetFlyingHover(inHover: true);
		if (SanctuaryManager.pCurPetInstance != null)
		{
			if (!SanctuaryManager.pCurPetInstance.pIsMounted)
			{
				SanctuaryPet.AddMountEvent(SanctuaryManager.pCurPetInstance, DragonMounted);
				SanctuaryManager.pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.FLY);
			}
			else
			{
				DragonMounted(mount: true, PetSpecialSkillType.FLY);
			}
		}
	}

	private void InitPowerups()
	{
		if (_UiConsumable != null)
		{
			_UiConsumable.ResetButtonStates();
			_UiConsumable.SetGameData(this, "ArenaFrenzy");
			_UiConsumable.SetVisibility(inVisible: true);
		}
		if (_PowerUpManager != null)
		{
			_PowerUpManager.Init(this, mPowerUpHelper);
		}
	}

	public void DragonMounted(bool mount, PetSpecialSkillType skill)
	{
		if (mGameMode == GAME_MODE.NONE)
		{
			if (UiAvatarControls.pInstance != null)
			{
				AvAvatar.pState = AvAvatarState.PAUSED;
				UiAvatarControls.pInstance.SetVisibility(inVisible: false);
			}
		}
		else if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.SetVisibility(inVisible: true);
			UiAvatarControls.pInstance.EnableDragonFireButton(inEnable: true);
			UiAvatarControls.pInstance.DisableAllDragonControls(inDisable: false);
			UiAvatarControls.EnableTiltControls(enable: true, recalibrate: false);
		}
	}

	public void OnConsumableUpdated(Consumable inConsumable)
	{
		if (_PowerUpManager != null)
		{
			PowerUp powerUp = null;
			powerUp = ((pGameMode != GAME_MODE.SINGLE_PLAYER) ? _PowerUpManager.InitPowerUp(inConsumable.name, isMMO: true) : _PowerUpManager.InitPowerUp(inConsumable.name, isMMO: false));
			if (powerUp != null)
			{
				powerUp.Activate();
			}
			else
			{
				UtDebug.LogError("Power Up is null");
			}
		}
	}

	public void ExitGame()
	{
		AvAvatar.pLevelState = mPrevLevelState;
		ProcessUserLeft();
		if ((bool)ArenaFrenzyMMOClient.pInstance)
		{
			ArenaFrenzyMMOClient.pInstance.DestroyMMO();
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.pIgnoreIdleTimeOut = false;
		}
		ResetAvatarProperties();
		if (SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.pDisablePetSwitch = false;
		}
		UtUtilities.LoadLevel();
	}

	private void ResetAvatarProperties()
	{
		AvAvatar.pObject.GetComponent<AvAvatarController>()._NoFlyingLanding = false;
	}

	public bool IsMMO()
	{
		if (pGameMode != 0)
		{
			return pGameMode != GAME_MODE.SINGLE_PLAYER;
		}
		return false;
	}

	public GameObject GetBombParentObject(MMOMessageReceivedEventArgs args)
	{
		return AvAvatar.pAvatarCam;
	}

	public void FireProjectile()
	{
		FireAProjectile(isPowerUp: true);
	}

	public void OnBombExplode(Vector3 bombPos, float radius)
	{
		foreach (PlacedElementInfo mTrackPiece in mTrackPieces)
		{
			ArenaFrenzyMapElement component = mTrackPiece._ElementGO.GetComponent<ArenaFrenzyMapElement>();
			if (!(component != null))
			{
				continue;
			}
			foreach (GameObject pTarget in component.pTargets)
			{
				if (pTarget != null)
				{
					ArenaFrenzyTarget component2 = pTarget.GetComponent<ArenaFrenzyTarget>();
					if (pTarget != null && component2._TeamScores[mPlayerTeamIdx] > 0 && Vector3.Distance(pTarget.transform.position, bombPos) <= radius)
					{
						component.HandleTargetHit(component2, isLocal: true, UserProfile.pProfileData);
					}
				}
			}
		}
	}

	public void JoinBuddyFailed()
	{
		InitMainMenu();
		ShowGenericDB("JoinBuddyFailed", _BuddyRoomFailedText, null, "DestroyDB");
	}

	private void ShowGenericDB(string inDBName, LocaleString inText, LocaleString inTitle, string okMessage = "")
	{
		if (mKAUiGenericDB != null)
		{
			DestroyDB();
		}
		mKAUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", inDBName);
		mKAUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, !string.IsNullOrEmpty(okMessage), inCloseBtn: false);
		mKAUiGenericDB._MessageObject = base.gameObject;
		mKAUiGenericDB._OKMessage = okMessage;
		mKAUiGenericDB.SetText(inText.GetLocalizedString(), interactive: false);
		if (inTitle != null)
		{
			mKAUiGenericDB.SetTitle(inTitle.GetLocalizedString());
		}
		KAUI.SetExclusive(mKAUiGenericDB);
	}

	private void DestroyDB()
	{
		if (!(mKAUiGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUiGenericDB);
			UnityEngine.Object.Destroy(mKAUiGenericDB.gameObject);
			mKAUiGenericDB = null;
		}
	}

	private void Show3DTargetHitScore(Vector3 inPosition, int inScore)
	{
		TargetHit3DScore.Show3DHitScore(_Hit3DScorePrefab, inPosition, inScore);
	}

	public void OnBackBtnClick()
	{
		if (pGameMode == GAME_MODE.SINGLE_PLAYER)
		{
			if (!_UiCountdown.IsCountDownOver())
			{
				_UiCountdown.ApplyPause(pause: true);
			}
			AvAvatar.pState = AvAvatarState.PAUSED;
			SanctuaryManager.pCurPetInstance.pMeterPaused = true;
			mIsGamePaused = true;
		}
	}

	public void OnClosingQuitDB()
	{
		if (pGameMode == GAME_MODE.SINGLE_PLAYER)
		{
			if (!_UiCountdown.IsCountDownOver())
			{
				_UiCountdown.ApplyPause(pause: false);
			}
			AvAvatar.pState = AvAvatarState.IDLE;
			SanctuaryManager.pCurPetInstance.pMeterPaused = false;
			mIsGamePaused = false;
		}
	}

	private void ProcessUserLeft()
	{
		if (IsMMO() && mIsInGame)
		{
			string moduleName = GetModuleName(isMultiplayer: true);
			pEndScreen.SetAdRewardData(moduleName, -2);
			WsWebService.ApplyPayout(moduleName, -2, null, null);
		}
	}

	private void OnApplicationQuit()
	{
		ProcessUserLeft();
	}
}
