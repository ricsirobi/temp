using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SWS;

public class BezierPathManager : PathManager
{
	public Vector3[] pathPoints = new Vector3[0];

	public List<BezierPoint> bPoints = new List<BezierPoint>();

	public bool showHandles = true;

	public bool connectHandles = true;

	public Color color3 = new Color(36f / 85f, 0.5921569f, 1f, 1f);

	public float pathDetail = 1f;

	public bool customDetail;

	public List<float> segmentDetail = new List<float>();

	private void Awake()
	{
		WaypointManager.AddPath(base.gameObject);
		if (bPoints != null && bPoints.Count != 0)
		{
			CalculatePath();
		}
	}

	public override void Create(Transform[] waypoints, bool makeChildren = false)
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
		bPoints.Clear();
		for (int j = 0; j < waypoints.Length; j++)
		{
			BezierPoint bezierPoint = new BezierPoint();
			bezierPoint.wp = waypoints[j];
			bezierPoint.cp = new Transform[2];
			bezierPoint.cp[0] = bezierPoint.wp.GetChild(0);
			bezierPoint.cp[1] = bezierPoint.wp.GetChild(1);
			bPoints.Add(bezierPoint);
		}
		CalculatePath();
	}

	private void OnDrawGizmos()
	{
		if (bPoints.Count > 0)
		{
			Vector3 position = bPoints[0].wp.position;
			Vector3 position2 = bPoints[bPoints.Count - 1].wp.position;
			Gizmos.color = color1;
			Gizmos.DrawWireCube(position, size * GetHandleSize(position) * 1.5f);
			Gizmos.DrawWireCube(position2, size * GetHandleSize(position2) * 1.5f);
			Gizmos.color = color2;
			for (int i = 1; i < bPoints.Count - 1; i++)
			{
				Gizmos.DrawWireSphere(bPoints[i].wp.position, radius * GetHandleSize(bPoints[i].wp.position));
			}
			if (drawCurved && bPoints.Count >= 2)
			{
				WaypointManager.DrawCurved(pathPoints);
			}
			else
			{
				WaypointManager.DrawStraight(pathPoints);
			}
		}
	}

	public override Vector3[] GetPathPoints(bool local = false)
	{
		if (local)
		{
			Vector3[] array = new Vector3[pathPoints.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = base.transform.InverseTransformPoint(pathPoints[i]);
			}
			return array;
		}
		return pathPoints;
	}

	public override int GetWaypointCount()
	{
		return bPoints.Count;
	}

	public override Transform GetWaypoint(int index)
	{
		return bPoints[index].wp;
	}

	public override int GetWaypointIndex(int point)
	{
		int result = -1;
		int num = 0;
		int num2 = 10;
		for (int i = 0; i < segmentDetail.Count; i++)
		{
			if (point == num)
			{
				result = i;
				break;
			}
			num = ((!customDetail) ? (num + Mathf.CeilToInt(pathDetail * (float)num2)) : (num + Mathf.CeilToInt(segmentDetail[i] * (float)num2)));
		}
		return result;
	}

	public void CalculatePath()
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < bPoints.Count - 1; i++)
		{
			BezierPoint bezierPoint = bPoints[i];
			float detail = pathDetail;
			if (customDetail)
			{
				detail = segmentDetail[i];
			}
			list.AddRange(GetPoints(bezierPoint.wp.position, bezierPoint.cp[1].position, bPoints[i + 1].cp[0].position, bPoints[i + 1].wp.position, detail));
		}
		pathPoints = list.Distinct().ToArray();
	}

	private List<Vector3> GetPoints(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float detail)
	{
		List<Vector3> list = new List<Vector3>();
		float num = detail * 10f;
		for (int i = 0; (float)i <= num; i++)
		{
			float num2 = (float)i / num;
			float num3 = 1f - num2;
			Vector3 zero = Vector3.zero;
			zero += p0 * num3 * num3 * num3;
			zero += p1 * num2 * 3f * num3 * num3;
			zero += p2 * 3f * num2 * num2 * num3;
			zero += p3 * num2 * num2 * num2;
			list.Add(zero);
		}
		return list;
	}
}
