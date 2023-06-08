using System;
using UnityEngine;

public class WorldEventShip : MonoBehaviour
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

	public WorldEventWeapon[] _Weapons;

	public DamageEffect[] _DamageLevels;

	private string mShipUID;

	public string pShipUID => mShipUID;

	public void Awake()
	{
		WorldEventTarget.OnTargetInit += Init;
		mShipUID = null;
	}

	public void Init(string parentID)
	{
		mShipUID = parentID;
		WorldEventTarget component = GetComponent<WorldEventTarget>();
		if (component != null)
		{
			component.OnTargetHealthUpdate = (WorldEventTarget.OnHealthUpdate)Delegate.Combine(component.OnTargetHealthUpdate, new WorldEventTarget.OnHealthUpdate(OnHealthChange));
		}
	}

	public void Fire(int weaponID, string AvatarID)
	{
		if (_Weapons != null && _Weapons.Length != 0 && !string.IsNullOrEmpty(mShipUID))
		{
			if (!string.IsNullOrEmpty(AvatarID))
			{
				_Weapons[weaponID].FireAtTarget(AvatarID);
			}
			else
			{
				_Weapons[weaponID].FireAOEBlast();
			}
		}
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
	}

	public void DeactivateWeapons()
	{
		WorldEventWeapon[] weapons = _Weapons;
		for (int i = 0; i < weapons.Length; i++)
		{
			weapons[i].enabled = false;
		}
	}

	private void OnDestroy()
	{
		WorldEventTarget.OnTargetInit -= Init;
	}
}
