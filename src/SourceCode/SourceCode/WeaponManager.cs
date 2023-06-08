using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : KAMonoBase
{
	[Serializable]
	public class WeaponData
	{
		public string name;

		public WeaponData(string inName)
		{
			name = inName;
		}
	}

	public delegate void WeaponChanged();

	public delegate void AvailableShotUpdated(int count);

	private const char MESSAGE_SEPARATOR = ':';

	private const string WEAPON_HIT = "WH";

	private const string WEAPON_FIRED = "WF";

	private const string WEAPON_FIRED_WITH_TARGET = "WFWT";

	private const float CENTER_WEIGHT = 2f;

	private const float TARGET_WINDOW_WIDTH = 0.8f;

	private const float TARGET_WINDOW_HEIGHT = 0.8f;

	private const float TARGET_WINDOW_CENTER = 0f;

	public WeaponChanged OnWeaponChanged;

	public AvailableShotUpdated OnAvailableShotUpdated;

	public WeaponData _MainWeapon = new WeaponData("Fireball");

	protected WeaponTuneData.Weapon CurrentWeapon;

	public int _DefaultTotalShots = 4;

	public float _DefaultCoolDown = 1.5f;

	public MinMax _DefaultRechargeRate = new MinMax(1f, 4f);

	protected float mCooldownTimer = -1f;

	private bool mUserControlledWeapon;

	public bool IsLocal;

	public string _AmmoPoolName;

	protected SpawnPool mAmmoPool;

	protected Transform mTarget;

	protected List<Transform> mTargetList;

	private Bounds mTargetWindowBounds;

	private float mTargetEvalTimer;

	private float mTargetEvalFreq;

	private float mPrevShotTimer = 999f;

	private Vector2 mReticleDrift = new Vector2(0f, 0f);

	private Transform mSnapTransform;

	private WeaponTuneData.ParticleInfo mParticleInfo;

	private AvAvatarLevelState mAvatarLevelState;

	public string _ShootPoint;

	[HideInInspector]
	public Transform ShootPointTrans;

	public int _NumShotsForNoTarget = 1;

	public bool _EnableFireTowardsCenter;

	public float _FireCoolDownRegenRate;

	public Transform _ShootingPointController;

	public List<Transform> _ShootingPoints = new List<Transform>();

	public List<Transform> _SpecialAttackShootingPoints = new List<Transform>();

	protected int mPhysicsLayerMask;

	private bool mLoggedNullException;

	private int mProjectileIndex;

	protected Transform mCurrentTarget;

	private GameObject mFiredMessageObject;

	public float pCooldownTimer => mCooldownTimer;

	public bool pUserControlledWeapon
	{
		get
		{
			return mUserControlledWeapon;
		}
		set
		{
			mUserControlledWeapon = value;
		}
	}

	public Transform pCurrentTarget
	{
		get
		{
			return mCurrentTarget;
		}
		set
		{
			mCurrentTarget = value;
		}
	}

	public GameObject pFiredMessageObject
	{
		set
		{
			mFiredMessageObject = value;
		}
	}

	public float pRange => GetCurrentWeapon()?._Range ?? 0f;

	public List<Transform> pTargetList
	{
		get
		{
			return mTargetList;
		}
		set
		{
			mTargetList = value;
		}
	}

	public Vector3 pShootPoint
	{
		get
		{
			if (!(ShootPointTrans == null))
			{
				return ShootPointTrans.position;
			}
			return base.transform.position;
		}
	}

	protected virtual void Awake()
	{
		IsLocal = false;
		if (_ShootingPoints == null)
		{
			_ShootingPoints = new List<Transform>();
		}
		if (_ShootingPoints.Count == 0)
		{
			if (!string.IsNullOrEmpty(_ShootPoint))
			{
				ShootPointTrans = UtUtilities.FindChildTransform(base.gameObject, _ShootPoint);
			}
			if (ShootPointTrans == null)
			{
				ShootPointTrans = base.transform;
			}
			_ShootingPoints.Add(ShootPointTrans);
		}
		else if (_ShootingPointController != null)
		{
			ShootPointTrans = _ShootingPointController;
		}
		else
		{
			ShootPointTrans = _ShootingPoints[0];
		}
		mPhysicsLayerMask = (1 << LayerMask.NameToLayer("IgnoreGroundRay")) | (1 << LayerMask.NameToLayer("Targetable"));
		mTargetWindowBounds = new Bounds(Vector3.zero, Vector3.zero);
		mTargetEvalFreq = 0.1f;
		mTargetEvalTimer = 0f;
		PoolManager.Pools.TryGetValue(_AmmoPoolName, out mAmmoPool);
	}

	protected virtual void Update()
	{
		mCooldownTimer -= Time.deltaTime;
		if (mCooldownTimer < 0f)
		{
			mCooldownTimer = 0f;
		}
		mPrevShotTimer += Time.deltaTime;
		if (WeaponTuneData.pInstance == null)
		{
			if (!mLoggedNullException)
			{
				mLoggedNullException = true;
				UtDebug.LogError("Weapon Tune Data is null:" + base.gameObject.name);
			}
		}
		else
		{
			if (!mUserControlledWeapon)
			{
				return;
			}
			float axis = KAInput.GetAxis("Horizontal");
			float axis2 = KAInput.GetAxis("Vertical");
			mReticleDrift.x += axis * Time.deltaTime * WeaponTuneData.pInstance._AimedReticle._Speed.x;
			mReticleDrift.y += axis2 * Time.deltaTime * WeaponTuneData.pInstance._AimedReticle._Speed.y;
			if (mSnapTransform == null)
			{
				float num = Mathf.Clamp(WeaponTuneData.pInstance._AimedReticle._CenterSpring, 0f, 1f);
				mReticleDrift *= 1f - num;
				mReticleDrift.x = Mathf.MoveTowards(mReticleDrift.x, 0f, WeaponTuneData.pInstance._AimedReticle._CenterSpeed.x);
				mReticleDrift.y = Mathf.MoveTowards(mReticleDrift.y, 0f, WeaponTuneData.pInstance._AimedReticle._CenterSpeed.y);
			}
			Vector2 limits = WeaponTuneData.pInstance._AimedReticle._Limits;
			limits.x = limits.x / 100f * (float)Screen.width;
			limits.y = limits.y / 100f * (float)Screen.height;
			mReticleDrift.x = Mathf.Clamp(mReticleDrift.x, 0f - limits.x, limits.x);
			mReticleDrift.y = Mathf.Clamp(mReticleDrift.y, 0f - limits.y, limits.y);
			if (!(SanctuaryManager.pCurPetInstance != null))
			{
				return;
			}
			EvaluateTargets();
			if (!(SanctuaryManager.pCurPetInstance.pWeaponManager == this) || !KAInput.GetButtonDown("TargetSwitch") || pTargetList == null)
			{
				return;
			}
			for (int i = 0; i < pTargetList.Count; i++)
			{
				if (pTargetList[i] == mCurrentTarget)
				{
					if (i + 1 < pTargetList.Count)
					{
						mCurrentTarget = pTargetList[i + 1];
						break;
					}
					mCurrentTarget = pTargetList[0];
				}
			}
		}
	}

	private void UpdateAimingTarget()
	{
		mTargetEvalTimer -= Time.deltaTime;
		if (mTargetEvalTimer <= 0f)
		{
			mTargetEvalTimer = mTargetEvalFreq;
			if (UiAvatarControls.pInstance != null)
			{
				SetTargetList(null);
				if (UiAvatarControls.pInstance != null)
				{
					UiAvatarControls.pInstance.ClearPreviousReticles(null);
				}
				List<Transform> allTargets = GetAllTargets();
				Vector2 baseReticlePos = GetBaseReticlePos();
				float num = 0f;
				Transform transform = null;
				foreach (Transform item in allTargets)
				{
					if (item == null || !Physics.Raycast(new Ray(pShootPoint, (item.transform.position - pShootPoint).normalized), out var hitInfo))
					{
						continue;
					}
					Collider component = item.GetComponent<Collider>();
					if (hitInfo.collider == component)
					{
						float magnitude = ((Vector2)Camera.main.WorldToScreenPoint(component.bounds.center) - baseReticlePos).magnitude;
						if (transform == null || magnitude < num)
						{
							num = magnitude;
							transform = item.transform;
						}
					}
				}
				float num2 = WeaponTuneData.pInstance._AimedReticle._SnapThreshold * (float)Screen.width;
				mSnapTransform = ((num < num2) ? transform : null);
			}
		}
		Vector2 reticlePos = GetReticlePos();
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.UpdateReticle(onTarget: true, new Vector3(reticlePos.x, reticlePos.y, 0f), base.gameObject.transform, 0f);
		}
	}

	private void UpdateHomingTargets()
	{
		mTargetEvalTimer -= Time.deltaTime;
		if (!(mTargetEvalTimer <= 0f))
		{
			return;
		}
		mTargetEvalTimer = mTargetEvalFreq;
		List<Transform> allTargets = GetAllTargets();
		if (allTargets.Count > 0)
		{
			Vector3 myPosition = base.transform.position;
			Vector3 myForward = Camera.main.transform.forward;
			allTargets.Sort(delegate(Transform t1, Transform t2)
			{
				Vector3 lhs = t1.position - myPosition;
				Vector3 lhs2 = t2.position - myPosition;
				float num = 1f - lhs.magnitude / pRange;
				float num2 = 1f - lhs2.magnitude / pRange;
				lhs.Normalize();
				lhs2.Normalize();
				float value = num + Vector3.Dot(lhs, myForward) * 2f;
				return (num2 + Vector3.Dot(lhs2, myForward) * 2f).CompareTo(value);
			});
			if (allTargets[0].GetComponent<ObTargetable>()._IsMultiTarget)
			{
				List<Transform> list = new List<Transform>(allTargets);
				allTargets.Clear();
				foreach (Transform item in list)
				{
					if (item.GetComponent<ObTargetable>()._IsMultiTarget)
					{
						allTargets.Add(item);
					}
				}
			}
			else if (mCurrentTarget == null || !mTargetList.Contains(mCurrentTarget) || mTargetList.Count <= 1)
			{
				mCurrentTarget = mTargetList[0];
				if (!allTargets.Contains(mCurrentTarget))
				{
					allTargets.Add(mCurrentTarget);
				}
			}
		}
		else
		{
			mCurrentTarget = null;
		}
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.ClearPreviousReticles(allTargets);
		}
		SetTargetList(allTargets);
	}

	public virtual bool FireWeapon(string weaponName, Transform target, bool useDirection, Vector3 direction, float parentSpeed)
	{
		WeaponTuneData.Weapon currentWeapon = CurrentWeapon;
		SetWeapon(weaponName);
		bool result = Fire(target, useDirection, direction, parentSpeed);
		CurrentWeapon = currentWeapon;
		return result;
	}

	public float GetWeaponEnergy()
	{
		if (CurrentWeapon == null)
		{
			return 1f;
		}
		return CurrentWeapon._Energy;
	}

	public virtual bool Fire(Transform target, bool useDirection, Vector3 direction, float parentSpeed)
	{
		if (IsLocal && mCooldownTimer > Mathf.Epsilon)
		{
			return false;
		}
		bool isPowerShot = false;
		if (target != null)
		{
			StartFire(target, useDirection, direction, parentSpeed, isPowerShot: false);
		}
		else if (mCurrentTarget != null)
		{
			StartFire(mCurrentTarget, useDirection, direction, parentSpeed, isPowerShot);
		}
		else if (pTargetList == null || pTargetList.Count == 0)
		{
			for (int i = 0; i < _NumShotsForNoTarget; i++)
			{
				if (_EnableFireTowardsCenter)
				{
					Vector3 forward = base.transform.forward;
					forward.y = 0f;
					StartFire(null, useDirection: true, forward, parentSpeed, isPowerShot);
				}
				else
				{
					StartFire(null, useDirection, direction, parentSpeed, isPowerShot);
				}
			}
		}
		else
		{
			foreach (Transform pTarget in pTargetList)
			{
				StartFire(pTarget, useDirection, direction, parentSpeed, isPowerShot);
			}
		}
		if (mFiredMessageObject != null)
		{
			mFiredMessageObject.SendMessage("OnWeaponFired");
		}
		mPrevShotTimer = 0f;
		return true;
	}

	public virtual void FireProp()
	{
		Fire(null, useDirection: true, base.transform.root.forward, 0f);
	}

	private void StartFire(Transform target, bool useDirection, Vector3 direction, float parentSpeed, bool isPowerShot)
	{
		WeaponTuneData.Weapon currentWeapon = GetCurrentWeapon();
		if (currentWeapon == null)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		foreach (Transform shootingPoint in _ShootingPoints)
		{
			Transform transform = CreateAmmoInstance(target, shootingPoint);
			if (!(transform != null))
			{
				continue;
			}
			ObAmmo component = transform.GetComponent<ObAmmo>();
			if (component != null)
			{
				if (mAmmoPool != null)
				{
					component._AmmoPoolName = _AmmoPoolName;
				}
				if (!useDirection)
				{
					direction = shootingPoint.forward;
				}
				if (mAvatarLevelState != AvAvatar.pLevelState)
				{
					mAvatarLevelState = AvAvatar.pLevelState;
					if (currentWeapon._ParticleInfo != null)
					{
						mParticleInfo = Array.Find(currentWeapon._ParticleInfo, (WeaponTuneData.ParticleInfo x) => x._LevelState == AvAvatar.pLevelState);
					}
				}
				if (mParticleInfo != null)
				{
					transform.localScale = Vector3.one * mParticleInfo._Scale;
				}
				component.Activate(IsLocal, base.transform, this, currentWeapon, currentWeapon._Homing ? target : null, direction, parentSpeed * 1.25f);
			}
			else
			{
				Debug.LogError("Could not find ammo component on " + transform.name);
			}
		}
		ForceCooldown();
		if (!IsLocal || !(MainStreetMMOClient.pInstance != null))
		{
			return;
		}
		string mMOUserName = GetMMOUserName(target);
		string text = ((GetCurrentWeapon() != null) ? GetCurrentWeapon()._Name : "");
		if (target != null)
		{
			if (string.IsNullOrEmpty(mMOUserName))
			{
				mMOUserName = target.name;
			}
			AvAvatar.AddUserEvent("WFWT:" + text + ":" + mMOUserName);
			return;
		}
		Vector3 vector = ((zero == Vector3.zero) ? direction : zero);
		AvAvatar.AddUserEvent("WF:" + text + ":" + vector.x + ":" + vector.y + ":" + vector.z);
	}

	public void EvaluateTargets()
	{
		if (!IsLocal || !(Camera.main != null))
		{
			return;
		}
		WeaponTuneData.Weapon currentWeapon = GetCurrentWeapon();
		if (currentWeapon != null)
		{
			if (currentWeapon._Homing)
			{
				UpdateHomingTargets();
			}
			else
			{
				UpdateAimingTarget();
			}
		}
		else
		{
			SetTargetList(null);
			if (UiAvatarControls.pInstance != null)
			{
				UiAvatarControls.pInstance.ClearPreviousReticles(null);
			}
		}
		if (!(UiAvatarControls.pInstance != null) || pTargetList == null)
		{
			return;
		}
		foreach (Transform pTarget in pTargetList)
		{
			if (pTarget != null)
			{
				float inRotationZSpeed = 0f;
				UiAvatarControls.pInstance.UpdateReticle(onTarget: true, Camera.main.WorldToScreenPoint(pTarget.GetComponent<Collider>().bounds.center), pTarget, inRotationZSpeed);
			}
		}
	}

	public Vector2 GetBaseReticlePos()
	{
		Vector2 screenOffset = WeaponTuneData.pInstance._AimedReticle._ScreenOffset;
		Vector2 vector = new Vector2((float)(Screen.width / 2) + screenOffset.x, (float)(Screen.height / 2) + screenOffset.y);
		Vector2 vector2 = (WeaponTuneData.pInstance._ReticleDrift ? mReticleDrift : Vector2.zero);
		if (UiOptions.pIsFlightInverted)
		{
			vector2.y = 0f - vector2.y;
		}
		return vector + vector2;
	}

	public Vector2 GetReticlePos()
	{
		if (mSnapTransform == null)
		{
			return GetBaseReticlePos();
		}
		return Camera.main.WorldToScreenPoint(mSnapTransform.GetComponent<Collider>().bounds.center);
	}

	public virtual void SetWeapon(string name)
	{
		CurrentWeapon = WeaponTuneData.pInstance.GetWeapon(name);
		if (OnWeaponChanged != null)
		{
			OnWeaponChanged();
		}
	}

	public virtual WeaponTuneData.Weapon GetCurrentWeapon()
	{
		if (CurrentWeapon == null)
		{
			SetWeapon(_MainWeapon.name);
		}
		return CurrentWeapon;
	}

	protected virtual Transform CreateAmmoInstance(Transform target, Transform shootingPoint = null)
	{
		if (shootingPoint == null)
		{
			shootingPoint = ShootPointTrans;
		}
		WeaponTuneData.Weapon currentWeapon = GetCurrentWeapon();
		if (currentWeapon != null)
		{
			if (currentWeapon._ProjectilePrefab == null)
			{
				Debug.LogError("Ammo Prefab is null for weapon : ");
				return null;
			}
			GameObject gameObject = null;
			gameObject = ((mAmmoPool != null) ? mAmmoPool.Spawn(GetProjectilePrefab(currentWeapon), shootingPoint.position, shootingPoint.rotation) : UnityEngine.Object.Instantiate(GetProjectilePrefab(currentWeapon), shootingPoint.position, shootingPoint.rotation));
			if (gameObject != null)
			{
				if (UtPlatform.IsWSA())
				{
					UtUtilities.ReAssignShader(gameObject);
				}
				return gameObject.transform;
			}
		}
		return null;
	}

	public virtual GameObject GetProjectilePrefab(WeaponTuneData.Weapon weapon)
	{
		GameObject projectilePrefab = weapon._ProjectilePrefab;
		weapon.pHitPrefab = weapon._HitPrefab;
		if ((weapon._AdditionalProjectile != null) & (weapon._AdditionalProjectile.Length != 0))
		{
			if (weapon._ProjectileRandom)
			{
				int num = UnityEngine.Random.Range(0, weapon._AdditionalProjectile.Length + 1);
				if (num != 0)
				{
					WeaponTuneData.AdditionalProjectile additionalProjectile = weapon._AdditionalProjectile[num - 1];
					projectilePrefab = additionalProjectile._ProjectilePrefab;
					weapon.pHitPrefab = additionalProjectile._HitPrefab;
				}
			}
			else
			{
				if (mProjectileIndex != 0)
				{
					WeaponTuneData.AdditionalProjectile additionalProjectile2 = weapon._AdditionalProjectile[mProjectileIndex - 1];
					projectilePrefab = additionalProjectile2._ProjectilePrefab;
					weapon.pHitPrefab = additionalProjectile2._HitPrefab;
				}
				if (mProjectileIndex >= weapon._AdditionalProjectile.Length)
				{
					mProjectileIndex = 0;
				}
				else
				{
					mProjectileIndex++;
				}
			}
		}
		return projectilePrefab;
	}

	public virtual GameObject GetWeaponHitPrefab(WeaponTuneData.Weapon weapon)
	{
		if (mParticleInfo != null && !mParticleInfo._UseHitPrefab)
		{
			return null;
		}
		if (!(weapon.pHitPrefab != null))
		{
			return weapon._HitPrefab;
		}
		return weapon.pHitPrefab;
	}

	private string GetMMOUserName(Transform inObject)
	{
		if (inObject == null)
		{
			return string.Empty;
		}
		MMOAvatar component = inObject.root.GetComponent<MMOAvatar>();
		if (component != null)
		{
			return component.pUserID;
		}
		return string.Empty;
	}

	public virtual float GetCooldown()
	{
		float num = ((GetCurrentWeapon() == null) ? _DefaultCoolDown : GetCurrentWeapon()._Cooldown);
		if (num < 0.1f)
		{
			num = 0.1f;
		}
		return num;
	}

	public virtual int GetWeaponTotalShots()
	{
		if (GetCurrentWeapon() != null)
		{
			return GetCurrentWeapon()._TotalShots;
		}
		return _DefaultTotalShots;
	}

	public virtual MinMax GetWeaponRechargeRange()
	{
		if (GetCurrentWeapon() != null)
		{
			return GetCurrentWeapon()._RechargeRange;
		}
		return _DefaultRechargeRate;
	}

	public virtual int GetWeaponShotsAvailable()
	{
		if (GetCurrentWeapon() != null)
		{
			return GetCurrentWeapon()._AvailableShots;
		}
		return 0;
	}

	public virtual void UpdateShotsAvailable(int shots)
	{
		WeaponTuneData.Weapon currentWeapon = GetCurrentWeapon();
		if (currentWeapon != null)
		{
			currentWeapon._AvailableShots = shots;
			if (OnAvailableShotUpdated != null)
			{
				OnAvailableShotUpdated(currentWeapon._AvailableShots);
			}
		}
	}

	public virtual void ForceCooldown()
	{
		mCooldownTimer = GetCooldown();
	}

	public virtual void ResetCooldown()
	{
		mCooldownTimer = 0f;
	}

	public virtual void SetTargetList(List<Transform> inTargetList)
	{
		pTargetList = inTargetList;
	}

	private List<Transform> GetAllTargets()
	{
		float num = (float)Screen.width * 0.8f;
		float num2 = (float)Screen.height * 0.8f;
		float num3 = (float)(Screen.width / 2) - num / 2f;
		float num4 = (float)(Screen.height / 2) - num2 / 2f - (float)Screen.height * 0f;
		mTargetWindowBounds.SetMinMax(new Vector3(num3, (float)Screen.height - (num4 + num2), 0.1f), new Vector3(num3 + num, (float)Screen.height - num4, 1000f));
		List<Transform> list = new List<Transform>();
		Collider[] array = Physics.OverlapSphere(base.transform.position, pRange, mPhysicsLayerMask);
		foreach (Collider collider in array)
		{
			if (!list.Contains(collider.transform) && IsValidTarget(collider.transform))
			{
				list.Add(collider.transform);
			}
		}
		if (AvAvatar.pLevelState == AvAvatarLevelState.RACING && MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.pPlayerList != null)
		{
			foreach (KeyValuePair<string, MMOAvatar> pPlayer in MainStreetMMOClient.pInstance.pPlayerList)
			{
				if (IsValidTarget(pPlayer.Value.gameObject.transform))
				{
					list.Add(pPlayer.Value.gameObject.transform);
				}
			}
		}
		mTargetList = list;
		return mTargetList;
	}

	public virtual bool IsValidTarget(Transform inTarget)
	{
		if (inTarget != null && inTarget.gameObject != AvAvatar.pObject && Vector3.Dot(base.transform.forward, inTarget.position - base.transform.position) > 0f)
		{
			ObTargetable component = inTarget.GetComponent<ObTargetable>();
			if (component != null && component._Active && (inTarget.transform.position - base.gameObject.transform.position).magnitude <= pRange)
			{
				Vector3 point = Camera.main.WorldToScreenPoint(inTarget.position);
				if (mTargetWindowBounds.Contains(point))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool SphereToSphereCollide(Vector3 P1, float Pr, Vector3 Pv, Vector3 Q1, float Qr, Vector3 Qv, float maxTime)
	{
		Vector3 vector = P1 + Pv * maxTime;
		Vector3 vector2 = Q1 + Qv * maxTime;
		Vector3 vector3 = P1 - Q1;
		Vector3 vector4 = vector - P1 - (vector2 - Q1);
		float num = Vector3.Dot(vector3, vector3);
		float num2 = Vector3.Dot(vector4, vector4);
		float num3 = Vector3.Dot(vector3, vector4);
		return num - num3 * num3 / num2 < (Pr + Qr) * (Pr + Qr);
	}
}
