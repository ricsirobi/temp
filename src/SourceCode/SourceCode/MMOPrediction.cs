using System;
using UnityEngine;

[Serializable]
public class MMOPrediction : MonoBehaviour
{
	public MMOAvatarUserVarData mLastReceivedPacket;

	protected Vector3 mCurVelocity = Vector3.zero;

	public float _TeleportTimeThreshold = 3f;

	public float _TeleportDistThreshold = 10f;

	public float _MMOTimeOut = 10f;

	public float _MMODropTime = 60f;

	protected AvAvatarController mController;

	protected Transform mTransform;

	private static float mCos80 = Mathf.Cos(80f);

	private bool mHasData;

	protected bool mHasReceivedPackage;

	private Quaternion mPrevRot;

	private Quaternion mSavedRot;

	private Vector3 mMoveDelta;

	private float mTeleportMinDist = 10f;

	private float mTeleportMinTime = 3f;

	private void Awake()
	{
		mTransform = base.transform;
	}

	public void ProcessNetworkData(Vector3 inPos, Quaternion inRot, Vector3 inVel, float inMaxSpeed, double inTimeStamp)
	{
		if (MMOTimeManager.pInstance.pAveragePing != 0.0)
		{
			if (mLastReceivedPacket == null)
			{
				mLastReceivedPacket = new MMOAvatarUserVarData();
			}
			mLastReceivedPacket._Position = inPos;
			mLastReceivedPacket._Rotation = inRot;
			mLastReceivedPacket._ServerTimeStamp = inTimeStamp;
			mLastReceivedPacket._TimeStamp = Time.time;
			mLastReceivedPacket._Velocity = inVel;
			mLastReceivedPacket._MaxSpeed = inMaxSpeed;
			mHasReceivedPackage = true;
		}
	}

	public void UpdatePlayer()
	{
		if (mLastReceivedPacket == null || !mHasReceivedPackage)
		{
			return;
		}
		double num = Time.time;
		Vector3 position = mLastReceivedPacket._Position;
		Vector3 velocity = mLastReceivedPacket._Velocity;
		float num2 = (float)(num - mLastReceivedPacket._TimeStamp);
		if ((bool)mController && num2 > _MMODropTime)
		{
			mController.RequestMMODelete();
		}
		else
		{
			if (num2 > _MMOTimeOut)
			{
				return;
			}
			Vector3 vector = position + velocity * Mathf.Min(num2, 2f);
			Vector3 position2 = mTransform.position;
			float magnitude = (vector - position2).magnitude;
			if (!IsInCameraViewFrustum())
			{
				mTransform.position = vector;
				mSavedRot = mLastReceivedPacket._Rotation;
				mPrevRot = mSavedRot;
				mCurVelocity = Vector3.zero;
				return;
			}
			float num3 = Mathf.Max(5f, velocity.magnitude);
			float num4 = magnitude / num3;
			num3 *= 1f + num4 / 10f;
			if (magnitude > mTeleportMinDist && num4 > mTeleportMinTime)
			{
				mTransform.position = vector;
				position2 = vector;
				mSavedRot = mLastReceivedPacket._Rotation;
				mPrevRot = mSavedRot;
				mCurVelocity = Vector3.zero;
			}
			else
			{
				position2 = Vector3.SmoothDamp(position2, vector, ref mCurVelocity, 0.2f, num3);
				mPrevRot = mSavedRot;
				mSavedRot = Quaternion.Lerp(mTransform.rotation, mLastReceivedPacket._Rotation, Time.deltaTime * 3f);
			}
			Vector3 vector2 = position2 - mTransform.position;
			if (mController != null)
			{
				mMoveDelta += vector2;
				mController.pVelocity = mCurVelocity;
				mController.pRotVel = Mathf.DeltaAngle(mSavedRot.eulerAngles.y, mPrevRot.eulerAngles.y);
			}
			mHasData = true;
		}
	}

	private bool IsInCameraViewFrustum()
	{
		if (AvAvatar.pAvatarCam == null)
		{
			return false;
		}
		return Vector3.Dot(AvAvatar.pAvatarCamForward, (mTransform.position - AvAvatar.AvatarCamPosition).normalized) > mCos80;
	}

	protected virtual void LateUpdate()
	{
		if (!mHasData)
		{
			return;
		}
		mHasData = false;
		mTransform.rotation = mSavedRot;
		float sqrMagnitude = mMoveDelta.sqrMagnitude;
		if (mController != null && mController.pController != null && sqrMagnitude > Mathf.Epsilon)
		{
			if (sqrMagnitude < 100f)
			{
				mController.pController.Move(mMoveDelta);
			}
			else
			{
				mController.SetPosition(base.transform.position + mMoveDelta);
			}
			mMoveDelta.x = (mMoveDelta.y = (mMoveDelta.z = 0f));
		}
	}

	public Vector3 GetPredictedPosition()
	{
		double num = Time.time;
		Vector3 position = mLastReceivedPacket._Position;
		Vector3 velocity = mLastReceivedPacket._Velocity;
		float a = (float)(num - mLastReceivedPacket._TimeStamp);
		return position + velocity * Mathf.Min(a, 2f);
	}
}
