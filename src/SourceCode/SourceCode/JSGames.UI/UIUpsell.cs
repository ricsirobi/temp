using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JSGames.UI;

public class UIUpsell : UI
{
	public LocaleString pUpsellText;

	public int pUpsellItemID;

	public int pUpsellItemStoreID;

	public InputField _TxtQuantityInput;

	public Action<bool, int> OnClose;

	public LocaleString _CurrentAmtText = new LocaleString("{0} needed: {1}");

	public int pUpsellCurrentItemAmount;

	[SerializeField]
	private UIWidget m_TxtCurrentAmt;

	private ItemData mItemData;

	[SerializeField]
	private UIWidget m_TxtDialog;

	[SerializeField]
	private UIWidget m_TotalPrice;

	[SerializeField]
	private UIWidget m_BtnNegative;

	[SerializeField]
	private UIWidget m_BtnPositive;

	[SerializeField]
	private UIWidget m_BtnBuy;

	[SerializeField]
	private UIWidget m_BtnClose;

	public int pUpsellPurchaseCount { get; set; }

	public void Initialize()
	{
		if (!m_TxtDialog || !m_TotalPrice || !_TxtQuantityInput)
		{
			OnClose?.Invoke(arg1: false, 0);
			return;
		}
		SetExclusive();
		_TxtQuantityInput.text = pUpsellPurchaseCount.ToString();
		_TxtQuantityInput.onValueChanged.AddListener(OnValueChanged);
		m_TxtDialog.pText = pUpsellText.GetLocalizedString();
		ItemData.Load(pUpsellItemID, OnUpsellItemLoad, null);
	}

	private void OnUpsellItemLoad(int itemID, ItemData dataItem, object inUserData)
	{
		mItemData = dataItem;
		bool flag = mItemData.Cost > 0;
		FindWidget("TotalPriceCoins").pVisible = flag;
		FindWidget("TotalPriceGems").pVisible = !flag;
		m_TotalPrice.pText = GetTotalUpsellPrice().ToString();
		m_TxtCurrentAmt.pText = string.Format(_CurrentAmtText.GetLocalizedString(), (!string.IsNullOrEmpty(mItemData.ItemNamePlural)) ? mItemData.ItemNamePlural : mItemData.ItemName, pUpsellPurchaseCount);
	}

	protected void OnValueChanged(string text)
	{
		int result = 0;
		int.TryParse(_TxtQuantityInput.text, out result);
		if (result < 1)
		{
			result = 1;
		}
		pUpsellPurchaseCount = result;
		_TxtQuantityInput.text = result.ToString();
		m_TotalPrice.pText = GetTotalUpsellPrice().ToString();
	}

	public int GetTotalUpsellPrice()
	{
		return pUpsellPurchaseCount * mItemData.FinalCashCost;
	}

	protected override void OnClick(UIWidget inWidget, PointerEventData eventData)
	{
		base.OnClick(inWidget, eventData);
		if (inWidget == m_BtnBuy)
		{
			OnPurchaseUpsell();
		}
		else if (inWidget == m_BtnNegative)
		{
			if (pUpsellPurchaseCount != 1)
			{
				pUpsellPurchaseCount--;
				_TxtQuantityInput.text = pUpsellPurchaseCount.ToString();
				m_TotalPrice.pText = GetTotalUpsellPrice().ToString();
			}
		}
		else if (inWidget == m_BtnPositive)
		{
			pUpsellPurchaseCount++;
			_TxtQuantityInput.text = pUpsellPurchaseCount.ToString();
			m_TotalPrice.pText = GetTotalUpsellPrice().ToString();
		}
		else if (inWidget == m_BtnClose)
		{
			OnCancelUpsellPrompt();
		}
	}

	private void OnPurchaseUpsell()
	{
		pState = WidgetState.NOT_INTERACTIVE;
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		CommonInventoryData.pInstance.AddPurchaseItem(pUpsellItemID, pUpsellPurchaseCount);
		CommonInventoryData.pInstance.DoPurchase(mItemData.GetPurchaseType(), pUpsellItemStoreID, OnUpsellPurchaseComplete);
	}

	private void OnUpsellPurchaseComplete(CommonInventoryResponse ret)
	{
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		if (!ret.Success)
		{
			UtDebug.LogError("Upsell purchase failed!");
			OnCancelUpsellPrompt();
			return;
		}
		RemoveExclusive();
		_TxtQuantityInput.onValueChanged.RemoveListener(OnValueChanged);
		OnClose?.Invoke(arg1: true, pUpsellPurchaseCount);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnCancelUpsellPrompt()
	{
		RemoveExclusive();
		_TxtQuantityInput.onValueChanged.RemoveListener(OnValueChanged);
		OnClose?.Invoke(arg1: false, 0);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
