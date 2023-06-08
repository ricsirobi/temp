using UnityEngine;

public class UiJournalCustomization : KAUI, IJournal
{
	public static bool _OpenedByExternal;

	public string _AvatarCustomizationBundlePath = "RS_DATA/PfUiAvatarCustomizationDO.unity3d/PfUiAvatarCustomizationDO";

	public string _DragonCustomizationBundlePath = "RS_DATA/PfUiDragonCustomizationDO.unity3d/PfUiDragonCustomizationDO";

	public int _TicketStoreID = 93;

	public int _DragonTicketItemID = 8227;

	private int mDragonTicketCost;

	private KAWidget mAvatarBtn;

	private KAWidget mDragonBtn;

	private KAWidget mStoreBtn;

	private KAWidget mTxtCoin;

	private KAWidget mTxtGem;

	private KAWidget mBlacksmithBtn;

	public LocaleString _NotEnoughVCashText = new LocaleString("You don't have enough Gems to customize your dragon. Do you want to buy more Gems?");

	public LocaleString _UseGemsForCustomizeText = new LocaleString("Dragon customization will cost you {cost} Gems. Do you want to continue?");

	public LocaleString _OfflineText = new LocaleString("You need to be connected to the Internet.");

	public LocaleString _StoreUnavailableText = new LocaleString("The store is unavailable at this time. Please try again later.");

	public LocaleString _TicketPurchaseFailed = new LocaleString("Transaction failed. Please try again.");

	public LocaleString _TicketPurchaseProcessing = new LocaleString("Processing purchase...");

	private KAUIGenericDB mKAUIGenericDB;

	public UiJournal _JournalUI;

	public StoreLoader.Selection _CoinsStoreInfo;

	public StoreLoader.Selection _VikingStoreInfo;

	[Header("BlackSmith button position")]
	public Vector2 _DragonUiPosition;

	public Vector2 _AvatarUiPosition;

	private Vector3 mPrevPosition = Vector3.zero;

	private GameObject mAvatarCustomizationUI;

	private UiDragonCustomization mDragonCustomizationUI;

	private bool mInitDragonTicketDB;

	private bool mIsProcessingClose;

	private bool mIsLoadingAsset;

	public bool pIsProcessingClose
	{
		set
		{
			mIsProcessingClose = value;
		}
	}

	public bool pIsLoadingAsset
	{
		set
		{
			mIsLoadingAsset = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		mAvatarBtn = FindItem("AvatarBtn");
		mDragonBtn = FindItem("DragonBtn");
		mStoreBtn = FindItem("StoreBtn");
		mBlacksmithBtn = FindItem("BlacksmithBtn");
		mTxtCoin = FindItem("TxtCoinAmount");
		mTxtGem = FindItem("TxtGemAmount");
		ItemStoreDataLoader.Load(_TicketStoreID, OnStoreLoaded);
	}

	public void OnStoreLoaded(StoreData sd)
	{
		ItemData itemData = sd.FindItem(_DragonTicketItemID);
		if (itemData != null)
		{
			mDragonTicketCost = itemData.FinalCashCost;
		}
	}

	public override void SetVisibility(bool isVisible)
	{
		base.SetVisibility(isVisible);
		if (isVisible)
		{
			if (!UtUtilities.IsConnectedToWWW())
			{
				mAvatarBtn.SetVisibility(inVisible: false);
				mDragonBtn.SetVisibility(inVisible: false);
				mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "ConnectOnline");
				mKAUIGenericDB._MessageObject = base.gameObject;
				mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
				mKAUIGenericDB.SetText(_OfflineText.GetLocalizedString(), interactive: false);
				mKAUIGenericDB._OKMessage = "OkConnect";
			}
			else
			{
				Money.AddNotificationObject(base.gameObject);
				SubstanceCustomization.Init("");
				if (UiJournal.SelectionWidget == "")
				{
					OnClick(mAvatarBtn);
				}
				else if (UiJournal.SelectionWidget == "CustomiseDragon")
				{
					OnClick(mDragonBtn);
				}
			}
		}
		else
		{
			DisableUI();
		}
	}

