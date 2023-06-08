using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiFuse : KAUISelect
{
	public enum FuseInteraction
	{
		ValidItem,
		SlotOccupied,
		InvalidItem,
		SlotDisabled
	}

	public LocaleString _FuseItemsConfirmationText = new LocaleString("[REVIEW] Are you sure you want to fuse your items ?");

	public LocaleString _BluePrintFailtureHeaderText = new LocaleString("[REVIEW] Blue Print Failed");

	public LocaleString _BluePrintFailtureText = new LocaleString("[REVIEW] Blue Print Failed To Load!!");

	public LocaleString _FuseFailtureHeaderText = new LocaleString("[REVIEW] Fuse Failed");

	public LocaleString _FuseFailtureText = new LocaleString("[REVIEW] Fuse Failed!!");

	public LocaleString _PurchaseConfirmationText = new LocaleString("[REVIEW] Do you want to purchase a Shard?");

	public LocaleString _NotEnoughCoinsText = new LocaleString("[REVIEW] You do not have enough coins to fuse, Do you want to purchase ?");

	public EffectsInfo[] _EffectsInfo;

	public UiBlacksmith _UiBlackSmith;

	private KAWidget mBtnFuseItems;

	private KAWidget mBtnCancel;

	private UiFuseInfo mFuseInfo;

	private Coroutine mEffectsCoroutine;

	private EffectsInfo mEffectsInfo;

	private UiItemStats mUiItemsStats;

	private List<UserItemData> mIngredients;

	private KAUIGenericDB mGenericDB;

	private int mShardCount;

	private int mCoinCount;

	private int mCreatedInventoryID;

	protected override void Start()
	{
		mBtnFuseItems = FindItem("BtnFuse");
		mBtnCancel = FindItem("BtnCancel");
		mFuseInfo = (UiFuseInfo)_UiList[0];
		mUiItemsStats = (UiItemStats)_UiList[1];
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		StopEffects();
	}

	public override void OnOpen()
	{
		EnableCancelButton(enable: false);
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			Initialize();
		}
		else if (mFuseInfo != null)
		{
			ResetFuse();
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnFuseItems)
		{
			if (FuseValidateCheck())
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _FuseItemsConfirmationText.GetLocalizedString(), "", base.gameObject, "OnFuseItems", "OnDBClose", "", "", inDestroyOnClick: true);
			}
		}
		else if (inWidget == mBtnCancel)
		{
			if (mFuseInfo.GetVisibility())
			{
				ResetFuse();
			}
		}
		else if (mKAUiSelectMenu.GetItems().Contains(inWidget))
		{
			BlueprintSelected(inWidget);
		}
		else
		{
			ShowItemStats(inWidget);
		}
	}

	public void ShowItemStats(KAWidget widget)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)widget.GetUserData();
		if (kAUISelectItemData != null)
		{
			if (kAUISelectItemData._UserItemData != null)
			{
				mUiItemsStats.ShowStats(kAUISelectItemData._UserItemData, widget.GetTexture());
			}
			else if (kAUISelectItemData._ItemData != null)
			{
				mUiItemsStats.ShowStats(kAUISelectItemData._ItemData);
			}
		}
	}

	public override KAWidget AddEmptySlot()
	{
		KAWidget kAWidget = mKAUiSelectMenu.AddWidget("EmptySlot");
		if (kAWidget != null)
		{
			AddWidgetData(kAWidget, null);
		}
		return kAWidget;
	}

	public void EnableFuseButton(bool enable)
	{
		mBtnFuseItems.SetDisabled(!enable);
	}

	public void EnableCancelButton(bool enable)
	{
		mBtnCancel.SetDisabled(!enable);
	}

	public void ShowBlueprintFailDB()
	{
		if (mGenericDB == null)
		{
			mGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _BluePrintFailtureText.GetLocalizedString(), _BluePrintFailtureHeaderText.GetLocalizedString(), base.gameObject, "", "", "DestroyDB", "", inDestroyOnClick: true);
		}
	}

	public void ShowFuseFailDB()
	{
		if (mGenericDB == null)
		{
			mGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _FuseFailtureText.GetLocalizedString(), _FuseFailtureHeaderText.GetLocalizedString(), base.gameObject, "", "", "DestroyDB", "", inDestroyOnClick: true);
		}
	}

	private void BlueprintSelected(KAWidget inWidget)
	{
		if (inWidget != null)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
			if (mFuseInfo != null && kAUISelectItemData != null)
			{
				_UiBlackSmith.OnBluePrintSelected(kAUISelectItemData._ItemData.BluePrint);
				mFuseInfo.ShowInfo(kAUISelectItemData);
				EnableCancelButton(enable: true);
			}
		}
	}

	private void PurchaseItem(int itemID, int storeID)
	{
		UiItemTradeGenericDB uiItemTradeGenericDB = (UiItemTradeGenericDB)GameUtilities.CreateKAUIGenericDB("PfUiBlacksmithTradeDB", "ItemTradeDB");
		if (uiItemTradeGenericDB != null)
		{
			uiItemTradeGenericDB.SetText(_PurchaseConfirmationText.GetLocalizedString(), interactive: false);
			uiItemTradeGenericDB._MessageObject = base.gameObject;
			uiItemTradeGenericDB.SetMode(UiItemTradeGenericDB.TradeType.Buy, itemID, storeID, ItemPurchaseSource.FUSE.ToString());
		}
	}

	protected override void OnItemPurchaseComplete()
	{
		base.OnItemPurchaseComplete();
		_UiBlackSmith.UpdateShards();
	}

	private bool FuseValidateCheck()
	{
		mShardCount = 0;
		mCoinCount = 0;
		if (mFuseInfo.BlueprintWidgetData._ItemData.BluePrint.Deductibles != null && mFuseInfo.BlueprintWidgetData._ItemData.BluePrint.Deductibles.Count > 0)
		{
			foreach (BluePrintDeductibleConfig deductible in mFuseInfo.BlueprintWidgetData._ItemData.BluePrint.Deductibles)
			{
				if (deductible.DeductibleType == DeductibleType.Item && deductible.ItemID == _UiBlackSmith._ShardItemId)
				{
					mShardCount += deductible.Quantity;
				}
				else if (deductible.DeductibleType == DeductibleType.Coins)
				{
					mCoinCount += deductible.Quantity;
				}
			}
			if (_UiBlackSmith.ShardItemData == null || _UiBlackSmith.ShardItemData.Quantity < mShardCount)
			{
				PurchaseItem(_UiBlackSmith._ShardItemId, _UiBlackSmith._ShardStoreId);
				return false;
			}
			if (Money.pGameCurrency < mCoinCount)
			{
				ShowConfirmationDB(_NotEnoughCoinsText, "OnBuyCoins");
				return false;
			}
		}
		return true;
	}

	private void ShowConfirmationDB(LocaleString message, string callbackMessage)
	{
		KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "ConfirmationDB");
		kAUIGenericDB.SetMessage(base.gameObject, callbackMessage, "OnCloseDB", null, null);
		kAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		kAUIGenericDB.SetText(message.GetLocalizedString(), interactive: false);
		kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		KAUI.SetExclusive(kAUIGenericDB);
	}

	private void OnBuyCoins()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.COINS, base.gameObject);
	}

	private void OnFuseItems()
	{
		KAUICursorManager.SetExclusiveLoadingGear(status: true);
		FuseItemsRequest fuseItemsRequest = new FuseItemsRequest();
		if (mFuseInfo.BlueprintWidgetData._UserItemData != null && mFuseInfo.BlueprintWidgetData._UserItemData.UserInventoryID > 0)
		{
			fuseItemsRequest.BluePrintInventoryID = mFuseInfo.BlueprintWidgetData._UserItemData.UserInventoryID;
		}
		else
		{
			fuseItemsRequest.BluePrintItemID = mFuseInfo.BlueprintWidgetData._ItemID;
		}
		List<BluePrintFuseItemMap> list = new List<BluePrintFuseItemMap>();
		mIngredients = new List<UserItemData>();
		for (int i = 0; i < mFuseInfo.pKAUiSelectMenu.GetItems().Count; i++)
		{
			KAWidget kAWidget = mFuseInfo.pKAUiSelectMenu.GetItems()[i];
			KAWidget kAWidget2 = mFuseInfo.pKAUiSelectMenu.GetItems()[i].pChildWidgets[0];
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)kAWidget.GetUserData();
			KAUIFuseItemData kAUIFuseItemData = (KAUIFuseItemData)kAWidget2.GetUserData();
			BluePrintFuseItemMap bluePrintFuseItemMap = new BluePrintFuseItemMap();
			bluePrintFuseItemMap.BluePrintSpecID = kAUIFuseItemData.BluePrintSpecID;
			bluePrintFuseItemMap.UserInventoryID = kAUISelectItemData._UserInventoryID;
			mIngredients.Add(kAUISelectItemData._UserItemData);
			list.Add(bluePrintFuseItemMap);
		}
		List<DeductibleItemInventoryMap> list2 = new List<DeductibleItemInventoryMap>();
		if (mFuseInfo.BlueprintWidgetData._ItemData.BluePrint.Deductibles.Count > 0)
		{
			foreach (BluePrintDeductibleConfig deductible in mFuseInfo.BlueprintWidgetData._ItemData.BluePrint.Deductibles)
			{
				if (deductible.DeductibleType == DeductibleType.Item && deductible.ItemID == _UiBlackSmith._ShardItemId && deductible.Quantity > 0)
				{
					DeductibleItemInventoryMap deductibleItemInventoryMap = new DeductibleItemInventoryMap();
					deductibleItemInventoryMap.ItemID = _UiBlackSmith._ShardItemId;
					deductibleItemInventoryMap.UserInventoryID = _UiBlackSmith.ShardItemData.UserInventoryID;
					deductibleItemInventoryMap.Quantity = deductible.Quantity;
					list2.Add(deductibleItemInventoryMap);
				}
			}
		}
		fuseItemsRequest.DeductibleItemInventoryMaps = list2;
		fuseItemsRequest.BluePrintFuseItemMaps = list;
		fuseItemsRequest.Locale = UtUtilities.GetLocaleLanguage();
		fuseItemsRequest.AvatarGender = AvatarData.GetGender();
		WsWebService.FuseItems(fuseItemsRequest, OnItemsFused, null);
	}

	private void OnItemsFused(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			KAUICursorManager.SetExclusiveLoadingGear(status: false);
			ShowFuseFailDB();
			ResetFuse();
			UtDebug.LogError("FUSE: " + inType.ToString() + " failed!!!!");
			break;
		case WsServiceEvent.COMPLETE:
			KAUICursorManager.SetExclusiveLoadingGear(status: false);
			if (inObject != null)
			{
				FuseItemsResponse fuseItemsResponse = (FuseItemsResponse)inObject;
				if (fuseItemsResponse.Status == Status.Success)
				{
					if (mShardCount > 0)
					{
						CommonInventoryData.pInstance.RemoveItem(_UiBlackSmith._ShardItemId, updateServer: false, mShardCount);
						_UiBlackSmith.UpdateShards();
					}
					UtDebug.Log("FUSE: Shard count updated by: " + mShardCount + " for: " + mFuseInfo.BlueprintWidgetData._ItemData.ItemName);
					if (mCoinCount > 0)
					{
						Money.AddToGameCurrency(-mCoinCount);
					}
					if (mFuseInfo.BlueprintWidgetData._UserItemData != null)
					{
						if (mFuseInfo.BlueprintWidgetData._UserItemData.UserInventoryID > 0)
						{
							CommonInventoryData.pInstance.RemoveItem(mFuseInfo.BlueprintWidgetData._UserItemData, 1);
							UtDebug.Log("FUSE: item count update for: " + mFuseInfo.BlueprintWidgetData._UserItemData.Item.ItemName + " Count: " + mFuseInfo.BlueprintWidgetData._UserItemData.Quantity);
						}
						else
						{
							UtDebug.LogError("FUSE: UserInventoryID invalid for: " + mFuseInfo.BlueprintWidgetData._UserItemData.Item.ItemName);
						}
					}
					else
					{
						UtDebug.LogError("FUSE: UserItemData is NULL");
					}
					foreach (UserItemData mIngredient in mIngredients)
					{
						CommonInventoryData.pInstance.RemoveItem(mIngredient, 1);
					}
					if (fuseItemsResponse.InventoryItemStatsMaps != null && fuseItemsResponse.InventoryItemStatsMaps.Count > 0)
					{
						UserItemData[] itemData = new UserItemData[fuseItemsResponse.InventoryItemStatsMaps.Count];
						int num = 0;
						foreach (InventoryItemStatsMap inventoryItemStatsMap in fuseItemsResponse.InventoryItemStatsMaps)
						{
							itemData[num] = new UserItemData();
							itemData[num].UserInventoryID = inventoryItemStatsMap.CommonInventoryID;
							itemData[num].Item = inventoryItemStatsMap.Item;
							itemData[num].Item.ItemStatsMap = inventoryItemStatsMap.ItemStatsMap;
							itemData[num].ItemStats = inventoryItemStatsMap.ItemStatsMap.ItemStats;
							itemData[num].ItemTier = inventoryItemStatsMap.ItemStatsMap.ItemTier;
							itemData[num].ItemID = inventoryItemStatsMap.ItemStatsMap.ItemID;
							itemData[num].Quantity = 1;
							CommonInventoryData.pInstance.AddToCategories(itemData[num]);
							UtDebug.Log("FUSE: Item Fused: " + itemData[num].Item.ItemName);
							string partName = AvatarData.GetPartName(inventoryItemStatsMap.Item);
							if (MissionManager.pInstance != null)
							{
								MissionManager.pInstance.CheckForTaskCompletion("Action", "FuseItem", partName);
								MissionManager.pInstance.CheckForTaskCompletion("Action", "FuseItem", itemData[num].Item.ItemName);
							}
							num++;
						}
						ResetFuse();
						mCreatedInventoryID = fuseItemsResponse.InventoryItemStatsMaps[fuseItemsResponse.InventoryItemStatsMaps.Count - 1].CommonInventoryID;
						ShowEffects("Fuse", delegate
						{
							_UiBlackSmith.CheckItemCustomization(itemData);
						});
						Initialize();
					}
					else
					{
						UtDebug.LogError("FUSE: Item Stats Map Missing For Fused Item!");
					}
				}
				else
				{
					ShowFuseFailDB();
					if (fuseItemsResponse.VMsg != null)
					{
						UtDebug.LogError("FUSE: Fuse Failed: " + fuseItemsResponse.VMsg.Status.ToString() + "  " + fuseItemsResponse.VMsg.Message + " " + fuseItemsResponse.UserID.ToString());
					}
				}
			}
			else
			{
				ShowFuseFailDB();
				ResetFuse();
				UtDebug.LogError("FUSE: " + inType.ToString() + " did not return valid object!!!!");
			}
			break;
		}
	}

	private void ResetFuse()
	{
		EnableCancelButton(enable: false);
		mKAUiSelectMenu.SetSelectedItem(null);
		_UiBlackSmith.OnBluePrintSelected(null);
		EnableFuseButton(enable: false);
		mFuseInfo.SetVisibility(inVisible: false);
	}

	private void ShowEffects(string type, Action actionToExecute)
	{
		if (_EffectsInfo != null && _EffectsInfo.Length != 0)
		{
			StopEffects();
			mEffectsInfo = Array.Find(_EffectsInfo, (EffectsInfo x) => x._RerollType == type);
			if (mEffectsInfo != null)
			{
				mEffectsInfo.pAction = actionToExecute;
				mEffectsCoroutine = StartCoroutine("ShowNewStats");
			}
		}
	}

	private void StopEffects()
	{
		if (mEffectsCoroutine == null)
		{
			return;
		}
		StopCoroutine("ShowNewStats");
		mEffectsCoroutine = null;
		if (mEffectsInfo != null)
		{
			mEffectsInfo.EnableEffects(enable: false);
			if (mEffectsInfo.pAction != null)
			{
				mEffectsInfo.pAction();
			}
			mEffectsInfo.pAction = null;
			mEffectsInfo = null;
		}
	}

	private IEnumerator ShowNewStats()
	{
		if (mEffectsInfo == null)
		{
			mEffectsCoroutine = null;
			yield break;
		}
		yield return new WaitForSeconds(mEffectsInfo._EffectsStartDelay);
		mEffectsInfo.EnableEffects(enable: true);
		yield return new WaitForSeconds(mEffectsInfo._StatsUpdateDelay);
		if (mEffectsInfo.pAction != null)
		{
			mEffectsInfo.pAction();
			mEffectsInfo.pAction = null;
		}
		_UiBlackSmith.SelectItem(mCreatedInventoryID);
		yield return new WaitForSeconds(mEffectsInfo._EffectsDuration - mEffectsInfo._StatsUpdateDelay);
		mEffectsInfo.EnableEffects(enable: false, mEffectsInfo._RestrictSFXDuration);
		mEffectsCoroutine = null;
	}

	private void DestroyDB()
	{
		if (mGenericDB != null)
		{
			UnityEngine.Object.Destroy(mGenericDB.gameObject);
		}
		ResetFuse();
	}
}
