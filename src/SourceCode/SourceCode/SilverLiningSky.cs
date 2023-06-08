using System;
using System.Collections.Generic;
using UnityEngine;

public class SilverLiningSky
{
	private double PI = 3.14159265;

	private float PIf = MathF.PI;

	public float XBoost;

	public float YBoost;

	public float ZBoost;

	public int boostExp = 3;

	public float sunTransmissionScale = 1f;

	public float sunScatteredScale = 1f;

	public float moonTransmissionScale = 1f;

	public float moonScatteredScale = 1f;

	public double aScale = 1.0;

	public double bScale = 1.0;

	public double cScale = 1.0;

	public double dScale = 1.0;

	public double eScale = 1.0;

	public double moonScale = 0.01;

	public double sunLuminanceScale = 1.0;

	public double moonLuminanceScale = 0.1;

	private double H = 8435.0;

	public float oneOverGamma = 0.45f;

	public float sunDistance = 90000f;

	public float sunWidthDegrees = 1f;

	public float moonDistance = 90000f;

	public float moonWidthDegrees = 1f;

	public float duskZenithLuminance = 0.02f;

	public float fogThickness = 500f;

	public int sphereSegments = 32;

	private SilverLiningEphemeris ephemeris;

	private Dictionary<int, float> twilightLuminance;

	public double T = 2.2;

	private double lastT;

	private double lastSunT;

	private double lastMoonT;

	private double lastSunZenith;

	private double lastMoonZenith;

	private bool lightingChanged = true;

	private double AY;

	private double BY;

	private double CY;

	private double DY;

	private double EY;

	private double Ax;

	private double Bx;

	private double Cx;

	private double Dx;

	private double Ex;

	private double Ay;

	private double By;

	private double Cy;

	private double Dy;

	private double Ey;

	private double thetaS;

	private double thetaM;

	private double xZenith;

	private double yZenith;

	private double YZenith;

	private double xMoon;

	private double yMoon;

	private double YMoon;

	private double sunx;

	private double suny;

	private double sunY;

	private double moonx;

	private double moony;

	private double moonY;

	public double maxSkylightLuminance = 1.0;

	private Color skyLight;

	private Vector3 sunTransmittedLuminance;

	private Vector3 moonTransmittedLuminance;

	private Vector3 sunScatteredLuminance;

	private Vector3 moonScatteredLuminance;

	private SilverLiningSpectrum sunSpectrum;

	private SilverLiningSpectrum lunarSpectrum;

	private bool isOvercast;

	private float overcastBlend = 1f;

	private float overcastTransmission = 0.2f;

	public double lightPollution;

	private double isothermalEffect = 1.0;

	private SilverLiningMatrix3 XYZ2RGB;

	private Matrix4x4 XYZ2RGB4;

	private GameObject sun;

	private GameObject sunLight;

	private GameObject moon;

	private GameObject moonLight;

	private Texture[] moonTextures;

	private Shader starFogShader;

	private Shader starNoFogShader;

	private SilverLiningStars stars;

	public float sunLightScale = 1f;

	public float moonLightScale = 1f;

	public float ambientLightScale = 1f;

	protected double DEGREES(double x)
	{
		return x * (180.0 / PI);
	}

	protected double RADIANS(double x)
	{
		return x * (PI / 180.0);
	}

	protected float RADIANS(float x)
	{
		return x * (PIf / 180f);
	}

	protected double NITS(double irradiance)
	{
		return irradiance * 683.0 / 3.14;
	}

	public SilverLiningSky()
	{
		sunDistance *= (float)SilverLining.unitScale;
		moonDistance *= (float)SilverLining.unitScale;
		H *= (float)SilverLining.unitScale;
		ephemeris = new SilverLiningEphemeris();
		InitTwilightLuminances();
		sunSpectrum = new SilverLiningSolarSpectrum();
		lunarSpectrum = new SilverLiningLunarSpectrum();
		XYZ2RGB = new SilverLiningMatrix3(3.240479, -0.969256, 0.055648, -1.53715, 1.875992, -0.204043, -0.498535, 0.041556, 1.057311);
		XYZ2RGB4 = default(Matrix4x4);
		XYZ2RGB4[0, 0] = 3.240479f;
		XYZ2RGB4[0, 1] = -0.969256f;
		XYZ2RGB4[0, 2] = 0.055648f;
		XYZ2RGB4[0, 3] = 0f;
		XYZ2RGB4[1, 0] = -1.53715f;
		XYZ2RGB4[1, 1] = 1.875992f;
		XYZ2RGB4[1, 2] = -0.204043f;
		XYZ2RGB4[1, 3] = 0f;
		XYZ2RGB4[2, 0] = -0.498535f;
		XYZ2RGB4[2, 1] = 0.041556f;
		XYZ2RGB4[2, 2] = 1.057311f;
		XYZ2RGB4[2, 3] = 0f;
		XYZ2RGB4[3, 0] = 0f;
		XYZ2RGB4[3, 1] = 0f;
		XYZ2RGB4[3, 2] = 0f;
		XYZ2RGB4[3, 3] = 1f;
	}

