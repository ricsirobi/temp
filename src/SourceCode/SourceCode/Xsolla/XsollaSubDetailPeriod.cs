using SimpleJSON;

namespace Xsolla;

public class XsollaSubDetailPeriod : IParseble
{
	public string mUnit;

	public int mValue;

	public IParseble Parse(JSONNode rootNode)
	{
		if (rootNode == (object)"null")
		{
			return null;
		}
		mUnit = rootNode["unit"].Value;
		mValue = rootNode["value"].AsInt;
		return this;
	}

	public override string ToString()
	{
		return string.Format("{1} {0}", mUnit, mValue);
	}
}
