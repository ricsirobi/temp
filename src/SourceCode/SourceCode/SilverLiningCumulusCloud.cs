using System;
using UnityEngine;

public class SilverLiningCumulusCloud
{
	public struct Voxel
	{
		public bool phaseTransition;

		public bool vapor;

		public bool hasCloud;

		public Color color;
	}

	public float voxelSize = 350f;

	public float extinctionProbability = 0.05f;

	public float transitionProbability = 0.001f;

	public float vaporProbability = 0.06f;

	public float initialVaporProbability = 0.3f;

	public int initialEvolve = 30;

	public double albedo = 0.9;

	public double quickLightingAttenuation = 6000.0;

	public double maxT = 1.0;

	public double dropletSize = 4.8E-06;

	public double dropletsPerCubicCm = 460.0;

	public double colorRandomness = 0.15;

	public static double minPhase = 0.75;

	public static double maxPhase = 1.0;

	public Vector3 lightingWorldPos;

	private Voxel[] voxels;

	private ParticleSystem.Particle[] particles;

	public GameObject cloudPrefab;

	public GameObject cloudObject;

	private int sizeX;

	private int sizeY;

	private int sizeZ;

	public ParticleSystem particleSystem;

	private double opticalDepth;

	private float alpha;

	public SilverLiningCumulusCloud(float width, float height, float depth, Vector3 position)
	{
		voxelSize *= (float)SilverLining.unitScale;
		quickLightingAttenuation *= SilverLining.unitScale;
		sizeX = (int)(width / voxelSize);
		sizeY = (int)(height / voxelSize);
		sizeZ = (int)(depth / voxelSize);
		particles = new ParticleSystem.Particle[sizeX * sizeY * sizeZ];
		voxels = new Voxel[sizeX * sizeY * sizeZ];
		alpha = 1f;
		double num = dropletsPerCubicCm / Math.Pow(SilverLining.unitScale, 3.0) * 100.0 * 100.0 * 100.0;
		dropletSize *= SilverLining.unitScale;
		opticalDepth = Math.PI * (dropletSize * dropletSize) * (double)voxelSize * num;
		cloudPrefab = GameObject.Find("CumulusCloudPrefab");
		if (cloudPrefab != null)
		{
			cloudObject = UnityEngine.Object.Instantiate(cloudPrefab, position, Quaternion.identity);
			GameObject gameObject = GameObject.Find("CumulusClouds");
			if ((bool)gameObject)
			{
				cloudObject.transform.parent = gameObject.transform;
			}
			particleSystem = cloudObject.GetComponent<ParticleSystem>();
			particleSystem.GetComponent<Renderer>().enabled = true;
			InitializeVoxels();
		}
	}

	public void Destroy()
	{
		if (cloudObject != null)
		{
			UnityEngine.Object.Destroy(cloudObject);
		}
	}

	public void Update()
	{
		if (cloudObject.activeSelf)
		{
			particleSystem.SetParticles(particles, sizeX * sizeY * sizeZ);
			particleSystem.Simulate(0.1f);
		}
	}