	public void Start()
	{
		sun = GameObject.Find("SilverLiningSun");
		sun.GetComponent<Transform>().position = new Vector3(sunDistance, 0f, 0f);
		ParticleSystem.MainModule main = sun.GetComponent<ParticleSystem>().main;
		float num = sunWidthDegrees * 1.8028169f;
		main.startSize = sunDistance * (float)Math.Tan(RADIANS(num * 0.5f)) * 2f;
		main.startLifetime = 1f;
		moon = GameObject.Find("SilverLiningMoon");
		moon.GetComponent<Transform>().position = new Vector3(moonDistance, 0f, 0f);
		main = moon.GetComponent<ParticleSystem>().main;
		float num2 = moonWidthDegrees * 1.024f;
		main.startSize = moonDistance * (float)Math.Tan(RADIANS(num2 * 0.5f)) * 2f;
		main.startLifetime = 1f;
		starFogShader = Shader.Find("Particles/Stars");
		starNoFogShader = Shader.Find("Particles/Stars No Fog");
		moonTextures = new Texture[30];
		for (int i = 0; i < 30; i++)
		{
			string path = "moonday" + (i + 1);
			moonTextures[i] = (Texture)Resources.Load(path);
		}
		ParticleSystemRenderer component = moon.GetComponent<ParticleSystemRenderer>();
		component.material.mainTexture = moonTextures[15];
		component.enabled = true;
		sun.GetComponent<ParticleSystemRenderer>().enabled = true;
		sunLight = GameObject.Find("SilverLiningSunLight");
		moonLight = GameObject.Find("SilverLiningMoonLight");
		stars = new SilverLiningStars(ephemeris);
		createSphere();
	}

