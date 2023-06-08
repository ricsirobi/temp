using System;
using UnityEngine;

namespace Xsolla;

public class PricePointsAdapter : IBaseAdapter
{
	private GameObject shopItemPrefab;

	public Action<float> OnBuyPricepoints;

	private XsollaPricepointsManager manager;

	private string _virtualCurrencyName = "Coins";

	private string _buyBtnText = "Buy";

	private int current;

	private ImageLoader imageLoader;

	public void Awake()
	{
		shopItemPrefab = Resources.Load("Prefabs/SimpleView/_ScreenShop/_ShopItemPricePoint") as GameObject;
	}

	public override int GetElementType(int id)
	{
		return 0;
	}

	public override int GetCount()
	{
		return manager.GetCount();
	}

	public XsollaPricepoint GetItem(int position)
	{
		return manager.GetItemByPosition(position);
	}

	public XsollaPricepoint GetItemById(int position)
	{
		return null;
	}

	public override GameObject GetView(int position)
	{
		GameObject obj = UnityEngine.Object.Instantiate(shopItemPrefab);
		XsollaPricepoint pricepoint = GetItem(position);
		ShopItemViewAdapter component = obj.GetComponent<ShopItemViewAdapter>();
		component.SetImage(pricepoint.GetImageUrl());
		component.SetName(pricepoint.GetOutString());
		component.SetDesc(_virtualCurrencyName);
		component.SetBuyText(_buyBtnText);
		component.SetSpecial(pricepoint.GetDescription());
		component.SetPrice(pricepoint.GetPriceString());
		component.SetLabel(pricepoint.GetAdvertisementType(), pricepoint.GetLabel());
		component.SetOnClickListener(delegate
		{
			OnClickBuy(pricepoint.outAmount);
		});
		return obj;
	}

	private void OnClickBuy(float i)
	{
		if (OnBuyPricepoints != null)
		{
			OnBuyPricepoints(i);
		}
	}

	public override GameObject GetPrefab()
	{
		return shopItemPrefab;
	}

	public void SetManager(XsollaPricepointsManager pricepoints)
	{
		manager = pricepoints;
	}

	public void SetManager(XsollaPricepointsManager pricepoints, string virtualCurrencyName, string buyBtnText)
	{
		_virtualCurrencyName = virtualCurrencyName;
		_buyBtnText = buyBtnText;
		SetManager(pricepoints);
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
