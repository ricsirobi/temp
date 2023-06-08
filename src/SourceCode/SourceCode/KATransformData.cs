using System;
using UnityEngine;

[Serializable]
public class KATransformData
{
	public bool _Apply;

	public Vector3 _LocalPosition = Vector3.zero;

	public Vector3 _LocalRotation = Vector3.zero;

	public Vector3 _LocalScale = Vector3.one;
}
