using UnityEngine;

public class UiTelescope : KAUI
{
	public GameObject _Camera;

	public float _ZoomSpeed = 3f;

	public MinMax _ZoomFOV = new MinMax(30f, 60f);

	public float _MoveSpeed = 10f;

	public float _HorizontalRange = 10f;

	public float _VerticalRange = 20f;

	public string _UseTelescopeMissionActionText = "UseTelescope";

	public float _TelescopeFrameMovementSpeed = 10f;

	public float _MoveTelescopeFrameBackDuration = 2f;

	private float mOldCameraSize;

	private float mCurrentHorizontalMovement;

	private float mCurrentVerticalMovement;

	private KAWidget mTelescopeFrame;

	private KAWidget mTelescopeBkg;

	private KAWidget mTelescopeBkgFill;

	private Vector3 mOriginalTelescopeFramePosition;

	private Vector3 mCurrentTelescopeFramePosition;

	private Vector3 mDefaultEulerAngles;

	private bool mMoveTelescopeFrameBack;

	private float mMoveTelescopeFrameBackTimer;

	private Quaternion mInitialOrientation = Quaternion.identity;

	protected override void Start()
	{
		base.Start();
		mTelescopeFrame = FindItem("AniTelescopeFrame");
		mTelescopeBkg = FindItem("BkgTelescope");
		mTelescopeBkgFill = FindItem("BkgTelescopeFill");
		mOriginalTelescopeFramePosition = mTelescopeFrame.transform.position;
	}

	protected override void Update()
	{
		base.Update();
		if (mMoveTelescopeFrameBack && mMoveTelescopeFrameBackTimer > 0f && mOriginalTelescopeFramePosition != mCurrentTelescopeFramePosition)
		{
			mMoveTelescopeFrameBackTimer -= Time.deltaTime;
			if (mMoveTelescopeFrameBackTimer <= 0f)
			{
				mMoveTelescopeFrameBackTimer = 0f;
				mMoveTelescopeFrameBack = false;
			}
			Vector3 movement = Vector3.Lerp(mOriginalTelescopeFramePosition, mCurrentTelescopeFramePosition, mMoveTelescopeFrameBackTimer / _MoveTelescopeFrameBackDuration) - mTelescopeFrame.transform.position;
			MoveTelescopeFrame(movement);
		}
	}

	private void OnEnable()
	{
		StartTelescope();
	}

	private void OnDisable()
	{
		EndTelescope();
	}

