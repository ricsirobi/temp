using System;
using System.Collections.Generic;
using UnityEngine;

public class AvAvatarSplashEffect : KAMonoBase
{
	[Serializable]
	public class ShaderSwapForEffect
	{
		public List<Shader> _Normal;

		public Shader _SplashEffect;
	}

	private class ShaderSwapInfo
	{
		public Shader mNormal;

		public Shader mSwapped;
	}

	public Texture2D _SplashEffect;

	public List<ShaderSwapForEffect> _ShadersSwapList;

	private Dictionary<string, ShaderSwapInfo> mPartShaderSwapInfo;

	private static AvAvatarSplashEffect mInstance;

	private bool mUpdateEffect;

	public static AvAvatarSplashEffect pInstance => mInstance;

	public bool pUpdateEffect
	{
		get
		{
			return mUpdateEffect;
		}
		set
		{
			mUpdateEffect = value;
		}
	}

	private void Awake()
	{
		if (mInstance != null)
		{
			UnityEngine.Object.Destroy(mInstance.gameObject);
		}
		mInstance = this;
		if (base.transform.parent != null)
		{
			base.transform.parent = null;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
		mPartShaderSwapInfo = new Dictionary<string, ShaderSwapInfo>();
	}

	private void OnEnable()
	{
		EnableEffect(inEnable: true);
	}

	private void OnDisable()
	{
		EnableEffect(inEnable: false);
	}

	private void EnableEffect(bool inEnable)
	{
		if (inEnable)
		{
			SetEffect();
			UiToolbar.AvatarUpdated += SetEffect;
		}
		else
		{
			UpdateAvatarWithEffect(inAddEffect: false, AvAvatar.mTransform);
			UiToolbar.AvatarUpdated -= SetEffect;
		}
	}

	private void OnDestroy()
	{
		if (mInstance == this)
		{
			EnableEffect(inEnable: false);
			mInstance = null;
		}
	}

	public void UpdateAvatarWithEffect(bool inAddEffect, Transform inAvatarTransform)
	{
		if (AvAvatar.pObject != null)
		{
			GameObject partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_HAIR, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_HAIR);
			}
			partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_HEAD, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_HEAD);
			}
			partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_TOP, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_TOP);
			}
			partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_LEGS, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_LEGS);
			}
			partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT);
			}
			partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT);
			}
			partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT);
			}
			partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT);
			}
			partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT);
			}
			partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT);
			}
			partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT);
			}
			partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT);
			}
			partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_HAT, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_HAT);
			}
			partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_FACEMASK, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_FACEMASK);
			}
			partObject = AvatarData.GetPartObject(inAvatarTransform, AvatarData.pPartSettings.AVATAR_PART_BACK, 0);
			if (partObject != null)
			{
				UpdateSplash(partObject.transform, inAddEffect, AvatarData.pPartSettings.AVATAR_PART_BACK, isDecal2: true);
			}
			mUpdateEffect = true;
		}
	}

	private void UpdateSplash(Transform go, bool addeffect, string partInfo, bool isDecal2 = false)
	{
		Renderer renderer = AvatarData.FindRenderer(go);
		if (!(renderer != null) || renderer.materials.Length == 0)
		{
			return;
		}
		Material material = renderer.materials[0];
		ShaderSwapInfo shaderSwapInfo = GetShaderSwapInfo(material, partInfo);
		if (shaderSwapInfo == null)
		{
			return;
		}
		if (addeffect)
		{
			material.shader = shaderSwapInfo.mSwapped;
			if (isDecal2)
			{
				material.SetTexture("_DecalTex2", _SplashEffect);
			}
			else
			{
				material.SetTexture("_DecalTex", _SplashEffect);
			}
			if (material.HasProperty("_DecalOpacity"))
			{
				material.SetFloat("_DecalOpacity", 1f);
			}
		}
		else
		{
			material.shader = shaderSwapInfo.mNormal;
		}
	}

	private ShaderSwapInfo GetShaderSwapInfo(Material mat, string partInfo)
	{
		ShaderSwapInfo result = null;
		if ((bool)mat)
		{
			Shader shader = mat.shader;
			if (mPartShaderSwapInfo != null)
			{
				if (shader.name.Contains("Splash"))
				{
					return mPartShaderSwapInfo[partInfo];
				}
				for (int i = 0; i < _ShadersSwapList.Count; i++)
				{
					for (int j = 0; j < _ShadersSwapList[i]._Normal.Count; j++)
					{
						if (shader.name.Equals(_ShadersSwapList[i]._Normal[j].name))
						{
							if (mPartShaderSwapInfo.ContainsKey(partInfo))
							{
								result = mPartShaderSwapInfo[partInfo];
							}
							else
							{
								result = new ShaderSwapInfo();
								mPartShaderSwapInfo.Add(partInfo, result);
							}
							result.mNormal = shader;
							result.mSwapped = _ShadersSwapList[i]._SplashEffect;
							return result;
						}
					}
				}
			}
		}
		return result;
	}

	private void SetEffect()
	{
		mUpdateEffect = false;
	}

	private void Update()
	{
		if (!pUpdateEffect)
		{
			UpdateAvatarWithEffect(inAddEffect: true, AvAvatar.mTransform);
		}
	}
}
