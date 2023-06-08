using System;
using UnityEngine;

public class TouchInfo
{
	[Flags]
	public enum State
	{
		NONE = 0,
		DRAGGING = 1,
		ROTATE = 2,
		ZOOM = 3
	}

	private Vector2 mPosition;

	private Vector2 mLastPosition;

	private float mTime;

	private int mFingerID;

	private State mState;

	public Vector2 pPosition
	{
		get
		{
			return mPosition;
		}
		set
		{
			mPosition = value;
		}
	}

	public Vector2 pLastPosition
	{
		get
		{
			return mLastPosition;
		}
		set
		{
			mLastPosition = value;
		}
	}

	public float pTime
	{
		get
		{
			return mTime;
		}
		set
		{
			mTime = value;
		}
	}

	public int pFingerID
	{
		get
		{
			return mFingerID;
		}
		set
		{
			mFingerID = value;
		}
	}

	public State pState
	{
		get
		{
			return mState;
		}
		set
		{
			mState = value;
		}
	}

	public TouchInfo(Vector2 inPos, int inFId, float inTime)
	{
		mLastPosition = (mPosition = inPos);
		mTime = inTime;
		mFingerID = inFId;
		mState = State.NONE;
	}
}
