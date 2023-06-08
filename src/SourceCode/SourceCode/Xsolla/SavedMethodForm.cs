using SimpleJSON;

namespace Xsolla;

public class SavedMethodForm : IParseble
{
	private string paymentSid;

	public string getPaymentSid()
	{
		return paymentSid;
	}

	public IParseble Parse(JSONNode pJsonNode)
	{
		paymentSid = pJsonNode["paymentSid"];
		return this;
	}
}
