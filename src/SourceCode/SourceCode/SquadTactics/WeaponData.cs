using System;

namespace SquadTactics;

[Serializable]
public class WeaponData
{
	public string _Name;

	public string _WeaponType;

	public ElementType _ElementType;

	public string _PrefabName;

	public string _MeshPrefab;

	public AnimationData _AnimData;

	public Stats _Stats;

	public WeaponData(WeaponData data)
	{
		_Name = data._Name;
		_WeaponType = data._WeaponType;
		_ElementType = data._ElementType;
		_PrefabName = data._PrefabName;
		_MeshPrefab = data._MeshPrefab;
		_AnimData = data._AnimData;
		_Stats = new Stats(data._Stats);
	}
}
