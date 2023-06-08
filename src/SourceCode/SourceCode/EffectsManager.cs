using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class EffectsManager : KAMonoBase
{
	public List<PostProcessSettings> _Settings;

	private PostProcessingBehaviour mPostProcessingBehaviour;

	private void Awake()
	{
		if (!UtPlatform.IsImageEffectSupported())
		{
			Object.Destroy(this);
		}
	}

	private void Start()
	{
		mPostProcessingBehaviour = base.gameObject.AddComponent<PostProcessingBehaviour>();
		LoadProfile();
	}

	public void LoadProfile()
	{
		if (mPostProcessingBehaviour == null)
		{
			return;
		}
		if (ProductConfig.GetEffectQuality().Equals(Quality.Off.ToString()))
		{
			mPostProcessingBehaviour.profile = null;
			mPostProcessingBehaviour.enabled = false;
			return;
		}
		PostProcessingProfile postProcessingProfile = null;
		if (_Settings != null && _Settings.Count > 0)
		{
			postProcessingProfile = EffectSettings.pInstance.GetProfile(_Settings);
		}
		if (postProcessingProfile == null)
		{
			postProcessingProfile = EffectSettings.pInstance.GetSavedProfile();
		}
		if (postProcessingProfile != null && (mPostProcessingBehaviour.profile == null || !postProcessingProfile.Equals(mPostProcessingBehaviour.profile)))
		{
			if (!mPostProcessingBehaviour.enabled)
			{
				mPostProcessingBehaviour.enabled = true;
			}
			mPostProcessingBehaviour.profile = postProcessingProfile;
		}
		else if (postProcessingProfile == null && mPostProcessingBehaviour.profile != null)
		{
			mPostProcessingBehaviour.profile = null;
			mPostProcessingBehaviour.enabled = false;
		}
	}
}
