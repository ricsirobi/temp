using System;
using System.Collections.Generic;
using MK.Glow;
using UnityEngine;

public class GlowManager : ScriptableObject
{
	public delegate void GlowAppliedEventHandler();

	[Serializable]
	public class PetGlowData
	{
		public int _PetTypeID = -1;

		[Range(0f, 2.5f)]
		public float _GlowPower;

		public Color _GlowColor = Color.white;

		[Range(0f, 10f)]
		public float _GlowTextureStrength;

		public Color _GlowTextureColor = Color.white;
	}

	[Serializable]
	public class GlowData
	{
		public string _Name = "";

		public LocaleString _DisplayText;

		public Color _Color;

		public PetGlowData _DefaultPetGlowData;

		public List<PetGlowData> _PetGlowData;
	}

	[Serializable]
	public class MKGlowSetting
	{
		public bool _Enabled = true;

		[Range(2f, 10f)]
		public int _Samples = 2;

		[Range(1f, 10f)]
		public int _BlurIterations = 1;

		public bool _UseLens;

		public Texture _LensTexture;
	}

	[Serializable]
	public class BundleQuality
	{
		public string _BundleQuality;

		public List<GlowProfile> _GlowProfiles;
	}

	[Serializable]
	public class GlowProfile
	{
		public Quality _EffectQuality;

		public MKGlowSetting _MKGlowSetting;
	}

	[Serializable]
	public class PlatformGlowSetting
	{
		public UtPlatform.PlatformType _PlatformType;

		public List<BundleQuality> _BundleQuality;
	}

	[Serializable]
	public class CameraGlowSetting
	{
		[Range(0f, 2f)]
		public float _SpreadInner = 0.6f;

		[Range(0f, 1f)]
		public float _IntensityInner = 0.4f;

		[Range(0f, 2f)]
		public float _SpreadOuter = 0.7f;

		[Range(0f, 1f)]
		public float _IntensityOuter = 0.3f;

		[Range(0f, 1f)]
		public float _LensIntensity;
	}

	[Serializable]
	public class MKEnableSetting
	{
		public bool _EnableDuringGlow;

		public List<string> _CameraNames;

		public List<string> _ExcludedScenes;
	}

	private const string DetailTex = "_DetailTex";

	private const string GlowColor = "_MKGlowColor";

	private const string GlowPower = "_MKGlowPower";

	private const string GlowTexture = "_MKGlowTex";

	private const string GlowTextureColor = "_MKGlowTexColor";

	private const string GlowTextureStrength = "_MKGlowTexStrength";

	private const string OverrideRenderType = "DragonGlow";

	private const string DefaultRenderType = "Opaque";

	public List<GlowData> _GlowData;

	public MKEnableSetting _MKEnableSetting;

	public CameraGlowSetting _CameraGlowSetting;

	public int _GlowShaderRenderQueue = 3000;

	public int _NormalShaderRenderQueue = 2000;

	public List<PlatformGlowSetting> _PlatformGlowSetting;

	public LocaleString _GlowRemovedSystemMessageText = new LocaleString("[[REVIEW]]The glow on your dragon has run out!");

	private List<GameObject> mGlowObjects = new List<GameObject>();

	private GlowProfile mGlowProfile;

	private List<MKGlow> mMKCameras;

	private static GlowManager mInstance;

