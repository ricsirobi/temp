using UnityEngine;

public class SEOhmMeter
{
	private float mTargetResistanceReading;

	private float mPrevResistanceReading;

	private float mCurrentResistanceReading;

	private float mNeedleVibratedDuration = -1f;

	private float mNeedleVibrateLamda;

	private bool mNeedleVibrateBackward = true;

	private float mSpeedModifier = 1f;

	private float mCurrentLerpTime;

	private float mTotalLerpTime = 1f;

	private UiScienceExperiment.OhmMeterProp mOhmMeterInfo;

	public float pCurrentResistanceReading => mCurrentResistanceReading;

	public SEOhmMeter(UiScienceExperiment.OhmMeterProp inOhmMeterInfo)
	{
		mOhmMeterInfo = inOhmMeterInfo;
		Reset();
	}

	public void SetResistance(float inResistance)
	{
		if (inResistance != mTargetResistanceReading)
		{
			mCurrentLerpTime = 0f;
			mPrevResistanceReading = mCurrentResistanceReading;
			mTargetResistanceReading = inResistance;
		}
	}

	public void Reset()
	{
		mCurrentResistanceReading = (mTargetResistanceReading = (mPrevResistanceReading = mOhmMeterInfo._NeedleMinReading));
	}

	public void Update(float inTime)
	{
		mCurrentLerpTime += Time.deltaTime * mSpeedModifier;
		mCurrentResistanceReading = Mathf.Lerp(mPrevResistanceReading, mTargetResistanceReading, mCurrentLerpTime / mTotalLerpTime);
	}

	public float ComputeNeedleVibration(float currentResistance, float targetResistance)
	{
		if (currentResistance / targetResistance >= 1f)
		{
			if (mNeedleVibratedDuration == -1f)
			{
				mNeedleVibratedDuration = 0f;
			}
			float minNeedleAngle = Mathf.Lerp(mOhmMeterInfo._NeedleMinAngle, mOhmMeterInfo._NeedleMaxAngle, targetResistance / mOhmMeterInfo._NeedleMaxReading) - Mathf.Abs(Random.Range(mOhmMeterInfo._NeedleVibrateAngleRange.x, mOhmMeterInfo._NeedleVibrateAngleRange.y));
			float maxNeedleAngle = Mathf.Lerp(mOhmMeterInfo._NeedleMinAngle, mOhmMeterInfo._NeedleMaxAngle, targetResistance / mOhmMeterInfo._NeedleMaxReading);
			currentResistance = VibrateNeedleOnReachingMaxPos(minNeedleAngle, maxNeedleAngle, targetResistance);
		}
		else
		{
			mNeedleVibrateLamda = 1f;
			mNeedleVibratedDuration = -1f;
			mNeedleVibrateBackward = true;
		}
		return currentResistance;
	}

	private float VibrateNeedleOnReachingMaxPos(float minNeedleAngle, float maxNeedleAngle, float targetResistance)
	{
		if (mNeedleVibratedDuration >= 0f)
		{
			if (mNeedleVibratedDuration < mOhmMeterInfo._NeedleVibrateDuration)
			{
				mNeedleVibratedDuration += Time.deltaTime;
				mNeedleVibrateLamda += (mNeedleVibrateBackward ? ((0f - Time.deltaTime) * mOhmMeterInfo._NeedleVibrateSpeed) : (Time.deltaTime * mOhmMeterInfo._NeedleVibrateSpeed));
				mNeedleVibrateLamda = Mathf.Clamp(mNeedleVibrateLamda, 0f, 1f);
				if (mNeedleVibrateLamda <= 0f)
				{
					mNeedleVibrateBackward = false;
				}
				if (mNeedleVibrateLamda >= 1f)
				{
					mNeedleVibrateBackward = true;
				}
			}
			else
			{
				mNeedleVibrateLamda = 1f;
			}
		}
		else
		{
			mNeedleVibrateLamda = 1f;
		}
		float num = Mathf.Abs(mOhmMeterInfo._NeedleMaxAngle - mOhmMeterInfo._NeedleMinAngle);
		float num2 = Mathf.Abs(Mathf.Lerp(minNeedleAngle, maxNeedleAngle, mNeedleVibrateLamda) - mOhmMeterInfo._NeedleMinAngle);
		return Mathf.Lerp(mOhmMeterInfo._NeedleMinReading, mOhmMeterInfo._NeedleMaxReading, num2 / num);
	}
}
