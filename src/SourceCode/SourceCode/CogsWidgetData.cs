using System.Collections.Generic;
using UnityEngine;

public class CogsWidgetData : CoBundleItemData
{
	public delegate void OnItemBundleReady();

	public ItemPrefabResData _ItemPrefabData = new ItemPrefabResData();

	public int _Quantity;

	public string _Asset;

	public Cog _Cog;

	private OnItemBundleReady mDownloadDoneCallBack;

	private static List<CogsWidgetData> CogsMenuList = new List<CogsWidgetData>();

	public CogsWidgetData(Cog inCog)
	{
		_Cog = inCog;
		if (!string.IsNullOrEmpty(inCog.Icon))
		{
			_ItemTextureData.Init(inCog.Icon);
		}
		_ItemPrefabData.Init(inCog.Asset);
		if (CogsMenuList == null)
		{
			CogsMenuList = new List<CogsWidgetData>();
		}
		CogsMenuList.Add(this);
	}

	public void LoadResource(OnItemBundleReady itemBundleReadyCallback)
	{
		mDownloadDoneCallBack = itemBundleReadyCallback;
		LoadResource();
		_ItemPrefabData.LoadBundle(this);
	}

	public void Unload()
	{
		if (_ItemPrefabData != null && !string.IsNullOrEmpty(_ItemPrefabData._ResBundleName))
		{
			RsResourceManager.Unload(_ItemPrefabData._ResBundleName);
		}
		_ItemPrefabData = null;
	}

	public static void UnloadAll()
	{
		if (CogsMenuList == null)
		{
			return;
		}
		foreach (CogsWidgetData cogsMenu in CogsMenuList)
		{
			cogsMenu?.Unload();
		}
		CogsMenuList.Clear();
		CogsMenuList = null;
	}

	public override void OnBundleReady(string inURL, AssetBundle bd)
	{
		_ItemPrefabData.OnBundleReady(inURL, bd);
		base.OnBundleReady(inURL, bd);
	}

	public override void OnAllDownloaded()
	{
		if (_ItemPrefabData.IsDataLoaded())
		{
			mDownloadDoneCallBack();
			base.OnAllDownloaded();
			KAWidget kAWidget = GetItem().FindChildItem("Loading");
			if (kAWidget != null)
			{
				GetItem().RemoveChildItem(kAWidget, destroy: true);
			}
		}
	}
}
