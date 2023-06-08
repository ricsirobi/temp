using System.Collections.Generic;
using UnityEngine;

public class KAUIIAPStoreChooseMenu : KAUIMenu
{
	public KAUIIAPStoreSyncPopUp _IAPStoreSyncPopUp;

	public KAUIIAPStore _IAPStoreUI;

	public Texture _IconTextureGems;

	public Color _OfferPriceColor = Color.green;

	public int _CoinsIconWHSize = 80;

	public UiIAPGemsStorePreview _IAPGemsStorePreview;

	private int mCurrentTopIdx;

	private KAWidget mCurrentItem;

	private KAWidget mCachedCurrentItem;

	private KAStoreItemData mChosenStoreItemData;

	private bool mWaitSubscriptionRefresh;

	private KAUIGenericDB mKAUIGenericDB;

	private bool mReInitInventory;

	protected override void Start()
	{
		base.Start();
		_IAPStoreSyncPopUp.SetVisibility(t: false);
		if (IAPManager.pInstance != null)
		{
			IAPManager.pInstance.AddToMsglist(base.gameObject);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (IAPManager.pInstance != null)
		{
			IAPManager.pInstance.RemoveFromMsglist(base.gameObject);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mWaitSubscriptionRefresh && SubscriptionInfo.pIsReady && ParentData.pIsReady)
		{
			mWaitSubscriptionRefresh = false;
			LoadIAPItems((int)_IAPStoreUI._IAPStoreCategory);
			OnSyncSuccess();
		}
		if (mReInitInventory && CommonInventoryData.pIsReady)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			mReInitInventory = false;
			ShowSyncSuccessfulPopup();
		}
	}

	public void ChangeIAPCategory(IAPStoreCategory category)
	{
		_IAPStoreUI._IAPStoreCategory = category;
		LoadIAPItems((int)_IAPStoreUI._IAPStoreCategory);
	}

	public void LoadIAPItems(int categoryID)
	{
		if (IAPManager.pIAPStoreData == null)
		{
			return;
		}
		ClearItems();
		IAPItemData[] dataListInCategory = IAPManager.pIAPStoreData.GetDataListInCategory(categoryID);
		if (dataListInCategory != null && dataListInCategory.Length != 0)
		{
			IAPItemData[] array = dataListInCategory;
			foreach (IAPItemData iAPItemData in array)
			{
				if (iAPItemData != null && GameUtilities.CheckItemValidity(iAPItemData.PurchaseDateRange, iAPItemData.Event) && (iAPItemData.PurchaseType != 0 || iAPItemData.ItemAvailable) && (categoryID != 2 || !SubscriptionInfo.pIsMember || (iAPItemData.PurchaseType != 1 && (!IAPManager.IsMembershipRecurring() || iAPItemData.BillFrequency > SubscriptionInfo.GetBillFrequency()))))
				{
					KAWidget kAWidget = DuplicateWidget(_Template);
					AddWidget(kAWidget);
					kAWidget.SetVisibility(inVisible: true);
					kAWidget.name = iAPItemData.ItemName.GetLocalizedString();
					KAWidget kAWidget2 = kAWidget.FindChildItem("AniCreditsInfo");
					kAWidget2.SetText(iAPItemData.ItemName.GetLocalizedString());
					kAWidget2.FindChildItem("AniCreditCard").SetVisibility(iAPItemData.PurchaseType == 1);
					kAWidget2.FindChildItem("TxtCurrency").SetText(iAPItemData.FormattedPrice);
					kAWidget.FindChildItem("AniInfoBehind").SetText(iAPItemData.Description.GetLocalizedString());
					IAPItemWidgetUserData iAPItemWidgetUserData = new IAPItemWidgetUserData(iAPItemData.IconName, null, iAPItemData.PreviewAsset);
					iAPItemWidgetUserData._AppStoreID = iAPItemData.AppStoreID;
					if (categoryID == 1 || (categoryID == 2 && !SubscriptionInfo.pIsMember))
					{
						iAPItemWidgetUserData._NoofCoins = iAPItemData.NumberOfCoins;
					}
					else
					{
						iAPItemWidgetUserData._NoofCoins = 0;
					}
					iAPItemWidgetUserData._IAPItemData = iAPItemData;
					kAWidget.FindChildItem("Icon").SetUserData(iAPItemWidgetUserData);
					iAPItemWidgetUserData.LoadResource();
				}
			}
			mCurrentGrid.repositionNow = true;
			if (GetNumItems() > 0)
			{
				if (UtPlatform.IsiOS())
				{
					_IAPStoreUI.ShowSubscriptionInfoBtn(categoryID == 2);
					_IAPStoreUI.ShowTermsAndPolicyBtn(categoryID == 2);
				}
				int numItemsPerPage = GetNumItemsPerPage();
				if (GetNumItems() <= numItemsPerPage)
				{
					mCurrentTopIdx = 0;
				}
				else if (mCurrentTopIdx >= GetNumItems())
				{
					mCurrentTopIdx -= GetNumItemsPerPage();
				}
				SetTopItemIdx(mCurrentTopIdx);
				SetSelectedItem(GetItemAt(0));
				KAWidget kAWidget3 = FindItemAt(0).FindChildItem("AniIconInfo").FindChildItem("AniIconFront");
				if (kAWidget3 != null)
				{
					OnClick(kAWidget3);
				}
			}
		}
		else if (categoryID == 425)
		{
			LoadCoinItems();
		}
	}

