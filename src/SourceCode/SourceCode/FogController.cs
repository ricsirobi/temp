using System.Collections;
using UnityEngine;

public class FogController : MonoBehaviour
{
	public Material _FogMaterial;

	public GameObject _FogPlane;

	public GameObject _DepthFogObject;

	public float _DepthFogParticleColorChangeTimer = 1f;

	public float _MinimumSpeedForDepthFX = 2f;

	public FogInfo _HorizontalFogInfo;

	public FogInfo _VerticalFogInfo;

	public Color _DefaultFogColor;

	[Tooltip("How fast should the plane fade in/out")]
	public float _FadeTimer = 3f;

	[Tooltip("What are the minimum and maximum flight speeds used for clamping.  Any flight speed under the min will have the default scroll effect, anything over the max will have maximum scroll effect.")]
	public MinMax _FlightSpeedClamps = new MinMax(3f, 20f);

	private Material mFogMaterial;

	private AvAvatarController mAvatarController;

	private Transform mFlyingBone;

	private Vector3 mFlyingBoneRotation = Vector3.zero;

	public bool _IsFogAlwaysActive;

	private Color mCurrentColor = Color.white;

	private ParticleSystem mDepthFogParticleSystem;

	private IEnumerator mFogCoroutine;

	private bool mIsFogActive;

	public const string _ShaderParamaterToOffset = "_MainTex";

