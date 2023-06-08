using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JSGames;
using JSGames.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace SquadTactics;

public class GameManager : MonoBehaviour
{
	public enum GameState
	{
		PAUSED,
		ENEMY,
		PLAYER,
		GAMEOVER
	}

	public enum TurnState
	{
		INITIALIZATION,
		INPUT,
		ABILITYONGOING,
		WAITINGTOEND
	}

	public const string WIN = "Win";

	public const string LOSE = "Lose";

	public const string QUIT = "Quit";

	public const string RESTART = "Restart";

	public const string SFX_POOL = "STSFX_Pool";

	public const string MUSIC_POOL = "STMusic_Pool";

	public const string AMBIENT_POOL = "STAmbient_Pool";

	public const string EFFECT_POOL = "STEffect_Pool";

	public const string CINEMATIC_POOL = "STCinematic_Pool";

	public static GameManager pInstance;

	public int _GameID = 101;

	public string _GameModuleName = "DOSquadTactics";

	public string _MainMenuLevelName = "STLevelSelectionDO";

	public string _QuestionCategory = "";

	public string _TriviaQuestionCategory = "";

	public string _WeaponBundlePath = "RS_DATA/STWeapons.unity3d/";

	public GameState _GameState;

	public TurnState _TurnState;

	public LocaleString _AlertedText = new LocaleString("ALERTED");

	public LocaleString _CriticalText = new LocaleString("CRITICAL");

	public LocaleString _CounterText = new LocaleString("BOOSTED");

	public LocaleString _FleeText = new LocaleString("RETREATING");

	public LocaleString _FledText = new LocaleString("RETREATED");

	public LocaleString _DodgeText = new LocaleString("DODGED");

	public LocaleString _IncapacitatedText = new LocaleString("INCAPACITATED");

	public LocaleString _NoMoveText = new LocaleString("NO MOVE ACTION");

	public LocaleString _NoAbilityText = new LocaleString("NO ABILITY ACTION");

	public int _CreditChestsAvailable = 4;

	public float _PositiveCounterMultiplier = 1.5f;

	public int _RevivalTurns = 3;

	public int _NormalMoveCost = 1;

	public int _MaxAnalyticsEventAbility = 10;

	public float _DiagonalMoveCost = 1.5f;

	public float _MovementTimePerNode = 1f;

	public float _RotationTimePerNode = 0.3f;

	public float _SelfCastTimer = 0.5f;

	public float _AbilitySetupTimer = 0.3f;

	public float _TurnInitializationTimer = 1.5f;

	public float _MinimumEnemyTurnDuration = 1f;

	public float _ResultScreenDelay = 2.5f;

	public float _ParticleSpawnDelay = 1f;

	public float _EnemySpawnDelay = 1f;

	public float[] _TimeScaleSpeeds;

	public bool _ShowNoMovementTip;

	public bool _ShowNoAbilityTip;

	public Vector2 _PlayerFleeNodeLocation;

	public Vector2 _EnemyFleeNodeLocation;

	public Color _TxtColorMandatoryObjective;

	public Color _TxtColorOptionalObjective;

	private int mTimeScaleIndex;

	private Node mPlayerFleeNode;

	private Node mEnemyFleeNode;

	public Material _PathMaterial;

	public Material _AttackPathMaterial;

	public Material _MovesMaterial;

	public Collider _Boundary;

	public List<Character> _ActiveUnits;

	public List<Character> _ConditionalUnits;

	private List<Character> mPlayerUnits = new List<Character>();

	private List<Character> mEnemyUnits = new List<Character>();

	private List<Character> mInanimateUnits = new List<Character>();

	private List<Character> mAllPlayerUnits = new List<Character>();

	private List<Character> mAllEnemyUnits = new List<Character>();

	private List<Character> mAllInanimateUnits = new List<Character>();

	public Grid _Grid;

	[NonSerialized]
	public Dictionary<string, UnitData> pUnitsAnalyticInfo = new Dictionary<string, UnitData>();

	[NonSerialized]
	public Dictionary<string, int> pUsedAbilityInfo = new Dictionary<string, int>();

	private LevelLayout mLayout;

	public Character _SelectedCharacter;

	public UiSquadTacticsHUD _HUD;

	public UiEndDB _UiSTEndDB;

	public Tutorial _Tutorial;

	public LayerMask _GridLayerMask;

	public GameObject _FloatingTextPrefab;

	public float _FloatingTextDuration = 2f;

	public AudioClip _GamePlayBGMusic;

	public AudioClip _AmbientNoise;

	public AudioClip _GameStartSFX;

	public AudioClip _GameWinMusic;

	public AudioClip _GameLoseMusic;

	public AudioClip _PlayerTurnBeginSFX;

	public AudioClip _PlayerTurnEndSFX;

	public AudioClip _ValidMoveSFX;

	public AudioClip _ValidAbilityUseSFX;

	private Node mLastSelectedNode;

	private float mSelfCastTimerStart;

	private int mDataLoaded;

	private bool mStartedMove;

	private AvPhotoManager mStillPhotoManager;

	private int mCreditChestsCollected;

	private List<UnitSelection> mPlayerSpawns;

	private List<UnitSpawn> mPlayerSpawnGrids;

	private List<UnitSpawn> mEnemySpawnGrids;

	private List<UnitSpawn> mInanimateSpawnGrids;

	private int mPlayerTurnCounter;

	private bool mLoadedAllowCinematicCamera;

	private bool mAllowCinematicCamera = true;

	public GameObject _ObjectiveMarker;

	public GameObject _BeamParticle;

	public float _ObjectivesCloseAfterDelay = 5f;

	private float mSessionStartTime;

	private int mObjectiveHiddenByParentCount;

	private int mObjectiveUnlockCount;

	public int pTimeScaleIndex => mTimeScaleIndex;

	public Node pPlayerFleeNode
	{
		get
		{
			if (mPlayerFleeNode == null)
			{
				mPlayerFleeNode = _Grid.GetNodeByPosition(0, 0);
			}
			return mPlayerFleeNode;
		}
	}

	public Node pEnemyFleeNode
	{
		get
		{
			if (mEnemyFleeNode == null)
			{
				mEnemyFleeNode = _Grid.GetNodeByPosition(0, 0);
			}
			return mEnemyFleeNode;
		}
	}

	public List<Character> pAllPlayerUnits => mAllPlayerUnits;

	public float pGamePlayTime { get; private set; }

	public AvPhotoManager pStillPhotoManager => mStillPhotoManager;

	public bool pAllowCinematicCamera
	{
		get
		{
			if (!mLoadedAllowCinematicCamera && UserInfo.pIsReady)
			{
				mAllowCinematicCamera = Convert.ToBoolean(PlayerPrefs.GetInt("STACC_" + UserInfo.pInstance.ParentUserID, 1));
				mLoadedAllowCinematicCamera = true;
			}
			return mAllowCinematicCamera;
		}
		set
		{
			if (UserInfo.pIsReady)
			{
				PlayerPrefs.SetInt("STACC_" + UserInfo.pInstance.ParentUserID, value ? 1 : 0);
			}
			mAllowCinematicCamera = value;
			mLoadedAllowCinematicCamera = true;
		}
	}

	public List<RaisedPetEntityMap> pRaisedPetEntityMap { get; set; }

	public List<Objective> GetHeaderObjectives()
	{
		return GetSceneData().pHeaderObjectiveList;
	}

	public List<Objective> GetFinalObjectives()
	{
		return GetSceneData().pFinalObjectiveList;
	}

	public SceneData GetSceneData()
	{
		return mLayout.pRandomSceneData;
	}

