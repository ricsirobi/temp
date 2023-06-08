using System.Collections.Generic;

public class IAPItemWidgetUserData : StoreWidgetUserData
{
	public string _AppStoreID;

	public int _NoofCoins;

	public IAPItemData _IAPItemData;

	public List<PreviewAssetData> _PreviewAssetsData;

	public IAPItemWidgetUserData(string iconTex, string rVo, List<PreviewAssetData> previewAsset)
	{
		_PreviewAssetsData = previewAsset;
		_ItemTextureData.Init(iconTex);
		_ItemRVOData.Init(rVo);
	}
}
