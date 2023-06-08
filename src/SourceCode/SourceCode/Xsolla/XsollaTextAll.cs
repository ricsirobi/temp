using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaTextAll : IParseble
{
	private List<XsollaError> errors;

	private List<XsollaInfo> info;

	private bool isFatal;

	public IParseble Parse(JSONNode textAllNode)
	{
		IEnumerator<JSONNode> enumerator = textAllNode["error"].Childs.GetEnumerator();
		errors = new List<XsollaError>();
		while (enumerator.MoveNext())
		{
			XsollaError xsollaError = new XsollaError();
			xsollaError.Parse(enumerator.Current);
			errors.Add(xsollaError);
		}
		IEnumerator<JSONNode> enumerator2 = textAllNode["info"].Childs.GetEnumerator();
		info = new List<XsollaInfo>();
		while (enumerator2.MoveNext())
		{
			XsollaInfo xsollaInfo = new XsollaInfo();
			xsollaInfo.Parse(enumerator2.Current);
			info.Add(xsollaInfo);
		}
		return this;
	}
}