	public void OnMoneyUpdated()
	{
		mTxtCoin.SetText(Money.pGameCurrency.ToString());
		mTxtGem.SetText(Money.pCashCurrency.ToString());
	}

	private void OkConnect()
	{
		Object.Destroy(mKAUIGenericDB.gameObject);
	}

	public void ActivateUI(int uiIndex, bool addToList = true)
	{
	}

	public void Clear()
	{
		if (SubstanceCustomization.pInstance != null)
		{
			SubstanceCustomization.pInstance.SaveData();
		}
		if (UtPlatform.IsMobile() || UtPlatform.IsWSA())
		{
			if (mAvatarCustomizationUI != null)
			{
				UiAvatarCustomization component = mAvatarCustomizationUI.GetComponent<UiAvatarCustomization>();
				RsResourceManager.Unload(component._BoySubstanceAvatarPath);
				RsResourceManager.Unload(component._GirlSubstanceAvatarPath);
			}
			RsResourceManager.Unload(_AvatarCustomizationBundlePath);
			RsResourceManager.Unload(_DragonCustomizationBundlePath);
		}
		if (mAvatarCustomizationUI != null)
		{
			Object.Destroy(mAvatarCustomizationUI);
		}
		if (mDragonCustomizationUI != null)
		{
			Object.Destroy(mDragonCustomizationUI.gameObject);
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.enabled = true;
		}
		if (WsUserMessage.pInstance != null && _OpenedByExternal)
		{
			_OpenedByExternal = false;
			WsUserMessage.pInstance.OnClose();
		}
	}

	public void ProcessClose()
	{
		SetInteractive(interactive: false);
		if (mAvatarCustomizationUI != null)
		{
			UiAvatarCustomization component = mAvatarCustomizationUI.GetComponent<UiAvatarCustomization>();
			if (component != null)
			{
				mIsProcessingClose = true;
				component.SetInteractive(interactive: false);
				component.OnCloseCustomization(checkProcessing: true);
			}
		}
		if (mDragonCustomizationUI != null)
		{
			mDragonCustomizationUI.SetInteractive(interactive: false);
			mDragonCustomizationUI.OnCloseCustomization();
		}
	}

	public bool IsBusy()
	{
		if (!mIsLoadingAsset && !AvatarData.pIsSaving && !mIsProcessingClose)
		{
			return UiDragonCustomization.pIsBusy;
		}
		return true;
	}

	public void Exit()
	{
	}

	private void SetButtonVisibility(KAWidget inItem)
	{
		if (inItem == mAvatarBtn)
		{
			mBlacksmithBtn.SetPosition(_AvatarUiPosition.x, _AvatarUiPosition.y);
			mAvatarBtn.SetVisibility(inVisible: false);
			if (SanctuaryManager.pCurPetInstance != null)
			{
				mDragonBtn.SetVisibility(inVisible: true);
			}
			else
			{
				mDragonBtn.SetVisibility(inVisible: false);
			}
		}
		else if (inItem == mDragonBtn)
		{
			mBlacksmithBtn.SetPosition(_DragonUiPosition.x, _DragonUiPosition.y);
			mAvatarBtn.SetVisibility(inVisible: true);
			mDragonBtn.SetVisibility(inVisible: false);
		}
	}

