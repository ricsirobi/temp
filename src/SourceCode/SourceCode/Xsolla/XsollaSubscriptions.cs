using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaSubscriptions : XsollaObjectsManager<XsollaSubscription>, IParseble
{
	private XsollaApi api;

	private XsollaActivePackage activeUserPackage;

	public IParseble Parse(JSONNode subscriptionsNode)
	{
		IEnumerator<JSONNode> enumerator = subscriptionsNode["packages"].Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AddItem(new XsollaSubscription().Parse(enumerator.Current) as XsollaSubscription);
		}
		if (subscriptionsNode["active_user_package"].AsObject != null)
		{
			activeUserPackage = new XsollaActivePackage().Parse(subscriptionsNode["active_user_package"]) as XsollaActivePackage;
		}
		api = new XsollaApi().Parse(subscriptionsNode["api"]) as XsollaApi;
		return this;
	}

	public XsollaActivePackage GetActivePackage()
	{
		return activeUserPackage;
	}

	public override string ToString()
	{
		return $"[XsollaSubscriptions: api={api}, activeUserPackage={activeUserPackage}]";
	}
}
