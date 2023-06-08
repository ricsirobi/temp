using System;
using UnityEngine;

[Serializable]
public class LeadingParticleData
{
	public GameObject _Particle;

	public string _ParticleAssetName = "";

	public Transform _Target;

	public string _TargetName = "";

	public float _Interval = 5f;

	public float _StartDistance = 5f;
}