	public void Update(SilverLiningTime time, SilverLiningLocation loc, Renderer renderer, bool bIsOvercast, bool doFog)
	{
		ephemeris.Update(time, loc);
		isOvercast = bIsOvercast;
		lightingChanged = false;
		ComputeSun(loc.GetAltitude());
		ComputeMoon(loc.GetAltitude());
		UpdatePerezCoefficients();
		UpdateZenith(loc.GetAltitude());
		sunx = Perezx(0.0, thetaS);
		suny = Perezy(0.0, thetaS);
		sunY = PerezY(0.0, thetaS);
		moonY = PerezY(0.0, thetaM);
		moonx = Perezx(0.0, thetaM);
		moony = Perezy(0.0, thetaM);
		ComputeLogAvg();
		ComputeToneMappedSkyLight();
		renderer.material.SetColor("theColor", Color.cyan);
		Vector4 value = new Vector4((float)sunx, (float)suny, (float)sunY, 1f);
		Vector4 value2 = new Vector4((float)xZenith, (float)yZenith, (float)YZenith, 1f);
		Vector4 value3 = new Vector4((float)moonx, (float)moony, (float)moonY, 1f);
		Vector4 value4 = new Vector4((float)xMoon, (float)yMoon, (float)YMoon, 1f);
		Vector4 value5 = new Vector4((float)Ax, (float)Bx, (float)Cx, 1f);
		Vector4 value6 = new Vector4((float)Dx, (float)Ex, 0f, 1f);
		Vector4 value7 = new Vector4((float)Ay, (float)By, (float)Cy, 1f);
		Vector4 value8 = new Vector4((float)Dy, (float)Ey, 0f, 0f);
		Vector4 value9 = new Vector4((float)AY, (float)BY, (float)CY, 1f);
		Vector4 value10 = new Vector4((float)DY, (float)EY, 0f, 0f);
		SilverLiningLuminanceMapper.GetLuminanceScales(out var rodSF, out var coneSF);
		Vector4 value11 = new Vector4((float)rodSF, (float)coneSF, 0f, 0f);
		Vector4 value12 = new Vector4((float)SilverLiningLuminanceMapper.GetRodConeBlend(), (float)SilverLiningLuminanceMapper.GetMaxDisplayLuminance(), oneOverGamma, 1f);
		Vector3 sunPositionHorizon = ephemeris.GetSunPositionHorizon();
		sunPositionHorizon.Normalize();
		Vector3 moonPositionHorizon = ephemeris.GetMoonPositionHorizon();
		moonPositionHorizon.Normalize();
		Vector4 value13 = new Vector4(isOvercast ? 1 : 0, isOvercast ? overcastBlend : 0f, overcastTransmission, 0f);
		Color color = new Color(1f, 1f, 1f, 1f);
		float w = 0f;
		float w2 = 1E+20f;
		if (RenderSettings.fog && doFog)
		{
			color = RenderSettings.fogColor;
			w = RenderSettings.fogDensity;
			w2 = fogThickness;
		}
		Vector4 value14 = new Vector4(color.r, color.g, color.b, w);
		value13.w = w2;
		renderer.material.SetVector("sunPos", sunPositionHorizon);
		renderer.material.SetVector("moonPos", moonPositionHorizon);
		renderer.material.SetVector("sunPerez", value);
		renderer.material.SetVector("moonPerez", value3);
		renderer.material.SetVector("zenithMoonPerez", value4);
		renderer.material.SetVector("zenithPerez", value2);
		renderer.material.SetVector("xPerezABC", value5);
		renderer.material.SetVector("xPerezDE", value6);
		renderer.material.SetVector("yPerezABC", value7);
		renderer.material.SetVector("yPerezDE", value8);
		renderer.material.SetVector("YPerezABC", value9);
		renderer.material.SetVector("YPerezDE", value10);
		renderer.material.SetVector("luminanceScales", value11);
		renderer.material.SetVector("kAndLdmax", value12);
		renderer.material.SetVector("overcast", value13);
		renderer.material.SetVector("fog", value14);
		renderer.material.SetMatrix("XYZtoRGB", XYZ2RGB4);
		UpdateSun();
		UpdateMoon();
		UpdateLight();
		if (YZenith < (double)duskZenithLuminance)
		{
			stars.Enable(enabled: true);
			stars.Update();
		}
		else
		{
			stars.Enable(enabled: false);
		}
		if (starFogShader != null && starNoFogShader != null)
		{
			ParticleSystemRenderer component = sun.GetComponent<ParticleSystemRenderer>();
			if (component != null)
			{
				component.material.shader = (doFog ? starFogShader : starNoFogShader);
			}
			component = moon.GetComponent<ParticleSystemRenderer>();
			if (component != null)
			{
				component.material.shader = (doFog ? starFogShader : starNoFogShader);
			}
			stars.SetShader(doFog ? starFogShader : starNoFogShader);
		}
	}

