using SimpleJSON;

namespace Xsolla;

public class XsollaSubPeriod : IParseble
{
	private int mValue;

	private string mUnit;

	public IParseble Parse(JSONNode rootNode)
	{
		mValue = rootNode["value"].AsInt;
		mUnit = rootNode["unit"].Value;
		return this;
	}

	public override string ToString()
	{
		return $"[XsollaSubPeriod: mValue={mValue}, mUnit={mUnit}]";
	}
}
