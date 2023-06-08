using UnityEngine;

public static class DebugUtils
{
	public static int numberOfSeconds = 3;

	public static void Draw(Color c1, params Vector3[] vectors)
	{
		Draw(c1, depthTest: false, vectors);
	}

	public static void Draw(Color c1, bool depthTest, params Vector3[] vectors)
	{
		Draw(c1, depthTest, default(Matrix4x4), vectors);
	}

	public static void Draw(Color c1, bool depthTest, Matrix4x4 matrix, params Vector3[] vectors)
	{
		if (vectors.Length == 1)
		{
			Vector3 vector = matrix.MultiplyPoint(new Vector3(vectors[0].x, vectors[0].y, vectors[0].z));
			Vector3 vector2 = matrix.MultiplyPoint(new Vector3(vectors[0].x + 1f, vectors[0].y, vectors[0].z));
			Vector3 vector3 = matrix.MultiplyPoint(new Vector3(vectors[0].x, vectors[0].y, vectors[0].z));
			Vector3 vector4 = matrix.MultiplyPoint(new Vector3(vectors[0].x, vectors[0].y + 1f, vectors[0].z));
			Vector3 vector5 = matrix.MultiplyPoint(new Vector3(vectors[0].x, vectors[0].y, vectors[0].z));
			Vector3 vector6 = matrix.MultiplyPoint(new Vector3(vectors[0].x, vectors[0].y, vectors[0].z + 1f));
			Draw(Color.red, depthTest, vector, vector2);
			Draw(Color.green, depthTest, vector3, vector4);
			Draw(Color.blue, depthTest, vector5, vector6);
		}
		else
		{
			for (int i = 0; i < vectors.Length - 1; i++)
			{
				Debug.DrawLine(matrix.MultiplyPoint(vectors[i]), matrix.MultiplyPoint(vectors[i + 1]), c1, numberOfSeconds, depthTest);
			}
		}
	}

	public static void DrawBoundingBox(this GameObject g1, Color c1)
	{
		Vector3[] array = g1.CalcOrientedBoundingBox();
		Debug.DrawLine(array[0], array[1], c1, numberOfSeconds, depthTest: false);
		Debug.DrawLine(array[1], array[2], c1, numberOfSeconds, depthTest: false);
		Debug.DrawLine(array[2], array[3], c1, numberOfSeconds, depthTest: false);
		Debug.DrawLine(array[3], array[0], c1, numberOfSeconds, depthTest: false);
		Debug.DrawLine(array[4], array[5], c1, numberOfSeconds, depthTest: false);
		Debug.DrawLine(array[5], array[6], c1, numberOfSeconds, depthTest: false);
		Debug.DrawLine(array[6], array[7], c1, numberOfSeconds, depthTest: false);
		Debug.DrawLine(array[7], array[4], c1, numberOfSeconds, depthTest: false);
		Debug.DrawLine(array[0], array[4], c1, numberOfSeconds, depthTest: false);
		Debug.DrawLine(array[1], array[5], c1, numberOfSeconds, depthTest: false);
		Debug.DrawLine(array[2], array[6], c1, numberOfSeconds, depthTest: false);
		Debug.DrawLine(array[3], array[7], c1, numberOfSeconds, depthTest: false);
	}

	public static void DrawDisc(this GameObject gameObject, Color c1, string label)
	{
	}

	public static void DrawPoweredBy(this GFGear gear1)
	{
		if (gear1.DrivenBy != null)
		{
			Debug.DrawLine(gear1.transform.position, gear1.DrivenBy.transform.position, Color.yellow, 3f, depthTest: false);
		}
	}
}
