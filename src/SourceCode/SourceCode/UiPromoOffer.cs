using System;
using System.Collections.Generic;
using UnityEngine;

public class UiPromoOffer : KAUI
{
	public UiPromoOfferMenu _OfferMenu;

	public LocaleString _NotEnoughGemsText = new LocaleString("You dont have enough gems to purchase offer. Do you want to buy gems?");

	public LocaleString _NotEnoughCoinsText = new LocaleString("You dont have enough coins to purchase offer. Do you want to buy coins?");

	public LocaleString _PurchaseFailedText = new LocaleString("Your purchase failed, please try again later.");

	public LocaleString _SyncSuccessText = new LocaleString("Your purchase is completed and synced to Server");

	public LocaleString _PurchaseConfirmationText = new LocaleString("[REVIEW] Do you want to proceed to purchase this item?");

	public LocaleString _DaysText = new LocaleString("Days");

	public string _GemsSpriteName = "AniDWDragonsBaseCurrencyGems";

	public string _CoinsSpriteName = "AniDWDragonsBaseCurrencyCoins";

	public string _MysteryBoxBundlePath = "RS_DATA/PfUiMysteryBoxDO.unity3d/PfUiMysteryBoxDO";

	public bool _CycleOffers;

	protected KAWidget mCloseBtn;

	protected KAWidget mRegularPriceCoins;

	protected KAWidget mRegularPriceGems;

	protected KAWidget mPackagePrice;

	protected KAWidget mBuyBtn;

	protected KAWidget mPackageBanner;

	protected KAWidget mPackageBannerFull;

	protected KAWidget mTxtPackageDes;

	protected KAWidget mTxtPackageTitle;

	protected KAWidget mPageLeftBtn;

	protected KAWidget mPageRightBtn;

	protected KAWidget mTxtTimeLeft;

	protected KAWidget mIcoTimer;

	protected KAWidget mIcoPackagePrice;

	protected List<PromoPackage> mPackagesToShow = new List<PromoPackage>();

	protected ItemData mCurrentPackageItemData;

	protected KAUIGenericDB mGenericDB;

	protected TimeSpan mTimeLeft = TimeSpan.MaxValue;

	protected bool mTimerAvailable;

	protected bool mInitialized;

	protected float mTimer;

	protected int mPackageIndex = -1;

	protected int mRegularCostCoins;

	protected int mRegularCostGems;

	protected int mPetID = -1;

	protected string mUsergender = "U";

	protected string mPromoOfferStoreName = ItemPurchaseSource.PROMO_OFFER.ToString();

	private CommonInventoryResponseItem[] mPurchasedItems;

	protected override void Start()
	{
		base.Start();
		Init();
	}

	protected virtual void Init()
	{
		if (!mInitialized)
		{
			mInitialized = true;
			mCloseBtn = FindItem("CloseBtn");
			mRegularPriceCoins = FindItem("TxtRegPriceCoins");
			mRegularPriceGems = FindItem("TxtRegPriceGems");
			mPackagePrice = FindItem("TxtPackagePrice");
			mBuyBtn = FindItem("BuyBtn");
			mPageLeftBtn = FindItem("PageLeftBtn");
			mPageRightBtn = FindItem("PageRightBtn");
			mPageLeftBtn.SetVisibility(inVisible: false);
			mPageRightBtn.SetVisibility(inVisible: false);
			mTxtTimeLeft = FindItem("TxtTimer");
			mIcoTimer = FindItem("IcoTimer");
			mPackageBanner = FindItem("PackageBanner");
			mPackageBannerFull = FindItem("PackageBannerFull");
			mTxtPackageDes = FindItem("TxtDescription");
			mTxtPackageTitle = FindItem("TxtTitle");
			mIcoPackagePrice = FindItem("IcoPackagePrice");
			mPageRightBtn.SetVisibility(mPackagesToShow.Count > 1);
			mPageLeftBtn.SetVisibility(mPackagesToShow.Count > 1);
			if (SanctuaryManager.pCurPetData != null)
			{
				mPetID = SanctuaryManager.pCurPetData.PetTypeID;
			}
			mUsergender = "U";
			switch (AvatarData.GetGender())
			{
			case Gender.Female:
				mUsergender = "F";
				break;
			case Gender.Male:
				mUsergender = "M";
				break;
			}
		}
	}