	public override void OnClick(KAWidget inItem)
	{
		base.OnClick(inItem);
		if (inItem == mAvatarBtn)
		{
			if (mDragonCustomizationUI != null)
			{
				mDragonCustomizationUI.gameObject.SetActive(value: false);
			}
			if (mAvatarCustomizationUI == null)
			{
				if (MainStreetMMOClient.pInstance != null)
				{
					MainStreetMMOClient.pInstance.SetBusy(busy: true);
				}
				mIsLoadingAsset = true;
				mPrevPosition = AvAvatar.GetPosition();
				KAUICursorManager.SetDefaultCursor("Loading");
				SetDragonBtnState(inInteractive: false);
				string[] array = _AvatarCustomizationBundlePath.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAvatarCustomizationLoaded, typeof(GameObject));
			}
			else
			{
				mAvatarCustomizationUI.SetActive(value: true);
			}
			SetButtonVisibility(mAvatarBtn);
		}
		else if (inItem == mDragonBtn)
		{
			if (mDragonCustomizationUI == null)
			{
				StartNewDragonCustomization();
				return;
			}
			if (mAvatarCustomizationUI != null)
			{
				mAvatarCustomizationUI.SetActive(value: false);
			}
			mDragonCustomizationUI.gameObject.SetActive(value: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetButtonVisibility(mDragonBtn);
		}
		else if (inItem == mStoreBtn)
		{
			if (_VikingStoreInfo != null)
			{
				_JournalUI.PopUpStoreUI(_VikingStoreInfo._Store, _VikingStoreInfo._Category, "EquipBtn");
			}
		}
		else if (inItem != null && inItem.name == "GemTotalBtn")
		{
			IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
		}
		else if (inItem != null && inItem.name == "CoinTotalBtn")
		{
			if (_CoinsStoreInfo != null)
			{
				_JournalUI.PopUpStoreUI(_CoinsStoreInfo._Store, _CoinsStoreInfo._Category, "EquipBtn");
			}
		}
		else if (inItem == mBlacksmithBtn)
		{
			string selectionWidget = "";
			if (mAvatarCustomizationUI != null && mAvatarCustomizationUI.activeInHierarchy)
			{
				selectionWidget = mAvatarBtn.name;
			}
			else if (mDragonCustomizationUI != null && mDragonCustomizationUI.gameObject.activeInHierarchy)
			{
				selectionWidget = mDragonBtn.name;
			}
			if (_JournalUI != null)
			{
				_JournalUI.OpenBlacksmith(selectionWidget);
			}
		}
	}

	public bool IsDragonCustomizationLocked()
	{
		return CommonInventoryData.pInstance.GetQuantity(_DragonTicketItemID) <= 0;
	}

	public bool VerifyDragonCustomizationTicket(bool inStatus)
	{
		bool num = CommonInventoryData.pInstance.GetQuantity(_DragonTicketItemID) > 0;
		if (!num && !mInitDragonTicketDB)
		{
			if (Money.pCashCurrency < mDragonTicketCost)
			{
				KillGenericDB();
				mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
				mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
				mKAUIGenericDB.SetText(_NotEnoughVCashText.GetLocalizedString(), interactive: false);
				mKAUIGenericDB._MessageObject = base.gameObject;
				mKAUIGenericDB._YesMessage = "ProceedToStore";
				mKAUIGenericDB._NoMessage = "KillGenericDB";
				KAUI.SetExclusive(mKAUIGenericDB);
			}
			else
			{
				KillGenericDB();
				mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
				mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
				mKAUIGenericDB.SetText(_UseGemsForCustomizeText.GetLocalizedString().Replace("{cost}", mDragonTicketCost.ToString()), interactive: false);
				mKAUIGenericDB._MessageObject = base.gameObject;
				mKAUIGenericDB._YesMessage = "PurchaseDragonCustomizeTicket";
				mKAUIGenericDB._NoMessage = "KillGenericDB";
				KAUI.SetExclusive(mKAUIGenericDB);
			}
		}
		mInitDragonTicketDB = inStatus;
		return num;
	}

