using System;
using UnityEngine;

[Serializable]
public class AvAvatarCamParams
{
	public Vector3 _Polar = new Vector3(0f, 30f, -5f);

	public float _FocusHeight = 2f;

	public float _Speed = 5f;

	public const float _MinCameraDistance = 2f;

	public float _MaxCameraDistance = 15f;

	public bool _IgnoreCollision;
}