	private void Awake()
	{
		if (pInstance == null)
		{
			pInstance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		mSessionStartTime = Time.time;
	}

	private IEnumerator Start()
	{
		mPlayerTurnCounter = 1;
		_Grid.CreateGrid();
		mLayout = UnityEngine.Object.FindObjectOfType<LevelLayout>();
		mLayout.Init();
		mObjectiveHiddenByParentCount = GetFinalObjectives().FindAll((Objective x) => x.pHiddenStatus == ObjectiveHiddenStatus.HIDDEN_BY_PARENT).Count;
		mObjectiveUnlockCount = GetFinalObjectives().FindAll((Objective x) => x.pHiddenStatus == ObjectiveHiddenStatus.UNHIDDEN).Count;
		foreach (Objective finalObjective in GetFinalObjectives())
		{
			CreateObjectiveVisitMarker(finalObjective);
		}
		_ActiveUnits.Clear();
		_ConditionalUnits.Clear();
		if (mStillPhotoManager == null)
		{
			mStillPhotoManager = AvPhotoManager.Init("PfSTPhotoMgr");
		}
		mPlayerFleeNode = _Grid.GetNodeByPosition((int)_PlayerFleeNodeLocation.x, (int)_PlayerFleeNodeLocation.y);
		mEnemyFleeNode = _Grid.GetNodeByPosition((int)_EnemyFleeNodeLocation.x, (int)_EnemyFleeNodeLocation.y);
		if ((bool)_Tutorial)
		{
			mPlayerSpawns = new List<UnitSelection>();
			RealmLevel tutorialData = LevelManager.pInstance.pSTLevelData.TutorialData;
			for (int i = 0; i < tutorialData.Units.Count(); i++)
			{
				UnitSelection unitSelection = new UnitSelection();
				unitSelection._UnitName = tutorialData.Units[i]._Name;
				unitSelection._UnitLevel = tutorialData.Units[i]._Level;
				unitSelection._Weapon = tutorialData.Units[i]._Weapon;
				mPlayerSpawns.Add(unitSelection);
			}
		}
		else
		{
			mPlayerSpawns = LevelManager.pInstance.GetSquadList();
		}
		mPlayerSpawnGrids = new List<UnitSpawn>(GetSceneData().GetPlayerSpawns(mPlayerSpawns.Count));
		mEnemySpawnGrids = new List<UnitSpawn>(GetSceneData().GetEnemySpawns());
		mInanimateSpawnGrids = new List<UnitSpawn>(GetSceneData().GetInanimateSpawns());
		for (int j = 0; j < mPlayerSpawns.Count; j++)
		{
			CharacterData character = CharacterDatabase.pInstance.GetCharacter(mPlayerSpawns[j]._UnitName, mPlayerSpawns[j]._RaisedPetID);
			if (character == null)
			{
				continue;
			}
			CharacterData characterData = new CharacterData(character);
			characterData.pRaisedPetID = mPlayerSpawns[j]._RaisedPetID;
			characterData._Team = Character.Team.PLAYER;
			characterData.pSpawnOrder = j;
			UnitSpawn unitData = GetUnitData(characterData, mPlayerSpawnGrids);
			if (characterData.pIsAvatar())
			{
				characterData.pLevel = UserRankData.pInstance.RankID;
			}
			else
			{
				RaisedPetData byID = RaisedPetData.GetByID(characterData.pRaisedPetID);
				characterData.pLevel = ((byID == null) ? mPlayerSpawns[j]._UnitLevel : CharacterDatabase.pInstance.GetLevel(byID));
				if (!LevelManager.pRestartLevel && byID != null && byID.PetTypeID > 0)
				{
					SanctuaryData.pInstance.UpdateActionMeterData(byID, PetActions.SQUADTACTICS, SanctuaryPetMeterType.ENERGY, 1f);
				}
			}
			if (!string.IsNullOrEmpty(mPlayerSpawns[j]._Weapon))
			{
				characterData.pWeaponOverridden = true;
				characterData._WeaponData = WeaponDatabase.pInstance.GetWeaponData(mPlayerSpawns[j]._Weapon);
			}
			else if (characterData.pIsAvatar())
			{
				CharacterDatabase.pInstance.ReInitAvatarWeapon();
				characterData._WeaponData = CharacterDatabase.pInstance.GetAvatarWeaponData();
				AvatarDataPart avatarDataPart = AvatarData.pInstanceInfo.FindPart(AvatarData.pPartSettings.AVATAR_PART_HAND_PROP_RIGHT);
				if (avatarDataPart == null)
				{
					avatarDataPart = AvatarData.pInstanceInfo.FindPart("DEFAULT_" + AvatarData.pPartSettings.AVATAR_PART_HAND_PROP_RIGHT);
				}
				if (avatarDataPart != null && avatarDataPart.UserInventoryId.HasValue && avatarDataPart.UserInventoryId.Value >= 0 && avatarDataPart.Geometries != null && avatarDataPart.Geometries.Length != 0 && !string.IsNullOrEmpty(avatarDataPart.Geometries[0]))
				{
					characterData._WeaponData._MeshPrefab = "RS_SHARED/" + avatarDataPart.Geometries[0];
				}
			}
			List<object> list = new List<object>();
			list.Add(characterData);
			list.Add(unitData);
			mDataLoaded++;
			RsResourceManager.LoadAssetFromBundle("RS_DATA/" + characterData._PrefabName + ".unity3d/" + characterData._PrefabName, OnCharacterBundleLoaded, typeof(GameObject), inDontDestroy: false, list);
		}
		for (int k = 0; k < mEnemySpawnGrids.Count; k++)
		{
			CharacterData characterData2 = ((!(mEnemySpawnGrids[k]._Variant != "")) ? CharacterDatabase.pInstance.GetCharacter(mEnemySpawnGrids[k]._UnitName) : CharacterDatabase.pInstance.GetCharacter(mEnemySpawnGrids[k]._UnitName, 0, mEnemySpawnGrids[k]._Variant));
			if (characterData2 != null)
			{
				CharacterData characterData3 = new CharacterData(characterData2);
				characterData3._Team = Character.Team.ENEMY;
				characterData3.pLevel = mEnemySpawnGrids[k]._Level;
				List<object> list2 = new List<object>();
				list2.Add(characterData3);
				list2.Add(mEnemySpawnGrids[k]);
				mEnemySpawnGrids[k].pIsUsed = true;
				mDataLoaded++;
				RsResourceManager.LoadAssetFromBundle("RS_DATA/" + characterData3._PrefabName + ".unity3d/" + characterData3._PrefabName, OnCharacterBundleLoaded, typeof(GameObject), inDontDestroy: false, list2);
			}
		}
		for (int l = 0; l < mInanimateSpawnGrids.Count; l++)
		{
			CharacterData character2 = CharacterDatabase.pInstance.GetCharacter(mInanimateSpawnGrids[l]._UnitName);
			if (character2 != null)
			{
				CharacterData characterData4 = new CharacterData(character2);
				characterData4._Team = Character.Team.INANIMATE;
				characterData4.pLevel = mInanimateSpawnGrids[l]._Level;
				List<object> list3 = new List<object>();
				list3.Add(characterData4);
				list3.Add(mInanimateSpawnGrids[l]);
				mInanimateSpawnGrids[l].pIsUsed = true;
				mDataLoaded++;
				RsResourceManager.LoadAssetFromBundle("RS_DATA/" + characterData4._PrefabName + ".unity3d/" + characterData4._PrefabName, OnCharacterBundleLoaded, typeof(GameObject), inDontDestroy: false, list3);
			}
		}
		while (mDataLoaded != 0)
		{
			yield return new WaitForEndOfFrame();
		}
		if (_Tutorial != null)
		{
			_Tutorial.Init();
		}
		foreach (Character activeUnit in _ActiveUnits)
		{
			PlaceCharacter(activeUnit, _Grid.GetNodeByPosition((int)activeUnit.pStartingPosition.x, (int)activeUnit.pStartingPosition.y));
		}
		AvatarData.InstanceInfo avatarInstanceInfo = SetUnitTeams();
		mPlayerUnits = mPlayerUnits.OrderBy((Character c) => c.pCharacterData.pSpawnOrder).ToList();
		mAllPlayerUnits.AddRange(mPlayerUnits);
		mAllEnemyUnits.AddRange(mEnemyUnits);
		mAllInanimateUnits.AddRange(mInanimateUnits);
		if (avatarInstanceInfo != null)
		{
			while (!avatarInstanceInfo.pIsReady)
			{
				yield return null;
			}
		}
		foreach (Character activeUnit2 in _ActiveUnits)
		{
			activeUnit2.Initialize(_Grid);
			if (!pUnitsAnalyticInfo.ContainsKey(activeUnit2.name))
			{
				pUnitsAnalyticInfo.Add(activeUnit2.name, new UnitData(activeUnit2));
			}
			if ((bool)_Tutorial)
			{
				activeUnit2.pCanProcessClick = false;
			}
		}
		if (_HUD != null)
		{
			_HUD.Initialize(mPlayerUnits);
		}
		Character character3 = mPlayerUnits.Find((Character item) => item.pCharacterData.pIsAvatar());
		if (character3 != null)
		{
			SelectCharacter(character3);
		}
		else
		{
			SelectCharacter(mPlayerUnits[0]);
		}
		CameraMovement.pInstance.UpdateCameraFocus(character3.transform.position);
		RsResourceManager.DestroyLoadScreen();
		UICursorManager.pVisibility = true;
		if ((bool)_GamePlayBGMusic)
		{
			SnChannel.Play(_GamePlayBGMusic, "STMusic_Pool", inForce: true);
		}
		if (_AmbientNoise != null)
		{
			SnChannel.Play(_AmbientNoise, "STAmbient_Pool", inForce: true);
		}
		if (_GameStartSFX != null)
		{
			SnChannel.Play(_GameStartSFX, "STSFX_Pool", inForce: true);
		}
		if ((bool)_Tutorial)
		{
			_Tutorial.ShowTutorial();
		}
		else if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
		}
		_HUD.SetObjectives();
		_HUD.UpdateTurnCounter(mPlayerTurnCounter);
		if (!_Tutorial)
		{
			_HUD.ShowObjectives(_ObjectivesCloseAfterDelay);
		}
		SetGameState(GameState.PLAYER);
		SetTurnState(TurnState.INPUT);
		LevelManager.pRestartLevel = false;
	}

