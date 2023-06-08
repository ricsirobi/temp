using System;

[Serializable]
public class SanctuaryPetTypeSettings
{
	public string _Name;

	public PetMeterActionData[] _ActionMeterData;

	public SanctuaryPetMeterInfo[] _Meters;

	public float _UpdateFrequency = 6f;

	public SanctuaryPetSpeedModifier[] _SpeedModifers;

	public SanctuaryPetMeterModifier _HealthModifier;

	public SanctuaryPetMeterModifier _EnergyModifier;

	public float _FiredUpThreshold = 0.81f;

	public float _HappyThreshold = 0.2f;

	public float _TiredThreshold = 0.1f;

	public float _MinPetMeterValue = 0.01f;
}
