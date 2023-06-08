using System;

[Serializable]
public class PetMeterActionData
{
	public PetActions _ID = PetActions.CHEWTOY;

	public SanctuaryPetMeterType _MeterType;

	public float _Delta;
}
