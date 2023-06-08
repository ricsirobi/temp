using System;
using UnityEngine;

public class UiScrollManager : KAMonoBase
{
	private class UiJSRect
	{
		private Vector2 mSize;

		public Vector2 Center { get; set; }

		public Vector2 Extents { get; private set; }

		public Vector2 Size => mSize;

		public float XMax => Center.x + Extents.x;

		public float YMax => Center.y + Extents.y;

		public float XMin => Center.x - Extents.x;

		public float YMin => Center.y - Extents.y;

		public void SetSize(Vector2 value)
		{
			mSize = value;
			Extents = mSize / 2f;
		}
	}

	public enum MoveDir
	{
		None = 0,
		Up = 1,
		Down = 2,
		Left = 4,
		Right = 8
	}

	public enum MovementType
	{
		None,
		Linear,
		Lerping
	}

	private enum FingerMovementType
	{
		None,
		Swipe,
		Drag
	}

	[Serializable]
	public struct ControlMapping
	{
		public MoveDir _MoveDir;

		public KAWidget _Button;

		public KeyCode[] _KeyCode;
	}

	public delegate void OnInitialized();

	public float _TouchSensitivity;

	public float _FlickSensitivity = 0.2f;

	public float _FlickDistance = 100f;

	public float _SwipeMultiplier = 6f;

	private float mTouchSensitivityOnStart;

	private float mFlickSensitivityOnStart;

	private float mFlickDistanceOnStart;

	private bool mMoveInXZPlan;

	private FingerMovementType mFingerMovementType;

	public MovementType _MoveType = MovementType.Lerping;

	public bool _LockIfSizeWithinViewPort = true;

	public bool _ChangeArrowVisibility = true;

	public float _MoveLinearSpeed = 100f;

	public float _MoveLerpSpeed = 3f;

	public float _MoveSwipSpeed = 3f;

	public float _DisplacementOnKeyPress = 3f;

	public float _MovementEdgeOffset = 90f;

	public GameObject _ScrollGameObject;

	public GameObject _DefaultMarker;

	public KAUI _ParentUI;

	public ControlMapping[] _ControlMapping;

	private KAWidget mLeftArrow;

	private KAWidget mRightArrow;

	private KAWidget mUpArrow;

	private KAWidget mDownArrow;

	private int mTouchFingerId = -1;

	private bool mScrollButtonPressed;

	private Vector2 mTargetPosition;

	private bool mInitialized;

	private UiJSRect mScrollArea = new UiJSRect();

	private UiJSRect mViewPort = new UiJSRect();

	private float mTimeFingerDown;

	private Vector2 mFingerDownPos;

	public MoveDir MoveDirection { get; set; }

	public bool IsScrolling { get; private set; }

	public event OnInitialized OnInitDone;

	protected virtual void Start()
	{
		mTouchSensitivityOnStart = TouchManager.pInstance._TouchSensitivity;
		mFlickSensitivityOnStart = TouchManager.pInstance._FlickSensitivity;
		mFlickDistanceOnStart = TouchManager.pInstance._FlickDistance;
		TouchManager.pInstance._TouchSensitivity = _TouchSensitivity;
		TouchManager.pInstance._FlickSensitivity = _FlickSensitivity;
		TouchManager.pInstance._FlickDistance = _FlickDistance;
		TouchManager.OnFingerDownEvent = (OnFingerDown)Delegate.Combine(TouchManager.OnFingerDownEvent, new OnFingerDown(OnFingerDown));
		TouchManager.OnFingerUpEvent = (OnFingerUp)Delegate.Combine(TouchManager.OnFingerUpEvent, new OnFingerUp(OnFingerUp));
		TouchManager.OnDragEvent = (OnDrag)Delegate.Combine(TouchManager.OnDragEvent, new OnDrag(OnDrag));
	}

	protected virtual void Initialize()
	{
		Vector3 vector = KAUIManager.pInstance.camera.ScreenToWorldPoint(Vector3.zero);
		Vector2 size = KAUIManager.pInstance.camera.ScreenToWorldPoint(new Vector3(Screen.safeArea.width, Screen.safeArea.height, 0f)) - vector;
		Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(_ScrollGameObject.transform);
		Vector2 size2 = new Vector2(bounds.size.x, bounds.size.y);
		mScrollArea.Center = bounds.center;
		mScrollArea.SetSize(size2);
		mViewPort.Center = bounds.center;
		mViewPort.SetSize(size);
		if (_DefaultMarker != null)
		{
			mTargetPosition = _DefaultMarker.transform.position;
			ClampTarget();
			mViewPort.Center = mTargetPosition;
			SetPosition();
		}
		else
		{
			mTargetPosition = mViewPort.Center;
		}
		ControlMapping[] controlMapping = _ControlMapping;
		for (int i = 0; i < controlMapping.Length; i++)
		{
			ControlMapping controlMapping2 = controlMapping[i];
			if (controlMapping2._MoveDir == MoveDir.Left)
			{
				mLeftArrow = controlMapping2._Button;
			}
			else if (controlMapping2._MoveDir == MoveDir.Right)
			{
				mRightArrow = controlMapping2._Button;
			}
			else if (controlMapping2._MoveDir == MoveDir.Up)
			{
				mUpArrow = controlMapping2._Button;
			}
			else if (controlMapping2._MoveDir == MoveDir.Down)
			{
				mDownArrow = controlMapping2._Button;
			}
		}
		if (this.OnInitDone != null)
		{
			this.OnInitDone();
		}
	}

