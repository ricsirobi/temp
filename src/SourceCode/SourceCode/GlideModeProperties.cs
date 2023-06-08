using System;
using UnityEngine;

public class GlideModeProperties : MonoBehaviour
{
	private static string[] mAnimList = new string[5] { "FlightSuitFly", "FlightSuitFlyFast", "FlightSuitFlySlow", "FlightSuitLaunch", "FlightSuitReadySet" };

	public PrtAttachment[] _GlideModePrtEmitterAttachments;

	public PrtAttachment[] _LaunchModePrtEmitterAttachments;

	public SnSound _GlideSound;

	public float _GlideModeMinRateOfDescent = -0.5f;

	public float _GlideModeMaxRateOfDescent = -6f;

	public float _GlideModeEarlyStartOffset = 2.5f;

	public float _LaunchModeGravityScale = 0.25f;

	private AvAvatarController mAAC;

	private Transform mLaunchTargetTransform;

	private bool mbLERPPlayerToTransform;

	private Vector3 mPlayerHorizontalVelocity = Vector3.zero;

	private float mPlayerAngularVelocity;

	public static string[] pAnimList => mAnimList;

	public void Start()
	{
		if (_LaunchModeGravityScale == 0f)
		{
			_LaunchModeGravityScale = 1f;
		}
		mAAC = GetPlayerObject().GetComponent<AvAvatarController>();
	}

	private void SetAvatarGlideMode()
	{
		OnLaunchModeEnded();
		mAAC.pGravityMultiplier = 1f;
		if (IsMainPlayer())
		{
			mAAC.pState = AvAvatarState.MOVING;
			mAAC.pSubState = AvAvatarSubState.GLIDING;
		}
	}

	public void LaunchPlayerToTransform(Transform targetTransform)
	{
		Vector3 position = GetPlayerObject().transform.position;
		mLaunchTargetTransform = targetTransform;
		Vector3 position2 = targetTransform.position;
		float num = LaunchPlayerToHeight(position2.y - position.y);
		num -= _GlideModeEarlyStartOffset;
		if (num != 0f)
		{
			float x = (position2.x - position.x) / num;
			float z = (position2.z - position.z) / num;
			Vector3 pVelocity = mAAC.pVelocity;
			pVelocity.x = x;
			pVelocity.z = z;
			mPlayerHorizontalVelocity = pVelocity;
			mPlayerHorizontalVelocity.y = 0f;
			float num2 = mLaunchTargetTransform.eulerAngles.y - mAAC.transform.eulerAngles.y;
			mPlayerAngularVelocity = num2 / num;
			mbLERPPlayerToTransform = true;
			if (IsMainPlayer())
			{
				AvAvatar.pInputEnabled = false;
			}
		}
	}

	public float LaunchPlayerToHeight(float launchHeight)
	{
		mAAC.pState = AvAvatarState.MOVING;
		mAAC.pSubState = AvAvatarSubState.NORMAL;
		mAAC.pGravityMultiplier = _LaunchModeGravityScale;
		float num = mAAC.pGravity * mAAC.pGravityMultiplier;
		launchHeight += 0.5f * (0f - num) * _GlideModeEarlyStartOffset * _GlideModeEarlyStartOffset;
		AvAvatar.PlayAnim(GetPlayerObject(), "FlightSuitLaunch");
		float num2 = (float)Math.Sqrt(2f * (0f - num) * launchHeight);
		mAAC.pVelocity = num2 * Vector3.up;
		float num3 = (float)Math.Sqrt(2f * launchHeight / (0f - num));
		Invoke("SetAvatarGlideMode", num3 - _GlideModeEarlyStartOffset);
		if (mAAC.pPetObject != null)
		{
			mAAC.pPetObject.OnAvatarLaunchModeStarted();
		}
		if (IsMainPlayer())
		{
			AvAvatar.pInputEnabled = true;
		}
		OnLaunchModeStarted();
		return num3;
	}

	private bool IsMainPlayer()
	{
		if (AvAvatar.IsCurrentPlayer(GetPlayerObject()))
		{
			return true;
		}
		return false;
	}

	private GameObject GetPlayerObject()
	{
		return base.transform.root.gameObject;
	}

	public void OnGlideModeStarted()
	{
		FindGlideModeAttachmentNodeTransforms();
		LoadAndAttachGlideModeParticles();
		PlayGlideSound();
	}

	public void OnLaunchModeStarted()
	{
		FindLaunchModeAttachmentNodeTransforms();
		LoadAndAttachLaunchModeParticles();
	}

	public void OnGlideModeEnded()
	{
		DestroyGlideModeEmitters();
		StopGlideSound();
	}

