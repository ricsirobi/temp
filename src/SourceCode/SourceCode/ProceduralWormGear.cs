using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralWormGear : MonoBehaviour
{
	[Serializable]
	public class Point
	{
		public Vector3 offset = new Vector3(1f, 0f, 0f);

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

		public float toothWidth = 0.88f;

		public float dk;

		public float d;

		public float ramp = 0.72f;

		public float radius = 5f;

		public float lenght = 10.5f;

		public int divisions = 20;

		public int teethParts = 1;

		public int bodyParts = 1;

		public bool autoUV = true;

		public bool tangens;

		public bool flanks = true;

		public bool capT = true;

		public bool capB = true;

		public bool lr;
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

	private int[] tp = new int[0];

	private int[,] tr = new int[0, 0];

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
		if (mesh == null || GetComponent<MeshFilter>().sharedMesh == null)
		{
			GetComponent<MeshFilter>().mesh = (mesh = new Mesh());
			mesh.name = "WormGear";
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
			points[3].offset.Set(0f, 0f - prefs.toothWidth, 0f);
			points[3].uvMapping = true;
			base.transform.localScale = Vector3.one;
		}
		points[prefs.bodyParts - 1].offset.y = prefs.lenght;
		points[prefs.bodyParts - 1].offset.x = 0f;
		if (prefs.divisions < 3)
		{
			prefs.divisions = 3;
		}
		int bodyParts = prefs.bodyParts;
		int teethParts = prefs.teethParts;
		int divisions = prefs.divisions;
		int num = teethParts + bodyParts;
		int mat = points[bodyParts].mat;
		int num2 = 0;
		points[bodyParts].offset.Set(0f, prefs.toothWidth, 0f);
		if (prefs.toothWidth < 0f)
		{
			prefs.toothWidth = 0f;
		}
		if (prefs.modul < 0f)
		{
			prefs.modul = 0f;
		}
		prefs.d = (float)divisions * prefs.modul * 0.5f;
		prefs.dk = ((float)divisions + 2f) * prefs.modul * 0.5f;
		df = prefs.radius;
		float num3 = 360f / (float)divisions;
		float num4 = prefs.modul * 2.25f;
		if (prefs.lr)
		{
			num3 = 0f - num3;
		}
		mesh.subMeshCount = GetComponent<Renderer>().sharedMaterials.Length;
		pointsLenght = points.GetLength(0);
		int num5 = 60000;
		tp = new int[mesh.subMeshCount];
		tr = new int[mesh.subMeshCount, num5 * 6];
		v = new Vector3[num5 + 10];
		Vector2[] array = new Vector2[num5 + 10];
		_ = new int[12];
		int[] array2 = (prefs.lr ? new int[12]
		{
			0, 2, 3, 0, 3, 1, 0, 3, 2, 0,
			1, 3
		} : new int[12]
		{
			0, 3, 2, 0, 1, 3, 0, 2, 3, 0,
			3, 1
		});
		for (int i = 0; i < bodyParts; i++)
		{
			if (num2 > num5)
			{
				break;
			}
			points[i].offset.z = 0f;
			float num6 = df;
			float num7 = ((i != prefs.bodyParts - 1) ? 0f : prefs.toothWidth);
			Vector3 vector = new Vector3(points[i].offset.x + num6, points[i].offset.y, 0f);
			Vector3 vector2 = new Vector3(points[i + 1].offset.x + num6, points[i + 1].offset.y - num7, 0f);
			if (prefs.autoUV)
			{
				points[i].uvMapping = GetAutoUV(i, 1f);
			}
			for (int j = 0; j < divisions + 1; j++)
			{
				if (num2 > num5)
				{
					break;
				}
				Quaternion quaternion = Quaternion.Euler(0f, num3 * 0.5f + num3 * (float)j, 0f);
				v[num2] = quaternion * vector;
				v[num2 + 1] = quaternion * vector2;
				SetUVs(array, i, j, num2, 5, 4, 0, 1);
				if (j != 0)
				{
					SetTriangles(array2, i, 0, num2 - 2);
				}
				num2 += 2;
			}
		}
		int num8 = num2;
		int num9 = ((mat >= 1) ? (num + 1) : 0);
		int num10;
		if (mat > 0)
		{
			points[num9 + mat].offset.y = 0f;
			points[num9 + mat].offset.x = 0f;
			num10 = mat;
		}
		else
		{
			num10 = bodyParts;
		}
		for (int k = 0; k < num10; k++)
		{
			if (num2 > num5)
			{
				break;
			}
			points[k + num9].offset.z = 0f;
			float num11 = df;
			Vector3 vector3 = new Vector3(points[k + num9].offset.x + num11, points[k + num9].offset.y, 0f);
			Vector3 vector4 = new Vector3(points[k + num9 + 1].offset.x + num11, points[k + 1 + num9].offset.y, 0f);
			if (prefs.autoUV && mat > 0)
			{
				points[k + num9].uvMapping = GetAutoUV(k + num9, -1f);
			}
			for (int l = 0; l < divisions + 1; l++)
			{
				if (num2 > num5)
				{
					break;
				}
				Quaternion quaternion2 = Quaternion.Euler(0f, num3 * 0.5f + num3 * (float)l, 0f);
				v[num2] = quaternion2 * vector3;
				v[num2 + 1] = quaternion2 * vector4;
				SetUVs(array, k, l, num2, 4, 5, 5, 5);
				if (l != 0)
				{
					for (int m = 0; m < 6; m++)
					{
						tr[points[k + num9].mat, m + tp[points[k + num9].mat]] = array2[m + 6] + num2 - 2;
					}
				}
				tp[points[k + num9].mat] += 6;
				num2 += 2;
			}
		}
		if (prefs.capB && points[0].offset.y != 0f)
		{
			array2 = (prefs.lr ? new int[6] { 0, 1, 2, 2, 1, 3 } : new int[6] { 0, 2, 1, 2, 3, 1 });
			for (int n = 0; n < divisions + 1; n++)
			{
				if (num2 > num5)
				{
					break;
				}
				v[num2] = v[n * 2];
				v[num2 + 1] = v[n * 2 + num8];
				int part = pointsLenght - 2;
				SetUVs(array, part, n, num2, 4, 5, 3, 2);
				num2 += 2;
				if (n != 0)
				{
					SetTriangles(array2, part, 0, num2 - 4);
				}
			}
		}
		Vector3[] array3 = new Vector3[8];
		array2 = (prefs.lr ? new int[12]
		{
			0, 2, 1, 1, 2, 3, 0, 1, 2, 1,
			3, 2
		} : new int[12]
		{
			0, 1, 2, 1, 3, 2, 0, 2, 1, 1,
			2, 3
		});
		for (int num12 = bodyParts; num12 < num; num12++)
		{
			if (num2 > num5)
			{
				break;
			}
			float x = points[num12].offset.x * num4 + df;
			float num13 = points[num12].offset.y * prefs.toothWidth;
			float z = (0f - points[num12].offset.z) * num4;
			float x2 = points[num12 + 1].offset.x * num4 + df;
			float num14 = points[num12 + 1].offset.y * prefs.toothWidth;
			float z2 = (0f - points[num12 + 1].offset.z) * num4;
			Vector3[] array4 = new Vector3[4]
			{
				new Vector3(x, num13, z),
				new Vector3(x, 0f - num13, z),
				new Vector3(x2, num14, z2),
				new Vector3(x2, 0f - num14, z2)
			};
			_ = Vector3.zero;
			float num15 = prefs.modul * MathF.PI;
			float num16 = points[bodyParts - 1].offset.y / num15 - 0.5f;
			if (prefs.autoUV)
			{
				points[num12].uvMapping = GetAutoUV(num12, 1f);
			}
			for (int num17 = (int)((float)divisions * 0.5f); (float)num17 < (float)divisions * num16; num17++)
			{
				if (num2 > num5)
				{
					break;
				}
				Quaternion quaternion3 = Quaternion.Euler(0f, num3 * (float)num17 + num3 * 0.5f, 0f);
				Vector3 vector5 = new Vector3(0f, num15 / (float)divisions * (float)num17, 0f);
				v[num2] = quaternion3 * array4[0] + vector5;
				v[1 + num2] = quaternion3 * array4[2] + vector5;
				SetUVs(array, num12, num17, num2, 5, 4, 0, 1);
				if (num17 != (int)((float)divisions * 0.5f))
				{
					SetTriangles(array2, num12 + 1, 0, num2 - 2);
				}
				else
				{
					array3[0] = v[num2];
					array3[1] = v[num2 + 1];
				}
				num2 += 2;
			}
			array3[4] = v[num2 - 2];
			array3[5] = v[num2 - 1];
			for (int num18 = (int)((float)divisions * 0.5f); (float)num18 < (float)divisions * num16; num18++)
			{
				if (num2 > num5)
				{
					break;
				}
				Quaternion quaternion3 = Quaternion.Euler(0f, num3 * (float)num18 + num3 * 0.5f, 0f);
				Vector3 vector5 = new Vector3(0f, num15 / (float)divisions * (float)num18, 0f);
				v[num2] = quaternion3 * array4[1] + vector5;
				v[1 + num2] = quaternion3 * array4[3] + vector5;
				SetUVs(array, num12, num18, num2, 5, 4, 0, 1);
				if (num18 != (int)((float)divisions * 0.5f))
				{
					SetTriangles(array2, num12 + 1, 6, num2 - 2);
				}
				else
				{
					array3[2] = v[num2];
					array3[3] = v[num2 + 1];
				}
				SetTriangles(array2, num12 + 1, 6, num2);
				num2 += 2;
			}
			array3[6] = v[num2 - 2];
			array3[7] = v[num2 - 1];
			if (num2 < num5)
			{
				v[num2] = array3[4];
				v[1 + num2] = array3[5];
				v[2 + num2] = array3[6];
				v[3 + num2] = array3[7];
				if (prefs.flanks)
				{
					SetUVs(array, points.Length - 1, 1, num2, 6, 6, 6, 6);
					SetUVs(array, points.Length - 1, 1, num2 + 2, 6, 6, 6, 6);
				}
				else
				{
					SetUVs(array, points.Length - 1, 1, num2, 5, 4, 6, 6);
					SetUVs(array, points.Length - 1, 1, num2 + 2, 5, 4, 6, 6);
				}
				SetTriangles(array2, num12 + 1, 0, num2);
				SetTriangles(array2, num12 + 1, 6, num2 + 2);
				num2 += 4;
				v[num2] = array3[0];
				v[1 + num2] = array3[2];
				v[2 + num2] = array3[1];
				v[3 + num2] = array3[3];
				if (prefs.flanks)
				{
					SetUVs(array, points.Length - 1, 1, num2, 6, 6, 6, 6);
					SetUVs(array, points.Length - 1, 1, num2 + 2, 6, 6, 6, 6);
				}
				else
				{
					SetUVs(array, points.Length - 1, 1, num2, 5, 4, 6, 6);
					SetUVs(array, points.Length - 1, 1, num2 + 2, 5, 4, 6, 6);
				}
				SetTriangles(array2, num12 + 1, 0, num2);
				SetTriangles(array2, num12 + 1, 6, num2 + 2);
				num2 += 4;
			}
			if (!prefs.capT || points[num].offset.y == 0f || num12 != num - 1)
			{
				continue;
			}
			for (int num19 = 0; (float)num19 < (float)divisions * num16 - (float)(int)((float)divisions * 0.5f); num19++)
			{
				if (num2 > num5)
				{
					break;
				}
				Quaternion quaternion3 = Quaternion.Euler(0f, num3 * (float)num19, 0f);
				Vector3 vector5 = new Vector3(0f, num15 / (float)divisions * (float)num19, 0f);
				v[num2] = quaternion3 * array3[1] + vector5;
				v[1 + num2] = quaternion3 * array3[3] + vector5;
				if (!points[num12].uvMapping)
				{
					SetUVs(array, points.Length - 1, num19, num2, 5, 4, 6, 6);
				}
				else
				{
					SetUVs(array, points.Length - 1, 0, num2, 5, 4, 6, 6);
				}
				if (num19 != 0)
				{
					SetTriangles(array2, num12 + 1, 0, num2 - 2);
				}
				num2 += 2;
			}
		}
		Vector3[] array5 = new Vector3[num2];
		for (int num20 = 0; num20 < num2; num20++)
		{
			array5[num20] = v[num20];
		}
		v = array5;
		Vector2[] array6 = new Vector2[num2];
		for (int num21 = 0; num21 < num2; num21++)
		{
			array6[num21] = array[num21];
		}
		array = array6;
		if (mesh.vertices.Length != num2)
		{
			mesh.Clear();
		}
		mesh.subMeshCount = GetComponent<Renderer>().sharedMaterials.Length;
		mesh.vertices = new Vector3[v.Length];
		mesh.vertices = v;
		for (int num22 = 0; num22 < mesh.subMeshCount; num22++)
		{
			int[] array7 = new int[tp[num22]];
			for (int num23 = 0; num23 < tp[num22]; num23++)
			{
				array7[num23] = tr[num22, num23];
			}
			mesh.SetTriangles(array7, num22);
		}
		mesh.colors = new Color[mesh.vertices.Length];
		mesh.uv = array;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		if (prefs.tangens)
		{
			RecalculateTangens(mesh);
		}
		vt = num2;
		v = null;
		tr = null;
		tp = null;
		create = false;
		stopWatch.Stop();
		ms = stopWatch.ElapsedMilliseconds;
	}

	private void SetTriangles(int[] _triangles, int _part, int _starIndex, int _vertexIndex)
	{
		for (int i = _starIndex; i < _starIndex + 6; i++)
		{
			tr[points[_part].mat, i + tp[points[_part].mat]] = _triangles[i] + _vertexIndex;
		}
		tp[points[_part].mat] += 6;
	}

	private void SetUVs(Vector2[] _UVs, int _part, int _index, int _vertexIndex, int _a, int _b, int _c, int _d)
	{
		if (points[_part].uvMapping)
		{
			_UVs[_vertexIndex] = UV(v[_vertexIndex], _part, _index, _b);
			_UVs[_vertexIndex + 1] = UV(v[_vertexIndex + 1], _part, _index, _a);
		}
		else
		{
			_UVs[_vertexIndex] = UV(v[_vertexIndex], _part, _index, _c);
			_UVs[_vertexIndex + 1] = UV(v[_vertexIndex + 1], _part, _index, _d);
		}
	}

	private Vector2 UV(Vector3 _point, int _p, int _i, int _j)
	{
		switch (_j)
		{
		case 2:
			return new Vector2((float)_i / (float)prefs.divisions * points[_p].uvScaleX + points[_p].uvOffX, points[_p].uvScaleX + points[_p].uvOffY + 0.5f);
		case 3:
			return new Vector2((float)_i / (float)prefs.divisions * points[_p].uvScaleX + points[_p].uvOffX, 1f + points[_p].uvOffY * points[_p].uvScaleY);
		case 6:
			return new Vector2((0f - _point.x) * points[_p].uvScaleX + points[_p].uvOffX, _point.y * points[_p].uvScaleY + points[_p].uvOffY * points[_p].uvScaleY);
		case 7:
			return new Vector2(points[_p].uvScaleX + points[_p].uvOffX, points[_p].uvScaleY + points[_p].uvOffY * points[_p].uvScaleY);
		default:
			if (points[_p].uvMapping)
			{
				return new Vector2((float)(_i + 1) / (float)prefs.divisions * points[_p].uvScaleX + points[_p].uvOffX, (float)_j * points[_p].uvScaleY + points[_p].uvOffY * points[_p].uvScaleY);
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
