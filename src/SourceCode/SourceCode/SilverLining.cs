using System;
using UnityEngine;

public class SilverLining : KAMonoBase
{
	private float todSliderValue;

	public int year = 2010;

	public int month = 7;

	public int day = 1;

	public int hour = 12;

	public int minutes;

	public double seconds;

	public bool daylightSavingsTime = true;

	public double timeZone = -8.0;

	public double latitude = 42.0;

	public double longitude = -122.0;

	public double altitude;

	public bool applyFogToSkyDome;

	public bool applyFogToClouds;

	public bool hasCumulusClouds = true;

	public float cumulusCoverage = 0.5f;

	public Vector3 cumulusPosition = new Vector3(0f, 3000f, 0f);

	public Vector3 cumulusDimensions = new Vector3(80000f, 200f, 80000f);

	public bool wrapCumulusClouds = true;

	public bool cumulusEllipseBounds;

	public bool hasCirrusClouds = true;

	public Vector3 cirrusPosition = new Vector3(0f, 5000f, 0f);

	public float cirrusSize = 80000f;

	public bool hasStratusClouds;

	public Vector3 stratusPosition = new Vector3(0f, 2000f, 0f);

	public float stratusSize = 80000f;

	public float stratusDensity = 0.8f;

	public float stratusThickness = 1000f;

	public Vector3 windVelocity = new Vector3(10f, 0f, 10f);

	public float ambientLightScale = 1f;

	public float sunLightScale = 1f;

	public float moonLightScale = 1f;

	private bool lastHasCumulusClouds;

	private bool lastHasCirrusClouds;

	private bool lastHasStratusClouds;

	private float lastCumulusCoverage;

	private float lastCirrusSize;

	private float lastStratusSize;

	private float lastStratusDensity;

	private float lastStratusThickness;

	private Vector3 lastCumulusPosition;

	private Vector3 lastCumulusDimensions;

	private Vector3 lastCirrusPosition;

	private Vector3 lastStratusPosition;

	private bool lastApplyFogToClouds;

	public static double unitScale = 1.0;

	private SilverLiningTime time;

	private SilverLiningLocation location;

	private SilverLiningSky sky;

	private SilverLiningCumulusCloudLayer cumulusClouds;

	private SilverLiningCirrusCloud cirrusCloud;

	private SilverLiningStratusCloud stratusCloud;

	private bool lastFog;

	private Color lastFogColor;

	private float lastFogDensity;

	private Color lastAmbientColor;

	private GameObject cumulusCloudTransform;

	private GameObject cirrusCloudTransform;

	private GameObject stratusCloudTransform;

	private Vector3 stratusWindPosition;

	private void Start()
	{
		time = new SilverLiningTime();
		sky = new SilverLiningSky();
		location = new SilverLiningLocation();
		lastFogColor = new Color(0f, 0f, 0f, 0f);
		lastFogDensity = -1f;
		todSliderValue = (float)hour + (float)minutes / 60f;
		sky.Start();
		CreateClouds();
		float farClipPlane = (float)Math.Max(100000.0 * unitScale, Camera.main.farClipPlane);
		Camera.main.farClipPlane = farClipPlane;
		cumulusCloudTransform = GameObject.Find("CumulusClouds");
		cirrusCloudTransform = GameObject.Find("CirrusClouds");
		stratusCloudTransform = GameObject.Find("StratusClouds");
	}

	private void Update()
	{
		bool flag = false;
		if (stratusCloud != null)
		{
			flag = stratusCloud.IsInsideCloud();
		}
		MeshFilter component = GetComponent<MeshFilter>();
		if (component != null && Camera.main != null)
		{
			component.mesh.bounds = new Bounds(Camera.main.transform.position, Vector3.one * 100000f);
		}
		time.SetDate(year, month, day);
		time.SetTime(hour, minutes, seconds, timeZone, daylightSavingsTime);
		location.SetLatitude(latitude);
		location.SetLongitude(longitude);
		location.SetAltitude(altitude);
		sky.ambientLightScale = ambientLightScale;
		sky.sunLightScale = sunLightScale;
		sky.moonScale = moonLightScale;
		sky.Update(time, location, base.renderer, IsOvercast(), applyFogToSkyDome || flag);
		if (hasCumulusClouds && cumulusClouds != null)
		{
			cumulusClouds.WrapAndUpdateClouds(wrapCumulusClouds, cumulusEllipseBounds, sky.GetSunOrMoonColor(), sky.GetSunOrMoonPosition());
			if (NeedsLightingUpdate())
			{
				cumulusClouds.UpdateLighting(sky.GetSunOrMoonColor(), sky.GetSunOrMoonPosition(), base.renderer);
				cumulusClouds.Update();
			}
			if (NeedsFogUpdate())
			{
				cumulusClouds.UpdateFog(applyFogToClouds || flag);
			}
		}
		if (hasStratusClouds && stratusCloud != null)
		{
			stratusCloud.Update(sky, stratusDensity, stratusWindPosition, applyFogToClouds || flag);
		}
		Vector3 vector = Time.deltaTime * windVelocity;
		cumulusCloudTransform.transform.position += vector;
		cirrusCloudTransform.transform.position += vector;
		stratusWindPosition += vector;
		CheckRecreateClouds();
	}

