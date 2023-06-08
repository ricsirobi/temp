using UnityEngine;

public class UiMOBASelectDragon : KAUI
{
	public int _StoreID = 105;

	public int _MinAgeToFlyMode = 3;

	public LocaleString _NoDragonMessageText = new LocaleString("You need your dragon. Follow the quests to get your own dragon from the Hatchery!");

	public LocaleString _DragonNotAgedText = new LocaleString("Your dragon needs to be Short Wing stage to enter.");

	public LocaleString _DragonNotPurchasedText = new LocaleString("Would you unlock {{name}} and the {{type}} levels for {{amount}} gems.");

	public LocaleString _DragonComingSoonText = new LocaleString("You have wait a bit to play with this Dragon.");

	public LocaleString _PurchaseSuccessText = new LocaleString("Hero Dragon purchase successful.");

	public LocaleString _PurchaseFailText = new LocaleString("Hero Dragon purchase failed.");

	public LocaleString _NotEnoughFeeText = new LocaleString("You do not have enough gems to pay, Please buy more!");

	public LocaleString _PurchaseProcessingText = new LocaleString("Processing purchase.");

	public KAWidget _PlayerTemplate;

	public Texture _NoPetTexture;

	private StoreData mStoreData;

	private UiMOBASelectDragonMenu mMenu;

	private AvPhotoManager mPhotoManager;

	private KAUIGenericDB mUiGenericDB;

	private bool mIsInitialized;

	private int mSelectedTicketID;

	public StoreData pStoreData => mStoreData;

	protected override void Start()
	{
		base.Start();
		mMenu = (UiMOBASelectDragonMenu)GetMenu("UiMOBASelectDragonMenu");
	}

