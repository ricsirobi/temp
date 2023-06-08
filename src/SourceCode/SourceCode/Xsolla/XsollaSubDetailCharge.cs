using System;
using SimpleJSON;

namespace Xsolla;

public class XsollaSubDetailCharge : IParseble
{
	public XsollaSubCharge mCharge;

	public DateTime mDateCreate;

	public string mPaymentMethod;

	public IParseble Parse(JSONNode rootNode)
	{
		mCharge = new XsollaSubCharge().Parse(rootNode["charge"]) as XsollaSubCharge;
		if (rootNode["date_create"] != null)
		{
			mDateCreate = DateTime.Parse(rootNode["date_create"].Value);
		}
		mPaymentMethod = rootNode["payment_method"].Value;
		return this;
	}
}
