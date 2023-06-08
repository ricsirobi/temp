using UnityEngine;

public class ActivateTargetTotem : GauntletTarget
{
	public int _RotationSpeed;

	public int[] _Angles;

	public float _TotalTime;

	public float _TargetSpinTime;

	public float _TargetStopTime;

	public Transform[] _Meshes;

	private int[] mTargetAngle = new int[4];

	private bool mActive;

	private bool mSubActive;

	private float mActiveTimer;

	private float mSubActiveTimer;

	protected override void Update()
	{
		if (mRenderer != null && !mRenderer.isVisible)
		{
			return;
		}
		base.Update();
		if (!mActive)
		{
			return;
		}
		mActiveTimer -= Time.deltaTime;
		mSubActiveTimer += Time.deltaTime;
		if (mSubActive)
		{
			if (mActiveTimer > 0f)
			{
				for (int i = 0; i < _Meshes.Length; i++)
				{
					_Meshes[i].Rotate(0f, (float)mTargetAngle[i] * Time.deltaTime * (float)_RotationSpeed, 0f);
				}
			}
			else
			{
				mActive = false;
				mSubActive = false;
			}
			if (mSubActiveTimer > _TargetSpinTime)
			{
				mSubActive = false;
			}
		}
		else if (mSubActiveTimer > _TargetSpinTime + _TargetStopTime)
		{
			SetTargetData();
		}
	}

	private void SetTargetData()
	{
		mSubActiveTimer = 0f;
		mSubActive = true;
		for (int i = 0; i < _Meshes.Length; i++)
		{
			mTargetAngle[i] = _Angles[Random.Range(0, _Angles.Length)];
		}
	}

	public override void ActivateTarget(bool active)
	{
		base.ActivateTarget(active);
		if (mIsActive)
		{
			mActiveTimer = _TotalTime;
			mActive = true;
			SetTargetData();
		}
		else
		{
			mActive = false;
			mActiveTimer = 0f;
		}
	}
}
