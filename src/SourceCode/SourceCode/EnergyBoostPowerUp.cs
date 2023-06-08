public class EnergyBoostPowerUp : PowerUp
{
	public float _EnergyRate = 0.005f;

	public string _EnergyMeterName = "MeterBarEnergy";

	public override void Activate()
	{
		base.Activate();
		if (mParticleSys != null && SanctuaryManager.pInstance.pPetMeter != null && !string.IsNullOrEmpty(_EnergyMeterName))
		{
			KAWidget kAWidget = SanctuaryManager.pInstance.pPetMeter.FindItem(_EnergyMeterName);
			if (kAWidget != null)
			{
				mParticleSys.transform.parent = kAWidget.transform;
				mParticleSys.transform.localPosition = _ParticlePos;
				mParticleSys.transform.localEulerAngles = _ParticleRot;
				mParticleSys.gameObject.SetActive(value: true);
			}
		}
	}
}
