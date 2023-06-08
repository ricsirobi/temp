using System;
using System.Collections.Generic;
using UnityEngine;

public class UiMultiEggHatching : KAUI
{
	public LocaleString _DragEggToSlotText = new LocaleString("[Review] Drag an egg into the slot to hatch it.");

	public LocaleString _HatchEggConfirmationText = new LocaleString("[Review] Do you want to hatch now?");

	public LocaleString _NoEmptyHatchingPedestalText = new LocaleString("[Review] There is no empty hatching pedestal.");

	public LocaleString _PreviousSlotLockedText = new LocaleString("[Review] Need to unlock previous slot to proceed.");

	public LocaleString _NotEnoughVCashText = new LocaleString("[Review] You don't have enough Gems to unlock slot. Do you want to buy more Gems?");

	public LocaleString _UseGemsForSlotPurchaseText = new LocaleString("[Review] Unlocking slot will cost you {cost} Gems. Do you want to continue?");

	public LocaleString _SlotPurchaseProcessingText = new LocaleString("[Review] Processing purchase...");

	public LocaleString _SlotPurchaseFailedText = new LocaleString("[Review] Transaction failed. Please try again.");

	public LocaleString _GetCashCostText = new LocaleString("[Review] Getting item cost.");

	public LocaleString _GetCashCostFailedText = new LocaleString("[Review] Failed to get item cost. Please try later.");

	public LocaleString _SlotUnlockUnavailableText = new LocaleString("[Review] Unavailable slot unlock.");

	public LocaleString _PlaceEggInRightSlotText = new LocaleString("[Review] Place egg in an unlocked empty Incubator slot.");

	public LocaleString _BuyStablesText = new LocaleString("[Review] You dont have any available Nests.Do you want to buy a Stable ?");

	public LocaleString _GoToHatcheryText = new LocaleString("[Review] Go to the hatchery?");

	public LocaleString _GoToHatcheryAndHatchText = new LocaleString("[Review] Go to the hatchery and hatch this egg?");

	[SerializeField]
	private UiHatchingSpeedUp m_UiHatchingSpeedUp;

	[SerializeField]
	private int m_DragonEggsCategory = 456;

	[SerializeField]
	private int m_GeneralStoreId = 93;

	[SerializeField]
	private StoreLoader.Selection m_EggStoreInfo;

	[SerializeField]
	private StoreLoader.Selection m_StableStoreInfo;

	[SerializeField]
	private KAWidget m_BuyAvailableTemplate;

	[SerializeField]
	private InteractiveTutManager m_MultiEggHatchingTutorial;

	private UiHatchingSlotsMenu mHatchingSlotsMenu;

	private UiEggsDisplayMenu mEggsDisplayMenu;

	private HatcheryManager mHatcheryManager;

	public HatcheryManager _HatcheryManager;

	public string _HatcheryMarker;

	private KAUIGenericDB mKAUIGenericDB;

	private KAWidget mSelectedEgg;

	private KAWidget mFirstEmptySlot;

	private IncubatorWidgetData mMoveInIncubatorData;

	private RaisedPetData mMoveInPetData;

	private KAWidget mCloseBtn;

	private KAWidget mGoToHatcheryBtn;

	private KAWidget mStoreBtn;

	public UiHatchingSpeedUp pUiHatchingSpeedUp => m_UiHatchingSpeedUp;

	public HatcheryManager pHatcheryManager => mHatcheryManager;

	public KAWidget pSelectedEgg => mSelectedEgg;

	protected override void Start()
	{
		base.Start();
		KAUICursorManager.SetDefaultCursor("Loading");
		mHatchingSlotsMenu = (UiHatchingSlotsMenu)GetMenuByIndex(0);
		mEggsDisplayMenu = (UiEggsDisplayMenu)GetMenuByIndex(1);
		if ((bool)_HatcheryManager)
		{
			Init(_HatcheryManager);
		}
	}

	public void Init(HatcheryManager manager)
	{
		AvAvatar.pInputEnabled = false;
		AvAvatar.SetUIActive(inActive: false);
		mHatcheryManager = manager;
		mHatcheryManager.UpdateIncubatorSlotCosts(PopulateSlots);
		mCloseBtn = FindItem("CloseBtn");
		mGoToHatcheryBtn = FindItem("GoToHatcheryBtn");
		mStoreBtn = FindItem("BtnDWDragonsStoreSelect");
	}