	protected virtual void OnDestroy()
	{
		TouchManager.pInstance._TouchSensitivity = mTouchSensitivityOnStart;
		TouchManager.pInstance._FlickSensitivity = mFlickSensitivityOnStart;
		TouchManager.pInstance._FlickDistance = mFlickDistanceOnStart;
		TouchManager.OnFingerDownEvent = (OnFingerDown)Delegate.Remove(TouchManager.OnFingerDownEvent, new OnFingerDown(OnFingerDown));
		TouchManager.OnFingerUpEvent = (OnFingerUp)Delegate.Remove(TouchManager.OnFingerUpEvent, new OnFingerUp(OnFingerUp));
		TouchManager.OnDragEvent = (OnDrag)Delegate.Remove(TouchManager.OnDragEvent, new OnDrag(OnDrag));
	}

	private void Update()
	{
		if (!mInitialized)
		{
			Initialize();
			mInitialized = true;
		}
		if (!IsActive())
		{
			return;
		}
		MoveWithArrowAndMouse();
		if (!(mViewPort.Center != mTargetPosition))
		{
			return;
		}
		if (mFingerMovementType != FingerMovementType.Swipe)
		{
			if (_MoveType == MovementType.Linear)
			{
				UpdateLinearMove();
			}
			else if (_MoveType == MovementType.Lerping)
			{
				UpdateLerping();
			}
		}
		else
		{
			UpdateSwipe();
		}
		SetPosition();
	}

	private void LateUpdate()
	{
		IsScrolling = mViewPort.Center != mTargetPosition || mFingerMovementType != FingerMovementType.None;
	}

	private bool IsActive()
	{
		if (_ParentUI != null && KAUI._GlobalExclusiveUI != null && KAUI._GlobalExclusiveUI != _ParentUI)
		{
			return false;
		}
		return true;
	}

	private void SetPosition()
	{
		if (!mMoveInXZPlan)
		{
			_ScrollGameObject.transform.localPosition = new Vector3(0f - mViewPort.Center.x, 0f - mViewPort.Center.y, _ScrollGameObject.transform.localPosition.z);
		}
		else
		{
			_ScrollGameObject.transform.localPosition = new Vector3(0f - mViewPort.Center.x, _ScrollGameObject.transform.localPosition.y, 0f - mViewPort.Center.y);
		}
	}

	public virtual void ResetToMarker(GameObject marker)
	{
		if (marker != null)
		{
			Vector2 resetPos = new Vector2(marker.transform.localPosition.x, marker.transform.localPosition.y) - mScrollArea.Center;
			ResetPosition(resetPos);
		}
	}

	public virtual void ResetPosition(Vector2 resetPos)
	{
		mTargetPosition = resetPos;
		ClampTarget();
		StartSwipe();
	}

	private Vector2 FlipX(Vector2 inPoint)
	{
		return new Vector2(0f - inPoint.x, inPoint.y);
	}

	private Vector2 FlipY(Vector2 inPoint)
	{
		return new Vector2(inPoint.x, 0f - inPoint.y);
	}

	protected virtual void OnFingerDown(int inFingerID, Vector2 inVecPosition)
	{
		if (mTouchFingerId == -1 && !mScrollButtonPressed && IsActive())
		{
			mTouchFingerId = inFingerID;
			mTimeFingerDown = Time.time;
			mFingerDownPos = inVecPosition;
			if (mFingerMovementType == FingerMovementType.Swipe)
			{
				StopSwipe();
			}
		}
	}

	protected virtual void OnFingerUp(int inFingerID, Vector2 inVecPosition)
	{
		if (inFingerID == mTouchFingerId && IsActive())
		{
			mTouchFingerId = -1;
			float num = Time.time - mTimeFingerDown;
			Vector2 vector = FlipX(inVecPosition - mFingerDownPos);
			float magnitude = vector.magnitude;
			if (num < _FlickSensitivity && magnitude > _FlickDistance)
			{
				mTargetPosition += vector.normalized * (magnitude * _SwipeMultiplier);
				ClampTarget();
				StartSwipe();
			}
		}
	}

