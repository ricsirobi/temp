using System;
using UnityEngine;

[Serializable]
public class KASkinScaleWidget
{
	public Vector3 _Scale = Vector2.one * 1.5f;

	public UITweener.Method _ScaleEffect = UITweener.Method.EaseInOut;

	public UIWidget _Widget;
}