	public void OnLaunchModeEnded()
	{
		if (mLaunchTargetTransform != null)
		{
			mAAC.transform.rotation = mLaunchTargetTransform.rotation;
			mLaunchTargetTransform = null;
			mbLERPPlayerToTransform = false;
			if (IsMainPlayer())
			{
				AvAvatar.pInputEnabled = true;
			}
		}
		DestroyLaunchModeEmitters();
	}

	private void PlayGlideSound()
	{
		if (_GlideSound == null)
		{
			UtDebug.LogWarning("Glide Sound is null. Glide SFX will not play");
		}
		else if (_GlideSound._AudioClip == null)
		{
			UtDebug.LogWarning("Glide Sound Audio Clip is null. Glide SFX will not play");
		}
		else
		{
			_GlideSound.Play(inForce: true);
		}
	}

	private void StopGlideSound()
	{
	}

	private void DestroyGlideModeEmitters()
	{
		DestroyAllEmittersInAttachmentArray(_GlideModePrtEmitterAttachments);
	}

	private void DestroyLaunchModeEmitters()
	{
		DestroyAllEmittersInAttachmentArray(_LaunchModePrtEmitterAttachments);
	}

	private void DestroyAllEmittersInAttachmentArray(PrtAttachment[] _PrtEmitterAttachments)
	{
		foreach (PrtAttachment prtAttachment in _PrtEmitterAttachments)
		{
			if (prtAttachment.pParticleGO != null)
			{
				if (prtAttachment._DisableEmitInsteadOfDestroy)
				{
					prtAttachment.pParticleGO.transform.parent = null;
					prtAttachment.pParticleGO.GetComponent<ParticleSystem>().Stop();
				}
				else
				{
					UnityEngine.Object.Destroy(prtAttachment.pParticleGO);
				}
				prtAttachment.pParticleGO = null;
			}
		}
	}

	private void LoadAndAttachGlideModeParticles()
	{
		LoadAndAttachParticlesInAttachmentArray(_GlideModePrtEmitterAttachments);
	}

	private void LoadAndAttachLaunchModeParticles()
	{
		LoadAndAttachParticlesInAttachmentArray(_LaunchModePrtEmitterAttachments);
	}

	private void LoadAndAttachParticlesInAttachmentArray(PrtAttachment[] _PrtEmitterAttachments)
	{
		foreach (PrtAttachment prtAttachment in _PrtEmitterAttachments)
		{
			if (!(prtAttachment.pNodeTransform == null))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(prtAttachment._PrtPrefab);
				gameObject.transform.parent = prtAttachment.pNodeTransform;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localEulerAngles = Vector3.zero;
				gameObject.transform.localScale = Vector3.one;
				gameObject.SetActive(value: true);
				prtAttachment.pParticleGO = gameObject;
			}
		}
	}

	private void FindGlideModeAttachmentNodeTransforms()
	{
		FindAttachmentNodeTransforms(_GlideModePrtEmitterAttachments);
	}

	private void FindLaunchModeAttachmentNodeTransforms()
	{
		FindAttachmentNodeTransforms(_LaunchModePrtEmitterAttachments);
	}

	private void FindAttachmentNodeTransforms(PrtAttachment[] _PrtEmitterAttachments)
	{
		Transform root = base.transform.root;
		foreach (PrtAttachment prtAttachment in _PrtEmitterAttachments)
		{
			if (!(prtAttachment.pNodeTransform != null))
			{
				Transform transform = FindInTree(root, prtAttachment._NodeName);
				if (transform == null)
				{
					UtDebug.LogWarning("Unable to find node: " + prtAttachment._NodeName);
					UtDebug.LogWarning("Particle effect won't be attached");
				}
				prtAttachment.pNodeTransform = transform;
			}
		}
	}

	private void Update()
	{
		if (mbLERPPlayerToTransform)
		{
			Vector3 position = mAAC.transform.position;
			position += mPlayerHorizontalVelocity * Time.deltaTime;
			mAAC.transform.position = position;
			Vector3 eulerAngles = mAAC.transform.eulerAngles;
			eulerAngles.y += mPlayerAngularVelocity * Time.deltaTime;
			mAAC.transform.eulerAngles = eulerAngles;
		}
	}

	private Transform FindInTree(Transform root, string nodeToBeFound)
	{
		if (root == null)
		{
			return null;
		}
		if (root.gameObject.name == nodeToBeFound)
		{
			return root;
		}
		Transform transform = null;
		foreach (Transform item in root)
		{
			transform = FindInTree(item, nodeToBeFound);
			if (transform != null)
			{
				break;
			}
		}
		return transform;
	}
}
