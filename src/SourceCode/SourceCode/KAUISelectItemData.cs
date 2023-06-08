using UnityEngine;

public class KAUISelectItemData : CoBundleItemData
{
	public string _PrefResName = "";

	public string _TextResName = "";

	public int _Quantity;

	public KAUISelectMenu _Menu;

	public int _WH;

	public int _ItemID = -1;

	public bool _SlotLocked;

	public bool _Locked;

	public bool _Disabled;

	public ItemData _ItemData;

	public UserItemData _UserItemData;

	public int _UserInventoryID = -1;

	public bool _IsBattleReady;

	public ItemPrefabResData _ItemPrefabData = new ItemPrefabResData();

	public ItemTextureResData _TextureData = new ItemTextureResData();

	public KAWidget pGreyMaskWidget { get; set; }

	public bool pEquippedItem { get; set; }

	public KAUISelectItemData()
	{
	}

	public KAUISelectItemData(KAUISelectMenu menu, ItemData item, int wh, int quantity, InventoryTabType tabType = InventoryTabType.ICON)
	{
		InitSelectItemData(menu, item, wh, quantity, -1, tabType);
	}

	public KAUISelectItemData(KAUISelectMenu menu, UserItemData userItemData, int wh, int quantity, InventoryTabType tabType = InventoryTabType.ICON)
	{
		_UserItemData = userItemData;
		InitSelectItemData(menu, userItemData.Item, wh, quantity, userItemData.UserInventoryID, tabType);
	}

	private void InitSelectItemData(KAUISelectMenu menu, ItemData itemData, int wh, int quantity, int userInventoryID = -1, InventoryTabType tabType = InventoryTabType.ICON)
	{
		_ItemID = itemData.ItemID;
		_Menu = menu;
		_ItemTextureData.Init(itemData.IconName);
		if (tabType == InventoryTabType.ICON)
		{
			_ItemPrefabData.Init(null);
		}
		else
		{
			_ItemPrefabData.Init(itemData.AssetName);
		}
		string text = "";
		if (itemData.Texture != null && itemData.Texture.Length != 0)
		{
			text = itemData.Texture[0].TextureName;
		}
		_TextureData.Init(text);
		_ItemRVOData.Init(null);
		_ItemData = itemData;
		string resName = "";
		if (itemData.Rollover != null)
		{
			resName = itemData.Rollover.Bundle + "/" + itemData.Rollover.DialogName;
		}
		if (itemData.Texture != null && itemData.Texture.Length != 0)
		{
			text = itemData.Texture[0].TextureName;
		}
		_ItemRVOData.Init(resName);
		_PrefResName = itemData.AssetName;
		_TextResName = text;
		_Menu = menu;
		_Quantity = quantity;
		_WH = wh;
		_Locked = itemData.Locked && !SubscriptionInfo.pIsMember;
		_UserInventoryID = userInventoryID;
		_IsBattleReady = (_UserItemData != null && _UserItemData.pIsBattleReady) || (_ItemData != null && _ItemData.IsStatAvailable());
	}

	public override void LoadResource()
	{
		base.LoadResource();
		_ItemPrefabData.LoadBundle(this);
		_TextureData.LoadBundle(this);
	}

	public override void OnBundleError(string inURL)
	{
		_ItemPrefabData.OnBundleReady(inURL, null);
		_TextureData.OnBundleReady(inURL, null);
		base.OnBundleError(inURL);
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
		ShowLoadingItem(inShow: false);
		KAWidget item = GetItem();
		if (CommonInventoryData.pInstance != null)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(_ItemID);
			if (userItemData != null)
			{
				string text = userItemData.Quantity.ToString();
				text = ((userItemData.Quantity <= 1 || string.IsNullOrEmpty(userItemData.Item.ItemNamePlural)) ? (text + " " + userItemData.Item.ItemName) : (text + " " + userItemData.Item.ItemNamePlural));
				if (CommonInventoryData.pShowItemID)
				{
					text = text + "(" + userItemData.Item.ItemID + ")";
				}
				item.SetToolTipText(text);
			}
			item.SetVisibility(inVisible: true);
		}
		if (_WH > 0 && item != null)
		{
			UITexture uITexture = item.GetUITexture();
			if (null != uITexture)
			{
				Vector3 localScale = uITexture.cachedTransform.localScale;
				localScale.Set(1f, 1f, 1f);
				uITexture.cachedTransform.localScale = localScale;
				uITexture.width = _WH;
				uITexture.height = _WH;
			}
		}
		if (_Quantity > 1 && !_Menu._MultiInstance && !string.IsNullOrEmpty(_Menu._QuantityWidgetName))
		{
			KAWidget kAWidget = GetItem().FindChildItem(_Menu._QuantityWidgetName);
			if (kAWidget != null)
			{
				kAWidget.SetText(_Quantity.ToString());
				kAWidget.SetState(KAUIState.NOT_INTERACTIVE);
				kAWidget.SetVisibility(inVisible: true);
			}
		}
		if (_Locked)
		{
			KAWidget kAWidget2 = GetItem().FindChildItem(_Menu._LockedIconName);
			if (kAWidget2 != null)
			{
				kAWidget2.SetState(KAUIState.NOT_INTERACTIVE);
				kAWidget2.SetVisibility(inVisible: true);
			}
		}
		if (_Disabled)
		{
			KAWidget kAWidget3 = GetItem().FindChildItem(_Menu._DisabledWidgetName);
			if (kAWidget3 != null)
			{
				kAWidget3.SetVisibility(inVisible: true);
			}
		}
		if (CommonInventoryData.pShowItemID && _Menu._ToolTipFont != null)
		{
			GetItem()._TooltipInfo._Style = TooltipStyle.SCALE;
			GetItem()._TooltipInfo._Font = _Menu._ToolTipFont;
			GetItem().SetToolTipText(_ItemID.ToString());
		}
		_Menu.OnItemLoaded(this);
	}

	public void ShowSlotLock(KAWidget inWidget, bool addIcon = false, bool locked = true)
	{
		_SlotLocked = locked;
		inWidget.SetInteractive(locked);
		KAWidget kAWidget = (addIcon ? GetItem().FindChildItem(_Menu._AddIconName) : GetItem().FindChildItem(_Menu._LockedIconName));
		if (kAWidget != null)
		{
			kAWidget.SetState(KAUIState.NOT_INTERACTIVE);
			kAWidget.SetVisibility(locked);
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
