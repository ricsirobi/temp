using System;
using UnityEngine;

public class DebugLine : MonoBehaviour
{
	public float destroy_time = float.MaxValue;

	public float fixed_destroy_time = float.MaxValue;

	public static void DrawRay(Vector3 start, Vector3 dir)
	{
		DrawLine(start, start + dir, Color.white);
	}

	public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration = 0f, float width = 1f)
	{
		DrawLine(start, start + dir, color, duration, width);
	}

	public static void DrawLine(Vector3 start, Vector3 end)
	{
		DrawLine(start, end, Color.white);
	}

	public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f, float width = 1f)
	{
		if ((bool)Camera.main)
		{
			GameObject obj = new GameObject("debug_line");
			LineRenderer lineRenderer = obj.AddComponent<LineRenderer>();
			lineRenderer.material = new Material(Shader.Find("Mobile/Particles/Additive"));
			lineRenderer.SetPosition(0, start);
			lineRenderer.SetPosition(1, end);
			lineRenderer.startColor = color;
			lineRenderer.endColor = color;
			lineRenderer.startWidth = CalcPixelHeightAtDist((start - Camera.main.transform.position).magnitude) * width;
			lineRenderer.endWidth = CalcPixelHeightAtDist((end - Camera.main.transform.position).magnitude) * width;
			DebugLine debugLine = obj.AddComponent<DebugLine>();
			if (Time.deltaTime == Time.fixedDeltaTime)
			{
				debugLine.fixed_destroy_time = Time.fixedTime + duration;
			}
			else
			{
				debugLine.destroy_time = Time.time + duration;
			}
		}
	}

	public static float CalcPixelHeightAtDist(float dist)
	{
		if (!Camera.main)
		{
			return 0f;
		}
		return 2f * dist * Mathf.Tan(Camera.main.fieldOfView * 0.5f * (MathF.PI / 180f)) / (float)Camera.main.pixelHeight;
	}

	public void Update()
	{
		if (Time.time > destroy_time)
		{
			UnityEngine.Object.Destroy(base.transform.gameObject);
		}
	}

	public void FixedUpdate()
	{
		if (Time.fixedTime > fixed_destroy_time)
		{
			UnityEngine.Object.Destroy(base.transform.gameObject);
		}
	}
}
