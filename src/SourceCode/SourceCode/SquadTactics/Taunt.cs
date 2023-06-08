namespace SquadTactics;

public class Taunt : Effect
{
	public EffectName _AppliedEffect;

	public override void Activate()
	{
		Effect effect = mOwner.pActiveStatusEffects.Find((Effect e) => e is Hide || e is Taunt);
		if ((bool)effect)
		{
			mOwner.RemoveStatusEffect(effect);
		}
		mOwner.ApplyStatusEffect(this);
	}

	public override void SetFxData()
	{
		mFxInfo = new StEffectFxInfo(Settings.pInstance.GetEffectFxData(_AppliedEffect), base.transform);
	}
}
