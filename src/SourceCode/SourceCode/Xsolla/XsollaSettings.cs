using SimpleJSON;

namespace Xsolla;

public class XsollaSettings : IParseble
{
	public string version;

	public XsollaPaystation2 paystation2;

	public Components components;

	public string theme;

	public string GetTheme()
	{
		if (theme != null && !"null".Equals(theme) && !"theme".Equals(theme))
		{
			if ("default".Equals(theme) || "dark".Equals(theme))
			{
				return theme;
			}
			return "dark";
		}
		return "dark";
	}

	public IParseble Parse(JSONNode rootNode)
	{
		version = rootNode["version"];
		JSONNode paystation2Node = rootNode["paystation2"];
		paystation2 = new XsollaPaystation2().Parse(paystation2Node) as XsollaPaystation2;
		theme = rootNode["theme"];
		components = new Components().Parse(rootNode["components"]) as Components;
		return this;
	}

	public override string ToString()
	{
		return $"[XsollaSettings]";
	}
}
