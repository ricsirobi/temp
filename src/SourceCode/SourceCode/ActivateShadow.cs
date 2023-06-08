using UnityEngine;
using UnityEngine.Rendering;

public class ActivateShadow : MonoBehaviour
{
	private Projector mProjector;

	private FS_ShadowSimple mFSShadow;

	private string mCurrentShadow;

	private bool mForceProjectionShadow;

	public bool pForceProjectionShadow
	{
		set
		{
			mForceProjectionShadow = value;
			UpdateShadow();
		}
	}

	private void Start()
	{
		mProjector = GetComponent<Projector>();
		mFSShadow = GetComponent<FS_ShadowSimple>();
		UpdateShadow();
		if (UtPlatform.IsEditor())
		{
			ReAssignProjectorShader();
		}
	}

	private void Update()
	{
		if (!ProductConfig.GetShadowQuality().Equals(mCurrentShadow))
		{
			UpdateShadow();
		}
	}

	private void UpdateShadow()
	{
		mCurrentShadow = ProductConfig.GetShadowQuality();
		bool flag = SystemInfo.supportsShadows && SystemInfo.graphicsShaderLevel > 7;
		if (mFSShadow != null)
		{
			mFSShadow.enabled = !mForceProjectionShadow && mCurrentShadow.Equals(Quality.Low.ToString());
		}
		if (mProjector != null)
		{
			mProjector.enabled = mForceProjectionShadow || mCurrentShadow.Equals(Quality.Mid.ToString()) || (mCurrentShadow.Equals(Quality.High.ToString()) && !flag);
		}
		Renderer[] componentsInChildren = base.transform.parent.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer is ParticleSystemRenderer)
			{
				break;
			}
			renderer.shadowCastingMode = ((mCurrentShadow.Equals(Quality.High.ToString()) && flag) ? ShadowCastingMode.On : ShadowCastingMode.Off);
		}
	}

	private void ReAssignProjectorShader()
	{
		if (mProjector != null && mProjector.material != null)
		{
			Shader shader = Shader.Find(mProjector.material.shader.name);
			if (shader != null)
			{
				mProjector.material.shader = shader;
			}
		}
	}
}
