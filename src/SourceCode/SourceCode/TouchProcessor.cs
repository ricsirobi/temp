using System;
using UnityEngine;

public class TouchProcessor : InputProcessor
{
	private InputInfo mInfo;

	private TouchID mLastTouchId;

	private float mRegTime = float.MaxValue;

	private const float DELAY_TIME = 0.2f;

	public TouchID pLastTouchId
	{
		get
		{
			return mLastTouchId;
		}
		set
		{
			mLastTouchId = value;
		}
	}

	public TouchProcessor(InputInfo inInfo)
	{
		mInfo = inInfo;
		if (TouchManager.pInstance == null)
		{
			TouchManager.Init();
		}
		if (TouchManager.pInstance != null)
		{
			TouchManager.OnFingerUpEvent = (OnFingerUp)Delegate.Combine(TouchManager.OnFingerUpEvent, new OnFingerUp(OnFingerUp));
			TouchManager.OnTapEvent = (OnTap)Delegate.Combine(TouchManager.OnTapEvent, new OnTap(OnTap));
			TouchManager.OnFlickEvent = (OnFlick)Delegate.Combine(TouchManager.OnFlickEvent, new OnFlick(OnFlick));
		}
	}

	public bool Update(GameInput inInput)
	{
		if (mLastTouchId != 0 && (int)((uint)mLastTouchId & (uint)mInfo._TouchEvents) > 0)
		{
			inInput._Value = Mathf.Lerp(inInput._Value, mInfo._MaxVal, Time.deltaTime * Mathf.Abs(mInfo._Step));
			return true;
		}
		return false;
	}

	public void LateUpdate(GameInput inInput)
	{
		if (mLastTouchId != 0 && Time.time > mRegTime)
		{
			mLastTouchId = TouchID.NONE;
		}
	}

	public bool IsPressed()
	{
		return IsTouched();
	}

	public bool IsUp()
	{
		return !IsTouched();
	}

	public bool IsDown()
	{
		return IsTouched();
	}

	private bool IsTouched()
	{
		if ((int)((uint)pLastTouchId & (uint)mInfo._TouchEvents) > 0)
		{
			return true;
		}
		return false;
	}

	public void OnFingerUp(int inFingerID, Vector2 inVecPosition)
	{
		pLastTouchId = TouchID.SINGLE_TAP;
		mRegTime = Time.time + 0.2f;
	}

	public void OnTap(int inFingerID, Vector2 inVecPosition)
	{
		pLastTouchId = TouchID.DOUBLE_TAP;
		mRegTime = Time.time + 0.2f;
	}

	public void OnFlick(int inFingerID, Vector2 inVecPosition, Vector2 inDeltaPos, int inDirection, int inMagnitude)
	{
		switch (inDirection)
		{
		case 1:
			pLastTouchId = TouchID.SWIPE_UP;
			break;
		case 5:
			pLastTouchId = TouchID.SWIPE_DOWN;
			break;
		case 3:
			pLastTouchId = TouchID.SWIPE_LEFT;
			break;
		case 7:
			pLastTouchId = TouchID.SWIPE_RIGHT;
			break;
		case 2:
			pLastTouchId = TouchID.SWIPE_DIGONAL_TOP_LEFT;
			break;
		case 8:
			pLastTouchId = TouchID.SWIPE_DIGONAL_TOP_RIGHT;
			break;
		case 4:
			pLastTouchId = TouchID.SWIPE_DIGONAL_BOTTOM_LEFT;
			break;
		case 6:
			pLastTouchId = TouchID.SWIPE_DIGONAL_BOTTOM_RIGHT;
			break;
		}
		mRegTime = Time.time + 0.2f;
	}

	public void Calibrate()
	{
	}

	public void SetCalibration(float x, float y, float z)
	{
	}
}
