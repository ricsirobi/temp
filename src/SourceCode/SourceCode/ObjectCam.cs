using UnityEngine;

public class ObjectCam : MonoBehaviour
{
	public Vector3 _TargetRelativeOffsetPos = new Vector3(0f, 2f, 4f);

	public Vector3 _TargetRelativeOffsetRot = new Vector3(0f, 0f, 0f);

	public float _MoveSpeed = 4f;

	public float _RotateSpeed = 4f;

	public float _MoveEnableTimeMilliSecs = 500f;

	public float _TargetReachLerpEpsilon = 0.5f;

	private Transform mTargetTransform;

	private Vector3 mTargetRelativeOffsetPos;

	private float mEnableMoveTimer = -1f;

	private bool mMoveBack;

	private GameObject mMessageObject;

	private bool mIsDone = true;

	private ObjectCamController mCamController;

	private void Start()
	{
		mCamController = GetComponent<ObjectCamController>();
	}

	public void MoveTo(Transform inSourceTransform, Transform inTargetTransform, GameObject inMessageObject)
	{
		if (inSourceTransform != null)
		{
			base.transform.position = inSourceTransform.position;
			base.transform.rotation = inSourceTransform.rotation;
			CalculateRelativeOffset(inSourceTransform, inTargetTransform);
		}
		mTargetTransform = inTargetTransform;
		mMoveBack = false;
		mMessageObject = inMessageObject;
		mEnableMoveTimer = _MoveEnableTimeMilliSecs;
		mIsDone = false;
	}

	public void MoveTo(Transform inTargetTransform, GameObject inMessageObject, bool isMoveBack, bool calculateRelativeOffset)
	{
		MoveTo(inTargetTransform, inMessageObject, isMoveBack, _MoveEnableTimeMilliSecs, calculateRelativeOffset);
	}

	public void MoveTo(Transform inTargetTransform, GameObject inMessageObject, bool isMoveBack, float inDelayMilliSecs, bool calculateRelativeOffset)
	{
		if (calculateRelativeOffset)
		{
			CalculateRelativeOffset(base.transform, inTargetTransform);
		}
		mTargetTransform = inTargetTransform;
		mMessageObject = inMessageObject;
		mMoveBack = isMoveBack;
		mEnableMoveTimer = inDelayMilliSecs;
		mIsDone = false;
	}

	private void CalculateRelativeOffset(Transform inSource, Transform inTarget)
	{
		Vector3 vector = inTarget.position - inSource.position;
		vector.Normalize();
		vector = -vector;
		mTargetRelativeOffsetPos = vector * Mathf.Abs(_TargetRelativeOffsetPos.z);
		mTargetRelativeOffsetPos.y = _TargetRelativeOffsetPos.y;
	}

	private void Update()
	{
		if (mEnableMoveTimer > 0f)
		{
			mEnableMoveTimer -= Time.deltaTime * 1000f;
		}
		else
		{
			if (mIsDone)
			{
				return;
			}
			Vector3 worldPosition = GetWorldPosition();
			Vector3 vector = mTargetTransform.position - worldPosition;
			vector.Normalize();
			vector = -vector * mCamController.pZoomValue;
			worldPosition += vector;
			Vector3 vector2 = ((!mMoveBack) ? worldPosition : mTargetTransform.position);
			base.transform.position = Vector3.Lerp(base.transform.position, vector2, Time.deltaTime * _MoveSpeed);
			Quaternion quaternion = Quaternion.LookRotation((!mMoveBack) ? (mTargetTransform.position - base.transform.position) : mTargetTransform.forward) * Quaternion.Euler(_TargetRelativeOffsetRot);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, Time.deltaTime * _RotateSpeed);
			if (Vector3.Distance(base.transform.position, vector2) < _TargetReachLerpEpsilon && Quaternion.Angle(base.transform.rotation, quaternion) < _TargetReachLerpEpsilon)
			{
				base.transform.position = vector2;
				base.transform.rotation = quaternion;
				if (mMessageObject != null)
				{
					mMessageObject.SendMessage("OnTargetReached", mMoveBack);
					mMessageObject = null;
				}
				mIsDone = true;
			}
		}
	}

	public Vector3 GetWorldPosition()
	{
		return mTargetTransform.position + mTargetRelativeOffsetPos;
	}

	public bool IsMoving()
	{
		return !mIsDone;
	}

	public void SetMoveDone(bool isDone)
	{
		mIsDone = isDone;
	}
}
