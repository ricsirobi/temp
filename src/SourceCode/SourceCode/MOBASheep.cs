using UnityEngine;

public class MOBASheep : MOBAPather
{
	public float _Velocity = 0.5f;

	private Animation mAnim;

	public override void Init()
	{
		base.Init();
		mAnim = base.gameObject.GetComponentInChildren<Animation>();
		if (MOBASDManager.pInstance != null)
		{
			GameObject gameObject = ((_TeamID != 1) ? MOBASDManager.pInstance._Team2SheepPath : MOBASDManager.pInstance._Team1SheepPath);
			if (gameObject != null)
			{
				mPathTarget = gameObject.GetComponent<MOBAPathTarget>();
				PathReset();
			}
		}
	}

	protected override void EntityUpdate(bool bIsAuthority)
	{
		base.EntityUpdate(bIsAuthority);
		mPathVelocity = _Velocity;
		if ((bool)mAnim)
		{
			if (rVel > Mathf.Epsilon)
			{
				mAnim.Play("Walking");
				mAnim["Walking"].speed = rVel / 2f;
			}
			else
			{
				mAnim.Play("Eating");
			}
		}
	}
}
