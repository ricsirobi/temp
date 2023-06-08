using System;
using System.Collections;
using UnityEngine;

namespace JSGames.UI;

public class UITimerWidget : UIWidget
{
	public Action<UIWidget> OnCounterEnd;

	public Action<UIWidget> OnTimerEnd;

	private DateTime mStartTime;

	private float mTotalSeconds;

	private int mCount;

	private int mTotal;

	private float mRate;

	private string mTimerPrefixText = "";

	private string mTimerSuffixText = "";

	[SerializeField]
	private bool m_OverrideVisibilityControl;

	[SerializeField]
	private UIProgressBar m_ProgressBar;

	[SerializeField]
	private bool m_ReverseFillDirection = true;

	private Coroutine mUpdateProgress;

	private void OnEnable()
	{
		if (OnTimerEnd != null)
		{
			CheckTimerCompletion(mTotalSeconds, mStartTime, OnTimerEnd, mTimerPrefixText, mTimerSuffixText);
		}
		else if (OnCounterEnd != null)
		{
			CheckCounterCompletion(mTotal, mRate, mStartTime, OnCounterEnd);
		}
	}

	public void CheckTimerCompletion(float totalSeconds, DateTime startTime, Action<UIWidget> callback, string prefixString = "", string suffixString = "")
	{
		mTotalSeconds = totalSeconds;
		mStartTime = startTime;
		OnTimerEnd = callback;
		mTimerPrefixText = prefixString;
		mTimerSuffixText = suffixString;
		if ((float)(DateTime.UtcNow - mStartTime).TotalSeconds < mTotalSeconds)
		{
			if (!m_OverrideVisibilityControl)
			{
				pVisible = true;
			}
			if (mUpdateProgress != null)
			{
				StopCoroutine(mUpdateProgress);
			}
			mUpdateProgress = StartCoroutine(UpdateTimerProgress());
		}
		else
		{
			if (!m_OverrideVisibilityControl)
			{
				pVisible = false;
			}
			OnTimerEnd?.Invoke(this);
			OnTimerEnd = null;
		}
	}

	private IEnumerator UpdateTimerProgress()
	{
		TimeSpan completedTime = DateTime.UtcNow - mStartTime;
		while (completedTime.TotalSeconds < (double)mTotalSeconds)
		{
			completedTime = DateTime.UtcNow - mStartTime;
			SetTimerProgressBar((float)completedTime.TotalSeconds, mTotalSeconds);
			yield return null;
		}
		pVisible = false;
		OnTimerEnd?.Invoke(this);
		OnTimerEnd = null;
	}

	public void SetTimerProgressBar(float completedTime, float totalTimeToComplete)
	{
		if (_Text != null)
		{
			pText = mTimerPrefixText + GameUtilities.FormatTime(TimeSpan.FromSeconds(totalTimeToComplete - completedTime)) + mTimerSuffixText;
		}
		SetProgressBar(completedTime, totalTimeToComplete);
	}

	public void SetCounterProgressBar(int count, int total)
	{
		if (_Text != null)
		{
			pText = count.ToString();
		}
		SetProgressBar(count, total);
	}

	private void SetProgressBar(float completed, float total)
	{
		if (m_ProgressBar != null)
		{
			if (m_ReverseFillDirection)
			{
				m_ProgressBar.pProgress = 1f - completed / total;
			}
			else
			{
				m_ProgressBar.pProgress = completed / total;
			}
		}
		if (base.pAnim2D != null)
		{
			base.pAnim2D.Play(0);
		}
	}

	public void StopProgress()
	{
		if (mUpdateProgress != null)
		{
			StopCoroutine(mUpdateProgress);
		}
		if (m_ProgressBar != null)
		{
			m_ProgressBar.pProgress = 0f;
		}
		pVisible = false;
	}

	public void CheckCounterCompletion(int total, float rate, DateTime startTime, Action<UIWidget> callback)
	{
		mTotal = total;
		mStartTime = startTime;
		mRate = rate;
		float num = (float)(DateTime.UtcNow - startTime).TotalMinutes;
		mCount = (int)(num * rate);
		OnCounterEnd = callback;
		if (mCount < mTotal)
		{
			pVisible = true;
			if (mUpdateProgress != null)
			{
				StopCoroutine(mUpdateProgress);
			}
			mUpdateProgress = StartCoroutine(UpdateCounterProgress());
		}
		else
		{
			pVisible = false;
			OnCounterEnd?.Invoke(this);
			OnCounterEnd = null;
		}
	}

	private IEnumerator UpdateCounterProgress()
	{
		while (mCount < mTotal)
		{
			float num = (float)(DateTime.UtcNow - mStartTime).TotalMinutes;
			mCount = (int)(num * mRate);
			SetCounterProgressBar(mCount, mTotal);
			yield return new WaitForSeconds(1f);
		}
		pVisible = false;
		OnCounterEnd?.Invoke(this);
		OnCounterEnd = null;
	}
}
