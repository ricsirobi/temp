using System;
using SimpleJSON;

namespace Xsolla;

public class XsollaSubNextPeriodPlanChange : IParseble
{
	public string name;

	public DateTime date;

	public IParseble Parse(JSONNode rootNode)
	{
		if (rootNode.Value == "null")
		{
			return null;
		}
		if (rootNode["name"] != null)
		{
			name = rootNode["name"].Value;
		}
		if (rootNode["date"] != null)
		{
			date = DateTime.Parse(rootNode["date"].Value);
		}
		return this;
	}
}
