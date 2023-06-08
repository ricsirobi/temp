using System.Collections.Generic;
using UnityEngine;

public class UiSelectHeroDragons : KAUI
{
	public int _StoreID = 105;

	public bool _AllowFlightlessDragons;

	public LocaleString _FlightlessPetBlockedText = new LocaleString("This Dragon cannot fly, please make another dragon your Active dragon to participate.");

	public LocaleString _NoDragonMesaageText = new LocaleString("You need your dragon. Follow the quests to get your own dragon from the Hatchery!");

	public LocaleString _DragonNotAgedText = new LocaleString("Your dragon needs to be Short Wing stage to enter.");

	public LocaleString _DragonNotPurchasedText = new LocaleString("Would you unlock {{name}} and the {{type}} levels for {{amount}} gems.");

	public LocaleString _DragonComingSoonText = new LocaleString("You have wait a bit to play with this Dragon.");

	public LocaleString _YourDragonText = new LocaleString("Your Dragon");

	public LocaleString _PurchaseSuccessText = new LocaleString("Hero Dragon purchase successful.");

	public LocaleString _PurchaseFailText = new LocaleString("Hero Dragon purchase failed.");

	public LocaleString _NotEnoughFeeText = new LocaleString("You do not have enough gems to pay, Please buy more!");

	public LocaleString _PurchaseProcessingText = new LocaleString("Processing purchase.");

	public LocaleString _NotConnected = new LocaleString("You need to be connected to internet to purchase. Please check your connection and try again!");

	public KAWidget _PlayerTemplate;

	public KAWidget _DragonInfoCardTemplate;

	public Texture _NoPetTexture;

	private StoreData mStoreData;

	private ObstacleCourseLevelManager mGameManager;

	private UiSelectHeroDragonsMenu mMenu;

	private KAUIGenericDB mUiGenericDB;

	private AvPhotoManager mPhotoManager;

	private KAWidget mNonMemberBanner;

	private bool mIsInitialized;

	private bool mIsStorePreLoaded;

	private static int mSelectedTicketID;

	public StoreData pStoreData => mStoreData;

	public int pSelectedTicketID
	{
		get
		{
			return mSelectedTicketID;
		}
		set
		{
			mSelectedTicketID = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		mMenu = (UiSelectHeroDragonsMenu)GetMenu("UiSelectHeroDragonsMenu");
	}

	public void Init(ObstacleCourseLevelManager inManager)
	{
		mGameManager = inManager;
		if (mStoreData == null)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			ItemStoreDataLoader.Load(_StoreID, OnStoreLoaded);
		}
		else
		{
			ProcessOnStoreLoaded();
		}
		if (mMenu != null && mMenu.GetState() != 0)
		{
			mMenu.SetInteractive(interactive: true);
		}
	}

