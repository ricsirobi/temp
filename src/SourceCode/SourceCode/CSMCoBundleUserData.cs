public class CSMCoBundleUserData : CoBundleItemData
{
	public ContextData _Data;

	public CSMCoBundleUserData(ContextData inData, string iconTex, string rVo)
	{
		_Data = inData;
		_ItemTextureData.Init(iconTex);
		_ItemRVOData.Init(rVo);
	}

	public override void OnAllDownloaded()
	{
		ShowLoadingItem(inShow: false);
		base.OnAllDownloaded();
		KAWidget item = GetItem();
		if (_ItemTextureData._Texture != null && item != null && item.pBackground != null)
		{
			item.pBackground.gameObject.SetActive(value: false);
		}
	}

	public override void ShowLoadingItem(bool inShow)
	{
		base.ShowLoadingItem(inShow);
		KAWidget item = GetItem();
		if (!(item == null))
		{
			KAWidget kAWidget = item.FindChildItem("Loading");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inShow);
			}
		}
	}
}