	private void Start()
	{
		mFogMaterial = base.gameObject.GetComponent<Renderer>().sharedMaterial;
		mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		mFlyingBone = mAvatarController._FlyingBone;
		if (_FogPlane != null)
		{
			_FogMaterial.SetColor("_Color", _DefaultFogColor);
			_FogPlane.SetActive(_IsFogAlwaysActive);
			mIsFogActive = _IsFogAlwaysActive;
		}
		else
		{
			Debug.LogWarning("No fog plane found, please ensure there is fog planes in the scene if you want a fog effect!");
		}
		if (_DepthFogObject != null)
		{
			mDepthFogParticleSystem = _DepthFogObject.GetComponent<ParticleSystem>();
			_DepthFogObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (!mIsFogActive)
		{
			if (_DepthFogObject.activeSelf != mIsFogActive)
			{
				_DepthFogObject.SetActive(mIsFogActive);
			}
		}
		else
		{
			if (!(_FogPlane != null))
			{
				return;
			}
			mFlyingBoneRotation = mFlyingBone.rotation.eulerAngles;
			if (_DepthFogObject != null)
			{
				UpdateDepthFog();
			}
			if (!mAvatarController.IsFlyingOrGliding())
			{
				UpdateFog(_HorizontalFogInfo, FogState.IDLE);
				UpdateFog(_VerticalFogInfo, FogState.IDLE);
				return;
			}
			if (mFlyingBoneRotation.x >= 10f && mFlyingBoneRotation.x <= 180f && mAvatarController.pFlightSpeed >= _FlightSpeedClamps.Min)
			{
				UpdateFog(_VerticalFogInfo, FogState.DIVING);
			}
			else if (mFlyingBoneRotation.x > 180f && mFlyingBoneRotation.x <= 350f && mAvatarController.pFlightSpeed >= _FlightSpeedClamps.Min)
			{
				UpdateFog(_VerticalFogInfo, FogState.CLIMBING);
			}
			else
			{
				UpdateFog(_VerticalFogInfo, FogState.IDLE);
			}
			if (mFlyingBoneRotation.z >= 10f && mFlyingBoneRotation.z <= 180f)
			{
				UpdateFog(_HorizontalFogInfo, FogState.LEFT);
			}
			else if (mFlyingBoneRotation.z > 180f && mFlyingBoneRotation.z <= 350f)
			{
				UpdateFog(_HorizontalFogInfo, FogState.RIGHT);
			}
			else
			{
				UpdateFog(_HorizontalFogInfo, FogState.IDLE);
			}
		}
	}

	public void UpdateFog(FogInfo fogInfo, FogState newFogState)
	{
		if (newFogState != fogInfo.pFogState)
		{
			fogInfo.pFogState = newFogState;
			if (fogInfo.pFogCoroutine != null)
			{
				StopCoroutine(fogInfo.pFogCoroutine);
			}
			fogInfo.pFogCoroutine = ChangeStep(fogInfo._Step.Min, fogInfo._Step.Max, fogInfo._StepDuration, fogInfo);
			StartCoroutine(fogInfo.pFogCoroutine);
		}
		else if (newFogState == fogInfo.pFogState && newFogState != FogState.IDLE)
		{
			if (fogInfo.pFogCoroutine == null)
			{
				UpdateShader("_MainTex", DetermineStep(fogInfo));
			}
		}
		else if (fogInfo.pFogState != FogState.IDLE)
		{
			if (fogInfo.pFogCoroutine != null)
			{
				StopCoroutine(fogInfo.pFogCoroutine);
			}
			fogInfo.pFogCoroutine = ChangeStep(fogInfo._Step.Max, fogInfo._Step.Min, fogInfo._ResetDuration, fogInfo);
			StartCoroutine(fogInfo.pFogCoroutine);
			fogInfo.pFogState = newFogState;
		}
	}

	private void UpdateDepthFog()
	{
		if (!mAvatarController.IsFlyingOrGliding())
		{
			_DepthFogObject.SetActive(value: false);
		}
		else if (mAvatarController.pFlightSpeed >= _MinimumSpeedForDepthFX)
		{
			_DepthFogObject.SetActive(value: true);
		}
		else
		{
			_DepthFogObject.SetActive(value: false);
		}
	}

	public void SetFogColor(Color inFogColor)
	{
		mIsFogActive = true;
		if (mFogCoroutine != null)
		{
			StopCoroutine(mFogCoroutine);
		}
		if (_FogPlane != null)
		{
			_FogPlane.SetActive(value: true);
		}
		if (_DepthFogObject != null)
		{
			_DepthFogObject.SetActive(value: true);
		}
		if (inFogColor != mCurrentColor)
		{
			mFogCoroutine = RunFogFade(mCurrentColor, inFogColor);
			ParticleSystem.MainModule main = mDepthFogParticleSystem.main;
			main.startColor = inFogColor;
			StartCoroutine(mFogCoroutine);
		}
	}

	public void ResetFogColor()
	{
		if (mFogCoroutine != null)
		{
			StopCoroutine(mFogCoroutine);
		}
		if (_IsFogAlwaysActive)
		{
			mFogCoroutine = RunFogFade(mCurrentColor, _DefaultFogColor);
		}
		else
		{
			mFogCoroutine = RunFogFade(mCurrentColor, Color.black);
		}
		ParticleSystem.MainModule main = mDepthFogParticleSystem.main;
		main.startColor = _DefaultFogColor;
		StartCoroutine(mFogCoroutine);
	}

	private IEnumerator RunFogFade(Color startColor, Color endColor)
	{
		float lerpPercentage = 0f;
		float currentTimer = 0f;
		while (lerpPercentage <= 1f)
		{
			lerpPercentage = currentTimer / _FadeTimer;
			mCurrentColor = Color.Lerp(startColor, endColor, lerpPercentage);
			_FogMaterial.SetColor("_Color", mCurrentColor);
			currentTimer += Time.deltaTime;
			yield return null;
		}
		if (endColor == Color.black)
		{
			mIsFogActive = false;
		}
		mFogCoroutine = null;
	}

	private IEnumerator ChangeStep(float startStep, float endStep, float timer, FogInfo fogInfo)
	{
		float lerpPercentage = 0f;
		float currentTimer = 0f;
		while (lerpPercentage <= 1f)
		{
			lerpPercentage = currentTimer / timer;
			currentTimer += Time.deltaTime;
			UpdateShader("_MainTex", DetermineStep(fogInfo));
			yield return null;
		}
		fogInfo.pFogCoroutine = null;
	}

	private void UpdateShader(string shaderParameter, Vector2 speed)
	{
		mFogMaterial.SetTextureOffset(shaderParameter, speed);
	}

	public Vector2 DetermineStep(FogInfo fogInfo)
	{
		Vector2 textureOffset = mFogMaterial.GetTextureOffset("_MainTex");
		Vector2 result = new Vector2(textureOffset.x, textureOffset.y);
		if (fogInfo == _HorizontalFogInfo)
		{
			switch (fogInfo.pFogState)
			{
			case FogState.LEFT:
				result.x += fogInfo._Step.Max;
				break;
			case FogState.RIGHT:
				result.x += fogInfo._Step.Max * -1f;
				break;
			}
		}
		else
		{
			switch (fogInfo.pFogState)
			{
			case FogState.DIVING:
				result.y += fogInfo._Step.Max;
				break;
			case FogState.CLIMBING:
				result.y += fogInfo._Step.Max * -1f;
				break;
			}
		}
		return result;
	}
}
