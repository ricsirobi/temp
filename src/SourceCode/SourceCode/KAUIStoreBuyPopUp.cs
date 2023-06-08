using UnityEngine;

public class KAUIStoreBuyPopUp : KAUIPopup
{
	public LocaleString _InitialStatusText = new LocaleString("Please wait while your purchase is being processed.");

	public LocaleString _PurchaseFailedText = new LocaleString("Your purchase failed, please try again later.");

	public LocaleString _PurchaseSuccessText = new LocaleString("Your purchase is completed.");

	public LocaleString _FreeText = new LocaleString("Free");

	protected KAUIStore mStoreUI;

	protected KAWidget mItemName;

	protected KAWidget mNonMemberPrice;

	protected KAWidget mMemberPrice;

	protected KAWidget mQuantityPlus;

	protected KAWidget mQuantityMinus;

	protected KAWidget mTotalPrice;

	protected KAWidget mSavings;

	protected KAWidget mSavingsLabel;

	protected KAWidget mSavingsMissedLabel;

	protected KAWidget mBtnBuy;

	protected KAWidget mBtnClose;

	protected KAWidget mNonMemberPriceCross;

	protected KAWidget mMember;

	protected KAWidget mSavingsMissed;

	protected KAEditBox mQuantity;

	protected KAWidget mBattleSlots;

	protected KAWidget mOccupiedBattleSlots;

	private int mItemQuantity = 1;

	private KAStoreItemData mItemData;

	private ItemData.ItemSale mCurSale;

	private InventorySetting.TabData mTabData;

	protected override void Start()
	{
		base.Start();
		mStoreUI = base.transform.parent.GetComponentInChildren<KAUIStore>();
		mItemName = FindItem("TxtTitle");
		KAWidget kAWidget = FindItem("NonMember");
		mNonMemberPrice = kAWidget.FindChildItem("ItemCostNonMember");
		mMember = FindItem("Member");
		mMemberPrice = mMember.FindChildItem("ItemCostMember");
		KAWidget kAWidget2 = FindItem("Quantity");
		mQuantity = (KAEditBox)kAWidget2.FindChildItem("TxtQuantityValue");
		mQuantityPlus = kAWidget2.FindChildItem("btnPositive");
		mQuantityMinus = kAWidget2.FindChildItem("btnNegative");
		mTotalPrice = FindItem("TotalPrice").FindChildItem("ItemCostTotalPrice");
		mSavingsMissed = FindItem("SavingsMissed");
		mSavings = mSavingsMissed.FindChildItem("ItemCostSavingsMissed");
		mSavingsLabel = mSavingsMissed.FindChildItem("TxtSavings");
		mSavingsMissedLabel = mSavingsMissed.FindChildItem("TxtSavingsMissed");
		mNonMemberPriceCross = mNonMemberPrice.FindChildItem("AnimCross");
		mBattleSlots = FindItem("BattleReadySlots");
		mOccupiedBattleSlots = mBattleSlots.FindChildItem("OccupiedSlots");
		mBtnBuy = FindItem("BtnBuy");
		mBtnClose = FindItem("BtnClose");
	}

	protected override void Update()
	{
		base.Update();
		if (!GetVisibility() || mItemData == null || mItemData._ItemData == null)
		{
			return;
		}
		if (mItemData._ItemData.IsOutdated() || mStoreUI == null || mStoreUI.pFilter == null || !mStoreUI.pFilter.CanFilter(mItemData._ItemData))
		{
			SetVisibility(t: false);
			return;
		}
		ItemData.ItemSale currentSale = mItemData._ItemData.GetCurrentSale();
		if ((currentSale == null && mCurSale != null) || (currentSale != null && mCurSale == null) || (currentSale != null && mCurSale != null && currentSale.mModifier != mCurSale.mModifier))
		{
			RefreshValues();
		}
	}