	public virtual void OnDataReady(List<ItemData> itemDataList, int inItemID)
	{
		SetRegularPrice(itemDataList);
		string sn = "";
		if (mCurrentPackageItemData.Rollover != null)
		{
			sn = mCurrentPackageItemData.Rollover.Bundle + "/" + mCurrentPackageItemData.Rollover.DialogName;
		}
		CoBundleItemData coBundleItemData = new CoBundleItemData(mCurrentPackageItemData.IconName, sn);
		if (mPackageBanner != null && mPackageBanner.GetVisibility())
		{
			mPackageBanner.SetUserData(coBundleItemData);
		}
		else if (mPackageBannerFull != null && mPackageBannerFull.GetVisibility())
		{
			mPackageBannerFull.SetUserData(coBundleItemData);
		}
		coBundleItemData.LoadResource();
		SetPackageInfo();
		ShowTimeLeft();
	}

	protected void SetRegularPrice(List<ItemData> itemDataList)
	{
		mRegularCostCoins = 0;
		mRegularCostGems = 0;
		if (itemDataList != null)
		{
			foreach (ItemData itemData in itemDataList)
			{
				if (IsItemRelevantToUser(itemData))
				{
					CalculateCost(itemData);
				}
			}
		}
		else if (IsItemRelevantToUser(mCurrentPackageItemData))
		{
			CalculateCost(mCurrentPackageItemData);
		}
		if (mRegularPriceCoins != null)
		{
			mRegularPriceCoins.SetText(mRegularCostCoins.ToString());
		}
		if (mRegularPriceGems != null)
		{
			mRegularPriceGems.SetText(mRegularCostGems.ToString());
		}
	}

	protected void SetPackageInfo()
	{
		mTxtPackageDes.SetText("");
		mTxtPackageTitle.SetText("");
		mPackagePrice.SetText("");
		mIcoPackagePrice.SetVisibility(mCurrentPackageItemData != null);
		if (mCurrentPackageItemData != null)
		{
			mTxtPackageDes.SetText(mCurrentPackageItemData.Description);
			mTxtPackageTitle.SetText(mCurrentPackageItemData.ItemName);
			mIcoPackagePrice.SetSprite((mCurrentPackageItemData.GetPurchaseType() == 2) ? _GemsSpriteName : _CoinsSpriteName);
			mPackagePrice.SetText((mCurrentPackageItemData.GetPurchaseType() == 2) ? mCurrentPackageItemData.FinalCashCost.ToString() : mCurrentPackageItemData.FinalCost.ToString());
		}
	}

	protected bool IsItemRelevantToUser(ItemData itemData)
	{
		if (itemData.HasAttribute("PetTypeID"))
		{
			if (mPetID == itemData.GetAttribute("PetTypeID", -1))
			{
				return true;
			}
			return false;
		}
		if (itemData.HasAttribute("Gender"))
		{
			if (mUsergender == itemData.GetAttribute("Gender", "U"))
			{
				return true;
			}
			return false;
		}
		return true;
	}

	protected void CalculateCost(ItemData itemData)
	{
		switch (itemData.GetPurchaseType())
		{
		case 1:
			mRegularCostCoins += itemData.FinalCost;
			break;
		case 2:
			mRegularCostGems += itemData.FinalCashCost;
			break;
		}
	}