	private void PopulateSlots()
	{
		if ((bool)_HatcheryManager)
		{
			mHatcheryManager = _HatcheryManager;
			HatcheryManager hatcheryManager = _HatcheryManager;
			hatcheryManager.OnSlotUnlockCostUpdated = (Action)Delegate.Remove(hatcheryManager.OnSlotUnlockCostUpdated, new Action(PopulateSlots));
		}
		else if ((bool)mHatcheryManager)
		{
			HatcheryManager hatcheryManager2 = mHatcheryManager;
			hatcheryManager2.OnSlotUnlockCostUpdated = (Action)Delegate.Remove(hatcheryManager2.OnSlotUnlockCostUpdated, new Action(PopulateSlots));
		}
		SetVisibility(inVisible: true);
		KAUI.SetExclusive(this);
		KAUICursorManager.SetDefaultCursor("Arrow");
		CreateIncubatorWidgets();
		CreateEggWidgets();
		InitTutorial();
	}

	public void CreateIncubatorWidgets()
	{
		mHatchingSlotsMenu.ClearItems();
		if (mHatcheryManager.pIncubators != null && mHatcheryManager.pIncubators.Count > 0)
		{
			for (int i = 0; i < mHatcheryManager.pIncubators.Count; i++)
			{
				KAWidget kAWidget = null;
				kAWidget = ((!mHatcheryManager.pIncubators[i].pReadyToBuy) ? mHatchingSlotsMenu._Template : m_BuyAvailableTemplate);
				KAWidget kAWidget2 = DuplicateWidget(kAWidget);
				kAWidget2.name = mHatcheryManager.pIncubators[i].name;
				kAWidget2.transform.localScale = kAWidget.transform.localScale;
				kAWidget2.SetVisibility(inVisible: true);
				IncubatorWidgetData incubatorWidgetData = new IncubatorWidgetData(mHatcheryManager.pIncubators[i]);
				incubatorWidgetData._Item = kAWidget2;
				kAWidget2.SetUserData(incubatorWidgetData);
				kAWidget2._MenuItemIndex = i;
				mHatchingSlotsMenu.AddWidget(kAWidget2);
				mHatchingSlotsMenu.SetupIncubatorWidget(kAWidget2);
			}
		}
	}

	public void CreateEggWidgets()
	{
		mEggsDisplayMenu.ClearItems();
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(m_DragonEggsCategory);
		if (items != null && items.Length != 0)
		{
			foreach (UserItemData userItemData in items)
			{
				KAWidget kAWidget = mEggsDisplayMenu.AddWidget(userItemData.Item.ItemName);
				EggDisplayWidgetData userData = new EggDisplayWidgetData(userItemData);
				kAWidget.SetUserData(userData);
				mEggsDisplayMenu.SetupEggDisplayWidget(kAWidget);
			}
		}
	}

