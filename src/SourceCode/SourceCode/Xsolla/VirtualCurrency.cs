using SimpleJSON;

namespace Xsolla;

public class VirtualCurrency : IParseble
{
	public bool customAmount;

	public IParseble Parse(JSONNode pRootNode)
	{
		customAmount = pRootNode["custom_amount"].AsBool;
		return this;
	}
}
