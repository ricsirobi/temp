using UnityEngine;

public class SilverLiningStratusCloud
{
	private GameObject cloudTop;

	private GameObject cloudBottom;

	private Shader fogShader;

	private Shader noFogShader;

	private Renderer topRenderer;

	private Renderer bottomRenderer;

	private float cloudSize;

	private float cloudThickness;

	private float scudThickness;

	private bool savedFog;

	private Color savedFogColor;

	private float savedFogDensity;

	private FogMode savedFogMode;

	private bool fogStateCaptured;

	private bool insideCloud;

	public SilverLiningStratusCloud(Vector3 position, float size, float thickness)
	{
		cloudSize = size;
		cloudThickness = thickness;
		fogStateCaptured = false;
		insideCloud = false;
		scudThickness = 100f;
		GameObject gameObject = GameObject.Find("StratusClouds");
		GameObject gameObject2 = GameObject.Find("StratusCloudPrefab");
		fogShader = Shader.Find("Custom/Stratus");
		noFogShader = Shader.Find("Custom/StratusNoFog");
		if (gameObject != null && gameObject2 != null)
		{
			cloudTop = Object.Instantiate(gameObject2, position, Quaternion.identity);
			cloudTop.transform.localScale = new Vector3(size * 0.1f, 1f, size * 0.1f);
			cloudTop.SetActive(value: true);
			topRenderer = cloudTop.GetComponent<MeshRenderer>();
			topRenderer.enabled = true;
			Quaternion rotation = Quaternion.AngleAxis(180f, new Vector3(1f, 0f, 0f));
			cloudBottom = Object.Instantiate(gameObject2, position, rotation);
			cloudBottom.transform.localScale = new Vector3(size * 0.1f, 1f, size * 0.1f);
			cloudBottom.SetActive(value: true);
			bottomRenderer = cloudBottom.GetComponent<MeshRenderer>();
			bottomRenderer.enabled = true;
			cloudTop.transform.parent = gameObject.transform;
			cloudBottom.transform.parent = gameObject.transform;
		}
	}

	public void Destroy()
	{
		if (cloudTop != null)
		{
			Object.Destroy(cloudTop);
		}
		if (cloudBottom != null)
		{
			Object.Destroy(cloudBottom);
		}
	}

	public void Update(SilverLiningSky sky, float pDensity, Vector3 center, bool doFog)
	{
		topRenderer.material.SetFloat("_Density", pDensity);
		bottomRenderer.material.SetFloat("_Density", pDensity);
		topRenderer.material.SetFloat("_CloudSize", cloudSize);
		bottomRenderer.material.SetFloat("_CloudSize", cloudSize);
		if (doFog)
		{
			if ((bool)fogShader)
			{
				topRenderer.material.shader = fogShader;
				bottomRenderer.material.shader = fogShader;
			}
		}
		else if ((bool)noFogShader)
		{
			topRenderer.material.shader = noFogShader;
			bottomRenderer.material.shader = noFogShader;
		}
		float num = cloudSize / 20f;
		Vector3 position = Camera.main.transform.position;
		Vector3 vector = default(Vector3);
		vector.x = 0f - position.x % num + center.x % num;
		vector.y = center.y - position.y;
		vector.z = 0f - position.z % num + center.z % num;
		cloudTop.transform.position = position + vector + new Vector3(0f, cloudThickness, 0f);
		cloudBottom.transform.position = position + vector;
		ApplyFog(center);
	}

	public bool IsInsideCloud()
	{
		return insideCloud;
	}

	private void ApplyFog(Vector3 cloudPos)
	{
		Vector3 position = Camera.main.transform.position;
		Renderer component = cloudTop.GetComponent<Renderer>();
		Renderer component2 = cloudBottom.GetComponent<Renderer>();
		if (position.y >= cloudPos.y - scudThickness && position.y <= cloudPos.y + cloudThickness + scudThickness)
		{
			insideCloud = true;
			if (!fogStateCaptured)
			{
				savedFog = RenderSettings.fog;
				savedFogColor = RenderSettings.fogColor;
				savedFogDensity = RenderSettings.fogDensity;
				savedFogMode = RenderSettings.fogMode;
				fogStateCaptured = true;
			}
			float num = 1f;
			if (position.y < cloudPos.y)
			{
				num = 1f - (cloudPos.y - position.y) / scudThickness;
			}
			else if (position.y > cloudPos.y + cloudThickness)
			{
				num = 1f - (position.y - (cloudPos.y + cloudThickness)) / scudThickness;
			}
			num = num * num * num * num;
			RenderSettings.fog = true;
			RenderSettings.fogColor = new Color(0.5f, 0.5f, 0.5f) * num + savedFogColor * (1f - num);
			float num2 = (savedFog ? savedFogDensity : 1E-20f);
			RenderSettings.fogDensity = 0.05f * num + num2 * (1f - num);
			RenderSettings.fogMode = FogMode.ExponentialSquared;
			if (num > 0.5f)
			{
				component2.enabled = false;
				component.enabled = false;
			}
			else
			{
				component2.enabled = true;
				component.enabled = true;
			}
		}
		else
		{
			insideCloud = false;
			component2.enabled = true;
			component.enabled = true;
			if (fogStateCaptured)
			{
				RenderSettings.fog = savedFog;
				RenderSettings.fogColor = savedFogColor;
				RenderSettings.fogDensity = savedFogDensity;
				RenderSettings.fogMode = savedFogMode;
				fogStateCaptured = false;
			}
		}
	}
}
