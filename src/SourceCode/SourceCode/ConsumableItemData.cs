public class ConsumableItemData : CoBundleItemData
{
	public int _ItemID;

	public virtual void Init(ItemData item)
	{
		_ItemID = item.ItemID;
		_ItemTextureData.Init(item.IconName);
		_ItemRVOData.Init(null);
	}

	public override void OnAllDownloaded()
	{
		ShowLoadingItem(inShow: false);
		base.OnAllDownloaded();
		GetItem().name = "Consumable";
		UpdateQuantity();
	}

	public void UpdateQuantity()
	{
		GetItem().SetText(CommonInventoryData.pInstance.GetQuantity(_ItemID).ToString());
	}

	public void SetPixelPerfect(bool inPixelPerfect)
	{
		mMakePixelPerfect = inPixelPerfect;
	}

	public override void ShowLoadingItem(bool inShow)
	{
		base.ShowLoadingItem(inShow);
		if (GetItem() != null)
		{
			KAWidget kAWidget = GetItem().FindChildItem("Loading");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inShow);
			}
		}
	}
}
