using SimpleJSON;

namespace Xsolla;

public class XsollaGoodsGroup : IXsollaObject, IParseble
{
	public long id { get; private set; }

	public string name { get; private set; }

	public string GetKey()
	{
		return id.ToString();
	}

	public string GetName()
	{
		return name;
	}

	public IParseble Parse(JSONNode goodsGroupNode)
	{
		id = goodsGroupNode["id"].AsInt;
		name = goodsGroupNode["name"];
		return this;
	}
}
