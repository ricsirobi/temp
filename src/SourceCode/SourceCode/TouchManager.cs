using UnityEngine;

public class TouchManager : MonoBehaviour
{
	public float _TouchSensitivity = 0.4f;

	public float _TapSensitivity = 0.12f;

	public float _FlickSensitivity = 0.2f;

	public float _FlickDistance = 49f;

	public bool _SingleInstance = true;

	public float _ZoomDistance = 3f;

	private const int FINGERS = 5;

	private TouchInfo[] mTouchInProgress;

	private Vector2? mLastMousePosition;

	private static TouchManager mInstance;

	public static OnTouch OnTouchEvent;

	public static OnTouchMove OnTouchMoveEvent;

	public static OnFingerUp OnFingerUpEvent;

	public static OnFingerDown OnFingerDownEvent;

	public static OnTap OnTapEvent;

	public static OnFlick OnFlickEvent;

	public static OnZoom OnZoomEvent;

	public static OnRotate OnRotateEvent;

	public static OnDragStart OnDragStartEvent;

	public static OnDragEnd OnDragEndEvent;

	public static OnDrag OnDragEvent;

	public static OnGesture OnGestureEvent;

	public static TouchManager pInstance => mInstance;

	public bool pIsMultiTouch => Input.touchCount > 1;