	protected virtual bool OnDrag(Vector2 inNewPosition, Vector2 inOldPosition, int inFingerID)
	{
		if (inFingerID == mTouchFingerId && IsActive())
		{
			mTimeFingerDown = Time.time;
			mFingerDownPos = inNewPosition;
			MoveDirection = MoveDir.None;
			mTargetPosition = mViewPort.Center;
			Vector2 vector = FlipX(inNewPosition - inOldPosition);
			mTargetPosition += vector.normalized * vector.magnitude;
			ClampTarget();
			mViewPort.Center = mTargetPosition;
			SetPosition();
			IsScrolling = true;
		}
		return true;
	}

	private void StartSwipe()
	{
		mFingerMovementType = FingerMovementType.Swipe;
		MoveDirection = MoveDir.None;
	}

	private void StopSwipe()
	{
		mFingerMovementType = FingerMovementType.None;
		MoveDirection = MoveDir.None;
		Vector2 vector2 = (mViewPort.Center = -_ScrollGameObject.transform.localPosition);
		mTargetPosition = vector2;
		SetPosition();
	}

	private void MoveTowards(MoveDir moveDir, float speed)
	{
		if ((moveDir & MoveDir.Right) != 0)
		{
			mTargetPosition += Vector2.right * speed;
		}
		if ((moveDir & MoveDir.Left) != 0)
		{
			mTargetPosition += Vector2.left * speed;
		}
		if ((moveDir & MoveDir.Up) != 0)
		{
			mTargetPosition += Vector2.up * speed;
		}
		if ((moveDir & MoveDir.Down) != 0)
		{
			mTargetPosition += Vector2.down * speed;
		}
		ClampTarget();
	}

	private void MoveWithArrowAndMouse()
	{
		ControlMapping[] controlMapping = _ControlMapping;
		for (int i = 0; i < controlMapping.Length; i++)
		{
			ControlMapping controlMapping2 = controlMapping[i];
			KeyCode[] keyCode = controlMapping2._KeyCode;
			foreach (KeyCode inKeyCode in keyCode)
			{
				if (KAInput.GetKeyUp(inKeyCode))
				{
					MoveDirection &= ~controlMapping2._MoveDir;
				}
				if (KAInput.anyKey && KAInput.GetKey(inKeyCode))
				{
					MoveDirection |= controlMapping2._MoveDir;
				}
			}
		}
		if (MoveDirection != 0)
		{
			MoveTowards(MoveDirection, _DisplacementOnKeyPress);
		}
		else if (ScrollAllowed() && mTouchFingerId == -1 && !UtPlatform.IsTouchInput())
		{
			if (KAInput.mousePosition.x > (float)Screen.width - _MovementEdgeOffset && KAInput.mousePosition.x < (float)Screen.width)
			{
				MoveTowards(MoveDir.Right, _DisplacementOnKeyPress);
			}
			else if (KAInput.mousePosition.x < _MovementEdgeOffset && KAInput.mousePosition.x >= 0f)
			{
				MoveTowards(MoveDir.Left, _DisplacementOnKeyPress);
			}
			if (KAInput.mousePosition.y > (float)Screen.height - _MovementEdgeOffset && KAInput.mousePosition.y < (float)Screen.height)
			{
				MoveTowards(MoveDir.Up, _DisplacementOnKeyPress);
			}
			else if (KAInput.mousePosition.y < _MovementEdgeOffset && KAInput.mousePosition.y >= 0f)
			{
				MoveTowards(MoveDir.Down, _DisplacementOnKeyPress);
			}
		}
	}

	private bool ScrollAllowed()
	{
		if (KAUI.GetGlobalMouseOverItem() == null)
		{
			return !mScrollButtonPressed;
		}
		return false;
	}

	public virtual void OnButtonPress(KAWidget button, bool inPressed)
	{
		ControlMapping[] controlMapping = _ControlMapping;
		for (int i = 0; i < controlMapping.Length; i++)
		{
			ControlMapping controlMapping2 = controlMapping[i];
			if (controlMapping2._Button != null && controlMapping2._Button == button)
			{
				if (inPressed)
				{
					MoveDirection |= controlMapping2._MoveDir;
				}
				else
				{
					MoveDirection &= ~controlMapping2._MoveDir;
				}
				mScrollButtonPressed = inPressed;
			}
		}
	}

	protected virtual void UpdateLinearMove()
	{
		Vector2 vector = mTargetPosition - mViewPort.Center;
		Vector2 normalized = vector.normalized;
		float sqrMagnitude = vector.sqrMagnitude;
		float num = _MoveLinearSpeed * Time.deltaTime;
		if (sqrMagnitude > num * num)
		{
			mViewPort.Center += normalized * num;
		}
		else
		{
			mViewPort.Center = mTargetPosition;
		}
	}

