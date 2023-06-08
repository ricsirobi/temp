using SimpleJSON;

namespace Xsolla;

public class Components : IParseble
{
	public VirtualCurrency virtualCurreny;

	public IParseble Parse(JSONNode pRootNode)
	{
		virtualCurreny = new VirtualCurrency().Parse(pRootNode["virtual_currency"]) as VirtualCurrency;
		return this;
	}
}
