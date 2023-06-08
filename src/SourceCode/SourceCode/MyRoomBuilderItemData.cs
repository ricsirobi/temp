using UnityEngine;

public class MyRoomBuilderItemData : CoBundleItemData
{
	public bool _Locked;

	public UiMyRoomBuilderMenu _Menu;

	public int _ItemID;

	public ItemPrefabResData _ItemPrefabData = new ItemPrefabResData();

	public ItemTextureResData _TextureData = new ItemTextureResData();

	private UiMyRoomBuilder mParent;

	public MyRoomBuilderItemData(UiMyRoomBuilderMenu menu, UiMyRoomBuilder inParent, ItemData item)
	{
		_ItemID = item.ItemID;
		_Menu = menu;
		mParent = inParent;
		_ItemTextureData.Init(item.IconName);
		_ItemPrefabData.Init(item.AssetName);
		string resName = "";
		if (item.Texture != null && item.Texture.Length != 0)
		{
			resName = item.Texture[0].TextureName;
		}
		_TextureData.Init(resName);
		_ItemRVOData.Init(null);
		_Locked = !SubscriptionInfo.pIsMember && item.Locked;
	}

	public void CopyData(MyRoomBuilderItemData s)
	{
		CopyData((CoBundleItemData)s);
		_Locked = s._Locked;
	}

	public override void LoadResource()
	{
		base.LoadResource();
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
		if (!_ItemPrefabData.IsDataLoaded() || !_TextureData.IsDataLoaded())
		{
			return;
		}
		base.OnAllDownloaded();
		KAWidget item = GetItem();
		if (item != null)
		{
			KAWidget kAWidget = item.FindChildItem("Quantity");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: true);
			}
			KAWidget kAWidget2 = item.FindChildItem("CreativePoints");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: true);
			}
			KAWidget kAWidget3 = item.FindChildItem("LoadingAnim");
			if (kAWidget3 != null)
			{
				item.RemoveChildItem(kAWidget3, destroy: true);
			}
			if (_Locked)
			{
				KAWidget kAWidget4 = mParent.DuplicateWidget("_TemplateLockedItem");
				kAWidget4.gameObject.SetActive(value: true);
				kAWidget4.SetState(KAUIState.NOT_INTERACTIVE);
				item.AddChild(kAWidget4);
			}
		}
	}

	public void UpdateQuantity()
	{
		KAWidget kAWidget = GetItem().FindChildItem("Quantity");
		if (kAWidget != null)
		{
			int quantity = CommonInventoryData.pInstance.GetQuantity(_ItemID);
			UILabel componentInChildren = kAWidget.GetComponentInChildren<UILabel>();
			if (componentInChildren != null)
			{
				componentInChildren.text = quantity.ToString();
			}
			kAWidget.SetVisibility(quantity > 0);
		}
	}
}
