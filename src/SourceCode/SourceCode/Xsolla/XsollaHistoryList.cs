using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaHistoryList : XsollaObjectsManager<XsollaHistoryItem>, IParseble
{
	public IParseble Parse(JSONNode pNode)
	{
		IEnumerator<JSONNode> enumerator = pNode.Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AddItem(new XsollaHistoryItem().Parse(enumerator.Current) as XsollaHistoryItem);
		}
		return this;
	}
}
