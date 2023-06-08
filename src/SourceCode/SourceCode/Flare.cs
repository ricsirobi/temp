using System;
using System.Collections.Generic;
using UnityEngine;

public class Flare : ObTargetable
{
	public delegate void OnFlareTargetHit(ObAmmo inAmmo, int? pointsOnHit = null);

	public float _TimeToShoot = 5f;

	public GameObject _HealthBar;

	public Vector3 _HealthBarOffset;

	public float _TimerAddOnHit = 1f;

	public int _PointsOnFlareHit = 10;

	public string _FlareLightAnim;

	public Animation _Animation;

	public ObTargetable _ParentTarget;

	public float _PercentageDamageToInflict = 100f;

	[HideInInspector]
	public string _ID;

	[HideInInspector]
	public WorldEventMissileWeapon _MissileWeapon;

	private KAWidget mTimerBarWidget;

	private float mElapsedTime;

	private DateTime mLastResponseTime = DateTime.MinValue;

	private bool mSendHealthInitial;

	public static event OnFlareTargetHit OnHit;

	protected override void Awake()
	{
		base.Awake();
		WorldEventManager.OnWEStatusChanged += OnWEStatusUpdated;
		mTimerBarWidget = _HealthBar.GetComponentInChildren<KAWidget>();
		if (mTimerBarWidget != null)
		{
			mTimerBarWidget.SetVisibility(inVisible: false);
		}
		_Active = false;
	}

	public void Init()
	{
		if (_HealthBar != null)
		{
			mElapsedTime = 0f;
			_HealthBar.transform.position = base.transform.position + _HealthBarOffset;
			if (mTimerBarWidget != null)
			{
				mTimerBarWidget.SetVisibility(inVisible: true);
			}
			UpdateTimerBar();
			if (!mSendHealthInitial && MainStreetMMOClient.pInstance != null && WorldEventManager.pInstance.pIsAIController)
			{
				SendFlareUpdate();
				mSendHealthInitial = true;
			}
		}
		_Active = true;
	}

	private void OnAmmoHit(ObAmmo inAmmo)
	{
		if (Flare.OnHit != null)
		{
			Flare.OnHit(inAmmo, _PointsOnFlareHit);
		}
	}

	private void SendFlareUpdate()
	{
		if (MainStreetMMOClient.pInstance != null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("fuid", _ID);
			dictionary.Add("oh", mElapsedTime);
			dictionary.Add("ts", MMOTimeManager.pInstance.GetServerDateTime().ToString());
			MainStreetMMOClient.pInstance.SendExtensionMessage("wex.OVF", dictionary);
		}
	}

	private void OnWEStatusUpdated(bool isEventStarted)
	{
		if (!isEventStarted)
		{
			WorldEventManager.OnWEStatusChanged -= OnWEStatusUpdated;
			if (mTimerBarWidget != null)
			{
				mTimerBarWidget.SetVisibility(inVisible: false);
				_Active = false;
			}
		}
	}

	private void Update()
	{
		if (_HealthBar == null || !WorldEventManager.pInstance.pIsEventActive)
		{
			return;
		}
		mElapsedTime += Time.deltaTime;
		if (mElapsedTime >= _TimeToShoot)
		{
			mElapsedTime = 0f;
			LightFlare();
			if (_MissileWeapon != null && WorldEventManager.pInstance.pIsAIController)
			{
				_MissileWeapon.FireMissile();
				SendFlareUpdate();
			}
			Invoke("UnlightFlare", 1f);
		}
		else
		{
			UpdateTimerBar();
		}
	}

	private void UpdateTimerBar()
	{
		float progressLevel = mElapsedTime / _TimeToShoot;
		if (mTimerBarWidget != null)
		{
			mTimerBarWidget.SetProgressLevel(progressLevel);
		}
	}

	public override void OnDamage(int damage, bool isLocal, bool isCritical)
	{
		if (_Active && isLocal)
		{
			base.OnDamage(damage, isLocal, isCritical);
			int num = (int)((float)damage * (_PercentageDamageToInflict / 100f));
			if (_ParentTarget != null)
			{
				_ParentTarget.SendMessage("OnIndirectDamage", num, SendMessageOptions.DontRequireReceiver);
			}
			WorldEventManager.pInstance.Show3DTargetHitScore(base.transform.position, num, isCritical);
			UnlightFlare();
			mElapsedTime -= _TimerAddOnHit;
			if (mElapsedTime < 0f)
			{
				mElapsedTime = 0f;
			}
			SendFlareUpdate();
			UpdateTimerBar();
		}
	}

	public void UnlightFlare()
	{
		if (_Animation != null && !string.IsNullOrEmpty(_FlareLightAnim))
		{
			_Animation.Stop(_FlareLightAnim);
		}
	}

	public void LightFlare()
	{
		if (_Animation != null && !string.IsNullOrEmpty(_FlareLightAnim))
		{
			_Animation.Play(_FlareLightAnim);
		}
	}

	public void UpdatedData(string data)
	{
		string[] array = data.Split(',');
		if (array.Length <= 1)
		{
			return;
		}
		DateTime result = DateTime.MinValue;
		float result2 = 0f;
		bool flag = float.TryParse(array[0], out result2);
		bool flag2 = DateTime.TryParse(array[1], out result);
		if (flag && flag2 && mLastResponseTime != result)
		{
			float num = 0f;
			num = (float)(MMOTimeManager.pInstance.GetServerDateTime() - result).TotalSeconds;
			mLastResponseTime = result;
			mElapsedTime = result2 + num;
			if (mElapsedTime < 0f)
			{
				mElapsedTime = 0f;
			}
		}
	}
}
