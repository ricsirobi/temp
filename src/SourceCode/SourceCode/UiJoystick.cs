using UnityEngine;
using UnityEngine.SceneManagement;

public class UiJoystick : KAUI
{
	private static UiJoystick mInstance;

	public bool _DrawRegion;

	public bool _AutoHide;

	public float _StickRadius = 10f;

	public float _DeadZone;

	public float _FlightDeadZone;

	public float _TouchRadius = 80f;

	public float _ViewRadius = 60f;

	public float _MaxTouchRange = 350f;

	public float _Gain = 1.75f;

	public float _FlightGain = 1f;

	public float _XLimit = 1f;

	public float _YLimit = 1f;

	public Vector2 _Position = Vector2.zero;

	public Rect _TapRegion = new Rect(0f, 0.5f, 0.5f, 0.5f);

	public float _HoldTime = 1f;

	[Tooltip("Use to disable code setting joystick invisible if InputMode is Mouse")]
	public bool _IgnoreInputMode;

	private int mTouchID = -1;

	private bool mIsPressed;

	private Vector3 mPrevMousePosition = Vector3.zero;

	private Vector3 mMousePosition = Vector3.zero;

	private float mElapsedTime;

	private Vector2 mInputPosition;

	private KAWidget mStick;

	private KAWidget mDpad;

	private KAWidget mDummy;

	private Vector2 mStickDefaultPosition;

	private bool mIsDraging;

	private bool mWaitOneFrame;

	private JoyStickPos mPos;

	private bool mIsInitialized;

	private bool mForceHide;

	public JoyStickPos pPos
	{
		get
		{
			return mPos;
		}
		set
		{
			mPos = value;
		}
	}

	public static UiJoystick pInstance
	{
		get
		{
			return mInstance;
		}
		set
		{
			mInstance = value;
		}
	}

	public bool pIsPressed
	{
		get
		{
			return mIsPressed;
		}
		set
		{
			mIsPressed = value;
		}
	}

