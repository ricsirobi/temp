using System;
using SimpleJSON;

namespace Xsolla;

public class XsollaSubHoldDates : IParseble
{
	public DateTime dateFrom;

	public DateTime dateTo;

	public IParseble Parse(JSONNode rootNode)
	{
		if (rootNode.Value == "null")
		{
			return null;
		}
		if (rootNode["date_from"] != null)
		{
			dateFrom = DateTime.Parse(rootNode["date_from"].Value);
		}
		if (rootNode["date_to"] != null)
		{
			dateTo = DateTime.Parse(rootNode["date_to"].Value);
		}
		return this;
	}

	public override string ToString()
	{
		return $"{$"{dateFrom:dd.MM.yyyy}"} - {$"{dateTo:dd.MM.yyyy}"}";
	}
}