	private void StartNewDragonCustomization()
	{
		if (mAvatarCustomizationUI != null)
		{
			mAvatarCustomizationUI.SetActive(value: false);
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SetBusy(busy: true);
		}
		mIsLoadingAsset = true;
		KAUICursorManager.SetDefaultCursor("Loading");
		mAvatarBtn.SetState(KAUIState.NOT_INTERACTIVE);
		string[] array = _DragonCustomizationBundlePath.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnDragonCustomizationLoaded, typeof(GameObject));
		SetButtonVisibility(mDragonBtn);
	}

	private void PurchaseDragonCustomizeTicket()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		CommonInventoryData.pInstance.AddPurchaseItem(_DragonTicketItemID, 1, ItemPurchaseSource.DRAGON_CUSTOMIZATION.ToString());
		CommonInventoryData.pInstance.DoPurchase(2, _TicketStoreID, TicketPurchaseHandler);
		KillGenericDB();
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
		mKAUIGenericDB.SetText(_TicketPurchaseProcessing.GetLocalizedString(), interactive: false);
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void ProceedToStore()
	{
		KillGenericDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	public void OnIAPStoreClosed()
	{
	}

	private void KillGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	public void TicketPurchaseHandler(CommonInventoryResponse ret)
	{
		KillGenericDB();
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (ret == null || !ret.Success)
		{
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			mKAUIGenericDB.SetText(_TicketPurchaseFailed.GetLocalizedString(), interactive: false);
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._OKMessage = "KillGenericDB";
			KAUI.SetExclusive(mKAUIGenericDB);
		}
		if (mDragonCustomizationUI != null)
		{
			mDragonCustomizationUI.RefreshLockBtn(isUnlockedNow: true);
		}
	}

	public void OnAvatarCustomizationLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			mIsLoadingAsset = false;
			mAvatarCustomizationUI = Object.Instantiate((GameObject)inObject);
			mAvatarCustomizationUI.name = "PfUiAvatarCustomization";
			UiAvatarCustomization component = mAvatarCustomizationUI.GetComponent<UiAvatarCustomization>();
			if (component != null)
			{
				mIsLoadingAsset = true;
				component.pPrevPosition = mPrevPosition;
				component.pDefaultTabIndex = component.ClothesTabIndex;
				component.pUiJournalCustomization = this;
			}
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mIsLoadingAsset = false;
			Debug.LogError("Failed to load Avatar Equipment....");
			break;
		}
	}

	public void OnDragonCustomizationLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			mIsLoadingAsset = false;
			mAvatarBtn.SetState(KAUIState.INTERACTIVE);
			GameObject gameObject = Object.Instantiate((GameObject)inObject);
			gameObject.name = "PfUiDragonCustomization";
			mDragonCustomizationUI = gameObject.GetComponent<UiDragonCustomization>();
			mDragonCustomizationUI.pPetData = SanctuaryManager.pCurPetInstance.pData;
			mDragonCustomizationUI.SetUiForJournal(isJournal: true);
			mDragonCustomizationUI.pUiJournalCustomization = this;
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			mIsLoadingAsset = false;
			Debug.LogError("Failed to load Avatar Equipment....");
			break;
		}
	}

	private void OnPetPictureDone(object inPicture)
	{
		if (mDragonCustomizationUI != null)
		{
			mDragonCustomizationUI.OnPetPictureDone(inPicture);
		}
	}

	public void DragonCustomizationDone(bool status)
	{
		if (status)
		{
			CommonInventoryData.pInstance.RemoveItem(_DragonTicketItemID, updateServer: true);
			SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: true);
			SanctuaryManager.pInstance.pSetFollowAvatar = false;
		}
	}

	private void DisableUI()
	{
		if (mAvatarCustomizationUI != null)
		{
			mAvatarCustomizationUI.SetActive(value: false);
		}
		if (mDragonCustomizationUI != null)
		{
			mDragonCustomizationUI.gameObject.SetActive(value: false);
		}
	}

	public void SetDragonBtnState(bool inInteractive)
	{
		if (inInteractive)
		{
			mDragonBtn.SetState(KAUIState.INTERACTIVE);
		}
		else
		{
			mDragonBtn.SetState(KAUIState.NOT_INTERACTIVE);
		}
	}

	public void SetBlacksmithButtonVisibility(bool visible)
	{
		if (mBlacksmithBtn != null)
		{
			mBlacksmithBtn.SetVisibility(visible);
		}
	}
}