	private void createSphere()
	{
		int num = 2 + sphereSegments * sphereSegments * 2;
		int num2 = sphereSegments * 4 + sphereSegments * 4 * (sphereSegments - 1);
		GameObject gameObject = GameObject.Find("_SilverLiningSky");
		if (gameObject == null)
		{
			Debug.LogError("_SilverLiningSky not found!");
			return;
		}
		MeshFilter component = gameObject.GetComponent<MeshFilter>();
		if (component == null)
		{
			Debug.LogError("MeshFilter not found!");
			return;
		}
		Mesh sharedMesh = component.sharedMesh;
		if (sharedMesh == null)
		{
			component.mesh = new Mesh();
			sharedMesh = component.sharedMesh;
		}
		sharedMesh.Clear();
		Vector3[] array = new Vector3[num];
		int[] array2 = new int[num2 * 3];
		float num3 = MathF.PI / ((float)sphereSegments + 1f);
		float num4 = MathF.PI / (float)sphereSegments;
		array[0] = new Vector3(0f, 1f, 0f);
		array[1] = new Vector3(0f, -1f, 0f);
		for (int i = 0; i < sphereSegments; i++)
		{
			for (int j = 0; j < sphereSegments * 2; j++)
			{
				float y = (float)Math.Cos((float)(i + 1) * num3);
				float x = (float)Math.Sin((float)j * num4) * (float)Math.Sin((float)(i + 1) * num3);
				float z = (float)Math.Cos((float)j * num4) * (float)Math.Sin((float)(i + 1) * num3);
				array[2 + j + i * sphereSegments * 2] = new Vector3(x, y, z);
			}
		}
		for (int j = 0; j < sphereSegments * 2; j++)
		{
			array2[3 * j] = 0;
			array2[3 * j + 1] = j + 2;
			array2[3 * j + 2] = j + 3;
			if (j == sphereSegments * 2 - 1)
			{
				array2[3 * j + 2] = 2;
			}
		}
		int num5;
		int num6;
		for (int i = 0; i < sphereSegments - 1; i++)
		{
			num5 = 2 + i * sphereSegments * 2;
			num6 = 3 * sphereSegments * 2 + i * 6 * sphereSegments * 2;
			for (int j = 0; j < sphereSegments * 2; j++)
			{
				array2[6 * j + num6] = num5 + j;
				array2[6 * j + 2 + num6] = num5 + j + 1;
				array2[6 * j + 1 + num6] = num5 + j + sphereSegments * 2;
				array2[6 * j + num6 + 3] = num5 + j + sphereSegments * 2;
				array2[6 * j + 2 + num6 + 3] = num5 + j + 1;
				array2[6 * j + 1 + num6 + 3] = num5 + j + sphereSegments * 2 + 1;
				if (j == sphereSegments * 2 - 1)
				{
					array2[6 * j + 2 + num6] = num5 + j + 1 - 2 * sphereSegments;
					array2[6 * j + 2 + num6 + 3] = num5 + j + 1 - 2 * sphereSegments;
					array2[6 * j + 1 + num6 + 3] = num5 + j + sphereSegments * 2 + 1 - 2 * sphereSegments;
				}
			}
		}
		num5 = num - sphereSegments * 2;
		num6 = num2 * 3 - 3 * sphereSegments * 2;
		for (int j = 0; j < sphereSegments * 2; j++)
		{
			array2[3 * j + num6] = 1;
			array2[3 * j + 1 + num6] = num5 + j + 1;
			array2[3 * j + 2 + num6] = num5 + j;
			if (j == sphereSegments * 2 - 1)
			{
				array2[3 * j + 1 + num6] = num5;
			}
		}
		sharedMesh.vertices = array;
		sharedMesh.triangles = array2;
		sharedMesh.RecalculateBounds();
	}

	private static void scaleDownToOne(ref Vector3 v)
	{
		float num = 0f;
		if (v.x < num)
		{
			num = v.x;
		}
		if (v.y < num)
		{
			num = v.y;
		}
		if (v.z < num)
		{
			num = v.z;
		}
		v.x -= num;
		v.y -= num;
		v.z -= num;
		float num2 = v.x;
		if (v.y > num2)
		{
			num2 = v.y;
		}
		if (v.z > num2)
		{
			num2 = v.z;
		}
		if (num2 > 1f)
		{
			v.x /= num2;
			v.y /= num2;
			v.z /= num2;
		}
	}

	private void UpdateLight()
	{
		Vector3 XYZ = sunTransmittedLuminance * (float)sunLuminanceScale;
		SilverLiningLuminanceMapper.DurandMapperXYZ(ref XYZ);
		Vector3 v = XYZ * XYZ2RGB;
		ApplyGamma(ref v);
		Color color = new Color(v.x, v.y, v.z);
		Transform component = sunLight.GetComponent<Transform>();
		Vector3 position = component.position;
		component.LookAt(position - ephemeris.GetSunPositionHorizon());
		Light component2 = sunLight.GetComponent<Light>();
		component2.color = color * sunLightScale;
		component2.enabled = component2.color.g > 1E-07f;
		Vector3 XYZ2 = moonTransmittedLuminance * (float)moonLuminanceScale;
		SilverLiningLuminanceMapper.DurandMapperXYZ(ref XYZ2);
		v = XYZ2 * XYZ2RGB;
		ApplyGamma(ref v);
		Color color2 = new Color(v.x, v.y, v.z);
		Transform component3 = moonLight.GetComponent<Transform>();
		Vector3 position2 = component3.position;
		component3.LookAt(position2 - ephemeris.GetMoonPositionHorizon());
		Light component4 = moonLight.GetComponent<Light>();
		component4.color = color2 * moonLightScale;
		component4.enabled = component4.color.g > 1E-07f;
		int num = (int)Math.Floor(ephemeris.GetMoonPhaseAngle() / (2.0 * PI) * 30.0);
		ParticleSystemRenderer component5 = moon.GetComponent<ParticleSystemRenderer>();
		component5.material.mainTexture = moonTextures[num];
		component5.enabled = true;
		RenderSettings.ambientLight = skyLight * ambientLightScale;
	}

