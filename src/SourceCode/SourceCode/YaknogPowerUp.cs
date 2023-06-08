using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class YaknogPowerUp : PowerUp
{
	public GameObject _LiquidEffect;

	private GameObject mLiquidEffect;

	public override void Init(MonoBehaviour gameManager, PowerUpManager manager, MMOMessageReceivedEventArgs args)
	{
		base.Init(gameManager, manager, args);
		if (!(args.MMOMessage.Sender.Username == ProductConfig.pToken) && !(args.MMOMessage.Sender.Username == WsWebService.pUserToken))
		{
			if (PowerUp.pImmune)
			{
				PowerUp.pHitCount++;
			}
			else if (mPowerUpManager.pPowerUpHelper._CanApplyEffect != null && mPowerUpManager.pPowerUpHelper._CanApplyEffect(args, _Type))
			{
				CreateLiquidEffect();
			}
		}
	}

	public override void SendMMOMessage(string message)
	{
		if (GauntletMMOClient.pInstance != null && UtPlatform.IsiOS())
		{
			message = message + ":" + (MMOTimeManager.pInstance.GetServerTime() + 0.0);
			MainStreetMMOClient.pInstance.SendPublicExtensionMessage(message);
		}
		else
		{
			base.SendMMOMessage(message);
		}
	}

	private void CreateLiquidEffect()
	{
		mLiquidEffect = Object.Instantiate(_LiquidEffect, Vector3.zero, Quaternion.identity);
		base.Activate();
	}

	public override void Activate()
	{
		if (mPowerUpManager.pPowerUpHelper._CanActivatePowerUp != null && mPowerUpManager.pPowerUpHelper._CanActivatePowerUp(_Type))
		{
			SendMMOMessage("POWERUP:" + _Type);
		}
	}

	public override void DeActivate()
	{
		base.DeActivate();
		if (mLiquidEffect != null)
		{
			Object.Destroy(mLiquidEffect);
		}
	}
}
