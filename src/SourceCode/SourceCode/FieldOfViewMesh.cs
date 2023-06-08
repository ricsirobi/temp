using UnityEngine;

public class FieldOfViewMesh : ProceduralMesh
{
	protected Vector3[] vertices;

	protected int[] triangles;

	protected Vector2[] uvs;

	public bool GenerateFOV(float angle, float radius, int curveSeg, int yPolygonLevel)
	{
		if (yPolygonLevel <= 0)
		{
			return false;
		}
		int num = (curveSeg + 2) * yPolygonLevel;
		vertices = new Vector3[num];
		Vector3 vector = Quaternion.AngleAxis(angle / 2f, Vector3.up) * Vector3.forward * radius;
		Vector3 vector2 = vector;
		Vector3 b = Quaternion.AngleAxis((0f - angle) / 2f, Vector3.up) * Vector3.forward * radius;
		int num2 = 0;
		for (int i = 0; i < yPolygonLevel; i++)
		{
			Vector3 vector3 = new Vector3(0f, i, 0f);
			vertices[num2++] = Vector3.zero + vector3;
			vertices[num2++] = vector2 + vector3;
			for (int j = 1; j <= curveSeg; j++)
			{
				Vector3 vector4 = Vector3.Slerp(vector, b, (float)j / (float)curveSeg) + Vector3.zero;
				vertices[num2++] = vector4 + vector3;
			}
		}
		uvs = new Vector2[vertices.Length];
		for (int k = 0; k < uvs.Length; k++)
		{
			uvs[k] = new Vector2(vertices[k].x, vertices[k].z);
		}
		GenerateTriangle(ref triangles, curveSeg, yPolygonLevel);
		GenerateMesh(vertices, triangles, uvs);
		return true;
	}

	private bool GenerateTriangle(ref int[] triangles, int curveSeg, int yLevel)
	{
		if (yLevel < 1)
		{
			return false;
		}
		int num = curveSeg + 2;
		int num2 = ((yLevel > 1) ? (curveSeg * 2) : curveSeg);
		int num3 = curveSeg * 2 * (yLevel - 1);
		int num4 = 2;
		int num5 = 2 * num4 * (yLevel - 1);
		triangles = new int[(num2 + num3 + num5) * 3];
		int num6 = 0;
		int num7 = num * yLevel - (curveSeg + 2);
		if (yLevel == 1)
		{
			for (int i = 0; i < curveSeg; i++)
			{
				triangles[num6++] = 0;
				triangles[num6++] = i + 2;
				triangles[num6++] = i + 1;
			}
		}
		else if (yLevel > 1)
		{
			for (int j = 0; j < curveSeg; j++)
			{
				triangles[num6++] = 0;
				triangles[num6++] = j + 1;
				triangles[num6++] = j + 2;
			}
			for (int k = 0; k < yLevel - 1; k++)
			{
				for (int l = 0; l < curveSeg; l++)
				{
					triangles[num6++] = l + 1 + num * k;
					triangles[num6++] = l + 1 + num * (k + 1);
					triangles[num6++] = l + 2 + num * k;
					triangles[num6++] = l + 2 + num * k;
					triangles[num6++] = l + 1 + num * (k + 1);
					triangles[num6++] = l + 2 + num * (k + 1);
				}
				triangles[num6++] = num * k;
				triangles[num6++] = 1 + num * (k + 1);
				triangles[num6++] = 1 + num * k;
				triangles[num6++] = num * k;
				triangles[num6++] = num * (k + 1);
				triangles[num6++] = 1 + num * (k + 1);
				triangles[num6++] = num * k;
				triangles[num6++] = curveSeg + 1 + num * k;
				triangles[num6++] = curveSeg + 1 + num * (k + 1);
				triangles[num6++] = num * k;
				triangles[num6++] = curveSeg + 1 + num * (k + 1);
				triangles[num6++] = num * (k + 1);
			}
			for (int m = 0; m < curveSeg; m++)
			{
				triangles[num6++] = num7;
				triangles[num6++] = num7 + m + 2;
				triangles[num6++] = num7 + m + 1;
			}
		}
		return true;
	}
}
