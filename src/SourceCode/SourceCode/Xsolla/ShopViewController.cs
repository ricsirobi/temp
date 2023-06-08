using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class ShopViewController : MonoBehaviour
{
	public ScrollableListCustom menu;

	public RadioGroupController radioGroup;

	public Text title;

	public GridView content;

	public PricePointsAdapter pAdapter;

	public SubscriptionsAdapter sAdapter;

	public GoodsAdapter gAdapter;

	public GameObject CustomAmountLink;

	public GameObject CustomAmountScreen;

	public GameObject ShopPanel;

	public Action DestroyAfter;

	public void OpenPricepoints(string title, XsollaPricepointsManager pricepoints, string virtualCurrencyName, string buyBtnText, bool pCustomHref = false, XsollaUtils pUtils = null)
	{
		Resizer.ResizeToParrent(base.gameObject);
		menu.transform.parent.parent.gameObject.SetActive(value: false);
		SetTitle(title);
		if (pCustomHref)
		{
			CustomAmountLink.SetActive(value: true);
			string customAmountShowTitle = pUtils.GetTranslations().Get(XsollaTranslations.PRICEPOINT_PAGE_CUSTOM_AMOUNT_SHOW_TITLE);
			string customAmountHideTitle = pUtils.GetTranslations().Get(XsollaTranslations.PRICEPOINT_PAGE_CUSTOM_AMOUNT_HIDE_TITLE);
			Text titleCustomAmount = CustomAmountLink.GetComponent<Text>();
			titleCustomAmount.text = customAmountShowTitle;
			CustomAmountLink.GetComponent<Toggle>().onValueChanged.AddListener(delegate(bool value)
			{
				if (value)
				{
					titleCustomAmount.text = customAmountHideTitle;
				}
				else
				{
					titleCustomAmount.text = customAmountShowTitle;
				}
				CustomAmountScreen.SetActive(value);
				ShopPanel.SetActive(!value);
				Logger.Log("Change value toggle " + value);
			});
			CustomAmountScreen.GetComponent<CustomVirtCurrAmountController>().initScreen(pUtils, pricepoints.GetItemByPosition(1).currency, CalcCustomAmount, TryPayCustomAmount);
		}
		else
		{
			CustomAmountLink.SetActive(value: false);
		}
		pAdapter.SetManager(pricepoints, virtualCurrencyName, buyBtnText);
		if (pAdapter.OnBuyPricepoints == null)
		{
			PricePointsAdapter pricePointsAdapter = pAdapter;
			pricePointsAdapter.OnBuyPricepoints = (Action<float>)Delegate.Combine(pricePointsAdapter.OnBuyPricepoints, (Action<float>)delegate(float outAmount)
			{
				OpenPaymentMethods(new Dictionary<string, object>(1) { { "out", outAmount } }, isVirtualPayment: false);
			});
		}
		DrawContent(pAdapter, 3);
	}

	public void TryPayCustomAmount(float pOutAmount)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>(1);
		dictionary.Add("out", pOutAmount);
		OpenPaymentMethods(dictionary, isVirtualPayment: false);
	}

	public void CalcCustomAmount(Dictionary<string, object> pParam)
	{
		base.gameObject.GetComponentInParent<XsollaPaystationController>().CalcCustomAmount(pParam);
	}

	public void OpenSubscriptions(string title, XsollaSubscriptions subscriptions)
	{
		Resizer.ResizeToParrent(base.gameObject);
		menu.transform.parent.parent.gameObject.SetActive(value: false);
		SetTitle(title);
		sAdapter.SetManager(subscriptions);
		if (sAdapter.OnBuySubscription == null)
		{
			SubscriptionsAdapter subscriptionsAdapter = sAdapter;
			subscriptionsAdapter.OnBuySubscription = (Action<string>)Delegate.Combine(subscriptionsAdapter.OnBuySubscription, (Action<string>)delegate(string subscriptionId)
			{
				OpenPaymentMethods(new Dictionary<string, object>(1) { { "id_package", subscriptionId } }, isVirtualPayment: false);
			});
		}
		DrawContent(sAdapter, 1);
	}

	public void OpenGoods(XsollaGroupsManager groups)
	{
		Resizer.ResizeToParrent(base.gameObject);
		menu.transform.parent.parent.gameObject.SetActive(value: true);
		SetTitle(groups.GetItemByPosition(0).name);
		menu.SetData(delegate(string groupId)
		{
			XsollaGoodsGroup itemByKey = groups.GetItemByKey(groupId);
			radioGroup.SelectItem(groups.GetItemsList().IndexOf(itemByKey));
			ChooseItemsGroup(itemByKey.id, itemByKey.name);
		}, groups.GetNamesDict());
		radioGroup.SetButtons(menu.GetItems());
		radioGroup.SelectItem(0);
	}

	public void UpdateGoods(XsollaGoodsManager goods, string buyBtnText)
	{
		gAdapter.SetManager(goods, buyBtnText);
		if (gAdapter.OnBuy == null)
		{
			GoodsAdapter goodsAdapter = gAdapter;
			goodsAdapter.OnBuy = (Action<string, bool>)Delegate.Combine(goodsAdapter.OnBuy, (Action<string, bool>)delegate(string sku, bool isVirtualPayment)
			{
				OpenPaymentMethods(new Dictionary<string, object>(1) { { sku, 1 } }, isVirtualPayment);
			});
		}
		if (gAdapter.OnFavorite == null)
		{
			GoodsAdapter goodsAdapter2 = gAdapter;
			goodsAdapter2.OnFavorite = (Action<bool, string, long>)Delegate.Combine(goodsAdapter2.OnFavorite, (Action<bool, string, long>)delegate(bool isFavorite, string sku, long id)
			{
				SetFavorite(new Dictionary<string, object>(1)
				{
					{
						"is_favorite",
						isFavorite ? 1 : 0
					},
					{ sku, 1 },
					{ "virtual_item_id", id }
				});
			});
		}
		DrawContent(gAdapter, 3);
	}

	public void SetTitle(string s)
	{
		if (title.gameObject != null)
		{
			title.text = s;
		}
	}

	private void DrawContent(IBaseAdapter adapter, int columnsCount)
	{
		content.SetAdapter(adapter, columnsCount);
	}

	private void ChooseItemsGroup(long groupId, string groupName)
	{
		SetTitle(groupName);
		base.gameObject.GetComponentInParent<XsollaPaystationController>().LoadGoods(groupId);
	}

	private void OpenVirtualPayment(Dictionary<string, object> purchase)
	{
		purchase.Add("is_virtual_payment", 1);
		base.gameObject.GetComponentInParent<XsollaPaystationController>().ChooseItem(purchase);
	}

	private void OpenPaymentMethods(Dictionary<string, object> purchase, bool isVirtualPayment)
	{
		base.gameObject.GetComponentInParent<XsollaPaystationController>().ChooseItem(purchase, isVirtualPayment);
	}

	private void SetFavorite(Dictionary<string, object> purchase)
	{
		base.gameObject.GetComponentInParent<XsollaPaystationController>().SetFavorite(purchase);
	}

	private void OnDestroy()
	{
		Logger.Log("ShopController was destroyed");
		if (DestroyAfter != null)
		{
			DestroyAfter();
		}
	}
}
