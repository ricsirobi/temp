using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class Character : MonoBehaviour
{
	public enum Team
	{
		PLAYER,
		ENEMY,
		INANIMATE
	}

	public enum DodgeState
	{
		NOTACTIVE,
		DODGEACTIVATED,
		DODGEHOLD,
		DODGERETURNING
	}

	public enum TurnPriority
	{
		MOVE,
		ABILITY
	}

	public enum ActionPriority
	{
		DISTANCE,
		HEALTH,
		TYPE
	}

	public enum State
	{
		IDLE,
		MOVING,
		ABILITY,
		INCAPACITATED,
		DEAD
	}

	public UiCharacterGridInfo _UiGridInfo;

	public UiCharacterEffects _UiEffects;

	public GameObject _SelectionLights;

	public GameObject _SelectionParticle;

	public State _State;

	public bool _DoesCharacterFlee = true;

	private CharacterData mCharacterData;

	private bool mStunned;

	private bool mIncapacitated;

	private bool mAlerted;

	public GameObject _IncapacitatedParticle;

	public bool _CanProcessClick = true;

	public int _Initiative;

	public float _AlertRange = 3f;

	public int _MaximumAlertTurn = 4;

	public float _PreferredTargetRange = 1f;

	public float _CinematicCameraFinishDelay = 0.5f;

	public float _AbiltyUseDelayTimeAfterAnimation = 0.5f;

	public float _DeathDelayTimer = 2f;

	public bool _HasMoveAction = true;

	public bool _HasAbilityAction = true;

	public GameObject _TakeHitParticle;

	public Transform _ProjectileImpactLocation;

	public string _ProjectileImpactLocationFallback = "Main_Root/Root_J";

	public Transform _ParentProjectileTransform;

	public bool _IsDead;

	public GameObject _DeathParticle;

	public FxAbilityData _DodgeEffect;

	public float _DodgeInitialAnimationTime = 1f;

	public float _DodgeHoldAnimationTime = 2f;

	public float _DodgeDistance = 3f;

	public TurnPriority _TurnPriority = TurnPriority.ABILITY;

	public ActionPriority[] _AbilityPriority;

	public ActionPriority[] _MovePriority;

	public Ability _CurrentAbility;

	public CinematicCameraSetting _CinematicCameraSetting;

	public string[] _AbilityAnimationClipName;

	public Animator _AdditionalAnimator;

	public string _TransparentMaterialPath = "JS Games/Transparent/Unlit Color";

	public Color _TransparentColor = new Color(1f, 1f, 1f, 0.1f);

	private Material mTransparentMaterialOverride;

	private Material[] mCachedMaterials;

	[Header("Node Info")]
	public Node _CurrentNode;

	public List<Node> _CurrentValidMoveNodes;

	public List<Node> _CurrentDisplayMoveNodes;

	public List<Path> _CurrentValidAttackPaths;

	private bool mBlockMoveNodes;

	private bool mBlockRangedAttacks;

	private List<CharacterPathInfo> mCharacterPathInfo = new List<CharacterPathInfo>();

	private List<CharacterPathInfo> mAlliesPathInfo = new List<CharacterPathInfo>();

	private Character mCurrentTarget;

	private Node mCurrentTargetNode;

	[Header("Audio")]
	public AudioClip[] _SelectSFX;

	public AudioClip[] _UnitMoveSFX;

	public AudioClip[] _UnitDieSFX;

	public AudioClip[] _HeroIncapacitatedSFX;

	public AudioClip[] _UnitTakeHitSFX;

	[Header("Ability Skip Chance")]
	public float _AbilitySkipChance = 10f;

	private bool mIsPlayer;

	private List<Ability> mPrioritizedAbilities = new List<Ability>();

	private List<Effect> mActiveStatusEffects = new List<Effect>();

	private Grid mGrid;

	private bool mIsFirstTurnIncapacitated;

	private int mCurrentRevivalCountdown;

	private bool mIsTakingAutoTurn;

	private Animator mAnimator;

	private Dictionary<int, float> mAbilityAnimationData = new Dictionary<int, float>();

	private List<FxAbilityData> mFxData = new List<FxAbilityData>();

	private List<CinematicLayerData> mCinematicLayerData = new List<CinematicLayerData>();

	private float mDodgeAnimationLerpTimer;

	private DodgeState mDodgeState;

	public bool pBlockMoveNodes
	{
		get
		{
			return mBlockMoveNodes;
		}
		set
		{
			mBlockMoveNodes = value;
		}
	}

	public bool pBlockRangedAttacks
	{
		get
		{
			return mBlockRangedAttacks;
		}
		set
		{
			mBlockRangedAttacks = value;
		}
	}

	public int pSpawnTurnCondition { get; set; }

	public int pObjectiveCondition { get; set; }

	public bool pIsPlayer => mIsPlayer;

	public bool DamageTaken { get; set; }

	public bool pIsDead => _IsDead;

	public List<Effect> pActiveStatusEffects => mActiveStatusEffects;

	public int pCurrentRevivalCountdown => mCurrentRevivalCountdown;

	public CharacterData pCharacterData
	{
		get
		{
			return mCharacterData;
		}
		set
		{
			mCharacterData = value;
		}
	}

	public bool pIsStunned => mStunned;

	public bool pAlerted
	{
		get
		{
			return mAlerted;
		}
		set
		{
			mAlerted = value;
		}
	}

	public bool pIsIncapacitated => mIncapacitated;

	public UiCharacterGridInfo pUiGridInfo => _UiGridInfo;

	public UiCharacterEffects pUiEffects => _UiEffects;

	public bool pCanProcessClick
	{
		get
		{
			return _CanProcessClick;
		}
		set
		{
			_CanProcessClick = value;
		}
	}

	public int pInitiative => _Initiative;

	public Ability pCurrentAbility => _CurrentAbility;

	public State pState
	{
		get
		{
			return _State;
		}
		set
		{
			_State = value;
		}
	}

	public Vector2 pStartingPosition { get; set; }

	public Vector3 pStartingRotation { get; set; }

	public bool pHasMoveAction => _HasMoveAction;

	public bool pHasAbilityAction => _HasAbilityAction;

	public List<Ability> pAbilities => pCharacterData._Weapon._Abilities;

	public List<Node> pCurrentDisplayMoveNodes => _CurrentDisplayMoveNodes;

	public void Initialize(Grid grid)
	{
		mAnimator = base.gameObject.GetComponentInChildren<Animator>();
		mGrid = grid;
		if (base.gameObject.tag == "Player")
		{
			mIsPlayer = true;
		}
		if (pCharacterData.pWeaponOverridden)
		{
			pCharacterData._Stats.SetInitialValues(pCharacterData.pLevel, pCharacterData._WeaponData._Stats);
		}
		else if (pIsPlayer)
		{
			pCharacterData._Stats.SetInitialValues(pCharacterData.pLevel, AvatarData.pInstanceInfo.GetPartsCombinedStats());
		}
		else
		{
			pCharacterData._Stats.SetInitialValues(pCharacterData.pLevel);
		}
		if (pCharacterData._WeaponData._AnimData != null)
		{
			mAnimator.runtimeAnimatorController = pCharacterData._WeaponData._AnimData._Controller;
		}
		SetupAbilityData();
		SetCinematicLayerData();
		if (_UiGridInfo != null)
		{
			_UiGridInfo.Initialize(this);
		}
		SetSelectionFX(active: false);
		SetTakeHitParticle(active: false);
		SetIncapacitatedParticle(active: false);
		SetDeathParticle(active: false);
		_HasMoveAction = true;
		_HasAbilityAction = true;
		UpdateCurrentValidMoveNodes();
		_CurrentAbility = pAbilities[0];
		mTransparentMaterialOverride = new Material(Shader.Find(_TransparentMaterialPath));
		mTransparentMaterialOverride.color = _TransparentColor;
		if (!(_ProjectileImpactLocation != null))
		{
			Transform transform = base.transform.GetComponentInChildren<Animator>().transform;
			_ProjectileImpactLocation = transform.Find(_ProjectileImpactLocationFallback);
			if (_ProjectileImpactLocation == null)
			{
				UtDebug.LogWarning("Projectle Impact Location for " + base.gameObject.name + " is null and transform " + _ProjectileImpactLocationFallback + " was not found! Projectiles will hit the ground under this character!");
			}
		}
	}

	public void SetCharacterState(State inState)
	{
		_State = inState;
		switch (inState)
		{
		case State.MOVING:
		case State.ABILITY:
			GameManager.pInstance.SetInteractiveHUD(enable: false);
			break;
		case State.IDLE:
			GameManager.pInstance.SetInteractiveHUD(enable: true);
			break;
		}
	}

	private void SetCinematicLayerData()
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			mCinematicLayerData.Add(new CinematicLayerData(renderer.gameObject, renderer.gameObject.layer));
		}
	}

	private void SetupAbilityData()
	{
		mPrioritizedAbilities.AddRange(pAbilities);
		mPrioritizedAbilities.Sort((Ability a1, Ability a2) => a1._Priority.CompareTo(a2._Priority));
		foreach (Ability pAbility in pAbilities)
		{
			FxAbilityData[] fX = pAbility._FX;
			foreach (FxAbilityData fxAbilityData in fX)
			{
				if (!string.IsNullOrEmpty(fxAbilityData._Name))
				{
					mFxData.Add(fxAbilityData);
					fxAbilityData.Initialize(base.transform);
				}
			}
		}
		if (!string.IsNullOrEmpty(_DodgeEffect._Name))
		{
			mFxData.Add(_DodgeEffect);
			_DodgeEffect.Initialize(base.transform);
		}
		if (pCharacterData._WeaponData._AnimData != null && pCharacterData._WeaponData._AnimData._AbilityAnimationClipName != null)
		{
			int num = 0;
			string[] abilityAnimationClipName = pCharacterData._WeaponData._AnimData._AbilityAnimationClipName;
			foreach (string text in abilityAnimationClipName)
			{
				AnimationClip[] animationClips = mAnimator.runtimeAnimatorController.animationClips;
				foreach (AnimationClip animationClip in animationClips)
				{
					if (animationClip.name == text)
					{
						mAbilityAnimationData.Add(num, animationClip.length);
						num++;
						break;
					}
				}
			}
		}
		else
		{
			if (_AbilityAnimationClipName == null || _AbilityAnimationClipName.Length > pCharacterData._Weapon._Abilities.Count)
			{
				return;
			}
			for (int k = 0; k < _AbilityAnimationClipName.Length; k++)
			{
				AnimationClip[] animationClips = mAnimator.runtimeAnimatorController.animationClips;
				foreach (AnimationClip animationClip2 in animationClips)
				{
					if (animationClip2.name == _AbilityAnimationClipName[k])
					{
						mAbilityAnimationData.Add(k, animationClip2.length);
						break;
					}
				}
			}
		}
	}

	public bool CanMove()
	{
		if (!_HasMoveAction || _IsDead || mIncapacitated)
		{
			return false;
		}
		return true;
	}

	public bool CanUseAbility()
	{
		if (!_HasAbilityAction || _IsDead || mIncapacitated)
		{
			return false;
		}
		return true;
	}

	public bool CanUseAbility(Character target)
	{
		if (target == null)
		{
			return false;
		}
		if ((pCurrentAbility._TargetType == Ability.Team.SAME && pCharacterData._Team == target.pCharacterData._Team) || (pCurrentAbility._TargetType == Ability.Team.OPPOSITE && pCharacterData._Team != target.pCharacterData._Team))
		{
			return true;
		}
		return false;
	}

	public void BeginTurn()
	{
		TickStatusEffects(Effect.TickPhase.BEGIN);
		if (mIncapacitated || mStunned)
		{
			_HasMoveAction = false;
			_HasAbilityAction = false;
			if (mIncapacitated)
			{
				if (mIsFirstTurnIncapacitated)
				{
					mIsFirstTurnIncapacitated = false;
				}
				else
				{
					mCurrentRevivalCountdown--;
				}
				if (mCurrentRevivalCountdown <= 0)
				{
					StartCoroutine(Die());
					return;
				}
			}
		}
		else
		{
			_HasMoveAction = true;
			_HasAbilityAction = true;
			UpdateCurrentValidMoveNodes();
		}
		for (int i = 0; i < pAbilities.Count; i++)
		{
			if (pAbilities[i].pCurrentCooldown > 0)
			{
				pAbilities[i].pCurrentCooldown--;
			}
		}
		_CurrentAbility = pAbilities[0];
		if (pCharacterData._Team == Team.ENEMY && !pAlerted)
		{
			_MaximumAlertTurn--;
			if (_MaximumAlertTurn <= 0)
			{
				SetAlertStatus();
			}
		}
	}

	public void EndTurn()
	{
		TickStatusEffects(Effect.TickPhase.END);
	}

	public void SetAsActiveCharacter()
	{
		if (_UiGridInfo != null)
		{
			_UiGridInfo.ShowSelectionWidget(active: true);
		}
		SetSelectionFX(active: true);
		UpdateCurrentValidMoveNodes();
		SetAbility();
	}

	public void DeselectCharacter()
	{
		SetSelectionFX(active: false);
	}

	private void SetSelectionFX(bool active)
	{
		if (UtPlatform.IsMobile() && _SelectionLights != null)
		{
			UnityEngine.Object.Destroy(_SelectionLights);
		}
		if (_SelectionLights != null)
		{
			_SelectionLights.SetActive(active);
		}
		if (_SelectionParticle != null)
		{
			_SelectionParticle.SetActive(active);
		}
		if (active && _SelectSFX != null && _SelectSFX.Length != 0)
		{
			SnChannel.Play(_SelectSFX[UnityEngine.Random.Range(0, _SelectSFX.Length)], "STSFX_Pool", inForce: true);
		}
	}

	public IEnumerator DoMovement(Node targetNode)
	{
		if (!CanMove())
		{
			yield break;
		}
		if (_UnitMoveSFX != null && _UnitMoveSFX.Length != 0)
		{
			SnChannel.Play(_UnitMoveSFX[UnityEngine.Random.Range(0, _UnitMoveSFX.Length)], "STSFX_Pool", inForce: true);
		}
		if (_State != State.ABILITY)
		{
			SetCharacterState(State.MOVING);
		}
		_HasMoveAction = false;
		Path movementPath = mGrid.GetPath(_CurrentNode, targetNode, this);
		Node startNode = _CurrentNode;
		UpdateCurrentNode(movementPath.GetFinalPathNode());
		if (GameManager.pInstance._HUD != null)
		{
			GameManager.pInstance._HUD.pCharacterInfoUI.UpdatePlayersInfoDisplay(this, updateAll: false);
		}
		if (pCharacterData._Team == Team.ENEMY)
		{
			CameraMovement.pInstance.UpdateCameraFocus(base.transform.position);
			while (CameraMovement.pInstance.pIsCameraMoving)
			{
				yield return null;
			}
			CameraMovement.pInstance.SetFollowTarget(base.transform);
		}
		SetAnimatorMove(active: true);
		foreach (Node node in movementPath._Nodes)
		{
			Vector3 currentPosition = base.transform.position;
			float moveTimer = 0f;
			float rotationTimer = 0f;
			Quaternion currentRotation = base.transform.rotation;
			Quaternion targetRotation = Quaternion.LookRotation(node.transform.position - base.transform.position);
			while (moveTimer < 1f)
			{
				moveTimer += Time.deltaTime / GameManager.pInstance._MovementTimePerNode;
				rotationTimer += Time.deltaTime / GameManager.pInstance._RotationTimePerNode;
				base.transform.position = Vector3.Lerp(currentPosition, node._WorldPosition, moveTimer);
				base.transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, rotationTimer);
				yield return null;
			}
		}
		SetAnimatorMove(active: false);
		if (pCharacterData._Team == Team.PLAYER)
		{
			GameManager.pInstance.CheckEnemyAlertStatus();
		}
		if (_State != State.ABILITY)
		{
			SetCharacterState(State.IDLE);
		}
		if (pCharacterData._Team == Team.PLAYER)
		{
			GameManager.pInstance.UpdateCharacterSelection();
		}
		GameManager.pInstance.UpdateObjectiveStatus();
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			Tutorial component = InteractiveTutManager._CurrentActiveTutorialObject.GetComponent<Tutorial>();
			if (component != null)
			{
				component.TutorialManagerAsyncMessage("Successfull_Move");
			}
		}
		if (!(startNode._CharacterOnNode == null))
		{
			yield break;
		}
		foreach (Character activeUnit in GameManager.pInstance._ActiveUnits)
		{
			if (activeUnit.pCharacterData._Team == Team.INANIMATE && activeUnit._CurrentNode == startNode)
			{
				startNode._CharacterOnNode = activeUnit;
			}
		}
	}

	public void UpdateCurrentNode(Node newNode)
	{
		if (_CurrentNode != null)
		{
			if (pIsDead)
			{
				foreach (Character activeUnit in GameManager.pInstance._ActiveUnits)
				{
					if (activeUnit._CurrentNode == _CurrentNode)
					{
						_CurrentNode._CharacterOnNode = activeUnit;
						return;
					}
				}
			}
			_CurrentNode._CharacterOnNode = null;
			_CurrentNode._Occupied = false;
		}
		if (newNode != null)
		{
			_CurrentNode = newNode;
			_CurrentNode._CharacterOnNode = this;
			_CurrentNode._Occupied = true;
		}
	}

	public bool HasValidAbility(Node targetNode, Node startNode = null)
	{
		if (startNode == null)
		{
			if (!(mGrid.GetDistanceBetweenNodes(_CurrentNode, targetNode) <= _CurrentAbility._Range) || !mGrid.HasLineOfSight(_CurrentNode, targetNode))
			{
				return false;
			}
			return true;
		}
		if (!(mGrid.GetDistanceBetweenNodes(startNode, targetNode) <= _CurrentAbility._Range) || !mGrid.HasLineOfSight(startNode, targetNode))
		{
			return false;
		}
		return true;
	}

	public bool HasValidMovePlusAbility(Node targetNode)
	{
		if (mGrid.GetDistanceBetweenNodes(_CurrentNode, targetNode) > pCharacterData._Stats._Movement.pCurrentValue + _CurrentAbility._Range)
		{
			return false;
		}
		if (GetCheapestAttackPath(targetNode) != null)
		{
			return true;
		}
		return false;
	}

	public IEnumerator DoMovePlusAbility(Character targetCharacter)
	{
		SetCharacterState(State.ABILITY);
		CameraMovement.pInstance.UpdateCameraFocus(base.transform.position);
		while (CameraMovement.pInstance.pIsCameraMoving)
		{
			yield return null;
		}
		CameraMovement.pInstance.SetFollowTarget(base.transform);
		bool hasLineOfSight = GetLineOfSight(targetCharacter);
		float distanceBetweenNodes = float.PositiveInfinity;
		if ((bool)mGrid)
		{
			distanceBetweenNodes = mGrid.GetDistanceBetweenNodes(_CurrentNode, targetCharacter._CurrentNode);
		}
		Node attackNode = GetCheapestAttackPath(targetCharacter._CurrentNode)?.GetFinalPathNode();
		if (distanceBetweenNodes > pCurrentAbility._Range || !hasLineOfSight)
		{
			if (attackNode != null)
			{
				yield return StartCoroutine(DoMovement(attackNode));
			}
			else
			{
				yield return StartCoroutine(DoAutoMove());
			}
		}
		hasLineOfSight = GetLineOfSight(targetCharacter);
		float rotationTimer = 0f;
		Quaternion currentRotation = base.transform.rotation;
		Quaternion targetRotation = Quaternion.LookRotation(targetCharacter._CurrentNode.transform.position - base.transform.position);
		while (rotationTimer < 1f)
		{
			rotationTimer += Time.deltaTime / GameManager.pInstance._RotationTimePerNode;
			base.transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, rotationTimer);
			yield return null;
		}
		base.transform.LookAt(targetCharacter.transform);
		if (attackNode != null || (CanUseAbility(targetCharacter) && hasLineOfSight && distanceBetweenNodes <= pCurrentAbility._Range))
		{
			yield return StartCoroutine(UseAbility(targetCharacter));
		}
		SetCharacterState(State.IDLE);
	}

	private bool GetLineOfSight(Character target)
	{
		return mGrid.HasLineOfSight(_CurrentNode, target._CurrentNode);
	}

	public void SetAbility(Ability ability = null)
	{
		if (ability != null)
		{
			_CurrentAbility = ability;
		}
		else
		{
			if (!(_CurrentAbility == null) && _CurrentAbility.pCurrentCooldown <= 0)
			{
				return;
			}
			for (int i = 0; i < pAbilities.Count; i++)
			{
				if (pAbilities[i].pCurrentCooldown <= 0)
				{
					_CurrentAbility = pAbilities[i];
					break;
				}
			}
		}
	}

	public IEnumerator UseAbility(Character target)
	{
		if (target == null || !CanUseAbility() || !CanUseAbility(target))
		{
			yield break;
		}
		mCurrentTarget = target;
		mCurrentTargetNode = target._CurrentNode;
		_HasAbilityAction = false;
		SetCharacterState(State.ABILITY);
		if (GameManager.pInstance._HUD != null)
		{
			GameManager.pInstance._HUD.pCharacterInfoUI.UpdatePlayersInfoDisplay(this, updateAll: false);
		}
		if (GameManager.pInstance.pUsedAbilityInfo.ContainsKey(_CurrentAbility._Name._Text))
		{
			GameManager.pInstance.pUsedAbilityInfo[_CurrentAbility._Name._Text]++;
		}
		else
		{
			GameManager.pInstance.pUsedAbilityInfo.Add(_CurrentAbility._Name._Text, 1);
		}
		base.transform.LookAt(target.transform);
		string abilityParameter = string.Empty;
		string abilityStateName = string.Empty;
		int key = 0;
		for (int i = 0; i < pAbilities.Count; i++)
		{
			if (_CurrentAbility == pAbilities[i])
			{
				switch (i)
				{
				case 0:
					abilityParameter = "bAbility01";
					abilityStateName = "Ability01";
					break;
				case 1:
					abilityParameter = "bAbility02";
					abilityStateName = "Ability02";
					break;
				case 2:
					abilityParameter = "bAbility03";
					abilityStateName = "Ability03";
					break;
				default:
					abilityParameter = "bAbility04";
					abilityStateName = "Ability04";
					break;
				}
				key = i;
				break;
			}
		}
		if (mAnimator != null)
		{
			if (GameManager.pInstance.pAllowCinematicCamera && mAbilityAnimationData.ContainsKey(key))
			{
				_CinematicCameraSetting._TimeToRotate = mAbilityAnimationData[key] + _CinematicCameraFinishDelay;
				yield return new WaitForSeconds(GameManager.pInstance._AbilitySetupTimer);
				CinematicCamera.pInstance.StartCapture(this, _CinematicCameraSetting);
			}
			mAnimator.SetBool(abilityParameter, value: true);
			if (_AdditionalAnimator != null)
			{
				_AdditionalAnimator.SetBool(abilityParameter, value: true);
			}
			float animDelay = 0f;
			while (!mAnimator.GetCurrentAnimatorStateInfo(0).IsName(abilityStateName))
			{
				animDelay += Time.deltaTime;
				yield return null;
			}
			mAnimator.SetBool(abilityParameter, value: false);
			if (_AdditionalAnimator != null)
			{
				_AdditionalAnimator.SetBool(abilityParameter, value: false);
			}
			if (GameManager.pInstance.pAllowCinematicCamera)
			{
				yield return new WaitForSeconds(_CinematicCameraSetting._TimeToRotate + _AbiltyUseDelayTimeAfterAnimation);
			}
			else
			{
				AnimationEvent animationEvent = Array.Find(mAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.events, (AnimationEvent t) => t.functionName.GetHashCode() == "AbilityProc".GetHashCode());
				if (animationEvent != null)
				{
					yield return new WaitForSeconds(Mathf.Clamp(animationEvent.time - CameraMovement.pInstance._CameraTransitionTime - animDelay, 0f, _CinematicCameraSetting._TimeToRotate + _AbiltyUseDelayTimeAfterAnimation));
				}
				else
				{
					yield return new WaitForEndOfFrame();
				}
			}
		}
		target.StopAllEffectVisuals();
		CameraMovement.pInstance.UpdateCameraFocus(target.transform.position, isForceStopAllowed: true);
		while (CameraMovement.pInstance.pIsCameraMoving)
		{
			yield return null;
		}
		yield return StartCoroutine(_CurrentAbility.Activate(this, target));
		SetCharacterState(State.IDLE);
		if (target._IsDead && CanMove())
		{
			UpdateCurrentValidMoveNodes();
			GameManager.pInstance.ShowCharacterMovementRange(active: true);
		}
		if (pCharacterData._Team == Team.PLAYER)
		{
			if (GameManager.pInstance._HUD != null && GameManager.pInstance._TurnState != GameManager.TurnState.WAITINGTOEND)
			{
				GameManager.pInstance._HUD.SetCharactersMenuState(KAUIState.INTERACTIVE);
			}
			GameManager.pInstance.UpdateCharacterSelection();
		}
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			Tutorial component = InteractiveTutManager._CurrentActiveTutorialObject.GetComponent<Tutorial>();
			if (component != null)
			{
				component.TutorialManagerAsyncMessage("Ability_Used");
			}
		}
	}

	public void TakeStatChange(Stat affectedStat, float amount, EffectName effect = null, bool isCritical = false, bool isCounter = false)
	{
		StStat stat = pCharacterData._Stats.GetStat(affectedStat);
		stat.pCurrentValue += amount;
		switch (affectedStat)
		{
		case Stat.DODGE:
			UserAchievementTask.Set(LevelManager.pInstance._SafetyFirstAchievementID);
			break;
		case Stat.HEALTH:
		{
			if (amount == 0f)
			{
				break;
			}
			if (mIncapacitated)
			{
				if (!(amount > 0f))
				{
					break;
				}
				Revive();
			}
			if (amount > 0f)
			{
				UserAchievementTask.Set(LevelManager.pInstance._FieldMedicAchievementID);
			}
			else
			{
				DamageTaken = true;
				UserAchievementTask.Set(new AchievementTask(LevelManager.pInstance._BringThePainAchievementID, string.Empty, 0, Math.Abs((int)amount)));
			}
			if (_UiGridInfo != null)
			{
				_UiGridInfo.UpdateHealthBar();
			}
			if (GameManager.pInstance.pUnitsAnalyticInfo.ContainsKey(pCharacterData._DisplayNameText._Text))
			{
				GameManager.pInstance.pUnitsAnalyticInfo[pCharacterData._DisplayNameText._Text]._Health = stat.pCurrentValue;
			}
			bool flag = false;
			if (pCharacterData._Stats._Health.pCurrentValue <= 0f)
			{
				if (pCharacterData._Team == Team.PLAYER)
				{
					BeginIncapacitatedCountdown();
					flag = true;
				}
				else if (effect == null)
				{
					StartCoroutine(Die());
				}
			}
			else if (amount < 0f)
			{
				StartCoroutine(AnimatorPlayTakeHit());
			}
			else
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(new GameObject(), base.transform.position, Quaternion.identity, base.transform);
				StEffectFxInfo stEffectFxInfo = new StEffectFxInfo(Settings.pInstance.GetEffectDataByName("HEALING")._FX, gameObject.transform);
				stEffectFxInfo._InFX.PlayFx(base.transform);
				UnityEngine.Object.Destroy(gameObject, stEffectFxInfo._InFX.pDuration);
			}
			if (GameManager.pInstance._HUD != null)
			{
				GameManager.pInstance._HUD.UpdateHealth(this);
			}
			if (amount == 0f || flag)
			{
				break;
			}
			Vector3 position = ((_UiEffects == null) ? (base.transform.position + new Vector3(0f, 2.5f, 0f)) : _UiEffects.transform.position);
			UiFloatingTip component = UnityEngine.Object.Instantiate(GameManager.pInstance._FloatingTextPrefab, position, Quaternion.identity).GetComponent<UiFloatingTip>();
			if (!(component != null))
			{
				break;
			}
			string text = string.Empty;
			if (isCritical)
			{
				text = GameManager.pInstance._CriticalText.GetLocalizedString();
			}
			if (isCounter)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += "\n";
				}
				text += GameManager.pInstance._CounterText.GetLocalizedString();
			}
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n";
			}
			text += Mathf.Round(amount);
			Color color = ((amount >= 0f) ? Color.green : Color.red);
			component.Initialize(text, color);
			break;
		}
		case Stat.MOVEMENT:
			UpdateCurrentValidMoveNodes();
			if (GameManager.pInstance._SelectedCharacter == this)
			{
				GameManager.pInstance.ShowCharacterMovementRange(active: true);
			}
			break;
		}
	}

	public void TakeCrowdControl(CrowdControlEnum type, bool active)
	{
		if (type == CrowdControlEnum.STUN)
		{
			UpdateStun(active);
		}
	}

	private void UpdateStun(bool active)
	{
		mStunned = active;
		SetAnimatorStunned(active);
		if (!active)
		{
			_HasMoveAction = true;
			_HasAbilityAction = true;
			UpdateCurrentValidMoveNodes();
		}
	}

	public void BeginIncapacitatedCountdown()
	{
		if (_HeroIncapacitatedSFX != null && _HeroIncapacitatedSFX.Length != 0)
		{
			SnChannel.Play(_HeroIncapacitatedSFX[UnityEngine.Random.Range(0, _HeroIncapacitatedSFX.Length)], "STSFX_Pool", inForce: true);
		}
		SetIncapacitatedParticle(active: true);
		RemoveAllStatusEffects();
		UiFloatingTip component = UnityEngine.Object.Instantiate(GameManager.pInstance._FloatingTextPrefab, base.transform.position + new Vector3(0f, 2.5f, 0f), Quaternion.identity).GetComponent<UiFloatingTip>();
		if (component != null)
		{
			component.Initialize(GameManager.pInstance._IncapacitatedText.GetLocalizedString(), Color.red);
		}
		mIsFirstTurnIncapacitated = true;
		mIncapacitated = true;
		mCurrentRevivalCountdown = GameManager.pInstance._RevivalTurns;
		SetAnimatorIncapacitated(active: true);
		GameManager.pInstance.CheckPlayerIncapacitation();
	}

	public void Revive()
	{
		mIncapacitated = false;
		SetAnimatorIncapacitated(active: false);
		SetIncapacitatedParticle(active: false);
		_HasMoveAction = true;
		if (GameManager.pInstance.pUnitsAnalyticInfo.ContainsKey(base.name))
		{
			GameManager.pInstance.pUnitsAnalyticInfo[base.name]._NumRevived++;
		}
	}

	public IEnumerator Die()
	{
		if (_UnitDieSFX != null && _UnitDieSFX.Length != 0)
		{
			SnChannel.Play(_UnitDieSFX[UnityEngine.Random.Range(0, _UnitDieSFX.Length)], "STSFX_Pool", 1, inForce: true);
		}
		_IsDead = true;
		GameManager.pInstance.RemoveUnit(this);
		if (_UiEffects != null)
		{
			RemoveAllStatusEffects();
		}
		if (_UiGridInfo != null)
		{
			_UiGridInfo.gameObject.SetActive(value: false);
		}
		SetIncapacitatedParticle(active: false);
		if (_DoesCharacterFlee)
		{
			Path fleePath = ((pCharacterData._Team == Team.PLAYER) ? mGrid.GetPath(_CurrentNode, GameManager.pInstance.pPlayerFleeNode, this, forcePath: true) : mGrid.GetPath(_CurrentNode, GameManager.pInstance.pEnemyFleeNode, this, forcePath: true));
			UpdateCurrentNode(null);
			SetAnimatorTakeHit(active: false);
			SetAnimatorDead(active: false);
			UiFloatingTip component = UnityEngine.Object.Instantiate(GameManager.pInstance._FloatingTextPrefab, base.transform.position + new Vector3(0f, 2.5f, 0f), Quaternion.identity).GetComponent<UiFloatingTip>();
			if (component != null)
			{
				component.Initialize(GameManager.pInstance._FleeText.GetLocalizedString(), Color.green);
			}
			yield return new WaitForSeconds(_DeathDelayTimer);
			SetAnimatorMove(active: true);
			StartCoroutine(DoFlee(fleePath));
		}
		else
		{
			UpdateCurrentNode(null);
			SetAnimatorDead(active: true);
			SetDeathParticle(active: true);
			yield return new WaitForSeconds(_DeathDelayTimer);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			Tutorial component2 = InteractiveTutManager._CurrentActiveTutorialObject.GetComponent<Tutorial>();
			if (component2 != null)
			{
				component2.TutorialManagerAsyncMessage(base.name);
			}
		}
	}

	public IEnumerator DoFlee(Path path)
	{
		if (path == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			yield break;
		}
		SetAnimatorMove(active: true);
		foreach (Node node in path._Nodes)
		{
			Vector3 currentPosition = base.transform.position;
			float moveTimer = 0f;
			float rotationTimer = 0f;
			Quaternion currentRotation = base.transform.rotation;
			Quaternion targetRotation = Quaternion.LookRotation(node.transform.position - base.transform.position);
			while (moveTimer < 1f)
			{
				moveTimer += Time.deltaTime / GameManager.pInstance._MovementTimePerNode;
				rotationTimer += Time.deltaTime / GameManager.pInstance._RotationTimePerNode;
				base.transform.position = Vector3.Lerp(currentPosition, node._WorldPosition, moveTimer);
				base.transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, rotationTimer);
				yield return null;
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void UpdateCurrentValidMoveNodes()
	{
		_CurrentValidMoveNodes.Clear();
		_CurrentDisplayMoveNodes.Clear();
		if (CanMove() && mGrid != null)
		{
			mGrid.UpdateCurrentValidMoves(ref _CurrentValidMoveNodes, _CurrentNode, this);
			GetDisplayMoveNodes();
		}
	}

	private void GetDisplayMoveNodes()
	{
		foreach (Node currentValidMoveNode in _CurrentValidMoveNodes)
		{
			_CurrentDisplayMoveNodes.Add(currentValidMoveNode);
		}
		for (int num = _CurrentValidMoveNodes.Count - 1; num >= 0; num--)
		{
			if (_CurrentValidMoveNodes[num]._CharacterOnNode != null && _CurrentValidMoveNodes[num]._CharacterOnNode.pCharacterData._Team == Team.INANIMATE && _CurrentValidMoveNodes[num]._CharacterOnNode.pBlockMoveNodes)
			{
				_CurrentValidMoveNodes.RemoveAt(num);
			}
			else if (_CurrentValidMoveNodes[num]._CharacterOnNode != null && _CurrentValidMoveNodes[num]._CharacterOnNode.pCharacterData._Team != pCharacterData._Team)
			{
				_CurrentValidMoveNodes.RemoveAt(num);
			}
		}
		foreach (Character activeUnit in GameManager.pInstance._ActiveUnits)
		{
			if (activeUnit.pCharacterData._Team != pCharacterData._Team)
			{
				Path path = mGrid.GetPath(_CurrentNode, activeUnit._CurrentNode, this, forcePath: true);
				if (path != null && path._Cost <= pCharacterData._Stats._Movement.pCurrentValue && !activeUnit.pBlockMoveNodes)
				{
					_CurrentDisplayMoveNodes.Add(activeUnit._CurrentNode);
				}
			}
		}
	}

	private void UpdateCurrentValidAttackPaths(Node targetNode)
	{
		if (mGrid != null)
		{
			_CurrentValidAttackPaths.Clear();
			if (CanMove() && CanUseAbility())
			{
				mGrid.GetCurrentValidAttackPaths(ref _CurrentValidAttackPaths, targetNode, this);
			}
		}
	}

	public Path GetCheapestAttackPath(Node targetNode)
	{
		UpdateCurrentValidAttackPaths(targetNode);
		if (_CurrentValidAttackPaths.Count <= 0)
		{
			return null;
		}
		Path path = _CurrentValidAttackPaths[0];
		foreach (Path currentValidAttackPath in _CurrentValidAttackPaths)
		{
			if ((currentValidAttackPath.GetFinalPathNode()._CharacterOnNode == null || (currentValidAttackPath.GetFinalPathNode()._CharacterOnNode != null && currentValidAttackPath.GetFinalPathNode()._CharacterOnNode.pCharacterData._Team == Team.INANIMATE && !currentValidAttackPath.GetFinalPathNode()._CharacterOnNode.pBlockMoveNodes)) && currentValidAttackPath._Cost < path._Cost)
			{
				path = currentValidAttackPath;
			}
		}
		return path;
	}

	public List<Ability> GetPrioritizedAbilities()
	{
		List<Ability> list = new List<Ability>();
		foreach (Ability pAbility in pAbilities)
		{
			if (pAbility.pCurrentCooldown <= 0)
			{
				list.Add(pAbility);
			}
		}
		list.Sort((Ability p1, Ability p2) => p1._Priority.CompareTo(p2._Priority));
		return list;
	}

	public IEnumerator TakeAutoTurn()
	{
		if (pCharacterData._Team == Team.ENEMY && !pAlerted)
		{
			EndAutoTurn();
			yield break;
		}
		CameraMovement.pInstance.UpdateCameraFocus(base.transform.position, isForceStopAllowed: false, overrideCurrentMove: true);
		while (CameraMovement.pInstance.pIsCameraMoving)
		{
			yield return null;
		}
		SetSelectionFX(active: true);
		if (_IsDead || mStunned)
		{
			EndAutoTurn();
			yield break;
		}
		foreach (Character activeUnit in GameManager.pInstance._ActiveUnits)
		{
			if (activeUnit.pCharacterData._Team != pCharacterData._Team && (bool)activeUnit.mActiveStatusEffects.Find((Effect p) => p is Taunt) && mGrid.GetDistanceBetweenNodes(_CurrentNode, activeUnit._CurrentNode) <= _AlertRange && mGrid.HasLineOfSight(_CurrentNode, activeUnit._CurrentNode))
			{
				_TurnPriority = TurnPriority.MOVE;
			}
		}
		yield return DoMovePlusAbility(GetBestTarget(_TurnPriority));
		EndAutoTurn();
	}

	private IEnumerator DoAutoMove()
	{
		if (!CanMove())
		{
			yield break;
		}
		Character bestTarget = GetBestTarget(TurnPriority.MOVE);
		if (bestTarget == null)
		{
			yield break;
		}
		bool flag = GameManager.pInstance._Grid.HasLineOfSight(_CurrentNode, bestTarget._CurrentNode);
		float distanceBetweenNodes = mGrid.GetDistanceBetweenNodes(_CurrentNode, bestTarget._CurrentNode);
		if (distanceBetweenNodes <= _PreferredTargetRange && flag && distanceBetweenNodes < _CurrentAbility._Range)
		{
			yield break;
		}
		Node node = mGrid.GetCheapestNeighbor(_CurrentNode, bestTarget._CurrentNode, this);
		if (node == null)
		{
			UpdateCurrentValidMoveNodes();
			if (_CurrentValidMoveNodes.Count <= 0)
			{
				yield break;
			}
			float num = mGrid.GetDistanceBetweenNodes(_CurrentValidMoveNodes[0], bestTarget._CurrentNode);
			node = _CurrentValidMoveNodes[0];
			foreach (Node currentValidMoveNode in _CurrentValidMoveNodes)
			{
				float distanceBetweenNodes2 = mGrid.GetDistanceBetweenNodes(currentValidMoveNode, bestTarget._CurrentNode);
				if (distanceBetweenNodes2 < num)
				{
					node = currentValidMoveNode;
					num = distanceBetweenNodes2;
				}
			}
		}
		if (!(node != null))
		{
			yield break;
		}
		Path path = mGrid.GetPath(_CurrentNode, node, this);
		path._Nodes.Reverse();
		foreach (Node node2 in path._Nodes)
		{
			if (node2._GCost <= pCharacterData._Stats._Movement.pCurrentValue && (node2._CharacterOnNode == null || (node2._CharacterOnNode != null && node2._CharacterOnNode.pCharacterData._Team == Team.INANIMATE && !node2._CharacterOnNode.pBlockMoveNodes)))
			{
				yield return StartCoroutine(DoMovement(node2));
				break;
			}
		}
	}

	private void EndAutoTurn()
	{
		SetSelectionFX(active: false);
	}

	public bool CheckAlertRange()
	{
		foreach (Character activeUnit in GameManager.pInstance._ActiveUnits)
		{
			if (activeUnit.pCharacterData._Team != pCharacterData._Team && activeUnit.pCharacterData._Team != Team.INANIMATE && !activeUnit.mActiveStatusEffects.Find((Effect p) => p is Hide) && mGrid.GetDistanceBetweenNodes(_CurrentNode, activeUnit._CurrentNode) <= _AlertRange && mGrid.HasLineOfSight(_CurrentNode, activeUnit._CurrentNode))
			{
				if (_UiGridInfo != null)
				{
					_UiGridInfo.ShowAlertWidget(active: true);
				}
				return true;
			}
		}
		return false;
	}

	public bool CheckIfNodeWithinAlertRange(Node node)
	{
		if (node._CharacterOnNode != null && (bool)node._CharacterOnNode.mActiveStatusEffects.Find((Effect p) => p is Hide))
		{
			return false;
		}
		if (node._CharacterOnNode != null && mGrid.GetDistanceBetweenNodes(_CurrentNode, node) <= _AlertRange && mGrid.HasLineOfSight(_CurrentNode, node))
		{
			return true;
		}
		return false;
	}

	private Character GetBestTarget(TurnPriority turnPriority, Ability ability = null)
	{
		UpdateCharacterPathInfo();
		List<CharacterPathInfo> targets = new List<CharacterPathInfo>();
		if (ability != null && ability._TargetType == Ability.Team.SAME)
		{
			if (ability._InfluencingStat == Stat.HEALINGPOWER)
			{
				mAlliesPathInfo.RemoveAll((CharacterPathInfo item) => item._TargetCharacter.pCharacterData._Stats.GetStatValue(Stat.HEALTH) >= item._TargetCharacter.pCharacterData._Stats._Health._Limits.Max);
			}
			if (mAlliesPathInfo.Count == 0)
			{
				return null;
			}
			targets.AddRange(mAlliesPathInfo);
		}
		else
		{
			targets.AddRange(mCharacterPathInfo);
		}
		targets.RemoveAll((CharacterPathInfo t) => t._TargetCharacter.mActiveStatusEffects.Find((Effect p) => p is Hide));
		targets.RemoveAll((CharacterPathInfo item) => item._TargetCharacter.pCharacterData._Team == Team.INANIMATE);
		if ((bool)GameManager.pInstance._Tutorial)
		{
			return targets.Find((CharacterPathInfo item) => item._TargetCharacter.pCharacterData.pIsAvatar())._TargetCharacter;
		}
		ActionPriority[] array = new ActionPriority[3];
		if (_MovePriority != null && _MovePriority.Length != 0 && turnPriority == TurnPriority.MOVE)
		{
			Array.Copy(_MovePriority, array, _MovePriority.Length);
		}
		if (_AbilityPriority != null && _AbilityPriority.Length != 0 && turnPriority == TurnPriority.ABILITY)
		{
			Array.Copy(_AbilityPriority, array, _AbilityPriority.Length);
		}
		CharacterPathInfo characterPathInfo = targets.Find((CharacterPathInfo item) => item._TargetCharacter.mActiveStatusEffects.Find((Effect p) => p is Taunt));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == ActionPriority.DISTANCE)
			{
				GetBestTargetsByDistance(ref targets, turnPriority);
			}
			else if (array[i] == ActionPriority.HEALTH)
			{
				GetBestTargetsByHealth(ref targets, turnPriority);
			}
			else if (array[i] == ActionPriority.TYPE)
			{
				GetBestTargetByType(ref targets, turnPriority);
			}
			if (characterPathInfo != null && (turnPriority == TurnPriority.MOVE || (turnPriority == TurnPriority.ABILITY && ability != null && characterPathInfo._Path._Cost <= ability._Range)))
			{
				return characterPathInfo._TargetCharacter;
			}
			if (targets.Count == 1)
			{
				return targets[0]._TargetCharacter;
			}
		}
		if (targets.Count > 0)
		{
			return targets[0]._TargetCharacter;
		}
		return null;
	}

	private void GetBestTargetsByDistance(ref List<CharacterPathInfo> targets, TurnPriority priority)
	{
		float num = float.PositiveInfinity;
		foreach (CharacterPathInfo target in targets)
		{
			if ((priority != TurnPriority.ABILITY || HasValidAbility(target._TargetCharacter._CurrentNode) || HasValidMovePlusAbility(target._TargetCharacter._CurrentNode)) && target._Path != null && target._Path._Cost < num)
			{
				num = target._Path._Cost;
				if (num < GameManager.pInstance._DiagonalMoveCost)
				{
					num = GameManager.pInstance._DiagonalMoveCost;
					break;
				}
			}
		}
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i]._Path != null && targets[i]._Path._Cost > num)
			{
				targets.RemoveAt(i);
				i--;
			}
		}
	}

	private void GetBestTargetByType(ref List<CharacterPathInfo> targets, TurnPriority priority)
	{
		bool flag = false;
		ElementType elementCounterType = GameManager.pInstance.GetElementCounterType(pCharacterData._WeaponData._ElementType);
		foreach (CharacterPathInfo target in targets)
		{
			if ((priority != TurnPriority.ABILITY || HasValidAbility(target._TargetCharacter._CurrentNode) || HasValidMovePlusAbility(target._TargetCharacter._CurrentNode)) && target._TargetCharacter.pCharacterData._WeaponData._ElementType == elementCounterType)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i]._TargetCharacter.pCharacterData._WeaponData._ElementType != elementCounterType)
			{
				targets.RemoveAt(i);
				i--;
			}
		}
	}

	private void GetBestTargetsByHealth(ref List<CharacterPathInfo> targets, TurnPriority priority)
	{
		float num = float.PositiveInfinity;
		foreach (CharacterPathInfo target in targets)
		{
			if ((priority != TurnPriority.ABILITY || HasValidAbility(target._TargetCharacter._CurrentNode) || HasValidMovePlusAbility(target._TargetCharacter._CurrentNode)) && target._TargetCharacter.pCharacterData._Stats._Health.pCurrentValue < num)
			{
				num = target._TargetCharacter.pCharacterData._Stats._Health.pCurrentValue;
			}
		}
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i]._TargetCharacter.pCharacterData._Stats._Health.pCurrentValue > num)
			{
				targets.RemoveAt(i);
				i--;
			}
		}
	}

	private void UpdateCharacterPathInfo()
	{
		mCharacterPathInfo.Clear();
		mAlliesPathInfo.Clear();
		foreach (Character activeUnit in GameManager.pInstance._ActiveUnits)
		{
			if (!activeUnit.mIncapacitated)
			{
				CharacterPathInfo item = new CharacterPathInfo(activeUnit, mGrid.GetPath(_CurrentNode, activeUnit._CurrentNode, this, forcePath: true));
				if (activeUnit.pCharacterData._Team == pCharacterData._Team)
				{
					mAlliesPathInfo.Add(item);
				}
				else
				{
					mCharacterPathInfo.Add(item);
				}
			}
		}
	}

	private void SetTakeHitParticle(bool active)
	{
		if (!(_TakeHitParticle == null))
		{
			if (active && _TakeHitParticle.activeSelf)
			{
				_TakeHitParticle.SetActive(value: false);
			}
			_TakeHitParticle.SetActive(active);
		}
	}

	private void SetDeathParticle(bool active)
	{
		if (_DeathParticle != null)
		{
			_DeathParticle.SetActive(active);
		}
	}

	private void SetIncapacitatedParticle(bool active)
	{
		if (_IncapacitatedParticle != null)
		{
			_IncapacitatedParticle.SetActive(active);
		}
	}

	public void SetCinematicLayer(int inLayer)
	{
		foreach (CinematicLayerData mCinematicLayerDatum in mCinematicLayerData)
		{
			if (mCinematicLayerDatum != null && mCinematicLayerDatum._RendererObject != null)
			{
				mCinematicLayerDatum._RendererObject.layer = inLayer;
			}
		}
	}

	public void ResetCinematicLayers()
	{
		foreach (CinematicLayerData mCinematicLayerDatum in mCinematicLayerData)
		{
			if (mCinematicLayerDatum != null && mCinematicLayerDatum._RendererObject != null)
			{
				mCinematicLayerDatum._RendererObject.layer = mCinematicLayerDatum._OriginalLayer;
			}
		}
	}

	public Node GetFinalNode()
	{
		return _CurrentValidAttackPaths[0].GetFinalPathNode();
	}

	public bool IsValidMove(Node node)
	{
		return _CurrentValidMoveNodes.Contains(node);
	}

	public void ApplyStatusEffect(Effect effect)
	{
		mActiveStatusEffects.Add(effect);
		GameManager.pInstance.UpdateCharacterStatus(this);
	}

	public void RemoveStatusEffect(Effect effect)
	{
		mActiveStatusEffects.Remove(effect);
		effect.Remove();
		GameManager.pInstance.UpdateCharacterStatus(this);
		if (_UiEffects != null)
		{
			_UiEffects.AddRemovedEffect(effect);
			_UiEffects.CheckEffectsStatus();
		}
	}

	private void RemoveAllStatusEffects()
	{
		if (mActiveStatusEffects.Count > 0)
		{
			for (int num = mActiveStatusEffects.Count - 1; num >= 0; num--)
			{
				RemoveStatusEffect(mActiveStatusEffects[num]);
			}
			if (_UiEffects != null)
			{
				_UiEffects.CheckEffectsStatus();
			}
		}
	}

	public void TickStatusEffects(Effect.TickPhase tickPhase)
	{
		if (mActiveStatusEffects.Count == 0)
		{
			return;
		}
		for (int i = 0; i < mActiveStatusEffects.Count; i++)
		{
			if (mActiveStatusEffects[i]._TickPhase != tickPhase)
			{
				continue;
			}
			mActiveStatusEffects[i].TickChange(tickPhase);
			if (pCharacterData._Stats._Health.pCurrentValue <= 0f)
			{
				if (pCharacterData._Team == Team.ENEMY)
				{
					StartCoroutine(Die());
					return;
				}
				if (mIncapacitated)
				{
					if (_UiEffects != null)
					{
						_UiEffects.CheckEffectsStatus();
					}
					return;
				}
				BeginIncapacitatedCountdown();
			}
			if (mActiveStatusEffects[i]._Duration <= 0)
			{
				RemoveStatusEffect(mActiveStatusEffects[i]);
				i--;
			}
		}
		GameManager.pInstance.UpdateCharacterStatus(this);
	}

	private void SetAnimatorIdle(bool active)
	{
		if (mAnimator != null)
		{
			mAnimator.SetBool("bIdle", active);
		}
		if (_AdditionalAnimator != null)
		{
			_AdditionalAnimator.SetBool("bIdle", active);
		}
	}

	private void SetAnimatorMove(bool active)
	{
		if (mAnimator != null)
		{
			mAnimator.SetBool("bMove", active);
		}
		if (_AdditionalAnimator != null)
		{
			_AdditionalAnimator.SetBool("bMove", active);
		}
	}

	public IEnumerator DodgeEffect()
	{
		UserAchievementTask.Set(LevelManager.pInstance._SmoothMovesAchievementID);
		mDodgeState = DodgeState.DODGEACTIVATED;
		mDodgeAnimationLerpTimer = 0f;
		int num = UnityEngine.Random.Range(0, 100);
		Vector3 destination = mAnimator.transform.position;
		Vector3 originalPosition = mAnimator.transform.position;
		if (num <= 50)
		{
			destination += mAnimator.transform.right * _DodgeDistance;
		}
		else
		{
			destination += mAnimator.transform.right * (0f - _DodgeDistance);
		}
		if (!(mAnimator != null))
		{
			yield break;
		}
		while (mDodgeState != 0)
		{
			mDodgeAnimationLerpTimer += Time.deltaTime * (1f / _DodgeInitialAnimationTime);
			if (mDodgeAnimationLerpTimer >= 1f)
			{
				if (mDodgeState == DodgeState.DODGEACTIVATED)
				{
					mDodgeState = DodgeState.DODGEHOLD;
					mDodgeAnimationLerpTimer = 0f;
					UiFloatingTip component = UnityEngine.Object.Instantiate(GameManager.pInstance._FloatingTextPrefab, base.transform.position + new Vector3(0f, 2.5f, 0f), Quaternion.identity).GetComponent<UiFloatingTip>();
					if (component != null)
					{
						component.Initialize(GameManager.pInstance._DodgeText.GetLocalizedString(), Color.green);
					}
				}
				else if (mDodgeState == DodgeState.DODGERETURNING && originalPosition == mAnimator.transform.position)
				{
					mDodgeState = DodgeState.NOTACTIVE;
				}
			}
			if (mDodgeState == DodgeState.DODGEACTIVATED)
			{
				mAnimator.transform.position = Vector3.Lerp(mAnimator.transform.position, destination, mDodgeAnimationLerpTimer);
			}
			else if (mDodgeState == DodgeState.DODGERETURNING)
			{
				mAnimator.transform.position = Vector3.Lerp(mAnimator.transform.position, originalPosition, mDodgeAnimationLerpTimer / 2f);
			}
			else if (mDodgeState == DodgeState.DODGEHOLD)
			{
				yield return new WaitForSeconds(_DodgeHoldAnimationTime);
				mDodgeState = DodgeState.DODGERETURNING;
			}
			yield return new WaitForEndOfFrame();
		}
		yield return null;
	}

	private void SetAnimatorTakeHit(bool active)
	{
		if (mAnimator != null)
		{
			mAnimator.SetBool("bTakeHit", active);
		}
		if (_AdditionalAnimator != null)
		{
			_AdditionalAnimator.SetBool("bTakeHit", active);
		}
	}

	private IEnumerator AnimatorPlayTakeHit()
	{
		if (mAnimator != null)
		{
			if (_UnitTakeHitSFX != null && _UnitTakeHitSFX.Length != 0)
			{
				SnChannel.Play(_UnitTakeHitSFX[UnityEngine.Random.Range(0, _UnitTakeHitSFX.Length)], "STSFX_Pool", inForce: true);
			}
			SetTakeHitParticle(active: true);
			SetAnimatorTakeHit(active: true);
			while (!mAnimator.GetCurrentAnimatorStateInfo(0).IsName("TakeHit"))
			{
				yield return null;
			}
			SetAnimatorTakeHit(active: false);
		}
	}

	private void SetAnimatorIncapacitated(bool active)
	{
		if (mAnimator != null)
		{
			mAnimator.SetBool("bDead", active);
		}
		if (_AdditionalAnimator != null)
		{
			_AdditionalAnimator.SetBool("bDead", active);
		}
	}

	private void SetAnimatorStunned(bool active)
	{
		if (mAnimator != null)
		{
			mAnimator.SetBool("bStunned", active);
		}
		if (_AdditionalAnimator != null)
		{
			_AdditionalAnimator.SetBool("bStunned", active);
		}
	}

	private void SetAnimatorDead(bool active)
	{
		if (mAnimator != null)
		{
			mAnimator.SetBool("bDead", active);
		}
		if (_AdditionalAnimator != null)
		{
			_AdditionalAnimator.SetBool("bDead", active);
		}
	}

	public void PlayFX(string name)
	{
		foreach (FxAbilityData item in mFxData.FindAll((FxAbilityData fx) => fx._Name.Equals(name)))
		{
			SetupFX(item._EnableObjects);
			item.PlayFx(base.transform);
			StartCoroutine(ResetFX(item));
		}
	}

	private void SetupFX(List<FxObjectData> inObjData)
	{
		List<FxObjectData> list = inObjData.FindAll((FxObjectData t) => t._FXType == FxObjectData.FXTYPE.STARTPARTICLE);
		if (list.Count > 0)
		{
			foreach (FxObjectData item in list)
			{
				if ((bool)item._Object && (bool)_ParentProjectileTransform)
				{
					item._Object.transform.position = _ParentProjectileTransform.position;
				}
			}
		}
		FxObjectData fxObjectData = inObjData.Find((FxObjectData t) => t._FXType == FxObjectData.FXTYPE.PROJECTILE);
		if (fxObjectData == null)
		{
			return;
		}
		float num = 1f;
		GameObject @object = fxObjectData._Object;
		ObAmmo component = fxObjectData._Object.GetComponent<ObAmmo>();
		if (!string.IsNullOrEmpty(fxObjectData._OverrideParentLocation))
		{
			Transform[] componentsInChildren = base.transform.GetComponentInChildren<Animator>().transform.GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				if (transform.name.Equals(fxObjectData._OverrideParentLocation))
				{
					_ParentProjectileTransform = transform;
					break;
				}
			}
		}
		if ((bool)@object && (bool)component)
		{
			if ((bool)fxObjectData._Object && (bool)_ParentProjectileTransform)
			{
				fxObjectData._Object.transform.position = _ParentProjectileTransform.position;
			}
			if ((bool)mCurrentTarget && (bool)mCurrentTarget._ProjectileImpactLocation)
			{
				@object.transform.LookAt(mCurrentTarget._ProjectileImpactLocation.position);
			}
			else
			{
				@object.transform.LookAt(mCurrentTargetNode.transform.position);
			}
			component.Velocity = (mCurrentTarget._ProjectileImpactLocation.position - @object.transform.position) * fxObjectData._ProjectileSpeed * Time.deltaTime;
			num = ((!mCurrentTarget || !mCurrentTarget._ProjectileImpactLocation) ? (Vector3.Distance(@object.transform.position, mCurrentTargetNode.transform.position) / component.Velocity.magnitude) : (Vector3.Distance(@object.transform.position, mCurrentTarget._ProjectileImpactLocation.position) / component.Velocity.magnitude));
			component.DespawnDelay(num);
		}
		FxObjectData fxObjectData2 = inObjData.Find((FxObjectData t) => t._FXType == FxObjectData.FXTYPE.ENDPARTICLE);
		if (fxObjectData2 != null)
		{
			if ((bool)mCurrentTarget && (bool)mCurrentTarget._ProjectileImpactLocation)
			{
				fxObjectData2._Object.transform.position = mCurrentTarget._ProjectileImpactLocation.position;
			}
			else
			{
				fxObjectData2._Object.transform.position = mCurrentTargetNode.transform.position;
			}
			ParticleSystem[] componentsInChildren2 = fxObjectData2._Object.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				ParticleSystem.MainModule main = componentsInChildren2[i].main;
				main.startDelay = num;
			}
		}
	}

	public void ActivateVFX(string name)
	{
		if (name == "SetTransparentMaterials")
		{
			SetTransparentMaterials(setTransparent: true);
		}
	}

	private IEnumerator ResetFX(FxAbilityData fxData)
	{
		yield return new WaitForSeconds(fxData._ResetTimer);
		fxData.ResetFX(base.transform);
	}

	public void StopAllEffectVisuals()
	{
		foreach (Effect mActiveStatusEffect in mActiveStatusEffects)
		{
			mActiveStatusEffect.StopFx(Effect.FxPlayType.ALL);
		}
		if (_UiEffects != null)
		{
			_UiEffects.Stop();
		}
	}

	public void SetAlertStatus()
	{
		pAlerted = true;
		if (_UiGridInfo != null)
		{
			_UiGridInfo.ShowAlertWidget(active: true);
		}
	}

	public void SetTransparentMaterials(bool setTransparent)
	{
		SkinnedMeshRenderer componentInChildren = base.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		if (setTransparent)
		{
			List<Material> list = new List<Material>();
			for (int i = 0; i <= componentInChildren.materials.Length - 1; i++)
			{
				if (componentInChildren.materials[i].name.Contains(mTransparentMaterialOverride.name))
				{
					return;
				}
				list.Add(componentInChildren.materials[i]);
			}
			mCachedMaterials = list.ToArray();
		}
		List<Material> list2 = new List<Material>();
		for (int j = 0; j <= componentInChildren.materials.Length - 1; j++)
		{
			list2.Add(setTransparent ? mTransparentMaterialOverride : mCachedMaterials[j]);
		}
		componentInChildren.materials = list2.ToArray();
	}
}
