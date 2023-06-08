using System;
using System.Collections.Generic;
using UnityEngine;

public class UiDragonAgeUp : KAUI
{
	public delegate void OnUiDragonAgeUpClose();

	public delegate void OnUiDragonAgeUpBuy();

	public PetAgeUpData[] _AgeUpData;

	public int _AgeUpStoreID;

	public KAUIMenu _AgeUpItemMenu;

	public int _TitanRankId = 20;

	public LocaleString _CongratulatonTitleText = new LocaleString("Congratulations!");

	public LocaleString _NotEnoughGemsText = new LocaleString("You do not have enough gems to pay, Please buy more!");

	public LocaleString _PurchaseConfirmationText = new LocaleString("Are you sure you want to purchase this Ageup?");

	public LocaleString _PurchaseProcessingText = new LocaleString("Processing purchase.");

	public LocaleString _PurchaseSuccessText = new LocaleString("Age Up purchase successful.");

	public LocaleString _PurchaseFailText = new LocaleString("Age Up purchase failed.");

	private RaisedPetStage mFromStage;

	private RaisedPetStage[] mRequiredStages;

	private RaisedPetData mRaisedPetData;

	private KAUIGenericDB mKAUIGenericDB;

	private KAWidget mCloseBtn;

	private KAWidget mBuyBtn;

	protected UiDragonAgeUpItem mProcessedAgeItem;

	protected DragonAgeUpConfig.OnDragonAgeUpCancel mUiDragonAgeUpCancelCallback;

	protected DragonAgeUpConfig.OnDragonAgeUpDone mUiDragonAgeUpDoneCallback;

	protected OnUiDragonAgeUpBuy mUiDragonAgeUpBuyCallback;

	private bool mIsAgeUpDone;

	private int mTicketID;

	private bool mInitialize;

	protected bool mAllowUnmountablePet;

	protected GameObject mCallbackObj;

	protected bool mAllowTriggerAction;

	public RaisedPetData pRaisedPetData => mRaisedPetData;

	public void Init(DragonAgeUpConfig.OnDragonAgeUpCancel inCancelCallback, DragonAgeUpConfig.OnDragonAgeUpDone inCloseCallBack, OnUiDragonAgeUpBuy inBuyCallback, RaisedPetStage fromStage, RaisedPetData inData, RaisedPetStage[] toStages, bool isAgeUpDone, bool isUnmountableAllowed = false, GameObject callbackObj = null)
	{
		mUiDragonAgeUpCancelCallback = inCancelCallback;
		mUiDragonAgeUpDoneCallback = inCloseCallBack;
		mUiDragonAgeUpBuyCallback = inBuyCallback;
		mFromStage = fromStage;
		mRequiredStages = toStages;
		mRaisedPetData = inData;
		mIsAgeUpDone = isAgeUpDone;
		mAllowUnmountablePet = isUnmountableAllowed;
		mCallbackObj = callbackObj;
		mAllowTriggerAction = false;
		if (mIsAgeUpDone && mRaisedPetData.pStage == RaisedPetStage.TITAN)
		{
			PetRankData.LoadUserRank(mRaisedPetData, OnUserRankReady, forceLoad: true);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!mInitialize && mRaisedPetData != null)
		{
			mInitialize = true;
			SetInteractive(interactive: false);
			KAUI.SetExclusive(this);
			KAUICursorManager.SetDefaultCursor("Loading");
			mCloseBtn = FindItem("BtnClose");
			if (mCloseBtn != null)
			{
				mCloseBtn.SetVisibility(inVisible: true);
			}
			ItemStoreDataLoader.Load(_AgeUpStoreID, OnAgeUpStoreLoaded);
		}
	}

