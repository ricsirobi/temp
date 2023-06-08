using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeHackPrevent : KAMonoBase
{
	public TimerDataSet[] _DataSet;

	public double _DefaultTolerance = 60.0;

	public double _DefaultCheckInterval = 120.0;

	private double mTolerance = 60.0;

	private double mCheckInterval = 120.0;

	public LocaleString _HackWarningText = new LocaleString("Game time is tampared. Please launch again.");

	private bool mStartChecking;

	private DateTime mSystemTime;

	private DateTime mCachedAppTime;

	private bool mHackDetected;

	public static TimeHackPrevent pInstance { get; set; }

	private void Start()
	{
		if (UtPlatform.IsWSA() || UtPlatform.IsStandAlone())
		{
			if (pInstance == null)
			{
				pInstance = this;
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
				SceneManager.sceneUnloaded += OnLevelUnloaded;
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public static bool Set(string inID)
	{
		if (pInstance != null && pInstance._DataSet != null && pInstance._DataSet.Length != 0)
		{
			TimerDataSet timerDataSet = Array.Find(pInstance._DataSet, (TimerDataSet dSet) => dSet._ID == inID);
			if (timerDataSet != null)
			{
				pInstance.mCheckInterval = timerDataSet._CheckInterval;
				pInstance.mTolerance = timerDataSet._Tolerance;
			}
			else
			{
				pInstance.mTolerance = pInstance._DefaultTolerance;
				pInstance.mCheckInterval = pInstance._DefaultCheckInterval;
			}
			pInstance.Init();
			return true;
		}
		return false;
	}

	public static bool Reset()
	{
		if (pInstance != null)
		{
			pInstance.mTolerance = pInstance._DefaultTolerance;
			pInstance.mCheckInterval = pInstance._DefaultCheckInterval;
			return true;
		}
		return false;
	}

	private void Init()
	{
		ResetTimers();
		mStartChecking = true;
	}

	private void ResetTimers()
	{
		UtDebug.Log("old sys time:: " + mSystemTime.ToString() + " old app time:: " + mCachedAppTime);
		mSystemTime = DateTime.UtcNow;
		mCachedAppTime = ServerTime.pCurrentTime;
		UtDebug.Log("new sys time:: " + mSystemTime.ToString() + " new app time:: " + mCachedAppTime);
	}

	private void Update()
	{
		if (!mStartChecking || mHackDetected || RsResourceManager.IsLoading())
		{
			return;
		}
		double totalSeconds = (ServerTime.pCurrentTime - mCachedAppTime).TotalSeconds;
		if (totalSeconds >= mCheckInterval)
		{
			double totalSeconds2 = (DateTime.UtcNow - mSystemTime).TotalSeconds;
			double num = totalSeconds - totalSeconds2;
			if (num < 0.0)
			{
				num *= -1.0;
			}
			UtDebug.Log("appTimeDiff:: " + totalSeconds + " ::sysTimeDiff:: " + totalSeconds2 + " ::timeDiff:: " + num);
			if (num > mTolerance)
			{
				KickPlayer();
			}
			else
			{
				ResetTimers();
			}
		}
	}

	private void KickPlayer()
	{
		mHackDetected = true;
		ServiceRequest.pBlockRequest = true;
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _HackWarningText.GetLocalizedString(), "Timer Alert", base.gameObject, "OnPlayerForceQuit");
	}

	private void OnPlayerForceQuit()
	{
		Application.Quit();
	}

	private void OnLevelUnloaded(Scene current)
	{
		mStartChecking = false;
	}

	private void OnDestroy()
	{
		SceneManager.sceneUnloaded -= OnLevelUnloaded;
	}
}
