using UnityEngine;

public class MysteryBoxPrizeItemData : CoBundleItemData
{
	public ItemData _ItemData;

	public Texture _DefaultTexture;

	private GameObject mMsgObj;

	public MysteryBoxPrizeItemData(string iconTex, string rVo, ItemData item, GameObject msgObj)
	{
		_ItemTextureData.Init(iconTex);
		_ItemRVOData.Init(rVo);
		_ItemData = item;
		mMsgObj = msgObj;
	}

	public void CopyData(KAStoreItemData s)
	{
		_ItemData = s._ItemData;
	}

	public override void OnAllDownloaded()
	{
		KAWidget item = GetItem();
		if (!(item == null))
		{
			ShowLoadingItem(inShow: false);
			base.OnAllDownloaded();
			item.SetState(KAUIState.NOT_INTERACTIVE);
			mMsgObj.SendMessage("ItemReady", SendMessageOptions.DontRequireReceiver);
		}
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
