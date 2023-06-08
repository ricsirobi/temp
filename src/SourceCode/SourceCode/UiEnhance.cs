using System;
using System.Collections;
using UnityEngine;

public class UiEnhance : KAUI
{
	public LocaleString _DisplayText = new LocaleString("[REVIEW] Re-roll item for: [COUNT]");

	public LocaleString _PurchaseConfirmationText = new LocaleString("[REVIEW] Do you want to purchase a Scroll?");

	public EnhanceInfo _ItemEnhanceInfo;

	public EnhanceInfo _StatEnhanceInfo;

	public UiBlacksmith _UiBlackSmith;

	public UiItemStats _UiItemStats;

	public UiEnhanceProcessDB _UiEnhanceItemProcessDB;

	public UiEnhanceProcessDB _UiEnhanceStatProcessDB;

	public EffectsInfo[] _EffectsInfo;

	private KAWidget mBtnRerollItem;

	private KAWidget mBtnRerollStat;

	private KAWidget mMessageDisplay;

	private UserItemData mUserItemData;

	private KAUISelectItemData mWidgetData;

	private int mItemDataRequested;

	private Coroutine mEffectsCoroutine;

	private EffectsInfo mEffectsInfo;

	protected override void Start()
	{
		base.Start();
		mMessageDisplay = FindItem("MessageDisplay");
		mBtnRerollItem = FindItem("BtnReRollItem");
		mBtnRerollStat = FindItem("BtnReRollStat");
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == null)
		{
			return;
		}
		if (inWidget == mBtnRerollItem && mUserItemData != null)
		{
			if (_ItemEnhanceInfo.pEnhance)
			{
				_UiEnhanceItemProcessDB.ShowEnhanceDB(mUserItemData, _ItemEnhanceInfo, base.gameObject);
				_UiEnhanceItemProcessDB._UiItemStats.ShowPossibleStats(mUserItemData);
			}
			else
			{
				BuyItemEnhance();
			}
		}
		if (inWidget == mBtnRerollStat && mUserItemData != null)
		{
			if (_StatEnhanceInfo.pEnhance)
			{
				_UiEnhanceStatProcessDB.ShowEnhanceDB(mUserItemData, _StatEnhanceInfo, base.gameObject);
				_UiEnhanceStatProcessDB._UiItemStats.ShowStats(mUserItemData);
			}
			else
			{
				BuyStatEnhance();
			}
		}
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (kAUISelectItemData != null && kAUISelectItemData != mWidgetData)
		{
			mWidgetData = kAUISelectItemData;
			mUserItemData = CommonInventoryData.pInstance.FindItemByUserInventoryID(kAUISelectItemData._UserInventoryID);
			if (mUserItemData != null)
			{
				if (mUserItemData.ItemTier.HasValue)
				{
					UpdateEnhanceInfo(mUserItemData, _ItemEnhanceInfo, mBtnRerollItem);
					UpdateEnhanceInfo(mUserItemData, _StatEnhanceInfo, mBtnRerollStat);
					ShowUi(show: true);
				}
				else
				{
					ShowUi(show: true, disableRerollBtns: true);
				}
				UpdateMessageDisplay();
				_UiItemStats.ShowStats(mUserItemData, inWidget.GetTexture());
			}
		}
		StopEffects();
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			ShowUi(show: false);
		}
		mWidgetData = null;
	}

	public void ShowEffects(string rerollType, Action actionToExecute)
	{
		if (_EffectsInfo != null && _EffectsInfo.Length != 0)
		{
			StopEffects();
			mEffectsInfo = Array.Find(_EffectsInfo, (EffectsInfo x) => x._RerollType == rerollType);
			if (mEffectsInfo != null)
			{
				mEffectsInfo.pAction = actionToExecute;
				mEffectsCoroutine = StartCoroutine("ShowNewStats");
			}
		}
	}

	public void StopEffects()
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

	private void ShowUi(bool show, bool disableRerollBtns = false)
	{
		if (mBtnRerollItem != null)
		{
			mBtnRerollItem.SetVisibility(show);
			mBtnRerollItem.SetDisabled(disableRerollBtns);
		}
		if (mBtnRerollStat != null)
		{
			mBtnRerollStat.SetVisibility(show);
			mBtnRerollStat.SetDisabled(disableRerollBtns);
		}
		if (mMessageDisplay != null)
		{
			mMessageDisplay.SetVisibility(show);
		}
		if (_UiItemStats != null)
		{
			_UiItemStats.SetVisibility(show);
		}
		if (!show)
		{
			StopEffects();
		}
	}

	private void UpdateEnhanceInfo(UserItemData userItemData, EnhanceInfo enhanceInfo, KAWidget inWidget)
	{
		bool flag = false;
		TierMap[] tierMap = enhanceInfo._TierMap;
		foreach (TierMap tierMap2 in tierMap)
		{
			RarityMap[] rarityMap = tierMap2._RarityMap;
			foreach (RarityMap rarityMap2 in rarityMap)
			{
				if (tierMap2._Tier == userItemData.ItemTier && rarityMap2._Rarity == userItemData.Item.ItemRarity)
				{
					flag = true;
					SetEnhanceInfo(enhanceInfo, inWidget, rarityMap2._Quantity);
					break;
				}
			}
		}
		if (!flag)
		{
			SetEnhanceInfo(enhanceInfo, inWidget);
		}
	}

	private void SetEnhanceInfo(EnhanceInfo enhanceInfo, KAWidget inWidget, int shardQuantity = 0)
	{
		UserItemData userItemData2 = (enhanceInfo.pUserItemData = CommonInventoryData.pInstance.FindItem(enhanceInfo._ItemId));
		enhanceInfo.pEnhance = userItemData2 != null && userItemData2.Quantity >= shardQuantity;
		enhanceInfo.pConsumeCount = shardQuantity;
		if (userItemData2 != null)
		{
			enhanceInfo.pCountText = enhanceInfo.pConsumeCount + " " + ((enhanceInfo.pConsumeCount == 1) ? userItemData2.Item.ItemName : userItemData2.Item.ItemNamePlural);
		}
		else
		{
			if (mItemDataRequested == 0)
			{
				KAUICursorManager.SetExclusiveLoadingGear(status: true);
			}
			mItemDataRequested++;
			ItemData.Load(enhanceInfo._ItemId, OnEnhanceItemLoaded, enhanceInfo);
		}
		inWidget.FindChildItem("Count").SetText((userItemData2?.Quantity ?? 0).ToString() ?? "");
	}

	private void OnEnhanceItemLoaded(int itemId, ItemData itemData, object userData)
	{
		if (itemData != null)
		{
			EnhanceInfo enhanceInfo = (EnhanceInfo)userData;
			enhanceInfo.pCountText = enhanceInfo.pConsumeCount + " " + ((enhanceInfo.pConsumeCount == 1) ? itemData.ItemName : itemData.ItemNamePlural);
			UpdateMessageDisplay();
		}
		mItemDataRequested--;
		if (mItemDataRequested <= 0)
		{
			KAUICursorManager.SetExclusiveLoadingGear(status: false);
		}
	}

	private void UpdateMessageDisplay()
	{
		if (mMessageDisplay != null)
		{
			string text = _DisplayText.GetLocalizedString().Replace("[ICOUNT]", _ItemEnhanceInfo.pCountText);
			text = text.Replace("[SCOUNT]", _StatEnhanceInfo.pCountText);
			mMessageDisplay.SetText(text);
			mMessageDisplay.SetVisibility(!text.Contains("COUNT"));
		}
	}

	private void OnStatRerollDone()
	{
		ResetUi();
		_UiBlackSmith.UpdateShards();
	}

	private void OnItemRerollDone()
	{
		ShowEffects("Item", ResetUi);
		_UiBlackSmith.UpdateShards();
	}

	private void ResetUi()
	{
		_UiItemStats.UpdateStats();
		UpdateEnhanceInfo(mUserItemData, _ItemEnhanceInfo, mBtnRerollItem);
		UpdateEnhanceInfo(mUserItemData, _StatEnhanceInfo, mBtnRerollStat);
	}

	private void BuyItemEnhance()
	{
		PurchaseItem(_ItemEnhanceInfo._ItemId, _ItemEnhanceInfo._StoreId);
	}

	private void BuyStatEnhance()
	{
		PurchaseItem(_StatEnhanceInfo._ItemId, _StatEnhanceInfo._StoreId);
	}

	private void PurchaseItem(int itemID, int storeID)
	{
		UiItemTradeGenericDB uiItemTradeGenericDB = (UiItemTradeGenericDB)GameUtilities.CreateKAUIGenericDB("PfUiBlacksmithTradeDB", "ItemTradeDB");
		if (uiItemTradeGenericDB != null)
		{
			uiItemTradeGenericDB.SetText(_PurchaseConfirmationText.GetLocalizedString(), interactive: false);
			uiItemTradeGenericDB._MessageObject = base.gameObject;
			uiItemTradeGenericDB.SetMode(UiItemTradeGenericDB.TradeType.Buy, itemID, storeID, ItemPurchaseSource.ENHANCE.ToString());
		}
	}

	private void OnItemPurchaseComplete()
	{
		UpdateEnhanceInfo(mUserItemData, _ItemEnhanceInfo, mBtnRerollItem);
		UpdateEnhanceInfo(mUserItemData, _StatEnhanceInfo, mBtnRerollStat);
		_UiBlackSmith.UpdateShards();
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
		yield return new WaitForSeconds(mEffectsInfo._EffectsDuration - mEffectsInfo._StatsUpdateDelay);
		mEffectsInfo.EnableEffects(enable: false, mEffectsInfo._RestrictSFXDuration);
		mEffectsCoroutine = null;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		StopEffects();
	}
}