	public override void SetVisibility(bool t)
	{
		if (t)
		{
			KAUI.SetExclusive(this, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		}
		else if (GetVisibility())
		{
			KAUI.RemoveExclusive(this);
			if (mStoreUI.pMainMenu != null)
			{
				mStoreUI.pMainMenu.SetState(KAUIState.INTERACTIVE);
			}
			if (mStoreUI.pCategoryMenu != null)
			{
				mStoreUI.pCategoryMenu.SetState(KAUIState.INTERACTIVE);
			}
			if (mStoreUI.pChooseMenu != null)
			{
				mStoreUI.pChooseMenu.SetState(KAUIState.INTERACTIVE);
			}
			mStoreUI.SetState(KAUIState.INTERACTIVE);
		}
		base.SetVisibility(t);
	}

	public void SetItemData(KAStoreItemData itemData, int qty)
	{
		mItemQuantity = qty;
		mItemData = itemData;
		if (mItemData == null)
		{
			UtDebug.LogError("Item user data missing");
			return;
		}
		RefreshValues();
		if (mStoreUI.pChooseUI.pChosenItemData._ItemData.IsStatAvailable())
		{
			if (mBattleSlots != null)
			{
				mBattleSlots.SetVisibility(inVisible: true);
			}
			if (InventorySetting.pInstance != null)
			{
				UpdateBattleSlotsStatus();
			}
		}
		else if (mBattleSlots != null)
		{
			mBattleSlots.SetVisibility(inVisible: false);
		}
	}

	private void RefreshValues()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = CommonInventoryData.pInstance.FindItem(mItemData._ItemData.ItemID)?.Quantity ?? 0;
		if (mItemData._ItemData.InventoryMax < 0)
		{
			num5 = (int)Mathf.Pow(10f, mQuantity.pInput.characterLimit) - 1;
		}
		else if (mItemData._ItemData.InventoryMax > 0)
		{
			num5 = mItemData._ItemData.InventoryMax - num6;
		}
		if (mItemQuantity > num5)
		{
			mItemQuantity = num5;
		}
		if (mItemData._ItemData != null)
		{
			mCurSale = mItemData._ItemData.GetCurrentSale();
		}
		else
		{
			mCurSale = null;
		}
		if (SubscriptionInfo.pIsMember)
		{
			num = mItemData._ItemData.GetFinalCost();
			num2 = mItemData._ItemData.GetFinalNonMemberCost();
			if (num2 == -1)
			{
				num2 = num;
			}
			num3 = num * mItemQuantity;
			num4 = num2 * mItemQuantity - num3;
		}
		else
		{
			num2 = mItemData._ItemData.GetFinalCost();
			num = mItemData._ItemData.GetFinalMemberCost();
			if (num == -1)
			{
				num = num2;
			}
			num3 = num2 * mItemQuantity;
			num4 = ((!mItemData._ItemData.HasCategory(514)) ? (num3 - num * mItemQuantity) : num3);
		}
		mSavingsLabel.SetVisibility(SubscriptionInfo.pIsMember);
		mSavingsMissedLabel.SetVisibility(!SubscriptionInfo.pIsMember);
		mNonMemberPrice.SetText(num2.ToString());
		if (!SubscriptionInfo.pIsMember && mItemData._ItemData.HasCategory(514))
		{
			mMemberPrice.SetText(_FreeText.GetLocalizedString());
		}
		else
		{
			mMemberPrice.SetText(num.ToString());
		}
		mItemName.SetText(mItemData._ItemData.ItemName);
		mQuantity.SetText(mItemQuantity.ToString());
		mTotalPrice.SetText(num3.ToString());
		mSavings.SetText(num4.ToString());
		mMember.SetVisibility(mStoreUI.pCategoryMenu.pType != KAStoreMenuItemData.StoreType.IAPStore);
		mSavingsMissed.SetVisibility(mStoreUI.pCategoryMenu.pType != KAStoreMenuItemData.StoreType.IAPStore);
		mNonMemberPriceCross.SetVisibility(mStoreUI.pCategoryMenu.pType != 0 && SubscriptionInfo.pIsMember);
		bool flag = mItemData._ItemData.HasCategory(657) && ItemCustomizationSettings.pInstance.MultiItemPays(mItemData._ItemData);
		mQuantityPlus.SetDisabled(flag || mItemQuantity >= num5);
		mQuantityMinus.SetDisabled(flag || mItemQuantity <= 1);
		mQuantity.SetDisabled(flag || num5 == 1);
	}

	public void UpdateBattleSlotsStatus()
	{
		mTabData = InventorySetting.pInstance.GetTabData("BattleReadyItems");
		if (mTabData != null)
		{
			int occupiedSlots = mTabData.GetOccupiedSlots();
			int totalSlots = mTabData.GetTotalSlots();
			if (mOccupiedBattleSlots != null)
			{
				mOccupiedBattleSlots.SetText(occupiedSlots + "/" + totalSlots);
			}
		}
	}

	private void OnItemPurchaseComplete()
	{
		UpdateBattleSlotsStatus();
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mBtnClose)
		{
			mTabData = null;
			SetVisibility(t: false);
		}
		else if (item == mQuantityPlus)
		{
			mItemQuantity++;
			RefreshValues();
		}
		else if (item == mQuantityMinus)
		{
			if (mItemQuantity > 1)
			{
				mItemQuantity--;
				RefreshValues();
			}
		}
		else
		{
			if (!(item == mBtnBuy) || mStoreUI.pChooseUI.pChosenItemData == null)
			{
				return;
			}
			if (mTabData != null && mTabData.GetOccupiedSlots() + mItemQuantity > mTabData.GetTotalSlots())
			{
				mTabData.BuySlot(base.gameObject);
				return;
			}
			if (InteractiveTutManager._CurrentActiveTutorialObject != null)
			{
				string value = "Buy_Confirm_" + mStoreUI.pChooseUI.pChosenItemData._ItemData.ItemID;
				InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", value);
			}
			mStoreUI.pChooseUI.BuyCurrentItem(mItemQuantity);
			SetVisibility(t: false);
			LocaleString message = null;
			if (mStoreUI._CurrentPurchaseItem.HasEnoughCurrency(ref message))
			{
				mStoreUI.SetStoreMode(KAUIStore.StoreMode.Syncing, update: true);
				return;
			}
			if (mStoreUI._CurrentPurchaseItem._ItemData.GetPurchaseType() == 2)
			{
				mStoreUI.ShowInsufficientGemsDB();
				return;
			}
			mStoreUI.ShowDialog(message._ID, message._Text);
			mStoreUI._CurrentPurchaseItem = null;
		}
	}

	public override void OnSelect(KAWidget widget, bool inSelected)
	{
		base.OnSelect(widget, inSelected);
		if (!inSelected && widget == mQuantity)
		{
			if (string.IsNullOrEmpty(mQuantity.GetText()))
			{
				mItemQuantity = 1;
			}
			RefreshValues();
		}
	}

	public override void OnInput(KAWidget inWidget, string inText)
	{
		base.OnInput(inWidget, inText);
		if (inWidget == mQuantity && !string.IsNullOrEmpty(mQuantity.GetText()))
		{
			mItemQuantity = int.Parse(mQuantity.GetText());
			RefreshValues();
		}
	}
}