	public void OnStoreLoaded(StoreData sd)
	{
		mStoreData = sd;
		if (sd == null || sd._Items == null || sd._Items.Length == 0)
		{
			Debug.Log("Storedata is not added");
			return;
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (mIsStorePreLoaded)
		{
			mIsStorePreLoaded = false;
			return;
		}
		mNonMemberBanner = FindItem("MemberBtn");
		if (mNonMemberBanner != null && !SubscriptionInfo.pIsMember)
		{
			mNonMemberBanner.SetVisibility(inVisible: true);
		}
		ProcessOnStoreLoaded();
	}

	private void ProcessOnStoreLoaded()
	{
		if (ObstacleCourseLevelManager.mMenuState == FSMenuState.FS_STATE_DRAGONSELECT)
		{
			if (mGameManager.pGameMode == FSGameMode.FLIGHT_MODE)
			{
				AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
				if (component != null)
				{
					component.pFlyingGlidingMode = false;
				}
			}
			else if (mGameManager.pGameMode == FSGameMode.GLIDE_MODE)
			{
				AvAvatarController component2 = AvAvatar.pObject.GetComponent<AvAvatarController>();
				if (component2 != null)
				{
					component2.pFlyingGlidingMode = true;
				}
			}
			else if (mGameManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE)
			{
				AvAvatarController component3 = AvAvatar.pObject.GetComponent<AvAvatarController>();
				if (component3 != null)
				{
					component3.pFlyingGlidingMode = false;
				}
			}
			ShowDragonCards();
		}
		else if (ObstacleCourseLevelManager.mMenuState == FSMenuState.FS_STATE_LEVELSELECT)
		{
			if (mGameManager.pGameMode == FSGameMode.GLIDE_MODE)
			{
				if (mGameManager._DefaultLevelSelectionUi != null)
				{
					mGameManager.LoadData();
				}
			}
			else if (mGameManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE)
			{
				if (mGameManager._DefaultLevelSelectionUi != null)
				{
					mGameManager.LoadData();
				}
			}
			else if (mGameManager.pGameMode == FSGameMode.FLIGHT_MODE)
			{
				LoadLevelWithHeroDragon(mGameManager._DragonSelectionUi.pSelectedTicketID);
			}
		}
		else
		{
			ShowDragonCards();
		}
	}

	private void ShowDragonCards()
	{
		if (ObstacleCourseLevelManager.mMenuState <= FSMenuState.FS_STATE_DRAGONSELECT)
		{
			ObstacleCourseLevelManager.mMenuState = FSMenuState.FS_STATE_DRAGONSELECT;
		}
		mGameManager.pGameMode = FSGameMode.FLIGHT_MODE;
		mGameManager.LoadPairData();
		mIsInitialized = false;
	}

	protected override void Update()
	{
		base.Update();
		if (SanctuaryManager.pCurrentPetType > -1 && SanctuaryManager.pCurPetInstance == null)
		{
			return;
		}
		if (!mIsInitialized && mStoreData != null && mGameManager != null && mGameManager.pIsPairDataReady && ObstacleCourseLevelManager.mMenuState == FSMenuState.FS_STATE_DRAGONSELECT)
		{
			SetVisibility(inVisible: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
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

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BackBtn")
		{
			SetVisibility(inVisible: false);
			ObstacleCourseLevelManager.mMenuState = FSMenuState.FS_STATE_MODESELECT;
			mGameManager.ShowGameModeUI();
		}
		else if (inWidget == mNonMemberBanner)
		{
			IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
		}
	}

	private void OnIAPStoreClosed()
	{
		if (SubscriptionInfo.pIsMember)
		{
			if (mNonMemberBanner != null)
			{
				mNonMemberBanner.SetVisibility(inVisible: false);
			}
			InitDragonCards();
		}
	}

	private void AddPlayerDragonsCards()
	{
		if (RaisedPetData.pActivePets == null)
		{
			return;
		}
		int num = 0;
		foreach (RaisedPetData[] value in RaisedPetData.pActivePets.Values)
		{
			if (value == null)
			{
				continue;
			}
			RaisedPetData[] array = value;
			foreach (RaisedPetData raisedPetData in array)
			{
				UiDragonsInfoCardItem uiDragonsInfoCardItem = (UiDragonsInfoCardItem)DuplicateWidget(_DragonInfoCardTemplate);
				uiDragonsInfoCardItem.name = "PlayerDragon_" + raisedPetData.Name;
				uiDragonsInfoCardItem.SetMessageObject(base.gameObject);
				uiDragonsInfoCardItem.SetVisibility(inVisible: true);
				uiDragonsInfoCardItem.pSelectedPetData = raisedPetData;
				uiDragonsInfoCardItem.RefreshUI();
				uiDragonsInfoCardItem.SetButtons(selectBtn: false, visitBtn: false, moveInBtn: false);
				if (SanctuaryManager.pCurPetData.RaisedPetID == raisedPetData.RaisedPetID)
				{
					mMenu.AddWidgetAt(0, uiDragonsInfoCardItem);
				}
				else
				{
					mMenu.AddWidgetAt(num, uiDragonsInfoCardItem);
				}
				num++;
			}
		}
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
			SetPlayerDragonData();
			HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(0);
			bool visibility = false;
			if (mGameManager.pFlightModeLevelUnlockingDataMap.ContainsKey(0))
			{
				visibility = mGameManager.pFlightModeLevelUnlockingDataMap[0].IsAllHeroLevelsPlayed() && mGameManager.pFlightModeLevelUnlockingDataMap[0].IsAllLevelsPlayed();
			}
			AddDragonDetails(kAWidget2, heroDragonFromID);
			if (SanctuaryManager.pCurPetInstance.pAge >= SanctuaryManager.pCurPetInstance.pTypeInfo._MinAgeToFly)
			{
				kAWidget3 = kAWidget2.FindChildItem("BkgComingSoon");
				if (kAWidget3 != null)
				{
					kAWidget3.SetVisibility(inVisible: false);
				}
				kAWidget3 = kAWidget2.FindChildItem("IcoCompletionStar");
				if (kAWidget3 != null)
				{
					kAWidget3.SetVisibility(visibility);
				}
			}
		}
		else
		{
			kAWidget3 = kAWidget2.FindChildItem("BkgComingSoon");
			if (kAWidget3 != null)
			{
				kAWidget3.SetText(_YourDragonText.GetLocalizedString());
				kAWidget3.SetVisibility(inVisible: true);
			}
		}
		kAWidget2.SetVisibility(inVisible: true);
		kAWidget2.name = "Player";
		mMenu.AddWidgetAt(0, kAWidget2);
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pInstance.TakePicture(SanctuaryManager.pCurPetInstance.gameObject, base.gameObject, inSendPicture: false);
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
			kAWidget3 = kAWidget4.FindChildItem("TxtDragonClass");
			if (kAWidget3 != null)
			{
				kAWidget3.SetVisibility(inVisible: false);
			}
			kAWidget3 = kAWidget4.FindChildItem("IcoDragonClass");
			if (kAWidget3 != null)
			{
				kAWidget3.SetVisibility(inVisible: false);
			}
			kAWidget3 = kAWidget4.FindChildItem("TxtDragonType");
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
		bool visibility = false;
		if (mGameManager.pFlightModeLevelUnlockingDataMap.ContainsKey(iData.ItemID))
		{
			visibility = mGameManager.pFlightModeLevelUnlockingDataMap[iData.ItemID].IsAllHeroLevelsPlayed() && mGameManager.pFlightModeLevelUnlockingDataMap[iData.ItemID].IsAllLevelsPlayed();
		}
		AddDragonDetails(kAWidget, heroDragonFromID);
		if (!SubscriptionInfo.pIsMember)
		{
			if (CommonInventoryData.pInstance.FindItem(iData.ItemID) == null)
			{
				bool flag = true;
				bool flag2 = true;
				if (iData.FinalCashCost < 0)
				{
					kAWidget.SetUserDataInt(-1);
					flag = true;
					flag2 = false;
				}
				else if (iData.FinalCashCost > 0)
				{
					kAWidget.SetUserDataInt(0);
					flag = false;
					flag2 = true;
				}
				else
				{
					kAWidget.SetUserDataInt(iData.ItemID);
					flag = false;
					flag2 = false;
					kAWidget2 = kAWidget.FindChildItem("AniIconFree");
					if (kAWidget2 != null)
					{
						kAWidget2.SetVisibility(inVisible: true);
					}
				}
				kAWidget2 = kAWidget.FindChildItem("BkgComingSoon");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(flag);
				}
				kAWidget2 = kAWidget.FindChildItem("IcoMemberLock");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(flag2);
				}
				kAWidget2 = kAWidget.FindChildItem("IcoGems");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(flag2);
				}
				kAWidget2 = kAWidget.FindChildItem("TxtGemsAmount");
				if (kAWidget2 != null)
				{
					kAWidget2.SetText(iData.FinalCashCost.ToString());
					kAWidget2.SetVisibility(flag2);
				}
				kAWidget2 = kAWidget.FindChildItem("IcoCompletionStar");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(visibility);
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
				if (iData.FinalCashCost > 0)
				{
					kAWidget2 = kAWidget.FindChildItem("IcoCompletionStar");
					if (kAWidget2 != null)
					{
						kAWidget2.SetVisibility(visibility);
					}
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
			if (iData.FinalCashCost < 0)
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
				kAWidget2 = kAWidget.FindChildItem("IcoCompletionStar");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(visibility);
				}
			}
		}
		kAWidget.name = "Hero_" + iData.ItemID;
		kAWidget.SetVisibility(inVisible: true);
		mMenu.AddWidget(kAWidget);
	}

	private void OnSelectDragonFinish(int petID)
	{
		SetInteractive(interactive: true);
		SetVisibility(inVisible: false);
	}

	private void OnSelectDragonFailed(int petID)
	{
		OnSelectDragonFinish(petID);
	}

	public void DoDragonAction(KAWidget inWidget)
	{
		KAWidget parentItem = inWidget.GetParentItem();
		string text = inWidget.transform.parent.name;
		_ = parentItem.FindChildItem("TxtDragonName").GetLabel().text;
		if (text.Contains("PlayerDragon"))
		{
			UiDragonsInfoCardItem uiDragonsInfoCardItem = (UiDragonsInfoCardItem)parentItem;
			if (uiDragonsInfoCardItem != null)
			{
				RaisedPetStage growthStage = RaisedPetData.GetGrowthStage(SanctuaryData.FindSanctuaryPetTypeInfo(uiDragonsInfoCardItem.pSelectedPetData.PetTypeID)._MinAgeToMount);
				if (uiDragonsInfoCardItem.pSelectedPetData.pStage < growthStage)
				{
					SetVisibility(inVisible: false);
					mMenu.SetVisibility(inVisible: false);
					DragonAgeUpConfig.ShowAgeUpUI(OnDragonAgeUpDone, uiDragonsInfoCardItem.pSelectedPetData.pStage, uiDragonsInfoCardItem.pSelectedPetData, new RaisedPetStage[1] { growthStage });
				}
				else
				{
					SetInteractive(interactive: false);
					uiDragonsInfoCardItem.MakeActiveDragon();
				}
			}
		}
		else if (text.Contains("Player"))
		{
			if (!_AllowFlightlessDragons && SanctuaryManager.pCurPetInstance.pTypeInfo._Flightless)
			{
				ShowWarningDB(_FlightlessPetBlockedText, null, null, null, "OnCloseDB");
			}
			else if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pAge < SanctuaryManager.pCurPetInstance.pTypeInfo._MinAgeToMount)
			{
				SetVisibility(inVisible: false);
				mMenu.SetVisibility(inVisible: false);
				ShowWarningDB(_DragonNotAgedText, null, null, null, "ShowAgeUp");
			}
			else
			{
				mMenu.SetInteractive(interactive: false);
				SetVisibility(inVisible: false);
				mGameManager.ShowLevelSelectionUI();
			}
		}
		else
		{
			if (!text.Contains("Hero_"))
			{
				return;
			}
			mGameManager.pGameMode = FSGameMode.FLIGHT_MODE;
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component.pFlyingGlidingMode = false;
			}
			mSelectedTicketID = -1;
			if (!SubscriptionInfo.pIsMember)
			{
				if (parentItem.GetUserData() == null)
				{
					return;
				}
				if (parentItem.GetUserDataInt() == 0)
				{
					if (!UtUtilities.IsConnectedToWWW())
					{
						ShowWarningDB(null, "OnCloseDB", "OnCloseDB", null, null, _NotConnected._Text);
						return;
					}
					mMenu.SetInteractive(interactive: false);
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
				else if (parentItem.GetUserDataInt() < 0)
				{
					ShowWarningDB(_DragonComingSoonText, null, null, null, "OnCloseDB");
				}
				else
				{
					mMenu.SetInteractive(interactive: false);
					SetSelectedTicket(text);
					LoadLevelWithHeroDragon(parentItem.GetUserDataInt());
				}
			}
			else if (parentItem.GetUserDataInt() < 0)
			{
				ShowWarningDB(_DragonComingSoonText, null, null, null, "OnCloseDB");
			}
			else
			{
				mMenu.SetInteractive(interactive: false);
				SetSelectedTicket(text);
				LoadLevelWithHeroDragon(parentItem.GetUserDataInt());
			}
		}
	}

	public bool ShowPlayerFlightModeLevels()
	{
		if (SanctuaryManager.pCurPetInstance == null)
		{
			ShowWarningDB(_NoDragonMesaageText, null, null, null, "OnCloseDB");
			return false;
		}
		if (SanctuaryManager.pCurPetInstance.pAge < SanctuaryManager.pCurPetInstance.pTypeInfo._MinAgeToFly)
		{
			ShowWarningDB(_DragonNotAgedText, null, null, null, "ShowAgeUp");
			return false;
		}
		if (SanctuaryManager.pCurPetInstance.IsActionAllowed(PetActions.FLIGHTSCHOOL))
		{
			mSelectedTicketID = 0;
			LoadLevelWithHeroDragon(mSelectedTicketID);
			return true;
		}
		UiPetEnergyGenericDB.Show(base.gameObject, "EnergyUpdate", "EnergyUpdate", isLowEnergy: true);
		return false;
	}

	private void EnergyUpdate()
	{
		mMenu.SetInteractive(interactive: true);
		SetVisibility(inVisible: true);
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
		if (mUiGenericDB != null)
		{
			KAUI.RemoveExclusive(mUiGenericDB);
			Object.DestroyImmediate(mUiGenericDB.gameObject);
			mUiGenericDB = null;
			if (mMenu != null && mMenu.GetState() != 0)
			{
				mMenu.SetInteractive(interactive: true);
			}
		}
	}

	private void ShowAgeUp()
	{
		OnCloseDB();
		List<RaisedPetStage> list = new List<RaisedPetStage>();
		for (int i = SanctuaryManager.pCurPetInstance.pTypeInfo._MinAgeToMount; i < SanctuaryManager.pCurPetInstance.pTypeInfo._AgeData.Length; i++)
		{
			list.Add(RaisedPetData.GetGrowthStage(i));
		}
		DragonAgeUpConfig.ShowAgeUpUI(OnDragonAgeUpDone, SanctuaryManager.pCurPetData.pStage, SanctuaryManager.pCurPetData, list.ToArray());
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
			if (mGameManager != null && mGameManager._LevelSelectionUi != null)
			{
				mGameManager.LoadData();
			}
		}
	}

	private void CreateDragon(HeroPetData data)
	{
		RaisedPetData raisedPetData = RaisedPetData.CreateCustomizedPetData(data._TypeID, data._Age, data._DataPath, data._Gender, null, noColorMap: true);
		raisedPetData.Name = data._Name;
		SanctuaryManager.CreatePet(raisedPetData, new Vector3(5000f, 5000f, 5000f), Quaternion.identity, base.gameObject, "Full");
		SanctuaryManager.pInstance.pCreateInstance = false;
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
			pet.SetMeterUI(SanctuaryManager.pInstance.pPetMeter);
			pet.mAvatar = AvAvatar.mTransform;
			SanctuaryManager.pCurPetInstance = pet;
			PetWeaponManager component = pet.gameObject.GetComponent<PetWeaponManager>();
			if (component != null)
			{
				component.pUserControlledWeapon = true;
			}
			SanctuaryManager.pInstance.SetWeaponRechargeData(pet);
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SetVisibility(inVisible: false);
			if (mGameManager != null && mGameManager._LevelSelectionUi != null)
			{
				mGameManager.LoadData();
			}
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
		CommonInventoryData.pInstance.AddPurchaseItem(mSelectedTicketID, 1, ItemPurchaseSource.FLIGHT_SCHOOL.ToString());
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
		if (mGameManager != null && mGameManager._FlightSchoolIntroTut != null)
		{
			mGameManager._FlightSchoolIntroTut.TutorialManagerAsyncMessage("SelectedFlightMode");
		}
	}

	private void AddDragonDetails(KAWidget inWidget, HeroPetData hpData)
	{
		if (inWidget == null || hpData == null)
		{
			return;
		}
		DragonClassInfo dragonClassInfo = SanctuaryData.GetDragonClassInfo(hpData._DragonClass);
		KAWidget kAWidget = inWidget.FindChildItem("TxtDragonName");
		if (kAWidget != null)
		{
			kAWidget.SetText(hpData._NameText.GetLocalizedString());
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtDragonClass");
		if (kAWidget != null)
		{
			string text = dragonClassInfo._InfoText.GetLocalizedString();
			if (text.Contains("_"))
			{
				text = text.Replace('_', ' ');
			}
			kAWidget.SetText(text);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("IcoDragonClass");
		if (kAWidget != null && !string.IsNullOrEmpty(dragonClassInfo._IconSprite))
		{
			kAWidget.pBackground.UpdateSprite(dragonClassInfo._IconSprite);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtDragonType");
		if (kAWidget != null)
		{
			SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(hpData._TypeID);
			kAWidget.SetText(sanctuaryPetTypeInfo._NameText.GetLocalizedString());
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtStrengthVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(hpData._Strength);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtSpeedVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(hpData._Speed);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtEnduranceVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(hpData._Endurance);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtArmorVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(hpData._Armor);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtFireVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(hpData._Fire);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtShotLimitVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(hpData._ShotLimit);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtWingSpanVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(hpData._WingSpan);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtLengthVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(hpData._Length);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("TxtWeightVal");
		if (kAWidget != null)
		{
			kAWidget.SetText(hpData._Weight);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("IcoRider");
		if (kAWidget != null && !string.IsNullOrEmpty(hpData._RiderSpriteName))
		{
			kAWidget.pBackground.UpdateSprite(hpData._RiderSpriteName);
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget.FindChildItem("IcoDragonFlightTexture");
		if (kAWidget != null && !string.IsNullOrEmpty(hpData._DragonSpriteName))
		{
			kAWidget.pBackground.UpdateSprite(hpData._DragonSpriteName);
			kAWidget.SetVisibility(inVisible: true);
		}
	}

	private void SetPlayerDragonData()
	{
		HeroPetData heroDragonFromID = SanctuaryData.GetHeroDragonFromID(0);
		heroDragonFromID._NameText._Text = SanctuaryManager.pCurPetInstance.pData.Name;
		heroDragonFromID._TypeID = SanctuaryManager.pCurPetInstance.pTypeInfo._TypeID;
		heroDragonFromID._DragonClass = SanctuaryManager.pCurPetInstance.pTypeInfo._DragonClass;
		heroDragonFromID._DragonType = SanctuaryManager.pCurPetInstance.pTypeInfo._NameText.GetLocalizedString();
		heroDragonFromID._Age = SanctuaryManager.pCurPetInstance.pData.pStage;
	}

	protected virtual void OnPetPictureDone(object inImage)
	{
		if (inImage == null)
		{
			return;
		}
		KAWidget kAWidget = mMenu.FindItem("Player");
		if (kAWidget != null)
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem("IcoDragonFlightTexture");
			if (kAWidget2 != null)
			{
				kAWidget2.SetTexture(inImage as Texture);
			}
		}
	}

	private void OnDragonAgeUpDone()
	{
		SetVisibility(inVisible: true);
		mMenu.SetVisibility(inVisible: true);
		AvAvatar.SetOnlyAvatarActive(active: false);
	}

	public void LoadTutStore()
	{
		if (mStoreData == null)
		{
			mIsStorePreLoaded = true;
			KAUICursorManager.SetDefaultCursor("Loading");
			ItemStoreDataLoader.Load(_StoreID, OnStoreLoaded);
		}
	}
}
