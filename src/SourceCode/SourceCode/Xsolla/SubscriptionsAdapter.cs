using System;
using UnityEngine;

namespace Xsolla;

public class SubscriptionsAdapter : IBaseAdapter
{
	public Action<string> OnBuySubscription;

	private GameObject subscriptionPrefab;

	private GameObject subscriptionSpecialPrefab;

	private XsollaSubscriptions manager;

	public void Awake()
	{
		subscriptionPrefab = Resources.Load("Prefabs/SimpleView/_ScreenShop/ShopItemSubscription") as GameObject;
		subscriptionSpecialPrefab = Resources.Load("Prefabs/SimpleView/_ScreenShop/ShopItemSubscriptionSpecial") as GameObject;
	}

	public override int GetElementType(int id)
	{
		return 0;
	}

	public override int GetCount()
	{
		return manager.GetCount();
	}

	public XsollaSubscription GetItem(int position)
	{
		return manager.GetItemByPosition(position);
	}

	public XsollaSubscription GetItemById(int position)
	{
		return null;
	}

	public override GameObject GetView(int position)
	{
		XsollaSubscription subscription = GetItem(position);
		GameObject gameObject = ((!subscription.IsSpecial()) ? UnityEngine.Object.Instantiate(subscriptionPrefab) : UnityEngine.Object.Instantiate(subscriptionSpecialPrefab));
		ShopItemViewAdapter componentInChildren = gameObject.GetComponentInChildren<ShopItemViewAdapter>();
		componentInChildren.SetPrice(subscription.name);
		componentInChildren.SetSpecial(subscription.description);
		componentInChildren.SetDesc(subscription.GetPriceString());
		componentInChildren.SetName(subscription.GetPeriodString("per"));
		componentInChildren.SetOnClickListener(delegate
		{
			OnClickBuy(subscription.id);
		});
		return gameObject;
	}

	private void OnClickBuy(string subscriptionId)
	{
		if (OnBuySubscription != null)
		{
			OnBuySubscription(subscriptionId);
		}
	}

	public override GameObject GetPrefab()
	{
		return subscriptionPrefab;
	}

	public void SetManager(XsollaSubscriptions pricepoints)
	{
		manager = pricepoints;
	}

	public override GameObject GetNext()
	{
		return null;
	}
}
