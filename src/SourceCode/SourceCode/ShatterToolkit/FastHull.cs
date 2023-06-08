using System.Collections.Generic;
using UnityEngine;

namespace ShatterToolkit;

public class FastHull : IHull
{
	protected static float smallestValidLength = 0.01f;

	protected static float smallestValidRatio = 0.05f;

	protected bool isValid = true;

	protected List<Vector3> vertices;

	protected List<Vector3> normals;

	protected List<Color32> colors;

	protected List<Vector4> tangents;

	protected List<Vector2> uvs;

	protected List<int> indices;

	public bool IsEmpty
	{
		get
		{
			if (isValid && vertices.Count >= 3)
			{
				return indices.Count < 3;
			}
			return true;
		}
	}

	public FastHull(Mesh mesh)
	{
		vertices = new List<Vector3>(mesh.vertices);
		indices = new List<int>(mesh.triangles);
		if (mesh.normals.Length != 0)
		{
			normals = new List<Vector3>(mesh.normals);
		}
		if (mesh.colors32.Length != 0)
		{
			colors = new List<Color32>(mesh.colors32);
		}
		if (mesh.tangents.Length != 0)
		{
			tangents = new List<Vector4>(mesh.tangents);
		}
		if (mesh.uv.Length != 0)
		{
			uvs = new List<Vector2>(mesh.uv);
		}
	}

	public FastHull(FastHull reference)
	{
		vertices = new List<Vector3>(reference.vertices.Count);
		indices = new List<int>(reference.indices.Count);
		if (reference.normals != null)
		{
			normals = new List<Vector3>(reference.normals.Count);
		}
		if (reference.colors != null)
		{
			colors = new List<Color32>(reference.colors.Count);
		}
		if (reference.tangents != null)
		{
			tangents = new List<Vector4>(reference.tangents.Count);
		}
		if (reference.uvs != null)
		{
			uvs = new List<Vector2>(reference.uvs.Count);
		}
	}

	public Mesh GetMesh()
	{
		if (isValid)
		{
			Mesh mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.triangles = indices.ToArray();
			if (normals != null)
			{
				mesh.normals = normals.ToArray();
			}
			if (colors != null)
			{
				mesh.colors32 = colors.ToArray();
			}
			if (tangents != null)
			{
				mesh.tangents = tangents.ToArray();
			}
			if (uvs != null)
			{
				mesh.uv = uvs.ToArray();
			}
			return mesh;
		}
		return null;
	}

	public void Split(Vector3 localPointOnPlane, Vector3 localPlaneNormal, bool fillCut, UvMapper uvMapper, ColorMapper colorMapper, out IHull resultA, out IHull resultB)
	{
		if (localPlaneNormal == Vector3.zero)
		{
			localPlaneNormal = Vector3.up;
		}
		FastHull fastHull = new FastHull(this);
		FastHull fastHull2 = new FastHull(this);
		AssignVertices(fastHull, fastHull2, localPointOnPlane, localPlaneNormal, out var vertexAbovePlane, out var oldToNewVertexMap);
		AssignTriangles(fastHull, fastHull2, vertexAbovePlane, oldToNewVertexMap, localPointOnPlane, localPlaneNormal, out var cutEdges);
		if (fillCut)
		{
			if (colors != null && colorMapper == null)
			{
				Debug.LogWarning("Fill cut failed: A ColorMapper was not provided even though the mesh has a color channel");
			}
			else if ((tangents != null || uvs != null) && uvMapper == null)
			{
				Debug.LogWarning("Fill cut failed: A UvMapper was not provided even though the mesh has a tangent/uv channel");
			}
			else
			{
				FillCutEdges(fastHull, fastHull2, cutEdges, localPlaneNormal, uvMapper, colorMapper);
			}
		}
		ValidateOutput(fastHull, fastHull2, localPlaneNormal);
		resultA = fastHull;
		resultB = fastHull2;
	}

