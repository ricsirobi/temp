using System;
using UnityEngine;

public class ObOrbit : KAMonoBase
{
	public GameObject _MessageObject;

	public float _OrbitSpeed = 5f;

	public float _Height = 5f;

	public float _Radius = 2f;

	private Transform mTarget;

	public bool _LookAtTarget;

	public bool _RevolveAroundOwnPosition;

	public bool _RevolveReverese;

	public bool _ReduceRadiusWithTime;

	public bool _ChangeHeightWithTime;

	public bool _IncreaseHeightWithTime;

	public float _TimeToChange = 5f;

	public float _OffsetAngle;

	private Vector3 mTargetPos = Vector3.zero;

	private float mRadiusElapsedTime;

	private float mTimeScale;

	private bool mIsAvatarAsTarget;

	public Transform pTarget
	{
		get
		{
			return mTarget;
		}
		set
		{
			mTarget = value;
			mIsAvatarAsTarget = mTarget == AvAvatar.mTransform;
		}
	}

	private void Start()
	{
		if (_RevolveAroundOwnPosition)
		{
			mTargetPos = base.transform.position;
		}
	}

	private void Update()
	{
		if (mTarget == null)
		{
			if (!mIsAvatarAsTarget || !(AvAvatar.pObject != null))
			{
				return;
			}
			mTarget = AvAvatar.mTransform;
		}
		if (_ReduceRadiusWithTime && mTimeScale >= 1f)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage("OnOrbitMotionDone", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}
		if (_ReduceRadiusWithTime)
		{
			mRadiusElapsedTime += Time.deltaTime;
		}
		Vector3 position = GetPosition();
		float f = Time.time * _OrbitSpeed + _OffsetAngle;
		if (_RevolveReverese)
		{
			position.x += Mathf.Cos(f) * GetRadius();
			position.z += Mathf.Sin(f) * GetRadius();
		}
		else
		{
			position.x += Mathf.Sin(f) * GetRadius();
			position.z += Mathf.Cos(f) * GetRadius();
		}
		position.y += GetHeight();
		base.transform.position = position;
		if (_LookAtTarget)
		{
			base.transform.LookAt(GetPosition());
		}
	}

	private float GetRadius()
	{
		if (_ReduceRadiusWithTime)
		{
			float result = _Radius + mTimeScale * (0f - _Radius);
			mTimeScale = mRadiusElapsedTime / _TimeToChange;
			return result;
		}
		return _Radius;
	}

	private float GetHeight()
	{
		if (_ChangeHeightWithTime)
		{
			float y = base.transform.position.y;
			float num = 0f;
			num = ((!_ReduceRadiusWithTime) ? (Time.time * MathF.PI * 2f) : (mTimeScale / _OrbitSpeed * MathF.PI * 2f));
			Collider component = mTarget.gameObject.GetComponent<Collider>();
			if (_IncreaseHeightWithTime)
			{
				return (0f - Mathf.Cos(num * _OrbitSpeed)) * (_Height - component.bounds.size.y);
			}
			return (0f - Mathf.Cos(num * _OrbitSpeed)) * (component.bounds.size.y - _Height);
		}
		return _Height;
	}

	private Vector3 GetPosition()
	{
		if (_RevolveAroundOwnPosition)
		{
			return mTargetPos;
		}
		if (_ReduceRadiusWithTime)
		{
			Vector3 position = mTarget.position;
			position.y = mTarget.gameObject.GetComponent<Collider>().bounds.center.y;
			return position;
		}
		return mTarget.position;
	}
}
