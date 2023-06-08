using System.Collections;
using UnityEngine;

public class UtTimerManager : MonoBehaviour
{
	private static UtTimerManager mInstance;

	internal static void StartTimer(UtTimer inTimer)
	{
		UtTimerManager utTimerManager = mInstance;
		if (utTimerManager == null)
		{
			utTimerManager = new GameObject
			{
				name = "TimerManager"
			}.AddComponent(typeof(UtTimerManager)) as UtTimerManager;
		}
		if (!inTimer.mTimer.mIsRunning)
		{
			utTimerManager.StartCoroutine(mInstance.StartTimerRoutine(inTimer.mTimer));
		}
	}

	private IEnumerator StartTimerRoutine(UtTimer.Timer inTimer)
	{
		inTimer.mIsRunning = true;
		WaitForSeconds mWaitForSeconds = new WaitForSeconds(inTimer.mInterval * 0.001f);
		while (inTimer.mIsRunning)
		{
			yield return mWaitForSeconds;
			if (inTimer.mElapsed != null && inTimer.mIsRunning)
			{
				inTimer.mElapsed(inTimer);
			}
		}
	}

	private void Awake()
	{
		mInstance = this;
	}
}