	private void UpdateMoon()
	{
		if (!(Camera.main == null))
		{
			Vector3 moonPositionHorizon = ephemeris.GetMoonPositionHorizon();
			moonPositionHorizon.Normalize();
			moonPositionHorizon *= moonDistance;
			moon.GetComponent<Transform>().position = moonPositionHorizon + Camera.main.transform.position;
			Vector3 v = moonTransmittedLuminance * XYZ2RGB;
			scaleDownToOne(ref v);
			v.Normalize();
			if (v.x < 0f)
			{
				v.x = 0f;
			}
			if (v.y < 0f)
			{
				v.y = 0f;
			}
			if (v.z < 0f)
			{
				v.z = 0f;
			}
			ApplyGamma(ref v);
			Vector3 vector = new Vector3(1f, 1f, 1f);
			v = v * (float)isothermalEffect + vector * (1f - (float)isothermalEffect);
			Color color = new Color(v.x, v.y, v.z);
			ParticleSystem.MainModule main = moon.GetComponent<ParticleSystem>().main;
			main.startColor = color;
		}
	}

	private void UpdateSun()
	{
		if (!(Camera.main == null))
		{
			Vector3 sunPositionHorizon = ephemeris.GetSunPositionHorizon();
			sunPositionHorizon.Normalize();
			sunPositionHorizon *= sunDistance;
			sun.GetComponent<Transform>().position = sunPositionHorizon + Camera.main.transform.position;
			Vector3 v = sunTransmittedLuminance * XYZ2RGB;
			scaleDownToOne(ref v);
			v.Normalize();
			if (v.x < 0f)
			{
				v.x = 0f;
			}
			if (v.y < 0f)
			{
				v.y = 0f;
			}
			if (v.z < 0f)
			{
				v.z = 0f;
			}
			ApplyGamma(ref v);
			Vector3 vector = new Vector3(1f, 1f, 1f);
			v = v * (float)isothermalEffect + vector * (1f - (float)isothermalEffect);
			Color color = new Color(v.x, v.y, v.z);
			ParticleSystem.MainModule main = sun.GetComponent<ParticleSystem>().main;
			main.startColor = color;
		}
	}

	private void ComputeLogAvg()
	{
		Vector3 vector = sunScatteredLuminance + moonScatteredLuminance + moonTransmittedLuminance;
		vector.y += (float)NightSkyLuminance() * 1000f;
		double coneNits = vector.y;
		SilverLiningLuminanceMapper.SetSceneLogAvg(-0.702 * (double)vector.x + 1.039 * (double)vector.y + 0.433 * (double)vector.z, coneNits);
	}

	private void ComputeToneMappedSkyLight()
	{
		Vector3 XYZ = sunScatteredLuminance * (float)sunLuminanceScale;
		XYZ.y += (float)NightSkyLuminance() * 1000f;
		Vector3 XYZ2 = moonScatteredLuminance * (float)moonLuminanceScale;
		SilverLiningLuminanceMapper.DurandMapperXYZ(ref XYZ);
		SilverLiningLuminanceMapper.DurandMapperXYZ(ref XYZ2);
		Vector3 vector = XYZ + XYZ2;
		if (vector.x > (float)maxSkylightLuminance)
		{
			vector.x = (float)maxSkylightLuminance;
		}
		if (vector.y > (float)maxSkylightLuminance)
		{
			vector.y = (float)maxSkylightLuminance;
		}
		if (vector.z > (float)maxSkylightLuminance)
		{
			vector.z = (float)maxSkylightLuminance;
		}
		Vector3 v = vector * XYZ2RGB;
		ApplyGamma(ref v);
		skyLight = new Color(v.y, v.y, v.y);
	}

	private void ApplyGamma(ref Vector3 v)
	{
		float num = 0f;
		if (v.x < num)
		{
			num = v.x;
		}
		if (v.y < num)
		{
			num = v.y;
		}
		if (v.z < num)
		{
			num = v.z;
		}
		num = 0f - num;
		v.x += num;
		v.y += num;
		v.z += num;
		float num2 = v.x;
		if (v.y > num2)
		{
			num2 = v.y;
		}
		if (v.z > num2)
		{
			num2 = v.z;
		}
		if (num2 > 1f)
		{
			v.x /= num2;
			v.y /= num2;
			v.z /= num2;
		}
		if (v.x > 0f)
		{
			v.x = (float)Math.Pow(v.x, oneOverGamma);
		}
		if (v.y > 0f)
		{
			v.y = (float)Math.Pow(v.y, oneOverGamma);
		}
		if (v.z > 0f)
		{
			v.z = (float)Math.Pow(v.z, oneOverGamma);
		}
	}

