using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaQuickPayments : XsollaObjectsManager<XsollaPaymentMethod>, IParseble
{
	private XsollaApi api;

	public object getApi()
	{
		return api;
	}

	public IParseble Parse(JSONNode quickPaymentsNode)
	{
		IEnumerator<JSONNode> enumerator = quickPaymentsNode["quick_instances"].Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			XsollaPaymentMethod xsollaPaymentMethod = new XsollaPaymentMethod().Parse(enumerator.Current) as XsollaPaymentMethod;
			if (xsollaPaymentMethod.id != 64 && xsollaPaymentMethod.id != 1738)
			{
				AddItem(xsollaPaymentMethod);
			}
		}
		api = new XsollaApi().Parse(quickPaymentsNode["api"]) as XsollaApi;
		return this;
	}
}
