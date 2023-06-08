using UnityEngine;

public class ObLookAt : MonoBehaviour
{
	public Transform _TargetObj;

	public float _MaxAngle = 100f;

	public float _LookAtSpeed = 3f;

	public float _EnableSpeed = 4f;

	public float _RemainingDisableTime;

	private float mWeight;

	private float mTargetWeight;

	private Quaternion mLastTargetLookAtRot = Quaternion.identity;

	private Vector3 mTargetPoint = Vector3.zero;

	private bool mUseTargetPoint;

	private void LateUpdate()
	{
		if (_RemainingDisableTime > 0f)
		{
			_RemainingDisableTime = Mathf.Max(0f, _RemainingDisableTime - Time.deltaTime);
			if (_RemainingDisableTime > 0f)
			{
				return;
			}
		}
		Quaternion quaternion = Quaternion.LookRotation(GetTargetLocation() - base.transform.position, Vector3.up);
		mTargetWeight = (HasTarget() ? 1 : 0);
		if (mTargetWeight > 0f && Quaternion.Angle(base.transform.rotation, quaternion) > _MaxAngle)
		{
			mTargetWeight = 0f;
		}
		mWeight = Mathf.MoveTowards(mWeight, mTargetWeight, _EnableSpeed * Time.deltaTime);
		if (mWeight > 0f)
		{
			quaternion = Quaternion.RotateTowards(mLastTargetLookAtRot, quaternion, _LookAtSpeed);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, mWeight);
		}
		mLastTargetLookAtRot = quaternion;
	}

	public void LookAt(Transform Obj)
	{
		mUseTargetPoint = false;
		_TargetObj = Obj;
	}

	public void LookAt(Vector3 Point)
	{
		mUseTargetPoint = true;
		mTargetPoint = Point;
	}

	public void DisableLookAt(bool Inmediate = false)
	{
		mUseTargetPoint = false;
		if (Inmediate)
		{
			mWeight = 0f;
		}
		mTargetWeight = 0f;
	}

	public void TimedDisable(float time)
	{
		_RemainingDisableTime = time;
	}

	public bool HasTarget()
	{
		if (!mUseTargetPoint)
		{
			return _TargetObj != null;
		}
		return true;
	}

	public Vector3 GetTargetLocation()
	{
		if (mUseTargetPoint)
		{
			return mTargetPoint;
		}
		if (_TargetObj != null)
		{
			return _TargetObj.position;
		}
		return base.transform.position + base.transform.forward * 10f;
	}
}
