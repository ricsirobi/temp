using UnityEngine;

public class ItemPurchase
{
	public enum Status
	{
		None,
		DataLoaded,
		Success,
		Fail,
		NotEnoughCurrency
	}

	public delegate void PurchaseCallback(Status state);

	public PurchaseCallback mStatusCallback;

	private LocaleString mSuccessText;

	private LocaleString mFailText;

	private LocaleString mNotEnoughCurrencyText;

	private LocaleString mConfirmationText;

	private LocaleString mProcessingText;

	private ItemData mItemData;

	private string mSourceName;

	private string mPurchaseType;

	private int mItemId;

	private int mStoreId;

	private int mItemCost = -1;

	private const string Cost = "{cost}";

	private KAUIGenericDB mKAUIGenericDB;

	public void SetMessages(LocaleString successText, LocaleString failText, LocaleString confirmationText, LocaleString processingText, LocaleString notEnoughCurrencyText)
	{
		mSuccessText = successText;
		mFailText = failText;
		mNotEnoughCurrencyText = notEnoughCurrencyText;
		mConfirmationText = confirmationText;
		mProcessingText = processingText;
	}

	public void Init(int storeId, int itemId, PurchaseCallback statusCallback, string sourceName, string purchaseType)
	{
		mItemId = itemId;
		mStoreId = storeId;
		mStatusCallback = statusCallback;
		mSourceName = sourceName;
		mPurchaseType = purchaseType;
	}

	public void ProcessPurchase()
	{
		if (mStoreId <= 0 || mItemId <= 0)
		{
			UtDebug.Log("Item purchase : itemd id or store id is not setup");
			if (mFailText != null && string.IsNullOrEmpty(mFailText._Text))
			{
				ShowMessageDB(mFailText);
			}
		}
		else if (mItemData == null)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			ItemData.Load(mItemId, OnItemDataLoaded, null);
		}
		else
		{
			ConfirmPurchase();
		}
	}

	public bool ItemAvailable(int itemId)
	{
		return CommonInventoryData.pInstance.GetQuantity(itemId) > 0;
	}

	private void OnItemDataLoaded(int itemID, ItemData itemData, object inUserData)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		mItemData = itemData;
		if (mItemData != null)
		{
			mItemCost = ((mItemData.GetPurchaseType() == 2) ? mItemData.FinalCashCost : mItemData.FinalCost);
		}
		ConfirmPurchase();
	}

	private void ConfirmPurchase()
	{
		if (mConfirmationText != null && !string.IsNullOrEmpty(mConfirmationText._Text))
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", mConfirmationText.GetLocalizedString().Replace("{cost}", mItemCost.ToString()), "", null, "PurchaseItem", "Close", "", "", inDestroyOnClick: true).OnMessageReceived += OnMessageReceived;
		}
		else
		{
			PurchaseItem();
		}
	}

	private void OnMessageReceived(string message)
	{
		if (!(message == "PurchaseItem"))
		{
			if (message == "OpenStore")
			{
				OpenStore();
			}
		}
		else
		{
			PurchaseItem();
		}
	}

	private void KillGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	private void PurchaseItem()
	{
		if (VerifyPurchase())
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			ShowPurchaseProcess();
			CommonInventoryData.pInstance.AddPurchaseItem(mItemId, 1, mSourceName);
			CommonInventoryData.pInstance.DoPurchase(mItemData.GetPurchaseType(), mStoreId, TicketPurchaseHandler);
		}
	}

	private bool VerifyPurchase()
	{
		if (((mItemData.GetPurchaseType() == 2) ? Money.pCashCurrency : Money.pGameCurrency) < mItemCost)
		{
			if (mStatusCallback != null)
			{
				mStatusCallback(Status.NotEnoughCurrency);
			}
			if (mNotEnoughCurrencyText != null && !string.IsNullOrEmpty(mNotEnoughCurrencyText._Text))
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", mNotEnoughCurrencyText.GetLocalizedString(), "", null, "OpenStore", "Close", "", "", inDestroyOnClick: true).OnMessageReceived += OnMessageReceived;
			}
			return false;
		}
		return true;
	}

	private void OpenStore()
	{
		if (mItemData.GetPurchaseType() == 2)
		{
			IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS);
		}
		else
		{
			StoreLoader.Load(setDefaultMenuItem: true, "Buy Coins", "Gems and Coins", null);
		}
	}

	private void ShowPurchaseProcess()
	{
		if (mProcessingText != null && !string.IsNullOrEmpty(mProcessingText._Text))
		{
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
			mKAUIGenericDB.SetText(mProcessingText.GetLocalizedString(), interactive: false);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	private void TicketPurchaseHandler(CommonInventoryResponse ret)
	{
		KillGenericDB();
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (ret == null || !ret.Success)
		{
			if (mFailText != null && !string.IsNullOrEmpty(mFailText._Text))
			{
				ShowMessageDB(mFailText);
			}
			if (mStatusCallback != null)
			{
				mStatusCallback(Status.Fail);
			}
		}
		else
		{
			if (mSuccessText != null && !string.IsNullOrEmpty(mSuccessText._Text))
			{
				ShowMessageDB(mSuccessText);
			}
			if (mStatusCallback != null)
			{
				mStatusCallback(Status.Success);
			}
		}
	}

	private void ShowMessageDB(LocaleString message)
	{
		GameUtilities.DisplayOKMessage("PfKAUIGenericDB", message.GetLocalizedString(), null, "");
	}

	private void OnItemInfoLoaded(int itemID, ItemData itemData, object inUserData)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		mItemData = itemData;
		if (mItemData != null)
		{
			mItemCost = ((mItemData.GetPurchaseType() == 2) ? mItemData.FinalCashCost : mItemData.FinalCost);
		}
		if (mStatusCallback != null)
		{
			mStatusCallback(Status.DataLoaded);
		}
	}

	public void LoadItemInfo()
	{
		if (mItemData == null)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			ItemData.Load(mItemId, OnItemInfoLoaded, null);
		}
		else if (mStatusCallback != null)
		{
			mStatusCallback(Status.DataLoaded);
		}
	}

	public int GetItemCost()
	{
		return mItemCost;
	}
}
