using UnityEngine;

public class DoubleShotPowerUp : PowerUp
{
	public float _DoubleShotDelay = 0.5f;

	public string _WeaponName = "Double_Shot";

	private bool mPlayAnim;

	private Vector3 mPetForward;

	public override void Activate()
	{
		mPlayAnim = true;
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.ForceCooldown(_Duration);
		}
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pWeaponManager != null)
		{
			SanctuaryManager.pCurPetInstance.pWeaponManager._PowerUpWeapon = _WeaponName;
		}
		base.Activate();
		if (SanctuaryManager.pCurPetInstance != null)
		{
			mPetForward = SanctuaryManager.pCurPetInstance.transform.forward;
			FireShot();
			mPlayAnim = false;
			Invoke("FireShot", _DoubleShotDelay);
		}
	}

	public override void DeActivate()
	{
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pWeaponManager != null)
		{
			SanctuaryManager.pCurPetInstance.pWeaponManager._PowerUpWeapon = null;
		}
	}

	public void FireShot()
	{
		SanctuaryManager.pCurPetInstance.Fire(null, useDirection: true, mPetForward, ignoreCoolDown: true, mPlayAnim);
	}
}
