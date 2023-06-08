namespace SquadTactics;

public class Hide : Effect
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
		mOwner.SetTransparentMaterials(setTransparent: true);
	}

	public override void Remove()
	{
		mOwner.SetTransparentMaterials(setTransparent: false);
		base.Remove();
	}

	public override void SetFxData()
	{
		mFxInfo = new StEffectFxInfo(Settings.pInstance.GetEffectFxData(_AppliedEffect), base.transform);
	}
}
