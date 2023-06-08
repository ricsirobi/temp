using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Orientation : MonoBehaviour
{
	private static bool _FirstTime = true;

	public bool _LandscapeOnly;

	public bool _AutoOrientation = true;

	private ScreenOrientation mCurrScreenOrientation;

	private List<object> _listeners = new List<object>();

	private bool mPrevAutoRotationToLandscape;

	private static Orientation mInstance = null;

	public static Orientation pInstance => mInstance;

	private void Awake()
	{
		mInstance = this;
		mCurrScreenOrientation = Screen.orientation;
	}

	private void Start()
	{
		if (!_FirstTime || (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android))
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			Object.DontDestroyOnLoad(base.gameObject);
			UtMobileUtilities.AddToPersistentScriptList(this);
		}
		_FirstTime = false;
		Orient();
	}

	private void Update()
	{
		if (_AutoOrientation)
		{
			Orient();
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (_AutoOrientation && !pause)
		{
			Orient();
		}
	}

	private void Orient()
	{
		bool flag = false;
		if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft && Screen.orientation != ScreenOrientation.Landscape)
		{
			mCurrScreenOrientation = ScreenOrientation.Landscape;
			flag = true;
		}
		else if (Input.deviceOrientation == DeviceOrientation.LandscapeRight && Screen.orientation != ScreenOrientation.LandscapeRight)
		{
			mCurrScreenOrientation = ScreenOrientation.LandscapeRight;
			flag = true;
		}
		else if (Input.deviceOrientation == DeviceOrientation.Portrait)
		{
			if (_LandscapeOnly && Screen.orientation != ScreenOrientation.Landscape && Screen.orientation != ScreenOrientation.LandscapeRight)
			{
				mCurrScreenOrientation = ScreenOrientation.Landscape;
				flag = true;
			}
			else if (!_LandscapeOnly && Screen.orientation != ScreenOrientation.Portrait)
			{
				mCurrScreenOrientation = ScreenOrientation.Portrait;
				flag = true;
			}
		}
		else if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
		{
			if (_LandscapeOnly && Screen.orientation != ScreenOrientation.Landscape && Screen.orientation != ScreenOrientation.LandscapeRight)
			{
				mCurrScreenOrientation = ScreenOrientation.Landscape;
				flag = true;
			}
			else if (!_LandscapeOnly && Screen.orientation != ScreenOrientation.PortraitUpsideDown)
			{
				mCurrScreenOrientation = ScreenOrientation.PortraitUpsideDown;
				flag = true;
			}
		}
		else if ((Input.deviceOrientation == DeviceOrientation.FaceUp || Input.deviceOrientation == DeviceOrientation.FaceDown) && _LandscapeOnly && Screen.orientation != ScreenOrientation.Landscape && Screen.orientation != ScreenOrientation.LandscapeRight)
		{
			mCurrScreenOrientation = ScreenOrientation.Landscape;
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		if (Screen.orientation != mCurrScreenOrientation)
		{
			Screen.orientation = mCurrScreenOrientation;
		}
		foreach (object listener in _listeners)
		{
			MethodInfo method = listener.GetType().GetMethod("OnOrient");
			if (method != null)
			{
				method.Invoke(listener, null);
			}
		}
	}

	public static void AddListener(object o)
	{
		if (mInstance != null)
		{
			mInstance._listeners.Add(o);
		}
		else
		{
			UtDebug.LogError("Orientation object not found!");
		}
	}

	public static ScreenOrientation GetOrientation()
	{
		if (!(pInstance == null) && UtPlatform.IsAndroid())
		{
			return pInstance.mCurrScreenOrientation;
		}
		return Screen.orientation;
	}

	public static void DisableAutoLandscapeRotation(bool set)
	{
		if (set)
		{
			mInstance.mPrevAutoRotationToLandscape = Screen.autorotateToLandscapeLeft;
			Screen.autorotateToLandscapeRight = false;
			Screen.autorotateToLandscapeLeft = false;
		}
		else
		{
			Screen.autorotateToLandscapeRight = mInstance.mPrevAutoRotationToLandscape;
			Screen.autorotateToLandscapeLeft = mInstance.mPrevAutoRotationToLandscape;
		}
	}
}
