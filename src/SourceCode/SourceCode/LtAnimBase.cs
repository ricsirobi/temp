using UnityEngine;

public class LtAnimBase : KAMonoBase
{
	public bool _PingPong = true;

	public float _DelayMin = 0.5f;

	public float _DelayMax = 1.5f;

	public bool _Paused;

	private float mTimer;

	private float mMaxTime;

	private float mSpeed = 1f;

	public virtual void Start()
	{
		if (!_Paused)
		{
			StartAnim();
		}
	}

	public void StartAnim()
	{
		OnEndCycle();
		mTimer = 0f;
		_Paused = false;
	}

	public void PauseAnim(bool p)
	{
		_Paused = p;
	}

	public void StopAnim()
	{
		mMaxTime = 0f;
	}

	private float FindRemainder(float val1, float val2)
	{
		int num = (int)(val1 / val2);
		return val1 - val2 * (float)num;
	}

	public virtual void UpdateValue(float f)
	{
	}

	public virtual void OnEndCycle()
	{
		mMaxTime = Random.Range(_DelayMin, _DelayMax);
	}

	private void Update()
	{
		if (!(mMaxTime > 0f) || _Paused)
		{
			return;
		}
		mTimer += mSpeed * Time.deltaTime;
		if (mSpeed > 0f)
		{
			if (mTimer > mMaxTime)
			{
				if (_PingPong)
				{
					mTimer = mMaxTime - FindRemainder(mTimer, mMaxTime);
					mSpeed = 0f - mSpeed;
				}
				else
				{
					OnEndCycle();
					mTimer = FindRemainder(mTimer, mMaxTime);
				}
			}
		}
		else if (mTimer < 0f)
		{
			if (_PingPong)
			{
				OnEndCycle();
				mTimer = FindRemainder(0f - mTimer, mMaxTime);
				mSpeed = 0f - mSpeed;
			}
			else
			{
				mTimer = mMaxTime - FindRemainder(mTimer, mMaxTime);
			}
		}
		UpdateValue(mTimer / mMaxTime);
	}
}
