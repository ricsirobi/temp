using System;
using UnityEngine;

namespace SquadTactics;

[Serializable]
public class StStat
{
	public float _BaseValue = 5f;

	public float _LevelMultiplier;

	public bool _LimitMultiplier;

	private float mCurrentValue;

	public MinMax _Limits;

	public bool _UseRoundedValues;

	public float pCurrentValue
	{
		get
		{
			return mCurrentValue;
		}
		set
		{
			mCurrentValue = Mathf.Clamp(value, _Limits.Min, _Limits.Max);
			if (_UseRoundedValues)
			{
				mCurrentValue = (float)Math.Round(mCurrentValue, MidpointRounding.AwayFromZero);
			}
		}
	}

	public float GetMultipliedValue()
	{
		return pCurrentValue * 10f;
	}

	public void InitStat(int level, float itemStat)
	{
		float num = _BaseValue + (float)level * _LevelMultiplier + itemStat;
		if (_LimitMultiplier)
		{
			_Limits.Min = num * _Limits.Min;
			_Limits.Max = num * _Limits.Max;
		}
		pCurrentValue = num;
	}

	public float GetClampedValue(float value)
	{
		if (value + mCurrentValue > _Limits.Max)
		{
			value = _Limits.Max - mCurrentValue;
		}
		else if (value + mCurrentValue < _Limits.Min)
		{
			value = _Limits.Min - mCurrentValue;
		}
		return value;
	}

	public StStat()
	{
		_BaseValue = 0f;
		_Limits = new MinMax();
	}

	public StStat(StStat newStat)
	{
		_BaseValue = newStat._BaseValue;
		_UseRoundedValues = newStat._UseRoundedValues;
		_LevelMultiplier = newStat._LevelMultiplier;
		_LimitMultiplier = newStat._LimitMultiplier;
		_Limits = new MinMax(newStat._Limits.Min, newStat._Limits.Max);
		mCurrentValue = newStat.pCurrentValue;
	}
}
