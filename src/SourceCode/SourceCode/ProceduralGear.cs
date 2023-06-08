using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralGear : MonoBehaviour
{
	[Serializable]
	public class Point
	{
		public Vector3 offset = new Vector3(1f, 1f, 0f);

		public float uvOffX;

		public float uvOffY;

		public float uvScaleX = 1f;

		public float uvScaleY = 1f;

		public bool uvMapping;

		public int mat;
	}

	[Serializable]
	public class Prefs
	{
		public float modul = 0.5f;

		public float thickness = 1f;

		public float dk;

		public float d;

		public float ramp = 0.7f;

		public float slope;

		public float coneX;

		public float coneY;

		public int teethCount = 20;

		public int teethParts = 1;

		public int bodyParts = 1;

		public bool autoUV = true;

		public bool tangens;

		public bool flanks = true;

		public bool inner;

		public bool capT = true;

		public bool capB = true;
	}

	public Point[] points;

	public Prefs prefs;

	public long ms;

	public float df;

	public int vt;

	private int pointsLenght;

	private Mesh mesh;

	private Vector3[] v;

	private bool create;

	public void UpdateGear()
	{
		if (!create)
		{
			StartCoroutine("CreateGear");
		}
	}

	private IEnumerator CreateGear()
	{
		create = true;
		Stopwatch stopWatch = new Stopwatch();
		stopWatch.Start();
		if (mesh == null)
		{
			GetComponent<MeshFilter>().mesh = (mesh = new Mesh());
			mesh.name = "Gear";
			mesh.hideFlags = HideFlags.HideAndDontSave;
		}
		while (prefs == null)
		{
			yield return 0;
		}
		if (points == null || points.Length == 0)
		{
			points = new Point[5]
			{
				new Point(),
				new Point(),
				new Point(),
				new Point(),
				new Point()
			};
			points[0].offset.x = -1f;
			points[2].offset.x = prefs.modul * 2f - 0.02f;
			points[3].offset.Set(-1f, -1f, 0f);
			points[3].uvMapping = true;
			base.transform.localScale = Vector3.one;
		}
		if (prefs.teethCount < 3)
		{
			prefs.teethCount = 3;
		}
		int bodyParts = prefs.bodyParts;
		int teethParts = prefs.teethParts;
		int teethCount = prefs.teethCount;
		int num = teethParts + bodyParts;
		int mat = points[bodyParts].mat;
		int num2 = 0;
		points[bodyParts].offset.Set(0f, prefs.thickness, 0f);
		if (prefs.thickness < 0f)
		{
			prefs.thickness = 0f;
		}
		if (prefs.modul < 0f)
		{
			prefs.modul = 0f;
		}
		prefs.d = (float)teethCount * prefs.modul * 0.5f;
		prefs.dk = ((float)teethCount + 2f) * prefs.modul * 0.5f;
		df = ((float)teethCount - 2.5f) * prefs.modul * 0.5f;
		float num3 = 360f / (float)teethCount;
		float num4 = prefs.modul * 2.25f;
		mesh.subMeshCount = GetComponent<Renderer>().sharedMaterials.Length;
		pointsLenght = points.GetLength(0);
		int num5 = teethCount * teethParts * 24 + ((mat > 0) ? (bodyParts + mat) : (bodyParts * 2)) * (teethCount + 1) * 2;
		if (prefs.capT)
		{
			num5 += teethParts * teethCount * 4;
		}
		if (prefs.capB)
		{
			num5 += bodyParts * (teethCount + 1) * 2;
		}
		if (num5 < 62000)
		{
			int[] array = new int[mesh.subMeshCount];
			int[,] array2 = new int[mesh.subMeshCount, num5 * 3];
			v = new Vector3[num5];
			Vector2[] array3 = new Vector2[num5];
			_ = new int[12];
			float num6 = 1f;
			float num7 = 0f;
			float z = 0f;
			int[] array4;
			if (prefs.inner)
			{
				num7 = prefs.modul * 2f;
				array4 = new int[12]
				{
					0, 2, 3, 0, 3, 1, 0, 3, 2, 0,
					1, 3
				};
			}
			else
			{
				array4 = new int[12]
				{
					0, 3, 2, 0, 1, 3, 0, 2, 3, 0,
					3, 1
				};
			}
			for (int i = 0; i < bodyParts; i++)
			{
				if (i == bodyParts - 1)
				{
					z = prefs.slope;
				}
				if (points[i].offset.z != 0f)
				{
					points[i].offset.z = 0f;
				}
				float num8 = df + num7;
				float num9 = prefs.thickness * num6;
				Vector3 vector = new Vector3(points[i].offset.x + num8, points[i].offset.y * num9 * num6, 0f);
				Vector3 vector2 = new Vector3(points[i + 1].offset.x + num8, points[i + 1].offset.y * num9 * num6, z);
				if (prefs.autoUV)
				{
					points[i].uvMapping = GetAutoUV(i, 1f);
				}
				for (int j = 0; j < teethCount + 1; j++)
				{
					Quaternion quaternion = Quaternion.Euler(0f, num3 * 0.5f + num3 * (float)j, 0f);
					v[num2] = quaternion * vector;
					v[num2 + 1] = quaternion * vector2;
					if (points[i].uvMapping)
					{
						array3[num2] = UV(v[num2], i, j, prefs.inner ? 5 : 4);
						array3[num2 + 1] = UV(v[num2 + 1], i, j, prefs.inner ? 4 : 5);
					}
					else
					{
						array3[num2] = UV(v[num2], i, j, 0);
						array3[num2 + 1] = UV(v[num2 + 1], i, j, 1);
					}
					if (j != 0)
					{
						for (int k = 0; k < 6; k++)
						{
							array2[points[i].mat, k + array[points[i].mat]] = array4[k] + num2 - 2;
						}
					}
					array[points[i].mat] += 6;
					num2 += 2;
				}
			}
			float num10 = 1f;
			z = 0f;
			int num11 = num2;
			int num12 = num2;
			int num13 = ((mat >= 1) ? (num + 1) : 0);
			int num14;
			if (mat > 0)
			{
				points[num13 + mat].offset.y = prefs.thickness;
				points[num13 + mat].offset.x = 0f;
				num10 = -1f;
				num14 = mat;
			}
			else
			{
				num14 = bodyParts;
			}
			for (int l = 0; l < num14; l++)
			{
				if (l == mat - 1 || points[bodyParts].mat == -1)
				{
					z = prefs.slope;
				}
				if (points[l + num13].offset.z != 0f)
				{
					points[l + num13].offset.z = 0f;
				}
				if (prefs.inner)
				{
					num7 = prefs.modul * 2f;
				}
				float num15 = df + num7;
				float num16 = prefs.thickness * num6;
				Vector3 vector3 = new Vector3(points[l + num13].offset.x + num15, (0f - points[l + num13].offset.y) * num16 * num10 * num6, 0f);
				if (num10 < 0f && l == num14 - 1)
				{
					num16 = 0f - num16;
				}
				Vector3 vector4 = new Vector3(points[l + num13 + 1].offset.x + num15, (0f - points[l + 1 + num13].offset.y) * num16 * num10 * num6, 0f - z);
				if (prefs.autoUV && mat > 0)
				{
					points[l + num13].uvMapping = GetAutoUV(l + num13, -1f);
				}
				for (int m = 0; m < teethCount + 1; m++)
				{
					Quaternion quaternion2 = Quaternion.Euler(0f, num3 * 0.5f + num3 * (float)m, 0f);
					v[num2] = quaternion2 * vector3;
					v[num2 + 1] = quaternion2 * vector4;
					if (points[l + num13].uvMapping)
					{
						array3[num2] = UV(v[num2], l + num13, m, prefs.inner ? 4 : 5);
						array3[num2 + 1] = UV(v[num2 + 1], l + num13, m, prefs.inner ? 5 : 4);
					}
					else
					{
						array3[num2] = UV(v[num2], l + num13, m, 5);
						array3[num2 + 1] = UV(v[num2 + 1], l + num13, m, 5);
					}
					if (m != 0)
					{
						for (int n = 0; n < 6; n++)
						{
							array2[points[l + num13].mat, n + array[points[l + num13].mat]] = array4[n + 6] + num2 - 2;
						}
					}
					array[points[l + num13].mat] += 6;
					num2 += 2;
				}
			}
			int num17 = num2;
			if (prefs.capB && points[0].offset.y != 0f)
			{
				array4 = (prefs.inner ? new int[6] { 0, 1, 2, 2, 1, 3 } : new int[6] { 0, 2, 1, 2, 3, 1 });
				for (int num18 = 0; num18 < teethCount + 1; num18++)
				{
					v[num2] = v[num18 * 2];
					v[num2 + 1] = v[num18 * 2 + num11];
					int num19 = pointsLenght - 2;
					if (points[num19].uvMapping)
					{
						array3[num2 + 1] = UV(v[num2 + 1], num19, num18, prefs.inner ? 5 : 4);
						array3[num2] = UV(v[num2], num19, num18, prefs.inner ? 4 : 5);
					}
					else
					{
						array3[num2 + 1] = UV(v[num2 + 1], num19, num18, 2);
						array3[num2] = UV(v[num2], num19, num18, 3);
					}
					num2 += 2;
					if (num18 != 0)
					{
						for (int num20 = 0; num20 < 6; num20++)
						{
							array2[points[num19].mat, num20 + array[points[num19].mat]] = array4[num20] + num2 - 4;
						}
						array[points[num19].mat] += 6;
					}
				}
			}
			array4 = (prefs.inner ? new int[24]
			{
				0, 2, 1, 1, 2, 3, 4, 5, 6, 5,
				7, 6, 8, 12, 10, 12, 14, 10, 11, 13,
				9, 11, 15, 13
			} : new int[24]
			{
				0, 1, 2, 1, 3, 2, 4, 6, 5, 5,
				6, 7, 8, 10, 12, 12, 10, 14, 11, 9,
				13, 11, 13, 15
			});
			int num21 = num2;
			float coneX = prefs.coneX;
			float num22 = prefs.coneY * 2f;
			float num23 = 0f;
			float num24 = 0f;
			for (int num25 = bodyParts; num25 < num; num25++)
			{
				if (num25 > bodyParts)
				{
					num23 = prefs.coneX;
					num24 = prefs.coneY * 2f;
				}
				if (points[num25].offset.z < 0f)
				{
					points[num25].offset.z = 0f;
				}
				if (points[num25 + 1].offset.z < 0f)
				{
					points[num25 + 1].offset.z = 0f;
				}
				if (points[num25].offset.y < 0f)
				{
					points[num25].offset.y = 0f;
				}
				if (points[num25 + 1].offset.y < 0f)
				{
					points[num25 + 1].offset.y = 0f;
				}
				num11 = num2;
				float num26 = points[num25].offset.x * num4 + df + num7;
				float num27 = points[num25].offset.y * prefs.thickness;
				float num28 = (0f - points[num25].offset.z) * num4;
				float num29 = points[num25 + 1].offset.x * num4 + df + num7;
				float num30 = points[num25 + 1].offset.y * prefs.thickness;
				float num31 = (0f - points[num25 + 1].offset.z) * num4;
				Vector3[] array5 = new Vector3[8]
				{
					new Vector3(num26 + num24, num27, (num28 + z) * (1f + num23)),
					new Vector3(num26 + num24, num27, (0f - num28 + z) * (1f + num23)),
					new Vector3(num26 - num24, 0f - num27, (num28 - z) * (1f - num23)),
					new Vector3(num26 - num24, 0f - num27, (0f - num28 - z) * (1f - num23)),
					new Vector3(num29 + num22, num30, (num31 + z) * (1f + coneX)),
					new Vector3(num29 + num22, num30, (0f - num31 + z) * (1f + coneX)),
					new Vector3(num29 - num22, 0f - num30, (num31 - z) * (1f - coneX)),
					new Vector3(num29 - num22, 0f - num30, (0f - num31 - z) * (1f - coneX))
				};
				if (prefs.autoUV)
				{
					points[num25].uvMapping = GetAutoUV(num25, 1f);
				}
				for (int num32 = 0; num32 < teethCount; num32++)
				{
					Quaternion quaternion3 = Quaternion.Euler(0f, num3 * (float)num32, 0f);
					if (num25 != bodyParts)
					{
						v[num2] = quaternion3 * array5[0];
						v[1 + num2] = quaternion3 * array5[1];
						v[2 + num2] = quaternion3 * array5[4];
						v[3 + num2] = quaternion3 * array5[5];
						v[4 + num2] = quaternion3 * array5[2];
						v[5 + num2] = quaternion3 * array5[3];
						v[6 + num2] = quaternion3 * array5[6];
						v[7 + num2] = quaternion3 * array5[7];
					}
					else
					{
						v[2 + num2] = quaternion3 * array5[4];
						v[3 + num2] = quaternion3 * array5[5];
						v[6 + num2] = quaternion3 * array5[6];
						v[7 + num2] = quaternion3 * array5[7];
						quaternion3 = Quaternion.Euler(0f, num3 * (float)num32 + num3 * 0.5f, 0f);
						v[num2] = quaternion3 * array5[0];
						v[4 + num2] = quaternion3 * array5[2];
						quaternion3 = Quaternion.Euler(0f, num3 * (float)num32 - num3 * 0.5f, 0f);
						v[5 + num2] = quaternion3 * array5[3];
						v[1 + num2] = quaternion3 * array5[1];
					}
					v[8 + num2] = v[num2];
					v[9 + num2] = v[1 + num2];
					v[10 + num2] = v[2 + num2];
					v[11 + num2] = v[3 + num2];
					v[12 + num2] = v[4 + num2];
					v[13 + num2] = v[5 + num2];
					v[14 + num2] = v[6 + num2];
					v[15 + num2] = v[7 + num2];
					if (points[num25].uvMapping)
					{
						array3[num2 + 2] = UV(array3[num2 + 2], num25, num32 + 1, 2);
						array3[num2 + 3] = UV(array3[num2 + 3], num25, num32, 2);
						array3[num2] = UV(array3[num2], num25, num32 + 1, 3);
						array3[num2 + 1] = UV(array3[num2 + 1], num25, num32, 3);
						array3[num2 + 4] = array3[num2 + 2];
						array3[num2 + 5] = array3[num2 + 3];
						array3[num2 + 6] = array3[num2];
						array3[num2 + 7] = array3[num2 + 1];
					}
					else
					{
						for (int num33 = 0; num33 < 4; num33++)
						{
							array3[num2 + num33] = UV(v[num2 + num33], num25, num32, 4);
							array3[num2 + num33 + 4] = UV(v[num2 + num33 + 4], num25, num32, 5);
						}
					}
					for (int num34 = 0; num34 < 4; num34++)
					{
						if (prefs.flanks)
						{
							array3[num2 + num34 + 8] = UV(v[num21 + num34 + 8], num25, 1, 6);
							array3[num2 + num34 + 12] = UV(v[num21 + num34 + 12], num25, 1, 6);
						}
						else
						{
							array3[num2 + num34 + 8] = UV(v[num2 + num34 + 8], num25, num32, 4);
							array3[num2 + num34 + 12] = UV(v[num2 + num34 + 12], num25, num32, 5);
						}
					}
					for (int num35 = 0; num35 < 24; num35++)
					{
						array2[points[num25 + 1].mat, num35 + array[points[num25 + 1].mat]] = array4[num35] + num2;
					}
					num2 += 16;
					array[points[num25 + 1].mat] += 24;
				}
			}
			if (prefs.capT && points[num].offset.y != 0f && points[num].offset.z != 0f)
			{
				array4 = (prefs.inner ? new int[6] { 2, 1, 0, 3, 1, 2 } : new int[6] { 2, 0, 1, 3, 2, 1 });
				num21 = num2;
				for (int num36 = -1; num36 < teethCount; num36++)
				{
					v[num2] = v[num11 + num36 * 16 + 3];
					v[num2 + 1] = v[num11 + num36 * 16 + 7];
					v[num2 + 2] = v[num11 + num36 * 16 + 2];
					v[num2 + 3] = v[num11 + num36 * 16 + 6];
					int num37 = pointsLenght - 1;
					if (points[num37].uvMapping)
					{
						for (int num38 = 0; num38 < 4; num38 += 2)
						{
							array3[num2 + num38] = UV(v[num2 + num38], num37, num36, prefs.inner ? 5 : 4);
							array3[num2 + 1 + num38] = UV(v[num2 + 1 + num38], num37, num36, prefs.inner ? 4 : 5);
						}
					}
					else
					{
						for (int num39 = 0; num39 < 4; num39++)
						{
							array3[num2 + num39] = UV(v[num21 + num39], num37, num36, 6);
						}
					}
					num2 += 4;
					for (int num40 = 0; num40 < 6; num40++)
					{
						array2[points[num37].mat, num40 + array[points[num37].mat]] = array4[num40] + num2;
					}
					array[points[num37].mat] += 6;
				}
			}
			if (mesh.vertices.Length != num2)
			{
				mesh.Clear();
			}
			mesh.subMeshCount = GetComponent<Renderer>().sharedMaterials.Length;
			mesh.vertices = new Vector3[v.Length];
			mesh.vertices = v;
			for (int num41 = 0; num41 < mesh.subMeshCount; num41++)
			{
				int[] array6 = new int[array[num41]];
				for (int num42 = 0; num42 < array[num41]; num42++)
				{
					array6[num42] = array2[num41, num42];
				}
				mesh.SetTriangles(array6, num41);
			}
			mesh.colors = new Color[mesh.vertices.Length];
			mesh.uv = array3;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			Vector3[] normals = mesh.normals;
			num14 = (teethCount + 1) * 2;
			num13 = teethCount * 2;
			for (int num43 = 0; num43 < ((mat < 0) ? (bodyParts * 2) : bodyParts); num43++)
			{
				normals[num13 + num14 * num43] = normals[num14 * num43];
				normals[num13 + num14 * num43 + 1] = normals[num14 * num43 + 1];
			}
			if (mat > 0)
			{
				for (int num44 = 0; num44 < mat; num44++)
				{
					normals[num12 + num13 + num14 * num44] = normals[num12 + num14 * num44];
					normals[num12 + num13 + num14 * num44 + 1] = normals[num12 + num14 * num44 + 1];
				}
			}
			normals[num17 + num13] = normals[num17];
			normals[num17 + num13 + 1] = normals[num17 + 1];
			mesh.normals = normals;
			if (prefs.tangens)
			{
				RecalculateTangens(mesh);
			}
		}
		vt = num2;
		v = null;
		create = false;
		stopWatch.Stop();
		ms = stopWatch.ElapsedMilliseconds;
	}

	private Vector2 UV(Vector3 _point, int _p, int _i, int _j)
	{
		switch (_j)
		{
		case 2:
			return new Vector2((float)_i / (float)prefs.teethCount * points[_p].uvScaleX + points[_p].uvOffX, points[_p].uvScaleX + points[_p].uvOffY + 0.5f);
		case 3:
			return new Vector2((float)_i / (float)prefs.teethCount * points[_p].uvScaleX + points[_p].uvOffX, 1f + points[_p].uvOffY * points[_p].uvScaleY);
		case 6:
			return new Vector2((0f - _point.x) * points[_p].uvScaleX + points[_p].uvOffX, _point.y * points[_p].uvScaleY + points[_p].uvOffY * points[_p].uvScaleY);
		case 7:
			return new Vector2(points[_p].uvScaleX + points[_p].uvOffX, points[_p].uvScaleY + points[_p].uvOffY * points[_p].uvScaleY);
		default:
			if (points[_p].uvMapping)
			{
				return new Vector2((float)(_i + 1) / (float)prefs.teethCount * points[_p].uvScaleX + points[_p].uvOffX, (float)_j * points[_p].uvScaleY + points[_p].uvOffY * points[_p].uvScaleY);
			}
			if (_j != 5)
			{
				return new Vector2(_point.x * points[_p].uvScaleX + points[_p].uvOffX, _point.z * points[_p].uvScaleY + points[_p].uvOffY * points[_p].uvScaleY);
			}
			return new Vector2(0f - (_point.x * points[_p].uvScaleX + points[_p].uvOffX), _point.z * points[_p].uvScaleY + points[_p].uvOffY * points[_p].uvScaleY);
		}
	}

	private bool GetAutoUV(int p, float side)
	{
		if (side * (points[p + 1].offset.y - points[p].offset.y) * Mathf.Asin(prefs.ramp) < (0f - (points[p + 1].offset.x - points[p].offset.x)) * Mathf.Acos(prefs.ramp))
		{
			return true;
		}
		return false;
	}

	private void RecalculateTangens(Mesh _mesh)
	{
		int[] triangles = _mesh.triangles;
		Vector3[] vertices = _mesh.vertices;
		Vector2[] uv = _mesh.uv;
		Vector3[] normals = _mesh.normals;
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

	public void SwitchInnerGearing()
	{
		prefs.inner = !prefs.inner;
		for (int i = 0; i < pointsLenght; i++)
		{
			points[i].offset.x = 0f - points[i].offset.x;
		}
		UpdateGear();
	}

	public void RemoveGear()
	{
		if (Application.isEditor)
		{
			GetComponent<MeshFilter>().mesh = null;
			UnityEngine.Object.DestroyImmediate(mesh, allowDestroyingAssets: true);
		}
	}

	private void OnEnable()
	{
		UpdateGear();
		if (GetComponent<Renderer>().sharedMaterial == null)
		{
			GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));
			if (GetComponent<Renderer>().sharedMaterial != null)
			{
				GetComponent<Renderer>().sharedMaterial.color = Color.grey;
			}
		}
	}

	private void OnDisable()
	{
		StopCoroutine("CreateGear");
	}

	private void OnDestroy()
	{
		StopCoroutine("CreateGear");
		RemoveGear();
	}

	private void Reset()
	{
		UpdateGear();
	}
}
