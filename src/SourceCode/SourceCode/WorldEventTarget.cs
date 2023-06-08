using System;
using System.Collections.Generic;
using System.Linq;
using SWS;
using UnityEngine;

public class WorldEventTarget : ObTargetable
{
	[Serializable]
	public class ObjectDamageInfo
	{
		public bool DamageInfoApplied;

		public float HealthPercentage;

		public GameObject[] PartsToEnable;

		public GameObject[] PartsToDisable;
	}

	public delegate void OnWorldEventTargetHit(ObAmmo inAmmo, int? value = null);

	public delegate void OnHealthUpdate(float healthPercentage);

	public delegate void OnWorldEventTargetInit(string ParentID);

	public AudioClip _ObjectHitSFX;

	public AudioClip _ObjectDestroyedSFX;

	public AudioClip _ObjectAmbientSnd;

	public string _ObjectSFXPool;

	public string _ObjectAmbientPool;

	public WorldEventManager.WorldEvent myEvent;

	public WorldEventManager.EventObject myObject;

	public Transform ParentPartsHolder;

	public List<ObjectDamageInfo> _ObjectDamageInfo = new List<ObjectDamageInfo>();

	public OnWorldEventTargetHit OnHit;

	public OnHealthUpdate OnTargetHealthUpdate;

	private KAWidget mHealthBarWidget;

	private KAWidget mDisplayNameWidget;

	private KAWidget mDisplayHealthWidget;

	public GameObject _HealthBar;

	public Vector3 _HealthBarOffset;

	private bool mDying;

	private bool mSendHealthInitial;

	private Vector3 mDeathPosOffset = Vector3.zero;

	private float mDeathRollOffset;

	public bool pDying => mDying;

	public static event OnWorldEventTargetInit OnTargetInit;