	public void Init()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		ItemStoreDataLoader.Load(_StoreID, OnStoreLoaded);
		mIsInitialized = false;
	}

	protected override void Update()
	{
		base.Update();
		if (!mIsInitialized && mStoreData != null)
		{
			SetVisibility(inVisible: true);
			InitDragonCards();
			mIsInitialized = true;
		}
		if (!Application.isEditor || !KAInput.GetKeyUp(KeyCode.J))
		{
			return;
		}
		for (int i = 1; i < mMenu.GetNumItems(); i++)
		{
			ItemData itemData = mStoreData._Items[i - 1];
			itemData.CashCost = 1000;
			KAWidget itemAt = mMenu.GetItemAt(i);
			KAWidget kAWidget = itemAt.FindChildItem("BkgComingSoon");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			itemAt.SetUserDataInt(itemData.ItemID);
		}
	}

	public void OnStoreLoaded(StoreData sd)
	{
		mStoreData = sd;
		if (sd == null || sd._Items == null || sd._Items.Length == 0)
		{
			Debug.Log("Storedata is not added");
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
	}

	private void AddPlayerCard()
	{
		KAWidget kAWidget = null;
		kAWidget = ((!(_PlayerTemplate != null)) ? mMenu._Template : _PlayerTemplate);
		if (kAWidget == null)
		{
			return;
		}
		KAWidget kAWidget2 = DuplicateWidget(kAWidget);
		KAWidget kAWidget3 = kAWidget2.FindChildItem("BtnDWDragonsFlight");
		if (kAWidget3 != null)
		{
			kAWidget3.SetVisibility(inVisible: true);
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(0);
			AddDragonDetails(kAWidget2, heroDragonFromID);
		}
		kAWidget2.SetVisibility(inVisible: true);
		kAWidget2.name = "Player";
		mMenu.AddWidgetAt(0, kAWidget2);
		if (SanctuaryManager.pCurPetInstance != null)
		{
			TakePetAShot();
		}
		else if (_NoPetTexture != null)
		{
			KAWidget kAWidget4 = mMenu.FindItem("Player");
			KAWidget kAWidget5 = kAWidget4.FindChildItem("IcoDragonFlightTexture");
			if (kAWidget5 != null)
			{
				kAWidget5.SetTexture(_NoPetTexture);
			}
			kAWidget3 = kAWidget4.FindChildItem("TxtDragonName");
			if (kAWidget3 != null)
			{
				kAWidget3.SetVisibility(inVisible: false);
			}
		}
	}

	private void AddHeroDragonCard(ItemData iData)
	{
		KAWidget kAWidget = DuplicateWidget(mMenu._Template);
		KAWidget kAWidget2 = kAWidget.FindChildItem("BtnDWDragonsFlight");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(inVisible: true);
		}
		HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(iData.ItemID);
		AddDragonDetails(kAWidget, heroDragonFromID);
		if (!SubscriptionInfo.pIsMember)
		{
			if (CommonInventoryData.pInstance.FindItem(iData.ItemID) == null)
			{
				kAWidget2 = kAWidget.FindChildItem("IcoMemberLock");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(inVisible: true);
				}
				kAWidget2 = kAWidget.FindChildItem("IcoGems");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(inVisible: true);
				}
				kAWidget2 = kAWidget.FindChildItem("TxtGemsAmount");
				if (kAWidget2 != null)
				{
					kAWidget2.SetText(iData.CashCost.ToString());
					kAWidget2.SetVisibility(inVisible: true);
				}
				if (iData.CashCost <= 0)
				{
					kAWidget.SetUserDataInt(-1);
					kAWidget2 = kAWidget.FindChildItem("BkgComingSoon");
					if (kAWidget2 != null)
					{
						kAWidget2.SetVisibility(inVisible: true);
					}
				}
				else
				{
					kAWidget.SetUserDataInt(0);
					kAWidget2 = kAWidget.FindChildItem("BkgComingSoon");
					if (kAWidget2 != null)
					{
						kAWidget2.SetVisibility(inVisible: false);
					}
				}
			}
			else
			{
				kAWidget.SetUserDataInt(iData.ItemID);
				kAWidget2 = kAWidget.FindChildItem("BkgComingSoon");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(inVisible: false);
				}
				kAWidget2 = kAWidget.FindChildItem("IcoGems");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(inVisible: false);
				}
				kAWidget2 = kAWidget.FindChildItem("TxtGemsAmount");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(inVisible: false);
				}
				kAWidget2 = kAWidget.FindChildItem("IcoMemberLock");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(inVisible: false);
				}
			}
		}
		else
		{
			kAWidget2 = kAWidget.FindChildItem("BkgComingSoon");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: false);
			}
			kAWidget2 = kAWidget.FindChildItem("IcoGems");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: false);
			}
			kAWidget2 = kAWidget.FindChildItem("TxtGemsAmount");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: false);
			}
			kAWidget2 = kAWidget.FindChildItem("IcoMemberLock");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: false);
			}
			if (iData.CashCost <= 0)
			{
				kAWidget.SetUserDataInt(-1);
				kAWidget2 = kAWidget.FindChildItem("BkgComingSoon");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(inVisible: true);
				}
			}
			else
			{
				kAWidget.SetUserDataInt(iData.ItemID);
				kAWidget2 = kAWidget.FindChildItem("BkgComingSoon");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(inVisible: false);
				}
			}
		}
		kAWidget.name = "Hero_" + iData.ItemID;
		kAWidget.SetVisibility(inVisible: true);
		mMenu.AddWidget(kAWidget);
	}

	public void DoDragonAction(KAWidget inWidget)
	{
		string text = inWidget.name;
		if (text.Contains("Player"))
		{
			if (SanctuaryManager.pCurPetInstance == null)
			{
				ShowWarningDB(_NoDragonMessageText, null, null, null, "OnCloseDB");
				return;
			}
			if (SanctuaryManager.pCurPetInstance.pAge < _MinAgeToFlyMode)
			{
				ShowWarningDB(_DragonNotAgedText, null, null, null, "OnCloseDB");
				return;
			}
			mSelectedTicketID = 0;
			LoadLevelWithHeroDragon(mSelectedTicketID);
		}
		else
		{
			if (!text.Contains("Hero_"))
			{
				return;
			}
			mSelectedTicketID = -1;
			if (!SubscriptionInfo.pIsMember)
			{
				if (inWidget.GetUserData() == null)
				{
					return;
				}
				if (inWidget.GetUserDataInt() == 0)
				{
					SetSelectedTicket(text);
					HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(mSelectedTicketID);
					int num = -1;
					ItemData[] items = mStoreData._Items;
					foreach (ItemData itemData in items)
					{
						if (itemData.ItemID == mSelectedTicketID)
						{
							num = itemData.FinalCashCost;
						}
					}
					string stringData = StringTable.GetStringData(_DragonNotPurchasedText._ID, _DragonNotPurchasedText._Text);
					stringData = stringData.Replace("{{name}}", heroDragonFromID._Name);
					stringData = stringData.Replace("{{type}}", heroDragonFromID._DragonType);
					stringData = stringData.Replace("{{amount}}", num.ToString());
					ShowWarningDB(null, "PurchaseHeroDragonTicket", "OnCloseDB", null, null, stringData);
				}
				else if (inWidget.GetUserDataInt() < 0)
				{
					ShowWarningDB(_DragonComingSoonText, null, null, null, "OnCloseDB");
				}
				else
				{
					SetSelectedTicket(text);
					LoadLevelWithHeroDragon(inWidget.GetUserDataInt());
				}
			}
			else if (inWidget.GetUserDataInt() < 0)
			{
				ShowWarningDB(_DragonComingSoonText, null, null, null, "OnCloseDB");
			}
			else
			{
				SetSelectedTicket(text);
				LoadLevelWithHeroDragon(inWidget.GetUserDataInt());
			}
		}
	}

	private int SetSelectedTicket(string inWidgetName)
	{
		string[] array = inWidgetName.Split('_');
		if (array[0] == "Hero")
		{
			int result = -1;
			if (int.TryParse(array[1], out result))
			{
				mSelectedTicketID = result;
			}
		}
		return -1;
	}

	private void ShowWarningDB(LocaleString inMessage, string YesMsg, string NoMsg, string CloseMsg, string OkMsg, string normalString = null)
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
		KAUI.SetExclusive(mUiGenericDB);
	}

	private void OnCloseDB()
	{
		KAUI.RemoveExclusive(mUiGenericDB);
		Object.DestroyImmediate(mUiGenericDB.gameObject);
		mUiGenericDB = null;
	}

	public void LoadLevelWithHeroDragon(int ItemID)
	{
		if (ItemID > 0)
		{
			HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(ItemID);
			if (heroDragonFromID != null)
			{
				CreateDragon(heroDragonFromID);
			}
		}
		else
		{
			SetVisibility(inVisible: false);
		}
	}

	private void CreateDragon(HeroPetData data)
	{
		RaisedPetData raisedPetData = RaisedPetData.CreateCustomizedPetData(data._TypeID, data._Age, data._DataPath, data._Gender, null, noColorMap: true);
		raisedPetData.Name = data._Name;
		SanctuaryManager.CreatePet(raisedPetData, new Vector3(5000f, 5000f, 5000f), Quaternion.identity, base.gameObject, "Basic");
		if (SanctuaryManager.pRequestSkillSync)
		{
			SanctuaryManager.pRequestSkillSync = false;
		}
	}

	private void OnPetReady(SanctuaryPet pet)
	{
		if (pet != null && SanctuaryManager.pCurPetInstance != pet)
		{
			if (SanctuaryManager.pCurPetInstance != null)
			{
				Object.Destroy(SanctuaryManager.pCurPetInstance.gameObject);
				SanctuaryManager.pCurPetInstance = null;
			}
			SanctuaryManager.pCurPetData = pet.pData;
			pet.SetClickActivateObject(SanctuaryManager.pInstance._PetClickActivateObject);
			pet.SetFlyUIObject(SanctuaryManager.pInstance._PetFlyUIObject);
			pet.pMeterPaused = true;
			pet.mAvatar = AvAvatar.mTransform;
			SanctuaryManager.pCurPetInstance = pet;
		}
	}

	private void PurchaseHeroDragonTicket()
	{
		OnCloseDB();
		int num = -1;
		if (mStoreData == null)
		{
			return;
		}
		ItemData[] items = mStoreData._Items;
		foreach (ItemData itemData in items)
		{
			if (itemData.ItemID == mSelectedTicketID)
			{
				num = itemData.FinalCashCost;
			}
		}
		if (num > 0 && Money.pCashCurrency >= num)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			PurchaseTicket();
		}
		else
		{
			ShowWarningDB(_NotEnoughFeeText, "BuyGemsOnline", "OnCloseDB", null, null);
		}
	}

	private void PurchaseTicket()
	{
		CommonInventoryData.pInstance.AddPurchaseItem(mSelectedTicketID, 1, ItemPurchaseSource.MOBA.ToString());
		CommonInventoryData.pInstance.DoPurchase(2, _StoreID, PurchaseDone);
		ShowWarningDB(_PurchaseProcessingText, null, null, null, null);
	}

	public void PurchaseDone(CommonInventoryResponse ret)
	{
		OnCloseDB();
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (ret != null && ret.Success)
		{
			ShowWarningDB(_PurchaseSuccessText, null, null, null, "OnCloseDB");
			InitDragonCards();
		}
		else
		{
			ShowWarningDB(_PurchaseFailText, null, null, null, "OnCloseDB");
		}
	}

	private void BuyGemsOnline()
	{
		OnCloseDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void InitDragonCards()
	{
		mMenu.ClearItems();
		AddPlayerCard();
		ItemData[] items = mStoreData._Items;
		foreach (ItemData iData in items)
		{
			AddHeroDragonCard(iData);
		}
	}

	private void AddDragonDetails(KAWidget inWidget, HeroPetData pData)
	{
		if (!(inWidget == null) && pData != null)
		{
			KAWidget kAWidget = inWidget.FindChildItem("TxtDragonName");
			if (kAWidget != null)
			{
				kAWidget.SetText(pData._Name);
				kAWidget.SetVisibility(inVisible: true);
			}
			kAWidget = inWidget.FindChildItem("IcoDragonTexture");
			if (kAWidget != null && !string.IsNullOrEmpty(pData._DragonSpriteName))
			{
				kAWidget.pBackground.UpdateSprite(pData._DragonSpriteName);
				kAWidget.SetVisibility(inVisible: true);
			}
		}
	}

	private void SetPlayerDragonData()
	{
		HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(0);
		heroDragonFromID._Name = SanctuaryManager.pCurPetInstance.pData.Name;
		heroDragonFromID._TypeID = SanctuaryManager.pCurPetInstance.pTypeInfo._TypeID;
		heroDragonFromID._DragonClass = SanctuaryManager.pCurPetInstance.pTypeInfo._DragonClass;
		heroDragonFromID._DragonType = SanctuaryManager.pCurPetInstance.pTypeInfo._NameText.GetLocalizedString();
		heroDragonFromID._Age = SanctuaryManager.pCurPetInstance.pData.pStage;
	}

	private void TakePetAShot()
	{
		if (mPhotoManager == null)
		{
			mPhotoManager = AvPhotoManager.Init("PfPetPhotoMgr");
		}
		Texture2D dstTexture = new Texture2D(256, 256, TextureFormat.ARGB32, mipChain: false);
		dstTexture.name = "PetMeterPic";
		mPhotoManager._HeadShotCamOffset = SanctuaryManager.pCurPetInstance.pCurAgeData._HUDPictureCameraOffset;
		string headBoneName = SanctuaryManager.pCurPetInstance.GetHeadBoneName();
		if (!string.IsNullOrEmpty(headBoneName))
		{
			Transform transform = SanctuaryManager.pCurPetInstance.FindBoneTransform(headBoneName);
			if (transform != null)
			{
				GameObject gameObject = SanctuaryManager.pCurPetInstance.gameObject;
				gameObject.transform.parent = mPhotoManager.transform;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				mPhotoManager._AvatarCam.OnTakePicture(gameObject, lookAtCamera: false);
				mPhotoManager.TakeAShot(SanctuaryManager.pCurPetInstance.gameObject, ref dstTexture, transform);
			}
			else
			{
				UtDebug.LogError("NO HEAD BONE FOUND!!!");
			}
		}
		else
		{
			UtDebug.LogError("NO HEAD BONE NAME PROVIDED!!");
		}
		KAWidget kAWidget = mMenu.FindItem("Player");
		if (kAWidget != null)
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem("IcoDragonTexture");
			if (kAWidget2 != null)
			{
				kAWidget2.SetTexture(dstTexture);
			}
		}
	}
}
