using SimpleJSON;

namespace Xsolla;

public class XsollaSubDetailPaymentAcc : IParseble
{
	public int mId;

	public string mType;

	public IParseble Parse(JSONNode rootNode)
	{
		if (rootNode.Value == "null")
		{
			return null;
		}
		mId = rootNode["id"].AsInt;
		mType = rootNode["type"].Value;
		return this;
	}
}
