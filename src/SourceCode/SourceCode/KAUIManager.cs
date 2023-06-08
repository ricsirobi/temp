using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class KAUIManager : KAUICamera
{
	[Serializable]
	public class CameraOrthoSize
	{
		public int _AspectWidth;

		public int _AspectHeight;

		public int _StandloneLandscapeOrthoSize;

		public int _MobileLandscapeOrthoSize;

		public int _StandlonePortraitOrthoSize;

		public int _MobilePortraitOrthoSize;
	}

	public int _MinLandscapeOrthoSize = 384;

	public int _MaxLandscapeOrthoSize = 384;

	public int _MinPortraitOrthoSize = 512;

	public int _MaxPortraitOrthoSize = 512;

	public float _2Dto3DScaleFactor = 0.005f;

	public CameraOrthoSize[] _CameraOrthoSize;

	private bool mCameraOrthoSet;

	private static KAUIManager mInstance;

	public bool _TestPortraitInEditor;

	public bool _TestSmallScreenInEditor;

	public float _SmallScreenDiagonalInches = 5f;

	public bool _UpdateCameraFOV;

	public float _PortraitFOV;

	public float _LandscapeFOV;

	private bool mIsPortraitMode;

	private bool mIsLandscapeMode;

	private string mInput;

	private KAWidget mSelectedWidget;

	private GameObject mSelectedObject;

	private KAWidget mDragItem;

	public static KAUIManager pInstance => mInstance;

	public KAWidget pDragItem
	{
		get
		{
			return mDragItem;
		}
		set
		{
			mDragItem = value;
		}
	}

	public static event Action<bool> OnOrientation;

	protected override void Awake()
	{
		base.Awake();
		if (mInstance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			mInstance = this;
		}
		if (Application.isEditor || _CameraOrthoSize.Length == 0)
		{
			return;
		}
		float b = (float)Screen.width / (float)Screen.height;
		CameraOrthoSize[] cameraOrthoSize = _CameraOrthoSize;
		foreach (CameraOrthoSize cameraOrthoSize2 in cameraOrthoSize)
		{
			if (Mathf.Approximately((float)cameraOrthoSize2._AspectWidth / (float)cameraOrthoSize2._AspectHeight, b))
			{
				float orthographicSize = base.camera.orthographicSize;
				if (Orientation.GetOrientation() == ScreenOrientation.Portrait || Orientation.GetOrientation() == ScreenOrientation.PortraitUpsideDown)
				{
					orthographicSize = (UtPlatform.IsMobile() ? cameraOrthoSize2._MobilePortraitOrthoSize : cameraOrthoSize2._StandlonePortraitOrthoSize);
					base.camera.orthographicSize = orthographicSize;
					_MinPortraitOrthoSize = (int)orthographicSize;
					_MaxPortraitOrthoSize = (int)orthographicSize;
				}
				else
				{
					orthographicSize = (UtPlatform.IsMobile() ? cameraOrthoSize2._MobileLandscapeOrthoSize : cameraOrthoSize2._StandloneLandscapeOrthoSize);
					base.camera.orthographicSize = orthographicSize;
					_MinLandscapeOrthoSize = (int)orthographicSize;
					_MaxLandscapeOrthoSize = (int)orthographicSize;
				}
				mCameraOrthoSet = true;
				break;
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		if (Application.isPlaying)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	public static bool IsSmallScreen()
	{
		if (mInstance == null)
		{
			return false;
		}
		if (mInstance._TestSmallScreenInEditor)
		{
			return true;
		}
		float num = Screen.dpi;
		if (num < 1f)
		{
			num = 200f;
		}
		float num2 = (float)Screen.width / num;
		float num3 = (float)Screen.height / num;
		return Mathf.Sqrt(num2 * num2 + num3 * num3) <= mInstance._SmallScreenDiagonalInches;
	}

	public static bool IsAutoRotate()
	{
		bool num = Screen.autorotateToPortrait && Screen.autorotateToPortraitUpsideDown;
		bool flag = Screen.autorotateToLandscapeLeft && Screen.autorotateToLandscapeRight;
		return num || flag;
	}

	public static bool IsPortrait()
	{
		if (Application.isEditor || !UtPlatform.IsMobile())
		{
			if (pInstance != null && pInstance._TestPortraitInEditor)
			{
				return true;
			}
			return false;
		}
		if (Orientation.GetOrientation() == ScreenOrientation.Portrait || Orientation.GetOrientation() == ScreenOrientation.PortraitUpsideDown)
		{
			return true;
		}
		return false;
	}

	public static bool IsLandscape()
	{
		if (Application.isEditor || !UtPlatform.IsMobile())
		{
			if (pInstance != null && !pInstance._TestPortraitInEditor)
			{
				return true;
			}
			return false;
		}
		if (Orientation.GetOrientation() == ScreenOrientation.Landscape || Orientation.GetOrientation() == ScreenOrientation.LandscapeRight)
		{
			return true;
		}
		return false;
	}

	private bool IsPerspectiveCamera()
	{
		if (base.camera != null && !base.camera.orthographic)
		{
			return true;
		}
		return false;
	}

	protected override void Update()
	{
		bool flag = _UpdateCameraFOV && IsPerspectiveCamera();
		if (!mCameraOrthoSet && !flag)
		{
			UpdateCameraSize();
		}
		if (IsAutoRotate())
		{
			if (!mIsPortraitMode && IsPortrait())
			{
				mIsPortraitMode = true;
				mIsLandscapeMode = false;
				if (KAUIManager.OnOrientation != null)
				{
					KAUIManager.OnOrientation(obj: true);
				}
				if (flag)
				{
					base.camera.fieldOfView = _PortraitFOV;
				}
			}
			else if (!mIsLandscapeMode && IsLandscape())
			{
				mIsLandscapeMode = true;
				mIsPortraitMode = false;
				if (KAUIManager.OnOrientation != null)
				{
					KAUIManager.OnOrientation(obj: false);
				}
				if (flag)
				{
					base.camera.fieldOfView = _LandscapeFOV;
				}
			}
		}
		if (UICamera.selectedObject != null)
		{
			if (mSelectedObject != UICamera.selectedObject)
			{
				mSelectedObject = UICamera.selectedObject;
				mSelectedWidget = base.pSelectedWidget;
			}
			if (mSelectedWidget != null)
			{
				mInput = Input.inputString;
				if (useKeyboard && Input.GetKeyDown(KeyCode.Delete))
				{
					mInput += "\b";
				}
				if (mInput.Length > 0)
				{
					if (mSelectedWidget.pUI != null)
					{
						mSelectedWidget.pUI.pEvents.ProcessInputEvent(base.pSelectedWidget, mInput);
					}
					else
					{
						mSelectedWidget.SendMessage("OnInput", mInput, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}
		base.Update();
	}

	private void UpdateCameraSize()
	{
		float num = base.camera.rect.yMin * (float)Screen.height;
		float value = (base.camera.rect.yMax * (float)Screen.height - num) * 0.5f * base.transform.lossyScale.y;
		value = ((Application.isEditor || !UtPlatform.IsMobile() || (Orientation.GetOrientation() != ScreenOrientation.Portrait && Orientation.GetOrientation() != ScreenOrientation.PortraitUpsideDown)) ? Mathf.Clamp(value, _MinLandscapeOrthoSize, _MaxLandscapeOrthoSize) : Mathf.Clamp(value, _MinPortraitOrthoSize, _MaxPortraitOrthoSize));
		if (!Mathf.Approximately(base.camera.orthographicSize, value))
		{
			base.camera.orthographicSize = value;
		}
	}
}
