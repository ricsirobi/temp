using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class FogInfo
{
	public MinMax _Step = new MinMax(0.005f, 0.01f);

	public float _StepDuration = 1f;

	[Tooltip("How long does it take the fog on this axis to return to normal after the scrolling condition is no longer met.")]
	public float _ResetDuration = 0.4f;

	private FogState mFogState = FogState.IDLE;

	private IEnumerator mFogCoroutine;

	public FogState pFogState
	{
		get
		{
			return mFogState;
		}
		set
		{
			mFogState = value;
		}
	}

	public IEnumerator pFogCoroutine
	{
		get
		{
			return mFogCoroutine;
		}
		set
		{
			mFogCoroutine = value;
		}
	}
}
