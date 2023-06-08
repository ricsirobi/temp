using System;
using DG.Tweening;
using SWS;
using UnityEngine;

public class BattleEnemy : MonoBehaviour
{
	[Serializable]
	public class DamageEffect
	{
		public float _Percentage;

		public GameObject _Particles;

		public Transform[] _Position;

		[HideInInspector]
		public bool _IsEffectOn;
	}

	public DamageEffect[] _DamageLevels;

	public LocaleString _NameText = new LocaleString("Scout unit");

	public CampSite[] _SafeZones;

	public SWS.PathManager _SplinePath;

	public static BattleEnemy pInstance;

	public float _DurationSeconds = 5f;

	public int _HealthMax;

	public float[] _HealthReductionOnRespawn;

	public BattleEnemyTarget _BattleEnemyTarget;

	public float _CameraShakeAmount = 0.17f;

	public float _CameraShakeDuration = 0.2f;

	public float _CameraShakeDecay = 1f;

	public float _HealthBarFlashDuration = 1f;

	public float _HealthBarFlashRate = 0.25f;

	public GameObject _RespawnParticles;

	public string _RespawnAnimState = "KnockOut";

	public AudioClip _PlayerHealthZeroSFX;

	public float _MaxWaitTimeForRewards = 5f;

	public AvAvatarStats _NormalRegenStats;

	private AvAvatarController mAvatarController;

	private KAWidget mHealthbar;

	private float mLastFlash;

	private float mFlashTimer;

	private int mRespawnCount;

	private bool mDismountPet;

	private float mDismountTime;

	private const float mDelayInDismount = 0.15f;

	private AvAvatarAnim mAvatarAnim;

	private float mRewardWaitTimer;

	private float mCachedAvatarHealthRegenRate;

	protected virtual void Awake()
	{
		pInstance = this;
	}

	protected virtual void Start()
	{
		splineMove component = GetComponent<splineMove>();
		if (null == component)
		{
			component = base.gameObject.AddComponent<splineMove>();
			if (null != component)
			{
				component.onStart = false;
				component.moveToPath = false;
				component.timeValue = splineMove.TimeValue.time;
				component.pathType = PathType.CatmullRom;
				component.lockRotation = AxisConstraint.X;
				component.ChangeSpeed(_DurationSeconds);
				component.SetPath(_SplinePath);
				component.reverse = true;
				component.loopType = splineMove.LoopType.loop;
				component.tween.fullPosition = 0f;
				component.Resume();
			}
		}
		BattleEnemyTarget battleEnemyTarget = _BattleEnemyTarget;
		battleEnemyTarget.OnTargetHealthUpdate = (BattleEnemyTarget.OnHealthUpdate)Delegate.Combine(battleEnemyTarget.OnTargetHealthUpdate, new BattleEnemyTarget.OnHealthUpdate(OnHealthChange));
	}

	protected virtual void Update()
	{
		if (mAvatarController == null && AvAvatar.pObject != null)
		{
			mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
			AvAvatarController avAvatarController = mAvatarController;
			avAvatarController.OnAvatarHealth = (AvAvatarController.OnAvatarHealthDelegate)Delegate.Combine(avAvatarController.OnAvatarHealth, new AvAvatarController.OnAvatarHealthDelegate(PlayerHealthOnAmmoHit));
			mCachedAvatarHealthRegenRate = mAvatarController._Stats._HealthRegenRate;
			mAvatarController._Stats._HealthRegenRate = _NormalRegenStats._HealthRegenRate;
		}
		if (mDismountPet && Time.realtimeSinceStartup > mDismountTime)
		{
			DismountPet();
			mDismountPet = false;
		}
		if (mFlashTimer <= _HealthBarFlashDuration)
		{
			if (Time.time - mLastFlash >= _HealthBarFlashRate)
			{
				if (mHealthbar != null)
				{
					mHealthbar.SetVisibility(!mHealthbar.GetVisibility());
				}
				mLastFlash = Time.time;
				mFlashTimer += _HealthBarFlashRate;
			}
		}
		else
		{
			mFlashTimer = float.MaxValue;
			mLastFlash = 0f;
			if (mHealthbar != null)
			{
				mHealthbar.SetVisibility(inVisible: true);
			}
		}
		if (mRewardWaitTimer > 0f)
		{
			mRewardWaitTimer -= Time.deltaTime;
			if (mRewardWaitTimer < 0f)
			{
				MissionManager.pInstance.Collect(base.gameObject);
			}
		}
	}

	private void DismountPet()
	{
		SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
		ShowRespawnEffects();
	}