	private void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
			if (_SingleInstance)
			{
				Object.DontDestroyOnLoad(base.gameObject);
				UtMobileUtilities.AddToPersistentScriptList(this);
			}
		}
		else if (_SingleInstance)
		{
			UtMobileUtilities.RemoveFromPersistentScriptList(this);
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		Input.multiTouchEnabled = true;
		mTouchInProgress = new TouchInfo[5];
	}

	protected void Update()
	{
		if (!UtPlatform.IsTouchInput() || UtPlatform.IsEditor())
		{
			if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
			{
				ProcessTouch(KATouch.FromMouse(ref mLastMousePosition));
			}
		}
		else if (Input.touchCount > 0)
		{
			for (int i = 0; i < Input.touchCount; i++)
			{
				ProcessTouch(KATouch.FromTouch(Input.GetTouch(i)));
			}
		}
	}

	private void ProcessTouch(KATouch inTouch)
	{
		Vector2 vector = ConvertToScreenPoint(inTouch.position);
		bool flag = inTouch.phase == TouchPhase.Ended || inTouch.phase == TouchPhase.Canceled;
		if (inTouch.fingerId >= 5)
		{
			return;
		}
		if (!flag)
		{
			OnTouch(inTouch.fingerId, vector);
		}
		if (inTouch.phase == TouchPhase.Began)
		{
			TouchInfo touchInfo = new TouchInfo(inTouch.position, inTouch.fingerId, Time.time);
			if (mTouchInProgress != null)
			{
				if (mTouchInProgress[inTouch.fingerId] == null)
				{
					mTouchInProgress[inTouch.fingerId] = touchInfo;
					OnTouchBegan(inTouch, vector);
				}
				else
				{
					mTouchInProgress[inTouch.fingerId] = touchInfo;
				}
			}
		}
		else if (inTouch.phase == TouchPhase.Moved)
		{
			OnTouchMoved(inTouch, vector);
		}
		else if (inTouch.phase == TouchPhase.Stationary)
		{
			OnTouchStationary(inTouch, vector);
		}
		else if (flag)
		{
			OnTouchEnded(inTouch, vector);
		}
	}

	public TouchInfo GetTouchForFinger(int inFingerID)
	{
		TouchInfo result = null;
		for (int i = 0; i < 5; i++)
		{
			if (mTouchInProgress != null && mTouchInProgress[i] != null && mTouchInProgress[i].pFingerID == inFingerID)
			{
				result = mTouchInProgress[i];
				break;
			}
		}
		return result;
	}

	public bool IsTap(KATouch inTouch, TouchInfo inTouchInfo)
	{
		float num = Time.time - inTouchInfo.pTime;
		if (inTouch.tapCount > 0)
		{
			return num < _TapSensitivity;
		}
		return false;
	}

	private void OnTouchBegan(KATouch inTouch, Vector2 inPosition)
	{
		OnFingerDown(inTouch.fingerId, inPosition);
	}

	private void OnTouchMoved(KATouch inTouch, Vector2 inPosition)
	{
		TouchInfo touchForFinger = GetTouchForFinger(inTouch.fingerId);
		if (touchForFinger == null || !(Time.time - touchForFinger.pTime > _TouchSensitivity))
		{
			return;
		}
		if (OnTouchMoveEvent != null)
		{
			OnTouchMoveEvent(inTouch.fingerId, ConvertToScreenPoint(inTouch.position));
		}
		bool num = Input.touchCount == 2 && mTouchInProgress[0] != null && mTouchInProgress[1] != null && KAUI.GetGlobalMouseOverItem() == null;
		bool flag = false;
		if (num)
		{
			Vector2 vector = mTouchInProgress[1].pPosition - mTouchInProgress[0].pPosition;
			Vector2 vector2 = Input.touches[1].position - Input.touches[0].position;
			mTouchInProgress[0].pPosition = Input.touches[0].position;
			mTouchInProgress[1].pPosition = Input.touches[1].position;
			Vector2 vector3 = vector / vector.magnitude;
			Vector2 vector4 = vector2 / vector2.magnitude;
			float num2 = vector.magnitude - vector2.magnitude;
			float sqrMagnitude = (vector3 - vector4).sqrMagnitude;
			if (sqrMagnitude > 0.1f)
			{
				if (Mathf.Acos(sqrMagnitude) > 1.5707963E-05f)
				{
					Vector3 lhs = new Vector3(vector.x, vector.y, 0f);
					Vector3 rhs = new Vector3(vector2.x, vector2.y, 0f);
					float z = Vector3.Cross(lhs, rhs).normalized.z;
					OnRotate(z == -1f);
					flag = true;
					mTouchInProgress[0].pState = TouchInfo.State.ROTATE;
					mTouchInProgress[1].pState = TouchInfo.State.ROTATE;
				}
			}
			else if (Mathf.Abs(num2) > _ZoomDistance)
			{
				OnZoom(num2 > 0f, num2);
				mTouchInProgress[0].pState = TouchInfo.State.ZOOM;
				mTouchInProgress[1].pState = TouchInfo.State.ZOOM;
				flag = true;
			}
		}
		if (!flag)
		{
			if (touchForFinger.pState != TouchInfo.State.DRAGGING)
			{
				touchForFinger.pState = TouchInfo.State.DRAGGING;
				OnDragStart(inPosition, touchForFinger.pFingerID);
			}
			else if (touchForFinger.pState == TouchInfo.State.DRAGGING)
			{
				touchForFinger.pLastPosition = touchForFinger.pPosition;
				touchForFinger.pPosition = inTouch.position;
				OnDrag(ConvertToScreenPoint(touchForFinger.pPosition), ConvertToScreenPoint(touchForFinger.pLastPosition), touchForFinger.pFingerID);
			}
		}
	}

	private void OnTouchStationary(KATouch inTouch, Vector2 inPosition)
	{
		TouchInfo touchForFinger = GetTouchForFinger(inTouch.fingerId);
		if (touchForFinger != null && Time.time - touchForFinger.pTime > _TouchSensitivity * 2f && touchForFinger.pState == TouchInfo.State.NONE && !IsTap(inTouch, touchForFinger))
		{
			touchForFinger.pState = TouchInfo.State.DRAGGING;
			OnDragStart(inPosition, touchForFinger.pFingerID);
		}
	}

	private void OnTouchEnded(KATouch inTouch, Vector2 inPosition)
	{
		TouchInfo touchForFinger = GetTouchForFinger(inTouch.fingerId);
		if (touchForFinger != null)
		{
			if (IsTap(inTouch, touchForFinger))
			{
				OnTap(touchForFinger.pFingerID, ConvertToScreenPoint(inTouch.position));
				touchForFinger.pTime = Time.time;
			}
			else if (Time.time - touchForFinger.pTime < _FlickSensitivity && (inTouch.position - touchForFinger.pLastPosition).sqrMagnitude > _FlickDistance)
			{
				touchForFinger.pLastPosition = touchForFinger.pPosition;
				touchForFinger.pPosition = inTouch.position;
				float angle = GetAngle(touchForFinger.pPosition, touchForFinger.pLastPosition);
				int direction = GetDirection(angle);
				int inMagnitude = (int)Vector2.Distance(touchForFinger.pPosition, touchForFinger.pLastPosition);
				Vector2 inDeltaPos = inTouch.position - touchForFinger.pLastPosition;
				OnFlick(touchForFinger.pFingerID, ConvertToScreenPoint(touchForFinger.pPosition), inDeltaPos, direction, inMagnitude);
			}
			if (touchForFinger.pState == TouchInfo.State.DRAGGING)
			{
				OnDragEnd(ConvertToScreenPoint(inTouch.position), touchForFinger.pFingerID);
			}
			OnFingerUp(inTouch.fingerId, inPosition);
			mTouchInProgress[inTouch.fingerId] = null;
		}
	}

	private Vector2 ConvertToScreenPoint(Vector2 inPoint)
	{
		return new Vector2(inPoint.x, (float)Screen.height - inPoint.y);
	}

	private float GetAngle(Vector2 P1, Vector2 P2)
	{
		Vector3 vector = new Vector3(P1.x, P1.y, 0f);
		Vector3 from = new Vector3(P2.x, P2.y, 0f) - vector;
		Vector3 to = new Vector3(-1f, 0f, 0f);
		float num = Vector3.Angle(from, to);
		if (P1.y <= P2.y)
		{
			num = 360f - num;
		}
		return num;
	}

	private int GetDirection(float fltAngle)
	{
		int result = -1;
		if ((double)fltAngle > 22.5 && (double)fltAngle <= 67.5)
		{
			result = 8;
		}
		else if ((double)fltAngle > 67.5 && (double)fltAngle <= 112.5)
		{
			result = 1;
		}
		else if ((double)fltAngle > 112.5 && (double)fltAngle <= 157.5)
		{
			result = 2;
		}
		else if ((double)fltAngle > 157.5 && (double)fltAngle <= 202.5)
		{
			result = 3;
		}
		else if ((double)fltAngle > 202.5 && (double)fltAngle <= 247.5)
		{
			result = 4;
		}
		else if ((double)fltAngle > 247.5 && (double)fltAngle <= 292.5)
		{
			result = 5;
		}
		else if ((double)fltAngle > 292.5 && (double)fltAngle <= 337.5)
		{
			result = 6;
		}
		else if ((double)fltAngle > 337.5 || (double)fltAngle <= 22.5)
		{
			result = 7;
		}
		return result;
	}

	protected void OnTouch(int inFingerID, Vector2 inVecPosition)
	{
		if (OnTouchEvent != null)
		{
			OnTouchEvent(inFingerID, inVecPosition);
		}
	}

	protected void OnFingerDown(int inFingerID, Vector2 inVecPosition)
	{
		if (OnFingerDownEvent != null)
		{
			OnFingerDownEvent(inFingerID, inVecPosition);
		}
	}

	protected void OnFingerUp(int inFingerID, Vector2 inVecPosition)
	{
		if (OnFingerUpEvent != null)
		{
			OnFingerUpEvent(inFingerID, inVecPosition);
		}
	}

	protected void OnTap(int inFingers, Vector2 inVecPosition)
	{
		if (OnTapEvent != null)
		{
			OnTapEvent(inFingers, inVecPosition);
		}
	}

	protected void OnFlick(int inFingers, Vector2 inVecPosition, Vector2 inDeltaPos, int inDirection, int inMagnitude)
	{
		if (OnFlickEvent != null)
		{
			OnFlickEvent(inFingers, inVecPosition, inDeltaPos, inDirection, inMagnitude);
		}
	}

	protected void OnZoom(bool inZoomOut, float inDist)
	{
		if (OnZoomEvent != null)
		{
			OnZoomEvent(inZoomOut, inDist);
		}
	}

	protected void OnRotate(bool inRight)
	{
		if (OnRotateEvent != null)
		{
			OnRotateEvent(inRight);
		}
	}

	protected void OnDragStart(Vector2 inVecPosition, int inFingerID)
	{
		if (OnDragStartEvent != null)
		{
			OnDragStartEvent(inVecPosition, inFingerID);
		}
	}

	protected void OnDragEnd(Vector2 inVecPosition, int inFingerID)
	{
		if (OnDragEndEvent != null)
		{
			OnDragEndEvent(inVecPosition, inFingerID);
		}
	}

	protected void OnDrag(Vector2 inNewPosition, Vector2 inOldPosition, int inFingerID)
	{
		if (OnDragEvent != null)
		{
			OnDragEvent(inNewPosition, inOldPosition, inFingerID);
		}
	}

	protected void OnGesture(string inGesture)
	{
		if (OnGestureEvent != null)
		{
			OnGestureEvent(inGesture);
		}
	}

	public static TouchManager Init()
	{
		if (mInstance == null)
		{
			GameObject obj = new GameObject("TouchManager");
			obj.transform.position = Vector3.zero;
			mInstance = obj.AddComponent<TouchManager>();
		}
		return mInstance;
	}

	public static void SetSensitivity(float inSensitivity)
	{
		if (!(mInstance == null))
		{
			mInstance._TouchSensitivity = inSensitivity;
		}
	}

	public static void SetFlickDist(float inDist)
	{
		if (!(mInstance == null))
		{
			mInstance._FlickDistance = inDist;
		}
	}
}
