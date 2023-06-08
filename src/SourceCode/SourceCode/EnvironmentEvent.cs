using System;
using UnityEngine;

[Serializable]
public class EnvironmentEvent
{
	public float _ActiveTime = 20f;

	public Light[] _LightOff;

	public Light[] _LightOn;

	public Color _AmbientLight;

	private float[] mIntensity;

	private Color mAmbientLight;

	private float mStartTime;

	private Color mDefaultSkyColor;

	public Color _SkyColor;

	private bool mIsRunning;

	public void Start()
	{
		mIntensity = new float[_LightOff.Length];
		for (int i = 0; i < _LightOff.Length; i++)
		{
			mIntensity[i] = _LightOff[i].intensity;
		}
		mAmbientLight = RenderSettings.ambientLight;
		mDefaultSkyColor = RenderSettings.skybox.GetColor("_Tint");
		mStartTime = 0f - _ActiveTime;
		UtDebug.Log("mDefaultSkyColor = " + mDefaultSkyColor.ToString(), 200);
	}

	public void Activate()
	{
		UtDebug.Log("EnvironmentEvent Activate = " + DateTime.Now, 200);
		mStartTime = Time.realtimeSinceStartup;
		mIsRunning = true;
	}

	public void DeActivate()
	{
		UtDebug.Log("EnvironmentEvent DeActivate = " + DateTime.Now, 200);
		mStartTime = Time.realtimeSinceStartup;
		mIsRunning = false;
	}

	public void Update()
	{
		if (mIsRunning)
		{
			for (int i = 0; i < _LightOff.Length; i++)
			{
				_LightOff[i].intensity = Mathf.Lerp(mIntensity[i], 0f, (Time.realtimeSinceStartup - mStartTime) / _ActiveTime);
			}
			for (int j = 0; j < _LightOn.Length; j++)
			{
				_LightOn[j].intensity = Mathf.Lerp(0f, 1f, (Time.realtimeSinceStartup - mStartTime) / _ActiveTime);
			}
			RenderSettings.skybox.SetColor("_Tint", Color.Lerp(mDefaultSkyColor, _SkyColor, (Time.realtimeSinceStartup - mStartTime) / _ActiveTime));
			RenderSettings.ambientLight = Color.Lerp(mAmbientLight, _AmbientLight, (Time.realtimeSinceStartup - mStartTime) / _ActiveTime);
		}
		else
		{
			for (int k = 0; k < _LightOff.Length; k++)
			{
				_LightOff[k].intensity = Mathf.Lerp(0f, mIntensity[k], (Time.realtimeSinceStartup - mStartTime) / _ActiveTime);
			}
			for (int l = 0; l < _LightOn.Length; l++)
			{
				_LightOn[l].intensity = Mathf.Lerp(1f, 0f, (Time.realtimeSinceStartup - mStartTime) / _ActiveTime);
			}
			RenderSettings.skybox.SetColor("_Tint", Color.Lerp(_SkyColor, mDefaultSkyColor, (Time.realtimeSinceStartup - mStartTime) / _ActiveTime));
			RenderSettings.ambientLight = Color.Lerp(_AmbientLight, mAmbientLight, (Time.realtimeSinceStartup - mStartTime) / _ActiveTime);
		}
	}
}
