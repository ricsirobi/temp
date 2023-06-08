using System.Collections.Generic;
using UnityEngine;

namespace SWS;

public class PathManager : MonoBehaviour
{
	public Transform[] waypoints = new Transform[0];

	public bool drawCurved = true;

	public bool drawDirection;

	public Color color1 = new Color(1f, 0f, 1f, 0.5f);

	public Color color2 = new Color(1f, 47f / 51f, 0.015686275f, 0.5f);

	public Vector3 size = new Vector3(0.7f, 0.7f, 0.7f);

	public float radius = 0.4f;

	public bool skipCustomNames = true;

	public GameObject replaceObject;

	private void Awake()
	{
		WaypointManager.AddPath(base.gameObject);
	}

	public void Create(Transform parent = null)
	{
		if (parent == null)
		{
			parent = base.transform;
		}
		List<Transform> list = new List<Transform>();
		foreach (Transform item in parent)
		{
			list.Add(item);
		}
		Create(list.ToArray());
	}

	public virtual void Create(Transform[] waypoints, bool makeChildren = false)
	{
		if (waypoints.Length < 2)
		{
			Debug.LogWarning("Not enough waypoints placed - minimum is 2. Cancelling.");
			return;
		}
		if (makeChildren)
		{
			for (int i = 0; i < waypoints.Length; i++)
			{
				waypoints[i].parent = base.transform;
			}
		}
		this.waypoints = waypoints;
	}

	private void OnDrawGizmos()
	{
		if (waypoints.Length != 0)
		{
			Vector3[] pathPoints = GetPathPoints();
			Vector3 vector = pathPoints[0];
			Vector3 vector2 = pathPoints[^1];
			Gizmos.color = color1;
			Gizmos.DrawWireCube(vector, size * GetHandleSize(vector) * 1.5f);
			Gizmos.DrawWireCube(vector2, size * GetHandleSize(vector2) * 1.5f);
			Gizmos.color = color2;
			for (int i = 1; i < pathPoints.Length - 1; i++)
			{
				Gizmos.DrawWireSphere(pathPoints[i], radius * GetHandleSize(pathPoints[i]));
			}
			if (drawCurved && pathPoints.Length >= 2)
			{
				WaypointManager.DrawCurved(pathPoints);
			}
			else
			{
				WaypointManager.DrawStraight(pathPoints);
			}
		}
	}

	public virtual float GetHandleSize(Vector3 pos)
	{
		return 1f;
	}

	public virtual Vector3[] GetPathPoints(bool local = false)
	{
		Vector3[] array = new Vector3[waypoints.Length];
		if (local)
		{
			for (int i = 0; i < waypoints.Length; i++)
			{
				array[i] = waypoints[i].localPosition;
			}
		}
		else
		{
			for (int j = 0; j < waypoints.Length; j++)
			{
				array[j] = waypoints[j].position;
			}
		}
		return array;
	}

	public virtual Transform GetWaypoint(int index)
	{
		return waypoints[index];
	}

	public virtual int GetWaypointIndex(int point)
	{
		return point;
	}

	public virtual int GetWaypointCount()
	{
		return waypoints.Length;
	}
}
