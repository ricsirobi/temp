using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Lever : ObjectBase
{
	public class PhysicsObjectInfo
	{
		public PhysicsObject physicsObject;

		public Vector3 originalPosition;

		public Vector3 originalRotation;

		public Vector3 lastPosition;

		public Vector3 lastRotation;
	}

	public Transform _Lever;

	public Transform _Pan1;

	public Transform _Pan2;

	public bool _DisableFreeControl = true;

	private Vector3 mLeverPosition;

	private Vector3 mPan1Position;

	private Vector3 mPan2Position;

	private List<PhysicsObjectInfo> mPhysicsObjects;

	private float mPivotXLimit;

	protected override void Awake()
	{
		base.Awake();
		if (_Lever != null)
		{
			mLeverPosition = _Lever.localPosition;
		}
		if (_Pan1 != null)
		{
			mPan1Position = _Pan1.localPosition;
		}
		if (_Pan2 != null)
		{
			mPan2Position = _Pan2.localPosition;
		}
		Collider2D collider2D = GetCollider2D();
		if (!(collider2D == null))
		{
			mPivotXLimit = collider2D.bounds.size.x / 2f;
		}
	}

	private void Update()
	{
		if (!Application.isPlaying && _DisableFreeControl)
		{
			SetPivotPosition();
			SetPivotRotation();
		}
	}

	public void SetPivotPosition()
	{
		if (!(_Pivot == null))
		{
			Vector3 localPosition = new Vector3(mPivotXLimit * _PivotPositionSlider, _Pivot.localPosition.y, _Pivot.localPosition.z);
			_Pivot.localPosition = localPosition;
		}
	}

	public void SetPivotRotation()
	{
		float pivotAngleLimit = _PivotAngleLimit;
		_PivotAngleLimit = ((pivotAngleLimit == 0f && _PivotRotationSlider != 0f) ? 180f : pivotAngleLimit);
		if (_Lever != null)
		{
			_Lever.localEulerAngles = Vector3.zero;
			_Lever.localPosition = mLeverPosition;
			_Lever.RotateAround(_Pivot.position, Vector3.forward, _PivotAngleLimit * _PivotRotationSlider);
		}
		if (_Pan1 != null)
		{
			_Pan1.localPosition = GetPanPosition(_Pan1, mPan1Position);
		}
		if (_Pan2 != null)
		{
			_Pan2.localPosition = GetPanPosition(_Pan2, mPan2Position);
		}
		_PivotAngleLimit = pivotAngleLimit;
	}

	public Vector3 GetPanPosition(Transform obj, Vector3 vec)
	{
		float num = vec.x - _Pivot.localPosition.x;
		float num2 = num * Mathf.Cos(_PivotRotationSlider * _PivotAngleLimit * (MathF.PI / 180f));
		float num3 = num * Mathf.Sin(_PivotRotationSlider * _PivotAngleLimit * (MathF.PI / 180f));
		return new Vector3(_Pivot.localPosition.x + num2, vec.y + num3, vec.z);
	}

	public override void Enable()
	{
		base.Enable();
		foreach (PhysicsObjectInfo mPhysicsObject in mPhysicsObjects)
		{
			mPhysicsObject.physicsObject.Enable();
		}
	}

	public override Collider2D GetCollider2D()
	{
		if (_Lever == null)
		{
			return null;
		}
		return _Lever.GetComponent<Collider2D>();
	}

	public override void Setup()
	{
		if (_Pivot != null)
		{
			HingeJoint2D component = _Pivot.GetComponent<HingeJoint2D>();
			if (component != null && _PivotAngleLimit > 0f)
			{
				component.useLimits = true;
				JointAngleLimits2D limits = default(JointAngleLimits2D);
				limits.min = 0f - _PivotAngleLimit;
				limits.max = _PivotAngleLimit;
				component.limits = limits;
			}
		}
		if (_PivotPositionSlider != 0f)
		{
			SetPivotPosition();
		}
		if (_PivotRotationSlider != 0f)
		{
			SetPivotRotation();
		}
		mPhysicsObjects = new List<PhysicsObjectInfo>();
		PhysicsObject[] componentsInChildren = GetComponentsInChildren<PhysicsObject>();
		foreach (PhysicsObject physicsObject in componentsInChildren)
		{
			PhysicsObjectInfo physicsObjectInfo = new PhysicsObjectInfo();
			physicsObjectInfo.physicsObject = physicsObject;
			physicsObjectInfo.originalPosition = physicsObject.transform.position;
			physicsObjectInfo.originalRotation = physicsObject.transform.eulerAngles;
			physicsObjectInfo.lastPosition = physicsObject.transform.position;
			physicsObjectInfo.lastRotation = physicsObject.transform.eulerAngles;
			mPhysicsObjects.Add(physicsObjectInfo);
		}
	}

	public override void Reset()
	{
		if (mPhysicsObjects == null)
		{
			return;
		}
		foreach (PhysicsObjectInfo mPhysicsObject in mPhysicsObjects)
		{
			mPhysicsObject.physicsObject.Reset();
			mPhysicsObject.physicsObject.transform.position = mPhysicsObject.lastPosition;
			mPhysicsObject.physicsObject.transform.eulerAngles = mPhysicsObject.lastRotation;
		}
	}

	public override void Restart()
	{
		foreach (PhysicsObjectInfo mPhysicsObject in mPhysicsObjects)
		{
			mPhysicsObject.physicsObject.transform.position = mPhysicsObject.originalPosition;
			mPhysicsObject.physicsObject.transform.eulerAngles = mPhysicsObject.originalRotation;
		}
	}
}
