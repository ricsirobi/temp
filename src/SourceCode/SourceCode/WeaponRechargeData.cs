using UnityEngine;

public class WeaponRechargeData
{
	public const int NPC_ID = -1;

	public MinMax mWeaponRechargeRange;

	public int mNumTotalShots;

	public int mShotsAvailable;

	public bool mNPC;

	public float mFireCoolDownRegenRate;

	public float mTotalShotsProgress;

	public WeaponManager.AvailableShotUpdated OnAvailableShotUpdated;

	public void Update(float dt, bool active)
	{
		if (mShotsAvailable != mNumTotalShots)
		{
			int num = mShotsAvailable;
			mShotsAvailable = Mathf.FloorToInt(mTotalShotsProgress * (float)mNumTotalShots);
			float num2 = Mathf.Lerp(mWeaponRechargeRange.Max, mWeaponRechargeRange.Min, (float)mShotsAvailable / (float)mNumTotalShots);
			mTotalShotsProgress += dt / ((float)mNumTotalShots * num2) + mFireCoolDownRegenRate * dt;
			if (active && num != mShotsAvailable && OnAvailableShotUpdated != null)
			{
				OnAvailableShotUpdated(mShotsAvailable);
			}
		}
	}

	public void Shot(int count)
	{
		mShotsAvailable += count;
		mTotalShotsProgress += (float)count / (float)mNumTotalShots;
	}

	public void UpdateShotsAvailable(int availableShots)
	{
		Shot(availableShots - mShotsAvailable);
	}
}
