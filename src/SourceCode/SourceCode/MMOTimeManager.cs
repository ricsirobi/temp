using System;
using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class MMOTimeManager : MonoBehaviour
{
	public delegate void TimeSyncCallback();

	public static float _Period = 5f;

	private static bool mShowTime = false;

	private static MMOTimeManager mInstance;

	private float mLastRequestTime = float.MaxValue;

	private float mTimeBeforeSync;

	private double mLastServerTime;

	private double mLastLocalTime;

	private DateTime mLastServerDateTime;

	private DateTime mLastLocalDateTime;

	private double mAveragePing;

	private int mPingCount;

	private bool mIsTimeSynced;

	private readonly int mAveragePingCount = 5;

	private double[] mPingValues;

	private int mPingValueIndex;

	private bool mIsInitialized;

	public TimeSyncCallback OnMMOTimeSyncComplete;

	public static MMOTimeManager pInstance => mInstance;

	public double pAveragePing => mAveragePing;

	public int pPingCount => mPingCount;

	public bool pIsTimeSynced => mIsTimeSynced;

	private void Awake()
	{
		mInstance = this;
	}

	public void Start()
	{
		mPingValues = new double[mAveragePingCount];
		mPingCount = 0;
		mPingValueIndex = 0;
	}

	public void ReinitializeTimeManager()
	{
		mIsInitialized = false;
		mPingValues = new double[mAveragePingCount];
		mPingCount = 0;
		mPingValueIndex = 0;
		mLastLocalTime = 0.0;
		mLastServerTime = 0.0;
		mLastRequestTime = float.MaxValue;
		mAveragePing = 0.0;
		mTimeBeforeSync = 0f;
		mIsTimeSynced = false;
	}

	private void OnGUI()
	{
		if (mShowTime && pInstance != null)
		{
			GUI.Box(new Rect(100f, 100f, 200f, 40f), GetServerDateTime().ToString());
			GUI.Box(new Rect(100f, 160f, 200f, 40f), GetServerDateTimeMilliseconds().ToString());
		}
	}

	public static void ShowTime()
	{
		mShowTime = !mShowTime;
	}

	public double GetLastLocalTimeDifference()
	{
		if (mLastLocalTime > 0.0)
		{
			return (double)Time.time - mLastLocalTime;
		}
		return mLastLocalTime;
	}

	public void Synchronize(double timeValue)
	{
		double ping = (Time.time - mTimeBeforeSync) * 1000f;
		CalculateAveragePing(ping);
		double num = mAveragePing / 2.0;
		mLastServerTime = timeValue + num;
		mLastLocalTime = Time.time;
	}

	public void SynchronizeDate(DateTime dateValue)
	{
		double ping = (Time.time - mTimeBeforeSync) * 1000f;
		CalculateAveragePing(ping);
		double value = mAveragePing / 2.0;
		mLastServerDateTime = dateValue.AddMilliseconds(value);
		mLastLocalDateTime = DateTime.Now;
		mIsTimeSynced = true;
		if (OnMMOTimeSyncComplete != null)
		{
			OnMMOTimeSyncComplete();
		}
	}

	private void Update()
	{
		if (MainStreetMMOClient.pInstance == null || MainStreetMMOClient.pInstance.pState != MMOClientState.IN_ROOM)
		{
			if (mIsInitialized)
			{
				ReinitializeTimeManager();
			}
			return;
		}
		if (!mIsInitialized)
		{
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("PNG", PingResponseEventHandler);
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("DT", GetDateTimeResponseEventHandler);
			mIsInitialized = true;
		}
		if (mLastRequestTime > _Period)
		{
			mLastRequestTime = 0f;
			mTimeBeforeSync = Time.time;
			if (Application.isEditor)
			{
				mTimeBeforeSync -= 0.001f;
			}
			TimeSyncRequest();
		}
		else
		{
			mLastRequestTime += Time.deltaTime;
		}
	}

	private void TimeSyncRequest()
	{
		Dictionary<string, object> inParams = new Dictionary<string, object>();
		MainStreetMMOClient.pInstance.SendExtensionMessage("", "PNG", inParams);
		MainStreetMMOClient.pInstance.SendExtensionMessage("", "DT", inParams);
	}

	private void PingResponseEventHandler(object sender, MMOExtensionResponseReceivedEventArgs args)
	{
		string value = args.ResponseDataObject["1"].ToString();
		Synchronize(Convert.ToDouble(value));
	}

	private void GetDateTimeResponseEventHandler(object sender, MMOExtensionResponseReceivedEventArgs args)
	{
		string s = args.ResponseDataObject["1"].ToString();
		SynchronizeDate(DateTime.Parse(s, UtUtilities.GetCultureInfo("en-US")));
	}

	public double GetServerTime()
	{
		return ((double)Time.time - mLastLocalTime) * 1000.0 + mLastServerTime;
	}

	public DateTime GetServerDateTime()
	{
		return mLastServerDateTime.AddMinutes((DateTime.Now - mLastLocalDateTime).Minutes);
	}

	public DateTime GetServerDateTimeMilliseconds()
	{
		return mLastServerDateTime.AddMilliseconds((DateTime.Now - mLastLocalDateTime).TotalMilliseconds);
	}

	private void CalculateAveragePing(double ping)
	{
		mPingValues[mPingValueIndex] = ping;
		mPingValueIndex++;
		if (mPingValueIndex >= mAveragePingCount)
		{
			mPingValueIndex = 0;
		}
		if (mPingCount < mAveragePingCount)
		{
			mPingCount++;
		}
		double num = 0.0;
		for (int i = 0; i < mPingCount; i++)
		{
			num += mPingValues[i];
		}
		mAveragePing = num / (double)mPingCount;
	}
}
