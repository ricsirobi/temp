using System;
using UnityEngine;

public class CountDownTimer
{
	private double mTotalTime;

	private DateTime mEndTime = DateTime.MinValue;

	private TimeSpan mTimeRemain = TimeSpan.MinValue;

	private float mPercentRemain;

	private bool mIsTimerRunning;

	private bool mIsUINeedUpdate;

	private CountDownEndCallBack mCallback;

	private float mTickInterval;

	private float mTimeCounter;

	public double pTotalTime => mTotalTime;

	public DateTime pStartTime
	{
		get
		{
			if (mTotalTime > 0.0)
			{
				return mEndTime - TimeSpan.FromHours(mTotalTime);
			}
			return DateTime.MinValue;
		}
	}

	public DateTime pEndTime
	{
		get
		{
			return mEndTime;
		}
		set
		{
			mEndTime = value;
		}
	}

	public bool pIsTimerUIMarkDirty
	{
		get
		{
			return mIsUINeedUpdate;
		}
		set
		{
			mIsUINeedUpdate = value;
		}
	}

	public string pTimeInHHMMSS => $"{mTimeRemain.Hours.ToString():00}:{mTimeRemain.Minutes.ToString():00}:{mTimeRemain.Seconds.ToString():00}";

	public double pTimeInSecs => mTimeRemain.TotalSeconds;

	public float pPercentRemain => mPercentRemain;

	public bool pIsCountDownActive => mEndTime > ServerTime.pCurrentTime;

	public bool pIsTimerRunning => mIsTimerRunning;

	public void SetTickInterval(float duration)
	{
		mTickInterval = duration;
	}

	public void Reset()
	{
		mTotalTime = 0.0;
		mEndTime = DateTime.MinValue;
		mPercentRemain = 0f;
		mTimeRemain = TimeSpan.MinValue;
		mIsTimerRunning = false;
		mIsUINeedUpdate = false;
	}

	public void SetCountDownEndCallback(CountDownEndCallBack countDownEndCallback)
	{
		mCallback = countDownEndCallback;
	}

	public void StartTimer(double forHours, float tickInterval = 0f)
	{
		mTotalTime = forHours;
		mEndTime = ServerTime.pCurrentTime.AddHours(mTotalTime);
		mTickInterval = tickInterval;
		mTimeCounter = mTickInterval;
		if (pIsCountDownActive)
		{
			mIsTimerRunning = true;
			mTimeRemain = mEndTime - ServerTime.pCurrentTime;
			mPercentRemain = (float)(mTimeRemain.TotalHours / mTotalTime);
			mIsUINeedUpdate = true;
		}
		UtDebug.Log("CountDownTimer.StartTimer() <> => pTotalTime:" + mTotalTime + " <> EndTime:" + mEndTime);
	}

	public void StartTimer(double hours, DateTime endTime, float tickInterval = 0f)
	{
		mTotalTime = hours;
		mEndTime = endTime;
		mTickInterval = tickInterval;
		mTimeCounter = mTickInterval;
		if (pIsCountDownActive)
		{
			mIsTimerRunning = true;
			mTimeRemain = mEndTime - ServerTime.pCurrentTime;
			mPercentRemain = (float)(mTimeRemain.TotalHours / mTotalTime);
			mIsUINeedUpdate = true;
		}
		UtDebug.Log("CountDownTimer.StartTimer() <> => pTotalTime:" + mTotalTime + " <> EndTime:" + mEndTime);
	}

	public void StopTimer()
	{
		mIsTimerRunning = false;
	}

	public void UpdateCountDown()
	{
		if (!mIsTimerRunning)
		{
			return;
		}
		mTimeCounter += Time.deltaTime;
		if (mTimeCounter >= mTickInterval)
		{
			mTimeCounter = 0f;
			mTimeRemain = mEndTime - ServerTime.pCurrentTime;
			mPercentRemain = (float)(mTimeRemain.TotalHours / mTotalTime);
			mIsUINeedUpdate = true;
		}
		UtDebug.Log("CountDownTimer.UpdateCountDown() <> DisplayTime:" + mTimeRemain.ToString() + " <> pPercentRemain:" + mPercentRemain);
		if (!pIsCountDownActive)
		{
			StopTimer();
			if (mCallback != null)
			{
				mCallback();
			}
		}
	}

	public static DateTime GetEndTimeFromHours(double hours)
	{
		DateTime result = DateTime.MinValue;
		if (hours > 0.0)
		{
			result = ServerTime.pCurrentTime.AddHours(hours);
		}
		return result;
	}
}
