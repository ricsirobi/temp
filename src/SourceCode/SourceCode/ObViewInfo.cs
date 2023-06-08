using System;
using UnityEngine;

[Serializable]
public class ObViewInfo
{
	public string _Name = "";

	public Vector3 _Offset = Vector3.zero;

	public Vector3 _Rotation = Vector3.zero;

	public Vector3 _Scale = Vector3.one;

	public void ApplyViewInfo(Transform t)
	{
		t.localPosition = _Offset;
		t.localEulerAngles = _Rotation;
		t.localScale = _Scale;
	}
}