	private void InitTutorial()
	{
		if (mEggsDisplayMenu.GetItemCount() > 0 && !m_MultiEggHatchingTutorial.IsShowingTutorial() && !m_MultiEggHatchingTutorial.TutorialComplete())
		{
			m_MultiEggHatchingTutorial.gameObject.SetActive(value: true);
			m_MultiEggHatchingTutorial.ShowTutorial();
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (!(UICursorManager.GetCursorName() == "Loading"))
		{
			base.OnClick(inWidget);
			if (inWidget == mGoToHatcheryBtn)
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _GoToHatcheryText.GetLocalizedString(), null, base.gameObject, "GoToHatchery", "KillGenericDB", "", "", inDestroyOnClick: true);
			}
			if (inWidget == mCloseBtn)
			{
				Exit();
			}
			if (inWidget == mStoreBtn && m_EggStoreInfo != null)
			{
				SetVisibility(inVisible: false);
				KAUI.RemoveExclusive(this);
				StoreLoader.Load(setDefaultMenuItem: true, m_EggStoreInfo._Category, m_EggStoreInfo._Store, base.gameObject);
			}
		}
	}

	public void GoToHatchery()
	{
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
		}
		AvAvatar.pStartLocation = _HatcheryMarker;
		Exit();
		RsResourceManager.LoadLevel("HatcheryINTDO");
	}

	public void Exit()
	{
		KAUI.RemoveExclusive(this);
		AvAvatar.pInputEnabled = true;
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		if ((bool)_HatcheryManager)
		{
			HatcheryManager hatcheryManager = Array.Find(UnityEngine.Object.FindObjectsOfType<HatcheryManager>(), (HatcheryManager t) => t != _HatcheryManager);
			if ((bool)hatcheryManager)
			{
				mHatcheryManager.GetCurrentUnlockedIncubatorCount();
				foreach (Incubator pIncubator in hatcheryManager.pIncubators)
				{
					pIncubator.ClearEgg();
				}
				hatcheryManager.SetupIncubators();
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			SetVisibility(inVisible: false);
		}
	}

	public void ProcessSlotUnlock(Incubator incubator = null)
	{
		if (incubator != null && !incubator.pReadyToBuy)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _PreviousSlotLockedText.GetLocalizedString(), "", base.gameObject, null, null, "KillGenericDB", null, inDestroyOnClick: true);
		}
		else if (mHatcheryManager.pSlotUnlockCost <= 0)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _GetCashCostFailedText.GetLocalizedString(), "", base.gameObject, null, null, "KillGenericDB", null, inDestroyOnClick: true);
		}
		else if (Money.pCashCurrency < mHatcheryManager.pSlotUnlockCost)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _NotEnoughVCashText.GetLocalizedString(), "", base.gameObject, "ProceedToStore", "KillGenericDB", null, null, inDestroyOnClick: true);
		}
		else
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _UseGemsForSlotPurchaseText.GetLocalizedString().Replace("{cost}", mHatcheryManager.pSlotUnlockCost.ToString()), "", base.gameObject, "OnLockedSlotPurchase", "KillGenericDB", null, null, inDestroyOnClick: true);
		}
	}

	private void ProceedToStore()
	{
		KillGenericDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void OnStoreClosed()
	{
		AvAvatar.SetUIActive(inActive: false);
		SetVisibility(inVisible: true);
		KAUI.SetExclusive(this);
		CreateEggWidgets();
		InitTutorial();
	}

	private void OnIAPStoreClosed()
	{
		ProcessSlotUnlock();
	}

	private void OnLockedSlotPurchase()
	{
		mKAUIGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _SlotPurchaseProcessingText.GetLocalizedString(), "", base.gameObject, null, null, null, null);
		KAUICursorManager.SetDefaultCursor("Loading");
		CommonInventoryData.pInstance.AddPurchaseItem(mHatcheryManager.GetNextIncubatorUnlockItem(), 1, ItemPurchaseSource.INCUBATOR.ToString());
		CommonInventoryData.pInstance.DoPurchase(2, m_GeneralStoreId, SlotUnlockHandler);
	}

	private void SlotUnlockHandler(CommonInventoryResponse ret)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		KillGenericDB();
		if (ret == null || !ret.Success)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _SlotPurchaseFailedText.GetLocalizedString(), "", base.gameObject, null, null, "KillGenericDB", null, inDestroyOnClick: true);
			return;
		}
		int currentUnlockedIncubatorCount = mHatcheryManager.GetCurrentUnlockedIncubatorCount();
		foreach (Incubator pIncubator in mHatcheryManager.pIncubators)
		{
			pIncubator.OnUnlockDone(currentUnlockedIncubatorCount);
		}
		mHatcheryManager.UpdateSlotUnlockCost(OnSlotUnlockCostDone);
		CreateIncubatorWidgets();
	}

	private void OnSlotUnlockCostDone()
	{
		for (int i = 0; i < mHatcheryManager.pIncubators.Count; i++)
		{
			if (mHatcheryManager.pIncubators[i].pReadyToBuy)
			{
				mHatchingSlotsMenu.SyncIncubatorWidget(mHatcheryManager.pIncubators[i]);
				break;
			}
		}
	}

	private void KillGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	public void ProcessEggClick(KAWidget widget)
	{
		mSelectedEgg = widget;
		mFirstEmptySlot = GetEmptyIncubatorSlot();
		if (mFirstEmptySlot != null)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _HatchEggConfirmationText.GetLocalizedString(), "", base.gameObject, "PlaceEgg", "KillGenericDB", null, null, inDestroyOnClick: true);
		}
		else
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _NoEmptyHatchingPedestalText.GetLocalizedString(), "", base.gameObject, null, null, "KillGenericDB", null, inDestroyOnClick: true);
		}
	}

	private KAWidget GetEmptyIncubatorSlot()
	{
		List<KAWidget> items = mHatchingSlotsMenu.GetItems();
		if (items == null || items.Count <= 0)
		{
			return null;
		}
		for (int i = 0; i < items.Count; i++)
		{
			IncubatorWidgetData obj = (IncubatorWidgetData)items[i].GetUserData();
			if (obj != null && obj.Incubator?.pMyState == Incubator.IncubatorStates.WAITING_FOR_EGG)
			{
				return items[i];
			}
		}
		return null;
	}

	private void PlaceEgg()
	{
		KillGenericDB();
		EggDisplayWidgetData eggDisplayWidgetData = mSelectedEgg.GetUserData() as EggDisplayWidgetData;
		mHatchingSlotsMenu.AddEggToIncubator(eggDisplayWidgetData.EggData, mFirstEmptySlot);
	}

	public void OnEggPick(KAWidget widget)
	{
		if ((EggDisplayWidgetData)widget.GetUserData() != null)
		{
			KAWidget kAWidget = CreateDragObject(widget, mEggsDisplayMenu.pPanel.depth + 1);
			kAWidget.FindChildItem("TxtEggCount").SetVisibility(inVisible: false);
			KAWidgetUserData userData = kAWidget.GetUserData();
			if (userData != null)
			{
				userData._Item = widget;
			}
		}
	}

	public void OnEggDropped(KAWidget widget)
	{
		KAWidget kAWidget = UICamera.hoveredObject?.GetComponent<KAWidget>();
		if (!(kAWidget?.GetUserData() is IncubatorWidgetData incubatorWidgetData) || incubatorWidgetData.Incubator == null)
		{
			return;
		}
		if (incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.WAITING_FOR_EGG)
		{
			if (widget.GetUserData() is EggDisplayWidgetData eggDisplayWidgetData)
			{
				mHatchingSlotsMenu.AddEggToIncubator(eggDisplayWidgetData.EggData, kAWidget);
			}
		}
		else
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _PlaceEggInRightSlotText.GetLocalizedString(), "", base.gameObject, null, null, "KillGenericDB", null, inDestroyOnClick: true);
		}
	}

	public void CheckForNestAssign(IncubatorWidgetData data)
	{
		if (!IsEmptyNestAvailable())
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _BuyStablesText.GetLocalizedString(), "", base.gameObject, "DisplayStablePurchaseStore", "KillGenericDB", null, null, inDestroyOnClick: true);
			return;
		}
		mMoveInIncubatorData = data;
		mMoveInPetData = RaisedPetData.GetHatchingPet(mMoveInIncubatorData.Incubator.pID);
		SetVisibility(inVisible: false);
		KAUI.RemoveExclusive(this);
		if (data.Incubator.pInitPetOnProximityStableHatch)
		{
			StableManager.pCurIncubatorID = data.Incubator.pID;
			data.Incubator.SetPetInProximityStableHatch();
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			UiDragonsStable.OpenStableListUI(base.gameObject);
		}
	}

	public bool IsEmptyNestAvailable()
	{
		bool result = false;
		StableData.UpdateInfo();
		for (int i = 0; i < StableData.pStableList.Count; i++)
		{
			if (StableData.pStableList[i].GetEmptyNest() != null)
			{
				result = true;
			}
		}
		return result;
	}

	public void DisplayStablePurchaseStore()
	{
		KillGenericDB();
		SetVisibility(inVisible: false);
		KAUI.RemoveExclusive(this);
		StoreLoader.Load(setDefaultMenuItem: true, m_StableStoreInfo._Category, m_StableStoreInfo._Store, base.gameObject);
	}

	private void OnStableUIOpened(UiDragonsStable UiStable)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		UiStable.pUiStablesInfoCard.pSelectedPetID = mMoveInPetData.RaisedPetID;
		UiStable.pUiStablesListCard.pCurrentMode = UiStablesListCard.Mode.NestAllocation;
		UiStable.pUiStablesInfoCard.SetCloseButtonVisibility(visible: false);
	}

	private void OnStableUIClosed()
	{
		if (StableData.GetByPetID(mMoveInPetData.RaisedPetID) != null)
		{
			if (mMoveInPetData.pIncubatorID >= 0)
			{
				mMoveInPetData.pIncubatorID = -1;
				mMoveInPetData.SaveDataReal();
			}
			mMoveInIncubatorData.Incubator.SetIncubatorState(Incubator.IncubatorStates.WAITING_FOR_EGG);
			mMoveInIncubatorData.Incubator.ResetIncubatorPet();
		}
		mMoveInIncubatorData = null;
		mMoveInPetData = null;
	}
}
