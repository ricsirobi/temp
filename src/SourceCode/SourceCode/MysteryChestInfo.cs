using System;
using UnityEngine;

[Serializable]
public class MysteryChestInfo
{
	public string _AssetPath = "";

	public ChestType _ChestTypeInfo;

	public float _SpawnChance;

	public Transform[] _SpawnNodeList;

	public MysteryBoxStoreInfo[] _StoreInfoList;
}