	public static GlowManager pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = (GlowManager)RsResourceManager.LoadAssetFromResources("GlowSetting.asset");
				if (mInstance == null)
				{
					mInstance = ScriptableObject.CreateInstance<GlowManager>();
				}
			}
			return mInstance;
		}
	}

	public static event GlowAppliedEventHandler OnGlowApplied;

	public void ApplyCameraSettings(MKGlow mkGlow)
	{
		if (!(mkGlow == null))
		{
			if (mMKCameras == null)
			{
				mMKCameras = new List<MKGlow>();
			}
			if (_MKEnableSetting._EnableDuringGlow && mMKCameras.Count == 0)
			{
				mGlowObjects.Clear();
			}
			if (!mMKCameras.Contains(mkGlow))
			{
				mMKCameras.Add(mkGlow);
			}
			if (mGlowProfile == null)
			{
				mGlowProfile = GetGlowProfile();
			}
			ApplyGlowProfile(mkGlow, mGlowProfile);
			if (mkGlow._ApplyGlobalGlowSetting)
			{
				mkGlow.BlurSpreadOuter = _CameraGlowSetting._SpreadOuter;
				mkGlow.GlowIntensityOuter = _CameraGlowSetting._IntensityOuter;
				mkGlow.BlurSpreadInner = _CameraGlowSetting._SpreadInner;
				mkGlow.GlowIntensityInner = _CameraGlowSetting._IntensityInner;
				mkGlow.LensIntensity = _CameraGlowSetting._LensIntensity;
			}
			CheckMKStatus();
		}
	}

	public void ApplyEffectSettings()
	{
		mGlowProfile = GetGlowProfile();
		if (mMKCameras == null || mMKCameras.Count == 0)
		{
			return;
		}
		foreach (MKGlow mMKCamera in mMKCameras)
		{
			ApplyGlowProfile(mMKCamera, mGlowProfile);
		}
		CheckMKStatus();
	}

	private void ApplyGlowProfile(MKGlow mkGlow, GlowProfile glowProfile)
	{
		if (glowProfile != null)
		{
			mkGlow.enabled = glowProfile._MKGlowSetting._Enabled;
			mkGlow.Samples = glowProfile._MKGlowSetting._Samples;
			mkGlow.BlurIterations = glowProfile._MKGlowSetting._BlurIterations;
			mkGlow.UseLens = glowProfile._MKGlowSetting._UseLens;
			mkGlow.LensTex = glowProfile._MKGlowSetting._LensTexture;
		}
	}

	private GlowProfile GetGlowProfile()
	{
		GlowProfile glowProfile = null;
		PlatformGlowSetting platformGlowSetting = _PlatformGlowSetting.Find((PlatformGlowSetting p) => p._PlatformType == UtPlatform.GetPlatformType());
		if (platformGlowSetting == null)
		{
			platformGlowSetting = _PlatformGlowSetting.Find((PlatformGlowSetting p) => p._PlatformType == UtPlatform.PlatformType.Unknown);
			UtDebug.Log("Glow Camera Apply Effect : Platform setting for " + UtPlatform.GetPlatformType().ToString() + " undefined in Glowsetting - Applying Default");
		}
		if (platformGlowSetting != null && platformGlowSetting._BundleQuality != null && platformGlowSetting._BundleQuality.Count > 0)
		{
			BundleQuality bundleQuality = platformGlowSetting._BundleQuality.Find((BundleQuality s) => s._BundleQuality == ProductConfig.GetBundleQuality());
			if (bundleQuality == null)
			{
				bundleQuality = platformGlowSetting._BundleQuality.Find((BundleQuality s) => s._BundleQuality == "Default");
			}
			if (bundleQuality != null)
			{
				glowProfile = bundleQuality._GlowProfiles.Find((GlowProfile s) => s._EffectQuality.ToString() == ProductConfig.GetEffectQuality());
				if (glowProfile == null)
				{
					glowProfile = bundleQuality._GlowProfiles.Find((GlowProfile s) => s._EffectQuality == Quality.All);
				}
			}
			else
			{
				UtDebug.Log("Glow Camera Apply Effect : Bundle Quality setting for " + ProductConfig.GetBundleQuality() + " undefined in Glowsetting");
			}
		}
		else
		{
			UtDebug.Log("Glow Camera Apply Effect : Platform setting undefined in Glowsetting");
		}
		return glowProfile;
	}

	public bool CanUpdateGlow(SanctuaryPet pet)
	{
		if (SanctuaryManager.pCurPetInstance == null)
		{
			return false;
		}
		if (AvAvatar.pToolbar != null && SanctuaryManager.pCurPetInstance == pet)
		{
			UiToolbar component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
			if (component != null && component.gameObject.activeInHierarchy)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveCameraReference(MKGlow mkCamera)
	{
		if (mMKCameras != null && mMKCameras.Count != 0)
		{
			mMKCameras.Remove(mkCamera);
		}
	}

	public void ApplyGlow(string color, int petID, GameObject obj, List<string> ignoreRenderers)
	{
		PetGlowData petGlowData = GetPetGlowData(color, petID);
		if (petGlowData != null)
		{
			ApplyGlowProperties(obj, petGlowData, ignoreRenderers);
		}
		else
		{
			UtDebug.Log("Apply Glow : pet " + petID + " info not found in glow setting");
		}
	}

	public void RemoveGlow(GameObject obj, List<string> ignoreRenderers, bool playFx = false, bool sendSysNotification = false)
	{
		ApplyGlowProperties(obj, null, ignoreRenderers);
		UpdateUser(obj.transform.position, playFx, sendSysNotification);
	}

	public void UpdateUser(Vector3 pos, bool playFx, bool sendSysNotification)
	{
		if (playFx)
		{
			GlowTransitionFx.PlayAt(pos, inPlaySound: false);
		}
		if (sendSysNotification)
		{
			UiChatHistory.AddSystemNotification(_GlowRemovedSystemMessageText.GetLocalizedString());
		}
	}

	public Color GetColor(string name)
	{
		if (_GlowData == null || _GlowData.Count == 0)
		{
			return Color.white;
		}
		GlowData glowData = _GlowData.Find((GlowData gd) => gd._Name.Equals(name));
		if (glowData == null)
		{
			UtDebug.Log("Glow : " + name + " info not found in glow setting");
			return Color.white;
		}
		return glowData._Color;
	}

	public string GetColorLocalizedText(string name)
	{
		if (_GlowData == null || _GlowData.Count == 0)
		{
			return "";
		}
		GlowData glowData = _GlowData.Find((GlowData gd) => gd._Name.Equals(name));
		if (glowData == null)
		{
			UtDebug.Log("Glow : " + name + " info not found in glow setting");
			return "";
		}
		return glowData._DisplayText.GetLocalizedString();
	}

	private void CheckMKStatus()
	{
		if (!_MKEnableSetting._EnableDuringGlow || !mGlowProfile._MKGlowSetting._Enabled)
		{
			return;
		}
		mGlowObjects.RemoveAll((GameObject x) => x == null);
		if (mMKCameras == null || mMKCameras.Count <= 0)
		{
			return;
		}
		foreach (MKGlow mMKCamera in mMKCameras)
		{
			if (_MKEnableSetting._CameraNames.Contains(mMKCamera.name) && !_MKEnableSetting._ExcludedScenes.Contains(RsResourceManager.pCurrentLevel))
			{
				mMKCamera.enabled = mGlowObjects.Count > 0;
			}
		}
	}

	public void AddRemoveGlowObject(GameObject obj, bool remove)
	{
		if (remove)
		{
			mGlowObjects.Remove(obj);
		}
		else if (!mGlowObjects.Contains(obj))
		{
			mGlowObjects.Add(obj);
		}
		CheckMKStatus();
	}

	private PetGlowData GetPetGlowData(string name, int petID)
	{
		if (_GlowData == null || _GlowData.Count == 0)
		{
			return null;
		}
		GlowData glowData = _GlowData.Find((GlowData gd) => gd._Name.Equals(name));
		if (glowData == null)
		{
			UtDebug.Log("Apply Glow : " + name + " info not found in glow setting");
			return null;
		}
		return glowData._PetGlowData.Find((PetGlowData x) => x._PetTypeID == petID) ?? glowData._DefaultPetGlowData;
	}

	private void ApplyGlowProperties(GameObject obj, PetGlowData petGlowData, List<string> ignoreRenderers)
	{
		SkinnedMeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
		if (componentsInChildren != null)
		{
			SkinnedMeshRenderer[] array = componentsInChildren;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
			{
				if (ignoreRenderers.Contains(skinnedMeshRenderer.name))
				{
					continue;
				}
				Material[] materials = skinnedMeshRenderer.materials;
				if (materials == null)
				{
					continue;
				}
				Material[] array2 = materials;
				foreach (Material material in array2)
				{
					if (material.HasProperty("_DetailTex") && material.HasProperty("_MKGlowTex"))
					{
						if (material.GetTexture("_MKGlowTex") == null)
						{
							material.SetTexture("_MKGlowTex", (petGlowData == null) ? null : material.GetTexture("_DetailTex"));
						}
						if (material.HasProperty("_MKGlowColor"))
						{
							material.SetColor("_MKGlowColor", petGlowData?._GlowColor ?? Color.white);
						}
						if (material.HasProperty("_MKGlowPower"))
						{
							material.SetFloat("_MKGlowPower", petGlowData?._GlowPower ?? 0f);
						}
						if (material.HasProperty("_MKGlowTexColor"))
						{
							material.SetColor("_MKGlowTexColor", petGlowData?._GlowTextureColor ?? Color.white);
						}
						if (material.HasProperty("_MKGlowTexStrength"))
						{
							material.SetFloat("_MKGlowTexStrength", petGlowData?._GlowTextureStrength ?? 0f);
						}
						material.SetOverrideTag("RenderType", (petGlowData == null) ? "Opaque" : "DragonGlow");
						material.renderQueue = ((petGlowData == null) ? _NormalShaderRenderQueue : _GlowShaderRenderQueue);
					}
					else
					{
						UtDebug.Log("Apply Glow " + obj.name + " Property _MKGlowTex not found on material " + material.name);
					}
				}
			}
			if (GlowManager.OnGlowApplied != null)
			{
				GlowManager.OnGlowApplied();
			}
			AddRemoveGlowObject(obj, petGlowData == null);
		}
		else
		{
			UtDebug.Log("Apply Glow " + obj.name + "No renderers found");
		}
	}

	public void ApplyMaterialParameters(GameObject obj, List<string> ignoreRenderers)
	{
		SkinnedMeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
		if (componentsInChildren == null)
		{
			return;
		}
		SkinnedMeshRenderer[] array = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			if (ignoreRenderers.Contains(skinnedMeshRenderer.name))
			{
				continue;
			}
			Material[] materials = skinnedMeshRenderer.materials;
			if (materials == null)
			{
				continue;
			}
			Material[] array2 = materials;
			foreach (Material material in array2)
			{
				if (material != null && material.HasProperty("_DetailTex") && material.HasProperty("_MKGlowTex"))
				{
					material.SetOverrideTag("RenderType", "DragonGlow");
					material.renderQueue = _GlowShaderRenderQueue;
				}
			}
		}
	}
}
