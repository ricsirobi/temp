using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class Loom : MonoBehaviour
{
	public struct DelayedQueueItem
	{
		public float time;

		public Action action;
	}

	public static int mMaxThreads = 8;

	private static int mNumThreads;

	private static bool mInitialized;

	private static Loom mCurrent;

	private int mCount;

	private List<Action> mActions = new List<Action>();

	private List<DelayedQueueItem> mDelayed = new List<DelayedQueueItem>();

	private List<DelayedQueueItem> mCurrentDelayed = new List<DelayedQueueItem>();

	private List<Action> mCurrentActions = new List<Action>();

	public static Loom pCurrent
	{
		get
		{
			Initialize();
			return mCurrent;
		}
	}

	private void Awake()
	{
		mCurrent = this;
		mInitialized = true;
	}

	private static void Initialize()
	{
		if ((!mInitialized || mCurrent == null) && Application.isPlaying)
		{
			mInitialized = true;
			GameObject obj = new GameObject("Loom");
			mCurrent = obj.AddComponent<Loom>();
			UnityEngine.Object.DontDestroyOnLoad(obj);
		}
	}

	private void OnDestroy()
	{
		mInitialized = false;
		mCurrent = null;
	}

	private void OnDisable()
	{
		if (mCurrent == this)
		{
			mCurrent = null;
		}
	}

	public static void QueueOnMainThread(Action action)
	{
		QueueOnMainThread(action, 0f);
	}

	public static void QueueOnMainThread(Action action, float time)
	{
		if (time != 0f)
		{
			lock (pCurrent.mDelayed)
			{
				pCurrent.mDelayed.Add(new DelayedQueueItem
				{
					time = Time.time + time,
					action = action
				});
				return;
			}
		}
		lock (pCurrent.mActions)
		{
			pCurrent.mActions.Add(action);
		}
	}

	public static Thread RunAsync(Action a)
	{
		Initialize();
		while (mNumThreads >= mMaxThreads)
		{
			Thread.Sleep(1);
		}
		Interlocked.Increment(ref mNumThreads);
		ThreadPool.QueueUserWorkItem(RunAction, a);
		return null;
	}

	private static void RunAction(object action)
	{
		try
		{
			((Action)action)();
		}
		catch
		{
		}
		finally
		{
			Interlocked.Decrement(ref mNumThreads);
		}
	}

	private void Update()
	{
		lock (mActions)
		{
			mCurrentActions.Clear();
			mCurrentActions.AddRange(mActions);
			mActions.Clear();
		}
		foreach (Action mCurrentAction in mCurrentActions)
		{
			mCurrentAction();
		}
		lock (mDelayed)
		{
			mCurrentDelayed.Clear();
			mCurrentDelayed.AddRange(mDelayed.Where((DelayedQueueItem d) => d.time <= Time.time));
			foreach (DelayedQueueItem item in mCurrentDelayed)
			{
				mDelayed.Remove(item);
			}
		}
		foreach (DelayedQueueItem item2 in mCurrentDelayed)
		{
			item2.action();
		}
	}
}
