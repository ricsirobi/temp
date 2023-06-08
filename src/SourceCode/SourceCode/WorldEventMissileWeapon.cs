using System.Collections.Generic;
using UnityEngine;

public class WorldEventMissileWeapon : WorldEventWeapon
{
	public Vector3 _Direction;

	public Flare[] _Flares;

	private bool mCanFire = true;

	private float mCooldownTime;

	private float mElapsedTime;

	protected override void Awake()
	{
		WorldEventTarget.OnTargetInit += Init;
		base.Awake();
	}

	private void Start()
	{
		mCooldownTime = GetCooldown();
		mTransform = base.transform;
		mInitialDirection = mTransform.up;
	}

	public void Init(string parentID)
	{
		if (_Flares.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _Flares.Length; i++)
		{
			if (_Flares[i] == null)
			{
				UtDebug.LogError("Flare setup is incorrect.");
				continue;
			}
			_Flares[i]._ID = parentID + "," + _WeaponID + "," + i;
			_Flares[i]._MissileWeapon = this;
			_Flares[i].Init();
		}
	}

	public void FireMissile()
	{
		if (CanAttack())
		{
			if (mTargetUserID == null)
			{
				mTargetUserID = "";
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("wID", _WeaponID.ToString());
			dictionary.Add("tID", mTargetUserID);
			dictionary.Add("uID", UserInfo.pInstance.UserID);
			dictionary.Add("objID", _ObjectUID);
			MainStreetMMOClient.pInstance.SendExtensionMessage("wex.ST", dictionary);
			Fire(null, useDirection: true, _Direction, 30f);
			if (_FiringParticleEffect != null)
			{
				Object.Instantiate(_FiringParticleEffect, mTransform.position, mTransform.rotation);
			}
			mCanFire = false;
			mElapsedTime = 0f;
		}
	}

	protected override bool CanAttack()
	{
		if (base.CanAttack())
		{
			return mCanFire;
		}
		return false;
	}

	protected override void Update()
	{
		if (!mCanFire)
		{
			mElapsedTime += Time.deltaTime;
			if (mElapsedTime >= mCooldownTime)
			{
				mCanFire = true;
			}
		}
	}

	private void OnDestroy()
	{
		WorldEventTarget.OnTargetInit -= Init;
	}

	public override void FireAOEBlast()
	{
		if (GetCurrentWeapon() == null)
		{
			UtDebug.LogError("Weapon not found");
			return;
		}
		SnChannel.Play(_FireSFX, SFXPool, inForce: true);
		Fire(null, useDirection: true, _Direction, 30f);
	}

	public override void UpdateWeaponData(int associateID, string data)
	{
		if (associateID < _Flares.Length)
		{
			_Flares[associateID].UpdatedData(data);
		}
	}
}
