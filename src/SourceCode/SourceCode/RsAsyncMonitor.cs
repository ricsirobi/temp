using UnityEngine;

public class RsAsyncMonitor : MonoBehaviour
{
	private static GameObject mObject;

	private static AsyncOperation mLoader;

	private static string mLevel;

	private static RsResourceEventHandler mEventDelegate;

	private static RsAsyncMonitor mInstance;

	private float mPrevious;

	public static RsAsyncMonitor pInstance
	{
		get
		{
			if (mInstance == null)
			{
				Init();
			}
			return mInstance;
		}
	}

	public static void Init()
	{
		if (mObject == null)
		{
			mObject = new GameObject("PfAsyncMonitor");
			Object.DontDestroyOnLoad(mObject);
			mInstance = mObject.AddComponent<RsAsyncMonitor>();
		}
	}

	public void AddTarget(AsyncOperation newTarget, string level, RsResourceEventHandler inCallback)
	{
		if (mLoader == null)
		{
			mLoader = newTarget;
			mLevel = level;
			mEventDelegate = inCallback;
		}
		else
		{
			UtDebug.LogError("AddNewTarget failed");
		}
	}

	private void Update()
	{
		if (mLoader != null)
		{
			if (mLoader.isDone)
			{
				mEventDelegate(mLevel, RsResourceLoadEvent.COMPLETE, 1f, null, null);
				mLoader = null;
			}
			else if (mLoader.progress > mPrevious + 0.25f)
			{
				mEventDelegate(mLevel, RsResourceLoadEvent.PROGRESS, mLoader.progress, null, null);
				mPrevious = mLoader.progress;
			}
		}
	}
}