	private double PerezY(double theta, double gamma)
	{
		return (1.0 + AY * Math.Exp(BY / Math.Cos(theta))) * (1.0 + CY * Math.Exp(DY * gamma) + EY * Math.Cos(gamma) * Math.Cos(gamma));
	}

	private double Perezx(double theta, double gamma)
	{
		return (1.0 + Ax * Math.Exp(Bx / Math.Cos(theta))) * (1.0 + Cx * Math.Exp(Dx * gamma) + Ex * Math.Cos(gamma) * Math.Cos(gamma));
	}

	private double Perezy(double theta, double gamma)
	{
		return (1.0 + Ay * Math.Exp(By / Math.Cos(theta))) * (1.0 + Cy * Math.Exp(Dy * gamma) + Ey * Math.Cos(gamma) * Math.Cos(gamma));
	}

	private double AngleBetween(Vector3 v1, Vector3 v2)
	{
		Vector3 lhs = v1;
		lhs.Normalize();
		Vector3 rhs = v2;
		rhs.Normalize();
		return Math.Acos(Vector3.Dot(lhs, rhs));
	}

	private double NightSkyLuminance()
	{
		double irradiance = lightPollution + 2E-06 + 1.2E-07 + 3E-08 + 5.1E-08 + 9.1E-09 + 9.1E-10;
		return NITS(irradiance) * isothermalEffect * 0.001;
	}

	private void UpdateZenith(double altitude)
	{
		Vector3 sunPositionHorizon = ephemeris.GetSunPositionHorizon();
		Vector3 v = new Vector3(0f, 1f, 0f);
		thetaS = AngleBetween(v, sunPositionHorizon);
		Vector3 moonPositionHorizon = ephemeris.GetMoonPositionHorizon();
		thetaM = AngleBetween(v, moonPositionHorizon);
		double num = sunScatteredLuminance.x + sunScatteredLuminance.y + sunScatteredLuminance.z;
		double num2 = moonScatteredLuminance.x + moonScatteredLuminance.y + moonScatteredLuminance.z;
		xZenith = (yZenith = (xMoon = (yMoon = 0.2)));
		if (num != 0.0)
		{
			xZenith = (double)sunScatteredLuminance.x / num;
			yZenith = (double)sunScatteredLuminance.y / num;
		}
		if (num2 != 0.0)
		{
			xMoon = (double)moonScatteredLuminance.x / num2;
			yMoon = (double)moonScatteredLuminance.y / num2;
		}
		YMoon = (double)moonScatteredLuminance.y * 0.001 * moonScale;
		YZenith = (double)sunScatteredLuminance.y * 0.001 + NightSkyLuminance();
		H *= SilverLining.unitScale;
		isothermalEffect = Math.Exp(0.0 - altitude / H);
		if (isothermalEffect < 0.0)
		{
			isothermalEffect = 0.0;
		}
		if (isothermalEffect > 1.0)
		{
			isothermalEffect = 1.0;
		}
		YZenith *= isothermalEffect;
		YMoon *= isothermalEffect;
	}

	private void UpdatePerezCoefficients()
	{
		if (T != lastT)
		{
			lastT = T;
			AY = (0.1787 * T - 1.463) * aScale;
			BY = (-0.3554 * T + 0.4275) * bScale;
			CY = (-0.0227 * T + 5.3251) * cScale;
			DY = (0.1206 * T - 2.5771) * dScale;
			EY = (-0.067 * T + 0.3702) * eScale;
			Ax = -0.0193 * T - 0.2592;
			Bx = -0.0665 * T + 0.0008;
			Cx = -0.0004 * T + 0.2125;
			Dx = -0.0641 * T - 0.8989;
			Ex = -0.0033 * T + 0.0452;
			Ay = -0.0167 * T - 0.2608;
			By = -0.095 * T + 0.0092;
			Cy = -0.0079 * T + 0.2102;
			Dy = -0.0441 * T - 1.6537;
			Ey = -0.0109 * T + 0.0529;
		}
	}