	private void LoadCoinItems()
	{
		if (_IAPStoreUI.GetCoinsData() == null)
		{
			return;
		}
		ClearItems();
		foreach (ItemData coinsDatum in _IAPStoreUI.GetCoinsData())
		{
			if (coinsDatum != null)
			{
				KAWidget kAWidget = DuplicateWidget(_Template);
				AddWidget(kAWidget);
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.name = coinsDatum.ItemName;
				string rVo = "";
				if (coinsDatum.Rollover != null)
				{
					rVo = coinsDatum.Rollover.Bundle + "/" + coinsDatum.Rollover.DialogName;
				}
				KAStoreItemData kAStoreItemData = new KAStoreItemData(coinsDatum.IconName, rVo, coinsDatum, _IAPStoreUI._CoinStoreID, _CoinsIconWHSize);
				kAStoreItemData.PurchaseStoreType = KAStoreMenuItemData.StoreType.GameStore;
				kAWidget.FindChildItem("AniCreditsInfo").SetText(coinsDatum.ItemName);
				kAWidget.FindChildItem("AniInfoBehind").SetText(coinsDatum.Description);
				KAWidget kAWidget2 = kAWidget.FindChildItem("Icon");
				if (kAWidget2 != null)
				{
					kAWidget2.SetUserData(kAStoreItemData);
				}
				kAStoreItemData.ShowLoadingItem(inShow: true);
				kAStoreItemData.LoadResource();
				UpdateCredits(kAStoreItemData._ItemData, kAWidget);
			}
		}
		mCurrentGrid.repositionNow = true;
		if (GetNumItems() > 0)
		{
			KAWidget kAWidget3 = FindItemAt(0).FindChildItem("AniIconInfo").FindChildItem("AniIconFront");
			if (kAWidget3 != null)
			{
				OnClick(kAWidget3);
			}
		}
	}