	protected void InitializeVoxels()
	{
		Vector3 vector = new Vector3(voxelSize * (float)(sizeX / 2), voxelSize * (float)(sizeY / 2), voxelSize * (float)(sizeZ / 2));
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				for (int k = 0; k < sizeZ; k++)
				{
					int num = i * (sizeY * sizeZ) + j * sizeZ + k;
					Vector3 position = new Vector3((float)i * voxelSize, (float)j * voxelSize, (float)k * voxelSize) - vector;
					particles[num].startColor = new Color(1f, 1f, 0f, 1f);
					float num2 = (UnityEngine.Random.value - 0.5f) * (voxelSize * 0.2f);
					particles[num].startSize = (voxelSize + num2) * 1.414f * 2f;
					particles[num].position = position;
					particles[num].rotation = UnityEngine.Random.value * 360f;
					particles[num].velocity = new Vector3(0f, 0f, 0f);
					Vector3 vector2 = new Vector3(UnityEngine.Random.value * voxelSize * 0.5f - voxelSize * 0.25f, UnityEngine.Random.value * voxelSize * 0.5f - voxelSize * 0.25f, UnityEngine.Random.value * voxelSize * 0.5f - voxelSize * 0.25f);
					particles[num].position += vector2;
					particles[num].angularVelocity = (UnityEngine.Random.value - 0.5f) * 180f;
					voxels[num].vapor = UnityEngine.Random.value < initialVaporProbability;
					voxels[num].hasCloud = false;
					voxels[num].phaseTransition = false;
					voxels[num].color = new Color(1f, 1f, 1f, 1f);
				}
			}
		}
		if (sizeX >= 4 && sizeZ >= 4)
		{
			int num3 = (sizeX >> 2) * sizeY * sizeZ + (sizeZ >> 2);
			voxels[num3].phaseTransition = true;
			num3 = (sizeX >> 2) * sizeY * sizeZ + (sizeZ - (sizeZ >> 2));
			voxels[num3].phaseTransition = true;
			num3 = (sizeX - (sizeX >> 2)) * sizeY * sizeZ + (sizeZ - (sizeZ >> 2));
			voxels[num3].phaseTransition = true;
			num3 = (sizeX - (sizeX >> 2)) * sizeY * sizeZ + (sizeZ >> 2);
			voxels[num3].phaseTransition = true;
		}
		for (int l = 0; l < initialEvolve; l++)
		{
			IterateCellularAutomata();
		}
	}

	protected int GetIdx(int x, int y, int z)
	{
		return x * sizeY * sizeZ + y * sizeZ + z;
	}

	protected void IterateCellularAutomata()
	{
		double num = (double)sizeX * 0.5;
		double num2 = sizeY;
		double num3 = (double)sizeZ * 0.5;
		double num4 = num * num;
		double num5 = num2 * num2;
		double num6 = num3 * num3;
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				for (int k = 0; k < sizeZ; k++)
				{
					double num7 = (double)i - num;
					double num8 = j;
					double num9 = (double)k - num3;
					double num10 = num7 * num7 / num4 + num8 * num8 / num5 + num9 * num9 / num6;
					double num11 = 1.0 - num10;
					if (num11 < 0.0)
					{
						num11 = 0.0;
					}
					bool flag = (i + 1 < sizeX && voxels[GetIdx(i + 1, j, k)].phaseTransition) || (k + 1 < sizeZ && voxels[GetIdx(i, j, k + 1)].phaseTransition) || (j + 1 < sizeY && voxels[GetIdx(i, j + 1, k)].phaseTransition) || (i - 1 >= 0 && voxels[GetIdx(i - 1, j, k)].phaseTransition) || (k - 1 >= 0 && voxels[GetIdx(i, j, k - 1)].phaseTransition) || (j - 1 >= 0 && voxels[GetIdx(i, j - 1, k)].phaseTransition) || (i - 2 >= 0 && voxels[GetIdx(i - 2, j, k)].phaseTransition) || (i + 2 < sizeX && voxels[GetIdx(i + 2, j, k)].phaseTransition) || (k - 2 >= 0 && voxels[GetIdx(i, j, k - 2)].phaseTransition) || (k + 2 < sizeZ && voxels[GetIdx(i, j, k + 2)].phaseTransition) || (j - 2 >= 0 && voxels[GetIdx(i, j - 2, k)].phaseTransition);
					int idx = GetIdx(i, j, k);
					bool phaseTransition = voxels[idx].phaseTransition;
					voxels[idx].phaseTransition = (!phaseTransition && voxels[idx].vapor && flag) || (double)UnityEngine.Random.value < (double)transitionProbability * num11;
					voxels[idx].vapor = (voxels[idx].vapor && !phaseTransition) || (double)UnityEngine.Random.value < (double)vaporProbability * num11;
					voxels[idx].hasCloud = (voxels[idx].hasCloud || phaseTransition) && (double)UnityEngine.Random.value > (double)extinctionProbability * (1.0 - num11);
					particles[idx].remainingLifetime = (voxels[idx].hasCloud ? float.MaxValue : 0f);
				}
			}
		}
	}

	public void UpdateLighting(Color lightColor, Vector3 lightDir)
	{
		lightDir.Normalize();
		Vector3 vector = lightDir * -1f;
		double num = (double)((float)sizeX * voxelSize) + (double)voxelSize * 2.0;
		double num2 = (double)((float)sizeY * voxelSize) + (double)voxelSize * 2.0;
		double num3 = (double)((float)sizeZ * voxelSize) + (double)voxelSize * 2.0;
		double num4 = num / (num2 * 2.0);
		double num5 = num / num3;
		double num6 = (double)(vector.x * vector.x) + num4 * num4 * (double)vector.y * (double)vector.y + num5 * num5 * (double)vector.z * (double)vector.z;
		double num7 = 2.0 * num6;
		double num8 = 1.0 / num7;
		double num9 = 4.0 * num6;
		double num10 = num * 0.5;
		double num11 = num10 * num10;
		double num12 = num4 * num4;
		double num13 = num5 * num5;
		double num14 = albedo / (Math.PI * 4.0);
		double num15 = 1.0 / quickLightingAttenuation;
		double num16 = 1.0 - Math.Exp(0.0 - opticalDepth);
		GetRenderer().material.SetFloat("extinction", (float)num16);
		for (int i = 0; i < sizeX * sizeY * sizeZ; i++)
		{
			if (!voxels[i].hasCloud)
			{
				continue;
			}
			Vector3 position = particles[i].position;
			double num17 = 2.0 * ((double)(position.x * vector.x) + num12 * (double)position.y * (double)vector.y + num13 * (double)position.z * (double)vector.z);
			double num18 = (double)(position.x * position.x) + num12 * (double)position.y * (double)position.y + num13 * (double)position.z * (double)position.z - num11;
			double num19 = num17 * num17 - num9 * num18;
			double num20 = Math.Abs((double)(position.x + position.y + position.z) % 100.0) * 0.01;
			num20 = 1.0 - colorRandomness + colorRandomness * num20;
			if (num19 >= 0.0)
			{
				double num21 = (0.0 - num17 - Math.Sqrt(num19)) * num8;
				num21 *= 0.0 - num15;
				if (num21 > maxT)
				{
					num21 = maxT;
				}
				if (num21 < 0.0)
				{
					num21 = 0.0;
				}
				num21 = 1.0 - num21;
				num21 *= num14 * opticalDepth;
				voxels[i].color = lightColor * (float)num21;
				voxels[i].color.a = (float)num20;
			}
			else
			{
				voxels[i].color = lightColor * (float)num14 * (float)opticalDepth;
				voxels[i].color.a = (float)num20;
			}
			particles[i].startColor = voxels[i].color;
		}
	}

	public void SetAlpha(float pAlpha)
	{
		if (pAlpha > 1f)
		{
			pAlpha = 1f;
		}
		if (pAlpha < 0f)
		{
			pAlpha = 0f;
		}
		alpha = pAlpha;
		cloudObject.GetComponent<Renderer>().material.SetFloat("fade", SilverLiningCumulusCloudLayer.cloudAlpha * alpha);
	}

	public float GetAlpha()
	{
		return alpha;
	}

	public Renderer GetRenderer()
	{
		return cloudObject.GetComponent<Renderer>();
	}

	public Vector3 GetPosition()
	{
		return cloudObject.transform.position;
	}

	public void SetPosition(Vector3 position)
	{
		cloudObject.transform.position = position;
	}
}