	private void ComputeSun(double altitude)
	{
		Vector3 sunPositionHorizon = ephemeris.GetSunPositionHorizon();
		sunPositionHorizon.Normalize();
		double num = sunPositionHorizon.y;
		if (lastSunT == T && lastSunZenith == num)
		{
			return;
		}
		lastSunT = T;
		lastSunZenith = num;
		lightingChanged = true;
		if (num > 0.0)
		{
			double num2 = Math.Acos(num);
			SilverLiningSpectrum directIrradiance = new SilverLiningSpectrum();
			SilverLiningSpectrum scatteredIrradiance = new SilverLiningSpectrum();
			sunSpectrum.ApplyAtmosphericTransmittance(num2, num, T, altitude, ref directIrradiance, ref scatteredIrradiance);
			sunTransmittedLuminance = directIrradiance.ToXYZ();
			sunScatteredLuminance = scatteredIrradiance.ToXYZ();
			double num3 = num2 / (PI * 0.5);
			for (int i = 0; i < boostExp; i++)
			{
				num3 *= num3;
			}
			sunScatteredLuminance.x *= (float)(1.0 + num3 * (double)XBoost);
			sunScatteredLuminance.y *= (float)(1.0 + num3 * (double)YBoost);
			sunScatteredLuminance.z *= (float)(1.0 + num3 * (double)ZBoost);
		}
		else
		{
			float num4 = (float)DEGREES(Math.Asin(sunPositionHorizon.y));
			int num5 = (int)Math.Floor(num4);
			int num6 = (int)Math.Ceiling(num4);
			float num7 = num4 - (float)num5;
			float num8 = 0f;
			float num9 = 0f;
			if (num5 >= -16 && num6 >= -16)
			{
				num8 = twilightLuminance[num5];
				num9 = twilightLuminance[num6];
			}
			SilverLiningSpectrum directIrradiance2 = new SilverLiningSpectrum();
			SilverLiningSpectrum scatteredIrradiance2 = new SilverLiningSpectrum();
			double num10 = PI * 0.5 - 0.001;
			sunSpectrum.ApplyAtmosphericTransmittance(num10, Math.Cos(num10), T, altitude, ref directIrradiance2, ref scatteredIrradiance2);
			sunTransmittedLuminance = directIrradiance2.ToXYZ();
			sunScatteredLuminance = scatteredIrradiance2.ToXYZ();
			float num11 = (1f - num7) * num8 + num7 * num9;
			float num12 = 0.25f;
			float num13 = 0.25f;
			float num14 = 0.1f;
			float x = num12 * (num11 / num13);
			float z = (1f - num12 - num13) * (num11 / num13);
			num7 = (0f - num4) / 2f;
			if (num7 > 1f)
			{
				num7 = 1f;
			}
			if (num7 < 0f)
			{
				num7 = 0f;
			}
			num7 *= num7;
			sunTransmittedLuminance = sunTransmittedLuminance * num11 * num14 * num7 + sunTransmittedLuminance * (1f - num7);
			Vector3 vector = new Vector3(x, num11, z);
			sunScatteredLuminance = vector * num7 + sunScatteredLuminance * (1f - num7);
			sunScatteredLuminance.x *= 1f + XBoost;
			sunScatteredLuminance.y *= 1f + YBoost;
			sunScatteredLuminance.z *= 1f + ZBoost;
		}
		if (isOvercast)
		{
			sunTransmittedLuminance = sunTransmittedLuminance * (overcastBlend * overcastTransmission) + sunTransmittedLuminance * (1f - overcastBlend);
			sunScatteredLuminance = sunScatteredLuminance * (overcastBlend * overcastTransmission) + sunScatteredLuminance * (1f - overcastBlend);
		}
		sunTransmittedLuminance *= sunTransmissionScale;
		sunScatteredLuminance *= sunScatteredScale;
	}

	private void ComputeMoon(double altitude)
	{
		Vector3 moonPositionHorizon = ephemeris.GetMoonPositionHorizon();
		moonPositionHorizon.Normalize();
		double num = moonPositionHorizon.y;
		double num2 = Math.Acos(num);
		if (lastMoonT != T || lastMoonZenith != num2)
		{
			lastMoonT = T;
			lastMoonZenith = num2;
			lightingChanged = true;
			SilverLiningSpectrum directIrradiance = new SilverLiningLunarSpectrum();
			SilverLiningSpectrum scatteredIrradiance = new SilverLiningLunarSpectrum();
			lunarSpectrum.ApplyAtmosphericTransmittance(num2, num, T, altitude, ref directIrradiance, ref scatteredIrradiance);
			float num3 = MoonLuminance();
			moonTransmittedLuminance = directIrradiance.ToXYZ() * num3;
			moonScatteredLuminance = scatteredIrradiance.ToXYZ() * num3;
			if (isOvercast)
			{
				moonTransmittedLuminance = moonTransmittedLuminance * (overcastBlend * overcastTransmission) + moonTransmittedLuminance * (1f - overcastBlend);
				moonScatteredLuminance = moonScatteredLuminance * (overcastBlend * overcastTransmission) + moonScatteredLuminance * (1f - overcastBlend);
			}
			moonTransmittedLuminance *= moonTransmissionScale;
			moonScatteredLuminance *= moonScatteredScale;
		}
	}

