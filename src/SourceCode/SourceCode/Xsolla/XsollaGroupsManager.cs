using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaGroupsManager : XsollaObjectsManager<XsollaGoodsGroup>, IParseble
{
	public IParseble Parse(JSONNode groupsNode)
	{
		IEnumerator<JSONNode> enumerator = groupsNode["groups"].Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AddItem(new XsollaGoodsGroup().Parse(enumerator.Current) as XsollaGoodsGroup);
		}
		return this;
	}
}
