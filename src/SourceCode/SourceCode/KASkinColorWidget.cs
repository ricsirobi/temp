using System;
using UnityEngine;

[Serializable]
public class KASkinColorWidget
{
	public Color _Color = Color.grey;

	public UITweener.Method _ColorEffect = UITweener.Method.EaseInOut;

	public UIWidget _Widget;
}
