using System;
using UnityEngine;

public class AutoGenerateSpline
{
	public Spline GenerateSpline(Vector3 pos, int maxNodes, MMOAvatarSimulator.MinMax randomOffsetMinMax, string splineName, float heightOffset)
	{
		Spline spline = new Spline(maxNodes, looping: false, constSpeed: true, alignTangent: true, hasQ: false);
		float randomOffset = UnityEngine.Random.Range(randomOffsetMinMax._Min, randomOffsetMinMax._Max);
		for (int i = 0; i < maxNodes; i++)
		{
			spline.SetControlPoint(i, GetRandomPos(pos + Vector3.up * heightOffset, randomOffset), GetRandomRotation(AvAvatar.mTransform.rotation, randomOffset), 0f);
		}
		spline.RecalculateSpline();
		return spline;
	}

	private Quaternion GetRandomRotation(Quaternion initRot, float randomOffset)
	{
		float num = Mathf.Cos((float)(UnityEngine.Random.Range(0, 24) * 15) * (MathF.PI / 180f)) * randomOffset;
		Vector3 eulerAngles = initRot.eulerAngles;
		eulerAngles.y += num;
		initRot.eulerAngles = eulerAngles;
		return initRot;
	}

	private Vector3 GetRandomPos(Vector3 inPosition, float randomOffset)
	{
		if (randomOffset > 0f)
		{
			float num = UnityEngine.Random.Range(0, 24) * 15;
			float num2 = Mathf.Cos(num * (MathF.PI / 180f)) * randomOffset;
			float num3 = Mathf.Sin(num * (MathF.PI / 180f)) * randomOffset;
			inPosition.x += num2;
			inPosition.z += num3;
		}
		if (Mathf.Approximately(inPosition.y, float.NegativeInfinity))
		{
			inPosition.y = AvAvatar.position.y;
			Debug.Log("Infinity Error");
		}
		float groundHeight = 0f;
		UtUtilities.GetGroundHeight(inPosition, 100f, out groundHeight);
		if (groundHeight < -10000f)
		{
			inPosition = AvAvatar.position;
		}
		else
		{
			inPosition.y = groundHeight;
		}
		return inPosition;
	}
}
