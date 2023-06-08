using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaHistoryVirtualItemsList : XsollaObjectsManager<XsollaHistoryVirtualItem>, IParseble
{
	public IParseble Parse(JSONNode pNode)
	{
		IEnumerator<JSONNode> enumerator = pNode.Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AddItem(new XsollaHistoryVirtualItem().Parse(enumerator.Current) as XsollaHistoryVirtualItem);
		}
		return this;
	}
}
