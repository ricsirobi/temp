using UnityEngine;

public class SpeedBoostPowerUp : PowerUp
{
	public float _SpeedMultiplier = 0.5f;

	private GameObject mAvatar;

	private float mOrgSpeedMultiplier = -1f;

	private AvAvatarController mController;

	public override void Activate()
	{
		base.Activate();
		mAvatar = AvAvatar.pObject;
		if (mAvatar != null)
		{
			mController = mAvatar.GetComponent<AvAvatarController>();
			if (mParticleSys != null)
			{
				mParticleSys.transform.parent = AvAvatar.pAvatarCam.transform;
				mParticleSys.transform.localPosition = _ParticlePos;
				mParticleSys.transform.localEulerAngles = _ParticleRot;
				mParticleSys.gameObject.SetActive(value: true);
				mParticleSys.Play();
			}
			if (mController != null)
			{
				mActive = true;
				mController.pFlyingBoostTimer = _Duration;
				mController.pFlyingFlapCooldownTimer = _Duration;
				mOrgSpeedMultiplier = mController._BonusMaxSpeedWithBoost;
				mController._BonusMaxSpeedWithBoost = _SpeedMultiplier;
				float pFlightSpeed = mController.pFlightSpeed * (1f + _SpeedMultiplier);
				mController.pFlightSpeed = pFlightSpeed;
			}
		}
	}

	public override void DeActivate()
	{
		mController._BonusMaxSpeedWithBoost = mOrgSpeedMultiplier;
		base.DeActivate();
	}
}