	private void OnCharacterBundleLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			List<object> list = (List<object>)inUserData;
			CharacterData characterData = (CharacterData)list[0];
			obj.name = characterData._NameID;
			Character component = obj.GetComponent<Character>();
			component.pCharacterData = characterData;
			UnitSpawn unitSpawn = (UnitSpawn)list[1];
			component.pStartingPosition = unitSpawn._Grid;
			component.pStartingRotation = unitSpawn._Rotation;
			component.pBlockMoveNodes = unitSpawn._BlocksMoveNodes;
			component.pBlockRangedAttacks = unitSpawn._BlocksRangedAttacks;
			if (unitSpawn._MaximumAlertTurn > 0)
			{
				component._MaximumAlertTurn = unitSpawn._MaximumAlertTurn;
			}
			if (unitSpawn._SpawnTurnCondition == -1 && unitSpawn._ObjectiveCondition == -1)
			{
				_ActiveUnits.Add(component);
			}
			else
			{
				component.pSpawnTurnCondition = unitSpawn._SpawnTurnCondition;
				component.pObjectiveCondition = unitSpawn._ObjectiveCondition;
				_ConditionalUnits.Add(component);
				if (component.pCharacterData._Team == Character.Team.ENEMY)
				{
					mAllEnemyUnits.Add(component);
				}
				if (component.pCharacterData._Team == Character.Team.INANIMATE)
				{
					mAllInanimateUnits.Add(component);
				}
				component.transform.position = new Vector3(5000f, 5000f, 5000f);
			}
			if (characterData._DefaultProps != null)
			{
				DefaultPropData[] defaultProps = characterData._DefaultProps;
				foreach (DefaultPropData defaultPropData in defaultProps)
				{
					mDataLoaded++;
					List<object> list2 = new List<object>();
					list2.Add(defaultPropData);
					list2.Add(component.transform);
					RsResourceManager.LoadAssetFromBundle(defaultPropData._Prefab, OnPropBundleLoaded, typeof(GameObject), inDontDestroy: false, list2);
				}
			}
			if (characterData.pIsAvatar() && !string.IsNullOrEmpty(component.pCharacterData._WeaponData._MeshPrefab))
			{
				mDataLoaded++;
				RsResourceManager.LoadAssetFromBundle(component.pCharacterData._WeaponData._MeshPrefab, OnWeaponMeshBundleLoaded, typeof(GameObject), inDontDestroy: false, component);
			}
			RsResourceManager.LoadAssetFromBundle(_WeaponBundlePath + characterData._WeaponData._PrefabName, OnWeaponBundleLoaded, typeof(GameObject), inDontDestroy: false, component);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.Log("Bundle Yet to be Loaded");
			break;
		}
	}

	private void OnWeaponBundleLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = obj.name.Replace("(Clone)", "");
			Character character = (Character)inUserData;
			Weapon component = obj.GetComponent<Weapon>();
			character.pCharacterData._Weapon = component;
			obj.transform.parent = character.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.name = character.pCharacterData._WeaponData._Name;
			mDataLoaded--;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.Log("Weapon Yet to be Loaded");
			break;
		}
	}

	private void OnPropBundleLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			List<object> list = (List<object>)inUserData;
			if (list != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
				DefaultPropData defaultPropData = (DefaultPropData)list[0];
				Transform bone = ((Transform)list[1]).Find(defaultPropData._BonePath);
				AttachPropToBone(defaultPropData._PartType, gameObject.transform, bone, defaultPropData._Position, defaultPropData._Rotation, defaultPropData._Scale);
			}
			mDataLoaded--;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.Log("Weapon Yet to be Loaded");
			break;
		}
	}

	private void OnWeaponMeshBundleLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			Transform transform = ((Character)inUserData).transform;
			if (transform != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
				string parentBone = AvatarData.GetParentBone(AvatarData.pPartSettings.AVATAR_PART_HAND_PROP_RIGHT);
				Transform bone = transform.root.Find(parentBone);
				AttachPropToBone(AvatarData.pPartSettings.AVATAR_PART_HAND_PROP_RIGHT, gameObject.transform, bone, Vector3.zero, Vector3.zero, Vector3.one);
			}
			mDataLoaded--;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.Log("Weapon Yet to be Loaded");
			break;
		}
	}

	private void AttachPropToBone(string partType, Transform prop, Transform bone, Vector3 pos, Vector3 rot, Vector3 scale)
	{
		if (!string.IsNullOrEmpty(partType))
		{
			GameObject obj = new GameObject(partType);
			obj.transform.parent = bone;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localEulerAngles = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			bone = obj.transform;
		}
		prop.transform.parent = bone;
		prop.transform.localPosition = pos;
		prop.transform.localEulerAngles = rot;
		prop.transform.localScale = scale;
	}

	private int GetRandNumber(int count)
	{
		return UnityEngine.Random.Range(0, count);
	}

	private UnitSpawn GetUnitData(CharacterData character, List<UnitSpawn> spawns)
	{
		UnitSpawn unitSpawn = spawns.Find((UnitSpawn item) => !item.pIsUsed && item._UnitName == character._NameID);
		if (unitSpawn == null)
		{
			unitSpawn = spawns.Find((UnitSpawn item) => !item.pIsUsed);
		}
		if (unitSpawn != null)
		{
			unitSpawn.pIsUsed = true;
		}
		return unitSpawn;
	}

	public void UpdateTimeScale()
	{
		mTimeScaleIndex++;
		if (mTimeScaleIndex >= _TimeScaleSpeeds.Length)
		{
			mTimeScaleIndex = 0;
		}
		Time.timeScale = _TimeScaleSpeeds[mTimeScaleIndex];
	}

	public void ResetTimeScale()
	{
		Time.timeScale = 1f;
		mTimeScaleIndex = 0;
	}

	private void Update()
	{
		if (_GameState == GameState.GAMEOVER)
		{
			return;
		}
		pGamePlayTime += Time.deltaTime;
		if (_TurnState != TurnState.INPUT || EventSystem.current.IsPointerOverGameObject())
		{
			return;
		}
		if (Input.GetMouseButtonDown(0))
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo, float.PositiveInfinity, _GridLayerMask))
			{
				Node nodeFromHitPoint = _Grid.GetNodeFromHitPoint(hitInfo.point);
				if (nodeFromHitPoint != null)
				{
					ProcessMouseDown(nodeFromHitPoint);
				}
			}
		}
		else if (Input.GetMouseButtonUp(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (!CameraMovement.pInstance.pAllowDrag)
			{
				CameraMovement.pInstance.pAllowDrag = true;
			}
			if (Physics.Raycast(ray, out var hitInfo2, float.PositiveInfinity, _GridLayerMask))
			{
				Node nodeFromHitPoint2 = _Grid.GetNodeFromHitPoint(hitInfo2.point);
				ProcessMouseUp(nodeFromHitPoint2);
			}
		}
		else if (Input.GetMouseButton(0) && mStartedMove)
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo3, float.PositiveInfinity, _GridLayerMask))
			{
				Node nodeFromHitPoint3 = _Grid.GetNodeFromHitPoint(hitInfo3.point);
				ProcessMouseHold(nodeFromHitPoint3);
				CameraMovement.pInstance.pAllowDrag = false;
			}
			else
			{
				mLastSelectedNode = null;
				UpdatePlayerFeedback(_SelectedCharacter._CurrentNode, offGrid: true);
			}
		}
	}

	public void SetTurnState(TurnState newState)
	{
		if (newState != TurnState.INPUT || _TurnState != TurnState.WAITINGTOEND)
		{
			_TurnState = newState;
		}
	}

	public void SetGameState(GameState newState)
	{
		_GameState = newState;
	}

	private void ProcessMouseDown(Node selectedNode)
	{
		if (Singleton<UIManager>.pInstance.GetGlobalMouseOverItem(0) != null)
		{
			return;
		}
		mLastSelectedNode = null;
		if (selectedNode._CharacterOnNode != null)
		{
			if (!selectedNode._CharacterOnNode.pCanProcessClick)
			{
				return;
			}
			if (selectedNode._CharacterOnNode._SelectSFX != null && selectedNode._CharacterOnNode._SelectSFX.Length != 0)
			{
				int num = UnityEngine.Random.Range(0, selectedNode._CharacterOnNode._SelectSFX.Length);
				SnChannel.Play(selectedNode._CharacterOnNode._SelectSFX[num], "STSFX_Pool", inForce: true);
			}
			if (selectedNode._CharacterOnNode.pCharacterData._Team == Character.Team.PLAYER)
			{
				if (selectedNode._CharacterOnNode != _SelectedCharacter)
				{
					SelectCharacter(selectedNode._CharacterOnNode);
				}
				mStartedMove = true;
				mSelfCastTimerStart = Time.time;
			}
			else if (_HUD != null)
			{
				_HUD.pEnemyInfoUI.UpdateEnemyDetails(selectedNode._CharacterOnNode, active: true);
				_HUD.UpdateElementCounter(_SelectedCharacter.pCharacterData._WeaponData._ElementType, selectedNode._CharacterOnNode.pCharacterData._WeaponData._ElementType, reset: false);
			}
		}
		else if (_HUD != null)
		{
			_HUD.pEnemyInfoUI.UpdateEnemyDetails(null, active: false);
			_HUD.UpdateElementCounter(_SelectedCharacter.pCharacterData._WeaponData._ElementType, ElementType.NONE, reset: true);
		}
	}

	private void ProcessMouseHold(Node selectedNode)
	{
		if (Singleton<UIManager>.pInstance.GetGlobalMouseOverItem(0) != null)
		{
			return;
		}
		if (_Tutorial != null && _Tutorial._TargetNode != null && _Tutorial._TargetNode != selectedNode)
		{
			Path path = _Grid.GetPath(_SelectedCharacter._CurrentNode, _Tutorial._TargetNode, _SelectedCharacter);
			if (path == null || !path._Nodes.Contains(selectedNode))
			{
				return;
			}
		}
		if (selectedNode._CharacterOnNode != null)
		{
			if (_SelectedCharacter == selectedNode._CharacterOnNode)
			{
				if (Time.time <= mSelfCastTimerStart + _SelfCastTimer)
				{
					return;
				}
			}
			else if (selectedNode._CharacterOnNode.pCharacterData._Team == Character.Team.ENEMY && _HUD != null)
			{
				_HUD.pEnemyInfoUI.UpdateEnemyDetails(selectedNode._CharacterOnNode, active: true);
				_HUD.UpdateElementCounter(_SelectedCharacter.pCharacterData._WeaponData._ElementType, selectedNode._CharacterOnNode.pCharacterData._WeaponData._ElementType, reset: false);
			}
		}
		if (selectedNode != mLastSelectedNode)
		{
			mLastSelectedNode = selectedNode;
			if (_SelectedCharacter != null)
			{
				UpdatePlayerFeedback(selectedNode);
			}
		}
	}

	private void OnDestroy()
	{
		ResetTimeScale();
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
	}

	private void ProcessMouseUp(Node selectedNode)
	{
		if (Singleton<UIManager>.pInstance.GetGlobalMouseOverItem(0) != null)
		{
			return;
		}
		if (_Tutorial != null && _Tutorial._TargetNode != null && _Tutorial._TargetNode != selectedNode)
		{
			ShowCharacterMovementRange(active: true);
			mStartedMove = false;
		}
		else
		{
			if (!mStartedMove)
			{
				return;
			}
			mStartedMove = false;
			if (selectedNode._CharacterOnNode == null && _SelectedCharacter.IsValidMove(selectedNode) && _SelectedCharacter.CanMove())
			{
				ShowCharacterMovementRange(active: false);
				StartCoroutine(_SelectedCharacter.DoMovement(selectedNode));
			}
			else if (selectedNode._CharacterOnNode != null)
			{
				if (!_SelectedCharacter.CanUseAbility() || _SelectedCharacter.pState == Character.State.MOVING || (_SelectedCharacter == selectedNode._CharacterOnNode && Time.time <= mSelfCastTimerStart + _SelfCastTimer))
				{
					return;
				}
				if (_SelectedCharacter.CanUseAbility(selectedNode._CharacterOnNode))
				{
					if (selectedNode._CharacterOnNode.pIsIncapacitated && _SelectedCharacter.pCurrentAbility._InfluencingStat != Stat.HEALINGPOWER && _SelectedCharacter.pCharacterData._Team != Character.Team.INANIMATE)
					{
						return;
					}
					if (_SelectedCharacter.HasValidAbility(selectedNode))
					{
						if (_HUD != null)
						{
							_HUD.SetCharactersMenuState(KAUIState.DISABLED);
						}
						ClearAllCharacterTips();
						ShowCharacterMovementRange(active: true);
						if (_Tutorial != null && _Tutorial._TargetNode != null && _Tutorial._TargetNode == selectedNode && _Tutorial._ForceDisableMove)
						{
							_Tutorial.TutorialManagerAsyncMessage("ForceStopMove");
							return;
						}
						StartCoroutine(_SelectedCharacter.UseAbility(selectedNode._CharacterOnNode));
					}
					else if (_SelectedCharacter.CanMove() && _SelectedCharacter.HasValidMovePlusAbility(selectedNode))
					{
						if (_HUD != null)
						{
							_HUD.SetCharactersMenuState(KAUIState.DISABLED);
						}
						ClearAllCharacterTips();
						ShowCharacterMovementRange(active: false);
						StartCoroutine(_SelectedCharacter.DoMovePlusAbility(selectedNode._CharacterOnNode));
					}
				}
			}
			if (_Tutorial != null && _Tutorial._TargetNode != null && _Tutorial._DirectionArrow != null)
			{
				_Tutorial._DirectionArrow.SetVisibility(inVisible: false);
			}
		}
	}

	private AvatarData.InstanceInfo SetUnitTeams()
	{
		AvatarData.InstanceInfo instanceInfo = null;
		foreach (Character activeUnit in _ActiveUnits)
		{
			if (activeUnit.pCharacterData._Team == Character.Team.PLAYER)
			{
				mPlayerUnits.Add(activeUnit);
				if (activeUnit.tag == "Player")
				{
					instanceInfo = AvatarData.ApplyCurrent(activeUnit.gameObject, new string[1] { AvatarData.pPartSettings.AVATAR_PART_HAND_PROP_RIGHT }, inIncludeFilter: false, new string[1] { AvatarData.pPartSettings.AVATAR_PART_BACK });
					if (instanceInfo != null && instanceInfo.CheckAttributeSet(AvatarData.pPartSettings.AVATAR_PART_HAND, "ToggleWings"))
					{
						Renderer partRenderer = instanceInfo.GetPartRenderer(activeUnit.transform, AvatarData.pPartSettings.AVATAR_PART_WING);
						if (partRenderer != null)
						{
							partRenderer.enabled = false;
						}
					}
				}
				else if (activeUnit.pCharacterData.pRaisedPetID > 0)
				{
					new ApplyPetCustomization().InitPetCustomization(RaisedPetData.GetByID(activeUnit.pCharacterData.pRaisedPetID), activeUnit.gameObject);
				}
			}
			else if (activeUnit.pCharacterData._Team == Character.Team.ENEMY)
			{
				mEnemyUnits.Add(activeUnit);
			}
			else if (activeUnit.pCharacterData._Team == Character.Team.INANIMATE)
			{
				mInanimateUnits.Add(activeUnit);
			}
		}
		return instanceInfo;
	}

	private IEnumerator SpawnEnemy(Character enemy)
	{
		enemy.gameObject.SetActive(value: false);
		Node nodeByPosition = _Grid.GetNodeByPosition((int)enemy.pStartingPosition.x, (int)enemy.pStartingPosition.y);
		if (nodeByPosition != null && nodeByPosition._CharacterOnNode == null && !nodeByPosition._Blocker)
		{
			PlaceCharacter(enemy, nodeByPosition);
		}
		else
		{
			Node node = nodeByPosition._Neighbors.Find((Node item) => item._CharacterOnNode == null && !item._Blocker);
			if (node == null)
			{
				node = nodeByPosition._Neighbors.Find((Node item) => item._Neighbors.Find((Node iter) => iter._CharacterOnNode == null && !item._Blocker));
			}
			if (node != null)
			{
				PlaceCharacter(enemy, node);
			}
		}
		enemy.Initialize(_Grid);
		if (!pUnitsAnalyticInfo.ContainsKey(enemy.name))
		{
			pUnitsAnalyticInfo.Add(enemy.name, new UnitData(enemy));
		}
		CameraMovement.pInstance.UpdateCameraFocus(enemy.transform.position, isForceStopAllowed: true);
		yield return new WaitForSeconds(_ParticleSpawnDelay);
		GameUtilities.PlayAt(enemy.transform.position, _BeamParticle);
		yield return new WaitForSeconds(_EnemySpawnDelay);
		_ActiveUnits.Add(enemy);
		mEnemyUnits.Add(enemy);
		enemy.gameObject.SetActive(value: true);
		enemy.SetAlertStatus();
		enemy._TurnPriority = Character.TurnPriority.MOVE;
	}

	private void PlaceCharacter(Character character, Node node)
	{
		character.transform.position = node._WorldPosition;
		character.UpdateCurrentNode(node);
		character.transform.eulerAngles = character.pStartingRotation;
	}

	public void SelectCharacter(Character character)
	{
		if (_HUD.GetState() == KAUIState.DISABLED)
		{
			return;
		}
		if (_SelectedCharacter != null)
		{
			_SelectedCharacter.DeselectCharacter();
		}
		_SelectedCharacter = character;
		character.SetAsActiveCharacter();
		ShowCharacterMovementRange(active: true);
		UpdateCharacterSelectionTip();
		UpdatePlayerFeedback(_SelectedCharacter._CurrentNode, offGrid: true);
		ShowMoveAbilityTips();
		if (_HUD != null)
		{
			_HUD.pCharacterInfoUI.UpdatePlayersInfoDisplay(character, updateAll: true);
			if (_HUD.pEnemyInfoUI.pSelectedEnemy != null && !_HUD.pEnemyInfoUI.pSelectedEnemy.pIsDead)
			{
				_HUD.UpdateElementCounter(_SelectedCharacter.pCharacterData._WeaponData._ElementType, _HUD.pEnemyInfoUI.pSelectedEnemy.pCharacterData._WeaponData._ElementType, reset: false);
			}
		}
		if (_Tutorial != null)
		{
			_Tutorial.TutorialManagerAsyncMessage(_SelectedCharacter.name);
		}
	}

	private void ShowMoveAbilityTips()
	{
		if ((_SelectedCharacter.pHasMoveAction && _SelectedCharacter.pHasAbilityAction) || (!_ShowNoMovementTip && !_ShowNoAbilityTip) || _SelectedCharacter.pIsIncapacitated)
		{
			return;
		}
		string text = string.Empty;
		if (!_SelectedCharacter.pHasAbilityAction && _ShowNoAbilityTip)
		{
			text = _NoAbilityText.GetLocalizedString();
		}
		if (!_SelectedCharacter.pHasMoveAction && _ShowNoMovementTip)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n ";
			}
			text += _NoMoveText.GetLocalizedString();
		}
		UiFloatingTip component = UnityEngine.Object.Instantiate(pInstance._FloatingTextPrefab, _SelectedCharacter.pUiEffects.transform.position, Quaternion.identity).GetComponent<UiFloatingTip>();
		component.transform.position = _SelectedCharacter.pUiEffects.transform.position;
		if (component != null)
		{
			component.Initialize(text, Color.white);
		}
	}

	public void ShowCharacterMovementRange(bool active)
	{
		_Grid.ShowCurrentValidMoves(show: false);
		if (active && _SelectedCharacter.CanMove())
		{
			_Grid.ShowCurrentValidMoves(show: true, _SelectedCharacter, _MovesMaterial);
		}
	}

	public void ShowCharacterAbilityInfo(Node targetNode)
	{
		_Grid.HidePath();
		if (!_SelectedCharacter.CanUseAbility(targetNode._CharacterOnNode) || (targetNode._CharacterOnNode.pIsIncapacitated && _SelectedCharacter.pCurrentAbility._InfluencingStat != Stat.HEALINGPOWER && _SelectedCharacter.pCharacterData._Team != Character.Team.INANIMATE))
		{
			return;
		}
		bool flag = false;
		if (_SelectedCharacter.HasValidAbility(targetNode))
		{
			_Grid.DrawAttackPath(_SelectedCharacter._CurrentNode, targetNode, _AttackPathMaterial);
			_SelectedCharacter.transform.LookAt(targetNode.transform);
			UpdateTargetTips(targetNode._CharacterOnNode);
			flag = true;
		}
		else if (_SelectedCharacter.CanMove())
		{
			Path cheapestAttackPath = _SelectedCharacter.GetCheapestAttackPath(targetNode);
			if (cheapestAttackPath != null)
			{
				_Grid.DrawPath(_SelectedCharacter._CurrentNode, cheapestAttackPath, _AttackPathMaterial);
				Node finalPathNode = cheapestAttackPath.GetFinalPathNode();
				if (finalPathNode != null)
				{
					_Grid.DrawAttackPath(null, targetNode, _AttackPathMaterial);
					_SelectedCharacter.transform.LookAt(targetNode.transform);
					UpdateTargetTips(targetNode._CharacterOnNode, useFinalPathNode: true);
					UpdateAlertTips(finalPathNode);
					flag = true;
				}
			}
		}
		if (flag && _ValidAbilityUseSFX != null)
		{
			SnChannel.Play(_ValidAbilityUseSFX, "STSFX_Pool", inForce: true);
		}
	}

	public void UpdateCharacterSelection()
	{
		if (_TurnState == TurnState.WAITINGTOEND)
		{
			return;
		}
		if (_SelectedCharacter.CanMove() || _SelectedCharacter.CanUseAbility())
		{
			UpdatePlayerFeedback(_SelectedCharacter._CurrentNode, offGrid: true);
			ShowCharacterMovementRange(active: true);
			return;
		}
		foreach (Character mPlayerUnit in mPlayerUnits)
		{
			if (mPlayerUnit.CanMove() || mPlayerUnit.CanUseAbility())
			{
				SelectCharacter(mPlayerUnit);
				CameraMovement.pInstance.UpdateCameraFocus(_SelectedCharacter.transform.position, isForceStopAllowed: true);
				return;
			}
		}
		EndPlayerTurn();
	}

	private void UpdatePlayerFeedback(Node node, bool offGrid = false)
	{
		if (offGrid)
		{
			if (_SelectedCharacter.CanUseAbility())
			{
				UpdateRangeTips(node);
			}
			else
			{
				ClearRangeTips();
			}
			ClearTargetTips();
			UpdateAlertTips(_SelectedCharacter._CurrentNode);
			_Grid.HidePath();
		}
		else if (!_SelectedCharacter.CanUseAbility() && !_SelectedCharacter.CanMove())
		{
			ClearRangeTips();
			ClearTargetTips();
		}
		else if (_SelectedCharacter.IsValidMove(node))
		{
			if (node._CharacterOnNode == null)
			{
				ClearTargetTips();
				_Grid.HidePath();
				bool num = _SelectedCharacter.CanUseAbility();
				bool flag = _SelectedCharacter.CanMove();
				if (num)
				{
					if (_SelectedCharacter.CanMove())
					{
						UpdateRangeTips(node);
					}
					else
					{
						UpdateRangeTips(_SelectedCharacter._CurrentNode);
					}
				}
				if (flag)
				{
					UpdateAlertTips(node);
					_Grid.DrawPath(_SelectedCharacter._CurrentNode, _Grid.GetPath(_SelectedCharacter._CurrentNode, node, _SelectedCharacter), _PathMaterial);
					_SelectedCharacter.transform.LookAt(node.transform);
					if (_ValidMoveSFX != null)
					{
						SnChannel.Play(_ValidMoveSFX, "STSFX_Pool", inForce: true);
					}
				}
			}
			else
			{
				UpdateAlertTips(_SelectedCharacter._CurrentNode);
				_Grid.HidePath();
				if (_SelectedCharacter.CanUseAbility() && _SelectedCharacter.pState != Character.State.MOVING)
				{
					UpdateRangeTips(_SelectedCharacter._CurrentNode);
					ClearTargetTips();
					ShowCharacterAbilityInfo(node._CharacterOnNode._CurrentNode);
				}
			}
		}
		else if (node._CharacterOnNode == null)
		{
			ClearTargetTips();
			UpdateAlertTips(_SelectedCharacter._CurrentNode);
			_Grid.HidePath();
			if (_SelectedCharacter.CanUseAbility() && _SelectedCharacter.CanUseAbility(node._CharacterOnNode))
			{
				UpdateRangeTips(_SelectedCharacter._CurrentNode);
			}
		}
		else if (_SelectedCharacter.CanUseAbility() && _SelectedCharacter.CanUseAbility(node._CharacterOnNode) && _SelectedCharacter.pState != Character.State.MOVING)
		{
			ClearTargetTips();
			UpdateAlertTips(_SelectedCharacter._CurrentNode);
			UpdateRangeTips(_SelectedCharacter._CurrentNode);
			ShowCharacterAbilityInfo(node._CharacterOnNode._CurrentNode);
		}
		else
		{
			_Grid.HidePath();
		}
	}

	private void ClearAllCharacterTips()
	{
		ClearRangeTips();
		ClearTargetTips();
	}

	private void UpdateCharacterSelectionTip()
	{
		foreach (Character mPlayerUnit in mPlayerUnits)
		{
			if (mPlayerUnit == _SelectedCharacter)
			{
				mPlayerUnit.pUiGridInfo.ShowSelectionWidget(active: true);
			}
			else
			{
				mPlayerUnit.pUiGridInfo.ShowSelectionWidget(active: false);
			}
		}
	}

	private void ClearTargetTips()
	{
		foreach (Character activeUnit in _ActiveUnits)
		{
			if (!(activeUnit.pUiGridInfo == null))
			{
				activeUnit.pUiGridInfo.ShowTargetWidget(active: false);
			}
		}
	}

	public void UpdateTargetTips(Character targetCharacter, bool useFinalPathNode = false)
	{
		foreach (Character activeUnit in _ActiveUnits)
		{
			if (!(activeUnit.pUiGridInfo == null))
			{
				if (activeUnit == targetCharacter && _SelectedCharacter.CanUseAbility(targetCharacter))
				{
					activeUnit.pUiGridInfo.ShowRangeWidget(active: false);
					activeUnit.pUiGridInfo.ShowTargetWidget(active: true);
				}
				else
				{
					activeUnit.pUiGridInfo.ShowTargetWidget(active: false);
				}
			}
		}
		if (!(_SelectedCharacter.pCurrentAbility is AbilityAoE))
		{
			return;
		}
		foreach (Character item in new List<Character>(((AbilityAoE)_SelectedCharacter.pCurrentAbility).GetValidAoETargets(_SelectedCharacter, targetCharacter, useFinalPathNode)))
		{
			if (!(item.pUiGridInfo == null))
			{
				item.pUiGridInfo.ShowTargetWidget(active: true);
			}
		}
	}

	private void ClearRangeTips()
	{
		foreach (Character activeUnit in _ActiveUnits)
		{
			if (!(activeUnit.pUiGridInfo == null))
			{
				activeUnit.pUiGridInfo.ShowRangeWidget(active: false);
			}
		}
	}

	public void UpdateRangeTips(Node selectedNode)
	{
		foreach (Character activeUnit in _ActiveUnits)
		{
			if (activeUnit.pUiGridInfo == null)
			{
				continue;
			}
			if (_SelectedCharacter.CanUseAbility(activeUnit) && _SelectedCharacter.HasValidAbility(activeUnit._CurrentNode, selectedNode))
			{
				if (activeUnit.pCharacterData._Team == Character.Team.PLAYER)
				{
					if (activeUnit.pIsIncapacitated && _SelectedCharacter.pCurrentAbility._InfluencingStat != Stat.HEALINGPOWER)
					{
						activeUnit.pUiGridInfo.ShowRangeWidget(active: false);
					}
					else
					{
						activeUnit.pUiGridInfo.ShowRangeWidget(active: true);
					}
				}
				else
				{
					activeUnit.pUiGridInfo.ShowRangeWidget(active: true);
				}
			}
			else
			{
				activeUnit.pUiGridInfo.ShowRangeWidget(active: false);
			}
		}
	}

	public void UpdateAlertTips(Node node)
	{
		foreach (Character mEnemyUnit in mEnemyUnits)
		{
			if (!(mEnemyUnit.pUiGridInfo == null) && !mEnemyUnit.pAlerted && mEnemyUnit.pCharacterData._Team != Character.Team.INANIMATE)
			{
				if (mEnemyUnit.CheckIfNodeWithinAlertRange(node))
				{
					mEnemyUnit.pUiGridInfo.ShowAlertWidget(active: true);
				}
				else
				{
					mEnemyUnit.pUiGridInfo.ShowAlertWidget(active: false);
				}
			}
		}
	}

	public void EndPlayerTurn()
	{
		if (_TurnState != TurnState.WAITINGTOEND)
		{
			SetTurnState(TurnState.WAITINGTOEND);
			ClearAllCharacterTips();
			if (_HUD != null)
			{
				_HUD.SetCharactersMenuState(KAUIState.DISABLED);
			}
			StartCoroutine(WaitForPlayerTurnToEnd());
		}
	}

	private IEnumerator WaitForPlayerTurnToEnd()
	{
		_HUD.VisibleWaitingPlayerTurnText(visible: true);
		foreach (Character character in mPlayerUnits)
		{
			while (character.pState == Character.State.MOVING || character.pState == Character.State.ABILITY)
			{
				yield return null;
			}
		}
		_HUD.VisibleWaitingPlayerTurnText(visible: false);
		if (_PlayerTurnEndSFX != null)
		{
			SnChannel.Play(_PlayerTurnEndSFX, "STSFX_Pool", inForce: true);
		}
		mPlayerTurnCounter++;
		_HUD.UpdateTurnCounter(mPlayerTurnCounter);
		UpdateObjectiveStatus(updateTurn: true);
		EndTurn(Character.Team.PLAYER);
	}

	private IEnumerator StartTurn(Character.Team team)
	{
		if (team == Character.Team.PLAYER)
		{
			SetGameState(GameState.PLAYER);
			SetTurnState(TurnState.INITIALIZATION);
			if ((bool)_PlayerTurnBeginSFX)
			{
				SnChannel.Play(_PlayerTurnBeginSFX, "STSFX_Pool", inForce: true);
			}
			for (int num = mPlayerUnits.Count - 1; num >= 0; num--)
			{
				mPlayerUnits[num].BeginTurn();
			}
			if (mPlayerUnits.Count > 0)
			{
				UpdateCharacterSelection();
			}
			if (_HUD != null)
			{
				_HUD.pCharacterInfoUI.UpdatePlayersInfoDisplay(_SelectedCharacter, updateAll: true);
				_HUD.SetCharactersMenuState(KAUIState.INTERACTIVE);
			}
			CameraMovement.pInstance.UpdateCameraFocus(_SelectedCharacter.transform.position, isForceStopAllowed: true, overrideCurrentMove: true);
			SetTurnState(TurnState.INPUT);
			yield break;
		}
		SetGameState(GameState.ENEMY);
		SetTurnState(TurnState.INITIALIZATION);
		mEnemyUnits.Sort((Character i1, Character i2) => -1 * i1.pInitiative.CompareTo(i2.pInitiative));
		yield return StartCoroutine(ConditionalEnemySpawns());
		for (int num2 = mEnemyUnits.Count - 1; num2 >= 0; num2--)
		{
			if (mEnemyUnits[num2].pCharacterData._Team != Character.Team.INANIMATE)
			{
				mEnemyUnits[num2].BeginTurn();
			}
		}
		StartCoroutine(DoEnemyTurn());
	}

	private void EndTurn(Character.Team team)
	{
		if (_GameState == GameState.GAMEOVER)
		{
			return;
		}
		if (team == Character.Team.PLAYER)
		{
			for (int num = mPlayerUnits.Count - 1; num >= 0; num--)
			{
				mPlayerUnits[num].EndTurn();
			}
			StartCoroutine(StartTurn(Character.Team.ENEMY));
			return;
		}
		for (int num2 = mEnemyUnits.Count - 1; num2 >= 0; num2--)
		{
			if (mEnemyUnits[num2].pCharacterData._Team != Character.Team.INANIMATE)
			{
				mEnemyUnits[num2].EndTurn();
			}
		}
		StartCoroutine(StartTurn(Character.Team.PLAYER));
	}

	public void SetAbility(Ability ability)
	{
		_SelectedCharacter.SetAbility(ability);
		UpdateRangeTips(_SelectedCharacter._CurrentNode);
	}

	public void UpdateCharacterStatus(Character character)
	{
		if (_HUD != null)
		{
			_HUD.UpdateStatus(character);
		}
	}

	public void RemoveUnit(Character character)
	{
		_ActiveUnits.Remove(character);
		_ConditionalUnits.Remove(character);
		if (character.pCharacterData._Team == Character.Team.PLAYER)
		{
			mPlayerUnits.Remove(character);
		}
		else
		{
			mEnemyUnits.Remove(character);
		}
		UpdateObjectiveStatus();
		if (mPlayerUnits.Count <= 0 || character.pIsPlayer)
		{
			GameOver(won: false);
		}
	}

	public void CheckPlayerIncapacitation()
	{
		foreach (Character mPlayerUnit in mPlayerUnits)
		{
			if (!mPlayerUnit.pIsIncapacitated)
			{
				return;
			}
		}
		GameOver(won: false);
	}

	private IEnumerator DoEnemyTurn()
	{
		_Grid.ShowCurrentValidMoves(show: false);
		if (_HUD != null)
		{
			_HUD.VisibleEnemyTurnText(visible: true);
			_HUD.VisibleEndTurnBtn(visible: false);
		}
		yield return new WaitForSeconds(_TurnInitializationTimer);
		float turnBeginTime = Time.time;
		for (int i = mEnemyUnits.Count - 1; i >= 0; i--)
		{
			yield return mEnemyUnits[i].TakeAutoTurn();
			if (mEnemyUnits[i].pIsDead)
			{
				RemoveUnit(mEnemyUnits[i]);
			}
			if (_GameState == GameState.GAMEOVER)
			{
				yield break;
			}
		}
		if (Time.time - turnBeginTime < _MinimumEnemyTurnDuration)
		{
			yield return new WaitForSeconds(_MinimumEnemyTurnDuration);
		}
		if (_HUD != null)
		{
			_HUD.VisibleEnemyTurnText(visible: false);
		}
		if (_GameState != GameState.GAMEOVER)
		{
			if (_HUD != null)
			{
				_HUD.VisibleEndTurnBtn(visible: true);
			}
			EndTurn(Character.Team.ENEMY);
		}
	}

	public List<Character> GetTeamCharacters(Character.Team team)
	{
		return team switch
		{
			Character.Team.ENEMY => mEnemyUnits, 
			Character.Team.PLAYER => mPlayerUnits, 
			Character.Team.INANIMATE => mInanimateUnits, 
			_ => null, 
		};
	}

	public void GameOver(bool won)
	{
		if (_GameState == GameState.GAMEOVER)
		{
			return;
		}
		StopAllCoroutines();
		SetGameState(GameState.GAMEOVER);
		AudioClip audioClip = (won ? _GameWinMusic : _GameLoseMusic);
		ResetTimeScale();
		float num = Time.time - mSessionStartTime;
		new Dictionary<string, object>
		{
			{ "SessionTime", num },
			{
				"Level",
				RsResourceManager.pCurrentLevel
			},
			{
				"Result",
				won ? "Won" : "Lost"
			}
		};
		if (audioClip != null)
		{
			SnChannel snChannel = SnChannel.Play(audioClip, "STMusic_Pool", inForce: true);
			if (snChannel != null)
			{
				snChannel.pLoop = false;
			}
		}
		if (won)
		{
			List<AchievementTask> list = new List<AchievementTask>();
			bool flag = false;
			foreach (Character pAllPlayerUnit in pAllPlayerUnits)
			{
				if (!pAllPlayerUnit.DamageTaken)
				{
					if (pAllPlayerUnit.pIsPlayer)
					{
						list.Add(new AchievementTask(LevelManager.pInstance._ProtectVIPAchievementID));
					}
				}
				else
				{
					flag = true;
				}
			}
			if (!flag)
			{
				list.Add(new AchievementTask(LevelManager.pInstance._FlawlessAchievementID));
			}
			UserAchievementTask.Set(list.ToArray());
		}
		if (!(InteractiveTutManager._CurrentActiveTutorialObject == null))
		{
			return;
		}
		if (won)
		{
			if (MissionManager.pInstance != null)
			{
				Task completedTask = MissionManager.pInstance.GetCompletedTask("Game", _GameModuleName, SceneManager.GetActiveScene().name);
				if (completedTask != null)
				{
					List<TaskSetup> setups = completedTask.GetSetups();
					if (completedTask.pData != null && setups != null)
					{
						for (int i = 0; i < setups.Count; i++)
						{
							TaskSetup taskSetup = setups[i];
							if (!string.IsNullOrEmpty(taskSetup.Scene))
							{
								LevelManager.pInstance.pLastLevel = taskSetup.Scene;
								break;
							}
						}
					}
				}
			}
			if (LevelManager.pInstance != null)
			{
				LevelManager.pInstance.UpdateUnlockedLevelData();
			}
		}
		StartCoroutine("ShowEndScreen", won);
	}

	private IEnumerator ShowEndScreen(bool won)
	{
		KAUICursorManager.SetExclusiveLoadingGear(status: true);
		CameraMovement.pInstance.pForceStopCameraMovement = true;
		_HUD.CloseSettingsUi();
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		pGamePlayTime = 0f;
		pUnitsAnalyticInfo.Clear();
		pUsedAbilityInfo.Clear();
		if (won)
		{
			int num = 0;
			foreach (Objective finalObjective in GetFinalObjectives())
			{
				if (finalObjective.pObjectiveStatus != ObjectiveStatus.COMPLETED && !finalObjective._IsMandatory)
				{
					num++;
				}
			}
			mCreditChestsCollected = _CreditChestsAvailable - num;
		}
		pRaisedPetEntityMap = new List<RaisedPetEntityMap>();
		foreach (Character mAllPlayerUnit in mAllPlayerUnits)
		{
			RaisedPetEntityMap raisedPetEntityMap = new RaisedPetEntityMap();
			if (mAllPlayerUnit.pCharacterData.pRaisedPetID > 0)
			{
				raisedPetEntityMap.RaisedPetID = mAllPlayerUnit.pCharacterData.pRaisedPetID;
				raisedPetEntityMap.EntityID = RaisedPetData.GetByID(raisedPetEntityMap.RaisedPetID).EntityID;
				pRaisedPetEntityMap.Add(raisedPetEntityMap);
			}
		}
		yield return new WaitForSeconds(_ResultScreenDelay);
		_HUD.SetVisibility(inVisible: false);
		UICursorManager.pCursorManager.RemoveExclusive();
		UiEndDB.ResultInfo resultInfo = new UiEndDB.ResultInfo();
		resultInfo._RewardType = (won ? LevelRewardType.LevelCompletion : LevelRewardType.LevelFailure);
		resultInfo._UnlockedCreditChests = mCreditChestsCollected;
		resultInfo._LockedChests = _CreditChestsAvailable - mCreditChestsCollected;
		resultInfo._MissedObjectives = GetSceneData().GetMissedObjectives(won);
		_UiSTEndDB.ShowGameResult(resultInfo);
	}

	public void Restart()
	{
		LevelManager.pRestartLevel = true;
		RsResourceManager.LoadLevel(SceneManager.GetActiveScene().name);
	}

	public void LoadMainMenu()
	{
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		RsResourceManager.LoadLevel(_MainMenuLevelName);
	}

	public void CheckEnemyAlertStatus()
	{
		foreach (Character mEnemyUnit in mEnemyUnits)
		{
			if (mEnemyUnit.pAlerted || mEnemyUnit.pCharacterData._Team == Character.Team.INANIMATE)
			{
				continue;
			}
			mEnemyUnit.pAlerted = mEnemyUnit.CheckAlertRange();
			if (mEnemyUnit.pAlerted && pInstance._FloatingTextPrefab != null)
			{
				UiFloatingTip component = UnityEngine.Object.Instantiate(pInstance._FloatingTextPrefab, mEnemyUnit.transform.position + new Vector3(0f, 2.5f, 0f), Quaternion.identity).GetComponent<UiFloatingTip>();
				if (component != null)
				{
					component.Initialize(pInstance._AlertedText.GetLocalizedString(), Color.red);
				}
			}
		}
	}

	public ElementType GetElementCounterType(ElementType element)
	{
		return Settings.pInstance.GetCounteredElement(element);
	}

	public ElementCounterResult GetElementCounterResult(ElementType firstElement, ElementType secondElement)
	{
		if (GetElementCounterType(firstElement) == secondElement)
		{
			return ElementCounterResult.POSITIVE;
		}
		if (GetElementCounterType(secondElement) == firstElement)
		{
			return ElementCounterResult.NEGATIVE;
		}
		return ElementCounterResult.NEUTRAL;
	}

	public void AutoMove(Node targetNode)
	{
		mStartedMove = true;
		ProcessMouseUp(targetNode);
	}

	public Character GetCharacter(string name)
	{
		foreach (Character activeUnit in _ActiveUnits)
		{
			if (activeUnit.pCharacterData._NameID == name)
			{
				return activeUnit;
			}
		}
		return null;
	}

	public List<Character> GetEnemyCharacters(string name)
	{
		List<Character> list = new List<Character>();
		foreach (Character mAllEnemyUnit in mAllEnemyUnits)
		{
			if (mAllEnemyUnit.pCharacterData._NameID == name)
			{
				list.Add(mAllEnemyUnit);
			}
		}
		return list;
	}

	public List<Character> GetInanimateCharacters(string name)
	{
		List<Character> list = new List<Character>();
		foreach (Character mAllInanimateUnit in mAllInanimateUnits)
		{
			if (mAllInanimateUnit.pCharacterData._NameID == name)
			{
				list.Add(mAllInanimateUnit);
			}
		}
		return list;
	}

	public void UpdateObjectiveStatus(bool updateTurn = false)
	{
		if ((bool)_Tutorial)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (Objective finalObjective in GetFinalObjectives())
		{
			Objective objective = ProcessObjective(finalObjective, updateTurn);
			if (objective._IsMandatory)
			{
				if (objective.pObjectiveStatus == ObjectiveStatus.COMPLETED)
				{
					num++;
				}
				if (objective.pObjectiveStatus == ObjectiveStatus.FAILED)
				{
					num2++;
				}
			}
		}
		foreach (Objective finalObjective2 in GetFinalObjectives())
		{
			UpdateHiddenStatus(finalObjective2);
		}
		int count = GetFinalObjectives().FindAll((Objective x) => x.pHiddenStatus == ObjectiveHiddenStatus.HIDDEN_BY_PARENT).Count;
		int count2 = GetFinalObjectives().FindAll((Objective x) => x.pHiddenStatus == ObjectiveHiddenStatus.UNHIDDEN).Count;
		bool flag = false;
		if (count < mObjectiveHiddenByParentCount)
		{
			flag = true;
			_HUD.SetObjectives();
			mObjectiveHiddenByParentCount = count;
		}
		if (!flag)
		{
			_HUD.UpdateObjectivesDisplay();
		}
		if (count2 > mObjectiveUnlockCount)
		{
			_HUD.PlayNewObjectiveAnim();
			mObjectiveUnlockCount = count2;
		}
		if (num2 > 0)
		{
			GameOver(won: false);
		}
		else if (GetSceneData().GetMandatoryObjectivesCount() == num)
		{
			GameOver(won: true);
		}
	}

	private IEnumerator ConditionalEnemySpawns()
	{
		foreach (Character conditionalUnit in _ConditionalUnits)
		{
			if (!_ActiveUnits.Contains(conditionalUnit))
			{
				if (conditionalUnit.pSpawnTurnCondition > 0 && mPlayerTurnCounter > conditionalUnit.pSpawnTurnCondition)
				{
					yield return StartCoroutine(SpawnEnemy(conditionalUnit));
				}
				else if (conditionalUnit.pObjectiveCondition >= 1 && GetFinalObjectives()[conditionalUnit.pObjectiveCondition - 1] != null && GetFinalObjectives()[conditionalUnit.pObjectiveCondition - 1].pObjectiveStatus == ObjectiveStatus.COMPLETED)
				{
					yield return StartCoroutine(SpawnEnemy(conditionalUnit));
				}
			}
		}
	}

	private Objective ProcessObjective(Objective objective, bool updateTurn)
	{
		if (objective.pHiddenStatus == ObjectiveHiddenStatus.HIDDEN_BY_TURNS && updateTurn)
		{
			objective.pNoOfTurnsToUnlock = ((!objective.IsTurnsCompleted()) ? (objective.pNoOfTurnsToUnlock - 1) : 0);
		}
		if (objective.pHiddenStatus == ObjectiveHiddenStatus.UNHIDDEN && (objective.pObjectiveStatus == ObjectiveStatus.INPROGRESS || (objective.SurviveFromStart() && objective.pObjectiveStatus == ObjectiveStatus.COMPLETED)))
		{
			List<Character> list = new List<Character>();
			if (objective._Objective == ObjectiveType.DEFEAT)
			{
				if (objective._UnitList.Count == 0)
				{
					if (objective._Team == Character.Team.ENEMY)
					{
						list.AddRange(mAllEnemyUnits);
					}
					else if (objective._Team == Character.Team.INANIMATE)
					{
						list.AddRange(mAllInanimateUnits);
					}
				}
				else
				{
					foreach (string unit in objective._UnitList)
					{
						if (objective._Team == Character.Team.ENEMY)
						{
							list.AddRange(GetEnemyCharacters(unit));
						}
						else if (objective._Team == Character.Team.INANIMATE)
						{
							list.AddRange(GetInanimateCharacters(unit));
						}
					}
				}
			}
			else if (objective._TeamSlot.Count == 0)
			{
				list.AddRange(mAllPlayerUnits);
			}
			else
			{
				foreach (int item in objective._TeamSlot)
				{
					if (item - 1 < mAllPlayerUnits.Count)
					{
						list.Add(mAllPlayerUnits[item - 1]);
					}
				}
			}
			int count = list.FindAll((Character character) => !character.pIsDead).Count;
			if (objective._Objective == ObjectiveType.DEFEAT)
			{
				if (count == 0)
				{
					objective.pObjectiveStatus = ObjectiveStatus.COMPLETED;
				}
				if (mPlayerTurnCounter > objective._TurnLimit && objective._TurnLimit > 0)
				{
					objective.pObjectiveStatus = ObjectiveStatus.FAILED;
				}
			}
			else if (objective._Objective == ObjectiveType.SURVIVE)
			{
				if (count != list.Count)
				{
					objective.pObjectiveStatus = ObjectiveStatus.FAILED;
				}
				if (!objective.SurviveFromStart() && ((objective._TurnLimit > 0 && mPlayerTurnCounter > objective._TurnLimit) || mEnemyUnits.Count == 0))
				{
					objective.pObjectiveStatus = ObjectiveStatus.COMPLETED;
				}
			}
			else if (objective._Objective == ObjectiveType.VISIT)
			{
				if ((mPlayerTurnCounter > objective._TurnLimit && objective._TurnLimit > 0) || count == 0)
				{
					objective.pObjectiveStatus = ObjectiveStatus.FAILED;
					if (objective.pMarker != null)
					{
						objective.pMarker.SetActive(value: false);
					}
				}
				foreach (Character item2 in list)
				{
					if (item2._CurrentNode._XPosition == (int)objective._Node.x && item2._CurrentNode._YPosition == (int)objective._Node.y)
					{
						objective.pObjectiveStatus = ObjectiveStatus.COMPLETED;
						if (objective.pMarker != null)
						{
							objective.pMarker.SetActive(value: false);
						}
					}
				}
			}
		}
		return objective;
	}

	private void UpdateHiddenStatus(Objective obj)
	{
		if (obj.pObjectiveStatus != 0 && (!obj.SurviveFromStart() || obj.pObjectiveStatus != ObjectiveStatus.COMPLETED))
		{
			return;
		}
		ObjectiveHiddenStatus pHiddenStatus = obj.pHiddenStatus;
		if (obj.pHiddenStatus == ObjectiveHiddenStatus.HIDDEN_BY_PARENT)
		{
			Objective objective = GetFinalObjectives().Find((Objective ob) => ob.pObjectiveId == obj.pParentId);
			if (objective != null)
			{
				obj.pHiddenStatus = ((objective.pObjectiveStatus != ObjectiveStatus.COMPLETED) ? ObjectiveHiddenStatus.HIDDEN_BY_PARENT : ((!obj.IsTurnsCompleted()) ? ObjectiveHiddenStatus.HIDDEN_BY_TURNS : ObjectiveHiddenStatus.UNHIDDEN));
			}
		}
		else if (obj.pHiddenStatus == ObjectiveHiddenStatus.HIDDEN_BY_TURNS)
		{
			obj.pHiddenStatus = ((!obj.IsTurnsCompleted()) ? ObjectiveHiddenStatus.HIDDEN_BY_TURNS : ObjectiveHiddenStatus.UNHIDDEN);
		}
		if (obj.pHiddenStatus == ObjectiveHiddenStatus.UNHIDDEN && pHiddenStatus != 0)
		{
			CreateObjectiveVisitMarker(obj);
		}
	}

	private void CreateObjectiveVisitMarker(Objective objective)
	{
		if (objective._Objective == ObjectiveType.VISIT && objective.pHiddenStatus == ObjectiveHiddenStatus.UNHIDDEN && _ObjectiveMarker != null)
		{
			Node nodeByPosition = _Grid.GetNodeByPosition((int)objective._Node.x, (int)objective._Node.y);
			objective.pMarker = UnityEngine.Object.Instantiate(_ObjectiveMarker, nodeByPosition._WorldPosition, Quaternion.identity);
		}
	}

	public void SetInteractiveHUD(bool enable)
	{
		if (enable && _HUD.GetState() == KAUIState.DISABLED)
		{
			_HUD.SetState(KAUIState.INTERACTIVE);
		}
		else if (!enable && _HUD.GetState() == KAUIState.INTERACTIVE)
		{
			_HUD.SetState(KAUIState.DISABLED);
		}
	}

	public void SelectCharacterAbility(string ability)
	{
		if (string.IsNullOrEmpty(ability) || !(_HUD != null) || !(_HUD.pCharacterInfoUI != null) || !(_HUD.pCharacterInfoUI.pCharacterAbilitiesMenu != null))
		{
			return;
		}
		KAWidget kAWidget = _HUD.pCharacterInfoUI.pCharacterAbilitiesMenu.FindItem(ability);
		if (kAWidget != null)
		{
			CharacterAbilityWidgetData characterAbilityWidgetData = (CharacterAbilityWidgetData)kAWidget.GetUserData();
			if (characterAbilityWidgetData != null)
			{
				_HUD.pCharacterInfoUI.SetSelectedAbilityInfo(characterAbilityWidgetData._Character, characterAbilityWidgetData._Ability);
			}
		}
	}
}
