using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class EffectSettings : ScriptableObject
{
	public List<PostProcessSettings> _Settings;

	private PostProcessingProfile mSavedProfile;

	private static EffectSettings mInstance;

	public static EffectSettings pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = (EffectSettings)RsResourceManager.LoadAssetFromResources("EffectSettings.asset");
				if (mInstance == null)
				{
					mInstance = ScriptableObject.CreateInstance<EffectSettings>();
				}
			}
			return mInstance;
		}
	}

	public PostProcessingProfile GetSavedProfile()
	{
		if (mSavedProfile == null)
		{
			mSavedProfile = GetProfile(_Settings);
		}
		return mSavedProfile;
	}

	public void RefreshSavedProfile()
	{
		mSavedProfile = GetProfile(_Settings);
	}

	public PostProcessingProfile GetProfile(List<PostProcessSettings> postProcessingSetting)
	{
		string currentEffectQuality = ProductConfig.GetEffectQuality();
		if (!currentEffectQuality.Equals(Quality.Off.ToString()))
		{
			PostProcessSettings postProcessSettings = postProcessingSetting.Find((PostProcessSettings item) => item._PlatformType == UtPlatform.GetPlatformType());
			if (postProcessSettings != null)
			{
				PostProcessQualityMapping postProcessQualityMapping = postProcessSettings._QualityMapping.Find((PostProcessQualityMapping item) => currentEffectQuality.Contains(item._Quality.ToString()));
				if (postProcessQualityMapping == null)
				{
					postProcessQualityMapping = postProcessSettings._QualityMapping.Find((PostProcessQualityMapping item) => item._Quality == Quality.All);
				}
				if (postProcessQualityMapping != null)
				{
					return postProcessQualityMapping._PostProcessingProfile;
				}
			}
		}
		return null;
	}
}
