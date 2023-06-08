using System;
using UnityEngine;

public class UiBuyPopup : KAUI
{
	public delegate void BoosterBuyClose();

	[Serializable]
	public class Mapping
	{
		public ElementMatchGame.BoosterType _Type;

		public int _ItemID;

		public KAWidget _QuantityTxt;

		public KAWidget _CostTxt;

		public KAWidget _DecCount;

		public KAWidget _IncCount;

		public int _Count;

		public int _Cost;

		public int _MemberCost;
	}

	public delegate void PurchaseSuccessfull();

	public float _SuccessMsgTimeOut = 0.5f;

	public LocaleString _InitialStatusText = new LocaleString("Please wait while your purchase is being processed.");

	public LocaleString _PurchaseFailedText = new LocaleString("Your purchase failed, please try again later.");

	public LocaleString _PurchaseSuccessText = new LocaleString("Your purchase is completed.");

	public LocaleString _NotEnoughGameCurrencyText = new LocaleString("You do not have enough Coins.");

	public LocaleString _NotEnoughCashCurrencyText = new LocaleString("You do not have enough Gems.");

	public LocaleString _NotEnoughCurrencyTitleText = new LocaleString("Error!");

	public PurchaseSuccessfull _OnPurchaseSuccessful;

	public Mapping[] _Mappings;

	public int _StoreID;

	private int mTotalGemCost;

	protected KAWidget mItemName;

	protected KAWidget mItemDescription;

	protected KAWidget mQuantityPlus;

	protected KAWidget mQuantityMinus;

	protected KAWidget mTotalPriceGems;

	protected KAWidget mTotalPrice;

	protected KAWidget mTxtQuanityValue;

	protected KAWidget mTxtSyncStatus;

	protected KAWidget mPurchase;

	protected KAWidget mBtnCancel;

	private ItemData mItemData;

	[HideInInspector]
	public bool bClicked;

	public static event BoosterBuyClose OnBoosterBuyClose;

	protected override void Start()
	{
		base.Start();
		Init();
	}

	private void Init()
	{
		mTotalPriceGems = FindItem("TxtCostTotal");
		mBtnCancel = FindItem("CancelBtn");
		mPurchase = FindItem("BuyBtn");
		UtDebug.Log("Init for(int i=0;i<_Mappings.Length;++i) -- start..");
		for (int i = 0; i < _Mappings.Length; i++)
		{
			if (SubscriptionInfo.pIsMember)
			{
				_Mappings[i]._Cost = _Mappings[i]._MemberCost;
			}
			_Mappings[i]._CostTxt.SetText(_Mappings[i]._Cost.ToString());
		}
	}

	public override void SetVisibility(bool isVisible)
	{
		bClicked = false;
		Init();
		for (int i = 0; i < _Mappings.Length; i++)
		{
			_Mappings[i]._Count = 0;
			if (null != _Mappings[i]._QuantityTxt)
			{
				_Mappings[i]._QuantityTxt.SetText(_Mappings[i]._Count.ToString());
			}
		}
		mTotalGemCost = 0;
		if (null != mTotalPriceGems)
		{
			mTotalPriceGems.SetText("0");
		}
		if (null != mPurchase)
		{
			mPurchase.SetDisabled(mTotalGemCost <= 0);
		}
		if (isVisible)
		{
			KAUI.SetExclusive(this, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		}
		else if (GetVisibility())
		{
			KAUI.RemoveExclusive(this);
		}
		if (null != mPurchase)
		{
			mPurchase.SetVisibility(inVisible: true);
		}
		if (null != mBtnCancel)
		{
			mBtnCancel.SetVisibility(inVisible: true);
		}
		base.SetVisibility(isVisible);
	}

	private void RefreshValues()
	{
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item.name == "CancelBtn")
		{
			CloseDB();
		}
		else if (item.name == "BuyBtn")
		{
			PurchaseItem(directPurchase: false);
		}
		else
		{
			Click(item);
		}
		if (null != mPurchase)
		{
			mPurchase.SetDisabled(mTotalGemCost <= 0);
		}
	}

