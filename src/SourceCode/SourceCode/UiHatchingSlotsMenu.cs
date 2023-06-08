using System.Collections.Generic;
using UnityEngine;

public class UiHatchingSlotsMenu : KAUIMenu
{
	private UiMultiEggHatching mUiMultiEggHatching;

	private KAWidget mInteractingWidget;

	protected override void Start()
	{
		base.Start();
		mUiMultiEggHatching = (UiMultiEggHatching)_ParentUi;
	}

	public void SetupIncubatorWidget(KAWidget widget)
	{
		if (!mUiMultiEggHatching && (bool)_ParentUi)
		{
			mUiMultiEggHatching = (UiMultiEggHatching)_ParentUi;
		}
		IncubatorWidgetData incubatorWidgetData = (IncubatorWidgetData)widget.GetUserData();
		if (widget is HatchSlotWidget)
		{
			((HatchSlotWidget)widget).UpdateIncubator(incubatorWidgetData);
		}
		if (incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.LOCKED)
		{
			widget.FindChildItem("LockedIcon").SetVisibility(!incubatorWidgetData.Incubator.pReadyToBuy);
			if (incubatorWidgetData.Incubator.pReadyToBuy)
			{
				KAWidget kAWidget = widget.FindChildItem("TxtCost");
				kAWidget.SetText(mUiMultiEggHatching.pHatcheryManager.pSlotUnlockCost.ToString());
				kAWidget.SetVisibility(inVisible: true);
			}
		}
		if (incubatorWidgetData.Incubator.pReadyToBuy)
		{
			return;
		}
		widget.FindChildItem("Loading").SetVisibility(inVisible: false);
		widget.FindChildItem("Quantity").SetVisibility(inVisible: false);
		widget.FindChildItem("BtnReady").SetVisibility(incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.WAITING_FOR_EGG);
		widget.FindChildItem("BtnSpeedUp").SetVisibility(incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.HATCHING);
		widget.FindChildItem("BtnHatch").SetVisibility(incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.HATCHED);
		widget.FindChildItem("BtnMoveIn").SetVisibility(incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.IDLE);
		KAWidget kAWidget2 = widget.FindChildItem("TxtTimer");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.HATCHING);
			if (kAWidget2.GetVisibility())
			{
				kAWidget2.SetText(incubatorWidgetData.Incubator.GetStatusText(incubatorWidgetData.Incubator.GetHatchTimeLeft()));
			}
		}
		KAWidget kAWidget3 = widget.FindChildItem("Icon");
		if (incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.HATCHED || incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.HATCHING || incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.IDLE)
		{
			SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(incubatorWidgetData.Incubator.mPetType);
			_ = sanctuaryPetTypeInfo._EggIconPath;
			widget.FindChildItem("Loading").SetVisibility(inVisible: true);
			kAWidget3.SetTextureFromBundle(sanctuaryPetTypeInfo._EggIconPath, null, OnEggIconDownload);
		}
		else
		{
			kAWidget3.SetVisibility(inVisible: false);
		}
	}

	private void OnEggIconDownload(KAWidget widget, bool success)
	{
		widget.SetVisibility(inVisible: true);
		widget.pParentWidget.FindChildItem("Loading").SetVisibility(inVisible: false);
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (UICursorManager.GetCursorName() == "Loading")
		{
			return;
		}
		base.OnClick(inWidget);
		IncubatorWidgetData incubatorWidgetData = (IncubatorWidgetData)inWidget.GetUserData();
		if (incubatorWidgetData == null)
		{
			incubatorWidgetData = (IncubatorWidgetData)inWidget.pParentWidget.GetUserData();
		}
		if (incubatorWidgetData == null || !(incubatorWidgetData.Incubator != null))
		{
			return;
		}
		if (incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.LOCKED)
		{
			mUiMultiEggHatching.ProcessSlotUnlock(incubatorWidgetData.Incubator);
		}
		if (incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.WAITING_FOR_EGG)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", mUiMultiEggHatching._DragEggToSlotText.GetLocalizedString(), "", base.gameObject, null, null, "OnClosePrompt", null, inDestroyOnClick: true);
		}
		else if (incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.HATCHING)
		{
			mUiMultiEggHatching.pUiHatchingSpeedUp.Init(base.gameObject, incubatorWidgetData.Incubator);
			mInteractingWidget = incubatorWidgetData._Item;
		}
		else if (incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.HATCHED)
		{
			if (RsResourceManager.pCurrentLevel != "HatcheryINTDO")
			{
				HatcheryManager.pIncubatorHatchID = incubatorWidgetData.Incubator.pID;
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", mUiMultiEggHatching._GoToHatcheryAndHatchText.GetLocalizedString(), null, base.gameObject, "GoToHatchery", "OnClosePrompt", "", "", inDestroyOnClick: true);
			}
			else if (mUiMultiEggHatching.IsEmptyNestAvailable())
			{
				if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
				{
					SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
				}
				mUiMultiEggHatching.Exit();
				if ((bool)mUiMultiEggHatching._HatcheryManager)
				{
					AvAvatar.SetPosition(GameObject.Find(mUiMultiEggHatching._HatcheryMarker).transform);
				}
				incubatorWidgetData.Incubator.PickUpEgg(fromStable: false);
			}
			else
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", mUiMultiEggHatching._BuyStablesText.GetLocalizedString(), "", mUiMultiEggHatching.gameObject, "DisplayStablePurchaseStore", "OnClosePrompt", null, null, inDestroyOnClick: true);
			}
		}
		else if (incubatorWidgetData.Incubator.pMyState == Incubator.IncubatorStates.IDLE)
		{
			mUiMultiEggHatching.CheckForNestAssign(incubatorWidgetData);
		}
	}

	public void GoToHatchery()
	{
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
		}
		mUiMultiEggHatching.Exit();
		AvAvatar.pStartLocation = mUiMultiEggHatching._HatcheryMarker;
		RsResourceManager.LoadLevel("HatcheryINTDO");
	}

	private void OnClosePrompt()
	{
		HatcheryManager.pIncubatorHatchID = 0;
	}

	public void AddEggToIncubator(UserItemData egg, KAWidget incubatorSlot)
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		if (incubatorSlot.GetUserData() is IncubatorWidgetData incubatorWidgetData && incubatorWidgetData.Incubator != null && egg != null)
		{
			string iconName = egg.Item.IconName;
			KAWidget kAWidget = incubatorSlot.FindChildItem("Icon");
			if (kAWidget != null)
			{
				incubatorSlot.FindChildItem("Loading").SetVisibility(inVisible: true);
				kAWidget.SetTextureFromBundle(iconName, null, OnEggIconDownload);
			}
			incubatorWidgetData.Incubator.CheckEggSelected(egg.Item.ItemName, OnEggPlaced);
			mInteractingWidget = incubatorSlot;
		}
	}

	private void OnEggPlaced(bool success, Incubator incubator)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetupIncubatorWidget(mInteractingWidget);
		mInteractingWidget = null;
		if (success)
		{
			mUiMultiEggHatching.CreateEggWidgets();
		}
	}

	private void OnSpeedUpDone(bool success)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (success)
		{
			SetupIncubatorWidget(mInteractingWidget);
		}
		mInteractingWidget = null;
	}

	public void SyncIncubatorWidget(Incubator incubator)
	{
		if (incubator == null)
		{
			return;
		}
		List<KAWidget> items = GetItems();
		if (!((items != null) & (items.Count > 0)))
		{
			return;
		}
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].GetUserData() is IncubatorWidgetData incubatorWidgetData && incubatorWidgetData.Incubator != null && incubatorWidgetData.Incubator.pID == incubator.pID)
			{
				SetupIncubatorWidget(items[i]);
				break;
			}
		}
	}
}
