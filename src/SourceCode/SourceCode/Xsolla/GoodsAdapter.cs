using System;
using UnityEngine;

namespace Xsolla;

public class GoodsAdapter : IBaseAdapter
{
	private GameObject shopItemPrefab;

	private XsollaGoodsManager manager;

	private string textValue = "Coins";

	private string _buyBtnText = "Buy";

	private int current;

	public Action<string, bool> OnBuy;

	public Action<bool, string, long> OnFavorite;

	public override string ToString()
	{
		return $"[GoodsAdapter: textValue={textValue}, _buyBtnText={_buyBtnText}]";
	}

	public void Start()
	{
		shopItemPrefab = Resources.Load("Prefabs/SimpleView/_ScreenShop/_ShopItemGood") as GameObject;
	}

	public override int GetElementType(int id)
	{
		return 0;
	}

	public override int GetCount()
	{
		return manager.GetCount();
	}

	public override GameObject GetView(int position)
	{
		GameObject obj = UnityEngine.Object.Instantiate(shopItemPrefab);
		obj.name = "ShopItemGood " + position;
		XsollaShopItem item = manager.GetItemByPosition(position);
		ShopItemViewAdapter component = obj.GetComponent<ShopItemViewAdapter>();
		component.SetPrice(item.GetPriceString());
		component.SetSpecial(item.GetBounusString());
		component.SetDesc(item.GetDescription());
		component.SetName(item.GetName());
		component.SetFullDesc(item.GetLongDescription());
		component.SetBuyText("Buy");
		component.SetImage(item.GetImageUrl());
		component.SetFavorite(item.IsFavorite());
		component.SetOnClickListener(delegate
		{
			OnClickBuy("sku[" + item.GetKey() + "]", item.IsVirtualPayment());
		});
		component.SetOnFavoriteChanged(delegate(bool b)
		{
			OnClickFavorite(b, "sku[" + item.GetKey() + "]", item.GetId());
		});
		component.SetLabel(item.GetAdvertisementType(), item.GetLabel());
		return obj;
	}

	private void OnClickFavorite(bool isFavorite, string sku, long virtualItemId)
	{
		if (OnFavorite != null)
		{
			OnFavorite(isFavorite, sku, virtualItemId);
		}
	}

	private void OnClickBuy(string sku)
	{
		if (OnBuy != null)
		{
			OnBuy(sku, arg2: false);
		}
	}

	private void OnClickBuy(string sku, bool isVirtualPayment)
	{
		if (OnBuy != null)
		{
			OnBuy(sku, isVirtualPayment);
		}
	}

	public override GameObject GetPrefab()
	{
		return shopItemPrefab;
	}

	public void SetManager(XsollaGoodsManager pricepoints, string buyBtnText)
	{
		_buyBtnText = buyBtnText;
		manager = pricepoints;
	}

	public override GameObject GetNext()
	{
		if (current < manager.GetCount())
		{
			GameObject view = GetView(current);
			current++;
			return view;
		}
		return null;
	}
}
