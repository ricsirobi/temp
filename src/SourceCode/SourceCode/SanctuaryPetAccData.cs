using System;
using UnityEngine;

public class SanctuaryPetAccData : CoBundleItemData
{
	public ItemPrefabResData m3DData = new ItemPrefabResData();

	public RaisedPetAccessory mAccData;

	private Action<SanctuaryPetAccData> mAction;

	public GameObject mObj;

	public Texture2D mTex;

	public SanctuaryPetAccData(RaisedPetAccessory adata, Action<SanctuaryPetAccData> action)
	{
		mAccData = adata;
		mAction = action;
		_ItemTextureData.Init(RaisedPetData.GetAccessoryTexture(mAccData));
		_ItemRVOData.Init(null);
		m3DData.Init(RaisedPetData.GetAccessoryGeometry(mAccData));
	}

	public override void OnBundleReady(string inURL, AssetBundle bd)
	{
		m3DData.OnBundleReady(inURL, bd);
		base.OnBundleReady(inURL, bd);
	}

	public override void LoadResource()
	{
		m3DData.LoadBundle(this);
		base.LoadResource();
		if (mNumBundles == 0)
		{
			OnAllDownloaded();
		}
	}

	public override void OnAllDownloaded()
	{
		if (m3DData._Prefab != null)
		{
			mObj = UnityEngine.Object.Instantiate(m3DData._Prefab.gameObject, Vector3.zero, Quaternion.identity);
		}
		mTex = (Texture2D)_ItemTextureData._Texture;
		if (mObj != null && mTex != null)
		{
			UtUtilities.SetObjectTexture(mObj, 0, mTex);
		}
		if (mAction != null)
		{
			mAction(this);
		}
		pIsReady = true;
	}
}
