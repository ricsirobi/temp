using System;
using UnityEngine;

[Serializable]
public class FXTransform
{
	public Transform _SpecialFX;

	public string _Bone = "";

	public Vector3 _Offset = Vector3.zero;

	public Vector3 _Rotation = Vector3.zero;
}