	protected override void Start()
	{
		if (UtPlatform.IsHandheldDevice() && pInstance == null)
		{
			Object.DontDestroyOnLoad(base.gameObject);
			mInstance = this;
			base.Start();
			mStick = FindItem("JoystickThumbHL");
			mDpad = FindItem("dpad");
			mDummy = FindItem("dummy");
			Vector3 position = mStick.GetPosition();
			mStickDefaultPosition.x = position.x;
			mStickDefaultPosition.y = position.y;
			ShowJoystick(!_AutoHide);
			ResetJoystick();
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	protected void ShowJoystick(bool inShow)
	{
		if (!inShow)
		{
			mElapsedTime = 0f;
		}
		if (mDpad != null)
		{
			mDpad.SetVisibility(inShow);
		}
		if (mStick != null)
		{
			mStick.SetVisibility(inShow);
		}
	}

	private bool IsInRegion(Vector3 mousePos)
	{
		mousePos.y = (float)Screen.height - mousePos.y;
		return _TapRegion.Contains(mousePos);
	}

	private bool GetMousePosition(out Vector3 outMousePos)
	{
		Vector3 vector = KAInput.mousePosition;
		bool result = false;
		if (!UtPlatform.IsEditor() && KAInput.pInstance.IsTouchInput())
		{
			Touch[] touches = KAInput.touches;
			for (int i = 0; i < touches.Length; i++)
			{
				Touch touch = touches[i];
				if (mTouchID == touch.fingerId)
				{
					vector = touch.position;
					result = true;
					break;
				}
				if (IsInRegion(touch.position))
				{
					result = true;
					mTouchID = touch.fingerId;
					vector = touch.position;
					break;
				}
			}
		}
		else if (Input.GetMouseButton(0))
		{
			if (IsInRegion(vector) && mTouchID == -1)
			{
				result = true;
				mTouchID = 1;
			}
			result = mTouchID > 0;
		}
		outMousePos = vector;
		return result;
	}

	protected override void Update()
	{
		base.Update();
		bool visibility = GetVisibility();
		if (!_IgnoreInputMode)
		{
			if (visibility && (KAInput.pInstance.pInputMode == KAInputMode.MOUSE || RsResourceManager.pLevelLoadingScreen || AvAvatar.pState == AvAvatarState.PAUSED || AvAvatar.pState == AvAvatarState.NONE))
			{
				SetVisibility(isVisible: false);
			}
			else if (!visibility && KAInput.pInstance.pInputMode == KAInputMode.TOUCH && AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pState != 0)
			{
				SetVisibility(isVisible: true);
			}
		}
		else
		{
			if (_IgnoreInputMode || (!UtPlatform.IsMobile() && !UtPlatform.IsWSA()))
			{
				return;
			}
			if (!mForceHide)
			{
				if (visibility && (UtUtilities.IsKeyboardAttached() || RsResourceManager.pLevelLoadingScreen || AvAvatar.pState == AvAvatarState.PAUSED || AvAvatar.pState == AvAvatarState.NONE))
				{
					OnSetVisibility(isVisible: false);
				}
				else if ((!visibility & !UtUtilities.IsKeyboardAttached()) && AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pState != 0)
				{
					OnSetVisibility(isVisible: true);
				}
			}
			else if (visibility)
			{
				OnSetVisibility(isVisible: false);
			}
		}
	}

	public void LateUpdate()
	{
		if (!mIsInitialized)
		{
			if (mPos == JoyStickPos.FREE)
			{
				_TapRegion.x *= Screen.width;
				_TapRegion.y *= Screen.height;
				_TapRegion.width *= Screen.width;
				_TapRegion.height *= Screen.height;
			}
			else if (mPos != 0)
			{
				float num = (float)mDpad.pBackground.width * mDpad.pBackground.cachedTransform.localScale.x;
				float num2 = (float)mDpad.pBackground.height * mDpad.pBackground.cachedTransform.localScale.y;
				_TapRegion.x = mDpad.transform.position.x - num / 2f;
				_TapRegion.y = mDpad.transform.position.y - num2 / 2f;
				if (KAUIManager.pInstance != null)
				{
					Vector3 vector = KAUIManager.pInstance.camera.WorldToScreenPoint(new Vector3(_TapRegion.x, _TapRegion.y, 0f));
					Vector3 vector2 = KAUIManager.pInstance.camera.WorldToScreenPoint(new Vector3(_TapRegion.x + num, _TapRegion.y + num2, 0f));
					_TapRegion.width = Mathf.Abs(vector2.x - vector.x);
					_TapRegion.height = Mathf.Abs(vector2.y - vector.y);
					_TapRegion.x = vector.x;
					_TapRegion.y = (float)Screen.height - (vector.y + _TapRegion.height);
				}
				else
				{
					_TapRegion.width = 1024f;
					_TapRegion.height = 1024f;
					_TapRegion.x = 0f;
					_TapRegion.y = 0f;
				}
			}
			mIsInitialized = true;
		}
		if (!GetVisibility())
		{
			return;
		}
		if (mPos == JoyStickPos.FREE)
		{
			bool flag = KAUI.GetGlobalMouseOverItem() != null;
			bool mouseButton = KAInput.GetMouseButton(0);
			if (AvAvatar.pState == AvAvatarState.PAUSED || (!mouseButton && mIsPressed) || KAUI._GlobalExclusiveUI != null || (KAInput.touchCount > 1 && !flag && !mWaitOneFrame))
			{
				ResetJoystick();
				ShowJoystick(!_AutoHide);
				return;
			}
			if (flag)
			{
				flag = false;
				mWaitOneFrame = true;
			}
			else
			{
				mWaitOneFrame = false;
			}
			Vector3 outMousePos = Vector3.zero;
			if (!GetMousePosition(out outMousePos))
			{
				ResetJoystick();
				ShowJoystick(!_AutoHide);
				return;
			}
			bool flag2 = IsInRegion(outMousePos);
			if (!mIsPressed && flag)
			{
				Rect inRect = default(Rect);
				KAUI.GetGlobalMouseOverItem().GetScreenRect(ref inRect);
				if (!inRect.Contains(outMousePos))
				{
					flag = false;
				}
			}
			if ((mouseButton && !flag) || mIsPressed)
			{
				mIsPressed = mIsDraging || flag2;
				if (mIsPressed)
				{
					mElapsedTime += Time.deltaTime;
					if (!(mElapsedTime >= _HoldTime))
					{
						return;
					}
					mMousePosition = outMousePos;
					if (!mIsDraging)
					{
						ShowJoystick(inShow: true);
						if (mPos == JoyStickPos.FREE)
						{
							Vector2 pos = KAUIManager.pInstance.camera.ScreenToWorldPoint(outMousePos);
							Vector2 localPosition = mDpad.GetLocalPosition(pos);
							mDpad.SetPosition(localPosition.x, localPosition.y);
							mStick.SetPosition(localPosition.x, localPosition.y);
							mStickDefaultPosition = localPosition;
							mInputPosition = localPosition;
							mIsDraging = true;
							mPrevMousePosition = outMousePos;
						}
						else
						{
							mPrevMousePosition = mMousePosition;
							mInputPosition = mStickDefaultPosition;
							mIsDraging = true;
						}
					}
					else if (mIsDraging && (outMousePos - mPrevMousePosition).sqrMagnitude > 0.0001f)
					{
						Vector3 vector3 = KAUIManager.pInstance.camera.ScreenToWorldPoint(outMousePos) - KAUIManager.pInstance.camera.ScreenToWorldPoint(mPrevMousePosition);
						OnDragStick(mStick, vector3);
						mPrevMousePosition = outMousePos;
					}
				}
				else
				{
					ResetJoystick();
					ShowJoystick(!_AutoHide);
				}
			}
			else
			{
				ResetJoystick();
				ShowJoystick(!_AutoHide);
			}
			return;
		}
		Vector3 outMousePos2 = Vector3.zero;
		if (!GetMousePosition(out outMousePos2))
		{
			ResetJoystick();
			ShowJoystick(!_AutoHide);
			return;
		}
		mMousePosition = outMousePos2;
		mIsPressed = true;
		if (!mIsDraging)
		{
			ShowJoystick(inShow: true);
			mPrevMousePosition = mMousePosition;
			mInputPosition = mStickDefaultPosition;
			mIsDraging = true;
		}
		else if (mIsDraging && (outMousePos2 - mPrevMousePosition).sqrMagnitude > 0.0001f)
		{
			Camera camera = ((KAUIManager.pInstance != null) ? KAUIManager.pInstance.camera : Camera.main);
			Vector3 vector4 = camera.ScreenToWorldPoint(outMousePos2) - camera.ScreenToWorldPoint(mPrevMousePosition);
			OnDragStick(mStick, vector4);
			mPrevMousePosition = outMousePos2;
		}
	}

	public void ResetJoystick()
	{
		mTouchID = -1;
		mIsPressed = false;
		mPrevMousePosition = Vector3.zero;
		mElapsedTime = 0f;
		mIsDraging = false;
		_Position = Vector2.zero;
		mInputPosition = mStickDefaultPosition;
		if (mStick != null)
		{
			mStick.SetPosition(mStickDefaultPosition.x, mStickDefaultPosition.y);
		}
	}

	public void OnDragStick(KAWidget inItem, Vector2 inDelta)
	{
		if (GetVisibility() && ((mPos == JoyStickPos.FREE && inItem == mDummy) || inItem == mStick))
		{
			bool num = AvAvatar.IsFlying();
			Vector3 position = mStick.GetPosition();
			Vector2 vector = default(Vector2);
			vector.x = position.x + inDelta.x;
			vector.y = position.y + inDelta.y;
			mInputPosition.x += inDelta.x;
			mInputPosition.y += inDelta.y;
			float value = (mInputPosition.x - mStickDefaultPosition.x) / _TouchRadius;
			float value2 = (mInputPosition.y - mStickDefaultPosition.y) / _TouchRadius;
			value = Mathf.Clamp(value, -1f, 1f);
			value2 = Mathf.Clamp(value2, -1f, 1f);
			float num2 = (num ? _FlightDeadZone : _DeadZone);
			if (value * value + value2 * value2 < num2 * num2)
			{
				value = (value2 = 0f);
			}
			float p = (num ? _FlightGain : _Gain);
			float num3 = ((value < 0f) ? (-1f) : 1f);
			float num4 = ((value2 < 0f) ? (-1f) : 1f);
			_Position.x = Mathf.Clamp(Mathf.Pow(Mathf.Abs(value), p), 0f, _XLimit) * num3;
			_Position.y = Mathf.Clamp(Mathf.Pow(Mathf.Abs(value2), p), 0f, _YLimit) * num4;
			Vector2 pos = ((KAUIManager.pInstance != null) ? KAUIManager.pInstance.camera : Camera.main).ScreenToWorldPoint(mMousePosition);
			Vector2 localPosition = mDpad.GetLocalPosition(pos);
			float sqrMagnitude = (mStickDefaultPosition - localPosition).sqrMagnitude;
			float sqrMagnitude2 = (mStickDefaultPosition - vector).sqrMagnitude;
			if (sqrMagnitude >= _ViewRadius * _ViewRadius || sqrMagnitude2 >= _ViewRadius * _ViewRadius)
			{
				Vector2 vector2 = localPosition - mStickDefaultPosition;
				vector = mStickDefaultPosition + vector2.normalized * Mathf.Min(vector2.magnitude, _ViewRadius);
			}
			mStick.SetPosition(vector.x, vector.y);
			if (sqrMagnitude >= _MaxTouchRange * _MaxTouchRange)
			{
				ResetJoystick();
				ShowJoystick(!_AutoHide);
				mIsDraging = false;
			}
		}
	}

	public override void OnDrag(KAWidget inItem, Vector2 inDelta)
	{
		if (GetVisibility())
		{
			base.OnDrag(inItem, inDelta);
		}
	}

	public override void SetVisibility(bool isVisible)
	{
		mForceHide = !isVisible;
		OnSetVisibility(isVisible);
	}

	private void OnSetVisibility(bool isVisible)
	{
		if (!_IgnoreInputMode && UtUtilities.IsKeyboardAttached() && KAInput.pInstance.pInputMode == KAInputMode.MOUSE)
		{
			base.SetVisibility(inVisible: false);
		}
		else
		{
			base.SetVisibility(isVisible);
		}
		if (!isVisible)
		{
			ResetJoystick();
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		mIsInitialized = false;
	}
}