	private float MoonLuminance()
	{
		float result = 0f;
		if (ephemeris != null)
		{
			Vector3 moonPositionHorizon = ephemeris.GetMoonPositionHorizon();
			moonPositionHorizon.Normalize();
			double num = DEGREES(Math.Asin(moonPositionHorizon.y));
			double num2 = ephemeris.GetMoonDistanceKM() * 1000.0;
			double num3 = 0.001;
			double num4 = ephemeris.GetMoonPhaseAngle();
			if (num4 < num3)
			{
				num4 = num3;
			}
			double num5;
			for (num5 = PI - num4; num5 < 0.0; num5 += 2.0 * PI)
			{
			}
			if (num5 < num3)
			{
				num5 = num3;
			}
			double num6 = 0.095 * (1.0 - Math.Sin(num4 / 2.0) * Math.Tan(num4 / 2.0) * Math.Log(1.0 / Math.Tan(num4 / 4.0)));
			double irradiance = 435022791840.0 / (3.0 * num2 * num2) * (num6 + 1905.0 * (1.0 - Math.Sin(num5 / 2.0) * Math.Tan(num5 / 2.0) * Math.Log(1.0 / Math.Tan(num5 / 4.0))));
			double num7 = NITS(irradiance);
			num7 *= 0.001;
			if (num < 0.0)
			{
				num7 *= Math.Exp(1.1247 * num);
			}
			result = (float)num7;
		}
		return result;
	}

	private void InitTwilightLuminances()
	{
		twilightLuminance = new Dictionary<int, float>();
		twilightLuminance[5] = 2200f / PIf;
		twilightLuminance[4] = 1800f / PIf;
		twilightLuminance[3] = 1400f / PIf;
		twilightLuminance[2] = 1200f / PIf;
		twilightLuminance[1] = 710f / PIf;
		twilightLuminance[0] = 400f / PIf;
		twilightLuminance[-1] = 190f / PIf;
		twilightLuminance[-2] = 77f / PIf;
		twilightLuminance[-3] = 28f / PIf;
		twilightLuminance[-4] = 9.4f / PIf;
		twilightLuminance[-5] = 2.9f / PIf;
		twilightLuminance[-6] = 0.9f / PIf;
		twilightLuminance[-7] = 0.3f / PIf;
		twilightLuminance[-8] = 0.11f / PIf;
		twilightLuminance[-9] = 0.047f / PIf;
		twilightLuminance[-10] = 0.021f / PIf;
		twilightLuminance[-11] = 0.0092f / PIf;
		twilightLuminance[-12] = 0.0031f / PIf;
		twilightLuminance[-13] = 0.0022f / PIf;
		twilightLuminance[-14] = 0.0019f / PIf;
		twilightLuminance[-15] = 0.0018f / PIf;
		twilightLuminance[-16] = 0.0018f / PIf;
	}

	public bool GetLightingChanged()
	{
		return lightingChanged;
	}

	public Vector3 GetSunOrMoonPosition()
	{
		if (ephemeris != null)
		{
			if (sunTransmittedLuminance.sqrMagnitude > moonTransmittedLuminance.sqrMagnitude)
			{
				return ephemeris.GetSunPositionHorizon();
			}
			return ephemeris.GetMoonPositionHorizon();
		}
		return new Vector3(0f, 1f, 0f);
	}

	public Color GetSunOrMoonColor()
	{
		Vector3 XYZ = sunTransmittedLuminance * (float)sunLuminanceScale;
		Vector3 XYZ2 = moonTransmittedLuminance * (float)moonLuminanceScale;
		SilverLiningLuminanceMapper.DurandMapperXYZ(ref XYZ);
		SilverLiningLuminanceMapper.DurandMapperXYZ(ref XYZ2);
		Vector3 v = (XYZ + XYZ2) * XYZ2RGB;
		ApplyGamma(ref v);
		return new Color(v.x, v.y, v.z);
	}
}
