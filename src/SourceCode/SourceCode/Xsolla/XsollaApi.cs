using SimpleJSON;

namespace Xsolla;

public class XsollaApi : IParseble
{
	private string version;

	public IParseble Parse(JSONNode apiNode)
	{
		version = apiNode["ver"];
		return this;
	}

	public string getVersion()
	{
		return version;
	}

	public override string ToString()
	{
		return $"[XsollaApi]";
	}
}
