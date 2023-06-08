using SimpleJSON;

namespace Xsolla;

public class XsollaSubLimitHoldPeriod : IParseble
{
	public int maxDays;

	public int minDays;

	public IParseble Parse(JSONNode rootNode)
	{
		maxDays = rootNode["max_days"].AsInt;
		minDays = rootNode["min_days"].AsInt;
		return this;
	}
}
