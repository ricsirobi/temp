using System;
using UnityEngine;

[Serializable]
public class EquipObject
{
	public string _Name = "";

	public GameObject _Object;

	public string _Bone = "";

	public Vector3 _Offset = Vector3.zero;

	public Vector3 _Rotation = Vector3.zero;
}