	protected virtual void Exit()
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mCloseBtn)
		{
			Exit();
		}
		else if (item == mBuyBtn)
		{
			ProcessPurchase();
		}
		else if (item == mPageRightBtn)
		{
			ShowNextOffer(mPackageIndex + 1);
		}
		else if (item == mPageLeftBtn)
		{
			ShowNextOffer(mPackageIndex - 1);
		}
	}

	public override void OnSwipe(KAWidget inSwipedWidget, Vector2 inSwipedTotalDelta)
	{
		base.OnSwipe(inSwipedWidget, inSwipedTotalDelta);
		if (inSwipedWidget == mPackageBanner || inSwipedWidget == mPackageBannerFull)
		{
			ShowNextOffer((inSwipedTotalDelta.x > 0f) ? (mPackageIndex - 1) : (mPackageIndex + 1));
		}
	}

	protected void ShowNextOffer(int offerIndex)
	{
		if (_CycleOffers)
		{
			if (offerIndex < 0)
			{
				offerIndex = mPackagesToShow.Count - 1;
			}
			else if (offerIndex >= mPackagesToShow.Count)
			{
				offerIndex = 0;
			}
			mPageRightBtn.SetDisabled(mPackagesToShow.Count == 1);
			mPageLeftBtn.SetDisabled(mPackagesToShow.Count == 1);
		}
		else
		{
			if (offerIndex < 0)
			{
				offerIndex = 0;
			}
			else if (offerIndex >= mPackagesToShow.Count)
			{
				offerIndex = mPackagesToShow.Count - 1;
			}
			mPageRightBtn.SetDisabled(offerIndex == mPackagesToShow.Count - 1);
			mPageLeftBtn.SetDisabled(offerIndex == 0);
		}
		if (mPackageIndex != offerIndex)
		{
			mPackageIndex = offerIndex;
			ShowCurrentOffer();
		}
	}

	protected void ProcessPurchase()
	{
		if (mCurrentPackageItemData != null && mCurrentPackageItemData.pBundledItems != null && mCurrentPackageItemData.pBundledItems.Count > 0)
		{
			UiBundleConfirmation.Init(ConfirmBundleStatus, mCurrentPackageItemData);
		}
		else
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _PurchaseConfirmationText.GetLocalizedString(), "", base.gameObject, "BuyItem", "CancelPurchase", "", "", inDestroyOnClick: true);
		}
	}

	private void ConfirmBundleStatus(UiBundleConfirmation.Status status)
	{
		switch (status)
		{
		case UiBundleConfirmation.Status.Accepted:
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _PurchaseConfirmationText.GetLocalizedString(), "", base.gameObject, "BuyItem", "CancelPurchase", "", "", inDestroyOnClick: true);
			break;
		case UiBundleConfirmation.Status.Confirmed:
			BuyItem();
			break;
		}
	}

	private void BuyItem()
	{
		if (mCurrentPackageItemData == null)
		{
			return;
		}
		if (mCurrentPackageItemData.GetPurchaseType() == 2)
		{
			if (Money.pCashCurrency < mCurrentPackageItemData.FinalCashCost)
			{
				ShowGenericDBCurrency(_NotEnoughGemsText, "BuyGems");
				return;
			}
		}
		else if (Money.pGameCurrency < mCurrentPackageItemData.FinalCost)
		{
			ShowGenericDBCurrency(_NotEnoughCoinsText, "BuyCoins");
			return;
		}
		if (int.TryParse(mPackagesToShow[mPackageIndex].StoreID, out var result))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			SetState(KAUIState.NOT_INTERACTIVE);
			mPurchasedItems = null;
			CommonInventoryData.pInstance.AddPurchaseItem(mPackagesToShow[mPackageIndex].ItemID, 1, mPromoOfferStoreName);
			CommonInventoryData.pInstance.DoPurchase(mCurrentPackageItemData.GetPurchaseType(), result, OnPurchaseComplete);
		}
	}

	public void OnPurchaseComplete(CommonInventoryResponse ret)
	{
		if (ret != null && ret.Success)
		{
			mPackagesToShow[mPackageIndex].UserPurchasedOffer();
			mPackagesToShow.Remove(mPackagesToShow[mPackageIndex]);
			ShowGenericDBPurchase(_SyncSuccessText);
			mPurchasedItems = ret.CommonInventoryIDs;
		}
		else
		{
			ShowGenericDBPurchase(_PurchaseFailedText);
		}
	}

	public virtual void PackagesToShow(List<PromoPackage> packages)
	{
		mPackagesToShow = packages;
		Init();
		ShowNextOffer(0);
	}

	protected virtual void ShowCurrentOffer()
	{
		mCurrentPackageItemData = null;
		if (mRegularPriceCoins != null)
		{
			mRegularPriceCoins.SetText("");
		}
		if (mRegularPriceGems != null)
		{
			mRegularPriceGems.SetText("");
		}
		_OfferMenu.ClearItems();
		if (mPackageBannerFull != null && mPackageBanner != null)
		{
			mPackageBanner.SetVisibility(!mPackagesToShow[mPackageIndex].FullView);
			mPackageBannerFull.SetVisibility(mPackagesToShow[mPackageIndex].FullView);
		}
		if (mPackagesToShow[mPackageIndex].ItemID > 0 && int.TryParse(mPackagesToShow[mPackageIndex].StoreID, out var result))
		{
			ItemStoreDataLoader.Load(result, OnStoreLoaded);
		}
		else
		{
			OnDataReady(null, 0);
		}
	}

	private void OnStoreLoaded(StoreData sd)
	{
		if (sd != null && sd._Items != null && sd._Items.Length != 0)
		{
			ItemData itemData = sd.FindItem(mPackagesToShow[mPackageIndex].ItemID);
			if (itemData != null)
			{
				PromoItemReady(mPackagesToShow[mPackageIndex].ItemID, itemData, null);
				return;
			}
		}
		OnDataReady(null, 0);
	}

	protected virtual void PromoItemReady(int itemId, ItemData itemData, object userData)
	{
		mCurrentPackageItemData = itemData;
		itemData.LoadBundledItems(PromoItemList);
	}

	private void PromoItemList(List<ItemData> itemData, int inItemID)
	{
		OnDataReady(itemData, inItemID);
	}

	private void ShowGenericDBCurrency(LocaleString message, string callback)
	{
		if (mGenericDB == null)
		{
			mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "BuyCurrency");
			mGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			mGenericDB.SetMessage(base.gameObject, callback, "OnCloseDB", "", "");
			mGenericDB.SetText(message.GetLocalizedString(), interactive: false);
			KAUI.SetExclusive(mGenericDB);
		}
	}

	private void ShowGenericDBPurchase(LocaleString message)
	{
		if (mGenericDB == null)
		{
			mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "PurchaseResult");
			mGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			mGenericDB.SetMessage(base.gameObject, "", "", "OnPurchaseClose", "");
			mGenericDB.SetText(message.GetLocalizedString(), interactive: false);
			KAUI.SetExclusive(mGenericDB);
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	private void OnCloseDB()
	{
		if (mGenericDB != null)
		{
			KAUI.RemoveExclusive(mGenericDB);
			UnityEngine.Object.Destroy(mGenericDB.gameObject);
		}
	}

	private void BuyGems()
	{
		OnCloseDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void BuyCoins()
	{
		OnCloseDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.COINS, base.gameObject);
	}

	public void OnIAPStoreClosed()
	{
	}

	protected virtual void ShowTimeLeft()
	{
		if (mPackagesToShow[mPackageIndex].Duration.HasValue)
		{
			mTimerAvailable = true;
			string offerStartTime = mPackagesToShow[mPackageIndex].GetOfferStartTime();
			if (offerStartTime != null)
			{
				DateTime result = DateTime.MinValue;
				DateTime minValue = DateTime.MinValue;
				DateTime pCurrentTime = ServerTime.pCurrentTime;
				if (DateTime.TryParse(offerStartTime, out result))
				{
					mTimeLeft = result.AddHours(mPackagesToShow[mPackageIndex].Duration.Value).Subtract(pCurrentTime);
				}
				else
				{
					mTimeLeft = new TimeSpan(0, 0, 0);
					UtDebug.LogError("Could not read xml");
				}
			}
		}
		else
		{
			mTimerAvailable = false;
		}
		mTxtTimeLeft.SetVisibility(mTimerAvailable);
		mIcoTimer.SetVisibility(mTimerAvailable);
	}

	protected override void Update()
	{
		base.Update();
		if (!mTimerAvailable)
		{
			return;
		}
		mTimer += Time.deltaTime;
		if (mTimer > 1f)
		{
			mTimer = 0f;
			mTimeLeft -= TimeSpan.FromSeconds(1.0);
			if (mTimeLeft <= TimeSpan.Zero)
			{
				mTimeLeft = TimeSpan.Zero;
			}
		}
		CalculateTime(mTimeLeft);
	}

	private void CalculateTime(TimeSpan time)
	{
		int days = time.Days;
		int hours = time.Hours;
		int minutes = time.Minutes;
		int seconds = time.Seconds;
		string text = "";
		text = ((days < 1) ? (hours.ToString("d2") + ":" + minutes.ToString("d2") + ":" + seconds.ToString("d2")) : (Mathf.CeilToInt((float)time.TotalDays).ToString("d2") + " " + _DaysText.GetLocalizedString()));
		mTxtTimeLeft.SetText(text);
	}

	private void OnPurchaseClose()
	{
		OnCloseDB();
		SetState(KAUIState.INTERACTIVE);
		if (mCurrentPackageItemData.IsPrizeItem() && mPurchasedItems != null && !string.IsNullOrEmpty(_MysteryBoxBundlePath))
		{
			OnMysteryBoxPurchased(mPurchasedItems);
		}
		else
		{
			ProcessPurchaseDone();
		}
	}

	private void ProcessPurchaseDone()
	{
		_OfferMenu.ClearItems();
		mPurchasedItems = null;
		if (mPackagesToShow.Count > 0)
		{
			mPageRightBtn.SetVisibility(mPackagesToShow.Count > 1);
			mPageLeftBtn.SetVisibility(mPackagesToShow.Count > 1);
			mPackageIndex = 0;
			ShowCurrentOffer();
		}
		else
		{
			OnClick(mCloseBtn);
		}
	}

	private void OnMysteryBoxPurchased(CommonInventoryResponseItem[] finalPrizes)
	{
		KAUIStore.EnableStoreUIs(inEnable: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = _MysteryBoxBundlePath.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], MysteryBoxUiHandler, typeof(GameObject), inDontDestroy: false, finalPrizes);
	}

	private void MysteryBoxUiHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiMysteryBox";
			obj.GetComponent<UiMysteryBox>().OpenMysteryBox(mCurrentPackageItemData.ItemID, (CommonInventoryResponseItem[])inUserData, base.gameObject);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			ProcessPurchaseDone();
			break;
		}
	}

	private void OnMysteryBoxClosed()
	{
		ProcessPurchaseDone();
	}
}
