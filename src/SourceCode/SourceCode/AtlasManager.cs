using System.Collections.Generic;
using UnityEngine;

public class AtlasManager
{
	private static List<ImageReferenceData> mAtlasData = new List<ImageReferenceData>();

	private static List<ImageReferenceData> mTextureData = new List<ImageReferenceData>();

	private const string mIdentifier = "--Ref";

	private static Transform mRootObject = null;

	public static List<ImageReferenceData> pTextureData => mTextureData;

	public static List<ImageReferenceData> pAtlasData => mAtlasData;

	public static void AddReference(UITexture uiTexture)
	{
	}

	public static void RemoveReference(UITexture uiTexture)
	{
	}

	public static void DumpTexlist()
	{
		for (int i = 0; i < mTextureData.Count; i++)
		{
			UtDebug.Log("Tex Cache : " + mTextureData[i].pTexture.name + " : " + mTextureData[i].pRefCount);
		}
	}

	public static UIAtlas AddReference(UIAtlas atlas)
	{
		if (atlas == null || !atlas.name.Contains("Atlas"))
		{
			return atlas;
		}
		if (mRootObject == null)
		{
			mRootObject = new GameObject("AtlasGroup").transform;
			Object.DontDestroyOnLoad(mRootObject.gameObject);
		}
		string name = atlas.name;
		name = name.Replace("--Ref", "");
		ImageReferenceData imageReferenceData = FindAtlasData(name);
		if (imageReferenceData == null)
		{
			imageReferenceData = new ImageReferenceData();
			imageReferenceData.pRefName = name;
			GameObject gameObject = Object.Instantiate(atlas.gameObject);
			gameObject.transform.parent = mRootObject;
			imageReferenceData.pAtlas = gameObject.GetComponent<UIAtlas>();
			imageReferenceData.pAtlas.name = name + "--Ref";
			UpdateMaterial(atlas, imageReferenceData);
			mAtlasData.Add(imageReferenceData);
		}
		else if (atlas.spriteMaterial != null && atlas.spriteMaterial.mainTexture != null && atlas.spriteMaterial.mainTexture != imageReferenceData.pAtlas.spriteMaterial.mainTexture)
		{
			DestroyTexture(atlas.spriteMaterial.mainTexture);
		}
		if (imageReferenceData.pAtlas.spriteMaterial == null || imageReferenceData.pAtlas.spriteMaterial.mainTexture == null)
		{
			UpdateMaterial(atlas, imageReferenceData);
		}
		imageReferenceData.pRefCount++;
		return imageReferenceData.pAtlas;
	}

	private static void UpdateMaterial(UIAtlas atlas, ImageReferenceData atlasData)
	{
		Material material = Object.Instantiate(atlas.spriteMaterial);
		atlasData.pAtlas.spriteMaterial = material;
		if (UtPlatform.IsEditor())
		{
			material.shader = Shader.Find(atlas.spriteMaterial.shader.name);
		}
		if (atlasData.pAtlas.spriteMaterial != null && atlasData.pAtlas.spriteMaterial.mainTexture != null && !atlasData.pAtlas.spriteMaterial.mainTexture.name.Contains("--Ref"))
		{
			atlasData.pAtlas.spriteMaterial.mainTexture.name += "--Ref";
		}
	}

	public static void RemoveReference(UIAtlas atlas)
	{
		if (atlas == null || !atlas.name.Contains("Atlas"))
		{
			return;
		}
		ImageReferenceData imageReferenceData = FindAtlasData(atlas.name.Replace("--Ref", ""));
		if (imageReferenceData != null)
		{
			imageReferenceData.pRefCount--;
			if (imageReferenceData.pRefCount > 0)
			{
				return;
			}
			imageReferenceData.pRefCount = 0;
			if (imageReferenceData.pAtlas != null)
			{
				if (imageReferenceData.pAtlas.spriteMaterial != null && imageReferenceData.pAtlas.spriteMaterial.mainTexture != null)
				{
					DestroyTexture(imageReferenceData.pAtlas.spriteMaterial.mainTexture);
				}
				Object.Destroy(imageReferenceData.pAtlas.gameObject);
			}
			imageReferenceData.pAtlas = null;
			mAtlasData.Remove(imageReferenceData);
		}
		else
		{
			UtDebug.Log("Wrong ref count " + atlas.name);
		}
	}

	private static ImageReferenceData FindAtlasData(string inAtlasName)
	{
		if (mAtlasData == null)
		{
			return null;
		}
		return mAtlasData.Find((ImageReferenceData inLoadedAtlas) => inLoadedAtlas != null && inLoadedAtlas.pRefName == inAtlasName);
	}

	private static ImageReferenceData FindTextureData(Texture inTex)
	{
		if (mTextureData == null && inTex == null)
		{
			return null;
		}
		return mTextureData.Find((ImageReferenceData inLoadedAtlas) => inLoadedAtlas != null && inLoadedAtlas.pTexture == inTex);
	}

	private static void DestroyTexture(Texture inTexture)
	{
		Resources.UnloadAsset(inTexture);
		inTexture = null;
	}
}
