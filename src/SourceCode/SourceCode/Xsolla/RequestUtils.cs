using SimpleJSON;

namespace Xsolla;

public class RequestUtils : BaseWWWRequest
{
	public RequestUtils(int type)
		: base(type)
	{
	}

	protected override string GetMethod()
	{
		return "/paystation2/api/utils";
	}

	protected override object[] ParseResult(JSONNode rootNode)
	{
		XsollaUtils xsollaUtils = new XsollaUtils().Parse(rootNode) as XsollaUtils;
		return new object[1] { xsollaUtils };
	}
}
