using System;
using System.Collections.Generic;
using UnityEngine;

namespace StableAbility;

[Serializable]
public class StableFishGroup
{
	public MinMax _FishSpawnAmount;

	public List<string> _FishAssetPaths;

	public int _Weight;

	[Range(0f, 100f)]
	public int _ChestSpawnChance;

	public List<AbilityChestInfo> _AbilityChests;
}
