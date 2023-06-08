using System;
using UnityEngine;

namespace StableAbility;

[Serializable]
public class AbilityChestInfo : MysteryChestInfo
{
	[HideInInspector]
	public SpawnedStableData.SpawnData ChestSpawnData;

	[HideInInspector]
	public MysteryChest ChestReference;
}
