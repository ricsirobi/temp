using System;
using UnityEngine;

namespace GearFactory.Utility;

public static class MathUtils
{
	public static Vector3 AddVectorLength(this Vector3 vector, float size)
	{
		float num = Vector3.Magnitude(vector);
		num += size;
		return Vector3.Scale(Vector3.Normalize(vector), new Vector3(num, num, num));
	}

	public static Vector3 SetVectorLength(this Vector3 vector, float size)
	{
		return Vector3.Normalize(vector) * size;
	}

	public static Quaternion SubtractRotation(this Quaternion B, Quaternion A)
	{
		return Quaternion.Inverse(A) * B;
	}

	public static void PlanePlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, Plane p1, Plane p2, Vector3 plane1Position, Vector3 plane2Position)
	{
		Vector3 normal = p1.normal;
		Vector3 normal2 = p2.normal;
		linePoint = Vector3.zero;
		lineVec = Vector3.zero;
		lineVec = Vector3.Cross(normal, normal2);
		Vector3 vector = Vector3.Cross(normal2, lineVec);
		float num = Vector3.Dot(normal, vector);
		if (Mathf.Abs(num) > 0.006f)
		{
			Vector3 rhs = plane1Position - plane2Position;
			float num2 = Vector3.Dot(normal, rhs) / num;
			linePoint = plane2Position + num2 * vector;
		}
	}

	public static Vector3 LinePlaneIntersection(Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
	{
		float num = Vector3.Dot(planePoint - linePoint, planeNormal);
		float num2 = Vector3.Dot(lineVec, planeNormal);
		if (num2 != 0f)
		{
			float size = num / num2;
			Vector3 vector = lineVec.SetVectorLength(size);
			return linePoint + vector;
		}
		return Vector3.zero;
	}

	public static void ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
	{
		closestPointLine1 = Vector3.zero;
		closestPointLine2 = Vector3.zero;
		float num = Vector3.Dot(lineVec1, lineVec1);
		float num2 = Vector3.Dot(lineVec1, lineVec2);
		float num3 = Vector3.Dot(lineVec2, lineVec2);
		float num4 = num * num3 - num2 * num2;
		if (num4 != 0f)
		{
			Vector3 rhs = linePoint1 - linePoint2;
			float num5 = Vector3.Dot(lineVec1, rhs);
			float num6 = Vector3.Dot(lineVec2, rhs);
			float num7 = (num2 * num6 - num5 * num3) / num4;
			float num8 = (num * num6 - num5 * num2) / num4;
			closestPointLine1 = linePoint1 + lineVec1 * num7;
			closestPointLine2 = linePoint2 + lineVec2 * num8;
		}
		else
		{
			closestPointLine1 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			closestPointLine2 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		}
	}

	public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
	{
		float num = Vector3.Dot(point - linePoint, lineVec);
		return linePoint + lineVec * num;
	}

	public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
	{
		float num = SignedDistancePlanePoint(planeNormal, planePoint, point);
		num *= -1f;
		Vector3 vector = planeNormal.SetVectorLength(num);
		return point + vector;
	}

	public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
	{
		return vector - Vector3.Dot(vector, planeNormal) * planeNormal;
	}

	public static float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
	{
		return Vector3.Dot(planeNormal, point - planePoint);
	}

	public static float SignedDotProduct(Vector3 vectorA, Vector3 vectorB, Vector3 normal)
	{
		return Vector3.Dot(Vector3.Cross(normal, vectorA), vectorB);
	}

	public static float AngleVectorPlane(this Vector3 vector, Vector3 normal)
	{
		float num = Mathf.Acos(Vector3.Dot(vector, normal));
		return MathF.PI / 2f - num;
	}

	public static float DotProductAngle(this Vector3 vec1, Vector3 vec2)
	{
		float num = Vector3.Dot(vec1, vec2);
		if (num < -1f)
		{
			num = -1f;
		}
		if (num > 1f)
		{
			num = 1f;
		}
		return Mathf.Acos(num);
	}
}
