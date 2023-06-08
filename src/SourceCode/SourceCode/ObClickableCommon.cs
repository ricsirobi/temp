using UnityEngine;

public class ObClickableCommon : ObClickable
{
	public int _StoreID;

	public StoreLoader.Selection _CoinsStoreInfo;

	public int _ItemID;

	public int _RedeemTicketItemID;

	public bool _ForMembersOnly;

	public bool _AllowMultiple;

	public LocaleString _BecomeAMemberText = new LocaleString("You need to become a member to interact with this item.");

	public LocaleString _PurchaseItemText = new LocaleString("Would you like to purchase this object for xxxx gems?");

	public LocaleString _BuyFundsText = new LocaleString("Would you like to buy funds?");

	public LocaleString _ItemPurchaseFailedText = new LocaleString("Item purchase failed.");

	public LocaleString _ItemPurchaseSuccessText = new LocaleString("Item purchased! Please check your inventory.");

	public LocaleString _PointLockedText = new LocaleString("You need to have {{Points}} points to buy this {{Item}}.");

	private ItemData mItemData;

	private KAUIGenericDB mUiGenericDB;

	public override void ProcessMouseUp()
	{
		if (_ForMembersOnly && !SubscriptionInfo.pIsMember && (_RedeemTicketItemID <= 0 || (CommonInventoryData.pInstance.FindItem(_RedeemTicketItemID) == null && ParentData.pIsReady && !ParentData.pInstance.HasItem(_RedeemTicketItemID))))
		{
			ShowDB(_BecomeAMemberText, "OnBecomeMember", "ProcessCloseDB", null, null);
		}
		else if (CanPurchase() && _ItemID > 0 && _StoreID > 0 && (_AllowMultiple || (CommonInventoryData.pInstance.FindItem(_ItemID) == null && ParentData.pIsReady && !ParentData.pInstance.HasItem(_ItemID))))
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			ItemStoreDataLoader.Load(_StoreID, OnStoreLoaded);
		}
		else
		{
			base.ProcessMouseUp();
		}
	}

	public virtual bool CanPurchase()
	{
		return true;
	}

	private void OnBecomeMember()
	{
		CloseDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
	}

	private void OnIAPStoreClosed()
	{
		ProcessCloseDB();
	}

	protected void OnStoreLoaded(StoreData sd)
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (sd == null || sd._Items == null || sd._Items.Length == 0)
		{
			Debug.LogError("Storedata is not added");
			return;
		}
		mItemData = sd.FindItem(_ItemID);
		if (mItemData != null)
		{
			int rid = 0;
			if (new KAStoreItemData(null, null, mItemData, _StoreID, 0).IsRankLocked(out rid, mItemData.RewardTypeID) && rid > 0)
			{
				string localizedString = _PointLockedText.GetLocalizedString();
				localizedString = localizedString.Replace("{{Points}}", rid.ToString());
				localizedString = localizedString.Replace("{{Item}}", mItemData.ItemName);
				_PointLockedText = new LocaleString(localizedString);
				ShowDB(_PointLockedText, null, null, null, "ProcessCloseDB");
			}
			else if (mItemData.FinalCashCost == 0 && mItemData.FinalCost == 0)
			{
				_ItemID = 0;
				OnMouseUp();
			}
			else
			{
				LocaleString localeString = new LocaleString(_PurchaseItemText.GetLocalizedString());
				localeString._Text = localeString.GetLocalizedString().Replace("xxxx", (mItemData.FinalCashCost > 0) ? mItemData.FinalCashCost.ToString() : mItemData.FinalCost.ToString());
				ShowDB(localeString, "OnPurchaseItemOk", "ProcessCloseDB", null, null);
			}
		}
		else
		{
			Debug.LogError("Couldn't find ItemID:" + _ItemID + "  in StoreID:" + _StoreID);
		}
	}

	private void OnPurchaseItemOk()
	{
		if (mItemData.FinalCashCost > 0 && Money.pCashCurrency > mItemData.FinalCashCost)
		{
			PurchaseItem(2);
			return;
		}
		if (mItemData.FinalCost > 0 && Money.pGameCurrency > mItemData.FinalCost)
		{
			PurchaseItem(1);
			return;
		}
		ProcessCloseDB();
		ShowDB(_BuyFundsText, "BuyFunds", "ProcessCloseDB", null, null);
	}

	private void PurchaseItem(int inCurrencyType)
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		CommonInventoryData.pInstance.AddPurchaseItem(_ItemID, 1, ItemPurchaseSource.OB_CLICKABLE_COMMON.ToString());
		CommonInventoryData.pInstance.DoPurchase(inCurrencyType, _StoreID, OnItemPurchaseDone);
	}

	public void OnItemPurchaseDone(CommonInventoryResponse response)
	{
		ProcessCloseDB();
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (response != null && response.Success)
		{
			ShowDB(_ItemPurchaseSuccessText, null, null, null, "ProcessCloseDB");
			OnMouseUp();
		}
		else
		{
			ShowDB(_ItemPurchaseFailedText, null, null, null, "ProcessCloseDB");
		}
	}

	private void BuyFunds()
	{
		ProcessCloseDB();
		if (mItemData.FinalCost > 0)
		{
			if (_CoinsStoreInfo != null)
			{
				StoreLoader.Load(setDefaultMenuItem: true, _CoinsStoreInfo._Category, _CoinsStoreInfo._Store, base.gameObject);
			}
		}
		else if (mItemData.FinalCashCost > 0)
		{
			IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
		}
	}

	private void OnStoreClosed()
	{
		OnMouseUp();
	}

	protected void ShowDB(LocaleString inMessage, string YesMsg, string NoMsg, string CloseMsg, string OkMsg, string normalString = null)
	{
		mUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "WarningDB");
		mUiGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(YesMsg), !string.IsNullOrEmpty(NoMsg), !string.IsNullOrEmpty(OkMsg), !string.IsNullOrEmpty(CloseMsg));
		mUiGenericDB._MessageObject = base.gameObject;
		if (!string.IsNullOrEmpty(YesMsg))
		{
			mUiGenericDB._YesMessage = YesMsg;
		}
		if (!string.IsNullOrEmpty(NoMsg))
		{
			mUiGenericDB._NoMessage = NoMsg;
		}
		if (!string.IsNullOrEmpty(OkMsg))
		{
			mUiGenericDB._OKMessage = OkMsg;
		}
		if (!string.IsNullOrEmpty(CloseMsg))
		{
			mUiGenericDB._CloseMessage = CloseMsg;
		}
		if (inMessage != null)
		{
			mUiGenericDB.SetTextByID(inMessage._ID, inMessage._Text, interactive: false);
		}
		else if (!string.IsNullOrEmpty(normalString))
		{
			mUiGenericDB.SetText(normalString, interactive: false);
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		KAUI.SetExclusive(mUiGenericDB);
	}

	protected void ProcessCloseDB()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		CloseDB();
	}

	private void CloseDB()
	{
		if (mUiGenericDB != null)
		{
			KAUI.RemoveExclusive(mUiGenericDB);
			Object.Destroy(mUiGenericDB.gameObject);
			mUiGenericDB = null;
		}
	}
}
