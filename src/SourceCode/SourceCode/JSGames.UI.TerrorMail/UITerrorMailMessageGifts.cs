using System;
using System.Collections;
using UnityEngine;

namespace JSGames.UI.TerrorMail;

public class UITerrorMailMessageGifts : UIMessagePopulator
{
	public UIMenu _GiftMenu;

	public InventoryTab _BattleReadyTab;

	public LocaleString _UpsellText = new LocaleString("Your gift cannot be claimed as your inventory is full!\nDo you want to purchase more slots?");

	public int _BackpackSlotUpsellItemID = 13717;

	public int _BackpackSlotUpsellItemStoreID = 91;

	private UIUpsell mUIUpsell;

	private MessageInfo mMessageInfo;

	private GiftData mGiftData;

	private bool mGiftClaimed;

	public Texture _GemImage;

	public Texture _CoinImage;

	public UIWidget _ClaimBtn;

	public UIWidget _DeleteBtn;

	private int mEmptySlotCount;

	private int mBattleReadyItemCount;

	public Action<MessageInfo> OnClaim;

	public GiftData pGiftData
	{
		get
		{
			return mGiftData;
		}
		set
		{
			mGiftData = value;
		}
	}

	private bool mSlotsAvailable
	{
		get
		{
			if (mEmptySlotCount > 0)
			{
				return mBattleReadyItemCount <= mEmptySlotCount;
			}
			return false;
		}
	}

	public void Initialize(MessageInfo inMessageInfo)
	{
		_ClaimBtn.pVisible = false;
		_DeleteBtn.pVisible = false;
		if (inMessageInfo != null && (bool)GiftManager.pInstance && GiftManager.pInstance._Gifts != null)
		{
			mGiftData = GiftManager.pInstance._Gifts.Find((GiftData t) => t.MessageID == inMessageInfo.MessageID);
			mGiftClaimed = GiftManager.pInstance.GetMessageTag(mGiftData, "Claim");
		}
		if (mGiftData == null)
		{
			UtDebug.LogError("No gift data found for " + inMessageInfo);
			OnClose();
			return;
		}
		_GiftMenu.ClearChildren();
		mMessageInfo = inMessageInfo;
		Populate(mMessageInfo);
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		InventorySetting.TabData tabData = InventorySetting.pInstance.GetTabData(_BattleReadyTab._TabID);
		mEmptySlotCount += tabData.GetTotalSlots() - tabData.GetOccupiedSlots();
		LoadItemData();
	}

	private void LoadItemData()
	{
		base.pParentUI.pState = WidgetState.NOT_INTERACTIVE;
		foreach (GiftSetup item in mGiftData.GiftSetup)
		{
			if (item.Type == GiftType.Item)
			{
				ItemData.Load(int.Parse(item.Value), OnLoadItemData, item);
			}
			else
			{
				CreateWidget(null, item);
			}
		}
		StartCoroutine(WaitForItemsLoad());
	}

	private IEnumerator WaitForItemsLoad()
	{
		while (ItemData.pLoadingList.Count != 0)
		{
			yield return new WaitForEndOfFrame();
		}
		pVisible = true;
		foreach (UIWidget pChildWidget in _GiftMenu.pChildWidgets)
		{
			pChildWidget.pVisible = true;
		}
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		_ClaimBtn.pVisible = !mGiftClaimed;
		_DeleteBtn.pVisible = mGiftClaimed;
		base.pParentUI.pState = WidgetState.INTERACTIVE;
		yield return null;
	}

	private void OnLoadItemData(int itemID, ItemData dataItem, object inUserData)
	{
		if (ValidateGender(dataItem))
		{
			CreateWidget(dataItem, inUserData);
		}
		if (dataItem.ItemStatsMap != null && dataItem.ItemStatsMap.ItemStats != null && dataItem.ItemStatsMap.ItemStats.Length != 0)
		{
			mBattleReadyItemCount++;
		}
		else if (itemID == _BackpackSlotUpsellItemID)
		{
			mEmptySlotCount++;
		}
	}

