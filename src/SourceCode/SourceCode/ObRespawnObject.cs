using System;
using UnityEngine;

[Serializable]
public class ObRespawnObject
{
	public GameObject _Object;

	public float _RespawnTime;

	[NonSerialized]
	public float mRespawnTime;
}
