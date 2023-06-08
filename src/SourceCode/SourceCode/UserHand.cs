using UnityEngine;

public class UserHand
{
	public delegate void OnFingerDown(int FingerIndex);

	public delegate void OnFingerUp(int FingerIndex);

	public delegate void OnFingerMoved(int FingerIndex);

	public delegate void OnDoubleTap(int FingerIndex);

	public delegate void OnFingerStationary(int FingerIndex);

	public OnFingerDown pFingerDownPntr;

	public OnFingerUp pFingerUpPntr;

	public OnFingerMoved pFingerMovedPntr;

	public OnDoubleTap pDoubleTapPntr;

	public OnFingerStationary pFingerStationary;

	private FakeTouchExt[] mAvgCurrFinger = new FakeTouchExt[2];

	private FakeTouchExt[] mAvgPrevFinger = new FakeTouchExt[2];

	private Vector2[] mAvgPositions = new Vector2[2];

	private int[] mAvgCounter = new int[2];

	private FakeTouchExt[,] mPrevFingers = new FakeTouchExt[20, 2];

	private bool bLocked;

	private const float MIN_GAP_BTW_2_CONT_FRAMES = 1f / 45f;

	private const int MAX_FRAMES_SAVED = 20;

	private const int MAX_FINGERS = 2;

	private const int MAX_AVERAGING_COUNT = 5;

	public FakeTouchExt[] pAvgCurrFinger => mAvgCurrFinger;

	public FakeTouchExt[] pAvgPrevFinger => mAvgPrevFinger;

