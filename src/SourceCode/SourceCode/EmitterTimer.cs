using UnityEngine;

public class EmitterTimer : KAMonoBase
{
	public float _TimeLimit;

	private float mFinishTime;

	private void Start()
	{
		mFinishTime = Time.time + _TimeLimit;
	}

	private void Update()
	{
		if (Time.time > mFinishTime)
		{
			base.particleSystem.Stop();
			if (base.particleSystem.particleCount == 0)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