	protected void AssignVertices(FastHull a, FastHull b, Vector3 pointOnPlane, Vector3 planeNormal, out bool[] vertexAbovePlane, out int[] oldToNewVertexMap)
	{
		vertexAbovePlane = new bool[vertices.Count];
		oldToNewVertexMap = new int[vertices.Count];
		for (int i = 0; i < vertices.Count; i++)
		{
			Vector3 vector = vertices[i];
			bool flag = Vector3.Dot(vector - pointOnPlane, planeNormal) >= 0f;
			vertexAbovePlane[i] = flag;
			if (flag)
			{
				oldToNewVertexMap[i] = a.vertices.Count;
				a.vertices.Add(vector);
				if (normals != null)
				{
					a.normals.Add(normals[i]);
				}
				if (colors != null)
				{
					a.colors.Add(colors[i]);
				}
				if (tangents != null)
				{
					a.tangents.Add(tangents[i]);
				}
				if (uvs != null)
				{
					a.uvs.Add(uvs[i]);
				}
			}
			else
			{
				oldToNewVertexMap[i] = b.vertices.Count;
				b.vertices.Add(vector);
				if (normals != null)
				{
					b.normals.Add(normals[i]);
				}
				if (colors != null)
				{
					b.colors.Add(colors[i]);
				}
				if (tangents != null)
				{
					b.tangents.Add(tangents[i]);
				}
				if (uvs != null)
				{
					b.uvs.Add(uvs[i]);
				}
			}
		}
	}

	protected void AssignTriangles(FastHull a, FastHull b, bool[] vertexAbovePlane, int[] oldToNewVertexMap, Vector3 pointOnPlane, Vector3 planeNormal, out IList<Vector3> cutEdges)
	{
		cutEdges = new List<Vector3>();
		int num = indices.Count / 3;
		for (int i = 0; i < num; i++)
		{
			int num2 = indices[i * 3];
			int num3 = indices[i * 3 + 1];
			int num4 = indices[i * 3 + 2];
			bool flag = vertexAbovePlane[num2];
			bool flag2 = vertexAbovePlane[num3];
			bool flag3 = vertexAbovePlane[num4];
			if (flag && flag2 && flag3)
			{
				a.indices.Add(oldToNewVertexMap[num2]);
				a.indices.Add(oldToNewVertexMap[num3]);
				a.indices.Add(oldToNewVertexMap[num4]);
				continue;
			}
			if (!flag && !flag2 && !flag3)
			{
				b.indices.Add(oldToNewVertexMap[num2]);
				b.indices.Add(oldToNewVertexMap[num3]);
				b.indices.Add(oldToNewVertexMap[num4]);
				continue;
			}
			int num5;
			int cw;
			int ccw;
			if (flag2 == flag3 && flag != flag2)
			{
				num5 = num2;
				cw = num3;
				ccw = num4;
			}
			else if (flag3 == flag && flag2 != flag3)
			{
				num5 = num3;
				cw = num4;
				ccw = num2;
			}
			else
			{
				num5 = num4;
				cw = num2;
				ccw = num3;
			}
			Vector3 cwIntersection;
			Vector3 ccwIntersection;
			if (vertexAbovePlane[num5])
			{
				SplitTriangle(a, b, oldToNewVertexMap, pointOnPlane, planeNormal, num5, cw, ccw, out cwIntersection, out ccwIntersection);
			}
			else
			{
				SplitTriangle(b, a, oldToNewVertexMap, pointOnPlane, planeNormal, num5, cw, ccw, out ccwIntersection, out cwIntersection);
			}
			if (cwIntersection != ccwIntersection)
			{
				cutEdges.Add(cwIntersection);
				cutEdges.Add(ccwIntersection);
			}
		}
	}

