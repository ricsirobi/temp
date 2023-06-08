using System;
using System.Collections;
using UnityEngine;

public class SilverLiningCumulusCloudLayer
{
	public double chi = 0.984;

	public double alpha = 0.001;

	public double nu = 0.5;

	public double beta = -0.1;

	public double minSize = 500.0;

	public double maxSize = 5000.0;

	public double epsilon = 100.0;

	public float ambientBoost = 0.7f;

	public static float cloudAlpha = 0.8f;

	private ArrayList clouds;

	private Vector3 dimensions;

	private Vector3 center;

	private int currentN;

	private int targetN;

	private double currentD;

	private double coverage;

	private float a;

	private float b;

	public SilverLiningCumulusCloudLayer(Vector3 pDimensions, Vector3 pCenter, double pCoverage)
	{
		alpha /= SilverLining.unitScale;
		minSize *= SilverLining.unitScale;
		maxSize *= SilverLining.unitScale;
		epsilon *= SilverLining.unitScale;
		dimensions = pDimensions;
		a = dimensions.x * 0.5f;
		b = dimensions.z * 0.5f;
		center = pCenter;
		coverage = pCoverage;
		clouds = new ArrayList();
		CreateClouds();
	}

	private bool TestInEllipse(Vector3 cloudPos)
	{
		Vector3 vector = cloudPos - center;
		return vector.x * vector.x / (a * a) + vector.z * vector.z / (b * b) < 1f;
	}

	public void Update()
	{
		for (int i = 0; i < clouds.Count; i++)
		{
			((SilverLiningCumulusCloud)clouds[i]).Update();
		}
	}

	public void UpdateFog(bool doFog)
	{
		Color fogColor = RenderSettings.fogColor;
		Vector4 value = new Vector4(fogColor.r, fogColor.g, fogColor.b, RenderSettings.fogDensity);
		if (!RenderSettings.fog || !doFog)
		{
			value.w = 0f;
		}
		Shader.SetGlobalVector("fog", value);
	}

	public void UpdateLighting(Color lightColor, Vector3 lightDirection, Renderer renderer)
	{
		Color ambientLight = RenderSettings.ambientLight;
		Vector4 value = new Vector4(ambientLight.r, ambientLight.g, ambientLight.b, 1f);
		Vector4 value2 = new Vector4(lightColor.r, lightColor.g, lightColor.b, 1f);
		Vector4 value3 = new Vector4(lightDirection.x, lightDirection.y, lightDirection.z);
		Shader.SetGlobalFloat("minPhase", (float)SilverLiningCumulusCloud.minPhase);
		Shader.SetGlobalFloat("maxPhase", (float)SilverLiningCumulusCloud.maxPhase);
		Shader.SetGlobalFloat("ambientBoost", ambientBoost);
		Shader.SetGlobalVector("ambient", value);
		Shader.SetGlobalVector("lightColor", value2);
		Shader.SetGlobalVector("lightDir", value3);
		for (int i = 0; i < clouds.Count; i++)
		{
			SilverLiningCumulusCloud silverLiningCumulusCloud = (SilverLiningCumulusCloud)clouds[i];
			silverLiningCumulusCloud.UpdateLighting(lightColor, lightDirection);
			silverLiningCumulusCloud.GetRenderer().material.SetFloat("fade", cloudAlpha * silverLiningCumulusCloud.GetAlpha());
		}
	}

