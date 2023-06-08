using UnityEngine;

public class ObDestroySoundChannel : ObSelfDestructTimer
{
	public override void OnEnable()
	{
	}

	public void OnSnEnd(SnEvent snEvent)
	{
		if (_Pool != null)
		{
			_Pool.Despawn(base.transform);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
