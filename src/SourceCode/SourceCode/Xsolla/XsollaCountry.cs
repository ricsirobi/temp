using SimpleJSON;

namespace Xsolla;

public class XsollaCountry : IXsollaObject, IParseble
{
	public string iso { get; private set; }

	public string aliases { get; private set; }

	public string name { get; private set; }

	public string GetKey()
	{
		return iso;
	}

	public string GetName()
	{
		return name;
	}

	public IParseble Parse(JSONNode countryNode)
	{
		iso = countryNode["ISO"];
		aliases = countryNode["aliases"];
		name = countryNode["name"];
		return this;
	}
}