	public UserHand()
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 20; j++)
			{
				mPrevFingers[j, i].dirty = true;
			}
		}
		mAvgCurrFinger[0].dirty = true;
		mAvgCurrFinger[0].phase = TouchPhase.Ended;
		mAvgCurrFinger[1].dirty = true;
		mAvgCurrFinger[1].phase = TouchPhase.Ended;
	}

	public void Reset()
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 20; j++)
			{
				mPrevFingers[j, i].dirty = true;
			}
		}
		mAvgCurrFinger[0].dirty = true;
		mAvgCurrFinger[0].phase = TouchPhase.Ended;
		mAvgCurrFinger[1].dirty = true;
		mAvgCurrFinger[1].phase = TouchPhase.Ended;
	}

	public int GetTouchCount()
	{
		int num = 0;
		for (int i = 0; i < 2; i++)
		{
			if (!pAvgCurrFinger[i].dirty && pAvgCurrFinger[i].phase != TouchPhase.Ended)
			{
				num++;
			}
		}
		return num;
	}

	public float GetGap()
	{
		if (GetTouchCount() <= 1)
		{
			return 0f;
		}
		return Vector2.Distance(pAvgCurrFinger[0].position, pAvgCurrFinger[1].position);
	}

	public float GetAngle()
	{
		if (GetTouchCount() <= 1)
		{
			return 0f;
		}
		return Mathf.Atan2(pAvgCurrFinger[0].position.x - pAvgCurrFinger[1].position.x, pAvgCurrFinger[0].position.y - pAvgCurrFinger[1].position.y) * 57.29578f;
	}

	public float GetAngle(Vector2 point1, Vector2 point2)
	{
		return Mathf.Atan2(point1.x - point2.x, point1.y - point2.y) * 57.29578f;
	}

	private FakeTouchExt ToFakeTouch(Touch tch)
	{
		FakeTouchExt result = default(FakeTouchExt);
		result.fingerId = tch.fingerId;
		result.position = tch.position;
		result.deltaPosition = tch.deltaPosition;
		result.phase = tch.phase;
		if (result.phase == TouchPhase.Stationary)
		{
			result.phase = TouchPhase.Moved;
		}
		if (result.phase == TouchPhase.Canceled)
		{
			result.phase = TouchPhase.Ended;
		}
		result.tapCount = tch.tapCount;
		result.dirty = false;
		result.time = Time.time;
		return result;
	}

	private void AddToAvgCounter(FakeTouchExt finger, int index)
	{
		if (finger.phase == TouchPhase.Began)
		{
			mAvgPositions[index] = Vector2.zero;
			mAvgCounter[index] = 0;
		}
		if (mAvgCounter[index] >= 5)
		{
			mAvgPositions[index] = finger.position + mAvgPositions[index] * (mAvgCounter[index] - 1);
		}
		else
		{
			mAvgPositions[index] = finger.position + mAvgPositions[index] * mAvgCounter[index];
			mAvgCounter[index]++;
		}
		mAvgPositions[index] /= (float)mAvgCounter[index];
	}

	private void AddNewFingerPosition(FakeTouchExt finger, int index)
	{
		AddToAvgCounter(finger, index);
		if (mPrevFingers[0, index].dirty || finger.phase != mPrevFingers[0, index].phase || !(mPrevFingers[0, index].time + 1f / 45f > finger.time))
		{
			for (int num = 19; num > 0; num--)
			{
				mPrevFingers[num, index] = mPrevFingers[num - 1, index];
			}
			mPrevFingers[0, index] = finger;
		}
	}

	private Vector2 GetAvgFingerPos(int index, int FrameCount)
	{
		Vector2 result = default(Vector2);
		int num = 0;
		if (FrameCount > 20)
		{
			FrameCount = 20;
		}
		for (int i = 0; i < FrameCount && !mPrevFingers[i, index].dirty && mPrevFingers[i, index].phase != TouchPhase.Ended; i++)
		{
			result += mPrevFingers[i, index].position;
			num++;
		}
		if (num > 0)
		{
			result /= (float)num;
		}
		return result;
	}

	public Vector2 GetAvgDisplacements(int index, int FrameCount)
	{
		Vector2 result = default(Vector2);
		int num = 0;
		if (FrameCount > 20)
		{
			FrameCount = 20;
		}
		for (int i = 0; i < FrameCount; i++)
		{
			if (i == 0)
			{
				if (mPrevFingers[i, index].dirty)
				{
					break;
				}
				continue;
			}
			if (mPrevFingers[i, index].dirty || mPrevFingers[i, index].phase == TouchPhase.Ended)
			{
				break;
			}
			result += mPrevFingers[i - 1, index].position - mPrevFingers[i, index].position;
			num++;
		}
		if (num > 0)
		{
			result /= (float)num;
		}
		return result;
	}

	public float GetAvgGap(int FrameCount)
	{
		float num = 0f;
		int num2 = 0;
		if (FrameCount > 20)
		{
			FrameCount = 20;
		}
		for (int i = 0; i < FrameCount; i++)
		{
			if (i == 0)
			{
				if (mPrevFingers[i, 0].dirty || mPrevFingers[i, 1].dirty)
				{
					break;
				}
				continue;
			}
			if (mPrevFingers[i, 0].dirty || mPrevFingers[i, 0].phase == TouchPhase.Ended || mPrevFingers[i, 1].dirty || mPrevFingers[i, 1].phase == TouchPhase.Ended)
			{
				break;
			}
			num += Vector2.Distance(mPrevFingers[i - 1, 0].position, mPrevFingers[i - 1, 1].position) - Vector2.Distance(mPrevFingers[i, 0].position, mPrevFingers[i, 1].position);
			num2++;
		}
		if (num2 > 0)
		{
			num /= (float)num2;
		}
		return num;
	}

	public float GetAvgDAngle(int FrameCount)
	{
		float num = 0f;
		int num2 = 0;
		if (FrameCount > 20)
		{
			FrameCount = 20;
		}
		for (int i = 0; i < FrameCount; i++)
		{
			if (i == 0)
			{
				if (mPrevFingers[i, 0].dirty || mPrevFingers[i, 1].dirty)
				{
					break;
				}
				continue;
			}
			if (mPrevFingers[i, 0].dirty || mPrevFingers[i, 0].phase == TouchPhase.Ended || mPrevFingers[i, 1].dirty || mPrevFingers[i, 1].phase == TouchPhase.Ended)
			{
				break;
			}
			num += GetAngle(mPrevFingers[i - 1, 0].position, mPrevFingers[i - 1, 1].position) - GetAngle(mPrevFingers[i, 0].position, mPrevFingers[i, 1].position);
			num2++;
		}
		if (num2 > 0)
		{
			num /= (float)num2;
		}
		return num;
	}

	private void RaiseForcedTouch(TouchPhase phase, int index)
	{
		switch (phase)
		{
		case TouchPhase.Ended:
		{
			FakeTouchExt prevFinger = GetPrevFinger(index);
			prevFinger.phase = TouchPhase.Ended;
			AddNewFingerPosition(prevFinger, prevFinger.fingerId);
			mAvgPrevFinger[prevFinger.fingerId] = mAvgCurrFinger[prevFinger.fingerId];
			mAvgCurrFinger[prevFinger.fingerId] = prevFinger;
			mAvgCurrFinger[prevFinger.fingerId].position = mAvgPositions[prevFinger.fingerId];
			mAvgCurrFinger[prevFinger.fingerId].dirty = true;
			if (pFingerUpPntr != null)
			{
				bLocked = true;
				pFingerUpPntr(prevFinger.fingerId);
				bLocked = false;
			}
			break;
		}
		default:
			_ = 1;
			break;
		case TouchPhase.Began:
			break;
		}
	}

	public void UpdateTouch()
	{
		if (bLocked)
		{
			return;
		}
		if (KAInput.GetMouseButtonDown(0))
		{
			FakeTouchExt fakeTouchExt = default(FakeTouchExt);
			fakeTouchExt.phase = TouchPhase.Began;
			fakeTouchExt.position = new Vector2(KAInput.mousePosition.x, KAInput.mousePosition.y);
			fakeTouchExt.fingerId = 1;
			fakeTouchExt.dirty = false;
			fakeTouchExt.time = Time.time;
			AddNewFingerPosition(fakeTouchExt, 1);
			mAvgPrevFinger[1] = mAvgCurrFinger[1];
			mAvgCurrFinger[1] = fakeTouchExt;
			mAvgCurrFinger[1].position = mAvgPositions[1];
			if (pFingerDownPntr != null)
			{
				bLocked = true;
				pFingerDownPntr(fakeTouchExt.fingerId);
				bLocked = false;
			}
		}
		else if (KAInput.GetMouseButtonUp(0))
		{
			FakeTouchExt fakeTouchExt2 = default(FakeTouchExt);
			fakeTouchExt2.phase = TouchPhase.Ended;
			fakeTouchExt2.position = new Vector2(KAInput.mousePosition.x, KAInput.mousePosition.y);
			fakeTouchExt2.fingerId = 1;
			fakeTouchExt2.dirty = false;
			fakeTouchExt2.time = Time.time;
			AddNewFingerPosition(fakeTouchExt2, 1);
			mAvgPrevFinger[1] = mAvgCurrFinger[1];
			mAvgCurrFinger[1] = fakeTouchExt2;
			mAvgCurrFinger[1].position = mAvgPositions[1];
			mAvgCurrFinger[1].dirty = true;
			if (pFingerUpPntr != null)
			{
				bLocked = true;
				pFingerUpPntr(fakeTouchExt2.fingerId);
				bLocked = false;
			}
		}
		else if (KAInput.GetMouseButton(0))
		{
			FakeTouchExt fakeTouchExt3 = default(FakeTouchExt);
			fakeTouchExt3.phase = TouchPhase.Moved;
			fakeTouchExt3.position = new Vector2(KAInput.mousePosition.x, KAInput.mousePosition.y);
			fakeTouchExt3.fingerId = 1;
			fakeTouchExt3.dirty = false;
			fakeTouchExt3.time = Time.time;
			AddNewFingerPosition(fakeTouchExt3, 1);
			mAvgPrevFinger[1] = mAvgCurrFinger[1];
			mAvgCurrFinger[1] = fakeTouchExt3;
			mAvgCurrFinger[1].position = mAvgPositions[1];
			if (pFingerMovedPntr != null)
			{
				bLocked = true;
				pFingerMovedPntr(fakeTouchExt3.fingerId);
				bLocked = false;
			}
		}
	}

	public FakeTouchExt GetCurrFinger(int index)
	{
		return mPrevFingers[0, index];
	}

	public FakeTouchExt GetPrevFinger(int index)
	{
		return mPrevFingers[1, index];
	}
}
