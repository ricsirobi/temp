using System;
using UnityEngine;

namespace SquadTactics;

[Serializable]
public class UnitSpawn
{
	[Header("Unit Position")]
	public Vector2 _Grid;

	public Vector3 _Rotation;

	[Header("Unit Details")]
	public int _MaximumAlertTurn;

	public string _UnitName;

	public int _Level;

	[Header("Character Variant")]
	public string _Variant;

	[Header("Block Conditions (For inanimate units)")]
	public bool _BlocksMoveNodes;

	public bool _BlocksRangedAttacks;

	[Header("Spawn Conditions")]
	public int _SpawnTurnCondition = -1;

	public int _ObjectiveCondition = -1;

	public bool pIsUsed { get; set; }
}
