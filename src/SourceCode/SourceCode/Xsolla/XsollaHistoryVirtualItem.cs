using SimpleJSON;

namespace Xsolla;

public class XsollaHistoryVirtualItem : IXsollaObject, IParseble
{
	public int id { get; private set; }

	public string name { get; private set; }

	public IParseble Parse(JSONNode pNode)
	{
		id = pNode["id"].AsInt;
		name = pNode["name"];
		return this;
	}

	public string GetKey()
	{
		return id.ToString();
	}

	public string GetName()
	{
		return name;
	}
}
