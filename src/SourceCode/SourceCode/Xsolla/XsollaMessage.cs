using SimpleJSON;

namespace Xsolla;

public class XsollaMessage : IParseble
{
	public string code { get; private set; }

	public string message { get; private set; }

	public IParseble Parse(JSONNode messageNode)
	{
		code = messageNode["code"].Value;
		message = messageNode["message"].Value;
		return this;
	}
}
