using System.Collections.Generic;
using GearFactory.Utility;
using UnityEngine;

public class GFGearGenProc
{
	public Mesh mesh;

	private GFGearGen gearGen;

	public List<Vector3> vertices;

	public List<int> faces;

	public List<Vector3> normals;

	private List<Vector3> normalsA;

	private List<Vector3> normalsB;

	private List<Vector3> verticesSideA;

	private List<int> facesSideA;

	private List<Vector3> verticesSideB;

	private List<int> facesSideB;

	private List<int> centersA;

	private List<int> centersB;

	public int numberOfVertices { get; private set; }

	public int numberOfFaces { get; private set; }

	public GFGearGenProc()
	{
		vertices = new List<Vector3>();
		faces = new List<int>();
		normals = new List<Vector3>();
		normalsA = new List<Vector3>();
		normalsB = new List<Vector3>();
		verticesSideA = new List<Vector3>();
		facesSideA = new List<int>();
		verticesSideB = new List<Vector3>();
		facesSideB = new List<int>();
		centersA = new List<int>();
		centersB = new List<int>();
	}

	private Vector2[] GetUVs(Vector3[] vertices, GFGearGenTextureCoordinates textureCoordinates)
	{
		List<Vector2> list = new List<Vector2>();
		Vector3 vector = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		Vector3 vector2 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 vector3 = vertices[i];
			vector.x = Mathf.Max(vector3.x, vector.x);
			vector.y = Mathf.Max(vector3.y, vector.y);
			vector.z = Mathf.Max(vector3.z, vector.z);
			vector2.x = Mathf.Min(vector3.x, vector2.x);
			vector2.y = Mathf.Min(vector3.y, vector2.y);
			vector2.z = Mathf.Min(vector3.z, vector2.z);
		}
		vector.x += 0f - vector2.x;
		vector.y += 0f - vector2.y;
		vector.z += 0f - vector2.z;
		for (int j = 0; j < vertices.Length; j++)
		{
			Vector3 vector4 = vertices[j];
			if (gearGen.generateTextureCoordinates == GFGearGenTextureCoordinates.Plane || gearGen.generateTextureCoordinates == GFGearGenTextureCoordinates.Box)
			{
				Vector2 item;
				if (gearGen.generateTextureCoordinates == GFGearGenTextureCoordinates.Box)
				{
					if (Mathf.Abs(normals[j].AngleVectorPlane(Vector3.up)) >= 0.7679449f)
					{
						Vector2 vector5 = new Vector2(vector4.x, vector4.z);
						vector5.x += 0f - vector2.x;
						vector5.y += 0f - vector2.z;
						item = new Vector2(gearGen.uvOffset.x + vector5.x / vector.x * gearGen.uvTiling.x, gearGen.uvOffset.z + vector5.y / vector.z * gearGen.uvTiling.z);
					}
					else if (Mathf.Abs(normals[j].AngleVectorPlane(Vector3.left)) >= 0.7679449f)
					{
						Vector2 vector5 = new Vector2(vector4.z, vector4.y);
						vector5.x += 0f - vector2.z;
						vector5.y += 0f - vector2.y;
						item = new Vector2(gearGen.uvOffset.z + vector5.x / vector.z * gearGen.uvTiling.z, gearGen.uvOffset.y + vector5.y / vector.y * gearGen.uvTiling.y);
					}
					else
					{
						Vector2 vector5 = new Vector2(vector4.x, vector4.y);
						vector5.x += 0f - vector2.x;
						vector5.y += 0f - vector2.y;
						item = new Vector2(gearGen.uvOffset.x + vector5.x / vector.x * gearGen.uvTiling.x, gearGen.uvOffset.y + vector5.y / vector.y * gearGen.uvTiling.y);
					}
				}
				else
				{
					Vector2 vector5 = new Vector2(vector4.x, vector4.y);
					vector5.x += 0f - vector2.x;
					vector5.y += 0f - vector2.y;
					item = new Vector2(gearGen.uvOffset.x + vector5.x / vector.x * gearGen.uvTiling.x, gearGen.uvOffset.y + vector5.y / vector.y * gearGen.uvTiling.y);
				}
				list.Add(item);
				continue;
			}
			return null;
		}
		return list.ToArray();
	}

	private void AddVertex(ref List<Vector3> vertices, ref List<Vector3> normals, Vector3 vertex, Vector3 normal)
	{
		vertices.Add(vertex);
		normals.Add(normal);
	}

	private void CreateSide(ref List<Vector3> vertices, ref List<int> faces, ref List<Vector3> normals, ref List<int> centerIndices, bool isBack, int startFacesIndexAt)
	{
		Vector3 normal = (isBack ? Vector3.forward : Vector3.back);
		float num = ((gearGen.is3d && !isBack) ? gearGen.tipAngleOffset : 0f);
		float num2 = ((gearGen.is3d && !isBack) ? gearGen.valleyAngleOffset : 0f);
		float z = (gearGen.is3d ? ((isBack ? 1f : (-1f)) * (gearGen.thickness / 2f)) : 0f);
		Vector3 vertex = new Vector3(0f, 0f, z);
		float num3 = (float)gearGen.numberOfTeeth * 4f;
		float num4 = -360f / num3;
		float num5 = (num4 + gearGen.tipSize) / 2f;
		if (gearGen.innerRadius > 0f)
		{
			Vector3 vector = new Vector3(vertex.x, vertex.y, vertex.z);
			vector.x += gearGen.innerRadius;
			for (int i = 0; (float)i < num3; i++)
			{
				float num6 = num4 * (float)i - num5;
				if (isBack && gearGen.twistOutside)
				{
					num6 += gearGen.twistAngle;
				}
				Vector3 vertex2 = Quaternion.Euler(0f, 0f, num6) * vector;
				AddVertex(ref vertices, ref normals, vertex2, normal);
				centerIndices.Add(i + startFacesIndexAt);
			}
		}
		else
		{
			AddVertex(ref vertices, ref normals, vertex, normal);
			centerIndices.Add(startFacesIndexAt);
		}
		int num7 = centerIndices.Count + startFacesIndexAt;
		int num8 = num7;
		bool flag = true;
		int num9 = 0;
		for (int j = 0; (float)j < num3; j++)
		{
			if (num9 == 2)
			{
				flag = !flag;
				num9 = 0;
			}
			Vector3 vector2 = new Vector3(flag ? (gearGen.radius - num) : (gearGen.radius - gearGen.tipLength - num2), 0f, z);
			float num10 = num4 * (float)j - num5;
			if (flag)
			{
				if (num9 == 0)
				{
					num10 += gearGen.tipSize;
				}
				if (num9 == 1)
				{
					num10 -= gearGen.tipSize;
				}
				num10 += gearGen.skew;
			}
			else
			{
				if (num9 == 0)
				{
					num10 -= gearGen.valleySize;
				}
				if (num9 == 1)
				{
					num10 += gearGen.valleySize;
				}
				num10 -= gearGen.skew;
			}
			if (isBack)
			{
				num10 += gearGen.twistAngle;
			}
			Vector3 vertex3 = Quaternion.Euler(0f, 0f, num10) * vector2;
			Vector3 normal2 = (isBack ? Vector3.forward : Vector3.back);
			normal2.Normalize();
			AddVertex(ref vertices, ref normals, vertex3, normal2);
			if ((isBack && !gearGen.innerTeeth) || (!isBack && gearGen.innerTeeth))
			{
				faces.Add(((float)j == num3 - 1f) ? num7 : (num8 + 1));
				faces.Add(num8);
				faces.Add(centerIndices[(!(gearGen.innerRadius <= 0f) && (float)j != num3 - 1f) ? j : 0]);
				if (gearGen.innerRadius > 0f)
				{
					faces.Add(centerIndices[j]);
					faces.Add(centerIndices[((float)j != num3 - 1f) ? (j + 1) : 0]);
					faces.Add(((float)j == num3 - 1f) ? num8 : (num8 + 1));
				}
			}
			else
			{
				faces.Add(centerIndices[(!(gearGen.innerRadius <= 0f) && (float)j != num3 - 1f) ? j : 0]);
				faces.Add(num8);
				faces.Add(((float)j == num3 - 1f) ? num7 : (num8 + 1));
				if (gearGen.innerRadius > 0f)
				{
					faces.Add(((float)j == num3 - 1f) ? num8 : (num8 + 1));
					faces.Add(centerIndices[((float)j != num3 - 1f) ? (j + 1) : 0]);
					faces.Add(centerIndices[j]);
				}
			}
			num8++;
			num9++;
		}
	}

	private void CreateGlue(List<int> facesA, List<int> centerIndicesA, List<int> facesB, List<int> centerIndicesB, ref List<Vector3> vertices, ref List<int> faces, ref List<Vector3> normals)
	{
		if (facesA.Count != facesB.Count)
		{
			return;
		}
		if ((!gearGen.innerTeeth && gearGen.fillOutside) || (gearGen.innerTeeth && gearGen.fillCenter))
		{
			for (int i = 0; i < facesA.Count; i++)
			{
				bool flag = i == facesA.Count - 1;
				bool flag2 = i == 0;
				if (!centerIndicesA.Contains(facesA[i]) && !centerIndicesB.Contains(facesB[i]))
				{
					if (gearGen.innerTeeth)
					{
						vertices.Add(vertices[facesB[i]]);
						vertices.Add(vertices[facesA[i]]);
						vertices.Add(vertices[facesB[flag ? 1 : (i + 1)]]);
					}
					else
					{
						vertices.Add(vertices[facesA[i]]);
						vertices.Add(vertices[facesB[i]]);
						vertices.Add(vertices[facesA[flag ? 1 : (i + 1)]]);
					}
					faces.Add(vertices.Count - 3);
					faces.Add(vertices.Count - 2);
					faces.Add(vertices.Count - 1);
					Vector3 item = MeshUtils.Normal(vertices[vertices.Count - 3], vertices[vertices.Count - 2], vertices[vertices.Count - 1]);
					normals.Add(item);
					normals.Add(item);
					normals.Add(item);
					if (gearGen.innerTeeth)
					{
						vertices.Add(vertices[facesA[i]]);
						vertices.Add(vertices[facesA[flag2 ? (facesA.Count - 1) : (i - 1)]]);
						vertices.Add(vertices[facesB[flag ? 1 : (i + 1)]]);
					}
					else
					{
						vertices.Add(vertices[facesB[i]]);
						vertices.Add(vertices[facesB[flag2 ? (facesA.Count - 1) : (i - 1)]]);
						vertices.Add(vertices[facesA[flag ? 1 : (i + 1)]]);
					}
					faces.Add(vertices.Count - 3);
					faces.Add(vertices.Count - 2);
					faces.Add(vertices.Count - 1);
					item = MeshUtils.Normal(vertices[vertices.Count - 3], vertices[vertices.Count - 2], vertices[vertices.Count - 1]);
					normals.Add(item);
					normals.Add(item);
					normals.Add(item);
				}
			}
		}
		if ((gearGen.innerTeeth || !gearGen.fillCenter) && (!gearGen.innerTeeth || !gearGen.fillOutside))
		{
			return;
		}
		for (int j = 0; j < centerIndicesA.Count; j++)
		{
			bool flag = j == centerIndicesA.Count - 1;
			if (gearGen.innerTeeth)
			{
				vertices.Add(vertices[centerIndicesB[j]]);
				vertices.Add(vertices[centerIndicesB[(!flag) ? (j + 1) : 0]]);
				vertices.Add(vertices[centerIndicesA[j]]);
			}
			else
			{
				vertices.Add(vertices[centerIndicesA[j]]);
				vertices.Add(vertices[centerIndicesA[(!flag) ? (j + 1) : 0]]);
				vertices.Add(vertices[centerIndicesB[j]]);
			}
			faces.Add(vertices.Count - 3);
			faces.Add(vertices.Count - 2);
			faces.Add(vertices.Count - 1);
			Vector3 item2 = MeshUtils.Normal(vertices[vertices.Count - 3], vertices[vertices.Count - 2], vertices[vertices.Count - 1]);
			normals.Add(item2);
			normals.Add(item2);
			normals.Add(item2);
			if (gearGen.innerTeeth)
			{
				vertices.Add(vertices[centerIndicesA[(!flag) ? (j + 1) : 0]]);
				vertices.Add(vertices[centerIndicesA[j]]);
				vertices.Add(vertices[centerIndicesB[(!flag) ? (j + 1) : 0]]);
			}
			else
			{
				vertices.Add(vertices[centerIndicesB[(!flag) ? (j + 1) : 0]]);
				vertices.Add(vertices[centerIndicesB[j]]);
				vertices.Add(vertices[centerIndicesA[(!flag) ? (j + 1) : 0]]);
			}
			faces.Add(vertices.Count - 3);
			faces.Add(vertices.Count - 2);
			faces.Add(vertices.Count - 1);
			item2 = MeshUtils.Normal(vertices[vertices.Count - 3], vertices[vertices.Count - 2], vertices[vertices.Count - 1]);
			normals.Add(item2);
			normals.Add(item2);
			normals.Add(item2);
		}
	}

	public void GenerateGear(GFGearGen gearGen)
	{
		this.gearGen = gearGen;
		GameObject gameObject = gearGen.gameObject;
		mesh = gameObject.GetSharedMesh();
		if (mesh != null)
		{
			mesh.Clear();
			verticesSideA.Clear();
			verticesSideB.Clear();
			facesSideA.Clear();
			facesSideB.Clear();
			vertices.Clear();
			faces.Clear();
			normals.Clear();
			normalsA.Clear();
			normalsB.Clear();
			centersA.Clear();
			centersB.Clear();
			CreateSide(ref verticesSideA, ref facesSideA, ref normalsA, ref centersA, isBack: false, 0);
			if (gearGen.is3d)
			{
				CreateSide(ref verticesSideB, ref facesSideB, ref normalsB, ref centersB, isBack: true, verticesSideA.Count);
			}
			vertices.AddRange(verticesSideA);
			normals.AddRange(normalsA);
			if (gearGen.is3d)
			{
				vertices.AddRange(verticesSideB);
				normals.AddRange(normalsB);
			}
			faces.AddRange(facesSideA);
			if (gearGen.is3d)
			{
				faces.AddRange(facesSideB);
				CreateGlue(facesSideA, centersA, facesSideB, centersB, ref vertices, ref faces, ref normals);
			}
			MeshUtils.SmoothNormals(gearGen.splitVerticesAngle, vertices, normals);
			mesh.vertices = vertices.ToArray();
			mesh.uv = GetUVs(mesh.vertices, gearGen.generateTextureCoordinates);
			mesh.triangles = faces.ToArray();
			mesh.normals = normals.ToArray();
			MeshUtils.TangentSolver(mesh);
			mesh.RecalculateBounds();
			numberOfVertices = mesh.vertexCount;
			numberOfFaces = mesh.triangles.Length;
		}
	}
}
