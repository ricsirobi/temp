using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SWS;

[RequireComponent(typeof(LineRenderer))]
public class PathRenderer : MonoBehaviour
{
	public bool onUpdate;

	public float spacing = 0.05f;

	private PathManager path;

	private LineRenderer line;

	private Vector3[] points;

	private void Start()
	{
		line = GetComponent<LineRenderer>();
		path = GetComponent<PathManager>();
		if ((bool)path)
		{
			StartCoroutine("StartRenderer");
		}
	}

	private IEnumerator StartRenderer()
	{
		Render();
		if (onUpdate)
		{
			while (true)
			{
				yield return null;
				Render();
			}
		}
	}

	private void Render()
	{
		spacing = Mathf.Clamp01(spacing);
		if (spacing == 0f)
		{
			spacing = 0.05f;
		}
		List<Vector3> list = new List<Vector3>();
		list.AddRange(path.GetPathPoints());
		if (path.drawCurved)
		{
			list.Insert(0, list[0]);
			list.Add(list[list.Count - 1]);
			points = list.ToArray();
			DrawCurved();
		}
		else
		{
			points = list.ToArray();
			DrawLinear();
		}
	}

	private void DrawCurved()
	{
		int num = Mathf.RoundToInt(1f / spacing) + 1;
		line.positionCount = num;
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			line.SetPosition(i, WaypointManager.GetPoint(points, num2));
			num2 += spacing;
		}
	}

	private void DrawLinear()
	{
		line.positionCount = points.Length;
		float num = 0f;
		for (int i = 0; i < points.Length; i++)
		{
			line.SetPosition(i, points[i]);
			num += spacing;
		}
	}
}
