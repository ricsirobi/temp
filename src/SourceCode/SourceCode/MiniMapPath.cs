using System;
using System.Collections.Generic;
using SWS;
using UnityEngine;

[Serializable]
public class MiniMapPath
{
	public LineRenderer _LineRenderer;

	public SWS.PathManager _RacingPath;

	public int _PathSubdivisions = 10;

	private List<Vector3> mRacingSplinePositions = new List<Vector3>();

	public void InitializePath()
	{
		for (int i = 0; i < _RacingPath.waypoints.Length; i++)
		{
			mRacingSplinePositions.Add(_RacingPath.waypoints[i].position);
		}
	}

	public void DrawMiniMapPath()
	{
		Vector3[] array = new Vector3[mRacingSplinePositions.Count + 2];
		mRacingSplinePositions.CopyTo(array, 1);
		array[0] = mRacingSplinePositions[1];
		array[^1] = mRacingSplinePositions[mRacingSplinePositions.Count - 1];
		int num = array.Length * _PathSubdivisions;
		Vector3[] array2 = new Vector3[num + 1];
		for (int i = 0; i <= num; i++)
		{
			float t = (float)i / (float)num;
			Vector3 point = GetPoint(array, t);
			array2[i] = point;
			array2[i].y = 0f;
		}
		_LineRenderer.positionCount = array2.Length;
		_LineRenderer.SetPositions(array2);
	}

	private Vector3 GetPoint(Vector3[] gizmoPoints, float t)
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
}