	public void WrapAndUpdateClouds(bool cloudWrap, bool cull, Color lightColor, Vector3 lightDir)
	{
		Vector3 vector = center;
		float num = dimensions.x * 0.5f;
		float num2 = dimensions.z * 0.5f;
		for (int i = 0; i < clouds.Count; i++)
		{
			SilverLiningCumulusCloud silverLiningCumulusCloud = (SilverLiningCumulusCloud)clouds[i];
			Vector3 position = silverLiningCumulusCloud.GetPosition();
			if ((double)(position - silverLiningCumulusCloud.lightingWorldPos).magnitude > 100.0 * SilverLining.unitScale)
			{
				silverLiningCumulusCloud.lightingWorldPos = position;
				silverLiningCumulusCloud.GetRenderer().material.SetVector("cloudPos", position);
			}
			if (cull)
			{
				bool activeSelf = silverLiningCumulusCloud.cloudObject.activeSelf;
				silverLiningCumulusCloud.cloudObject.SetActive(TestInEllipse(position));
				if (silverLiningCumulusCloud.cloudObject.activeSelf && !activeSelf)
				{
					silverLiningCumulusCloud.particleSystem.Simulate(0.1f);
				}
			}
			if (!cloudWrap || !silverLiningCumulusCloud.cloudObject.activeSelf)
			{
				continue;
			}
			bool flag = false;
			bool flag2;
			do
			{
				flag2 = false;
				if (position.x > vector.x + num)
				{
					position.x = vector.x - num + (position.x - (vector.x + num));
					flag2 = (flag = true);
				}
				if (position.x < vector.x - num)
				{
					position.x = vector.x + num - (vector.x - num - position.x);
					flag2 = (flag = true);
				}
				if (position.z > vector.z + num2)
				{
					position.z = vector.z - num2 + (position.z - (vector.z + num2));
					flag2 = (flag = true);
				}
				if (position.z < vector.z - num2)
				{
					position.z = vector.z + num2 - (vector.z - num2 - position.z);
					flag2 = (flag = true);
				}
			}
			while (flag2);
			if (flag)
			{
				silverLiningCumulusCloud.SetPosition(position);
			}
			float num3 = num * 2f + num2 * 2f;
			float num4 = Math.Abs(position.x - (vector.x + num)) / num;
			if (num4 < num3)
			{
				num3 = num4;
			}
			num4 = Math.Abs(position.x - (vector.x - num)) / num;
			if (num4 < num3)
			{
				num3 = num4;
			}
			num4 = Math.Abs(position.z - (vector.z + num2)) / num2;
			if (num4 < num3)
			{
				num3 = num4;
			}
			num4 = Math.Abs(position.z - (vector.z - num2)) / num2;
			if (num4 < num3)
			{
				num3 = num4;
			}
			float num5 = 1f - num3;
			if (num5 < 0f)
			{
				num5 = 0f;
			}
			if (num5 > 1f)
			{
				num5 = 1f;
			}
			float num6 = 1f - num5 * num5 * num5 * num5 * num5;
			float num7 = 0.01f;
			if (Math.Abs(silverLiningCumulusCloud.GetAlpha() - num6) > num7)
			{
				silverLiningCumulusCloud.SetAlpha(num6);
			}
		}
	}

	protected void CreateClouds()
	{
		currentN = 0;
		targetN = 0;
		currentD = maxSize * epsilon * 0.5;
		double width;
		double depth;
		double height;
		while (GetNextCloud(out width, out depth, out height))
		{
			Vector3 position = new Vector3(UnityEngine.Random.value * dimensions.x, UnityEngine.Random.value * dimensions.y, UnityEngine.Random.value * dimensions.z);
			position += center;
			position -= new Vector3(dimensions.x * 0.5f, 0f, dimensions.z * 0.5f);
			SilverLiningCumulusCloud value = new SilverLiningCumulusCloud((float)width, (float)height, (float)depth, position);
			clouds.Add(value);
		}
	}

	public void DestroyClouds()
	{
		for (int i = 0; i < clouds.Count; i++)
		{
			((SilverLiningCumulusCloud)clouds[i]).Destroy();
		}
		clouds.Clear();
	}

	protected bool GetNextCloud(out double width, out double depth, out double height)
	{
		width = (depth = (height = 0.0));
		while (currentN >= targetN)
		{
			currentD -= epsilon;
			if (currentD <= minSize)
			{
				return false;
			}
			currentN = 0;
			targetN = (int)(2.0 * (double)dimensions.x * (double)dimensions.z * epsilon * alpha * alpha * alpha * coverage / (Math.PI * chi) * Math.Exp((0.0 - alpha) * currentD));
		}
		if (currentD <= minSize)
		{
			return false;
		}
		double num = (double)UnityEngine.Random.value * epsilon;
		double num2 = (double)UnityEngine.Random.value * epsilon;
		width = currentD - epsilon * 0.5 + num;
		depth = currentD - epsilon * 0.5 + num2;
		double num3 = (width + depth) * 0.5;
		double num4 = nu * Math.Pow(num3 / maxSize, beta);
		height = num3 * num4;
		currentN++;
		return true;
	}
}
