using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaGoodsManager : XsollaObjectsManager<XsollaShopItem>, IParseble
{
	public IParseble Parse(JSONNode goodsNode)
	{
		IEnumerator<JSONNode> enumerator = goodsNode["virtual_items"].Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			XsollaShopItem item = new XsollaShopItem().Parse(enumerator.Current) as XsollaShopItem;
			AddItem(item);
		}
		return this;
	}

	public void setItemVirtCurrName(string pName)
	{
		foreach (XsollaShopItem items in GetItemsList())
		{
			items.SetVirtCurrName(pName);
		}
	}
}
