using SimpleJSON;

namespace Xsolla;

public class RequestSubscriptions : BaseWWWRequest
{
	public RequestSubscriptions(int type)
		: base(type)
	{
	}

	protected override string GetMethod()
	{
		return "/paystation2/api/recurring";
	}

	protected override object[] ParseResult(JSONNode rootNode)
	{
		XsollaSubscriptions xsollaSubscriptions = new XsollaSubscriptions().Parse(rootNode) as XsollaSubscriptions;
		return new object[1] { xsollaSubscriptions };
	}
}