	protected void SplitTriangle(FastHull topHull, FastHull bottomHull, int[] oldToNewVertexMap, Vector3 pointOnPlane, Vector3 planeNormal, int top, int cw, int ccw, out Vector3 cwIntersection, out Vector3 ccwIntersection)
	{
		Vector3 vector = vertices[top];
		Vector3 vector2 = vertices[cw];
		Vector3 vector3 = vertices[ccw];
		float num = Vector3.Dot(vector2 - vector, planeNormal);
		float num2 = Mathf.Clamp01(Vector3.Dot(pointOnPlane - vector, planeNormal) / num);
		float num3 = Vector3.Dot(vector3 - vector, planeNormal);
		float num4 = Mathf.Clamp01(Vector3.Dot(pointOnPlane - vector, planeNormal) / num3);
		Vector3 vector4 = default(Vector3);
		vector4.x = vector.x + (vector2.x - vector.x) * num2;
		vector4.y = vector.y + (vector2.y - vector.y) * num2;
		vector4.z = vector.z + (vector2.z - vector.z) * num2;
		Vector3 vector5 = default(Vector3);
		vector5.x = vector.x + (vector3.x - vector.x) * num4;
		vector5.y = vector.y + (vector3.y - vector.y) * num4;
		vector5.z = vector.z + (vector3.z - vector.z) * num4;
		int count = topHull.vertices.Count;
		topHull.vertices.Add(vector4);
		int count2 = topHull.vertices.Count;
		topHull.vertices.Add(vector5);
		topHull.indices.Add(oldToNewVertexMap[top]);
		topHull.indices.Add(count);
		topHull.indices.Add(count2);
		int count3 = bottomHull.vertices.Count;
		bottomHull.vertices.Add(vector4);
		int count4 = bottomHull.vertices.Count;
		bottomHull.vertices.Add(vector5);
		bottomHull.indices.Add(oldToNewVertexMap[cw]);
		bottomHull.indices.Add(oldToNewVertexMap[ccw]);
		bottomHull.indices.Add(count4);
		bottomHull.indices.Add(oldToNewVertexMap[cw]);
		bottomHull.indices.Add(count4);
		bottomHull.indices.Add(count3);
		if (normals != null)
		{
			Vector3 vector6 = normals[top];
			Vector3 vector7 = normals[cw];
			Vector3 vector8 = normals[ccw];
			Vector3 item = default(Vector3);
			item.x = vector6.x + (vector7.x - vector6.x) * num2;
			item.y = vector6.y + (vector7.y - vector6.y) * num2;
			item.z = vector6.z + (vector7.z - vector6.z) * num2;
			item.Normalize();
			Vector3 item2 = default(Vector3);
			item2.x = vector6.x + (vector8.x - vector6.x) * num4;
			item2.y = vector6.y + (vector8.y - vector6.y) * num4;
			item2.z = vector6.z + (vector8.z - vector6.z) * num4;
			item2.Normalize();
			topHull.normals.Add(item);
			topHull.normals.Add(item2);
			bottomHull.normals.Add(item);
			bottomHull.normals.Add(item2);
		}
		if (colors != null)
		{
			Color32 a = colors[top];
			Color32 b = colors[cw];
			Color32 b2 = colors[ccw];
			Color32 item3 = Color32.Lerp(a, b, num2);
			Color32 item4 = Color32.Lerp(a, b2, num4);
			topHull.colors.Add(item3);
			topHull.colors.Add(item4);
			bottomHull.colors.Add(item3);
			bottomHull.colors.Add(item4);
		}
		if (tangents != null)
		{
			Vector4 vector9 = tangents[top];
			Vector4 vector10 = tangents[cw];
			Vector4 vector11 = tangents[ccw];
			Vector4 item5 = default(Vector4);
			item5.x = vector9.x + (vector10.x - vector9.x) * num2;
			item5.y = vector9.y + (vector10.y - vector9.y) * num2;
			item5.z = vector9.z + (vector10.z - vector9.z) * num2;
			item5.Normalize();
			item5.w = vector10.w;
			Vector4 item6 = default(Vector4);
			item6.x = vector9.x + (vector11.x - vector9.x) * num4;
			item6.y = vector9.y + (vector11.y - vector9.y) * num4;
			item6.z = vector9.z + (vector11.z - vector9.z) * num4;
			item6.Normalize();
			item6.w = vector11.w;
			topHull.tangents.Add(item5);
			topHull.tangents.Add(item6);
			bottomHull.tangents.Add(item5);
			bottomHull.tangents.Add(item6);
		}
		if (uvs != null)
		{
			Vector2 vector12 = uvs[top];
			Vector2 vector13 = uvs[cw];
			Vector2 vector14 = uvs[ccw];
			Vector2 item7 = default(Vector2);
			item7.x = vector12.x + (vector13.x - vector12.x) * num2;
			item7.y = vector12.y + (vector13.y - vector12.y) * num2;
			Vector2 item8 = default(Vector2);
			item8.x = vector12.x + (vector14.x - vector12.x) * num4;
			item8.y = vector12.y + (vector14.y - vector12.y) * num4;
			topHull.uvs.Add(item7);
			topHull.uvs.Add(item8);
			bottomHull.uvs.Add(item7);
			bottomHull.uvs.Add(item8);
		}
		cwIntersection = vector4;
		ccwIntersection = vector5;
	}

