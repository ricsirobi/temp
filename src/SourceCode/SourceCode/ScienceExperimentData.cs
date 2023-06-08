using System.Collections.Generic;
using UnityEngine;

public class ScienceExperimentData : CoBundleItemData
{
	public delegate void OnItemBundleReady();

	public ItemPrefabResData _ItemPrefabData = new ItemPrefabResData();

	public ItemTextureResData _TextureData = new ItemTextureResData();

	public LabItem mLabItem;

	public static List<ScienceExperimentData> ExperimentList = new List<ScienceExperimentData>();

	private OnItemBundleReady mDownloadDoneCallBack;

	public ScienceExperimentData(LabItem inLabItem)
	{
		mLabItem = inLabItem;
		if (!string.IsNullOrEmpty(inLabItem.Icon))
		{
			_ItemTextureData.Init(inLabItem.Icon);
		}
		_ItemPrefabData.Init(inLabItem.Prefab);
		_TextureData.Init("");
		_ItemRVOData.Init(null);
		if (ExperimentList == null)
		{
			ExperimentList = new List<ScienceExperimentData>();
		}
		ExperimentList.Add(this);
	}

	public void Unload()
	{
		if (mLabItem != null)
		{
			mLabItem.Unload();
		}
		if (_ItemPrefabData != null && !string.IsNullOrEmpty(_ItemPrefabData._ResBundleName))
		{
			RsResourceManager.Unload(_ItemPrefabData._ResBundleName);
		}
		_ItemPrefabData = null;
		if (_TextureData != null && !string.IsNullOrEmpty(_TextureData._ResBundleName))
		{
			RsResourceManager.Unload(_TextureData._ResBundleName);
		}
		_TextureData = null;
	}

	public static void UnloadAll()
	{
		if (ExperimentList == null)
		{
			return;
		}
		foreach (ScienceExperimentData experiment in ExperimentList)
		{
			experiment?.Unload();
		}
		ExperimentList.Clear();
		ExperimentList = null;
	}

	public void LoadResource(OnItemBundleReady itemBundleReadyCallback)
	{
		mDownloadDoneCallBack = itemBundleReadyCallback;
		LoadResource();
		_ItemPrefabData.LoadBundle(this);
		_TextureData.LoadBundle(this);
	}

	public override void OnBundleReady(string inURL, AssetBundle bd)
	{
		_ItemPrefabData.OnBundleReady(inURL, bd);
		_TextureData.OnBundleReady(inURL, bd);
		base.OnBundleReady(inURL, bd);
	}

	public override void OnAllDownloaded()
	{
		if (_ItemPrefabData.IsDataLoaded() && _TextureData.IsDataLoaded())
		{
			mDownloadDoneCallBack();
			base.OnAllDownloaded();
			KAWidget kAWidget = GetItem().FindChildItem("LoadingAnim");
			if (kAWidget != null)
			{
				GetItem().RemoveChildItem(kAWidget, destroy: true);
			}
		}
	}
}
