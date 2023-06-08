using UnityEngine;

public class LtStrobe : KAMonoBase
{
	public float _OnMin = 0.02f;

	public float _OnMax = 0.04f;

	public float _OffMin = 0.05f;

	public float _OffMax = 0.06f;

	public float _TransTimeMin = 0.01f;

	public float _TransTimeMax = 0.05f;

	private float mTimer;

	private bool mIsOn = true;

	private float mDir;

	private float mDuration;

	private float mMaxIntensisty = 1f;

	private void Start()
	{
		mTimer = Random.Range(_OnMin, _OnMax);
		mDir = 0f;
		mIsOn = true;
		mMaxIntensisty = base.light.intensity;
	}

	public void SetIntensity(float f)
	{
		float intensity = f * mMaxIntensisty;
		base.light.intensity = intensity;
	}

	private void Update()
	{
		mTimer -= Time.deltaTime;
		if (mTimer <= 0f)
		{
			if (mDir == 0f)
			{
				if (mIsOn)
				{
					mDir = -1f;
				}
				else
				{
					mDir = 1f;
				}
				mDuration = (mTimer = Random.Range(_TransTimeMin, _TransTimeMax));
				return;
			}
			mIsOn = !mIsOn;
			if (mIsOn)
			{
				SetIntensity(1f);
				mTimer = Random.Range(_OnMin, _OnMax);
			}
			else
			{
				SetIntensity(0f);
				mTimer = Random.Range(_OffMin, _OffMax);
			}
			mDir = 0f;
		}
		else if (mDir != 0f)
		{
			if (mIsOn)
			{
				SetIntensity(mTimer / mDuration);
			}
			else
			{
				SetIntensity(1f - mTimer / mDuration);
			}
		}
	}
}
