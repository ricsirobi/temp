using UnityEngine;

public class ObFarmHarvestEmitter : ObBouncyCoinEmitter
{
	public bool _AutoDestroy = true;

	public override void Update()
	{
		bool flag = mCoinsToEmit > 0;
		base.Update();
		if (_AutoDestroy && flag && mCoinsToEmit <= 0)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