	public void UpdateCredits(ItemData inItemData, KAWidget inWidget)
	{
		if (inItemData == null || inWidget == null)
		{
			return;
		}
		KAWidget kAWidget = inWidget.FindChildItem("AniCreditCard");
		KAWidget kAWidget2 = inWidget.FindChildItem("TxtCurrency");
		Color color = Color.black;
		KAWidget kAWidget3 = inWidget.FindChildItem("TxtStriked");
		if (kAWidget3 == null)
		{
			kAWidget3 = inWidget.FindChildItem("TxtStrikedCurrency");
		}
		KAWidget kAWidget4 = inWidget.FindChildItem("TxtSaleCost");
		UILabel label = kAWidget3.GetLabel();
		float num = -1f;
		Texture texture = null;
		int num2 = 0;
		int num3 = 0;
		num2 = inItemData.FinalCashCost;
		texture = _IconTextureGems;
		num3 = (SubscriptionInfo.pIsMember ? inItemData.pMemberCashCost : inItemData.pNonMemberCashCost);
		if (kAWidget != null)
		{
			if (!kAWidget.GetVisibility())
			{
				kAWidget.SetVisibility(inVisible: true);
			}
			kAWidget.SetTexture(texture);
		}
		if (kAWidget2 != null)
		{
			color = kAWidget2.GetLabel().color;
			if (!kAWidget2.GetVisibility())
			{
				kAWidget2.SetVisibility(inVisible: true);
			}
			kAWidget2.SetText(num2.ToString());
		}
		if (kAWidget3 != null)
		{
			if (num3 > num2)
			{
				kAWidget3.SetVisibility(inVisible: true);
				kAWidget3.SetText(num3.ToString());
				if (kAWidget4 != null)
				{
					kAWidget4.SetVisibility(inVisible: true);
					kAWidget4.SetText(num2.ToString());
					if (kAWidget2 != null)
					{
						kAWidget2.SetVisibility(inVisible: false);
					}
				}
				else if (kAWidget2 != null && kAWidget2.GetLabel() != null)
				{
					kAWidget2.GetLabel().color = _OfferPriceColor;
				}
				if (label != null)
				{
					num = label.transform.localScale.x * (float)label.width + 5f;
				}
			}
			else
			{
				if (kAWidget2 != null && kAWidget2.GetLabel() != null)
				{
					kAWidget2.GetLabel().color = color;
				}
				kAWidget3.SetVisibility(inVisible: false);
				kAWidget3.SetText(string.Empty);
			}
		}
		if (!(num <= 0f) && !(kAWidget3.pBackground == null))
		{
			kAWidget3.pBackground.width = (int)num;
			Vector3 localPosition = kAWidget3.pBackground.transform.localPosition;
			localPosition.x = 0f - num / 2f;
			kAWidget3.pBackground.transform.localPosition = localPosition;
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == null)
		{
			_IAPStoreUI.ShowBuyButton(show: false);
			_IAPStoreUI.ShowItemPreview(show: false);
			return;
		}
		if (item.name == "BtnInfo" || item.name == "AniInfoBehind")
		{
			KAWidget rootItem = item.GetRootItem();
			if (item.name == "AniInfoBehind")
			{
				TweenScale.Begin(rootItem.gameObject, 0.5f, new Vector3(0f, 1f, 1f));
				UITweener component = rootItem.gameObject.GetComponent<UITweener>();
				component.eventReceiver = rootItem.gameObject;
				component.callWhenFinished = "RotatedToFrontHalf";
			}
			else
			{
				TweenScale.Begin(rootItem.gameObject, 0.5f, new Vector3(0f, 1f, 1f));
				UITweener component2 = rootItem.gameObject.GetComponent<UITweener>();
				component2.eventReceiver = rootItem.gameObject;
				component2.callWhenFinished = "RotatedToBackHalf";
			}
		}
		if (item.name != "AniInfoBehind" && item.name != "AniIconFront" && item.name != "BtnInfo")
		{
			return;
		}
		KAWidget kAWidget = item.GetRootItem().FindChildItem("AniIconImage").FindChildItemAt(0);
		if (kAWidget == null)
		{
			return;
		}
		mCurrentItem = kAWidget;
		mCachedCurrentItem = mCurrentItem;
		mChosenStoreItemData = null;
		if (mCurrentItem.GetUserData().GetType() == typeof(KAStoreItemData))
		{
			mChosenStoreItemData = (KAStoreItemData)kAWidget.GetUserData();
		}
		else
		{
			IAPItemWidgetUserData iAPItemWidgetUserData = (IAPItemWidgetUserData)mCurrentItem.GetUserData();
			if (iAPItemWidgetUserData._IAPItemData.PurchaseType == 1)
			{
				int iSMStoreID = iAPItemWidgetUserData._IAPItemData.ISMStoreID;
				ItemData itemData = new ItemData();
				itemData.ItemID = iAPItemWidgetUserData._IAPItemData.ItemID;
				KAStoreItemData kAStoreItemData = new KAStoreItemData(itemData.IconName, "", itemData, iSMStoreID, 0);
				kAStoreItemData.PurchaseStoreType = KAStoreMenuItemData.StoreType.GameStore;
				mChosenStoreItemData = kAStoreItemData;
			}
		}
		_IAPStoreUI.OnSelectStoreItem(mCurrentItem);
	}