	private void CheckRecreateClouds()
	{
		if (hasCumulusClouds != lastHasCumulusClouds || cumulusDimensions != lastCumulusDimensions || cumulusPosition != lastCumulusPosition || cumulusCoverage != lastCumulusCoverage || applyFogToClouds != lastApplyFogToClouds)
		{
			if (cumulusClouds != null)
			{
				cumulusClouds.DestroyClouds();
			}
			cumulusClouds = null;
			if (hasCumulusClouds)
			{
				cumulusClouds = new SilverLiningCumulusCloudLayer(cumulusDimensions, cumulusPosition, cumulusCoverage);
				cumulusClouds.UpdateLighting(sky.GetSunOrMoonColor(), sky.GetSunOrMoonPosition(), base.renderer);
				cumulusClouds.UpdateFog(applyFogToClouds);
				cumulusClouds.Update();
			}
			lastHasCumulusClouds = hasCumulusClouds;
			lastCumulusDimensions = cumulusDimensions;
			lastCumulusPosition = cumulusPosition;
			lastCumulusCoverage = cumulusCoverage;
			lastApplyFogToClouds = applyFogToClouds;
		}
		if (hasStratusClouds != lastHasStratusClouds || stratusPosition != lastStratusPosition || stratusSize != lastStratusSize || stratusThickness != lastStratusThickness)
		{
			if (stratusCloud != null)
			{
				stratusCloud.Destroy();
			}
			stratusCloud = null;
			if (hasStratusClouds)
			{
				stratusCloud = new SilverLiningStratusCloud(stratusPosition, stratusSize, stratusThickness);
				stratusWindPosition = stratusPosition;
			}
			lastHasStratusClouds = hasStratusClouds;
			lastStratusPosition = stratusPosition;
			lastStratusSize = stratusSize;
			lastStratusThickness = stratusThickness;
		}
		if (hasCirrusClouds != lastHasCirrusClouds || cirrusPosition != lastCirrusPosition || cirrusSize != lastCirrusSize)
		{
			if (cirrusCloud != null)
			{
				cirrusCloud.Destroy();
			}
			cirrusCloud = null;
			if (hasCirrusClouds)
			{
				cirrusCloud = new SilverLiningCirrusCloud(cirrusPosition, cirrusSize);
			}
			lastHasCirrusClouds = hasCirrusClouds;
			lastCirrusPosition = cirrusPosition;
			lastCirrusSize = cirrusSize;
		}
	}

	private void CreateClouds()
	{
		if (hasCumulusClouds)
		{
			cumulusClouds = new SilverLiningCumulusCloudLayer(cumulusDimensions, cumulusPosition, cumulusCoverage);
		}
		if (hasCirrusClouds)
		{
			cirrusCloud = new SilverLiningCirrusCloud(cirrusPosition, cirrusSize);
		}
		if (hasStratusClouds)
		{
			stratusCloud = new SilverLiningStratusCloud(stratusPosition, stratusSize, stratusThickness);
			stratusWindPosition = stratusPosition;
		}
		lastHasCumulusClouds = hasCumulusClouds;
		lastCumulusDimensions = cumulusDimensions;
		lastCumulusPosition = cumulusPosition;
		lastCumulusCoverage = cumulusCoverage;
		lastHasCirrusClouds = hasCirrusClouds;
		lastCirrusPosition = cirrusPosition;
		lastCirrusSize = cirrusSize;
		lastHasStratusClouds = hasStratusClouds;
		lastStratusPosition = stratusPosition;
		lastStratusSize = stratusSize;
		lastStratusThickness = stratusThickness;
		lastApplyFogToClouds = applyFogToClouds;
	}

	private bool NeedsFogUpdate()
	{
		bool result = false;
		if (RenderSettings.fog != lastFog)
		{
			lastFog = RenderSettings.fog;
			result = true;
		}
		if (RenderSettings.fogColor != lastFogColor)
		{
			lastFogColor = RenderSettings.fogColor;
			result = true;
		}
		if (RenderSettings.fogDensity != lastFogDensity)
		{
			lastFogDensity = RenderSettings.fogDensity;
			result = true;
		}
		return result;
	}

	private bool NeedsLightingUpdate()
	{
		bool result = false;
		if (sky.GetLightingChanged())
		{
			result = true;
		}
		if (stratusDensity != lastStratusDensity)
		{
			lastStratusDensity = stratusDensity;
			result = true;
		}
		if (RenderSettings.ambientLight != lastAmbientColor)
		{
			lastAmbientColor = RenderSettings.ambientLight;
			result = true;
		}
		return result;
	}

	public bool IsOvercast()
	{
		if (hasStratusClouds && (double)stratusDensity >= 1.0 && Camera.main.transform.position.y < stratusPosition.y)
		{
			return true;
		}
		return false;
	}
}
