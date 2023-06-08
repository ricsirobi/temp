using System.Collections.Generic;
using GearFactory.Utility;
using UnityEngine;

public static class MeshUtils
{
	public enum PlaneSide
	{
		Back,
		Front
	}

	public enum IntersectType
	{
		Outside,
		Inside,
		Intersect
	}

	public static PlaneSide TestPoint(Plane p, Vector3 point)
	{
		if (!p.GetSide(point))
		{
			return PlaneSide.Back;
		}
		return PlaneSide.Front;
	}

	public static IntersectType Intersect(Vector3[] obb1, Vector3[] obb2)
	{
		if (obb1 != null && obb2 != null)
		{
			int num = obb2.Length;
			int num2 = 0;
			int num3 = 0;
			Plane[] array = new Plane[6]
			{
				new Plane(obb1[1], obb1[0], obb1[2]),
				new Plane(obb1[2], obb1[3], obb1[6]),
				new Plane(obb1[6], obb1[7], obb1[5]),
				new Plane(obb1[5], obb1[4], obb1[0]),
				new Plane(obb1[0], obb1[7], obb1[3]),
				new Plane(obb1[1], obb1[2], obb1[5])
			};
			for (int i = 0; i < array.Length; i++)
			{
				num3 = 0;
				for (int j = 0; j < num; j++)
				{
					if (TestPoint(array[i], obb2[j]) == PlaneSide.Back)
					{
						num3++;
					}
					else
					{
						num2++;
					}
				}
				if (num3 == num)
				{
					return IntersectType.Outside;
				}
			}
			if (num2 == 8 * array.Length)
			{
				return IntersectType.Inside;
			}
			return IntersectType.Intersect;
		}
		return IntersectType.Outside;
	}

	public static Vector3[] CalcOrientedBoundingBox(this GameObject g1)
	{
		Mesh sharedMesh = g1.GetSharedMesh();
		if (sharedMesh != null)
		{
			Vector3 min = sharedMesh.bounds.min;
			Vector3 max = sharedMesh.bounds.max;
			if (sharedMesh.bounds.extents.z == 0f)
			{
				max.z = min.z + 1f;
			}
			Vector3 position = new Vector3(min.x, min.y, max.z);
			Vector3 position2 = new Vector3(min.x, min.y, min.z);
			Vector3 position3 = new Vector3(max.x, min.y, min.z);
			Vector3 position4 = new Vector3(max.x, min.y, max.z);
			Vector3 position5 = new Vector3(min.x, max.y, max.z);
			Vector3 position6 = new Vector3(min.x, max.y, min.z);
			Vector3 position7 = new Vector3(max.x, max.y, min.z);
			Vector3 position8 = new Vector3(max.x, max.y, max.z);
			position = g1.transform.TransformPoint(position);
			position2 = g1.transform.TransformPoint(position2);
			position3 = g1.transform.TransformPoint(position3);
			position4 = g1.transform.TransformPoint(position4);
			position5 = g1.transform.TransformPoint(position5);
			position6 = g1.transform.TransformPoint(position6);
			position7 = g1.transform.TransformPoint(position7);
			position8 = g1.transform.TransformPoint(position8);
			return new Vector3[8] { position, position2, position3, position4, position5, position6, position7, position8 };
		}
		return null;
	}

	public static Plane CalcXAlignedCenterPlane(this GameObject g1, Space space)
	{
		return new Plane((space == Space.World) ? g1.transform.TransformDirection(Vector3.up) : Vector3.up, (space == Space.World) ? g1.transform.TransformPoint(Vector3.zero) : Vector3.zero);
	}

	public static Plane CalcXAlignedCenterPlane(this GameObject g1)
	{
		return g1.CalcXAlignedCenterPlane(Space.World);
	}

	public static Plane CalcYAlignedCenterPlane(this GameObject g1, Space space)
	{
		return new Plane((space == Space.World) ? g1.transform.TransformDirection(Vector3.forward) : Vector3.forward, (space == Space.World) ? g1.transform.TransformPoint(Vector3.zero) : Vector3.zero);
	}

	public static Plane CalcYAlignedCenterPlane(this GameObject g1)
	{
		return g1.CalcYAlignedCenterPlane(Space.World);
	}

	public static Plane CalcZAlignedCenterPlane(this GameObject g1, Space space)
	{
		return new Plane((space == Space.World) ? g1.transform.TransformDirection(Vector3.right) : Vector3.right, (space == Space.World) ? g1.transform.TransformPoint(Vector3.zero) : Vector3.zero);
	}

	public static Plane CalcZAlignedCenterPlane(this GameObject g1)
	{
		return g1.CalcZAlignedCenterPlane(Space.World);
	}

	public static Vector3 GetCenter(this Plane p1)
	{
		return p1.normal.SetVectorLength(p1.distance);
	}

	public static Mesh GetMesh(this GameObject g1)
	{
		return g1.GetComponent<MeshFilter>().GetMesh();
	}

	public static Mesh GetSharedMesh(this GameObject g1)
	{
		return g1.GetComponent<MeshFilter>().GetSharedMesh();
	}

	public static Mesh GetMesh(this MeshFilter meshFilter)
	{
		if (meshFilter != null)
		{
			return meshFilter.mesh;
		}
		return null;
	}

	public static Mesh GetSharedMesh(this MeshFilter meshFilter)
	{
		if (meshFilter != null)
		{
			return meshFilter.sharedMesh;
		}
		return null;
	}

