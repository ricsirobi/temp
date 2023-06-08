using UnityEngine;

public class InventoryItemDataGeneric : CoBundleItemData
{
	public bool _MemberLocked;

	public bool _SlotLocked;

	public int _ItemID;

	public UiInventoryCategoryItemMenu _Menu;

	public ItemPrefabResData _ItemPrefabData = new ItemPrefabResData();

	public ItemTextureResData _TextureData = new ItemTextureResData();

	private KAWidget mQuantityItem;

	private Vector3 mScale = new Vector3(36f, 28f, 0f);

	public virtual void Init(UiInventoryCategoryItemMenu menu, ItemData item)
	{
		_ItemID = item.ItemID;
		_Menu = menu;
		_ItemTextureData.Init(item.IconName);
		_ItemPrefabData.Init(item.AssetName);
		string resName = "";
		if (item.Texture != null && item.Texture.Length != 0)
		{
			resName = item.Texture[0].TextureName;
		}
		_TextureData.Init(resName);
		_ItemRVOData.Init(null);
		_MemberLocked = !SubscriptionInfo.pIsMember && item.Locked;
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
		ShowLoadingItem(inShow: false);
		base.OnAllDownloaded();
		UpdateQuantity();
		KAWidget item = GetItem();
		if (_MemberLocked && item != null)
		{
			KAWidget kAWidget = item.FindChildItem("LockedIcon");
			if (kAWidget != null)
			{
				kAWidget.SetState(KAUIState.NOT_INTERACTIVE);
				kAWidget.SetVisibility(inVisible: true);
			}
		}
		if (item != null)
		{
			UITexture uITexture = item.GetUITexture();
			if (uITexture != null)
			{
				Vector3 localScale = uITexture.cachedTransform.localScale;
				localScale.Set(1f, 1f, 1f);
				uITexture.cachedTransform.localScale = localScale;
				uITexture.width = (int)mScale.x;
				uITexture.height = (int)mScale.y;
			}
		}
	}

	public void UpdateQuantity()
	{
		if (mQuantityItem == null)
		{
			KAWidget item = GetItem();
			if (item != null)
			{
				mQuantityItem = item.FindChildItem("Quantity");
			}
			if (mQuantityItem != null)
			{
				mQuantityItem.SetVisibility(inVisible: true);
			}
		}
		if (mQuantityItem != null && CommonInventoryData.pInstance != null && CommonInventoryData.pIsReady && mQuantityItem != null)
		{
			int quantity = CommonInventoryData.pInstance.GetQuantity(_ItemID);
			mQuantityItem.SetText(quantity.ToString());
		}
	}

	public void ShowSlotLock(KAWidget inWidget)
	{
		_SlotLocked = true;
		KAWidget kAWidget = GetItem().FindChildItem("LockedIcon");
		if (kAWidget != null)
		{
			kAWidget.SetState(KAUIState.NOT_INTERACTIVE);
			kAWidget.SetVisibility(inVisible: true);
		}
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
