using System;
using UnityEngine;

public class GauntletTarget : TargetBase
{
	public int _HitScore = 200;

	public string _HitAnim;

	public AudioClip _SpinSFX;

	public GauntletTargetHitBehaviour _HitBehaviour;

	public float _OnHitSpinAngle = 10f;

	public GauntletTargetSpinAxis _SpinAxis;

	public bool _IsPowerUp;

	[NonSerialized]
	public Vector3 _SpinAroundAxis = Vector3.zero;

	protected bool mIsHit;

	protected Renderer mRenderer;

	protected bool mIsActive;

	protected virtual void Start()
	{
		mRenderer = GetComponentInChildren<Renderer>();
		_SpinAroundAxis = base.transform.forward;
		if (_SpinAxis == GauntletTargetSpinAxis.RIGHT)
		{
			_SpinAroundAxis = base.transform.right;
		}
		else if (_SpinAxis == GauntletTargetSpinAxis.UP)
		{
			_SpinAroundAxis = base.transform.up;
		}
		if (base.gameObject.GetComponent<ObTargetable>() == null)
		{
			ObTargetable obTargetable = base.gameObject.AddComponent<ObTargetable>();
			if (obTargetable != null)
			{
				obTargetable._Active = false;
			}
		}
	}

	protected virtual void OnTriggerEnter(Collider collider)
	{
		ObAmmo component = collider.GetComponent<ObAmmo>();
		if (component != null)
		{
			OnAmmoHit(component);
		}
	}

	protected override void OnAmmoHit(ObAmmo projectile)
	{
		if (!(GauntletRailShootManager.pInstance != null))
		{
			return;
		}
		Transform transform = GauntletRailShootManager.pInstance._GauntletController._AvatarCamera.transform;
		if (Vector3.Dot(base.transform.TransformDirection(Vector3.forward), transform.TransformDirection(Vector3.forward)) > -0.2f)
		{
			UtDebug.LogError("Target not visible " + base.gameObject.name);
		}
		else if (!mIsHit && mIsActive)
		{
			PlayParticle(projectile.pHitPos);
			if (_HitBehaviour == GauntletTargetHitBehaviour.SPIN)
			{
				PlayAudio(_SpinSFX);
			}
			else
			{
				PlayAudio(_HitSFX);
			}
			GauntletRailShootManager.pInstance.HandleTargetHit(base.gameObject, _HitScore, _HitAnim, null);
			if (_IsPowerUp)
			{
				GauntletRailShootManager.pInstance.Collect();
			}
			HandleHitBehaviour();
		}
		else
		{
			GauntletRailShootManager.pInstance.PlayNegativeSFX();
		}
	}

	protected virtual void HandleProjectileHit(int inScore, string inAnimName)
	{
		if (GauntletRailShootManager.pInstance != null)
		{
			GauntletRailShootManager.pInstance.AddScore(inScore);
		}
		if (inAnimName != null && inAnimName.Length > 0)
		{
			Component[] componentsInChildren = GetComponentsInChildren<Animation>();
			componentsInChildren = componentsInChildren;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Animation)componentsInChildren[i]).CrossFade(inAnimName, 0.2f);
			}
		}
	}

	protected virtual void Update()
	{
		if ((!(mRenderer != null) || mRenderer.isVisible) && mIsHit)
		{
			if (_SpinAxis == GauntletTargetSpinAxis.DEPEND_PARENT && base.transform.parent != null)
			{
				_SpinAroundAxis = (base.transform.position - base.transform.parent.position).normalized;
			}
			base.transform.Rotate(_SpinAroundAxis, _OnHitSpinAngle * Time.deltaTime);
		}
	}

	public virtual void HandleHitBehaviour()
	{
		if (_HitBehaviour == GauntletTargetHitBehaviour.DESTROY)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else if (_HitBehaviour == GauntletTargetHitBehaviour.SPIN)
		{
			mIsHit = true;
		}
	}

	public virtual void ActivateTarget(bool active)
	{
		if (base.gameObject.activeInHierarchy)
		{
			mIsActive = active;
		}
	}
}