	private void CreateWidget(ItemData dataItem, object inUserData)
	{
		GiftSetup giftSetup = inUserData as GiftSetup;
		UIWidget uIWidget = _GiftMenu.AddWidget(_GiftMenu._Template.name);
		uIWidget.pVisible = false;
		uIWidget.pText = giftSetup.Quantity.ToString();
		if (giftSetup.Type == GiftType.Item && dataItem != null)
		{
			uIWidget.SetImage(dataItem.IconName);
		}
		else
		{
			uIWidget._RawImageBackground.texture = ((giftSetup.Type == GiftType.Gems) ? _GemImage : _CoinImage);
		}
		uIWidget.pText = giftSetup.Quantity.ToString();
		uIWidget.gameObject.SetActive(value: true);
		uIWidget.pState = ((!mGiftClaimed) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
	}

	private bool ValidateGender(ItemData item)
	{
		if (item == null)
		{
			return false;
		}
		string text = "U";
		if (AvatarData.GetGender() == Gender.Male)
		{
			text = "M";
		}
		else if (AvatarData.GetGender() == Gender.Female)
		{
			text = "F";
		}
		string attribute = item.GetAttribute("Gender", "U");
		if (!(attribute == text))
		{
			return attribute == "U";
		}
		return true;
	}

	public void AttemptClaim()
	{
		if (mBattleReadyItemCount == 0 || mSlotsAvailable)
		{
			ClaimGifts();
			return;
		}
		base.pParentUI.pState = WidgetState.NOT_INTERACTIVE;
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("UpsellAsset"), OnUpsellLoaded, typeof(GameObject), inDontDestroy: false, typeof(GameObject));
	}

	private void OnUpsellLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			mUIUpsell = UnityEngine.Object.Instantiate((GameObject)inObject).GetComponent<UIUpsell>();
			if (mUIUpsell == null)
			{
				base.pParentUI.pState = WidgetState.INTERACTIVE;
				break;
			}
			mUIUpsell.pUpsellText._Text = _UpsellText.GetLocalizedString();
			mUIUpsell.pUpsellCurrentItemAmount = InventorySetting.pInstance.GetTabData(_BattleReadyTab._TabID).GetOccupiedSlots();
			mUIUpsell.pUpsellPurchaseCount = mBattleReadyItemCount - mEmptySlotCount;
			mUIUpsell.pUpsellItemID = _BackpackSlotUpsellItemID;
			mUIUpsell.pUpsellItemStoreID = _BackpackSlotUpsellItemStoreID;
			mUIUpsell.Initialize();
			mUIUpsell.SetExclusive();
			UIUpsell uIUpsell = mUIUpsell;
			uIUpsell.OnClose = (Action<bool, int>)Delegate.Combine(uIUpsell.OnClose, new Action<bool, int>(OnSlotPurchaseClosed));
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Upsell failed to load!");
			KAUICursorManager.SetDefaultCursor("Arrow");
			base.pParentUI.pState = WidgetState.INTERACTIVE;
			break;
		}
	}

	private void OnSlotPurchaseClosed(bool purchaseSuccess, int purchaseAmount)
	{
		UIUpsell uIUpsell = mUIUpsell;
		uIUpsell.OnClose = (Action<bool, int>)Delegate.Remove(uIUpsell.OnClose, new Action<bool, int>(OnSlotPurchaseClosed));
		base.pParentUI.pState = WidgetState.INTERACTIVE;
		if (!purchaseSuccess)
		{
			UtDebug.LogError("Slot purchase failed!");
			return;
		}
		mEmptySlotCount += purchaseAmount;
		if (mSlotsAvailable)
		{
			ClaimGifts();
		}
	}

	private void ClaimGifts()
	{
		base.pParentUI.pState = WidgetState.NOT_INTERACTIVE;
		KAUICursorManager.SetDefaultCursor("Loading");
		WsWebService.SetAchievementAndGetReward(mGiftData.AchievementID, "", ServiceEventHandler, false);
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			base.pParentUI.pState = WidgetState.INTERACTIVE;
			if (inObject != null)
			{
				AchievementReward[] array = (AchievementReward[])inObject;
				if (array != null)
				{
					GameUtilities.AddRewards(array, inUseRewardManager: false, inImmediateShow: false);
				}
			}
			if (GiftManager.pIsReady)
			{
				GiftManager.pInstance.UpdateMessageTag(mGiftData, "Claim");
			}
			_ClaimBtn.pVisible = false;
			_DeleteBtn.pVisible = true;
			OnClaim?.Invoke(mMessageInfo);
			OnClaim = null;
			{
				foreach (UIWidget pChildWidget in _GiftMenu.pChildWidgets)
				{
					pChildWidget.pState = WidgetState.DISABLED;
				}
				break;
			}
		case WsServiceEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			base.pParentUI.pState = WidgetState.INTERACTIVE;
			UtDebug.LogError("Unable to process Gift with Achievement ID :: " + mGiftData.AchievementID);
			OnClose();
			break;
		}
	}

	public void OnClose()
	{
		pVisible = false;
		_GiftMenu.ClearChildren();
	}
}
