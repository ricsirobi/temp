using System.Collections.Generic;
using UnityEngine;

public class HeroDragonActionsFS : MonoBehaviour
{
	public GameObject _WaterObject;

	public string _BubbleParticleName = "PfPrtBubbleTrail";

	public Transform _SplashParticleFxTransform;

	public Transform _BubbleParticleFxTransform;

	public Vector3 _ParticleOffset = new Vector3(0f, 0f, 5f);

	public Transform _CameraWaterSpotsFx;

	public Vector3 _CameraWaterSpotsFxOffset = new Vector3(0f, 0f, 5f);

	private Transform mAvatarTransform;

	private float mPrevAvatarPosY;

	private GameObject mSplashParticleFxObject;

	private GameObject mBubbleParticleFxObject;

	private GameObject mCameraWaterSpots;

	private bool mInitialized;

	private List<GameObject> mDefaultTrailParticles;

	private List<GameObject> mBubbleTrailParticles;

	private void Start()
	{
		mAvatarTransform = AvAvatar.pObject.transform;
	}

	private void Update()
	{
		if (!(mAvatarTransform != null) || !(_WaterObject != null) || !(SanctuaryManager.pCurPetInstance != null))
		{
			return;
		}
		if (!mInitialized)
		{
			mDefaultTrailParticles = SanctuaryManager.pCurPetInstance.pTrailFX;
			List<Transform> outList = new List<Transform>();
			UtUtilities.FindChildTransformsContaining(SanctuaryManager.pCurPetInstance.gameObject, _BubbleParticleName, ref outList);
			mBubbleTrailParticles = new List<GameObject>();
			foreach (Transform item in outList)
			{
				mBubbleTrailParticles.Add(item.gameObject);
			}
			if (mAvatarTransform.position.y < _WaterObject.transform.position.y)
			{
				UpdateTrailRenderer(mDefaultTrailParticles, mBubbleTrailParticles);
				EnableFog(isEnable: true);
			}
			mInitialized = true;
		}
		if (mAvatarTransform.position.y > _WaterObject.transform.position.y && mPrevAvatarPosY < _WaterObject.transform.position.y)
		{
			StopParticle(ref mBubbleParticleFxObject);
			PlayParticle(ref mSplashParticleFxObject, _SplashParticleFxTransform, _ParticleOffset);
			PlayParticle(ref mCameraWaterSpots, _CameraWaterSpotsFx, _CameraWaterSpotsFxOffset);
			UpdateTrailRenderer(mBubbleTrailParticles, mDefaultTrailParticles);
			EnableFog(isEnable: false);
		}
		else if (mAvatarTransform.position.y < _WaterObject.transform.position.y && mPrevAvatarPosY > _WaterObject.transform.position.y)
		{
			StopParticle(ref mSplashParticleFxObject);
			StopParticle(ref mCameraWaterSpots);
			PlayParticle(ref mBubbleParticleFxObject, _BubbleParticleFxTransform, _ParticleOffset);
			UpdateTrailRenderer(mDefaultTrailParticles, mBubbleTrailParticles);
			EnableFog(isEnable: true);
		}
		mPrevAvatarPosY = mAvatarTransform.position.y;
	}

	private void UpdateTrailRenderer(List<GameObject> oldTrailRendererList, List<GameObject> newTrailRendererList)
	{
		foreach (GameObject oldTrailRenderer in oldTrailRendererList)
		{
			if (oldTrailRenderer != null)
			{
				oldTrailRenderer.SetActive(value: false);
			}
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.pTrailFX = newTrailRendererList;
		}
	}

	private void EnableFog(bool isEnable)
	{
		RenderSettings.fog = isEnable;
	}

	private void PlayParticle(ref GameObject inParticleInstance, Transform inTransform, Vector3 inOffset)
	{
		if (inParticleInstance == null && inTransform != null)
		{
			inParticleInstance = Object.Instantiate(inTransform.gameObject);
			inParticleInstance.transform.parent = Camera.main.transform;
			inParticleInstance.transform.localPosition = inOffset;
		}
		ParticleSystem component = inParticleInstance.GetComponent<ParticleSystem>();
		component.Simulate(0f);
		component.Play();
	}

	private void StopParticle(ref GameObject inParticleInstance)
	{
		if (inParticleInstance != null)
		{
			ParticleSystem component = inParticleInstance.GetComponent<ParticleSystem>();
			component.Stop(withChildren: true);
			component.Clear();
		}
	}

	private void OnDisable()
	{
		UpdateTrailRenderer(mBubbleTrailParticles, mDefaultTrailParticles);
	}
}
