using UnityEngine;

public class UiAgeUpBuy : KAUI
{
	[SerializeField]
	private LocaleString m_CongratulatonTitleText = new LocaleString("Congratulations!");

	[SerializeField]
	private LocaleString m_NotEnoughGemsText = new LocaleString("You do not have enough gems to pay, Please buy more!");

	[SerializeField]
	private LocaleString m_PurchaseConfirmationText = new LocaleString("Are you sure you want to purchase this Ageup?");

	[SerializeField]
	private LocaleString m_PurchaseProcessingText = new LocaleString("Processing purchase.");

	[SerializeField]
	private LocaleString m_PurchaseSuccessText = new LocaleString("Age Up purchase successful.");

	[SerializeField]
	private LocaleString m_PurchaseFailText = new LocaleString("Age Up purchase failed.");

	[SerializeField]
	private Texture m_TextureAdultUpgrade;

	[SerializeField]
	private Texture m_TextureTitanUpgrade;

	[SerializeField]
	private KAWidget m_TxtQuantity;

	[SerializeField]
	private KAWidget m_TxtTotalPrice;

	[SerializeField]
	private KAWidget m_BtnIncrementQuantity;

	[SerializeField]
	private KAWidget m_BtnDecrementQuantity;

	[SerializeField]
	private KAWidget m_BtnBuy;

	[SerializeField]
	private UITexture m_IcoDragonUpgrade;

	private UiDragonsAgeUp mUiDragonsAgeUp;

	private RaisedPetData mRaisedPetData;

	private PetAgeUpData mPetAgeData;

	private KAUIGenericDB mKAUIGenericDB;

	private int mItemCost;

	private int mQuantity;

	public void Init(UiDragonsAgeUp parent, RaisedPetData data, PetAgeUpData ageData)
	{
		mUiDragonsAgeUp = parent;
		mRaisedPetData = data;
		mPetAgeData = ageData;
		ItemData itemData = mUiDragonsAgeUp.pAgeUpStoreData.FindItem(mPetAgeData._AgeUpItemID);
		mItemCost = itemData.FinalCashCost;
		mQuantity = 1;
		m_TxtQuantity.SetText(mQuantity.ToString());
		m_TxtTotalPrice.SetText((mQuantity * mItemCost).ToString());
		if (ageData._ToPetStage == RaisedPetStage.ADULT)
		{
			m_IcoDragonUpgrade.mainTexture = m_TextureAdultUpgrade;
		}
		else
		{
			m_IcoDragonUpgrade.mainTexture = m_TextureTitanUpgrade;
		}
		SetVisibility(inVisible: true);
		KAUI.SetExclusive(this);
	}

	private void Exit()
	{
		mUiDragonsAgeUp.UpdateUpgradeAvailibility();
		KAUICursorManager.SetDefaultCursor("Arrow");
		KAUI.RemoveExclusive(this);
		SetVisibility(inVisible: false);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == _BackButtonName)
		{
			Exit();
		}
		else if (inWidget == m_BtnIncrementQuantity)
		{
			UpdateQuantityAndCost(increment: true);
		}
		else if (inWidget == m_BtnDecrementQuantity)
		{
			UpdateQuantityAndCost(increment: false);
		}
		else if (inWidget == m_BtnBuy)
		{
			BuyAgeUp();
		}
	}

	private void UpdateQuantityAndCost(bool increment)
	{
		if (increment)
		{
			mQuantity++;
		}
		else if (mQuantity > 1)
		{
			mQuantity--;
		}
		m_TxtQuantity.SetText(mQuantity.ToString());
		m_TxtTotalPrice.SetText((mQuantity * mItemCost).ToString());
	}

	private void BuyAgeUp()
	{
		if (Money.pCashCurrency >= int.Parse(m_TxtTotalPrice.GetText()))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			DoPurchase();
		}
		else
		{
			ShowKAUIDialog("PfKAUIGenericDB", "Insufficient Gems", "BuyGems", "DestroyDB", "", "", destroyDB: true, m_NotEnoughGemsText, base.gameObject);
		}
	}

	private void DoPurchase()
	{
		ShowKAUIDialog("PfKAUIGenericDB", "Purchase Progress", "", "", "", "", destroyDB: true, m_PurchaseProcessingText);
		CommonInventoryData.pInstance.AddPurchaseItem(mPetAgeData._AgeUpItemID, mQuantity, ItemPurchaseSource.DRAGON_AGE_UP.ToString());
		CommonInventoryData.pInstance.DoPurchase(2, mUiDragonsAgeUp.pAgeUpStoreData._ID, OnAgeUpPurchase);
	}

	public virtual void OnAgeUpPurchase(CommonInventoryResponse ret)
	{
		DestroyDB();
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (ret != null && ret.Success)
		{
			ShowKAUIDialog("PfKAUIGenericDB", "Purchase Success", "", "", "FinishPurchase", "", destroyDB: true, m_PurchaseSuccessText, base.gameObject);
		}
		else
		{
			ShowKAUIDialog("PfKAUIGenericDB", "Purchase Falied", "", "", "DestroyDB", "", destroyDB: true, m_PurchaseFailText, base.gameObject);
		}
	}

	public void ShowKAUIDialog(string assetName, string dbName, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, LocaleString localeString, GameObject msgObject = null)
	{
		if (mKAUIGenericDB != null)
		{
			DestroyDB();
		}
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB(assetName, dbName);
		if (mKAUIGenericDB != null)
		{
			if (msgObject == null)
			{
				msgObject = base.gameObject;
			}
			mKAUIGenericDB.SetMessage(msgObject, yesMessage, noMessage, okMessage, closeMessage);
			mKAUIGenericDB.SetDestroyOnClick(destroyDB);
			mKAUIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
			mKAUIGenericDB.SetTextByID(localeString._ID, localeString._Text, interactive: false);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	public void DestroyDB()
	{
		if (!(mKAUIGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	private void FinishPurchase()
	{
		DestroyDB();
		Exit();
		mUiDragonsAgeUp.pUiDragonAgeUpConfirm.Init(mUiDragonsAgeUp, mRaisedPetData, mPetAgeData);
	}

	private void BuyGems()
	{
		DestroyDB();
		Exit();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, mUiDragonsAgeUp.gameObject);
	}
}
