public class KAStoreInventoryItemData : CoBundleItemData
{
	public int _Quantity;

	public KAUIStoreInvMenuBase _Menu;

	public int _WH;

	public int _ItemID = -1;

	public KAStoreInventoryItemData(KAUIStoreInvMenuBase menu, string iconTex, string rVO, int q, int wh, int itemID)
	{
		_Menu = menu;
		_ItemTextureData.Init(iconTex);
		_ItemRVOData.Init(rVO);
		_ItemID = itemID;
		_Quantity = q;
		_WH = wh;
	}

	public void CopyData(KAStoreInventoryItemData s)
	{
		CopyData((CoBundleItemData)s);
		_Quantity = s._Quantity;
		_Menu = s._Menu;
	}

	public override void OnAllDownloaded()
	{
		base.OnAllDownloaded();
		_ = _WH;
		_ = 0;
	}
}