	protected void FillCutEdges(FastHull a, FastHull b, IList<Vector3> edges, Vector3 planeNormal, UvMapper uvMapper, ColorMapper colorMapper)
	{
		int num = edges.Count / 2;
		List<Vector3> list = new List<Vector3>(num);
		List<int> list2 = new List<int>(num * 2);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			int num3 = i + 1;
			int num4 = num2;
			float num5 = (edges[i * 2 + 1] - edges[num2 * 2]).sqrMagnitude;
			for (int j = num3; j < num; j++)
			{
				float sqrMagnitude = (edges[i * 2 + 1] - edges[j * 2]).sqrMagnitude;
				if (sqrMagnitude < num5)
				{
					num4 = j;
					num5 = sqrMagnitude;
				}
			}
			if (num4 == num2 && i > num2)
			{
				int count = list.Count;
				int item = count;
				for (int k = num2; k < i; k++)
				{
					list.Add(edges[k * 2]);
					list2.Add(item++);
					list2.Add(item);
				}
				list.Add(edges[i * 2]);
				list2.Add(item);
				list2.Add(count);
				num2 = num3;
			}
			else if (num3 < num)
			{
				Vector3 value = edges[num3 * 2];
				Vector3 value2 = edges[num3 * 2 + 1];
				edges[num3 * 2] = edges[num4 * 2];
				edges[num3 * 2 + 1] = edges[num4 * 2 + 1];
				edges[num4 * 2] = value;
				edges[num4 * 2 + 1] = value2;
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		((ITriangulator)new Triangulator(list, list2, planeNormal)).Fill(out int[] _, out int[] newTriangles, out int[] _);
		int count2 = a.vertices.Count;
		int count3 = b.vertices.Count;
		a.vertices.AddRange(list);
		b.vertices.AddRange(list);
		if (normals != null)
		{
			Vector3 item2 = -planeNormal;
			for (int l = 0; l < list.Count; l++)
			{
				a.normals.Add(item2);
				b.normals.Add(planeNormal);
			}
		}
		if (colors != null)
		{
			colorMapper.Map(list, planeNormal, out var colorsA, out var colorsB);
			a.colors.AddRange(colorsA);
			b.colors.AddRange(colorsB);
		}
		if (tangents != null || uvs != null)
		{
			uvMapper.Map(list, planeNormal, out var tangentsA, out var tangentsB, out var uvsA, out var uvsB);
			if (tangents != null)
			{
				a.tangents.AddRange(tangentsA);
				b.tangents.AddRange(tangentsB);
			}
			if (uvs != null)
			{
				a.uvs.AddRange(uvsA);
				b.uvs.AddRange(uvsB);
			}
		}
		int num6 = newTriangles.Length / 3;
		for (int m = 0; m < num6; m++)
		{
			a.indices.Add(count2 + newTriangles[m * 3]);
			a.indices.Add(count2 + newTriangles[m * 3 + 2]);
			a.indices.Add(count2 + newTriangles[m * 3 + 1]);
			b.indices.Add(count3 + newTriangles[m * 3]);
			b.indices.Add(count3 + newTriangles[m * 3 + 1]);
			b.indices.Add(count3 + newTriangles[m * 3 + 2]);
		}
	}

	protected void ValidateOutput(FastHull a, FastHull b, Vector3 planeNormal)
	{
		float num = a.LengthAlongAxis(planeNormal);
		float num2 = b.LengthAlongAxis(planeNormal);
		float num3 = num + num2;
		if (num3 < smallestValidLength)
		{
			a.isValid = false;
			b.isValid = false;
		}
		else if (num / num3 < smallestValidRatio)
		{
			a.isValid = false;
		}
		else if (num2 / num3 < smallestValidRatio)
		{
			b.isValid = false;
		}
	}

	protected float LengthAlongAxis(Vector3 axis)
	{
		if (vertices.Count > 0)
		{
			float num = Vector3.Dot(vertices[0], axis);
			float num2 = num;
			foreach (Vector3 vertex in vertices)
			{
				float a = Vector3.Dot(vertex, axis);
				num = Mathf.Min(a, num);
				num2 = Mathf.Max(a, num2);
			}
			return num2 - num;
		}
		return 0f;
	}
}
