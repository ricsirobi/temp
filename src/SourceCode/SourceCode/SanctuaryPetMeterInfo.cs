using System;

[Serializable]
public class SanctuaryPetMeterInfo
{
	public SanctuaryPetMeterType _Type;

	public float _DecreaseRate = 0.01f;

	public DecreaseRateAtAge[] DecreaseRateModifierAtAge;

	public bool _DecreaseRateInPercent;

	[NonSerialized]
	public float _DecreaseRateAttributeMultiplier;

	[NonSerialized]
	public float _DecreaseRateMultiplier;

	public SanctuaryPetMeterDecreaseRate[] _RelatedMeters;

	public float _WarningVal = 0.1f;

	public string _WarningMessage = "OnHungry";

	public int _MeterIdx;

	public bool _LocalOnly;
}
