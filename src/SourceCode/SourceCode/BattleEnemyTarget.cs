using System;
using System.Collections.Generic;
using System.Linq;
using SWS;
using UnityEngine;

public class BattleEnemyTarget : ObTargetable
{
	[Serializable]
	public class ObjectDamageInfo
	{
		public bool DamageInfoApplied;

		public float HealthPercentage;

		public GameObject[] PartsToEnable;

		public GameObject[] PartsToDisable;
	}

	public delegate void OnHealthUpdate(float healthPercentage);

	public AudioClip _ObjectHitSFX;

	public AudioClip _ObjectDestroyedSFX;

	public AudioClip _ObjectAmbientSnd;

	public string _ObjectSFXPool;

	public string _ObjectAmbientPool;

	public Transform ParentPartsHolder;

	public List<ObjectDamageInfo> _ObjectDamageInfo = new List<ObjectDamageInfo>();

	public OnHealthUpdate OnTargetHealthUpdate;

	public KAWidget _HealthBarWidget;

	public KAWidget _DisplayNameWidget;

	public GameObject _HealthBar;

	public Vector3 _HealthBarOffset;

	public BattleEnemy _BattleEnemy;

	public Vector3 _DeathPosOffset = Vector3.zero;

	public float _DeathRollOffset;

	public float _PostDeathSpeed = 1f;

	public float _DeathDrownSpeed = 2f;

	private bool mDying;

	public bool pDying => mDying;

	protected override void Awake()
	{
		base.Awake();
		if (ObAmmo._DisableHitSoundObjectList != null)
		{
			ObAmmo._DisableHitSoundObjectList.Add(base.gameObject);
		}
		if (_ObjectDamageInfo.Count > 0)
		{
			_ObjectDamageInfo = _ObjectDamageInfo.OrderBy((ObjectDamageInfo offer) => offer.HealthPercentage).ToList();
		}
		_Health = _BattleEnemy._HealthMax;
		if (_HealthBar != null)
		{
			_HealthBar.transform.position = base.transform.position + _HealthBarOffset;
			_HealthBarWidget.SetVisibility(inVisible: true);
			_DisplayNameWidget.SetVisibility(inVisible: true);
			_DisplayNameWidget.SetText(_BattleEnemy._NameText.GetLocalizedString());
			UpdateHealthBar((float)_Health / (float)_BattleEnemy._HealthMax);
		}
	}

	protected virtual void Update()
	{
		if (mDying)
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition = Vector3.MoveTowards(localPosition, _DeathPosOffset, _PostDeathSpeed * Time.deltaTime);
			base.transform.localPosition = localPosition;
			Vector3 localEulerAngles = base.transform.localEulerAngles;
			localEulerAngles.z = Mathf.MoveTowardsAngle(localEulerAngles.z, _DeathRollOffset, _DeathDrownSpeed * Time.deltaTime);
			base.transform.localEulerAngles = localEulerAngles;
		}
	}

	public virtual void Kill()
	{
		SnChannel.StopPool(_ObjectAmbientPool);
		SnChannel.Play(_ObjectDestroyedSFX, _ObjectSFXPool, inForce: true);
		mDying = true;
		_DeathPosOffset = base.transform.localPosition + _DeathPosOffset;
		_DeathRollOffset = base.transform.localEulerAngles.z + _DeathRollOffset;
		_Active = false;
		_HealthBarWidget.SetVisibility(inVisible: false);
		_DisplayNameWidget.SetVisibility(inVisible: false);
		splineMove component = GetComponent<splineMove>();
		if (null != component)
		{
			component.Stop();
			UnityEngine.Object.Destroy(component);
		}
	}

	public override void OnDamage(int damage, bool isLocal, bool isCritical)
	{
		if (_Active && isLocal && _Health != 0)
		{
			base.OnDamage(damage, isLocal, isCritical);
			ApplyDamage(damage, indirect: false, isCritical);
		}
	}

	private void ApplyDamage(int damage, bool indirect, bool isCritical = false)
	{
		float healthPercentage = (float)_Health / (float)_BattleEnemy._HealthMax;
		UpdateHealthBar(healthPercentage);
		UpdateObjectState(healthPercentage);
	}

	private void UpdateHealthBar(float healthPercentage)
	{
		if (_HealthBarWidget != null)
		{
			_HealthBarWidget.SetProgressLevel(healthPercentage);
		}
		if (OnTargetHealthUpdate != null)
		{
			OnTargetHealthUpdate(healthPercentage);
		}
	}

	private void UpdateObjectState(float healthPercentage)
	{
		if (_ObjectDamageInfo.Count == 0)
		{
			return;
		}
		foreach (ObjectDamageInfo item in _ObjectDamageInfo)
		{
			if (!((double)healthPercentage <= (double)item.HealthPercentage / 100.0))
			{
				continue;
			}
			if (item.DamageInfoApplied)
			{
				break;
			}
			ResetObjectParts();
			GameObject[] partsToEnable = item.PartsToEnable;
			foreach (GameObject gameObject in partsToEnable)
			{
				if (gameObject != null)
				{
					gameObject.SetActive(value: true);
				}
			}
			partsToEnable = item.PartsToDisable;
			foreach (GameObject gameObject2 in partsToEnable)
			{
				if (gameObject2 != null)
				{
					gameObject2.SetActive(value: false);
				}
			}
			item.DamageInfoApplied = true;
			break;
		}
	}

	private void ResetObjectParts()
	{
		if (!(ParentPartsHolder == null))
		{
			for (int i = 0; i < ParentPartsHolder.childCount; i++)
			{
				ParentPartsHolder.GetChild(i).gameObject.SetActive(value: false);
			}
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (!mDying && AvAvatar.pObject != null && collider.gameObject == AvAvatar.pObject)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (SanctuaryManager.pCurPetInstance != null)
			{
				SanctuaryManager.pCurPetInstance.SetMeter(SanctuaryPetMeterType.HEALTH, 0f);
			}
			component.TakeHit(component._Stats._CurrentHealth);
		}
	}
}
