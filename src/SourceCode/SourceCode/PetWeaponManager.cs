using UnityEngine;

public class PetWeaponManager : WeaponManager
{
	public GameObject _WeaponPfOverride;

	public GameObject _WeaponHitPfOverride;

	public ParticleSystem[] _DragonOnFireParticles;

	public float _ReticleRotZSpeed = -30f;

	public bool _ShowTargetWindow;

	public string _PowerUpWeapon;

	private string mModifiedWeaponName;

	[HideInInspector]
	public SanctuaryPet SanctuaryPet;

	[HideInInspector]
	public bool mAltFireActive = true;

	protected override void Awake()
	{
		base.Awake();
		SanctuaryPet = base.gameObject.GetComponent<SanctuaryPet>();
	}

	protected override void Update()
	{
		base.Update();
		if (SanctuaryPet != null && !IsLocal && SanctuaryPet == SanctuaryManager.pCurPetInstance)
		{
			IsLocal = true;
		}
		string powerUpWeapon = _MainWeapon.name;
		if (!string.IsNullOrEmpty(mModifiedWeaponName))
		{
			powerUpWeapon = mModifiedWeaponName;
		}
		if (!string.IsNullOrEmpty(_PowerUpWeapon))
		{
			powerUpWeapon = _PowerUpWeapon;
		}
		if (CurrentWeapon == null || CurrentWeapon._Name != powerUpWeapon)
		{
			SetWeapon(powerUpWeapon);
		}
	}

	public void ModifyWeapon(string weaponName)
	{
		mModifiedWeaponName = weaponName;
	}

	public override bool Fire(Transform target, bool useDirection, Vector3 direction, float parentSpeed)
	{
		bool num = base.Fire(target, useDirection, direction, parentSpeed);
		if (num && SanctuaryPet != null)
		{
			AudioClip breatheFireSound = SanctuaryPet._BreatheFireSound;
			if (breatheFireSound != null)
			{
				SanctuaryPet.PlaySFX(breatheFireSound, looping: false);
				return num;
			}
			SanctuaryPet.PlayAnimSFX(SanctuaryPet._AttackAnim, looping: false);
		}
		return num;
	}

	public void PlayDragonOnFireParticle(bool isPlay)
	{
		if (_DragonOnFireParticles == null)
		{
			return;
		}
		ParticleSystem[] dragonOnFireParticles = _DragonOnFireParticles;
		foreach (ParticleSystem particleSystem in dragonOnFireParticles)
		{
			if (isPlay)
			{
				particleSystem.gameObject.SetActive(value: true);
				particleSystem.Play();
			}
			else
			{
				particleSystem.Stop();
				particleSystem.gameObject.SetActive(value: false);
			}
		}
	}

	public void SetDragonOnFire(float inAmount)
	{
		if (!(SanctuaryManager.pCurPetInstance != null))
		{
			return;
		}
		SkinnedMeshRenderer[] componentsInChildren = SanctuaryManager.pCurPetInstance.GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] materials = componentsInChildren[i].materials;
			foreach (Material material in materials)
			{
				if (material.HasProperty("_OnFire"))
				{
					material.SetFloat("_OnFire", inAmount);
				}
			}
		}
	}

	public override GameObject GetProjectilePrefab(WeaponTuneData.Weapon weapon)
	{
		if (_WeaponPfOverride != null)
		{
			return _WeaponPfOverride;
		}
		return base.GetProjectilePrefab(weapon);
	}

	public override GameObject GetWeaponHitPrefab(WeaponTuneData.Weapon weapon)
	{
		GameObject weaponHitPrefab = base.GetWeaponHitPrefab(weapon);
		if (_WeaponHitPfOverride != null && weaponHitPrefab != null)
		{
			return _WeaponHitPfOverride;
		}
		return weaponHitPrefab;
	}
}
