using System;
using UnityEngine;

namespace SquadTactics;

[Serializable]
public class HealthBarRange
{
	[Range(0f, 1f)]
	public float _Percentage;

	public Color _Color;
}
