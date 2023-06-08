using System;
using System.Collections.Generic;
using UnityEngine;

public class UiBundleConfirmation : KAUI
{
	public enum Status
	{
		Loaded,
		Accepted,
		Confirmed,
		Closed
	}

	public delegate void Callback(Status status);

	public string _GemsSpriteName = "AniDWDragonsBaseCurrencyGems";

	public string _CoinsSpriteName = "AniDWDragonsBaseCurrencyCoins";

	public LocaleString _OwnAllItemsText = new LocaleString("You already own all of the items in this bundle!");

	public LocaleString _NewText = new LocaleString("NEW");

	public LocaleString _OwnedText = new LocaleString("OWNED");

	public Color _OwnedItemColor = Color.black;

	public Color _NewItemColor = Color.green;

	private KAWidget mBtnConfirm;

	private KAWidget mBtnClose;

	private KAWidget mTxtPrice;

	private KAWidget mIcoPrice;

	private bool mAllItemsOwned;

	private const string mWidgetName = "ItemName";

	private const string mSeparator = " x ";

	private static ItemData mBundleItemData;

	private static Callback mCallBack;

	public static void Init(Callback callback, ItemData bundleItemData)
	{
		mCallBack = callback;
		mBundleItemData = bundleItemData;
		if (bundleItemData != null)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			CommonInventoryData.pInstance.GetBundledItemsInInventory(bundleItemData, ObtainInventoryItems);
		}
	}

	private static void ObtainInventoryItems(List<UserItemData> userItems)
	{
		if (userItems != null && userItems.Count > 0)
		{
			foreach (UserItemData userItem in userItems)
			{
				if (userItem.Item.InventoryMax > 0)
				{
					RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("BundleConfirmationAsset"), OnBundleReady, typeof(GameObject), inDontDestroy: false, userItems);
					return;
				}
			}
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		SendCallback(Status.Accepted);
	}

	private static void OnBundleReady(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			UiBundleConfirmation uiBundleConfirmation = null;
			if (gameObject != null)
			{
				uiBundleConfirmation = gameObject.GetComponent<UiBundleConfirmation>();
			}
			if (uiBundleConfirmation != null && inUserData != null)
			{
				mCallBack?.Invoke(Status.Loaded);
			}
			uiBundleConfirmation.DisplayItems((List<UserItemData>)inUserData);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			SendCallback(Status.Closed);
			break;
		}
	}

	public void DisplayItems(List<UserItemData> userItems)
	{
		KAUIMenu kAUIMenu = _MenuList[0];
		if (!(kAUIMenu != null))
		{
			return;
		}
		kAUIMenu.ClearItems();
		if (mBundleItemData != null && mBundleItemData.pBundledItems != null && mBundleItemData.pBundledItems.Count > 0)
		{
			List<ItemData> list = new List<ItemData>();
			if (userItems != null && userItems.Count > 0)
			{
				foreach (ItemData item2 in mBundleItemData.pBundledItems)
				{
					if (item2.InventoryMax < 0 || userItems.Find((UserItemData i) => i.Item.ItemID == item2.ItemID) == null)
					{
						list.Add(item2);
					}
				}
			}
			else
			{
				list.AddRange(mBundleItemData.pBundledItems);
			}
			if (list.Count > 0)
			{
				ShowInMenu(kAUIMenu, "[u]" + _NewText.GetLocalizedString(), _NewItemColor);
				foreach (ItemData item in list)
				{
					ItemDataRelationship itemDataRelationship = Array.Find(mBundleItemData.Relationship, (ItemDataRelationship r) => r.ItemId == item.ItemID);
					if (itemDataRelationship != null)
					{
						ShowInMenu(kAUIMenu, item.ItemName + ((itemDataRelationship.Quantity > 1) ? (" x " + itemDataRelationship.Quantity) : ""), _NewItemColor);
					}
				}
			}
			else
			{
				mAllItemsOwned = true;
			}
		}
		ShowInMenu(kAUIMenu, "", _OwnedItemColor);
		ShowInMenu(kAUIMenu, "[u]" + _OwnedText.GetLocalizedString(), _OwnedItemColor);
		if (userItems == null || userItems.Count <= 0)
		{
			return;
		}
		foreach (UserItemData userItem in userItems)
		{
			if (userItem.Item.InventoryMax > 0)
			{
				ShowInMenu(kAUIMenu, userItem.Item.ItemName + ((userItem.Quantity > 1) ? (" x " + userItem.Quantity) : ""), _OwnedItemColor);
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		mBtnClose = FindItem("BtnClose");
		mBtnConfirm = FindItem("BtnConfirm");
		mTxtPrice = FindItem("TxtPrice");
		mIcoPrice = FindItem("IcoPrice");
		SetPrice();
		KAUI.SetExclusive(this);
	}

	private void SetPrice()
	{
		if (mIcoPrice != null)
		{
			mIcoPrice.SetSprite((mBundleItemData.GetPurchaseType() == 2) ? _GemsSpriteName : _CoinsSpriteName);
		}
		if (mTxtPrice != null)
		{
			mTxtPrice.SetText((mBundleItemData.GetPurchaseType() == 2) ? mBundleItemData.FinalCashCost.ToString() : mBundleItemData.FinalCost.ToString());
		}
	}

	private void ShowInMenu(KAUIMenu menu, string name, Color color)
	{
		UILabel label = menu.AddWidget("ItemName").GetLabel();
		if (label != null)
		{
			label.text = name;
			label.color = color;
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item.name == "BtnConfirm")
		{
			if (mAllItemsOwned)
			{
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _OwnAllItemsText.GetLocalizedString(), null, "");
			}
			else
			{
				CloseUI(Status.Confirmed);
			}
		}
		else if (item.name == "BtnClose")
		{
			CloseUI(Status.Closed);
		}
	}

	private void CloseUI(Status status)
	{
		KAUI.RemoveExclusive(this);
		UnityEngine.Object.Destroy(base.gameObject);
		SendCallback(status);
	}

	private static void SendCallback(Status status)
	{
		if (mCallBack != null)
		{
			mCallBack(status);
		}
		mCallBack = null;
		mBundleItemData = null;
	}
}