	public void StartTelescope()
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.pAvatarCam.SetActive(value: false);
		_Camera.SetActive(value: true);
		mOldCameraSize = _Camera.GetComponent<Camera>().orthographicSize;
		mInitialOrientation = _Camera.transform.rotation;
		mDefaultEulerAngles = _Camera.transform.localEulerAngles;
		mCurrentHorizontalMovement = 0f;
		mCurrentVerticalMovement = 0f;
	}

	public void EndTelescope()
	{
		AvAvatar.pAvatarCam.SetActive(value: true);
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		_Camera.SetActive(value: false);
		Camera component = _Camera.GetComponent<Camera>();
		component.orthographicSize = mOldCameraSize;
		component.fieldOfView = (_ZoomFOV.Max + _ZoomFOV.Min) / 2f;
		_Camera.transform.rotation = mInitialOrientation;
	}

	public override void OnPress(KAWidget inWidget, bool inPressed)
	{
		base.OnPress(inWidget, inPressed);
		if (inWidget.name == "BtnUpArrow" || inWidget.name == "BtnDownArrow" || inWidget.name == "BtnLeftArrow" || inWidget.name == "BtnRightArrow")
		{
			mMoveTelescopeFrameBack = !inPressed;
			if (mMoveTelescopeFrameBack)
			{
				mMoveTelescopeFrameBackTimer = _MoveTelescopeFrameBackDuration;
				mCurrentTelescopeFramePosition = mTelescopeFrame.transform.position;
			}
		}
	}

	public override void OnPressRepeated(KAWidget inWidget, bool inPressed)
	{
		base.OnPressRepeated(inWidget, inPressed);
		bool flag = !inPressed;
		if (inWidget.name == "BtnZoomIn")
		{
			Camera component = _Camera.GetComponent<Camera>();
			if (component != null)
			{
				component.fieldOfView -= _ZoomSpeed * Time.deltaTime;
				flag = component.fieldOfView < _ZoomFOV.Min;
				component.fieldOfView = (flag ? _ZoomFOV.Min : component.fieldOfView);
			}
		}
		else if (inWidget.name == "BtnZoomOut")
		{
			Camera component2 = _Camera.GetComponent<Camera>();
			if (component2 != null)
			{
				component2.fieldOfView += _ZoomSpeed * Time.deltaTime;
				flag = component2.fieldOfView > _ZoomFOV.Max;
				component2.fieldOfView = (flag ? _ZoomFOV.Max : component2.fieldOfView);
			}
		}
		else if (inWidget.name == "BtnUpArrow")
		{
			flag = !(mCurrentVerticalMovement > 0f - _VerticalRange);
			if (mCurrentVerticalMovement > 0f - _VerticalRange)
			{
				float num = _MoveSpeed * Time.deltaTime;
				if (mCurrentVerticalMovement - num < 0f - _VerticalRange)
				{
					num = _VerticalRange - Mathf.Abs(mCurrentVerticalMovement);
				}
				flag = num <= 0f;
				mCurrentVerticalMovement -= num;
				MoveTelescopeFrame(new Vector3(0f, _TelescopeFrameMovementSpeed * Time.deltaTime, 0f));
			}
		}
		else if (inWidget.name == "BtnDownArrow")
		{
			flag = !(mCurrentVerticalMovement < _VerticalRange);
			if (mCurrentVerticalMovement < _VerticalRange)
			{
				float num2 = _MoveSpeed * Time.deltaTime;
				if (mCurrentVerticalMovement + num2 > _VerticalRange)
				{
					num2 = _VerticalRange - Mathf.Abs(mCurrentVerticalMovement);
				}
				flag = num2 <= 0f;
				mCurrentVerticalMovement += num2;
				MoveTelescopeFrame(new Vector3(0f, (0f - _TelescopeFrameMovementSpeed) * Time.deltaTime, 0f));
			}
		}
		else if (inWidget.name == "BtnLeftArrow")
		{
			flag = !(mCurrentHorizontalMovement > _HorizontalRange);
			if (mCurrentHorizontalMovement > 0f - _HorizontalRange)
			{
				float num3 = _MoveSpeed * Time.deltaTime;
				if (mCurrentHorizontalMovement - num3 < 0f - _HorizontalRange)
				{
					num3 = _HorizontalRange - Mathf.Abs(mCurrentHorizontalMovement);
				}
				flag = num3 <= 0f;
				mCurrentHorizontalMovement -= num3;
				MoveTelescopeFrame(new Vector3((0f - _TelescopeFrameMovementSpeed) * Time.deltaTime, 0f, 0f));
			}
		}
		else if (inWidget.name == "BtnRightArrow")
		{
			flag = !(mCurrentHorizontalMovement < _HorizontalRange);
			if (mCurrentHorizontalMovement < _HorizontalRange)
			{
				float num4 = _MoveSpeed * Time.deltaTime;
				if (mCurrentHorizontalMovement + num4 > _HorizontalRange)
				{
					num4 = _HorizontalRange - Mathf.Abs(mCurrentHorizontalMovement);
				}
				flag = num4 <= 0f;
				mCurrentHorizontalMovement += num4;
				MoveTelescopeFrame(new Vector3(_TelescopeFrameMovementSpeed * Time.deltaTime, 0f, 0f));
			}
		}
		if (flag)
		{
			KAButton kAButton = inWidget as KAButton;
			kAButton._PressInfo.DoEffect(inShowEffect: false, kAButton);
		}
		_Camera.transform.localEulerAngles = mDefaultEulerAngles + new Vector3(mCurrentVerticalMovement, mCurrentHorizontalMovement, 0f);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnBack")
		{
			ProcessCloseUI();
		}
	}

	public void ProcessCloseUI()
	{
		base.gameObject.SetActive(value: false);
		if (!string.IsNullOrEmpty(_UseTelescopeMissionActionText) && MissionManager.pInstance != null && MissionManager.pIsReady)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", _UseTelescopeMissionActionText);
		}
	}

	private void MoveTelescopeFrame(Vector3 movement)
	{
		if (mTelescopeFrame != null)
		{
			mTelescopeFrame.transform.Translate(movement);
		}
		if (mTelescopeBkg != null)
		{
			mTelescopeBkg.transform.Translate(movement);
		}
		if (mTelescopeBkgFill != null)
		{
			mTelescopeBkgFill.transform.Translate(movement);
		}
	}
}
