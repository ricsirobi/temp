using UnityEngine;

namespace SquadTactics;

public class Buff : Effect
{
	public Stat _AffectedStat;

	public int _BaseAmount = 5;

	public float _Multiplier;

	protected float mAmount;

	public float pAmount => mAmount;

	public override void Activate()
	{
		foreach (Effect pActiveStatusEffect in mOwner.pActiveStatusEffects)
		{
			if (!(pActiveStatusEffect is Buff) || pActiveStatusEffect is Tick)
			{
				continue;
			}
			Buff buff = (Buff)pActiveStatusEffect;
			if (_AffectedStat == buff._AffectedStat)
			{
				bool num = IsPositive() == buff.IsPositive();
				mOwner.RemoveStatusEffect(buff);
				if (!num)
				{
					Object.Destroy(base.gameObject);
					return;
				}
				break;
			}
		}
		mAmount = mOwner.pCharacterData._Stats.GetStatValue(_AffectedStat) * _Multiplier + (float)_BaseAmount;
		if (!IsPositive())
		{
			mAmount *= -1f;
		}
		mAmount = mOwner.pCharacterData._Stats.GetStat(_AffectedStat).GetClampedValue(mAmount);
		if (mOwner.pCharacterData._Stats.GetStat(_AffectedStat)._UseRoundedValues)
		{
			mAmount = Mathf.Round(mAmount);
		}
		mOwner.TakeStatChange(_AffectedStat, mAmount);
		mOwner.ApplyStatusEffect(this);
	}

	public override void SetFxData()
	{
		mFxInfo = new StEffectFxInfo(Settings.pInstance.GetStatFxData(_AffectedStat), base.transform);
	}

	public override void Remove()
	{
		mAmount *= -1f;
		mOwner.TakeStatChange(_AffectedStat, mAmount);
		base.Remove();
	}
}
