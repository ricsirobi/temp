using System.Collections.Generic;
using UnityEngine;

namespace ShatterToolkit;

public class SolidColorMapper : ColorMapper
{
	public Color32 fillColor = Color.cyan;

	public override void Map(IList<Vector3> points, Vector3 planeNormal, out Color32[] colorsA, out Color32[] colorsB)
	{
		Color32[] array = new Color32[points.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = fillColor;
		}
		colorsA = array;
		colorsB = array;
	}
}
