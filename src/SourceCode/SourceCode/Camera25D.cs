using System;
using UnityEngine;

public class Camera25D : MonoBehaviour
{
	public enum Mode
	{
		NONE,
		FREECAM,
		FOLLOW,
		TRANSITION,
		FOLLOW2,
		GOTO
	}

	public enum MovementStyle
	{
		NONE,
		SLIDE,
		ZOOM,
		ROTATE
	}

	public enum BoundaryType
	{
		PERIMETER_BOUNDARIES,
		ALTITUDE_BOUNDARIES
	}

	[Serializable]
	public class TransformData
	{
		public Vector3 _Position;
	}

	private static Camera _Camera;

	public bool _EnableCameraMovement = true;

	public Camera _Cam;

	public BoxCollider[] _PerimeterBoundaries;

	public BoxCollider[] _AltitudeBoundaries;

	public GameObject _GroundMarker;

	public Vector3 _CamDepthAxis = new Vector3(0f, 1f, 0f);

	public Vector3 _CameraRotatesInAxis = new Vector3(0f, 0f, 1f);

	public MinMax _SlideSpeed;

	public float _SecdrySlideSpeedMF = 0.1f;

	public float _SlideDampingForce = 0.6f;

	public float _MinSlideDist = 10f;

	public float _ZoomSpeed = 1f;

	public float _SecdryZoomSpeed = 0.1f;

	public float _ZoomDampingForce = 0.8f;

	public float _MinZoomDist = 0.25f;

	public float _RotateSpeed = 1f;

	public float _SecdryRotateSpeed = 0.1f;

	public float _RotateDampingForce = 1.3f;

	public float _MinRotateAngle = 20f;

	public float _MaxRotateAngle = 25f;

	public float _TransitionTime = 1f;

	public Vector3 _CamInitialOrientation;

	public Vector3 _CamTopDownOrientation;

	public bool _DragGizmos = true;

	public Vector3 _CamBoundingCubeCenter;

	public Vector3 _CamBoundingCubeSize;

	public Vector3 _CamTopDownBoundingCubeCenter;

	public Vector3 _CamTopDownBoundingCubeSize;

	public float _CamTopDownRightOffset = 1f;

	public float _CamTopDownLeftOffset = 3.5f;

	public float _FollowCamSpeed = 8f;

	public TransformData _LockTransformData;

	public Vector3 _GoToPositionOffset = Vector3.zero;

	public Vector3 _FollowCameraOffset = Vector3.zero;

	public float _ScrollSpeed = 7f;

	public float _CameraChangingSpeed = 7f;

	private GameObject mObjectToFollow;

	private Vector3 mGoToPosition = Vector3.zero;

	private float mCurrSlideSpeed;

	private float mMaxHeightForCam;

	private float mCurrHeightOfCam;

	private Vector2 mDragMagnitude = Vector2.zero;

	private float mFUTime;

	private float mTotalStartGapDt;

	private float mTotalCurrGapDt;

	private float mCurrent2FingerGap;

	private float mPrev2FingerGap;

	private float mFingerGapDt;

	private float mFingerAngleDt;

	private float mCurrent2FingerAngle;

	private float mPrev2FingerAngle;

	private UserHand mHand = new UserHand();

	private Mode mCamMode = Mode.FREECAM;

	private Mode mCamNextMode;

	private MovementStyle mMvmtStyle;

	private GameObject mO;

	private bool mInvertCamMovement;

	private bool mEnableTopDownCamera;

	private bool mCheckIfObjectSelected;

	private GameObject mCurrObjectToFollow;

	private GameObject mNewObjectToFollow;

	private Vector3 DistanceFromFollowObject;

	private float mDTCamStartAngle;

	private float mDTCamEndAngle;

	private Vector3 mDTCamStartPoint = Vector3.zero;

	private Vector3 mDTCamEndPoint = Vector3.zero;

	private float mCurrTransitionTime;

	private Plane mCameraPlane;

