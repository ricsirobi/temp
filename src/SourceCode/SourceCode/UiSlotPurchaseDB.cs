using UnityEngine;

public class UiSlotPurchaseDB : UiItemTradeGenericDB
{
	public LocaleString _CurrentStatusText = new LocaleString("[REVIEW] Current");

	public Color _OverloadColor = Color.red;

	private KAWidget mCurrentStatus;

	public int pOccupiedCount { get; set; }

	public int pTotalCount { get; set; }

	public static void ShowSlotPurchaseDB(string displayText, int itemID, int storeID, int occupiedCount, int totalCount, GameObject messageObject)
	{
		UiSlotPurchaseDB uiSlotPurchaseDB = (UiSlotPurchaseDB)GameUtilities.CreateKAUIGenericDB("PfUiSlotPurchaseDBDO", "SlotPurchaseDB");
		if (uiSlotPurchaseDB != null)
		{
			if (!string.IsNullOrEmpty(displayText))
			{
				uiSlotPurchaseDB.SetText(displayText, interactive: false);
			}
			uiSlotPurchaseDB.pOccupiedCount = occupiedCount;
			uiSlotPurchaseDB.pTotalCount = totalCount;
			uiSlotPurchaseDB._MessageObject = messageObject;
			uiSlotPurchaseDB.SetMode(TradeType.Buy, itemID, storeID, ItemPurchaseSource.SLOT.ToString());
		}
	}

	public static void ShowSlotPurchaseDB(string displayText, int itemID, int storeID, int occupiedCount, int totalCount, int quantity, GameObject messageObject)
	{
		UiSlotPurchaseDB uiSlotPurchaseDB = (UiSlotPurchaseDB)GameUtilities.CreateKAUIGenericDB("PfUiSlotPurchaseDBDO", "SlotPurchaseDB");
		if (uiSlotPurchaseDB != null)
		{
			if (!string.IsNullOrEmpty(displayText))
			{
				uiSlotPurchaseDB.SetText(displayText, interactive: false);
			}
			uiSlotPurchaseDB.pOccupiedCount = occupiedCount;
			uiSlotPurchaseDB.pTotalCount = totalCount;
			uiSlotPurchaseDB._MessageObject = messageObject;
			uiSlotPurchaseDB.SetMode(TradeType.Buy, itemID, storeID, ItemPurchaseSource.SLOT.ToString());
			uiSlotPurchaseDB.SetFixedQuantity(quantity);
		}
	}

	protected override void UpdateUI()
	{
		base.UpdateUI();
		string text = ((pOccupiedCount > pTotalCount) ? ("[c]" + UtUtilities.GetKAUIColorString(_OverloadColor) + pOccupiedCount + "[-]") : pOccupiedCount.ToString()) + "/" + pTotalCount;
		mCurrentStatus = FindItem("TxtCurrentStatus");
		if (mCurrentStatus != null)
		{
			mCurrentStatus.SetText(_CurrentStatusText.GetLocalizedString() + " " + mItemData.ItemName + " " + text);
		}
	}
}
