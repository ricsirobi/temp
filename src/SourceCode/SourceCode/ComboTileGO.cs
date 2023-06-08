using System;
using UnityEngine;

[Serializable]
public class ComboTileGO : IComparable
{
	public int _MatchCount;

	public int _SpawnCount;

	[HideInInspector]
	public ComboTileInfo[] _Tiles;

	public int CompareTo(object inObj)
	{
		return (inObj as ComboTileGO)._MatchCount.CompareTo(_MatchCount);
	}
}
