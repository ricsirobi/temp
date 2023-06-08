using UnityEngine;

public class MagnetPowerUp : PowerUp
{
	private GameObject mAdditiveLevel;

	public override void Init(MonoBehaviour gameManager, PowerUpManager manager)
	{
		base.Init(gameManager, manager);
		if (mParticleSys != null)
		{
			mParticleSys.gameObject.transform.parent = AvAvatar.pAvatarCam.transform;
		}
	}

	public override void Activate()
	{
		base.Activate();
		if (mGameManager != null)
		{
			mAdditiveLevel = GameObject.Find("ThemeRoot");
			if (mAdditiveLevel != null)
			{
				mAdditiveLevel.BroadcastMessage("OnEnableMagnet", true, SendMessageOptions.DontRequireReceiver);
				if (mParticleSys != null)
				{
					mParticleSys.gameObject.transform.localPosition = _ParticlePos;
					mParticleSys.gameObject.transform.localEulerAngles = _Particle.transform.localEulerAngles;
					mParticleSys.gameObject.SetActive(value: true);
				}
			}
			else
			{
				UtDebug.LogError("@@@@@ Magnet Powerup : Level " + (mAdditiveLevel == null));
			}
		}
		else
		{
			UtDebug.LogError("@@@@@ Magnet Powerup : mGameManager " + (mGameManager == null));
		}
	}

	public override void DeActivate()
	{
		base.DeActivate();
		if (mAdditiveLevel != null)
		{
			mAdditiveLevel.BroadcastMessage("OnEnableMagnet", false, SendMessageOptions.DontRequireReceiver);
		}
	}
}