	private Plane mGroundPlane;

	private Vector3 mCurrentCamBoundingCubeCenter;

	private Vector3 mCurrentCamBoundingCubeSize;

	public static Camera pCamera => _Camera;

	public GameObject pObjectToFollow
	{
		get
		{
			return mObjectToFollow;
		}
		set
		{
			mObjectToFollow = value;
			if (value == null)
			{
				mCamMode = Mode.FREECAM;
				mDragMagnitude = Vector2.zero;
				mHand.Reset();
			}
			else
			{
				mCamMode = Mode.FOLLOW2;
			}
		}
	}

	public Vector3 pGoToPosition
	{
		get
		{
			return mGoToPosition;
		}
		set
		{
			if (value == Vector3.zero)
			{
				mCamMode = Mode.FREECAM;
			}
			else
			{
				mCamMode = Mode.GOTO;
			}
			mGoToPosition = value;
		}
	}

	public bool pInvertCamMovement
	{
		get
		{
			return mInvertCamMovement;
		}
		set
		{
			mInvertCamMovement = value;
		}
	}

	public bool pEnableTopDownCamera
	{
		get
		{
			return mEnableTopDownCamera;
		}
		set
		{
			mEnableTopDownCamera = value;
			mCurrentCamBoundingCubeCenter = (mEnableTopDownCamera ? _CamTopDownBoundingCubeCenter : _CamBoundingCubeCenter);
			mCurrentCamBoundingCubeSize = (mEnableTopDownCamera ? _CamTopDownBoundingCubeSize : _CamBoundingCubeSize);
			Vector3 vector = _Cam.ScreenToWorldPoint(new Vector3(0f, Screen.height, base.transform.position.y));
			Vector3 vector2 = _Cam.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height, base.transform.position.y));
			mCurrentCamBoundingCubeSize.x -= (vector2.x - vector.x) * 2f;
			if (mEnableTopDownCamera)
			{
				float camTopDownRightOffset = _CamTopDownRightOffset;
				mCurrentCamBoundingCubeSize.x += camTopDownRightOffset;
				mCurrentCamBoundingCubeCenter.x += camTopDownRightOffset / 2f;
				float camTopDownLeftOffset = _CamTopDownLeftOffset;
				mCurrentCamBoundingCubeSize.x += camTopDownLeftOffset;
				mCurrentCamBoundingCubeCenter.x -= camTopDownLeftOffset / 2f;
			}
			base.transform.position = PushCameraInsideBounds(base.transform.position);
		}
	}

	private void Start()
	{
		mCameraPlane.SetNormalAndPosition(_CamDepthAxis, _Cam.transform.position);
		mGroundPlane.SetNormalAndPosition(_CamDepthAxis, _GroundMarker.transform.position);
		Vector3 position = _Cam.transform.position;
		Vector3 up = base.transform.up;
		Collider[] altitudeBoundaries = _AltitudeBoundaries;
		Vector3 intrstnPntOnColliders = GetIntrstnPntOnColliders(position, up, altitudeBoundaries);
		Vector3 position2 = _Cam.transform.position;
		Vector3 dir = -base.transform.up;
		altitudeBoundaries = _AltitudeBoundaries;
		Vector3 intrstnPntOnColliders2 = GetIntrstnPntOnColliders(position2, dir, altitudeBoundaries);
		mMaxHeightForCam = Vector3.Distance(intrstnPntOnColliders2, intrstnPntOnColliders);
		mCurrHeightOfCam = Vector3.Distance(intrstnPntOnColliders2, _Cam.transform.position);
		mCurrSlideSpeed = GetIntermediateValue(_SlideSpeed.Min, _SlideSpeed.Max, mCurrHeightOfCam / mMaxHeightForCam);
		mHand.pFingerDownPntr = OnFingerDown;
		mHand.pFingerUpPntr = OnFingerUp;
		mHand.pFingerMovedPntr = OnFingerMove;
		mHand.pDoubleTapPntr = OnFingerDoubleTap;
		_Camera = _Cam;
		pEnableTopDownCamera = false;
	}

	private void OnEnable()
	{
		_Cam.transform.eulerAngles = _CamInitialOrientation;
	}

	private void LateUpdate()
	{
		if (!_EnableCameraMovement)
		{
			return;
		}
		if (mEnableTopDownCamera)
		{
			_Cam.transform.eulerAngles = Vector3.Lerp(_Cam.transform.eulerAngles, _CamTopDownOrientation, Time.deltaTime * _CameraChangingSpeed);
		}
		else
		{
			_Cam.transform.eulerAngles = Vector3.Lerp(_Cam.transform.eulerAngles, _CamInitialOrientation, Time.deltaTime * _CameraChangingSpeed);
		}
		if (KAUI.GetGlobalMouseOverItem() != null || (!KAInput.pInstance.IsTouchInput() && (KAUICamera.IsHovering() || (bool)UICamera.selectedObject)))
		{
			return;
		}
		if (KAInput.GetKey(KeyCode.W))
		{
			mDragMagnitude = new Vector2(mDragMagnitude.x, 0f - _ScrollSpeed);
		}
		else if (KAInput.GetKey(KeyCode.S))
		{
			mDragMagnitude = new Vector2(mDragMagnitude.x, _ScrollSpeed);
		}
		if (KAInput.GetKey(KeyCode.A))
		{
			mDragMagnitude = new Vector2(_ScrollSpeed, mDragMagnitude.y);
		}
		else if (KAInput.GetKey(KeyCode.D))
		{
			mDragMagnitude = new Vector2(0f - _ScrollSpeed, mDragMagnitude.y);
		}
		if (mCamMode == Mode.TRANSITION)
		{
			if (mCheckIfObjectSelected && mCurrObjectToFollow != null)
			{
				if (mCurrObjectToFollow == mNewObjectToFollow)
				{
					mCurrObjectToFollow = null;
					mCamNextMode = Mode.FREECAM;
				}
				else if (mNewObjectToFollow != null)
				{
					mCurrObjectToFollow = mNewObjectToFollow;
					mCamNextMode = Mode.FOLLOW;
				}
				else
				{
					_ = mNewObjectToFollow == null;
				}
				mNewObjectToFollow = null;
				mCheckIfObjectSelected = false;
			}
			if (mCamNextMode == Mode.FOLLOW)
			{
				mDTCamEndPoint.x = mCurrObjectToFollow.transform.position.x;
				mDTCamEndPoint.z = mCurrObjectToFollow.transform.position.z;
			}
			mCurrTransitionTime += Time.deltaTime;
			if (mCurrTransitionTime >= _TransitionTime)
			{
				mCurrTransitionTime = _TransitionTime;
				if (mCamNextMode == Mode.NONE)
				{
					mCamMode = Mode.FREECAM;
				}
				else
				{
					mCamMode = mCamNextMode;
				}
				mCamNextMode = Mode.NONE;
			}
			base.transform.rotation = Quaternion.identity;
			base.transform.Rotate(_CamDepthAxis, mDTCamStartAngle + (mDTCamEndAngle - mDTCamStartAngle) * mCurrTransitionTime / _TransitionTime);
			Vector3 zero = Vector3.zero;
			Vector3 POC = Vector3.zero;
			zero = Vector3.Lerp(mDTCamStartPoint, mDTCamEndPoint, mCurrTransitionTime / _TransitionTime);
			if (!IsPointInsideColliders(BoundaryType.PERIMETER_BOUNDARIES, zero, base.transform.position, out POC))
			{
				base.transform.position = zero;
			}
			else
			{
				base.transform.position = POC;
			}
			return;
		}
		if (mCamMode == Mode.FREECAM)
		{
			mHand.UpdateTouch();
			mPrev2FingerAngle = mCurrent2FingerAngle;
			mPrev2FingerGap = mCurrent2FingerGap;
			if (mDragMagnitude != Vector2.zero)
			{
				Vector3 position = base.transform.position;
				float t = Mathf.Exp((Time.time - mFUTime) / (1f / _SlideDampingForce)) - 1f;
				float num = ((mMvmtStyle == MovementStyle.SLIDE) ? mCurrSlideSpeed : (mCurrSlideSpeed * _SecdrySlideSpeedMF));
				if (pInvertCamMovement)
				{
					float num2 = mCurrHeightOfCam * 2f;
					position += base.transform.forward * (num + num2) * Time.deltaTime * mDragMagnitude.y;
					position += base.transform.right * (num + num2) * Time.deltaTime * mDragMagnitude.x;
				}
				else
				{
					position += base.transform.forward * num * Time.deltaTime * (0f - mDragMagnitude.y);
					position += base.transform.right * num * Time.deltaTime * (0f - mDragMagnitude.x);
				}
				base.transform.position = PushCameraInsideBounds(position);
				mDragMagnitude = Vector2.Lerp(mDragMagnitude, Vector2.zero, t);
			}
			if (mFingerGapDt != 0f)
			{
				Vector3 position2 = _Cam.transform.position;
				float num3 = ((mMvmtStyle == MovementStyle.NONE || mMvmtStyle == MovementStyle.ZOOM) ? _ZoomSpeed : _SecdryZoomSpeed);
				position2 += _Cam.transform.forward * num3 * Time.deltaTime * mFingerGapDt;
				base.transform.position = PushCameraInsideBounds(position2);
				float num4 = Mathf.Exp((Time.time - mFUTime) / (1f / _ZoomDampingForce)) - 1f;
				float num5 = Mathf.Sign(mFingerGapDt);
				mFingerGapDt += (0f - mFingerGapDt) * num4;
				if (num5 != Mathf.Sign(mFingerGapDt))
				{
					mFingerGapDt = 0f;
				}
				mCameraPlane.SetNormalAndPosition(_CamDepthAxis, _Cam.transform.position);
				Vector3 position3 = _Cam.transform.position;
				Vector3 dir = -base.transform.up;
				Collider[] altitudeBoundaries = _AltitudeBoundaries;
				Vector3 intrstnPntOnColliders = GetIntrstnPntOnColliders(position3, dir, altitudeBoundaries);
				mCurrHeightOfCam = Vector3.Distance(intrstnPntOnColliders, _Cam.transform.position);
				mCurrSlideSpeed = GetIntermediateValue(_SlideSpeed.Min, _SlideSpeed.Max, mCurrHeightOfCam / mMaxHeightForCam);
			}
			base.transform.position = new Vector3((_LockTransformData._Position.x == 0f) ? base.transform.position.x : _LockTransformData._Position.x, (_LockTransformData._Position.y == 0f) ? base.transform.position.y : _LockTransformData._Position.y, (_LockTransformData._Position.z == 0f) ? base.transform.position.z : _LockTransformData._Position.z);
			return;
		}
		if (mCamMode == Mode.FOLLOW)
		{
			mHand.UpdateTouch();
			mPrev2FingerAngle = mCurrent2FingerAngle;
			mPrev2FingerGap = mCurrent2FingerGap;
			if (mFingerGapDt != 0f)
			{
				Vector3 position4 = _Cam.transform.position;
				Vector3 POC2 = default(Vector3);
				float num6 = ((mMvmtStyle == MovementStyle.NONE || mMvmtStyle == MovementStyle.ZOOM) ? _ZoomSpeed : _SecdryZoomSpeed);
				position4 += _Cam.transform.forward * num6 * Time.deltaTime * mFingerGapDt;
				if (!IsPointInsideColliders(BoundaryType.ALTITUDE_BOUNDARIES, position4, _Cam.transform.position, out POC2))
				{
					_Cam.transform.position = position4;
				}
				float num7 = Mathf.Exp((Time.time - mFUTime) / (1f / _ZoomDampingForce)) - 1f;
				float num8 = Mathf.Sign(mFingerGapDt);
				mFingerGapDt += (0f - mFingerGapDt) * num7;
				if (num8 != Mathf.Sign(mFingerGapDt))
				{
					mFingerGapDt = 0f;
				}
				mCameraPlane.SetNormalAndPosition(_CamDepthAxis, _Cam.transform.position);
				Vector3 position5 = _Cam.transform.position;
				Vector3 dir2 = -base.transform.up;
				Collider[] altitudeBoundaries = _AltitudeBoundaries;
				Vector3 intrstnPntOnColliders2 = GetIntrstnPntOnColliders(position5, dir2, altitudeBoundaries);
				mCurrHeightOfCam = Vector3.Distance(intrstnPntOnColliders2, _Cam.transform.position);
				mCurrSlideSpeed = GetIntermediateValue(_SlideSpeed.Min, _SlideSpeed.Max, mCurrHeightOfCam / mMaxHeightForCam);
			}
			Vector3 position6 = base.transform.position;
			position6.x = mCurrObjectToFollow.transform.position.x;
			position6.z = mCurrObjectToFollow.transform.position.z;
			base.transform.position = position6;
		}
		if (mCamMode == Mode.GOTO)
		{
			mHand.UpdateTouch();
			if (mDragMagnitude != Vector2.zero)
			{
				pGoToPosition = Vector3.zero;
				return;
			}
			Vector3 b = pGoToPosition;
			b.y = base.transform.position.y;
			if (!mEnableTopDownCamera)
			{
				b += _GoToPositionOffset;
			}
			Vector3 inCamPosition = Vector3.Lerp(base.transform.position, b, 5f * Time.deltaTime);
			base.transform.position = PushCameraInsideBounds(inCamPosition);
		}
		if (mCamMode != Mode.FOLLOW2)
		{
			return;
		}
		if (pObjectToFollow == null)
		{
			mCamMode = Mode.FREECAM;
			return;
		}
		Vector3 position7 = pObjectToFollow.transform.position;
		Vector3 position8 = pObjectToFollow.transform.position;
		position8.y = base.transform.position.y;
		if (!mEnableTopDownCamera)
		{
			position8 += _FollowCameraOffset;
		}
		Vector3 vector = Camera.main.WorldToViewportPoint(position7);
		if (vector.x < 0.1f || vector.x > 0.9f || vector.y < 0.1f || vector.y > 0.9f)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, position8, _FollowCamSpeed * Time.deltaTime);
			base.transform.position = PushCameraInsideBounds(base.transform.position);
		}
	}

	private Vector3 PushCameraInsideBounds(Vector3 inCamPosition)
	{
		Vector3 vector = mCurrentCamBoundingCubeCenter - mCurrentCamBoundingCubeSize / 2f;
		Vector3 vector2 = mCurrentCamBoundingCubeCenter + mCurrentCamBoundingCubeSize / 2f;
		if (inCamPosition.x < vector.x)
		{
			inCamPosition.x = vector.x;
		}
		if (inCamPosition.x > vector2.x)
		{
			inCamPosition.x = vector2.x;
		}
		if (inCamPosition.y < vector.y)
		{
			inCamPosition.y = vector.y;
		}
		if (inCamPosition.y > vector2.y)
		{
			inCamPosition.y = vector2.y;
		}
		if (inCamPosition.z < vector.z)
		{
			inCamPosition.z = vector.z;
		}
		if (inCamPosition.z > vector2.z)
		{
			inCamPosition.z = vector2.z;
		}
		return inCamPosition;
	}

	private void OnFingerDoubleTap(int FingerIndex)
	{
		if (mCamMode != Mode.TRANSITION)
		{
			mCheckIfObjectSelected = true;
		}
	}

	private void OnFingerDown(int FingerIndex)
	{
		if (mHand.GetTouchCount() == 1)
		{
			mMvmtStyle = MovementStyle.SLIDE;
		}
		if (mHand.GetTouchCount() >= 2)
		{
			mMvmtStyle = MovementStyle.NONE;
			mTotalStartGapDt = (mPrev2FingerGap = (mCurrent2FingerGap = mHand.GetGap()));
			mFingerGapDt = 0f;
			mFingerAngleDt = 0f;
			mTotalCurrGapDt = 0f;
			mDragMagnitude = Vector2.zero;
		}
	}

	private void OnFingerUp(int FingerIndex)
	{
		if (mO != null)
		{
			UnityEngine.Object.Destroy(mO);
			mO = null;
		}
		if (mHand.GetTouchCount() == 0)
		{
			mDragMagnitude = mHand.GetAvgDisplacements(FingerIndex, 5);
		}
		else if (mHand.GetTouchCount() == 1)
		{
			mFingerGapDt = mHand.GetAvgGap(5);
			mFingerAngleDt = mHand.GetAvgDAngle(5);
			if (Mathf.Abs(mFingerAngleDt) > _MaxRotateAngle)
			{
				mFingerAngleDt = Mathf.Sign(mFingerAngleDt) * _MaxRotateAngle;
			}
		}
		mFUTime = Time.time;
	}

	private void OnFingerMove(int FingerIndex)
	{
		if (KAUI.GetGlobalMouseOverItem() != null)
		{
			return;
		}
		if (mHand.GetTouchCount() == 1)
		{
			mDragMagnitude = mHand.pAvgCurrFinger[FingerIndex].position - mHand.pAvgPrevFinger[FingerIndex].position;
		}
		else
		{
			if (mHand.GetTouchCount() < 2)
			{
				return;
			}
			mCurrent2FingerGap = mHand.GetGap();
			mCurrent2FingerAngle = mHand.GetAngle();
			mFingerGapDt = mCurrent2FingerGap - mPrev2FingerGap;
			mFingerAngleDt = mCurrent2FingerAngle - mPrev2FingerAngle;
			if (mMvmtStyle != MovementStyle.ROTATE && mMvmtStyle != MovementStyle.ZOOM)
			{
				mMvmtStyle = MovementStyle.NONE;
				mTotalCurrGapDt = Mathf.Abs(mCurrent2FingerGap - mTotalStartGapDt);
				if (mTotalCurrGapDt / (float)Screen.width > _MinZoomDist)
				{
					mMvmtStyle = MovementStyle.ZOOM;
					return;
				}
				mDragMagnitude = mHand.pAvgCurrFinger[FingerIndex].position - mHand.pAvgPrevFinger[FingerIndex].position;
				mMvmtStyle = MovementStyle.SLIDE;
			}
			else
			{
				mDragMagnitude = mHand.pAvgCurrFinger[FingerIndex].position - mHand.pAvgPrevFinger[FingerIndex].position;
				mMvmtStyle = MovementStyle.SLIDE;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (_DragGizmos)
		{
			Gizmos.color = new Color(255f, 0f, 0f);
			Gizmos.DrawWireCube(mCurrentCamBoundingCubeCenter, mCurrentCamBoundingCubeSize);
		}
	}

	private float GetAngle(Vector2 vector)
	{
		return Mathf.Atan2(vector.x, vector.y) * 57.29578f;
	}

	private float GetAngle(Vector2 point1, Vector2 point2)
	{
		return Mathf.Atan2(point1.x - point2.x, point1.y - point2.y) * 57.29578f;
	}

	private float GetAngle(Vector3 vec, Vector3 axis)
	{
		return Mathf.Acos(Vector3.Dot(vec, axis) / (vec.magnitude * axis.magnitude)) * 57.29578f;
	}

	private bool IsPointInsideColliders(BoundaryType type, Vector3 Pos, Vector3 PrevPos)
	{
		Collider[] array = null;
		switch (type)
		{
		case BoundaryType.PERIMETER_BOUNDARIES:
		{
			Collider[] altitudeBoundaries = _PerimeterBoundaries;
			array = altitudeBoundaries;
			break;
		}
		case BoundaryType.ALTITUDE_BOUNDARIES:
		{
			Collider[] altitudeBoundaries = _AltitudeBoundaries;
			array = altitudeBoundaries;
			break;
		}
		}
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit hitInfo = default(RaycastHit);
			Vector3 direction = array[i].bounds.center - Pos;
			Ray ray = new Ray(Pos, direction);
			if (!array[i].Raycast(ray, out hitInfo, direction.magnitude))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsPointInsideColliders(BoundaryType type, Vector3 Pos, Vector3 PrevPos, out Vector3 POC)
	{
		if (IsPointInsideColliders(type, Pos, PrevPos))
		{
			Vector3 vector = Pos - PrevPos;
			Vector3[] array = new Vector3[3]
			{
				new Vector3(vector.x, 0f, 0f),
				new Vector3(0f, vector.y, 0f),
				new Vector3(0f, 0f, vector.z)
			};
			Vector3 vector2 = PrevPos;
			Vector3 vector3 = vector2;
			for (int i = 0; i < 3; i++)
			{
				vector3 = vector2 + array[i];
				if (!IsPointInsideColliders(type, vector3, vector2))
				{
					vector2 = vector3;
				}
			}
			POC = vector2;
			return true;
		}
		POC = PrevPos;
		return false;
	}

	private Vector2 ToXZPlane(Vector3 vec3d)
	{
		return new Vector2(vec3d.x, vec3d.z);
	}

	private float GetCameraHeight()
	{
		return Mathf.Abs(Vector3.Dot(_GroundMarker.transform.position, _CamDepthAxis) - Vector3.Dot(base.transform.position, _CamDepthAxis));
	}

	private Vector3 GetCenterPoint()
	{
		float enter = 0f;
		Ray ray = new Ray(base.transform.position, _Cam.transform.forward);
		mGroundPlane.Raycast(ray, out enter);
		if (enter <= 0f)
		{
			Debug.LogError("Watttt??");
		}
		return ray.GetPoint(enter);
	}

	private float GetHypDistance()
	{
		return Vector3.Distance(GetCenterPoint(), base.transform.position);
	}

	private float GetHypProjectionDistance()
	{
		float hypDistance = GetHypDistance();
		float cameraHeight = GetCameraHeight();
		return Mathf.Sqrt(hypDistance * hypDistance + cameraHeight * cameraHeight);
	}

	private Vector3 GetIntrstnPntAtGround(Vector3 point, Vector3 Dir)
	{
		float enter = 0f;
		Ray ray = new Ray(point, Dir);
		mGroundPlane.Raycast(ray, out enter);
		if (enter <= 0f)
		{
			Debug.LogError("Watttt??");
			return Vector3.zero;
		}
		return ray.GetPoint(enter);
	}

	private Vector3 GetIntrstnPntOnColliders(Vector3 point, Vector3 Dir, Collider[] Colliders)
	{
		Ray ray = new Ray(point, Dir);
		for (int i = 0; i < Colliders.Length; i++)
		{
			if (Colliders[i].Raycast(ray, out var hitInfo, Vector3.Distance(point, Colliders[i].transform.position)))
			{
				return hitInfo.point;
			}
		}
		return Vector3.zero;
	}

	private Vector3 GetCameraGroundPosition()
	{
		float enter = 0f;
		Ray ray = new Ray(base.transform.position, -base.transform.up);
		mGroundPlane.Raycast(ray, out enter);
		if (enter <= 0f)
		{
			Debug.LogError("Watttt??");
			return Vector3.zero;
		}
		return ray.GetPoint(enter);
	}

	private float MakeAnglePositive(float Angle)
	{
		while (Angle < 0f)
		{
			Angle += 360f;
		}
		while (Angle > 360f)
		{
			Angle -= 360f;
		}
		return Angle;
	}

	public float GetIntermediateValue(float A, float B, float Ratio)
	{
		return A + (B - A) * Ratio;
	}
}
