using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaPaymentMethods : XsollaObjectsManager<XsollaPaymentMethod>, IParseble
{
	private object lastPayment;

	private XsollaApi api;

	public object getApi()
	{
		return api;
	}

	public object GetLastPayment()
	{
		return lastPayment;
	}

	public List<XsollaPaymentMethod> GetListOnType(XsollaPaymentMethod.TypePayment pType)
	{
		return itemsList.FindAll((XsollaPaymentMethod xpm) => xpm.typePayment == pType);
	}

	public List<XsollaPaymentMethod> GetListOnType()
	{
		return itemsList;
	}

	public List<XsollaPaymentMethod> GetSortedItems(string s)
	{
		return itemsList.FindAll((XsollaPaymentMethod xpm) => xpm.name.ToLower().StartsWith(s.ToLower()));
	}

	public List<XsollaPaymentMethod> GetRecomendedItems()
	{
		return GetItemsList().FindAll((XsollaPaymentMethod xpm) => xpm.isHidden == 0 && xpm.isRecommended == 1);
	}

	public IParseble Parse(JSONNode paymentListNode)
	{
		IEnumerator<JSONNode> enumerator = paymentListNode["quick_instances"].Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			XsollaPaymentMethod xsollaPaymentMethod = new XsollaPaymentMethod().Parse(enumerator.Current) as XsollaPaymentMethod;
			if (GetCount() <= 2)
			{
				xsollaPaymentMethod.SetType(XsollaPaymentMethod.TypePayment.QUICK);
			}
			else
			{
				xsollaPaymentMethod.SetType(XsollaPaymentMethod.TypePayment.REGULAR);
			}
			if (xsollaPaymentMethod.id != 64 && xsollaPaymentMethod.id != 1738)
			{
				AddItem(xsollaPaymentMethod);
			}
		}
		enumerator = paymentListNode["regular_instances"].Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			XsollaPaymentMethod xsollaPaymentMethod2 = new XsollaPaymentMethod().Parse(enumerator.Current) as XsollaPaymentMethod;
			xsollaPaymentMethod2.SetType(XsollaPaymentMethod.TypePayment.REGULAR);
			if (xsollaPaymentMethod2.id != 64 && xsollaPaymentMethod2.id != 1738)
			{
				AddItem(xsollaPaymentMethod2);
			}
		}
		lastPayment = null;
		api = new XsollaApi().Parse(paymentListNode["api"]) as XsollaApi;
		return this;
	}
}
