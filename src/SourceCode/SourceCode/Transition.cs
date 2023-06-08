using System;
using UnityEngine;

public class Transition : MonoBehaviour
{
	public float _delayTime = 0.1f;

	private void Awake()
	{
		UtDebug.Log("Transition level started!!!", 25);
		RsResourceManager.ProcessTransitionLevel();
		MainStreetMMOClient.DestroyPool();
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	private void Update()
	{
		if (Time.timeSinceLevelLoad > _delayTime)
		{
			RsResourceManager.TransitionToLevel();
			base.enabled = false;
		}
	}
}
