using System;
using UnityEngine;

namespace SquadTactics;

[Serializable]
public class DefaultPropData
{
	public string _Prefab;

	public string _PartType;

	public string _BonePath;

	public Vector3 _Position;

	public Vector3 _Rotation;

	public Vector3 _Scale = Vector3.one;
}
