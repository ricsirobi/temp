public class ExtraTimePowerUp : PowerUp
{
	public override void Activate()
	{
		base.Activate();
		if (mPowerUpManager.pPowerUpHelper._ApplyExtraTime != null)
		{
			mPowerUpManager.pPowerUpHelper._ApplyExtraTime(mActive, _Duration);
		}
	}
}
