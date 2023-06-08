using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaBonusItem : IParseble
{
	public string name { get; private set; }

	public string quantity { get; private set; }

	public static List<XsollaBonusItem> ParseMany(JSONNode bonusItemsNode)
	{
		List<XsollaBonusItem> list = new List<XsollaBonusItem>(bonusItemsNode.Count);
		IEnumerator<JSONNode> enumerator = bonusItemsNode.Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			list.Add(new XsollaBonusItem().Parse(enumerator.Current) as XsollaBonusItem);
		}
		return list;
	}

	public IParseble Parse(JSONNode bonusItemsNode)
	{
		name = bonusItemsNode["name"];
		quantity = bonusItemsNode["quantity"];
		return this;
	}
}