	private void OnHealthChange(float healthPercentage)
	{
		DamageEffect[] damageLevels = _DamageLevels;
		foreach (DamageEffect damageEffect in damageLevels)
		{
			if (damageEffect._IsEffectOn || !(healthPercentage <= damageEffect._Percentage))
			{
				continue;
			}
			Transform[] position = damageEffect._Position;
			foreach (Transform transform in position)
			{
				if (transform != null && damageEffect._Particles != null)
				{
					UnityEngine.Object.Instantiate(damageEffect._Particles, transform.position, Quaternion.identity).transform.parent = base.gameObject.transform;
				}
			}
			damageEffect._IsEffectOn = true;
		}
		if (healthPercentage <= 0f)
		{
			mRewardWaitTimer = _MaxWaitTimeForRewards;
			_BattleEnemyTarget.Kill();
		}
	}

	private void PlayerHealthOnAmmoHit(float currentHealth)
	{
		if ((SanctuaryManager.pCurPetInstance == null && currentHealth <= 0f) || (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.HEALTH) <= SanctuaryManager.pCurPetInstance._MinPetMeterValue))
		{
			OnPlayerHealthZero();
		}
		if (null != AvAvatar.pAvatarCam)
		{
			CaAvatarCam component = AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>();
			if (null != component)
			{
				component.Shake(_CameraShakeAmount, _CameraShakeDuration, _CameraShakeDecay);
			}
		}
		if (null == mHealthbar)
		{
			UiToolbar component2 = AvAvatar.pToolbar.GetComponent<UiToolbar>();
			if (null != component2)
			{
				mHealthbar = component2.FindItem("MeterBarHPs");
			}
		}
		if (null != mHealthbar && mLastFlash <= 0f)
		{
			mFlashTimer = 0f;
		}
	}

	private void OnPlayerHealthZero()
	{
		SnChannel.Play(_PlayerHealthZeroSFX);
		mAvatarController.pCanRegenHealth = false;
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
		{
			SanctuaryManager.pCurPetInstance.OnFlyLanding(AvAvatar.pObject);
			mDismountPet = true;
			mDismountTime = Time.realtimeSinceStartup + 0.15f;
		}
		else
		{
			if (mAvatarController != null && mAvatarController.pIsPlayerGliding)
			{
				mAvatarController.OnGlideLanding();
			}
			else
			{
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.pSubState = AvAvatarSubState.NORMAL;
			}
			ShowRespawnEffects();
		}
		AvAvatar.pInputEnabled = false;
	}

	private void ShowRespawnEffects()
	{
		if (mAvatarAnim == null)
		{
			mAvatarAnim = AvAvatar.pObject.GetComponentInChildren<AvAvatarAnim>();
		}
		if (mAvatarAnim == null)
		{
			UtDebug.Log("AvAvatar Anim not found in PfAvatar");
			OnRespawnAnimEnd(_RespawnAnimState);
			return;
		}
		mAvatarAnim.PlayCannedAnim(_RespawnAnimState, bQuitOnMovement: false, bQuitOnEnd: true, bFreezePlayer: false, OnRespawnAnimEnd);
		if (UtPlatform.IsWSA())
		{
			UtUtilities.ReAssignShader(UnityEngine.Object.Instantiate(_RespawnParticles, AvAvatar.pObject.transform.position, AvAvatar.pObject.transform.rotation));
		}
		else
		{
			UnityEngine.Object.Instantiate(_RespawnParticles, AvAvatar.pObject.transform.position, AvAvatar.pObject.transform.rotation);
		}
	}

	private void OnRespawnAnimEnd(string animName)
	{
		if (!(animName != _RespawnAnimState))
		{
			int num = ((mRespawnCount < _HealthReductionOnRespawn.Length) ? mRespawnCount : (_HealthReductionOnRespawn.Length - 1));
			if (SanctuaryManager.pCurPetInstance != null)
			{
				float maxMeter = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.HEALTH, SanctuaryManager.pCurPetInstance.pData);
				SanctuaryManager.pCurPetInstance.SetMeter(SanctuaryPetMeterType.HEALTH, _HealthReductionOnRespawn[num] * maxMeter);
			}
			else
			{
				mAvatarController._Stats._CurrentHealth = _HealthReductionOnRespawn[num] * mAvatarController._Stats._MaxHealth;
			}
			mRespawnCount++;
			mAvatarController.pCanRegenHealth = true;
			AvAvatar.pInputEnabled = true;
		}
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

	private void OnDestroy()
	{
		BattleEnemyTarget battleEnemyTarget = _BattleEnemyTarget;
		battleEnemyTarget.OnTargetHealthUpdate = (BattleEnemyTarget.OnHealthUpdate)Delegate.Remove(battleEnemyTarget.OnTargetHealthUpdate, new BattleEnemyTarget.OnHealthUpdate(OnHealthChange));
		mAvatarController._Stats._HealthRegenRate = mCachedAvatarHealthRegenRate;
		AvAvatarController avAvatarController = mAvatarController;
		avAvatarController.OnAvatarHealth = (AvAvatarController.OnAvatarHealthDelegate)Delegate.Remove(avAvatarController.OnAvatarHealth, new AvAvatarController.OnAvatarHealthDelegate(PlayerHealthOnAmmoHit));
	}
}
