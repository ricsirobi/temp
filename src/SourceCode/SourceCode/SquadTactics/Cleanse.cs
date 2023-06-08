using System;

namespace SquadTactics;

public class Cleanse : Effect
{
	public EffectName _AppliedEffect;

	public Stat[] _Stat;

	public CrowdControlEnum[] _CrowdControl;

	public EffectName[] _Effect;

	public override void Activate()
	{
		for (int num = mOwner.pActiveStatusEffects.Count - 1; num >= 0; num--)
		{
			if (mOwner.pActiveStatusEffects[num] is Tick)
			{
				Tick tick = (Tick)mOwner.pActiveStatusEffects[num];
				EffectName[] effect = _Effect;
				for (int i = 0; i < effect.Length; i++)
				{
					if (string.Equals(effect[i]._Name, tick._AppliedEffect._Name, StringComparison.OrdinalIgnoreCase) && ((mOwner.pCharacterData._Team == mCreator.pCharacterData._Team && !tick.IsPositive()) || (mOwner.pCharacterData._Team != mCreator.pCharacterData._Team && tick.IsPositive())))
					{
						mOwner.RemoveStatusEffect(mOwner.pActiveStatusEffects[num]);
						break;
					}
				}
			}
			else if (mOwner.pActiveStatusEffects[num] is Buff)
			{
				Buff buff = (Buff)mOwner.pActiveStatusEffects[num];
				Stat[] stat = _Stat;
				for (int i = 0; i < stat.Length; i++)
				{
					if (stat[i] == buff._AffectedStat && ((mOwner.pCharacterData._Team == mCreator.pCharacterData._Team && !buff.IsPositive()) || (mOwner.pCharacterData._Team != mCreator.pCharacterData._Team && buff.IsPositive())))
					{
						mOwner.RemoveStatusEffect(mOwner.pActiveStatusEffects[num]);
						break;
					}
				}
			}
			else if (mOwner.pActiveStatusEffects[num] is CrowdControl)
			{
				CrowdControl crowdControl = (CrowdControl)mOwner.pActiveStatusEffects[num];
				CrowdControlEnum[] crowdControl2 = _CrowdControl;
				for (int i = 0; i < crowdControl2.Length; i++)
				{
					if (crowdControl2[i] == crowdControl._Type)
					{
						mOwner.RemoveStatusEffect(mOwner.pActiveStatusEffects[num]);
						break;
					}
				}
			}
		}
		Remove();
	}

	public override void SetFxData()
	{
		mFxInfo = Settings.pInstance.GetEffectFxData(_AppliedEffect);
	}
}
