using System;
using UnityEngine;

[Serializable]
public class PetMoodParticleData
{
	public SanctuaryPetMeterType _Type;

	public string _ParticleObject;

	public string _ParentBone;

	public Vector3 _ParticleOffset;

	public int _ThresholdPercentage;

	public AdditionalParticleData[] _AdditionalParticleData;
}
