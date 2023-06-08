using System;
using UnityEngine;

public class EnEnemy : SplineControl
{
	public enum FadeState
	{
		NONE,
		FADEIN,
		FADEOUT
	}

	public static GameObject _Target;

	public EnEnemyStateDescription _StateSettings;

	public Material _FaceMaterial;

	public AudioClip _BattleCry;

	public AudioClip _Poof;

	public float _WalkSpeed;

	public float _WalkAnimScale;

	public float _AttackSpeed;

	public float _AttackAnimScale;

	public float _DropOffGroundCheckDistance = 3f;

	public EnEnemyTimeRange _TimeToIdle;

	public EnEnemyTimeRange _IdleTime;

	public EnEnemyTimeRange _TiredTime;

	public EnEnemyTimeRange _StunTime;

	public GameObject _StunAnim;

	public EnEnemyTimeRange _AlertTime;

	public EnEnemyTimeRange _CelebrationTime;

	public GameObject _DeathParticles;

	public GameObject _SweatParticles;

	public float _AlertRadius;

	public float _AttackRadius;

	public float _AttackDistance;

	public float _AttackPathRegenTime;

	public bool _TakeAvatarMoney = true;

	public float _RunAwayRadius;

	public int _HitPoints;

	public bool _IsButtStompVulnerable;

	public bool _IsProjectileVulnerable;

	public bool _CanWalkOnWater;

	public bool _CanShoot;

	public bool _CanTilt;

	public Transform _ProjectileStartPoint;

	public Vector3 _ProjectileStartOffset = new Vector3(0.1f, 0.55f, 0f);

	public float _ShooterSecAttackRadius;

	public GameObject _Projectile;

	public float _RateOfFire = 1.5f;

	public int _ShotsFiredUntilReload = 5;

	public float _ReloadTime = 5f;

	public float _TimeToShoot = 0.5f;

	public int _Damage = 1;

	public float _ImmunityTime = 5f;

	public Vector3 _AttackForce;

	public float _LaunchHeight;

	public bool _RandomizeStartUpAnim = true;

	public bool _CanRunAway;

	public float _RunAwayTime = 3f;

	public float _RunAwayPathRegenTime = 5f;

	public bool _WaitForStart;

	public bool _DoDamage = true;

	public int _RunAwayDistance = 10;

	public float _FadeOutTime = 2f;

	public float _RespawnTime = 10f;

	protected CharacterController mController;

	private Spline mOriginalSpline;

	private bool mOriginalLoop;

	private Vector3 mOriginalPosition;

	private Vector3 mOriginalRotation;

	private float mOriginalSplinePos;

	private Vector3 mMovement;

	protected EnEnemyState mState;

	private EnEnemyState mPrevState;

	private float mTimer;

	private float mPosOnSplineBeforeAttack;

	private Vector3 mPosBeforeAttack;

	private SnChannel mSndChannel;

	private bool mSoundDisabled;

	private bool mPlayingAttackSfx;

	private GameObject mDeathParticle;

	private float mDeathParticleTimer = 1f;

	private bool mDeathParticleTimerActive;

	private float mDeathParticleDuration = 1.8f;

	private float mSweatParticleTimer = 1f;

	private bool mSweatParticleTimerActive;

	private float mSweatParticleDuration = 2.25f;

	private bool mLoaded;

	protected int mStartHitPoints;

	private float mReloadTime;

	private float mTimeElapsedAfterShot;

	private int mShotsFiredUntilReload;

	private bool mDropOffFlag;

	private float mImmunityTime;

	private float mFlashingTimer;

	private float mFlashingInterval;

	private bool mInvincible;

	private float mRunAwayTimer;

	private bool mWaitForStart;

	private float mRunAwayPathRegenTimer;

	private float mRespawnTimer;

	private float mFadeOutTimer;

	private EnEnemySubState mSubState;

	private FadeState mFadeState;

	public float _RunAwayArcDegree = 45f;

	public float pSpeed
	{
		get
		{
			return Speed;
		}
		set
		{
			Speed = value;
		}
	}

	public EnEnemyState pState
	{
		get
		{
			return mState;
		}
		set
		{
			mPrevState = mState;
			mState = value;
		}
	}

	public bool pInvincible
	{
		get
		{
			return mInvincible;
		}
		set
		{
			mInvincible = value;
			if (!mInvincible)
			{
				StopFlashing();
				return;
			}
			mImmunityTime = _ImmunityTime;
			StartFlashing(0.1f);
		}
	}

	public void Awake()
	{
		mController = (CharacterController)collider;
	}

	public override void Start()
	{
		base.Start();
		mSndChannel = (SnChannel)base.gameObject.GetComponent("SnChannel");
		mOriginalSpline = mSpline;
		mOriginalLoop = Looping;
		mOriginalPosition = base.transform.position;
		mOriginalRotation = base.transform.eulerAngles;
		mOriginalSplinePos = CurrentPos;
		Initialize();
		mShotsFiredUntilReload = _ShotsFiredUntilReload;
		mReloadTime = _ReloadTime;
		mStartHitPoints = _HitPoints;
		mRunAwayTimer = _RunAwayTime;
		mRespawnTimer = _RespawnTime;
		mFadeOutTimer = _FadeOutTime;
		mRunAwayPathRegenTimer = _RunAwayPathRegenTime;
	}

	private void Initialize()
	{
		mWaitForStart = _WaitForStart;
		if (_WalkSpeed > 0f && mOriginalSpline != null && !_WaitForStart)
		{
			mState = EnEnemyState.PATROL;
			mTimer = UnityEngine.Random.Range(_TimeToIdle._Min, _TimeToIdle._Max);
			pSpeed = _WalkSpeed;
			PlayAnim(_StateSettings._Walk._Anim, 0f, 0.2f, WrapMode.Loop, _WalkAnimScale * _WalkSpeed);
			MoveOnSpline(0f);
			return;
		}
		mState = EnEnemyState.IDLE;
		if (!_RandomizeStartUpAnim)
		{
			PlayAnim(_StateSettings._Idle._Anim, WrapMode.Loop);
		}
		else
		{
			float offset = UnityEngine.Random.Range(0f, 1f);
			PlayAnim(_StateSettings._Idle._Anim, offset, 0.2f, WrapMode.Loop, 1f);
		}
		if (_WalkSpeed <= 0f)
		{
			PlaySfx(_StateSettings._Idle._Sfx, inLoop: true);
		}
		if (mOriginalSpline != null)
		{
			MoveOnSpline(0f);
		}
	}