	protected override void Awake()
	{
		base.Awake();
		_Active = false;
		if (ObAmmo._DisableHitSoundObjectList != null)
		{
			ObAmmo._DisableHitSoundObjectList.Add(base.gameObject);
		}
		Collider[] components = GetComponents<Collider>();
		if (components != null && components.Length != 0)
		{
			Collider[] array = components;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].isTrigger = true;
			}
		}
	}

	public void Init()
	{
		if (_ObjectDamageInfo.Count > 0)
		{
			_ObjectDamageInfo = _ObjectDamageInfo.OrderBy((ObjectDamageInfo offer) => offer.HealthPercentage).ToList();
		}
		if (_HealthBar != null)
		{
			WorldEventManager.OnWEStatusChanged += OnWEStatusUpdated;
			_HealthBar.transform.position = base.transform.position + _HealthBarOffset;
			UiWorldEventObjectInfo componentInChildren = _HealthBar.GetComponentInChildren<UiWorldEventObjectInfo>();
			mHealthBarWidget = componentInChildren._HealthBarWidget.GetComponentInChildren<KAWidget>();
			mDisplayNameWidget = componentInChildren._DisplayNameWidget;
			mDisplayHealthWidget = componentInChildren._DisplayHealthWidget;
			if (mHealthBarWidget != null)
			{
				mHealthBarWidget.SetVisibility(inVisible: true);
			}
			if (mDisplayNameWidget != null)
			{
				mDisplayNameWidget.SetVisibility(inVisible: true);
			}
			if (mDisplayHealthWidget != null)
			{
				mDisplayHealthWidget.SetVisibility(inVisible: true);
			}
			mDisplayNameWidget.SetText(myObject._NameText.GetLocalizedString());
			UpdateHealthBar(_Health, myObject._HealthMax);
			WorldEventShip component = GetComponent<WorldEventShip>();
			if (component != null && component._Weapons.Length != 0)
			{
				for (int i = 0; i < component._Weapons.Length; i++)
				{
					if (component._Weapons[i] == null)
					{
						UtDebug.LogError("Weapons setup is incorrect.");
						continue;
					}
					component._Weapons[i]._WeaponID = i;
					component._Weapons[i]._ObjectUID = myObject.UID;
				}
			}
			if (!mSendHealthInitial)
			{
				SendHealthUpdate();
				mSendHealthInitial = true;
			}
			if (WorldEventTarget.OnTargetInit != null)
			{
				WorldEventTarget.OnTargetInit(myObject.UID);
			}
		}
		_Active = true;
	}

	protected virtual void Update()
	{
		if (myEvent != null && myObject != null)
		{
			if (myObject.LiveHealth < _Health)
			{
				SilentDamage(_Health - myObject.LiveHealth);
			}
			if (mDying)
			{
				Vector3 localPosition = base.transform.localPosition;
				localPosition = Vector3.MoveTowards(localPosition, mDeathPosOffset, 1f * Time.deltaTime);
				base.transform.localPosition = localPosition;
				Vector3 localEulerAngles = base.transform.localEulerAngles;
				localEulerAngles.z = Mathf.MoveTowardsAngle(localEulerAngles.z, mDeathRollOffset, 2f * Time.deltaTime);
				base.transform.localEulerAngles = localEulerAngles;
			}
		}
	}

	public void SilentDamage(int damage)
	{
		base.OnDamage(damage, isLocal: true);
	}

	private void OnWEStatusUpdated(bool isEventStarted)
	{
		if (!isEventStarted)
		{
			WorldEventManager.OnWEStatusChanged -= OnWEStatusUpdated;
			_Active = false;
			if (mHealthBarWidget != null)
			{
				mHealthBarWidget.SetVisibility(inVisible: false);
			}
			if (mDisplayNameWidget != null)
			{
				mDisplayNameWidget.SetVisibility(inVisible: false);
			}
			if (mDisplayHealthWidget != null)
			{
				mDisplayHealthWidget.SetVisibility(inVisible: false);
			}
		}
	}

	public virtual void Kill()
	{
		SnChannel.StopPool(_ObjectAmbientPool);
		SnChannel.Play(_ObjectDestroyedSFX, _ObjectSFXPool, inForce: true);
		mDying = true;
		mDeathPosOffset = base.transform.localPosition + new Vector3(0f, -4f, 0f);
		mDeathRollOffset = base.transform.localEulerAngles.z + 35f;
		if (mHealthBarWidget != null)
		{
			mHealthBarWidget.SetVisibility(inVisible: false);
		}
		if (mDisplayHealthWidget != null)
		{
			mDisplayHealthWidget.SetVisibility(inVisible: false);
		}
		splineMove component = GetComponent<splineMove>();
		if (null != component)
		{
			component.Stop();
			UnityEngine.Object.Destroy(component);
		}
	}

	public void OnIndirectDamage(int damage)
	{
		ApplyDamage(damage, indirect: true);
	}

	public override void OnDamage(int damage, bool isLocal, bool isCritical)
	{
		if (_Active && isLocal && _Health != 0 && myEvent != null && myObject != null)
		{
			int health = _Health;
			base.OnDamage(damage, isLocal, isCritical);
			ApplyDamage(damage, indirect: false, isCritical);
			_Health = health;
		}
	}

	private void ApplyDamage(int damage, bool indirect, bool isCritical = false)
	{
		if (myEvent == null)
		{
			return;
		}
		float value = (float)damage / (float)myObject._HealthMax;
		value = Mathf.Clamp(value, 0f, 1f);
		if (myEvent.mClientSimulated)
		{
			if (MainStreetMMOClient.pInstance != null)
			{
				string key = "WEH_" + myObject.UID;
				string text = ((float)_Health / (float)myObject._HealthMax).ToString();
				string text2 = MMOTimeManager.pInstance.GetServerDateTime().ToString();
				MainStreetMMOClient.pInstance.SetRoomVariable(key, text + "," + text2);
			}
		}
		else if (MainStreetMMOClient.pInstance != null)
		{
			SendHealthUpdate(value);
		}
		if (!indirect)
		{
			WorldEventManager.pInstance.Show3DTargetHitScore(base.transform.position, damage, isCritical);
		}
	}

	private void SendHealthUpdate(float healthPerc = 0f)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("uid", myObject.UID);
		dictionary.Add("oh", healthPerc);
		dictionary.Add("eventUID", myEvent.mUID);
		dictionary.Add("event", myEvent._Name);
		MainStreetMMOClient.pInstance.SendExtensionMessage("wex.OV", dictionary);
	}

	private void OnAmmoHit(ObAmmo inAmmo)
	{
		if (OnHit != null)
		{
			OnHit(inAmmo);
		}
	}

	public void HealthUpdateFromServer(int currentHealth)
	{
		if (_Health != currentHealth)
		{
			SnChannel.Play(_ObjectHitSFX, _ObjectSFXPool, inForce: true);
			_Health = currentHealth;
		}
		UpdateHealthBar(_Health, myObject._HealthMax);
		UpdateObjectState((float)_Health / (float)myObject._HealthMax);
	}

	private void UpdateHealthBar(int currentHealth, int maxHealth)
	{
		float num = (float)currentHealth / (float)maxHealth;
		if (mHealthBarWidget != null)
		{
			mHealthBarWidget.SetProgressLevel(num);
		}
		if (mDisplayHealthWidget != null)
		{
			mDisplayHealthWidget.SetText(currentHealth + "/" + maxHealth);
		}
		if (OnTargetHealthUpdate != null)
		{
			OnTargetHealthUpdate(num);
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
		if (!(AvAvatar.pObject != null) || !(collider.gameObject == AvAvatar.pObject))
		{
			return;
		}
		if (_Active && !mDying)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (SanctuaryManager.pCurPetInstance != null)
			{
				SanctuaryManager.pCurPetInstance.SetMeter(SanctuaryPetMeterType.HEALTH, 0f);
			}
			component.TakeHit(component._Stats._CurrentHealth);
		}
		else
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("Respawn");
			GameObject gameObject = array[UnityEngine.Random.Range(0, array.Length)];
			if (gameObject != null)
			{
				AvAvatar.TeleportToObject(gameObject);
			}
		}
	}

	private void OnDestroy()
	{
		WorldEventManager.OnWEStatusChanged -= OnWEStatusUpdated;
	}
}
