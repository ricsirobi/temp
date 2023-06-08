using System.Collections.Generic;
using UnityEngine;

namespace Xsolla;

public class PaymentListView : MonoBehaviour
{
	private XsollaPaymentMethods _paymentMethods;

	private PaymentMethodsAdapter adapter;

	private GridView gridView;

	public void SetPaymentMethods(XsollaPaymentMethods paymentMethods)
	{
		_paymentMethods = paymentMethods;
		if (adapter == null)
		{
			adapter = GetComponent<PaymentMethodsAdapter>();
		}
		adapter.SetManager(_paymentMethods);
		gridView = GetComponent<GridView>();
		gridView.SetAdapter(adapter, 6);
	}

	public void Sort(string s)
	{
		List<XsollaPaymentMethod> sortedItems = _paymentMethods.GetSortedItems(s);
		if (sortedItems.Count > 0)
		{
			adapter.UpdateElements(sortedItems);
			gridView.SetAdapter(adapter, 6);
		}
	}

	private void Update()
	{
	}
}
