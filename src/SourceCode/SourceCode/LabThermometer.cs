using UnityEngine;

public class LabThermometer
{
	private float mThermometerLevel;

	private LabCrucible mCrucible;

	private float mDefaultMinLevel;

	private float mDefaultMaxLevel;

	private float mMinTargetLevel;

	private float mMaxTargetLevel;

	private float mMinLevel;

	private float mMaxLevel;

	public float pMinLevel => mMinLevel;

	public float pMaxLevel => mMaxLevel;

	public float pMinTargetLevel
	{
		get
		{
			return mMinTargetLevel;
		}
		set
		{
			mMinTargetLevel = Mathf.Min(mDefaultMinLevel, value);
		}
	}

	public float pMaxTargetLevel
	{
		get
		{
			return mMaxTargetLevel;
		}
		set
		{
			mMaxTargetLevel = Mathf.Max(mDefaultMaxLevel, value);
		}
	}

	public float pThermometerLevel => mThermometerLevel;

	public void UseDefaultMinLevel()
	{
		pMinTargetLevel = mDefaultMinLevel;
	}

	public void UseDefaultMaxLevel()
	{
		pMaxTargetLevel = mDefaultMaxLevel;
	}

	public LabThermometer(LabCrucible inCrucible, float inMinDefaultLevel, float inMaxDefaultLevel)
	{
		mThermometerLevel = 0f;
		mCrucible = inCrucible;
		mDefaultMinLevel = inMinDefaultLevel;
		mDefaultMaxLevel = inMaxDefaultLevel;
		mMinTargetLevel = mDefaultMinLevel;
		mMaxTargetLevel = mDefaultMaxLevel;
		mMinLevel = mMinTargetLevel;
		mMaxLevel = mMaxTargetLevel;
	}

	public void DoUpdate()
	{
		mMinLevel = Mathf.Min(mMinLevel, Mathf.Min(mThermometerLevel, Mathf.Lerp(mMinLevel, mMinTargetLevel, Time.deltaTime * 2f)));
		mMaxLevel = Mathf.Max(mMaxLevel, Mathf.Max(mThermometerLevel, Mathf.Lerp(mMaxLevel, mMaxTargetLevel, Time.deltaTime * 2f)));
		mThermometerLevel = mCrucible.pTemperature;
	}
}
