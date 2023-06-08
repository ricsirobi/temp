using System.Collections.Generic;
using UnityEngine;

namespace ShatterToolkit.Examples;

public class OutlineCreator : MonoBehaviour
{
	protected List<Vector3> points = new List<Vector3>();

	protected List<int> edges = new List<int>();

	protected List<int> triangles = new List<int>();

	protected List<int> triangleEdges = new List<int>();

	protected bool isTriangulated;

	protected bool isLoopClosed;

	protected int loopStart;

	public int LoopPointCount => points.Count - loopStart;

	public void Reset()
	{
		points.Clear();
		edges.Clear();
		triangles.Clear();
		triangleEdges.Clear();
		isTriangulated = false;
		isLoopClosed = false;
		loopStart = 0;
	}

	public void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (isTriangulated)
			{
				Reset();
			}
			Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
			Vector3 item = Camera.main.ScreenToWorldPoint(position);
			points.Add(item);
			if (LoopPointCount >= 2)
			{
				edges.Add(points.Count - 2);
				edges.Add(points.Count - 1);
			}
			isLoopClosed = false;
		}
		else if (Input.GetMouseButtonDown(1) && LoopPointCount >= 3)
		{
			edges.Add(points.Count - 1);
			edges.Add(loopStart);
			isLoopClosed = true;
			loopStart = points.Count;
		}
		if (Input.GetKeyDown(KeyCode.Space) && !isTriangulated && isLoopClosed)
		{
			((ITriangulator)new Triangulator(points, edges, Vector3.up)).Fill(out int[] newEdges, out int[] newTriangles, out int[] newTriangleEdges);
			edges.AddRange(newEdges);
			triangles.AddRange(newTriangles);
			triangleEdges.AddRange(newTriangleEdges);
			isTriangulated = true;
		}
	}

	public void OnGUI()
	{
		GUI.Box(new Rect(0f, 0f, 500f, 100f), "Please turn on Gizmos!\nCreate an outline by left-clicking in a clockwise order on the screen.\nRight-click to close a loop.\nCreate a hole by left-clicking in a counter-clockwise order inside a shape.\nBe careful not to overlap edges.\nPress SPACE to triangulate the closed loops!");
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		for (int i = 0; i < points.Count; i++)
		{
			Gizmos.DrawSphere(points[i], 0.1f);
		}
		for (int j = 0; j < edges.Count / 2; j++)
		{
			int num = j * 2;
			Gizmos.DrawLine(points[edges[num]], points[edges[num + 1]]);
		}
		for (int k = 0; k < triangles.Count / 3; k++)
		{
			int num2 = k * 3;
			Vector3 from = (points[triangles[num2]] + points[triangles[num2 + 1]] + points[triangles[num2 + 2]]) / 3f;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(from, (points[edges[triangleEdges[num2] * 2]] + points[edges[triangleEdges[num2] * 2 + 1]]) * 0.5f);
			Gizmos.color = Color.green;
			Gizmos.DrawLine(from, (points[edges[triangleEdges[num2 + 1] * 2]] + points[edges[triangleEdges[num2 + 1] * 2 + 1]]) * 0.5f);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(from, (points[edges[triangleEdges[num2 + 2] * 2]] + points[edges[triangleEdges[num2 + 2] * 2 + 1]]) * 0.5f);
		}
	}
}
