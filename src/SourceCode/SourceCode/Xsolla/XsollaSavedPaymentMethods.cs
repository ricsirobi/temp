using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaSavedPaymentMethods : XsollaObjectsManager<XsollaSavedPaymentMethod>, IParseble
{
	private XsollaApi api;

	public object getApi()
	{
		return api;
	}

	public List<XsollaSavedPaymentMethod> GetSortedItems(string s)
	{
		return itemsList.FindAll((XsollaSavedPaymentMethod xpm) => xpm.GetName().ToLower().StartsWith(s.ToLower()));
	}

	public List<XsollaSavedPaymentMethod> GetItemList()
	{
		return itemsList;
	}

	public IParseble Parse(JSONNode paymentListNode)
	{
		IEnumerator<JSONNode> enumerator = paymentListNode["list"].Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			XsollaSavedPaymentMethod item = new XsollaSavedPaymentMethod().Parse(enumerator.Current) as XsollaSavedPaymentMethod;
			AddItem(item);
		}
		api = new XsollaApi().Parse(paymentListNode["api"]) as XsollaApi;
		return this;
	}

	public override bool Equals(object obj)
	{
		if (base.Count != (obj as XsollaSavedPaymentMethods).Count)
		{
			return false;
		}
		foreach (XsollaSavedPaymentMethod items in GetItemsList())
		{
			bool flag = false;
			foreach (XsollaSavedPaymentMethod item in (obj as XsollaSavedPaymentMethods).GetItemList())
			{
				if (items.Equals(item))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
