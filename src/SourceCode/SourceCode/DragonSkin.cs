using System;
using UnityEngine;

public class DragonSkin : MonoBehaviour
{
	[Serializable]
	public class WeaponData
	{
		public string name;

		public WeaponData(string inName)
		{
			name = inName;
		}
	}

	[Serializable]
	public class ParticleData
	{
		public GameObject _ParticleObject;

		public string _BoneName;

		public Vector3 _PositionOffset = Vector3.zero;

		public Vector3 _RotationOffset = Vector3.zero;
	}

	public Material[] _Materials;

	public ParticleData[] _Particles;

	public Material[] _LODMaterials;

	public ParticleData[] _LODParticles;

	public Material[] _BabyMaterials;

	public ParticleData[] _BabyParticles;

	public Material[] _TeenMaterials;

	public ParticleData[] _TeenParticles;

	public Material[] _TeenLODMaterials;

	public ParticleData[] _TeenLODParticles;

	public Material[] _TitanMaterials;

	public ParticleData[] _TitanParticles;

	public Material[] _TitanLODMaterials;

	public ParticleData[] _TitanLODParticles;

	public Mesh _Mesh;

	public Mesh _BabyMesh;

	public Mesh _TeenMesh;

	public Mesh _TitanMesh;

	public string[] _RenderersToChange;

	public WeaponData _Weapon = new WeaponData("");

	public bool IsRendererAllowedToChange(string inName)
	{
		string[] renderersToChange = _RenderersToChange;
		foreach (string value in renderersToChange)
		{
			if (inName.Contains(value))
			{
				return true;
			}
		}
		return false;
	}
}
