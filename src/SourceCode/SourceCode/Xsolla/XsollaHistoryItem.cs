using System;
using SimpleJSON;

namespace Xsolla;

public class XsollaHistoryItem : IXsollaObject, IParseble
{
	public string comment { get; private set; }

	public string couponeCode { get; private set; }

	public DateTime date { get; private set; }

	public int invoiceId { get; private set; }

	public string operationType { get; private set; }

	public float paymentAmount { get; private set; }

	public string paymentCurrency { get; private set; }

	public string paymentName { get; private set; }

	public string statusCode { get; private set; }

	public string statusText { get; private set; }

	public float userBalance { get; private set; }

	public string userCustom { get; private set; }

	public float vcAmount { get; private set; }

	public XsollaHistoryVirtualItems virtualItems { get; private set; }

	public string virtualItemsOperationType { get; private set; }

	public IParseble Parse(JSONNode pNode)
	{
		comment = pNode["comment"];
		couponeCode = pNode["couponCode"];
		date = DateTime.Parse(pNode["date"]);
		invoiceId = pNode["invoiceId"].AsInt;
		operationType = pNode["operationType"];
		paymentAmount = pNode["paymentAmount"].AsFloat;
		paymentCurrency = pNode["paymentCurrency"];
		paymentName = pNode["paymentName"];
		statusCode = pNode["statusCode"];
		statusText = pNode["statusText"];
		userBalance = pNode["userBalance"].AsFloat;
		userCustom = pNode["userCustom"];
		vcAmount = pNode["vcAmount"].AsFloat;
		virtualItems = new XsollaHistoryVirtualItems().Parse(pNode["virtualItems"]) as XsollaHistoryVirtualItems;
		virtualItemsOperationType = pNode["virtualItemsOperationType"];
		return this;
	}

	public string GetKey()
	{
		return date.ToString("u");
	}

	public string GetName()
	{
		return comment;
	}
}
