using System;
using UnityEngine;

public class WeaponTuneData : MonoBehaviour
{
	[Serializable]
	public enum AmmoType
	{
		FIRE,
		ICE,
		ELECTRIC
	}

	[Serializable]
	public class ParticleInfo
	{
		public AvAvatarLevelState _LevelState = AvAvatarLevelState.TARGETPRACTICE;

		public float _Scale = 1f;

		public bool _UseHitPrefab;
	}

	[Serializable]
	public class AdditionalProjectile
	{
		public GameObject _ProjectilePrefab;

		public GameObject _HitPrefab;
	}

	[Serializable]
	public class Weapon
	{
		public string _Name;

		public float _MinSpeed;

		public float _MaxTimeToTarget;

		public int _Damage;

		public float _CriticalDamageMultiplier = 1f;

		public int _CriticalDamageChance = 20;

		public float _Energy;

		public float _TouchRadius;

		public float _Lifetime;

		public int _TotalShots;

		public float _Cooldown;

		public float _Range;

		public bool _Homing;

		public bool _SlowEffect;

		public GameObject _ProjectilePrefab;

		public GameObject _HitPrefab;

		public AdditionalProjectile[] _AdditionalProjectile;

		public bool _ProjectileRandom;

		public AudioClip _HitSound;

		public GameObject _HitSoundSourcePrefab;

		public Color _FireTint;

		public AvAvatarLevelState[] _AvatarNonImmuneLevelStates;

		public AmmoType _AmmoType;

		public MinMax _RechargeRange;

		public ParticleInfo[] _ParticleInfo;

		[HideInInspector]
		public int _AvailableShots;

		public GameObject pHitPrefab { get; set; }
	}

	[Serializable]
	public class AimedReticle
	{
		public Vector2 _ScreenOffset = Vector2.zero;

		public Vector2 _Limits = new Vector2(25f, 25f);

		public Vector2 _Speed = new Vector2(250f, 250f);

		public Vector2 _CenterSpeed = Vector2.zero;

		public float _CenterSpring = 0.01f;

		public float _SnapThreshold = 0.05f;
	}

	[Serializable]
	public class WeaponEffect
	{
		public float _Strength = 1f;

		public float _Time = 3f;
	}

	public Weapon[] _Weapons;

	public WeaponEffect _SlowEffect;

	public AimedReticle _AimedReticle;

	public bool _AimedShotForRacing;

	public bool _ReticleDrift = true;

	private string[] WeaponNames;

	private int CachedLength;

	private static WeaponTuneData mInstance;

	public static WeaponTuneData pInstance
	{
		get
		{
			if (mInstance == null)
			{
				GameObject gameObject = RsResourceManager.LoadAssetFromResources("PfWeaponTuneData") as GameObject;
				if (gameObject != null)
				{
					mInstance = gameObject.GetComponent<WeaponTuneData>();
					if (mInstance != null)
					{
						InitializeWeaponShots(mInstance._Weapons);
					}
					else
					{
						UtDebug.LogError("Weapon Instance is NULL");
					}
				}
				else
				{
					UtDebug.LogError("weaponObject from PfWeaponTuneData did not get loaded");
				}
			}
			return mInstance;
		}
	}

	public string[] GetWeaponNames()
	{
		if ((_Weapons != null && WeaponNames == null) || CachedLength != _Weapons.Length)
		{
			WeaponNames = new string[_Weapons.Length];
			for (int i = 0; i < _Weapons.Length; i++)
			{
				WeaponNames[i] = _Weapons[i]._Name;
			}
		}
		return WeaponNames;
	}

	public int FindWeaponIndex(string weaponName)
	{
		if (_Weapons != null)
		{
			for (int i = 0; i < _Weapons.Length; i++)
			{
				if (_Weapons[i]._Name == weaponName)
				{
					return i;
				}
			}
		}
		return 0;
	}

	public Weapon GetWeapon(string weaponName)
	{
		if (_Weapons != null)
		{
			for (int i = 0; i < _Weapons.Length; i++)
			{
				if (_Weapons[i]._Name == weaponName)
				{
					return _Weapons[i];
				}
			}
		}
		return null;
	}

	public static void InitializeWeaponShots(Weapon[] weapons)
	{
		if (weapons == null)
		{
			return;
		}
		foreach (Weapon weapon in weapons)
		{
			if (weapon != null)
			{
				weapon._AvailableShots = weapon._TotalShots;
			}
		}
	}
}
