using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaSummary : IParseble
{
	private List<IXsollaSummaryItem> purchases;

	private XsollaFinance finance;

	public List<IXsollaSummaryItem> GetPurchases()
	{
		return purchases;
	}

	public XsollaFinance GetFinance()
	{
		return finance;
	}

	public IParseble Parse(JSONNode summaryNode)
	{
		purchases = new List<IXsollaSummaryItem>();
		JSONNode jSONNode = summaryNode["purchase"]["virtual_items"];
		JSONNode jSONNode2 = summaryNode["purchase"]["virtual_currency"];
		JSONNode jSONNode3 = summaryNode["purchase"]["subscriptions"];
		if (jSONNode != null)
		{
			IEnumerator<JSONNode> enumerator = jSONNode.Childs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				JSONNode current = enumerator.Current;
				purchases.Add(new XsollaSummaryItem().Parse(current) as IXsollaSummaryItem);
			}
		}
		if (jSONNode2 != null)
		{
			IEnumerator<JSONNode> enumerator2 = jSONNode2.Childs.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				JSONNode current2 = enumerator2.Current;
				purchases.Add(new XsollaSummaryItem().Parse(current2) as IXsollaSummaryItem);
			}
		}
		if (jSONNode3 != null)
		{
			IEnumerator<JSONNode> enumerator3 = jSONNode3.Childs.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				JSONNode current3 = enumerator3.Current;
				purchases.Add(new XsollaSummarySubscription().Parse(current3) as IXsollaSummaryItem);
			}
		}
		finance = new XsollaFinance().Parse(summaryNode["finance"]) as XsollaFinance;
		return this;
	}
}