	public void ShowItem()
	{
		mItemName.SetText(mItemData.ItemName);
		if (mItemDescription != null)
		{
			mItemDescription.SetText(mItemData.Description);
		}
		UserItemData userItemData = (CommonInventoryData.pIsReady ? CommonInventoryData.pInstance.FindItem(mItemData.ItemID) : null);
		FindItem("AniBuyItem").SetTextureFromBundle(mItemData.IconName);
		KAWidget kAWidget = FindItem("BuyItemQuantity");
		if (userItemData != null)
		{
			kAWidget.SetText(userItemData.Quantity.ToString());
		}
		else
		{
			kAWidget.SetText("0");
		}
		RefreshValues();
	}

	public void PurchaseItem(bool directPurchase = true)
	{
		if (mTotalGemCost <= Money.pCashCurrency)
		{
			ProcessPurchase(isGems: true);
		}
		else
		{
			ShowNotEnoughCurrencyMessage(_NotEnoughCashCurrencyText, _NotEnoughCurrencyTitleText);
		}
	}

	private void ShowNotEnoughCurrencyMessage(LocaleString inText, LocaleString inTitle)
	{
		SetVisibility(isVisible: false);
		KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "NotEnoughGemsDB");
		kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		kAUIGenericDB._MessageObject = base.gameObject;
		kAUIGenericDB._OKMessage = "OnNotEnoughGemsOK";
		kAUIGenericDB.SetText(inText.GetLocalizedString(), interactive: false);
		kAUIGenericDB.SetTitle(inTitle.GetLocalizedString());
		KAUI.SetExclusive(kAUIGenericDB);
	}

	private void ProcessPurchase(bool isGems)
	{
		bClicked = true;
		mPurchase.SetVisibility(inVisible: false);
		mBtnCancel.SetVisibility(inVisible: false);
		if (null != mTxtSyncStatus)
		{
			mTxtSyncStatus.SetText(_InitialStatusText.GetLocalizedString());
		}
		SetState(KAUIState.NOT_INTERACTIVE);
		KAUICursorManager.SetDefaultCursor("Loading");
		ElementMatchGame elementMatchGame = (ElementMatchGame)TileMatchPuzzleGame.pInstance;
		for (int i = 0; i < _Mappings.Length; i++)
		{
			if (_Mappings[i]._Count > 0)
			{
				_Mappings[i]._ItemID = elementMatchGame.pBoosterIDMap[_Mappings[i]._Type];
				CommonInventoryData.pInstance.AddPurchaseItem(_Mappings[i]._ItemID, _Mappings[i]._Count, ItemPurchaseSource.BUY_POP_UP.ToString());
			}
		}
		CommonInventoryData.pInstance.DoPurchase(2, _StoreID, OnPurchaseDone);
	}

	public void OnPurchaseDone(CommonInventoryResponse ret)
	{
		SetState(KAUIState.INTERACTIVE);
		if (ret != null && ret.Success)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			UtDebug.Log("Purchase Successs");
			if (null != mTxtSyncStatus)
			{
				mTxtSyncStatus.SetText(_PurchaseSuccessText.GetLocalizedString());
			}
			if (_OnPurchaseSuccessful != null)
			{
				_OnPurchaseSuccessful();
			}
			Invoke("CloseDB", _SuccessMsgTimeOut);
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (null != mTxtSyncStatus)
			{
				mTxtSyncStatus.SetText(_PurchaseFailedText.GetLocalizedString());
			}
			mBtnCancel.SetVisibility(inVisible: true);
			UtDebug.LogError("Purchase Failed");
		}
	}

	public void CloseDB()
	{
		SetVisibility(isVisible: false);
		if (UiBuyPopup.OnBoosterBuyClose != null)
		{
			UiBuyPopup.OnBoosterBuyClose();
		}
	}

	private void Click(KAWidget inWidget)
	{
		mTotalGemCost = 0;
		for (int i = 0; i < _Mappings.Length; i++)
		{
			if (inWidget.name == _Mappings[i]._DecCount.name || inWidget.name == _Mappings[i]._IncCount.name)
			{
				_Mappings[i]._Count += ((!(inWidget.name == _Mappings[i]._DecCount.name)) ? 1 : (-1));
				_Mappings[i]._Count = Mathf.Clamp(_Mappings[i]._Count, 0, int.MaxValue);
				_Mappings[i]._QuantityTxt.SetText(_Mappings[i]._Count.ToString());
			}
			mTotalGemCost += _Mappings[i]._Count * _Mappings[i]._Cost;
		}
		mTotalPriceGems.SetText(mTotalGemCost.ToString());
	}

	private void OnNotEnoughGemsOK()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void OnIAPStoreClosed()
	{
		CloseDB();
	}
}