	protected virtual void OnAgeUpStoreLoaded(StoreData sd)
	{
		if (sd != null)
		{
			SanctuaryPetTypeInfo inTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(mRaisedPetData.PetTypeID);
			List<PetAgeUpData> ageUpData = GetAgeUpData(mFromStage);
			_AgeUpItemMenu.ClearItems();
			if (ageUpData.Count > 0)
			{
				foreach (PetAgeUpData tempAgeUpData in ageUpData)
				{
					ItemData itemData = sd.FindItem(tempAgeUpData._AgeUpItemID);
					if (itemData == null)
					{
						continue;
					}
					string attribute = itemData.GetAttribute("NewPetStage", string.Empty);
					RaisedPetStage raisedPetStage = RaisedPetStage.ADULT;
					if (!string.IsNullOrEmpty(attribute) && Enum.IsDefined(typeof(RaisedPetStage), attribute))
					{
						raisedPetStage = (RaisedPetStage)Enum.Parse(typeof(RaisedPetStage), attribute);
					}
					if (tempAgeUpData._ToPetStage != raisedPetStage)
					{
						continue;
					}
					UiDragonAgeUpItem uiDragonAgeUpItem = (UiDragonAgeUpItem)DuplicateWidget(tempAgeUpData._TemplateWidget);
					uiDragonAgeUpItem.name = itemData.ItemName;
					bool isAgeUpDone = false;
					if (mRequiredStages != null && mRequiredStages[0] != 0)
					{
						uiDragonAgeUpItem.pAgeUpDisabled = mIsAgeUpDone;
						bool flag = Array.Exists(mRequiredStages, (RaisedPetStage x) => x == tempAgeUpData._ToPetStage);
						if (mIsAgeUpDone)
						{
							isAgeUpDone = flag;
						}
						uiDragonAgeUpItem.pAgeUpDisabled = !flag;
					}
					uiDragonAgeUpItem.Init(inTypeInfo, tempAgeUpData, itemData.FinalCashCost, this, isAgeUpDone);
					_AgeUpItemMenu.AddWidget(uiDragonAgeUpItem);
					uiDragonAgeUpItem.SetVisibility(inVisible: true);
				}
			}
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (_AgeUpItemMenu.GetNumItems() == 0)
		{
			KAUI.RemoveExclusive(this);
			if (mUiDragonAgeUpDoneCallback != null)
			{
				mUiDragonAgeUpDoneCallback();
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			mCloseBtn.SetVisibility(inVisible: true);
			SetInteractive(interactive: true);
		}
	}

	public virtual List<PetAgeUpData> GetAgeUpData(RaisedPetStage fromStage)
	{
		List<PetAgeUpData> list = new List<PetAgeUpData>();
		if (_AgeUpData == null)
		{
			return null;
		}
		PetAgeUpData[] ageUpData = _AgeUpData;
		foreach (PetAgeUpData petAgeUpData in ageUpData)
		{
			if (petAgeUpData._FromPetStage == fromStage)
			{
				list.Add(petAgeUpData);
			}
		}
		return list;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCloseBtn)
		{
			mUiDragonAgeUpCancelCallback?.Invoke();
		}
		else if (inWidget == mBuyBtn)
		{
			mUiDragonAgeUpBuyCallback?.Invoke();
		}
		KAUI.RemoveExclusive(this);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void BuyAgeUp(UiDragonAgeUpItem inUiItem)
	{
		if (!mAllowTriggerAction)
		{
			mCallbackObj = null;
		}
		mBuyBtn = inUiItem;
		OnClick(mBuyBtn);
		UiDragonsAgeUp.Init(mUiDragonAgeUpDoneCallback, closeOnAgeUp: true, mCallbackObj);
	}

	public bool HasTicket(int inTicketID)
	{
		if (inTicketID > 0)
		{
			if (ParentData.pIsReady && ParentData.pInstance.HasItem(inTicketID))
			{
				return true;
			}
			if (CommonInventoryData.pIsReady && CommonInventoryData.pInstance.FindItem(inTicketID) != null)
			{
				return true;
			}
		}
		return false;
	}

	private void PurchaseAgeUp()
	{
		CommonInventoryData.pInstance.AddPurchaseItem(mProcessedAgeItem.pPetAgeUpData._AgeUpItemID, 1, ItemPurchaseSource.DRAGON_AGE_UP.ToString());
		CommonInventoryData.pInstance.DoPurchase(2, _AgeUpStoreID, AgeUpPurchaseDone);
		ShowKAUIDialog("PfKAUIGenericDB", "Purchase Progress", "", "", "", "", destroyDB: true, _PurchaseProcessingText);
	}

	public virtual void AgeUpPurchaseDone(CommonInventoryResponse ret)
	{
		DestroyDB();
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (ret != null && ret.Success)
		{
			ShowKAUIDialog("PfKAUIGenericDB", "Purchase Success", "", "", "DestroyDB", "", destroyDB: true, _PurchaseSuccessText, base.gameObject);
			PerformAgeUp(mProcessedAgeItem.pPetAgeUpData._AgeUpItemID);
		}
		else
		{
			ShowKAUIDialog("PfKAUIGenericDB", "Purchase Falied", "", "", "DestroyDB", "", destroyDB: true, _PurchaseFailText, base.gameObject);
		}
	}

	protected void PerformAgeUp(int inTicketID)
	{
		SanctuaryManager.pInstance.SetAge(mRaisedPetData, RaisedPetData.GetAgeIndex(mProcessedAgeItem.pPetAgeUpData._ToPetStage));
		CommonInventoryRequest[] array = new CommonInventoryRequest[1]
		{
			new CommonInventoryRequest()
		};
		array[0].ItemID = inTicketID;
		array[0].Quantity = -1;
		mRaisedPetData.SaveDataReal(OnSetAgeDone, array);
		mTicketID = inTicketID;
		SetInteractive(interactive: false);
		if (_AgeUpItemMenu != null)
		{
			_AgeUpItemMenu.SetInteractive(interactive: false);
		}
	}

	public void OnSetAgeDone(SetRaisedPetResponse response)
	{
		if (response != null && response.RaisedPetSetResult == RaisedPetSetResult.Success)
		{
			mFromStage = mProcessedAgeItem.pPetAgeUpData._ToPetStage;
			if (CommonInventoryData.pInstance.RemoveItem(mTicketID, updateServer: false) < 0)
			{
				if (ParentData.pInstance.pInventory.pData.RemoveItem(mTicketID, updateServer: false) >= 0)
				{
					ParentData.pInstance.pInventory.pData.ClearSaveCache();
				}
			}
			else
			{
				CommonInventoryData.pInstance.ClearSaveCache();
			}
			if (mProcessedAgeItem.pPetAgeUpData._ToPetStage == RaisedPetStage.TITAN)
			{
				UserAchievementTask.Set(SanctuaryManager.pInstance._DragonTitanAchievemetID);
			}
			PetRankData.LoadUserRank(mRaisedPetData, OnUserRankReady, forceLoad: true);
		}
		else
		{
			SanctuaryManager.pInstance.SetAge(mRaisedPetData, RaisedPetData.GetAgeIndex(mFromStage), inSave: false);
			SetInteractive(interactive: true);
		}
	}

	protected virtual void OnUserRankReady(UserRank rank, object userData)
	{
		RefreshItems();
		if (mRaisedPetData.pStage == RaisedPetStage.TITAN && rank.RankID < _TitanRankId)
		{
			UserAchievementTask.Set(SanctuaryManager.pInstance._DragonTitanAchievemetID);
			PetRankData.LoadUserRank(mRaisedPetData, OnUserRankReady, forceLoad: true);
		}
		else
		{
			ResetUI();
		}
		SetInteractive(interactive: true);
	}

	private void RefreshItems()
	{
		foreach (KAWidget item in _AgeUpItemMenu.GetItems())
		{
			UiDragonAgeUpItem component = item.GetComponent<UiDragonAgeUpItem>();
			if (component != null)
			{
				if (mProcessedAgeItem != null && component == mProcessedAgeItem)
				{
					component.UpdateWidget();
				}
				else
				{
					component.SetDisabled(isDisabled: true);
				}
			}
		}
	}

	public void ResetUI()
	{
		KAWidget kAWidget = FindItem("TxtTitle");
		if (kAWidget != null)
		{
			kAWidget.SetTextByID(_CongratulatonTitleText._ID, _CongratulatonTitleText._Text);
		}
		if (mCloseBtn != null)
		{
			mCloseBtn.SetVisibility(inVisible: true);
		}
		if (mProcessedAgeItem != null)
		{
			mProcessedAgeItem = null;
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
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	private void BuyGemsOnline()
	{
		DestroyDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}
}
