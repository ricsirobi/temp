using System;
using UnityEngine;

namespace SquadTactics;

public class LineSegment : IEquatable<LineSegment>
{
	public Vector3 _Start;

	public Vector3 _End;

	public LineSegment(Vector3 start, Vector3 end)
	{
		_Start = start;
		_End = end;
	}

	public Vector3 Mid()
	{
		return new Vector3((_Start.x + _End.x) / 2f, _Start.y, (_Start.z + _End.z) / 2f);
	}

	public bool Equals(LineSegment other)
	{
		if (other._Start == _Start)
		{
			return other._End == _End;
		}
		return false;
	}
}
