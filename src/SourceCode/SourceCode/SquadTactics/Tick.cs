using UnityEngine;

namespace SquadTactics;

public class Tick : Buff
{
	public Stat _InfluencingStat;

	public EffectName _AppliedEffect;

	public override void Activate()
	{
		foreach (Effect pActiveStatusEffect in mOwner.pActiveStatusEffects)
		{
			if (pActiveStatusEffect is Tick)
			{
				Tick tick = (Tick)pActiveStatusEffect;
				if (_AppliedEffect._Name == tick._AppliedEffect._Name)
				{
					mOwner.RemoveStatusEffect(tick);
					break;
				}
			}
		}
		mAmount = mCreator.pCharacterData._Stats.GetStatValue(_InfluencingStat) * _Multiplier + (float)_BaseAmount;
		if (!IsPositive())
		{
			if (GameManager.pInstance.GetElementCounterResult(mCreator.pCharacterData._WeaponData._ElementType, mOwner.pCharacterData._WeaponData._ElementType) == ElementCounterResult.POSITIVE)
			{
				mAmount *= GameManager.pInstance._PositiveCounterMultiplier;
			}
			if (mOwner.pCharacterData._Stats.GetStat(_AffectedStat)._UseRoundedValues)
			{
				mAmount = Mathf.Round(mAmount);
			}
			mAmount *= -1f;
		}
		mOwner.ApplyStatusEffect(this);
	}

	public override void SetFxData()
	{
		mFxInfo = new StEffectFxInfo(Settings.pInstance.GetEffectFxData(_AppliedEffect), base.transform);
	}

	public override void TickChange(TickPhase tickPhase)
	{
		if (tickPhase == _TickPhase)
		{
			mOwner.TakeStatChange(_AffectedStat, mAmount, _AppliedEffect);
			_Duration--;
		}
	}

	public override void Remove()
	{
		Object.Destroy(base.gameObject, mFxInfo._OutFX.pDuration);
	}
}
