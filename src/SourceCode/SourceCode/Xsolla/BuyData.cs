using SimpleJSON;

namespace Xsolla;

public class BuyData : IParseble
{
	public string currency { get; private set; }

	public string sum { get; private set; }

	public bool enabled { get; private set; }

	public string example { get; private set; }

	public string isMandatory { get; private set; }

	public string isPakets { get; private set; }

	public string isReadonly { get; private set; }

	public string isVisible { get; private set; }

	public string name { get; private set; }

	public string options { get; private set; }

	public string title { get; private set; }

	public string tooltip { get; private set; }

	public string type { get; private set; }

	public string value { get; private set; }

	public IParseble Parse(JSONNode buyDataNode)
	{
		if (buyDataNode["currency"] != null)
		{
			currency = buyDataNode["currency"];
		}
		if (buyDataNode["sum"] != null)
		{
			sum = buyDataNode["sum"];
		}
		if (buyDataNode["enabled"] != null)
		{
			enabled = buyDataNode["enabled"].AsBool;
		}
		if (buyDataNode["example"] != null)
		{
			example = buyDataNode["example"];
		}
		if (buyDataNode["isMandatory"] != null)
		{
			isMandatory = buyDataNode["isMandatory"];
		}
		if (buyDataNode["isPakets"] != null)
		{
			isPakets = buyDataNode["isPakets"];
		}
		if (buyDataNode["isReadonly"] != null)
		{
			isReadonly = buyDataNode["isReadonly"];
		}
		if (buyDataNode["isVisible"] != null)
		{
			isVisible = buyDataNode["isVisible"];
		}
		if (buyDataNode["name"] != null)
		{
			name = buyDataNode["name"];
		}
		if (buyDataNode["options"] != null)
		{
			options = buyDataNode["options"];
		}
		if (buyDataNode["title"] != null)
		{
			title = buyDataNode["title"];
		}
		if (buyDataNode["tooltip"] != null)
		{
			tooltip = buyDataNode["tooltip"];
		}
		if (buyDataNode["type"] != null)
		{
			type = buyDataNode["type"];
		}
		if (buyDataNode["value"] != null)
		{
			value = buyDataNode["value"];
		}
		return this;
	}
}