	protected virtual void UpdateLerping()
	{
		mViewPort.Center = Vector2.Lerp(mViewPort.Center, mTargetPosition, _MoveLerpSpeed * Time.deltaTime);
	}

	protected virtual void UpdateSwipe()
	{
		mViewPort.Center = Vector2.Lerp(mViewPort.Center, mTargetPosition, _MoveSwipSpeed * Time.deltaTime);
	}

	private void ClampTarget()
	{
		if (mViewPort.Size.x < mScrollArea.Size.x)
		{
			if (mTargetPosition.x < mScrollArea.XMin + mViewPort.Extents.x)
			{
				mTargetPosition.x = mScrollArea.XMin + mViewPort.Extents.x;
			}
			else if (mTargetPosition.x > mScrollArea.XMax - mViewPort.Extents.x)
			{
				mTargetPosition.x = mScrollArea.XMax - mViewPort.Extents.x;
			}
		}
		else if (!_LockIfSizeWithinViewPort)
		{
			if (mTargetPosition.x > mScrollArea.XMin + mViewPort.Extents.x)
			{
				mTargetPosition.x = mScrollArea.XMin + mViewPort.Extents.x;
			}
			else if (mTargetPosition.x < mScrollArea.XMax - mViewPort.Extents.x)
			{
				mTargetPosition.x = mScrollArea.XMax - mViewPort.Extents.x;
			}
		}
		else
		{
			mTargetPosition.x = mViewPort.Center.x;
		}
		if (mViewPort.Size.y < mScrollArea.Size.y)
		{
			if (mTargetPosition.y < mScrollArea.YMin + mViewPort.Extents.y)
			{
				mTargetPosition.y = mScrollArea.YMin + mViewPort.Extents.y;
			}
			else if (mTargetPosition.y > mScrollArea.YMax - mViewPort.Extents.y)
			{
				mTargetPosition.y = mScrollArea.YMax - mViewPort.Extents.y;
			}
		}
		else if (!_LockIfSizeWithinViewPort)
		{
			if (mTargetPosition.y > mScrollArea.YMin + mViewPort.Extents.y)
			{
				mTargetPosition.y = mScrollArea.YMin + mViewPort.Extents.y;
			}
			else if (mTargetPosition.y < mScrollArea.YMax - mViewPort.Extents.y)
			{
				mTargetPosition.y = mScrollArea.YMax - mViewPort.Extents.y;
			}
		}
		else
		{
			mTargetPosition.y = mViewPort.Center.y;
		}
		if (_ChangeArrowVisibility)
		{
			UpdateArrowVisibility();
		}
	}

	private void UpdateArrowVisibility(float thresholdValue = 1f)
	{
		if (mViewPort.Size.x < mScrollArea.Size.x)
		{
			bool flag = true;
			bool flag2 = true;
			if (mTargetPosition.x <= mScrollArea.XMin + mViewPort.Extents.x + thresholdValue)
			{
				flag = false;
			}
			else if (mTargetPosition.x >= mScrollArea.XMax - mViewPort.Extents.x - thresholdValue)
			{
				flag2 = false;
			}
			if (mLeftArrow != null && flag != mLeftArrow.GetVisibility())
			{
				mLeftArrow.SetVisibility(flag);
			}
			else if (mRightArrow != null && flag2 != mRightArrow.GetVisibility())
			{
				mRightArrow.SetVisibility(flag2);
			}
		}
		else if (mLeftArrow != null && mLeftArrow.GetVisibility())
		{
			mLeftArrow.SetVisibility(inVisible: false);
		}
		else if (mRightArrow != null && mRightArrow.GetVisibility())
		{
			mRightArrow.SetVisibility(inVisible: false);
		}
		if (mViewPort.Size.x < mScrollArea.Size.x)
		{
			bool flag3 = true;
			bool flag4 = true;
			if (mTargetPosition.y <= mScrollArea.YMin + mViewPort.Extents.y + thresholdValue)
			{
				flag3 = false;
			}
			else if (mTargetPosition.y >= mScrollArea.YMax - mViewPort.Extents.y - thresholdValue)
			{
				flag4 = false;
			}
			if (mUpArrow != null && flag4 != mUpArrow.GetVisibility())
			{
				mUpArrow.SetVisibility(flag4);
			}
			else if (mDownArrow != null && flag3 != mDownArrow.GetVisibility())
			{
				mDownArrow.SetVisibility(flag3);
			}
		}
		else if (mUpArrow != null && mUpArrow.GetVisibility())
		{
			mUpArrow.SetVisibility(inVisible: false);
		}
		else if (mDownArrow != null && mDownArrow.GetVisibility())
		{
			mDownArrow.SetVisibility(inVisible: false);
		}
	}
}
