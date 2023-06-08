using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace SWS;

public class WaypointManager : MonoBehaviour
{
	public static readonly Dictionary<string, PathManager> Paths = new Dictionary<string, PathManager>();

	private void Awake()
	{
		DOTween.Init();
	}

	public static void AddPath(GameObject path)
	{
		string text = path.name;
		if (text.Contains("(Clone)"))
		{
			text = text.Replace("(Clone)", "");
		}
		PathManager componentInChildren = path.GetComponentInChildren<PathManager>();
		if (componentInChildren == null)
		{
			Debug.LogWarning("Called AddPath() but GameObject " + text + " has no PathManager attached.");
			return;
		}
		CleanUp();
		if (Paths.ContainsKey(text))
		{
			int num = 1;
			while (Paths.ContainsKey(text + "#" + num))
			{
				num++;
			}
			text = text + "#" + num;
			Debug.Log("Renamed " + path.name + " to " + text + " because a path with the same name was found.");
		}
		path.name = text;
		Paths.Add(text, componentInChildren);
	}

	public static void CleanUp()
	{
		string[] array = (from p in Paths
			where p.Value == null
			select p.Key).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Paths.Remove(array[i]);
		}
	}

	private void OnDestroy()
	{
		Paths.Clear();
	}

	public static void DrawStraight(Vector3[] waypoints)
	{
		for (int i = 0; i < waypoints.Length - 1; i++)
		{
			Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
		}
	}

	public static void DrawCurved(Vector3[] pathPoints)
	{
		pathPoints = GetCurved(pathPoints);
		Vector3 to = pathPoints[0];
		for (int i = 1; i < pathPoints.Length; i++)
		{
			Vector3 vector = pathPoints[i];
			Gizmos.DrawLine(vector, to);
			to = vector;
		}
	}

	public static Vector3[] GetCurved(Vector3[] waypoints)
	{
		Vector3[] array = new Vector3[waypoints.Length + 2];
		waypoints.CopyTo(array, 1);
		array[0] = waypoints[1];
		array[^1] = array[^2];
		int num = array.Length * 10;
		Vector3[] array2 = new Vector3[num + 1];
		for (int i = 0; i <= num; i++)
		{
			float t = (float)i / (float)num;
			Vector3 point = GetPoint(array, t);
			array2[i] = point;
		}
		return array2;
	}

	public static Vector3 GetPoint(Vector3[] gizmoPoints, float t)
	{
		int num = gizmoPoints.Length - 3;
		int num2 = (int)Mathf.Floor(t * (float)num);
		int num3 = num - 1;
		if (num3 > num2)
		{
			num3 = num2;
		}
		float num4 = t * (float)num - (float)num3;
		Vector3 vector = gizmoPoints[num3];
		Vector3 vector2 = gizmoPoints[num3 + 1];
		Vector3 vector3 = gizmoPoints[num3 + 2];
		Vector3 vector4 = gizmoPoints[num3 + 3];
		return 0.5f * ((-vector + 3f * vector2 - 3f * vector3 + vector4) * (num4 * num4 * num4) + (2f * vector - 5f * vector2 + 4f * vector3 - vector4) * (num4 * num4) + (-vector + vector3) * num4 + 2f * vector2);
	}

	public static float GetPathLength(Vector3[] waypoints)
	{
		float num = 0f;
		for (int i = 0; i < waypoints.Length - 1; i++)
		{
			num += Vector3.Distance(waypoints[i], waypoints[i + 1]);
		}
		return num;
	}

	public static List<Vector3> SmoothCurve(List<Vector3> pathToCurve, int interpolations)
	{
		int num = 0;
		int num2 = 0;
		if (interpolations < 1)
		{
			interpolations = 1;
		}
		num = pathToCurve.Count;
		num2 = num * Mathf.RoundToInt(interpolations) - 1;
		List<Vector3> list = new List<Vector3>(num2);
		float num3 = 0f;
		for (int i = 0; i < num2 + 1; i++)
		{
			num3 = Mathf.InverseLerp(0f, num2, i);
			List<Vector3> list2 = new List<Vector3>(pathToCurve);
			for (int num4 = num - 1; num4 > 0; num4--)
			{
				for (int j = 0; j < num4; j++)
				{
					list2[j] = (1f - num3) * list2[j] + num3 * list2[j + 1];
				}
			}
			list.Add(list2[0]);
		}
		return list;
	}
}
