using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaManagerSubscriptions : XsollaObjectsManager<XsollaManagerSubscription>, IParseble
{
	private XsollaApi api;

	public IParseble Parse(JSONNode rootNode)
	{
		api = new XsollaApi().Parse(rootNode["api"]) as XsollaApi;
		IEnumerator<JSONNode> enumerator = rootNode.Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AddItem(new XsollaManagerSubscription().Parse(enumerator.Current) as XsollaManagerSubscription);
		}
		return this;
	}

	public override string ToString()
	{
		return $"[XsollaManagerSubscriptions: api={api}]";
	}
}
