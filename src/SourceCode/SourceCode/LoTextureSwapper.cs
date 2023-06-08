using System;
using UnityEngine;

public class LoTextureSwapper : ObStatus
{
	public string _BundleName;

	public LoTextureMaterial[] _Materials;

	private static LoTextureSwapper mInstance;

	private AssetBundle mAssetBundle;

	public static LoTextureSwapper pInstance => mInstance;

	private void Start()
	{
		mInstance = this;
		_BundleName = RsResourceManager.FormatBundleURL(_BundleName);
		string key = _BundleName.Split('/')[^1];
		if (UtWWWAsync.pVersionList.TryGetValue(key, out var value))
		{
			AssetVersion.Variant closestVariant = value.GetClosestVariant(UtUtilities.GetLocaleLanguage());
			if (closestVariant != null && !closestVariant.Locale.Equals("en-US", StringComparison.OrdinalIgnoreCase))
			{
				RsResourceManager.Load(_BundleName, AssetBundleReady);
			}
			else
			{
				base.pIsReady = true;
			}
		}
		else
		{
			base.pIsReady = true;
		}
	}

	private void Swap()
	{
		if (_Materials != null)
		{
			LoTextureMaterial[] materials = _Materials;
			foreach (LoTextureMaterial loTextureMaterial in materials)
			{
				Texture texture = (Texture)mAssetBundle.LoadAsset(loTextureMaterial._TextureName);
				if ((bool)texture)
				{
					if (loTextureMaterial._Renderer == null)
					{
						UtDebug.LogWarning("Renderer not found for " + loTextureMaterial._TextureName);
					}
					else if (loTextureMaterial._Renderer.materials == null)
					{
						UtDebug.LogWarning("Renderer has no materials for " + loTextureMaterial._TextureName);
					}
					else if (loTextureMaterial._MaterialIndex >= loTextureMaterial._Renderer.materials.Length)
					{
						UtDebug.LogWarning("Material Index out of range for " + loTextureMaterial._TextureName);
					}
					else if (string.IsNullOrEmpty(loTextureMaterial._MaterialProperty))
					{
						loTextureMaterial._Renderer.materials[loTextureMaterial._MaterialIndex].mainTexture = texture;
					}
					else if (!loTextureMaterial._Renderer.materials[loTextureMaterial._MaterialIndex].HasProperty(loTextureMaterial._MaterialProperty))
					{
						Debug.LogError("Property not found! " + loTextureMaterial._MaterialProperty);
					}
					else
					{
						loTextureMaterial._Renderer.materials[loTextureMaterial._MaterialIndex].SetTexture(loTextureMaterial._MaterialProperty, texture);
					}
				}
			}
		}
		base.pIsReady = true;
	}

	public void AssetBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mAssetBundle = (AssetBundle)inFile;
			Swap();
			break;
		case RsResourceLoadEvent.ERROR:
			base.pIsReady = true;
			break;
		}
	}

	public static Texture GetLocalizedTexture(Texture inTexture)
	{
		if (mInstance != null && mInstance.mAssetBundle != null)
		{
			Texture texture = (Texture)mInstance.mAssetBundle.LoadAsset(inTexture.name);
			if (texture != null)
			{
				inTexture = texture;
			}
		}
		return inTexture;
	}
}