	public void BuyCurrentItem()
	{
		if (mCurrentItem == null)
		{
			UtDebug.LogError("Current item cannot be null!");
			return;
		}
		if (mCurrentItem != null && mCurrentItem != mCachedCurrentItem)
		{
			mCurrentItem = mCachedCurrentItem;
		}
		if (mChosenStoreItemData != null && mChosenStoreItemData.PurchaseStoreType == KAStoreMenuItemData.StoreType.GameStore)
		{
			if (HasEnoughCurrency(mCurrentItem.GetUserData()))
			{
				ShowConfirmationPopup();
			}
			else
			{
				ShowNotEnoughCurrencyMessage(_IAPStoreSyncPopUp._NotEnoughCashCurrencyText.GetLocalizedString(), _IAPStoreSyncPopUp._NotEnoughCurrencyTitleText.GetLocalizedString());
			}
			return;
		}
		IAPItemWidgetUserData iAPItemWidgetUserData = (IAPItemWidgetUserData)mCurrentItem.GetUserData();
		if (GameUtilities.CheckItemValidity(iAPItemWidgetUserData._IAPItemData.PurchaseDateRange, iAPItemWidgetUserData._IAPItemData.Event))
		{
			ShowSyncPopUp();
			IAPManager.pInstance.PurchaseProduct(iAPItemWidgetUserData._IAPItemData, _IAPStoreUI._IAPStoreCategory, _IAPStoreUI.gameObject);
		}
		else
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _IAPStoreUI._ItemExpiredText.GetLocalizedString(), null, base.gameObject, null, null, "OnDBClose", null, inDestroyOnClick: true);
		}
	}

	public void BuyIAPOfferItem(KAWidget currentItem)
	{
		if (currentItem == null)
		{
			UtDebug.Log("iap offer item cannot be null");
			return;
		}
		mCurrentItem = currentItem;
		if (((StoreWidgetUserData)mCurrentItem.GetUserData()).PurchaseStoreType == KAStoreMenuItemData.StoreType.IAPStore)
		{
			ShowSyncPopUp();
			IAPItemWidgetUserData iAPItemWidgetUserData = (IAPItemWidgetUserData)mCurrentItem.GetUserData();
			IAPManager.pInstance.PurchaseProduct(iAPItemWidgetUserData._IAPItemData, _IAPStoreUI._IAPStoreCategory, _IAPStoreUI.gameObject);
		}
	}

	private bool HasEnoughCurrency(KAWidgetUserData currentItem)
	{
		if (currentItem.GetType() == typeof(IAPItemWidgetUserData))
		{
			return int.Parse(((IAPItemWidgetUserData)currentItem)._IAPItemData.FormattedPrice) <= Money.pCashCurrency;
		}
		if (currentItem.GetType() == typeof(KAStoreItemData))
		{
			return ((KAStoreItemData)currentItem)._ItemData.CashCost <= Money.pCashCurrency;
		}
		return false;
	}

	private void ShowNotEnoughCurrencyMessage(string inText, string inTitle)
	{
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "NotEnoughGemsDB");
		mKAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "OpenGemsStore";
		mKAUIGenericDB.SetText(inText, interactive: false);
		mKAUIGenericDB.SetTitle(inTitle);
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void ShowConfirmationPopup()
	{
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Buy Trial Membership");
		mKAUIGenericDB.SetTitle("Warning");
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		mKAUIGenericDB.SetText(_IAPStoreSyncPopUp._PurchaseConfirmationText.GetLocalizedString(), interactive: false);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._YesMessage = "ProceedToBuy";
		mKAUIGenericDB._NoMessage = "CancelPurchase";
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void OpenGemsStore()
	{
		KillGenericDB();
		ChangeIAPCategory(IAPStoreCategory.GEMS);
	}

	private void CancelPurchase()
	{
		KillGenericDB();
		OnPurchaseCancelled("");
	}

	private void KillGenericDB()
	{
		if (!(mKAUIGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	private void ProceedToBuy()
	{
		KillGenericDB();
		ShowSyncPopUp();
		if (mChosenStoreItemData != null && mChosenStoreItemData.PurchaseStoreType == KAStoreMenuItemData.StoreType.GameStore)
		{
			DoPurchase(mChosenStoreItemData._ItemData.ItemID, 2, mChosenStoreItemData._StoreID);
		}
	}

	private void ShowSyncPopUp()
	{
		KillGenericDB();
		if (!_IAPStoreSyncPopUp.GetVisibility())
		{
			_IAPStoreUI.SetState(KAUIState.NOT_INTERACTIVE);
			_IAPGemsStorePreview.SetState(KAUIState.NOT_INTERACTIVE);
			SetState(KAUIState.NOT_INTERACTIVE);
			_IAPStoreSyncPopUp._MessageObject = base.gameObject;
			_IAPStoreSyncPopUp.SetVisibility(t: true);
			_IAPStoreSyncPopUp.ShowPopup(IAPPurchaseStatus.INITIATED);
		}
	}

	private void DoPurchase(int itemID, int currencyType, int storeID)
	{
		WsWebService.PurchaseItems(new List<int> { itemID }.ToArray(), currencyType, 1, storeID, OnPurchaseComplete, null);
	}

	private void SyncPopupClosed()
	{
		_IAPStoreUI.SetState(KAUIState.INTERACTIVE);
		_IAPGemsStorePreview.SetState(KAUIState.INTERACTIVE);
		SetState(KAUIState.INTERACTIVE);
	}

	private void OnSyncFailed()
	{
		OnSyncFailed(null);
	}

	private void OnSyncFailed(ReceiptRedemptionResult result)
	{
		_IAPStoreSyncPopUp.ShowPopup(IAPPurchaseStatus.SYNC_FAILED);
	}

	private void OnSyncSuccess()
	{
		if (UserInfo.pInstance != null && mCurrentItem != null && mCurrentItem.GetUserData().GetType() == typeof(IAPItemWidgetUserData))
		{
			IAPItemWidgetUserData iAPItemWidgetUserData = (IAPItemWidgetUserData)mCurrentItem.GetUserData();
			Money.AddToCashCurrency(iAPItemWidgetUserData._NoofCoins);
			if ((IAPManager.pIAPStoreData != null && IAPManager.pIAPStoreData.GetIAPCategoryType(iAPItemWidgetUserData._AppStoreID) == IAPCategoryType.Item) || IAPManager.pIAPStoreData.GetIAPCategoryType(iAPItemWidgetUserData._AppStoreID) == IAPCategoryType.Membership)
			{
				mReInitInventory = true;
				KAUICursorManager.SetDefaultCursor("Loading");
				CommonInventoryData.ReInit();
			}
		}
		if (!mReInitInventory)
		{
			ShowSyncSuccessfulPopup();
		}
	}

	private void ShowSyncSuccessfulPopup()
	{
		_IAPStoreSyncPopUp.ShowPopup(IAPPurchaseStatus.SYNC_SUCCEDED);
		SetSelectedItem(GetItemAt(0));
		KAWidget kAWidget = FindItemAt(0).FindChildItem("AniIconInfo").FindChildItem("AniIconFront");
		if (kAWidget != null)
		{
			OnClick(kAWidget);
		}
	}

	private void OnPurchaseSuccessful(string purchase)
	{
		_IAPStoreSyncPopUp.ShowPopup(IAPPurchaseStatus.SUCCEDED);
	}

	private void OnPurchaseFailed(string error)
	{
		_IAPStoreSyncPopUp.ShowPopup(IAPPurchaseStatus.FAILED);
	}

	private void OnPurchaseCancelled(string error)
	{
		if (UtPlatform.IsAndroid())
		{
			_IAPStoreSyncPopUp.ExitPopUp();
		}
		else
		{
			_IAPStoreSyncPopUp.ShowPopup(IAPPurchaseStatus.FAILED);
		}
	}

	private void OnPurchaseComplete(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			CommonInventoryResponse commonInventoryResponse = (CommonInventoryResponse)inObject;
			if (commonInventoryResponse != null && commonInventoryResponse.Success)
			{
				if (commonInventoryResponse.UserGameCurrency != null)
				{
					Money.SetMoney(commonInventoryResponse.UserGameCurrency);
				}
				ParentData.Reset();
				ParentData.Init();
				SubscriptionInfo.Reset();
				SubscriptionInfo.Init();
				mWaitSubscriptionRefresh = true;
			}
			else
			{
				OnPurchaseFailed("");
			}
			mChosenStoreItemData = null;
			break;
		}
		case WsServiceEvent.ERROR:
			OnPurchaseFailed("");
			mChosenStoreItemData = null;
			break;
		}
	}
}
