using System.Collections.Generic;
using UnityEngine;

public class ObSyncSplineControlManager : MonoBehaviour
{
	public List<ObSyncSplineControl> SyncSplineControlList = new List<ObSyncSplineControl>();

	public float GetElapsedTimeInterval = 10f;

	private float mTimer;

	private float mLastCallTime;

	private void Update()
	{
		mTimer -= Time.deltaTime;
		if (mTimer <= 0f)
		{
			mTimer = GetElapsedTimeInterval;
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SendGetElapsedTimeEvent(base.gameObject);
				mLastCallTime = Time.time;
			}
		}
	}

	private void OnGetElapsedTimeEvent(float inValue)
	{
		float num = Time.time - mLastCallTime;
		float time = inValue + num * 0.5f;
		foreach (ObSyncSplineControl syncSplineControl in SyncSplineControlList)
		{
			syncSplineControl.OnGetElapsedTimeEvent(time);
		}
	}
}
