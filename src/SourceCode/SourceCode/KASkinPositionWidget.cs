using System;
using UnityEngine;

[Serializable]
public class KASkinPositionWidget
{
	public Vector3 _Offset = Vector3.zero;

	public float _Time = 0.2f;

	public UITweener.Method _PositionEffect = UITweener.Method.EaseInOut;

	public UIWidget _Widget;
}
