using System;
using UnityEngine;

namespace SquadTactics;

[Serializable]
public class CinematicCameraSetting
{
	public float _InitialDistance = 10f;

	public float _InitialHeight = 5f;

	public float _InitialCameraAngle = 45f;

	public float _FinalCameraAngle = -45f;

	public float _TimeToRotate = 1f;

	public Vector3 _FocusOffset;
}
