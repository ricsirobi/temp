using System;

[Serializable]
public class SanctuaryPetMeterModifier
{
	public PetMeterMultiplierAtAge[] _MeterMultiplierAtAge;

	public float _LevelMultiplier;

	public float Modify(float currentVal, RaisedPetData inPetData)
	{
		float num = currentVal;
		int rankID = PetRankData.GetUserRank(inPetData).RankID;
		PetMeterMultiplierAtAge petMeterMultiplierAtAge = Array.Find(_MeterMultiplierAtAge, (PetMeterMultiplierAtAge x) => x._PetStage == inPetData.pStage);
		if (petMeterMultiplierAtAge != null)
		{
			num += currentVal * petMeterMultiplierAtAge._Value;
		}
		return num + _LevelMultiplier * (float)rankID * currentVal;
	}
}
