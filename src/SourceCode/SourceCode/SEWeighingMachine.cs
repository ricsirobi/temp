using UnityEngine;

public class SEWeighingMachine
{
	private float mTargetWeightPoint;

	private float mCurrentWeightPoint;

	private float mCurrentSpeed;

	private float mSpeedModifier;

	private float mBrakeSpeedModifier;

	private bool mSlowing;

	private float mCurrentSpeedModifier;

	private float mDefaultMaxWeight;

	private float mDisplacement;

	private float mMaxWeight;

	private float mTargetMaxWeight;

	public float pMaxWeight => mMaxWeight;

	public float pTargetMaxWeight
	{
		get
		{
			return mTargetMaxWeight;
		}
		set
		{
			mTargetMaxWeight = Mathf.Max(mDefaultMaxWeight, value);
		}
	}

	public float pCurrentWeightPoint => mCurrentWeightPoint;

	public SEWeighingMachine(float inDefaultMaxWeight, float inSpeedModifier, float inBrakeSpeedModifier)
	{
		mDefaultMaxWeight = inDefaultMaxWeight;
		mTargetMaxWeight = mDefaultMaxWeight;
		mMaxWeight = mTargetMaxWeight;
		mSpeedModifier = inSpeedModifier;
		mBrakeSpeedModifier = inBrakeSpeedModifier;
		Reset();
	}

	public void SetWeight(float inWeight)
	{
		mTargetWeightPoint = inWeight;
	}

	public void Reset()
	{
		mTargetWeightPoint = 0f;
		mCurrentSpeedModifier = mSpeedModifier;
		mCurrentWeightPoint = 0f;
		mSlowing = false;
		mCurrentSpeed = 0f;
		mDisplacement = 0f;
	}

	public void Update(float inTime)
	{
		mMaxWeight = Mathf.Lerp(mMaxWeight, mTargetMaxWeight, Time.deltaTime * 2f);
		float num = mTargetWeightPoint - mCurrentWeightPoint;
		if (mSlowing)
		{
			if (num > 0f)
			{
				mSlowing = false;
				mCurrentSpeedModifier = mBrakeSpeedModifier;
				if (Mathf.Abs(num) < 0.01f)
				{
					return;
				}
			}
		}
		else if (num < 0f)
		{
			mSlowing = true;
			mCurrentSpeedModifier = mBrakeSpeedModifier;
			if (Mathf.Abs(num) < 0.01f)
			{
				return;
			}
		}
		if (mSlowing)
		{
			mCurrentSpeed -= mCurrentSpeedModifier * inTime;
		}
		else
		{
			mCurrentSpeed += mCurrentSpeedModifier * inTime;
		}
		float num2 = mCurrentWeightPoint;
		mCurrentWeightPoint += mCurrentSpeed * Time.deltaTime;
		if (mCurrentWeightPoint >= mMaxWeight)
		{
			mCurrentSpeed = 0f;
			mCurrentWeightPoint = mMaxWeight;
		}
		else if (mCurrentWeightPoint <= 0f)
		{
			mCurrentSpeed = 0f;
			mCurrentWeightPoint = 0f;
		}
		if (num2 - mCurrentWeightPoint < 0f && mDisplacement > 0f)
		{
			mCurrentSpeedModifier = mSpeedModifier;
		}
		else if (num2 - mCurrentWeightPoint > 0f && mDisplacement < 0f)
		{
			mCurrentSpeedModifier = mSpeedModifier;
		}
		mDisplacement = num2 - mCurrentWeightPoint;
	}
}
