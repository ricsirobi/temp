using UnityEngine;

public class GrFog : MonoBehaviour
{
	public Color _FogColor;

	public float _FogDensity;

	public Color _AmbientLight;

	private bool mOldFog;

	private Color mOldFogColor;

	private float mOldFogDensity;

	private Color mOldAmbientLight;

	public void Activate()
	{
		mOldFog = RenderSettings.fog;
		mOldFogColor = RenderSettings.fogColor;
		mOldFogDensity = RenderSettings.fogDensity;
		mOldAmbientLight = RenderSettings.ambientLight;
		RenderSettings.fog = true;
		RenderSettings.fogColor = _FogColor;
		RenderSettings.fogDensity = _FogDensity;
		RenderSettings.ambientLight = _AmbientLight;
	}

	public void Restore()
	{
		RenderSettings.fog = mOldFog;
		RenderSettings.fogColor = mOldFogColor;
		RenderSettings.fogDensity = mOldFogDensity;
		RenderSettings.ambientLight = mOldAmbientLight;
	}
}
