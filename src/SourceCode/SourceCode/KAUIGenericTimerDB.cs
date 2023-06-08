using System;
using UnityEngine;

public class KAUIGenericTimerDB : KAUIGenericDB
{
	private float mTickInterval = 1f;

	private float mTimeCounter;

	private KAWidget mDBTimerText;

	private DateTime mEndTime = DateTime.MinValue;

	private bool mCanUpdateTimer;

	private bool mShowTimer;

	private TimeFormat mTimeFormatType;

	private bool mAutoCloseOnTimeUp;

	protected override void Start()
	{
		base.Start();
		mDBTimerText = FindItem("TxtDialogTimer");
		mCanUpdateTimer = true;
		if (mDBTimerText != null)
		{
			mDBTimerText.SetVisibility(mShowTimer);
		}
		UpdateTime();
	}

	protected override void Update()
	{
		base.Update();
		if (mCanUpdateTimer)
		{
			mTimeCounter += Time.deltaTime;
			if (mTimeCounter >= mTickInterval)
			{
				mTimeCounter = 0f;
				UpdateTime();
			}
		}
	}

	public void Init(bool showTimer, DateTime endTime, TimeFormat timeFormat = TimeFormat.SS, bool autoCloseOnTimeUp = true, float tickTime = 1f)
	{
		mShowTimer = showTimer;
		mEndTime = endTime;
		mTimeFormatType = timeFormat;
		mAutoCloseOnTimeUp = autoCloseOnTimeUp;
		mTickInterval = tickTime;
		if (mDBTimerText != null)
		{
			mDBTimerText.SetVisibility(mShowTimer);
		}
	}

	public void Init(bool showTimer, double second, TimeFormat timeFormat = TimeFormat.SS, bool autoCloseOnTimeUp = true, float tickTime = 1f)
	{
		mShowTimer = showTimer;
		mEndTime = ServerTime.pCurrentTime.AddSeconds(second);
		mTimeFormatType = timeFormat;
		mAutoCloseOnTimeUp = autoCloseOnTimeUp;
		if (mDBTimerText != null)
		{
			mDBTimerText.SetVisibility(mShowTimer);
		}
	}

	private void UpdateTime()
	{
		TimeSpan time = mEndTime - ServerTime.pCurrentTime;
		if (time.TotalSeconds >= 0.0)
		{
			SetTime(time);
			return;
		}
		SetTime(TimeSpan.Zero);
		mCanUpdateTimer = false;
		if (mAutoCloseOnTimeUp && (int)time.TotalSeconds <= 0)
		{
			Destroy();
		}
	}

	private void SetTime(TimeSpan timeRemain)
	{
		if ((bool)mDBTimerText)
		{
			string text = "00";
			text = mTimeFormatType switch
			{
				TimeFormat.HHMMSS => $"{timeRemain.Hours:d2}:{timeRemain.Minutes:d2}:{timeRemain.Seconds:d2}", 
				TimeFormat.HHMM => $"{timeRemain.Hours:d2}:{timeRemain.Minutes:d2}", 
				TimeFormat.MMSS => $"{timeRemain.Hours * 60 + timeRemain.Minutes:d2}:{timeRemain.Seconds:d2}", 
				TimeFormat.HH => $"{(int)timeRemain.TotalHours:d2}", 
				TimeFormat.MM => $"{(int)timeRemain.TotalMinutes:d2}", 
				TimeFormat.SS => $"{(int)timeRemain.TotalSeconds:d2}", 
				_ => $"{timeRemain.Hours:d2}:{timeRemain.Minutes:d2}:{timeRemain.Seconds:d2}", 
			};
			mDBTimerText.SetText(text);
		}
	}
}
