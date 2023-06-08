using System;
using System.Reflection;

namespace SquadTactics;

[Serializable]
public class Stats
{
	public StStat _CriticalChance;

	public StStat _CriticalDamageMultiplier;

	public StStat _DodgeChance;

	public StStat _FirePower;

	public StStat _HealingPower;

	public StStat _Health;

	public StStat _Movement;

	public StStat _Strength;

	public Stats()
	{
		_CriticalChance = new StStat();
		_CriticalDamageMultiplier = new StStat();
		_DodgeChance = new StStat();
		_FirePower = new StStat();
		_HealingPower = new StStat();
		_Health = new StStat();
		_Movement = new StStat();
		_Strength = new StStat();
	}

	public Stats(Stats newStat)
	{
		_CriticalChance = new StStat(newStat._CriticalChance);
		_CriticalDamageMultiplier = new StStat(newStat._CriticalDamageMultiplier);
		_DodgeChance = new StStat(newStat._DodgeChance);
		_FirePower = new StStat(newStat._FirePower);
		_HealingPower = new StStat(newStat._HealingPower);
		_Health = new StStat(newStat._Health);
		_Movement = new StStat(newStat._Movement);
		_Strength = new StStat(newStat._Strength);
	}

	private Stats GetItemStats(ItemStat[] itemStats)
	{
		Stats stats = new Stats();
		if (itemStats == null)
		{
			return stats;
		}
		foreach (ItemStat itemStat in itemStats)
		{
			StStatInfo[] statInfo = Settings.pInstance._StatInfo;
			foreach (StStatInfo stStatInfo in statInfo)
			{
				if (itemStat.ItemStatID == stStatInfo._StatID && stStatInfo._FieldToModify != "")
				{
					object value = stats.GetType().GetField(stStatInfo._FieldToModify).GetValue(stats);
					FieldInfo field = value.GetType().GetField("_BaseValue");
					float num = (float)field.GetValue(value);
					field.SetValue(value, num + float.Parse(itemStat.Value) * stStatInfo._Value);
				}
			}
		}
		return stats;
	}

	public void SetInitialValues(int level, ItemStat[] itemStats = null)
	{
		SetInitialValues(level, GetItemStats(itemStats));
	}

	public void SetInitialValues(int level, Stats itemStat)
	{
		_Health.InitStat(level, itemStat.GetStatValue(Stat.HEALTH, getCurrentValue: false));
		_FirePower.InitStat(level, itemStat.GetStatValue(Stat.FIREPOWER, getCurrentValue: false));
		_HealingPower.InitStat(level, itemStat.GetStatValue(Stat.HEALINGPOWER, getCurrentValue: false));
		_Strength.InitStat(level, itemStat.GetStatValue(Stat.STRENGTH, getCurrentValue: false));
		_Movement.InitStat(level, itemStat.GetStatValue(Stat.MOVEMENT, getCurrentValue: false));
		_DodgeChance.InitStat(level, itemStat.GetStatValue(Stat.DODGE, getCurrentValue: false));
		_CriticalChance.InitStat(level, itemStat.GetStatValue(Stat.CRITICALCHANCE, getCurrentValue: false));
		_CriticalDamageMultiplier.InitStat(level, itemStat.GetStatValue(Stat.CRITICALMULTIPLIER, getCurrentValue: false));
		_CriticalChance._UseRoundedValues = Settings.pInstance.GetAttributeIsRounded(Stat.CRITICALCHANCE);
		_CriticalDamageMultiplier._UseRoundedValues = Settings.pInstance.GetAttributeIsRounded(Stat.CRITICALMULTIPLIER);
		_DodgeChance._UseRoundedValues = Settings.pInstance.GetAttributeIsRounded(Stat.DODGE);
		_FirePower._UseRoundedValues = Settings.pInstance.GetAttributeIsRounded(Stat.FIREPOWER);
		_HealingPower._UseRoundedValues = Settings.pInstance.GetAttributeIsRounded(Stat.HEALINGPOWER);
		_Health._UseRoundedValues = Settings.pInstance.GetAttributeIsRounded(Stat.HEALTH);
		_Movement._UseRoundedValues = Settings.pInstance.GetAttributeIsRounded(Stat.MOVEMENT);
		_Strength._UseRoundedValues = Settings.pInstance.GetAttributeIsRounded(Stat.STRENGTH);
	}

	public StStat GetStat(Stat stat)
	{
		return stat switch
		{
			Stat.CRITICALCHANCE => _CriticalChance, 
			Stat.CRITICALMULTIPLIER => _CriticalDamageMultiplier, 
			Stat.DODGE => _DodgeChance, 
			Stat.FIREPOWER => _FirePower, 
			Stat.HEALINGPOWER => _HealingPower, 
			Stat.HEALTH => _Health, 
			Stat.MOVEMENT => _Movement, 
			_ => _Strength, 
		};
	}

	public float GetStatValue(Stat stat, bool getCurrentValue = true)
	{
		switch (stat)
		{
		case Stat.CRITICALCHANCE:
			if (getCurrentValue)
			{
				return _CriticalChance.pCurrentValue;
			}
			return _CriticalChance._BaseValue;
		case Stat.CRITICALMULTIPLIER:
			if (getCurrentValue)
			{
				return _CriticalDamageMultiplier.pCurrentValue;
			}
			return _CriticalDamageMultiplier._BaseValue;
		case Stat.DODGE:
			if (getCurrentValue)
			{
				return _DodgeChance.pCurrentValue;
			}
			return _DodgeChance._BaseValue;
		case Stat.FIREPOWER:
			if (getCurrentValue)
			{
				return _FirePower.pCurrentValue;
			}
			return _FirePower._BaseValue;
		case Stat.HEALINGPOWER:
			if (getCurrentValue)
			{
				return _HealingPower.pCurrentValue;
			}
			return _HealingPower._BaseValue;
		case Stat.HEALTH:
			if (getCurrentValue)
			{
				return _Health.pCurrentValue;
			}
			return _Health._BaseValue;
		case Stat.MOVEMENT:
			if (getCurrentValue)
			{
				return _Movement.pCurrentValue;
			}
			return _Movement._BaseValue;
		default:
			if (getCurrentValue)
			{
				return _Strength.pCurrentValue;
			}
			return _Strength._BaseValue;
		}
	}
}