	public void OnStart()
	{
		mWaitForStart = false;
	}

	public virtual void OnRestart(bool resetHitPoints)
	{
		mFadeOutTimer = 0f;
		mRunAwayTimer = _RunAwayTime;
		mRespawnTimer = _RespawnTime;
		mRunAwayPathRegenTimer = _RunAwayPathRegenTime;
		if (mState == EnEnemyState.NONE)
		{
			return;
		}
		mWaitForStart = _WaitForStart;
		mSpline = mOriginalSpline;
		Looping = mOriginalLoop;
		base.transform.position = mOriginalPosition;
		base.transform.eulerAngles = mOriginalRotation;
		CurrentPos = mOriginalSplinePos;
		if (resetHitPoints)
		{
			_HitPoints = mStartHitPoints;
		}
		Initialize();
		if (_StunAnim != null)
		{
			Transform transform = base.transform.Find("MainRoot/Root_J/SquashPivot_J/Hair01_J/" + _StunAnim.name);
			if ((bool)transform)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		if (mDeathParticleTimerActive)
		{
			mDeathParticleTimerActive = false;
			ParticleSystem component = mDeathParticle.GetComponent<ParticleSystem>();
			component.Clear();
			component.Stop();
			UnityEngine.Object.Destroy(mDeathParticle);
			mDeathParticle = null;
		}
		if (mSweatParticleTimerActive)
		{
			mSweatParticleTimerActive = false;
			ParticleSystem component2 = _SweatParticles.GetComponent<ParticleSystem>();
			component2.Clear();
			component2.Stop();
		}
	}

	public virtual bool TargetActive()
	{
		if (_Target == null)
		{
			return false;
		}
		if (_Target != AvAvatar.pObject)
		{
			return _Target.activeInHierarchy;
		}
		if (!AvAvatar.pObject.activeInHierarchy || !AvAvatar.pToolbar.activeInHierarchy || !((UiToolbar)AvAvatar.pToolbar.GetComponent("UiToolbarNGUI")).enabled)
		{
			return false;
		}
		return true;
	}

	public override void Update()
	{
		if (!TargetActive())
		{
			if (!mSoundDisabled)
			{
				mSoundDisabled = true;
				DisableSound(inDisable: true);
			}
		}
		else if (mSoundDisabled)
		{
			mSoundDisabled = false;
			DisableSound(inDisable: false);
		}
		if (TargetActive())
		{
			Physics.IgnoreCollision(collider, _Target.GetComponent<Collider>());
		}
		if (mDeathParticleTimerActive)
		{
			mDeathParticleTimer -= Time.deltaTime;
			if (mDeathParticleTimer <= 0f)
			{
				mDeathParticleTimerActive = false;
				ParticleSystem component = mDeathParticle.GetComponent<ParticleSystem>();
				component.Clear();
				component.Stop();
				UnityEngine.Object.Destroy(mDeathParticle);
				mDeathParticle = null;
			}
		}
		if (mSweatParticleTimerActive)
		{
			mSweatParticleTimer -= Time.deltaTime;
			if (mSweatParticleTimer <= 0f)
			{
				mSweatParticleTimerActive = false;
				_SweatParticles.GetComponent<ParticleSystem>().Stop();
			}
		}
		switch (mState)
		{
		case EnEnemyState.IDLE:
			if (_WalkSpeed > 0f && mSpline != null && !mWaitForStart)
			{
				mTimer -= Time.deltaTime;
				if (mTimer < 0f)
				{
					mState = EnEnemyState.PATROL;
					pSpeed = _WalkSpeed;
					mTimer = UnityEngine.Random.Range(_TimeToIdle._Min, _TimeToIdle._Max);
					PlaySfx(_StateSettings._Walk._Sfx, inLoop: false);
					PlayAnim(_StateSettings._Walk._Anim, 0f, 0.2f, WrapMode.Loop, _WalkAnimScale * _WalkSpeed);
				}
			}
			break;
		case EnEnemyState.PATROL:
			mTimer -= Time.deltaTime;
			if (mTimer < 0f)
			{
				mState = EnEnemyState.IDLE;
				mTimer = UnityEngine.Random.Range(_IdleTime._Min, _IdleTime._Max);
				PlayAnim(_StateSettings._Idle._Anim, WrapMode.Loop);
				if (_WalkSpeed <= 0f)
				{
					PlaySfx(_StateSettings._Idle._Sfx, inLoop: true);
				}
			}
			else
			{
				base.Update();
				if (!(_Target == null) && mEndReached && !Looping)
				{
					mEndReached = false;
					pSpeed = 0f - pSpeed;
				}
			}
			break;
		case EnEnemyState.ALERT:
			if (!TargetActive())
			{
				break;
			}
			mTimer -= Time.deltaTime;
			if (mTimer < 0f)
			{
				if (_CanShoot)
				{
					mState = EnEnemyState.SHOOT;
				}
				else if (_CanRunAway)
				{
					RunAway();
				}
				else if (!mDropOffFlag)
				{
					Attack();
				}
			}
			else
			{
				Vector3 position = _Target.transform.position;
				position.y = base.transform.position.y;
				base.transform.LookAt(position);
			}
			break;
		case EnEnemyState.ATTACK:
			if (!TargetActive())
			{
				Alert();
				break;
			}
			mReloadTime = 0f;
			mTimer -= Time.deltaTime;
			if (mTimer < 0f)
			{
				mTimer = _AttackPathRegenTime;
				if (FindPath(base.transform.position, _Target.transform.position))
				{
					if (pSpeed == 0f)
					{
						if (!mDropOffFlag)
						{
							PlayAnim(_StateSettings._Attack._Anim, 0f, 0.2f, WrapMode.Loop, _AttackAnimScale * _AttackSpeed);
						}
						pSpeed = _AttackSpeed;
					}
				}
				else if (Speed != 0f)
				{
					PlayAnim(_StateSettings._Alert._Anim, WrapMode.Loop);
					pSpeed = 0f;
				}
			}
			base.Update();
			if (mEndReached && Speed != 0f)
			{
				PlayAnim(_StateSettings._Alert._Anim, WrapMode.Loop);
				pSpeed = 0f;
			}
			if (mDropOffFlag)
			{
				if (DistanceToTarget() > _AttackRadius)
				{
					Alert();
					return;
				}
				if (!base.animation.IsPlaying(_StateSettings._Alert._Anim))
				{
					PlayAnim(_StateSettings._Alert._Anim, WrapMode.Loop);
				}
			}
			else if (Speed != 0f && !base.animation.IsPlaying(_StateSettings._Attack._Anim))
			{
				PlayAnim(_StateSettings._Attack._Anim, WrapMode.Loop);
			}
			if ((base.transform.position - mPosBeforeAttack).magnitude >= _AttackDistance)
			{
				PlaySfx(_StateSettings._Tired._Sfx, inLoop: false);
				if (_SweatParticles != null)
				{
					_SweatParticles.GetComponent<ParticleSystem>().Play();
					mSweatParticleTimerActive = true;
					mSweatParticleTimer = mSweatParticleDuration;
				}
				mState = EnEnemyState.TIRED;
				mTimer = UnityEngine.Random.Range(_TiredTime._Min, _TiredTime._Max);
				PlayAnim(_StateSettings._Tired._Anim, WrapMode.Loop);
			}
			break;
		case EnEnemyState.TIRED:
			if (TargetActive())
			{
				mTimer -= Time.deltaTime;
				if (mTimer < 0f)
				{
					Return();
				}
			}
			break;
		case EnEnemyState.STUNNED:
			if (!TargetActive())
			{
				break;
			}
			mTimer -= Time.deltaTime;
			if (!(mTimer < 0f))
			{
				break;
			}
			if (_StunAnim != null)
			{
				UnityEngine.Object.Destroy(base.transform.Find("MainRoot/Root_J/SquashPivot_J/Hair01_J/" + _StunAnim.name).gameObject);
			}
			if (_WalkSpeed > 0f)
			{
				Alert();
				break;
			}
			mState = EnEnemyState.IDLE;
			PlayAnim(_StateSettings._Idle._Anim, WrapMode.Loop);
			if (_WalkSpeed <= 0f)
			{
				PlaySfx(_StateSettings._Idle._Sfx, inLoop: true);
			}
			break;
		case EnEnemyState.RETURN:
			base.Update();
			if (!mEndReached)
			{
				break;
			}
			SetSpline(mOriginalSpline);
			Looping = mOriginalLoop;
			CurrentPos = mPosOnSplineBeforeAttack;
			if (mOriginalSpline == null)
			{
				mState = EnEnemyState.IDLE;
				PlayAnim(_StateSettings._Idle._Anim, WrapMode.Loop);
				if (_WalkSpeed <= 0f)
				{
					PlaySfx(_StateSettings._Idle._Sfx, inLoop: true);
				}
			}
			else
			{
				mState = EnEnemyState.PATROL;
				mTimer = UnityEngine.Random.Range(_TimeToIdle._Min, _TimeToIdle._Max);
			}
			break;
		case EnEnemyState.TAKEHIT:
			if ((double)base.animation[_StateSettings._TakeHit._Anim].normalizedTime > 1.0)
			{
				Stun();
			}
			break;
		case EnEnemyState.DEATH:
			if (!base.animation.IsPlaying(_StateSettings._Death._Anim))
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			break;
		case EnEnemyState.SHOOT:
		{
			if (base.animation.IsPlaying(_StateSettings._Shoot._Anim))
			{
				break;
			}
			if (_Target != null && _Target.transform != null)
			{
				if (_CanTilt)
				{
					base.transform.LookAt(_Target.transform);
				}
				else
				{
					Vector3 position2 = _Target.transform.position;
					position2.y = base.transform.position.y;
					base.transform.LookAt(position2);
				}
			}
			if (!TargetActive())
			{
				break;
			}
			if (!mLoaded)
			{
				if (base.animation.IsPlaying(_StateSettings._Load._Anim))
				{
					if (base.animation[_StateSettings._Load._Anim].normalizedTime >= 1f)
					{
						mLoaded = true;
					}
				}
				else
				{
					PlayAnim(_StateSettings._Load._Anim, WrapMode.ClampForever);
				}
			}
			else if (!base.animation.IsPlaying(_StateSettings._ShootIdle._Anim))
			{
				PlayAnim(_StateSettings._ShootIdle._Anim, WrapMode.Loop);
			}
			float num = DistanceToTarget();
			if (_ShooterSecAttackRadius > 0f && num <= _ShooterSecAttackRadius)
			{
				Attack();
				break;
			}
			if (num > _AlertRadius)
			{
				Return();
				break;
			}
			if (mReloadTime > 0f)
			{
				mReloadTime -= Time.deltaTime;
				break;
			}
			mReloadTime = _ReloadTime;
			if (!(mTimeElapsedAfterShot > 0f))
			{
				mTimeElapsedAfterShot = _RateOfFire;
				PlayAnim(_StateSettings._Shoot._Anim, WrapMode.Once);
				Invoke("Fire", _TimeToShoot);
			}
			break;
		}
		case EnEnemyState.CELEBRATE:
		{
			mTimer -= Time.deltaTime;
			if (!(mTimer <= 0f))
			{
				break;
			}
			float num2 = DistanceToTarget();
			if (_ShooterSecAttackRadius > 0f && num2 <= _ShooterSecAttackRadius)
			{
				Attack();
				break;
			}
			if (_CanShoot && num2 <= _AlertRadius)
			{
				mState = EnEnemyState.SHOOT;
				break;
			}
			mState = EnEnemyState.IDLE;
			PlayAnim(_StateSettings._Idle._Anim, WrapMode.Loop);
			if (_WalkSpeed <= 0f)
			{
				PlaySfx(_StateSettings._Idle._Sfx, inLoop: true);
			}
			break;
		}
		case EnEnemyState.RUNAWAY:
			mRunAwayTimer -= Time.deltaTime;
			if (mRunAwayTimer <= 0f)
			{
				SetFadeState(FadeState.FADEOUT);
			}
			if (mEndReached)
			{
				RunAway();
			}
			base.Update();
			break;
		}
		mMovement += Physics.gravity * Time.deltaTime;
		mTimeElapsedAfterShot -= Time.deltaTime;
		if (mSubState == EnEnemySubState.ON_WATER && _CanWalkOnWater)
		{
			mMovement.y = 0f;
		}
		mController.Move(mMovement);
		mMovement = Vector3.zero;
		CheckForTarget();
		if (pInvincible)
		{
			UpdateImmunity();
		}
	}

	private void OnTargetDamaged()
	{
		if (!string.IsNullOrEmpty(_StateSettings._Celebrate._Anim) && mState != EnEnemyState.TAKEHIT && mState != EnEnemyState.DEATH && !pInvincible)
		{
			mState = EnEnemyState.CELEBRATE;
			mTimer = UnityEngine.Random.Range(_CelebrationTime._Min, _CelebrationTime._Max);
			PlayAnim(_StateSettings._Celebrate._Anim, WrapMode.Loop);
		}
	}

	private void StartFlashing(float iInterval)
	{
		mFlashingTimer = iInterval;
		mFlashingInterval = iInterval;
	}

	private void StopFlashing()
	{
		mFlashingTimer = 0f;
		Component[] componentsInChildren = GetComponentsInChildren(typeof(Renderer));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Renderer)componentsInChildren[i]).enabled = true;
		}
	}

	private void UpdateImmunity()
	{
		mImmunityTime -= Time.deltaTime;
		if (mImmunityTime < 0f)
		{
			pInvincible = false;
			return;
		}
		if (mImmunityTime < _ImmunityTime * 0.33f)
		{
			mFlashingInterval = 0.05f;
		}
		mFlashingTimer -= Time.deltaTime;
		if (!(mFlashingTimer > 0f))
		{
			Component[] componentsInChildren = GetComponentsInChildren(typeof(Renderer));
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Renderer obj = (Renderer)componentsInChildren[i];
				obj.enabled = !obj.enabled;
			}
			mFlashingTimer = mFlashingInterval;
		}
	}

	private void Fire()
	{
		Transform projectileStartPoint = _ProjectileStartPoint;
		Vector3 center = _Target.GetComponent<Collider>().bounds.center;
		Vector3 iStartPoint = projectileStartPoint.position + projectileStartPoint.rotation * _ProjectileStartOffset;
		PlayAnim(_StateSettings._Shoot._Anim, WrapMode.Once);
		PrProjectileManager.FireAProjectile(base.gameObject, iStartPoint, _Projectile, center);
		if (!string.IsNullOrEmpty(_StateSettings._ShootIdle._Anim))
		{
			PlayAnim(_StateSettings._ShootIdle._Anim, WrapMode.Loop);
		}
		mShotsFiredUntilReload--;
		if (mShotsFiredUntilReload <= 0)
		{
			mLoaded = false;
			mShotsFiredUntilReload = _ShotsFiredUntilReload;
		}
	}

	public override void SetPosOnSpline(float p)
	{
		mSpline.GetPosQuatByDist(CurrentPos, out var pos, out var quat);
		Vector3 forward = pos - base.transform.position;
		forward.y = 0f;
		if ((double)forward.magnitude >= 0.01)
		{
			quat = Quaternion.LookRotation(forward);
			base.transform.rotation = quat;
		}
		mMovement = forward;
		if (_DropOffGroundCheckDistance <= 0f)
		{
			return;
		}
		pos.y = base.transform.position.y + _DropOffGroundCheckDistance;
		if (UtUtilities.GetGroundHeight(pos, _DropOffGroundCheckDistance * 2f) != float.NegativeInfinity)
		{
			mDropOffFlag = false;
			return;
		}
		mMovement = Vector3.zero;
		mDropOffFlag = true;
		if (mState == EnEnemyState.RETURN)
		{
			mSpline = null;
			mState = EnEnemyState.IDLE;
			mTimer = UnityEngine.Random.Range(_IdleTime._Min, _IdleTime._Max);
			PlayAnim(_StateSettings._Idle._Anim, WrapMode.Loop);
		}
		else
		{
			pSpeed = 0f;
		}
	}

	private void CheckForTarget()
	{
		if (_Target == null)
		{
			return;
		}
		float num = DistanceToTarget();
		switch (mState)
		{
		case EnEnemyState.IDLE:
		case EnEnemyState.PATROL:
			if (_CanShoot)
			{
				mReloadTime = _ReloadTime;
				if (_ShooterSecAttackRadius > 0f && num <= _ShooterSecAttackRadius && TargetActive())
				{
					mPosOnSplineBeforeAttack = CurrentPos;
					mPosBeforeAttack = base.transform.position;
					Attack();
				}
				else if (_AlertRadius > 0f && num <= _AlertRadius)
				{
					mPosOnSplineBeforeAttack = CurrentPos;
					mPosBeforeAttack = base.transform.position;
					Alert();
				}
				else if (_AttackRadius > 0f && num <= _AttackRadius)
				{
					mState = EnEnemyState.SHOOT;
				}
			}
			else if (_CanRunAway)
			{
				if (_RunAwayRadius > 0f && num <= _RunAwayRadius && TargetActive())
				{
					mPosOnSplineBeforeAttack = CurrentPos;
					mPosBeforeAttack = base.transform.position;
					RunAway();
				}
				else if (_AlertRadius > 0f && num <= _AlertRadius)
				{
					mPosOnSplineBeforeAttack = CurrentPos;
					mPosBeforeAttack = base.transform.position;
					Alert();
				}
			}
			else if (_AttackRadius > 0f && num <= _AttackRadius && TargetActive())
			{
				mPosOnSplineBeforeAttack = CurrentPos;
				mPosBeforeAttack = base.transform.position;
				Attack();
			}
			else if (_AlertRadius > 0f && num <= _AlertRadius)
			{
				mPosOnSplineBeforeAttack = CurrentPos;
				mPosBeforeAttack = base.transform.position;
				Alert();
			}
			break;
		case EnEnemyState.ALERT:
			if (num > _AlertRadius)
			{
				Return();
			}
			else
			{
				if (!TargetActive())
				{
					break;
				}
				if (_CanShoot)
				{
					if (_ShooterSecAttackRadius > 0f && num <= _ShooterSecAttackRadius)
					{
						Attack();
					}
					else if (num <= _AttackRadius)
					{
						mState = EnEnemyState.SHOOT;
					}
				}
				else if (_CanRunAway)
				{
					if (_RunAwayRadius > 0f && num <= _RunAwayRadius)
					{
						mPosOnSplineBeforeAttack = CurrentPos;
						mPosBeforeAttack = base.transform.position;
						RunAway();
					}
				}
				else if (_AttackRadius > 0f && num <= _AttackRadius)
				{
					Attack();
				}
			}
			break;
		case EnEnemyState.RETURN:
			if (_CanRunAway)
			{
				if (_RunAwayRadius > 0f && num <= _RunAwayRadius && TargetActive())
				{
					mPosOnSplineBeforeAttack = CurrentPos;
					mPosBeforeAttack = base.transform.position;
					RunAway();
				}
				else if (_AlertRadius > 0f && num <= _AlertRadius)
				{
					Alert();
				}
			}
			else if (num <= CurrentPos)
			{
				Attack();
			}
			break;
		case EnEnemyState.RUNAWAY:
			mRunAwayPathRegenTimer -= Time.deltaTime;
			if (_RunAwayRadius > 0f && num <= _RunAwayRadius && mRunAwayPathRegenTimer <= 0f)
			{
				RunAway();
			}
			break;
		}
		if (_CanRunAway)
		{
			if (mFadeState != 0)
			{
				Fade();
			}
			if (mFadeOutTimer <= 0f)
			{
				mRespawnTimer -= Time.deltaTime;
				if (mRespawnTimer <= 0f && (_AlertRadius <= 0f || num > _AlertRadius))
				{
					mState = EnEnemyState.IDLE;
					mPrevState = EnEnemyState.IDLE;
					OnRestart(resetHitPoints: true);
					SetFadeState(FadeState.FADEIN);
				}
			}
		}
		DoDamageIfInRange(num);
	}

	protected virtual void DoDamageIfInRange(float distanceToTarget)
	{
		float num = 0f;
		CharacterController characterController = (CharacterController)_Target.GetComponent(typeof(CharacterController));
		num = ((!(characterController != null)) ? GetRadius() : characterController.radius);
		if (distanceToTarget <= mController.radius + num && mState != EnEnemyState.DEATH && mState != EnEnemyState.DEAD)
		{
			if (mState == EnEnemyState.CELEBRATE)
			{
				mState = EnEnemyState.IDLE;
			}
			_Target.SendMessage("OnEnemyCollide", base.gameObject, SendMessageOptions.DontRequireReceiver);
			if (mState != EnEnemyState.TAKEHIT && mState != EnEnemyState.DEATH && mState != EnEnemyState.CELEBRATE && mState != 0)
			{
				Stun();
			}
		}
	}

	private Collider GetCollider()
	{
		Collider component = _Target.GetComponent<Collider>();
		if (component != null)
		{
			return component;
		}
		return _Target.GetComponentInChildren(typeof(Collider)) as Collider;
	}

	protected virtual float GetRadius()
	{
		float result = 0f;
		Collider collider = GetCollider();
		if (collider != null)
		{
			Type type = collider.GetType();
			if (type == typeof(CapsuleCollider))
			{
				CapsuleCollider capsuleCollider = (CapsuleCollider)collider;
				if (capsuleCollider != null)
				{
					result = capsuleCollider.radius;
				}
			}
			else if (type == typeof(SphereCollider))
			{
				SphereCollider sphereCollider = (SphereCollider)collider;
				if (sphereCollider != null)
				{
					result = sphereCollider.radius;
				}
			}
		}
		return result;
	}

	private void Alert()
	{
		mState = EnEnemyState.ALERT;
		PlayAnim(_StateSettings._Alert._Anim, WrapMode.Loop);
		mTimer = UnityEngine.Random.Range(_AlertTime._Min, _AlertTime._Max);
		PlaySfx(_StateSettings._Alert._Sfx, inLoop: true);
	}

	private void Attack()
	{
		mReloadTime = 0f;
		mState = EnEnemyState.ATTACK;
		mTimer = _AttackPathRegenTime;
		if (FindPath(base.transform.position, _Target.transform.position))
		{
			if (_BattleCry != null)
			{
				PlaySfx(new AudioClip[1] { _BattleCry }, inLoop: false);
			}
			if (!mPlayingAttackSfx)
			{
				PlaySfx(_StateSettings._Attack._Sfx, inLoop: true);
				mPlayingAttackSfx = true;
			}
			PlayAnim(_StateSettings._Attack._Anim, 0f, 0.2f, WrapMode.Loop, _AttackAnimScale * _AttackSpeed);
			pSpeed = _AttackSpeed;
		}
		else
		{
			PlayAnim(_StateSettings._Alert._Anim, WrapMode.Loop);
			pSpeed = 0f;
		}
	}

	private void Return()
	{
		mState = EnEnemyState.RETURN;
		if (FindPath(base.transform.position, mPosBeforeAttack))
		{
			pSpeed = _WalkSpeed;
			PlayAnim(_StateSettings._Walk._Anim, 0f, 0.2f, WrapMode.Loop, _WalkAnimScale * _WalkSpeed);
			StopSfx();
		}
		else if (_WalkSpeed > 0f && mOriginalSpline != null)
		{
			mState = EnEnemyState.PATROL;
			mTimer = UnityEngine.Random.Range(_TimeToIdle._Min, _TimeToIdle._Max);
			pSpeed = _WalkSpeed;
			PlayAnim(_StateSettings._Walk._Anim, 0f, 0.2f, WrapMode.Loop, _WalkAnimScale * _WalkSpeed);
			StopSfx();
			MoveOnSpline(0f);
		}
		else
		{
			mState = EnEnemyState.IDLE;
			PlayAnim(_StateSettings._Idle._Anim, WrapMode.Loop);
			if (_WalkSpeed <= 0f)
			{
				PlaySfx(_StateSettings._Idle._Sfx, inLoop: true);
			}
			else
			{
				StopSfx();
			}
		}
	}

	protected virtual void TakeHit(int damage)
	{
		if ((_CanRunAway && mState == EnEnemyState.NONE) || pInvincible || _HitPoints <= 0)
		{
			return;
		}
		_HitPoints -= damage;
		if (_HitPoints <= 0)
		{
			PlaySfx(_StateSettings._Death._Sfx, inLoop: false);
			mState = EnEnemyState.DEATH;
			PlayAnim(_StateSettings._Death._Anim, WrapMode.Once);
			if (_DeathParticles != null)
			{
				mDeathParticleTimerActive = true;
				mDeathParticle = UnityEngine.Object.Instantiate(_DeathParticles, base.transform.position, base.transform.rotation);
				mDeathParticle.transform.parent = base.transform;
				mDeathParticle.GetComponent<ParticleSystem>().Play();
				mDeathParticleTimer = mDeathParticleDuration;
			}
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.pToolbar.BroadcastMessage("Collect", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
			GenerateCoins();
		}
		else
		{
			pInvincible = true;
			PlaySfx(_StateSettings._TakeHit._Sfx, inLoop: false);
			mState = EnEnemyState.TAKEHIT;
			PlayAnim(_StateSettings._TakeHit._Anim, WrapMode.ClampForever);
		}
	}

	protected virtual void GenerateCoins()
	{
		ObBouncyCoinEmitter obBouncyCoinEmitter = (ObBouncyCoinEmitter)GetComponent("ObBouncyCoinEmitter");
		if (obBouncyCoinEmitter != null)
		{
			obBouncyCoinEmitter.GenerateCoins();
		}
	}

	protected void Stun()
	{
		if (mState != EnEnemyState.STUNNED)
		{
			if (mState == EnEnemyState.IDLE || mState == EnEnemyState.PATROL)
			{
				mPosOnSplineBeforeAttack = CurrentPos;
				mPosBeforeAttack = base.transform.position;
			}
			mState = EnEnemyState.STUNNED;
			if (_StunAnim != null)
			{
				Transform parent = base.transform.Find("MainRoot/Root_J/SquashPivot_J/Hair01_J");
				GameObject obj = UnityEngine.Object.Instantiate(_StunAnim);
				obj.name = _StunAnim.name;
				obj.transform.parent = parent;
				obj.transform.localPosition = new Vector3(0f, 0.2f, 0f);
				obj.transform.localRotation = Quaternion.identity;
			}
			PlaySfx(_StateSettings._Stunned._Sfx, inLoop: false);
			PlayAnim(_StateSettings._Stunned._Anim, WrapMode.Loop);
			mTimer = UnityEngine.Random.Range(_StunTime._Min, _StunTime._Max);
		}
		else
		{
			mTimer = UnityEngine.Random.Range(_StunTime._Min, _StunTime._Max);
		}
	}

	protected virtual float DistanceToTarget()
	{
		return (base.transform.position + mController.center - _Target.transform.position).magnitude;
	}

	private void RestorePreviousState()
	{
		pState = mPrevState;
		pSpeed = _WalkSpeed;
		switch (mState)
		{
		case EnEnemyState.IDLE:
			PlayAnim(_StateSettings._Idle._Anim, WrapMode.Loop);
			break;
		case EnEnemyState.PATROL:
		case EnEnemyState.RETURN:
			PlayAnim(_StateSettings._Walk._Anim, WrapMode.Loop);
			break;
		case EnEnemyState.ALERT:
			PlayAnim(_StateSettings._Alert._Anim, WrapMode.Loop);
			break;
		case EnEnemyState.ATTACK:
			PlayAnim(_StateSettings._Attack._Anim, WrapMode.Loop);
			pSpeed = _AttackSpeed;
			break;
		case EnEnemyState.TIRED:
			PlayAnim(_StateSettings._Tired._Anim, WrapMode.Loop);
			break;
		case EnEnemyState.STUNNED:
			PlayAnim(_StateSettings._Stunned._Anim, WrapMode.Loop);
			break;
		case EnEnemyState.CELEBRATE:
			PlayAnim(_StateSettings._Celebrate._Anim, WrapMode.Loop);
			break;
		case EnEnemyState.RUNAWAY:
			RunAway();
			break;
		case EnEnemyState.BLOCK_SHOT:
		case EnEnemyState.TAKEHIT:
		case EnEnemyState.DEATH:
		case EnEnemyState.SHOOT:
			break;
		}
	}

	public void Block(GameObject goProjectile)
	{
		if (!(goProjectile == null) && mState != EnEnemyState.BLOCK_SHOT && mState != EnEnemyState.DEATH && !_IsProjectileVulnerable)
		{
			Vector3 position = goProjectile.transform.position;
			position.y = base.transform.position.y;
			base.transform.LookAt(position);
			mPrevState = mState;
			mState = EnEnemyState.BLOCK_SHOT;
			Vector3 vector = position - base.transform.position;
			PrProjectile prProjectile = goProjectile.GetComponent(typeof(PrProjectile)) as PrProjectile;
			if (prProjectile != null && (double)vector.magnitude <= (double)prProjectile._ProjectileSpeed * 0.2)
			{
				PlayAnim(_StateSettings._BlockShot._Anim, 0f, 0f, WrapMode.Loop, 1f);
			}
			else
			{
				PlayAnim(_StateSettings._BlockShot._Anim, WrapMode.Loop);
			}
			pSpeed = 0f;
		}
	}

	public void UnBlock()
	{
		if (mState != EnEnemyState.DEATH)
		{
			RestorePreviousState();
		}
	}

	private void CheckOnWater(GameObject iGameObject)
	{
		if (!iGameObject || mSubState == EnEnemySubState.ON_WATER)
		{
			return;
		}
		int num = LayerMask.NameToLayer("Water");
		if (iGameObject.layer == num)
		{
			mSubState = EnEnemySubState.ON_WATER;
			if (base.transform.position.y < iGameObject.transform.position.y)
			{
				float y = iGameObject.transform.position.y - base.transform.position.y;
				mController.Move(new Vector3(0f, y, 0f));
			}
		}
	}

	private void OnTriggerEnter(Collider iCollider)
	{
		if (_CanWalkOnWater)
		{
			CheckOnWater(iCollider.gameObject);
		}
	}

	private void OnTriggerStay(Collider iCollider)
	{
		if (_CanWalkOnWater)
		{
			CheckOnWater(iCollider.gameObject);
		}
	}

	private void OnTriggerExit(Collider iCollider)
	{
		if (mSubState == EnEnemySubState.ON_WATER)
		{
			mSubState = EnEnemySubState.NORMAL;
		}
	}

	private void OnCollisionEnter(Collision iCollision)
	{
		if (iCollision.transform.CompareTag("Projectile") && _IsProjectileVulnerable)
		{
			PrProjectile prProjectile = iCollision.gameObject.GetComponent(typeof(PrProjectile)) as PrProjectile;
			int damage = 1;
			if ((bool)prProjectile)
			{
				damage = prProjectile._Damage;
			}
			if (!prProjectile || prProjectile.pProjectileSource != base.gameObject)
			{
				TakeHit(damage);
			}
		}
	}

	public override void OnDrawGizmos()
	{
		if (_Draw)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(base.transform.position, _AlertRadius);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(base.transform.position, _AttackRadius);
		}
		base.OnDrawGizmos();
	}

	private bool FindPath(Vector3 start, Vector3 end)
	{
		Spline spline = new Spline(2, looping: false, constSpeed: true, alignTangent: true, hasQ: false);
		spline.SetControlPoint(0, start, Quaternion.identity, 0f);
		spline.SetControlPoint(1, end, Quaternion.identity, 0f);
		spline.RecalculateSpline();
		SetSpline(spline);
		Looping = false;
		return true;
	}

	public void PlaySfx(AudioClip[] snd, bool inLoop)
	{
		if (!(mSndChannel == null) && snd != null && snd.Length != 0)
		{
			AudioClip[] pClipQueue = new AudioClip[1] { snd[UnityEngine.Random.Range(0, snd.Length)] };
			mSndChannel.pClipQueue = pClipQueue;
			mSndChannel.pAudioSource.loop = inLoop;
			mSndChannel.Play();
			mPlayingAttackSfx = false;
		}
	}

	private void StopSfx()
	{
		if (!(mSndChannel == null))
		{
			mSndChannel.Stop();
		}
	}

	public void DisableSound(bool inDisable)
	{
		if (!(mSndChannel == null))
		{
			mSndChannel.enabled = !inDisable;
			mSndChannel.pVolume = 0f;
		}
	}

	public void PlayAnim(string inAnimName, WrapMode inWrapMode)
	{
		UpdateFace(inAnimName);
		AvAvatarAnimation avAvatarAnimation = new AvAvatarAnimation(inAnimName);
		avAvatarAnimation.mWrapMode = inWrapMode;
		Animate(avAvatarAnimation);
	}

	public void PlayAnim(string inAnimName, float offset, float fadelength, WrapMode wrapmode, float speed)
	{
		UpdateFace(inAnimName);
		AvAvatarAnimation inSettings = new AvAvatarAnimation(inAnimName, offset, fadelength, wrapmode, speed);
		Animate(inSettings);
	}

	public void Animate(AvAvatarAnimation inSettings)
	{
		if (!(base.animation[inSettings.mName] == null))
		{
			if (inSettings.mFadeLength > 0f && inSettings.IsEnabled(2u))
			{
				base.animation.CrossFade(inSettings.mName, inSettings.mFadeLength);
			}
			else
			{
				base.animation.Play(inSettings.mName);
			}
			if (inSettings.IsEnabled(4u))
			{
				base.animation[inSettings.mName].wrapMode = inSettings.mWrapMode;
			}
			if (inSettings.IsEnabled(8u))
			{
				base.animation[inSettings.mName].speed = inSettings.mSpeed;
			}
			if (inSettings.IsEnabled(1u))
			{
				base.animation[inSettings.mName].time = inSettings.mOffset * base.animation[inSettings.mName].length;
			}
		}
	}

	public void UpdateFace(string inAnimName)
	{
		if (!Application.isPlaying || _FaceMaterial == null)
		{
			return;
		}
		string text = _FaceMaterial.name + " (Instance)";
		Material material = null;
		Component[] componentsInChildren = base.transform.GetComponentsInChildren(typeof(Renderer));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Renderer renderer = (Renderer)componentsInChildren[i];
			for (int j = 0; j < renderer.materials.Length; j++)
			{
				if (renderer.materials[j].name == text)
				{
					material = renderer.materials[j];
					break;
				}
			}
			if ((bool)material)
			{
				break;
			}
		}
		if (material == null)
		{
			return;
		}
		if (inAnimName == _StateSettings._Idle._Anim)
		{
			if (_StateSettings._Idle._Face != null)
			{
				material.SetTexture("_MainTex", _StateSettings._Idle._Face);
			}
		}
		else if (inAnimName == _StateSettings._Walk._Anim)
		{
			if (_StateSettings._Walk._Face != null)
			{
				material.SetTexture("_MainTex", _StateSettings._Walk._Face);
			}
		}
		else if (inAnimName == _StateSettings._Alert._Anim)
		{
			if (_StateSettings._Alert._Face != null)
			{
				material.SetTexture("_MainTex", _StateSettings._Alert._Face);
			}
		}
		else if (inAnimName == _StateSettings._Attack._Anim)
		{
			if (_StateSettings._Attack._Face != null)
			{
				material.SetTexture("_MainTex", _StateSettings._Attack._Face);
			}
		}
		else if (inAnimName == _StateSettings._Tired._Anim)
		{
			if (_StateSettings._Tired._Face != null)
			{
				material.SetTexture("_MainTex", _StateSettings._Tired._Face);
			}
		}
		else if (inAnimName == _StateSettings._Stunned._Anim)
		{
			if (_StateSettings._Stunned._Face != null)
			{
				material.SetTexture("_MainTex", _StateSettings._Stunned._Face);
			}
		}
		else if (inAnimName == _StateSettings._TakeHit._Anim)
		{
			if (_StateSettings._TakeHit._Face != null)
			{
				material.SetTexture("_MainTex", _StateSettings._TakeHit._Face);
			}
		}
		else if (inAnimName == _StateSettings._Death._Anim)
		{
			if (_StateSettings._Death._Face != null)
			{
				material.SetTexture("_MainTex", _StateSettings._Death._Face);
			}
		}
		else if (inAnimName == _StateSettings._Load._Anim)
		{
			if (_StateSettings._Load._Face != null)
			{
				material.SetTexture("_MainTex", _StateSettings._Load._Face);
			}
		}
		else if (inAnimName == _StateSettings._Shoot._Anim)
		{
			if (_StateSettings._Shoot._Face != null)
			{
				material.SetTexture("_MainTex", _StateSettings._Shoot._Face);
			}
		}
		else if (inAnimName == _StateSettings._ShootIdle._Anim)
		{
			if (_StateSettings._ShootIdle._Face != null)
			{
				material.SetTexture("_MainTex", _StateSettings._ShootIdle._Face);
			}
		}
		else if (inAnimName == _StateSettings._Celebrate._Anim && _StateSettings._Celebrate._Face != null)
		{
			material.SetTexture("_MainTex", _StateSettings._Celebrate._Face);
		}
	}

	public void SetWalkSpeed(float inSpeed)
	{
		_WalkSpeed = inSpeed;
		pSpeed = inSpeed;
	}

	private void RunAway()
	{
		mState = EnEnemyState.RUNAWAY;
		mRunAwayPathRegenTimer = _RunAwayPathRegenTime;
		mRespawnTimer = _RespawnTime;
		if (FindPath(base.transform.position, GetRandomRunawayPosition()))
		{
			pSpeed = _WalkSpeed;
			PlayAnim(_StateSettings._Walk._Anim, 0f, 0.2f, WrapMode.Loop, _WalkAnimScale * _WalkSpeed);
			StopSfx();
		}
		else
		{
			mRunAwayPathRegenTimer = 0f;
			UtDebug.LogError("NO Path found");
		}
	}

	private Vector3 GetRandomRunawayPosition()
	{
		Vector3 position = base.transform.position;
		Vector3 vector = base.transform.position - _Target.transform.position;
		vector.Normalize();
		position = base.transform.position + vector * _RunAwayDistance;
		float num = UnityEngine.Random.Range(0f - _RunAwayArcDegree, _RunAwayArcDegree);
		num *= MathF.PI / 180f;
		position.x *= Mathf.Cos(num);
		position.z *= Mathf.Sin(num);
		return position;
	}

	private void SetVisibility(bool visible)
	{
		Component[] componentsInChildren = GetComponentsInChildren(typeof(Renderer));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Renderer)componentsInChildren[i]).enabled = visible;
		}
	}

	private void SetFadeState(FadeState inState)
	{
		if (mState != 0)
		{
			mFadeState = inState;
		}
	}

	private void Fade()
	{
		if (mFadeState == FadeState.FADEOUT)
		{
			mFadeOutTimer -= Time.deltaTime;
			if (mFadeOutTimer <= 0f)
			{
				mFadeState = FadeState.NONE;
				SetVisibility(visible: false);
				mState = EnEnemyState.NONE;
				mPrevState = EnEnemyState.NONE;
				base.transform.position = mOriginalPosition;
			}
			else
			{
				Component[] componentsInChildren = GetComponentsInChildren(typeof(Renderer));
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					Renderer obj = (Renderer)componentsInChildren[i];
					obj.enabled = !obj.enabled;
				}
			}
		}
		else
		{
			if (mFadeState != FadeState.FADEIN)
			{
				return;
			}
			mFadeOutTimer += Time.deltaTime;
			if (mFadeOutTimer >= _FadeOutTime)
			{
				mFadeOutTimer = _FadeOutTime;
				mFadeState = FadeState.NONE;
				SetVisibility(visible: true);
				return;
			}
			Component[] componentsInChildren = GetComponentsInChildren(typeof(Renderer));
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Renderer obj2 = (Renderer)componentsInChildren[i];
				obj2.enabled = !obj2.enabled;
			}
		}
	}

	public void OnEnemyRespawn()
	{
		if (_CanRunAway)
		{
			SetFadeState(FadeState.FADEOUT);
			mFadeOutTimer = 0f;
			mRespawnTimer = 0f;
		}
		else
		{
			OnRestart(resetHitPoints: true);
		}
	}

	protected virtual void GenerateCoins(int numCoinsToEmit)
	{
		ObBouncyCoinEmitter component = GetComponent<ObBouncyCoinEmitter>();
		if (component != null)
		{
			component._CoinsToEmit._Min = (component._CoinsToEmit._Max = numCoinsToEmit);
			component.GenerateCoins();
		}
	}
}