	public static void SetSharedMesh(this MeshFilter meshFilter, Mesh mesh)
	{
		Mesh sharedMesh = meshFilter.sharedMesh;
		meshFilter.sharedMesh = mesh;
		if (sharedMesh != null)
		{
			if (Application.isEditor)
			{
				Object.DestroyImmediate(sharedMesh);
			}
			else
			{
				Object.Destroy(sharedMesh);
			}
		}
	}

	public static void SetSharedMesh(this GameObject g1, Mesh mesh)
	{
		MeshFilter meshFilter = g1.GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = (MeshFilter)g1.AddComponent(typeof(MeshFilter));
		}
		if (meshFilter != null)
		{
			meshFilter.SetSharedMesh(mesh);
		}
	}

	public static void SetMesh(this MeshFilter meshFilter, Mesh mesh)
	{
		Mesh mesh2 = meshFilter.mesh;
		meshFilter.mesh = mesh;
		if (mesh2 != null)
		{
			if (Application.isEditor)
			{
				Object.DestroyImmediate(mesh2);
			}
			else
			{
				Object.Destroy(mesh2);
			}
		}
	}

	public static void SetMesh(this GameObject g1, Mesh mesh)
	{
		MeshFilter meshFilter = g1.GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = (MeshFilter)g1.AddComponent(typeof(MeshFilter));
		}
		if (meshFilter != null)
		{
			meshFilter.SetMesh(mesh);
		}
	}

	public static void TangentSolver(Mesh mesh)
	{
		if (mesh.uv.Length != mesh.vertexCount)
		{
			return;
		}
		int[] triangles = mesh.triangles;
		Vector3[] vertices = mesh.vertices;
		Vector2[] uv = mesh.uv;
		Vector3[] normals = mesh.normals;
		int num = triangles.Length;
		int num2 = vertices.Length;
		Vector3[] array = new Vector3[num2];
		Vector3[] array2 = new Vector3[num2];
		Vector4[] array3 = new Vector4[num2];
		for (long num3 = 0L; num3 < num; num3 += 3)
		{
			long num4 = triangles[num3];
			long num5 = triangles[num3 + 1];
			long num6 = triangles[num3 + 2];
			Vector3 vector = vertices[num4];
			Vector3 vector2 = vertices[num5];
			Vector3 vector3 = vertices[num6];
			Vector2 vector4 = uv[num4];
			Vector2 vector5 = uv[num5];
			Vector2 vector6 = uv[num6];
			float num7 = vector2.x - vector.x;
			float num8 = vector3.x - vector.x;
			float num9 = vector2.y - vector.y;
			float num10 = vector3.y - vector.y;
			float num11 = vector2.z - vector.z;
			float num12 = vector3.z - vector.z;
			float num13 = vector5.x - vector4.x;
			float num14 = vector6.x - vector4.x;
			float num15 = vector5.y - vector4.y;
			float num16 = vector6.y - vector4.y;
			float num17 = num13 * num16 - num14 * num15;
			float num18 = ((num17 == 0f) ? 0f : (1f / num17));
			Vector3 vector7 = new Vector3((num16 * num7 - num15 * num8) * num18, (num16 * num9 - num15 * num10) * num18, (num16 * num11 - num15 * num12) * num18);
			Vector3 vector8 = new Vector3((num13 * num8 - num14 * num7) * num18, (num13 * num10 - num14 * num9) * num18, (num13 * num12 - num14 * num11) * num18);
			array[num4] += vector7;
			array[num5] += vector7;
			array[num6] += vector7;
			array2[num4] += vector8;
			array2[num5] += vector8;
			array2[num6] += vector8;
			if (float.IsNaN(array[1].x))
			{
				Debug.Log("IsNaN");
			}
		}
		for (long num19 = 0L; num19 < num2; num19++)
		{
			Vector3 normal = normals[num19];
			Vector3 tangent = array[num19];
			Vector3.OrthoNormalize(ref normal, ref tangent);
			array3[num19].x = tangent.x;
			array3[num19].y = tangent.y;
			array3[num19].z = tangent.z;
			array3[num19].w = ((Vector3.Dot(Vector3.Cross(normal, tangent), array2[num19]) < 0f) ? (-1f) : 1f);
		}
		mesh.tangents = array3;
	}

	public static Vector3 Normal(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		return Vector3.Cross(v2 - v1, v3 - v1).normalized;
	}

	public static List<int[]> SmoothNormals(float sharpEdgeAngleThreshold, List<Vector3> vertices, List<Vector3> normals)
	{
		Vector3[] array = vertices.ToArray();
		List<int> list = new List<int>();
		List<int[]> list2 = new List<int[]>();
		for (int i = 0; i < array.Length - 1; i++)
		{
			list.Clear();
			for (int j = i + 1; j < array.Length; j++)
			{
				if (array[j] == array[i] && Mathf.Abs(Vector3.Angle(normals[i], normals[j])) < sharpEdgeAngleThreshold)
				{
					list.Add(j);
				}
			}
			if (list.Count > 0)
			{
				int[] array2 = list.ToArray();
				for (int k = 0; k < array2.Length; k++)
				{
					normals[i] += normals[array2[k]];
				}
				normals[i] /= (float)array2.Length + 1f;
				for (int l = 0; l < array2.Length; l++)
				{
					normals[array2[l]] = normals[i];
				}
				list.Insert(0, i);
				array2 = list.ToArray();
				list2.Add(array2);
			}
		}
		return list2;
	}
}
