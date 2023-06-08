using System;
using System.Collections;
using UnityEngine;

public class ObTargetable : MonoBehaviour
{
	public bool _Active = true;

	public int _TeamID;

	public bool _DestroyOnHit;

	public int _ScoreOnHit;

	[NonSerialized]
	public bool mHasActiveTarget;

	[NonSerialized]
	public float mActiveTargetTime;

	[NonSerialized]
	public bool mHasActiveFire;

	public bool _DeActivateOnHit;

	public float _RespawnTime;

	public bool _IsMultiTarget;

	public int _Health = 1;

	public GameObject _Hit3DScorePrefab;

	public LocaleString _CriticalText = new LocaleString("CRITICAL HIT");

	protected int mOriginalHealth;

	protected virtual void Awake()
	{
		mHasActiveTarget = false;
		mActiveTargetTime = 0f;
		mHasActiveFire = false;
		mOriginalHealth = _Health;
	}

	private void OnEnable()
	{
		if (_DeActivateOnHit)
		{
			Activate(active: true);
		}
	}

	public virtual void OnDamage(int damage, bool isLocal, bool isCritical = false)
	{
		if (!_Active)
		{
			return;
		}
		_Health -= damage;
		if (_Hit3DScorePrefab != null)
		{
			Show3DTargetHitScore(base.transform.position, damage * -1, isCritical);
		}
		if (_Health <= 0)
		{
			_Health = 0;
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.pToolbar.BroadcastMessage("HitObject", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
			if (_DeActivateOnHit)
			{
				Activate(active: false);
				UnityEngine.Object.Destroy((UnityEngine.Object.FindObjectOfType(typeof(ObAmmo)) as ObAmmo).gameObject);
			}
			else if (_DestroyOnHit)
			{
				_Active = false;
				UnityEngine.Object.Destroy(base.gameObject);
			}
			if (_RespawnTime > 0f)
			{
				StartCoroutine(Respawn(_RespawnTime));
			}
		}
	}

	protected IEnumerator Respawn(float timer)
	{
		Activate(active: false);
		yield return new WaitForSeconds(timer);
		_Health = mOriginalHealth;
		Activate(active: true);
	}

	protected void Activate(bool active)
	{
		Renderer[] componentsInChildren = base.transform.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = active;
		}
		_Active = active;
	}

	public void Show3DTargetHitScore(Vector3 inPosition, int inScore, bool isCritical)
	{
		TargetHit3DScore.Show3DHitScore(_Hit3DScorePrefab, inPosition, inScore, isCritical);
	}
}
