using System;
using UnityEngine;

[Serializable]
public class ScaleConfig
{
	public bool _ScaleUI;

	public float _ScaleFactor = 1f;

	public Vector3 _Offset = Vector3.zero;

	public float _DiagonalInches = 5f;
}
