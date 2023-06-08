using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class PaymentMethodsAdapter : IBaseAdapter
{
	private GameObject paymentMethodPrefab;

	private List<XsollaPaymentMethod> paymentList;

	private XsollaPaymentMethods manager;

	public ImageLoader imageLoader;

	public object getManager()
	{
		return manager;
	}

	public void Awake()
	{
		paymentMethodPrefab = Resources.Load("Prefabs/SimpleView/PaymentMethodPrefab") as GameObject;
		if (imageLoader == null)
		{
			imageLoader = base.gameObject.AddComponent<ImageLoader>();
		}
	}

	public override int GetCount()
	{
		return paymentList.Count;
	}

	public override int GetElementType(int position)
	{
		return 0;
	}

	public XsollaPaymentMethod GetItem(int position)
	{
		return paymentList[position];
	}

	public override GameObject GetView(int position)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(paymentMethodPrefab);
		XsollaPaymentMethod item = GetItem(position);
		imageLoader.LoadImage(gameObject.GetComponentsInChildren<Image>()[2], item.GetImageUrl());
		gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate
		{
			OnChoosePaymentMethod(item.id);
		});
		return gameObject;
	}

	public override GameObject GetNext()
	{
		throw new NotImplementedException();
	}

	public override GameObject GetPrefab()
	{
		return paymentMethodPrefab;
	}

	public void OnChoosePaymentMethod(long paymentMethodId)
	{
		GetComponentInParent<PaymentListScreenController>().ChoosePaymentMethod(paymentMethodId);
	}

	public void SetManager(XsollaPaymentMethods paymentMethods)
	{
		manager = paymentMethods;
		paymentList = paymentMethods.GetRecomendedItems();
	}

	public void UpdateElements(List<XsollaPaymentMethod> newPaymentList)
	{
		paymentList = newPaymentList;
	}
}
