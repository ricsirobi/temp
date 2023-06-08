using System;
using UnityEngine;

public class AvAvatarTaxiController : MonoBehaviour
{
	private class TaxiPath
	{
		private Vector3 mStartPos;

		private Vector3 mEndPos;

		private Vector3 mDirection;

		public Vector3 pStartPos => mStartPos;

		public Vector3 pEndPos => mEndPos;

		public Vector3 pDirection => mDirection;

		public TaxiPath(Vector3 inStartPos, Vector3 inEndPos)
		{
			mStartPos = inStartPos;
			mEndPos = inEndPos;
			mDirection = (mEndPos - mStartPos).normalized;
		}

		public Vector3 GetNearestPos(Vector3 inPos, bool inClampToPath = true)
		{
			float num = Vector3.Dot(inPos - mStartPos, mDirection);
			Vector3 result = mStartPos + mDirection * num;
			if (inClampToPath)
			{
				if (num > (mEndPos - mStartPos).magnitude)
				{
					result = mStartPos + mDirection * (mEndPos - mStartPos).magnitude;
				}
				else if (num < 0f)
				{
					result = mStartPos;
				}
			}
			return result;
		}

		public bool ReachedEnd(Vector3 inPos)
		{
			return Vector3.Dot(inPos - mStartPos, mDirection) > (mEndPos - mStartPos).magnitude;
		}
	}

	public GameObject _TargetObject;

	public float _DistanceConstraint = 5f;

	public float _RotationConstraint = 90f;

	private AvAvatarController mAvatarController;

	private bool mIsStarted;

	private TaxiPath mCurrTaxiPath;

	private void Start()
	{
	}

	private void Update()
	{
		if (mAvatarController == null && AvAvatar.pObject != null)
		{
			mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		}
		if (!mIsStarted && _TargetObject != null && mAvatarController != null)
		{
			StartTaxi();
		}
	}

	private void LateUpdate()
	{
		if (mIsStarted)
		{
			Vector3 nearestPos = mCurrTaxiPath.GetNearestPos(base.transform.position);
			Vector3 vector = mAvatarController.transform.position - nearestPos;
			float magnitude = vector.magnitude;
			if (magnitude > _DistanceConstraint)
			{
				Vector3 position = nearestPos + vector * (_DistanceConstraint / magnitude);
				mAvatarController.transform.position = position;
			}
			float num = Vector3.Dot(mCurrTaxiPath.pDirection, base.transform.forward);
			float num2 = Mathf.Cos(MathF.PI * _RotationConstraint / 360f);
			if (num < num2)
			{
				Quaternion b = Quaternion.LookRotation(mCurrTaxiPath.pDirection);
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, 0.1f);
			}
		}
	}

	private void StartTaxi()
	{
		if (mAvatarController != null && _TargetObject != null)
		{
			mCurrTaxiPath = new TaxiPath(mAvatarController.transform.position, _TargetObject.transform.position);
			mIsStarted = true;
		}
	}

	private void OnDrawGizmos()
	{
		if (mAvatarController != null && _TargetObject != null)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(mCurrTaxiPath.pStartPos, mCurrTaxiPath.pEndPos);
			Vector3 nearestPos = mCurrTaxiPath.GetNearestPos(base.transform.position);
			Gizmos.DrawCube(nearestPos, new Vector3(0.25f, 0.25f, 0.25f));
			Gizmos.DrawLine(nearestPos, mAvatarController.transform.position);
			Vector3 vector = mAvatarController.transform.position - nearestPos;
			float magnitude = vector.magnitude;
			if (magnitude > _DistanceConstraint)
			{
				Gizmos.DrawCube(nearestPos + vector * (_DistanceConstraint / magnitude), new Vector3(0.25f, 0.25f, 0.25f));
			}
		}
	}
}
