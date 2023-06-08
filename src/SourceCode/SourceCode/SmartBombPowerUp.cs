using System.Collections;
using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class SmartBombPowerUp : PowerUp
{
	public float _ExplosionRadius = 15f;

	public override void Init(MonoBehaviour gameManager, PowerUpManager powerUpManager, MMOMessageReceivedEventArgs args)
	{
		base.Init(gameManager, powerUpManager, args);
		if (args != null && !(args.MMOMessage.Sender.Username == ProductConfig.pToken) && !(args.MMOMessage.Sender.Username == WsWebService.pUserToken))
		{
			string[] array = args.MMOMessage.MessageText.Split(':');
			if (array.Length > 2)
			{
				Vector3 inDefaultPos = Vector3.up * 2000f;
				UtUtilities.Vector3FromString(array[2], ref inDefaultPos);
				mParticleSys.transform.position = inDefaultPos;
				mParticleSys.gameObject.SetActive(value: true);
				mActive = true;
			}
		}
	}

	public override void Activate()
	{
		base.Activate();
		StartCoroutine(WaitForFire());
		if (mPowerUpManager.pPowerUpHelper._FireProjectile != null)
		{
			mPowerUpManager.pPowerUpHelper._FireProjectile();
		}
	}

	private IEnumerator WaitForFire()
	{
		if (mParticleSys != null && mPowerUpManager.pPowerUpHelper._GetParentObject != null)
		{
			GameObject gameObject = mPowerUpManager.pPowerUpHelper._GetParentObject(null);
			mParticleSys.transform.parent = gameObject.transform;
			mParticleSys.transform.localPosition = _ParticlePos;
			Vector3 bombPos = mParticleSys.transform.position;
			if (mPowerUpManager.pPowerUpHelper != null && mPowerUpManager.pPowerUpHelper._IsMMO != null && mPowerUpManager.pPowerUpHelper._IsMMO())
			{
				SendMMOMessage("POWERUP:" + _Type + ":" + bombPos.ToString());
			}
			yield return new WaitForSeconds(0.5f);
			mParticleSys.gameObject.SetActive(value: true);
			if (mPowerUpManager.pPowerUpHelper._OnBombExplode != null)
			{
				mPowerUpManager.pPowerUpHelper._OnBombExplode(bombPos, _ExplosionRadius);
			}
		}
	}

	public override void Update()
	{
		if (mParticleSys != null && mParticleSys.isStopped && mActive)
		{
			DeActivate();
		}
	}
}
