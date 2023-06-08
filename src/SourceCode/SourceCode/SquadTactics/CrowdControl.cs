namespace SquadTactics;

public class CrowdControl : Effect
{
	public CrowdControlEnum _Type;

	public EffectName _AppliedEffect;

	public override void Activate()
	{
		foreach (Effect pActiveStatusEffect in mOwner.pActiveStatusEffects)
		{
			if (pActiveStatusEffect is CrowdControl)
			{
				CrowdControl crowdControl = (CrowdControl)pActiveStatusEffect;
				if (crowdControl._Type == _Type)
				{
					mOwner.RemoveStatusEffect(crowdControl);
					break;
				}
			}
		}
		mOwner.TakeCrowdControl(_Type, active: true);
		mOwner.ApplyStatusEffect(this);
	}

	public override void SetFxData()
	{
		mFxInfo = new StEffectFxInfo(Settings.pInstance.GetEffectFxData(_AppliedEffect), base.transform);
	}

	public override void Remove()
	{
		mOwner.TakeCrowdControl(_Type, active: false);
		base.Remove();
	}
}
