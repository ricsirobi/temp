public class HealthPotionPowerup : PowerUp
{
	public float _PercentUpdate = 100f;

	public override void Activate()
	{
		base.Activate();
		if (SanctuaryManager.pCurPetInstance != null)
		{
			float maxMeter = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.HEALTH, SanctuaryManager.pCurPetInstance.pData);
			SanctuaryManager.pCurPetInstance.SetMeter(SanctuaryPetMeterType.HEALTH, _PercentUpdate * 0.01f * maxMeter);
		}
		else
		{
			if (!(AvAvatar.pObject != null))
			{
				return;
			}
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component._Stats._CurrentHealth += component._Stats._MaxHealth * _PercentUpdate * 0.01f;
				if (component._Stats._CurrentHealth > component._Stats._MaxHealth)
				{
					component._Stats._CurrentHealth = component._Stats._MaxHealth;
				}
			}
		}
	}
}
