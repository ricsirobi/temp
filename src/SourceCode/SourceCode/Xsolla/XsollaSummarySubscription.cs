using SimpleJSON;

namespace Xsolla;

public class XsollaSummarySubscription : IParseble, IXsollaSummaryItem
{
	public float amount { get; private set; }

	public int period { get; private set; }

	public string currency { get; private set; }

	public string description { get; private set; }

	public string package_info { get; private set; }

	public string period_type { get; private set; }

	public string expiration_period_type { get; private set; }

	public string recurrent_type { get; private set; }

	public string date_next_charge { get; private set; }

	public string amount_next_charge { get; private set; }

	public string currency_next_charge { get; private set; }

	public string GetImgUrl()
	{
		return "";
	}

	public string GetName()
	{
		return period + " " + period_type + " " + description;
	}

	public string GetPrice()
	{
		return PriceFormatter.Format(amount, currency);
	}

	public string GetDescription()
	{
		return "until " + date_next_charge;
	}

	public string GetBonus()
	{
		return "";
	}

	public IParseble Parse(JSONNode purchaseNode)
	{
		amount = purchaseNode["amount"].AsFloat;
		period = purchaseNode["period"].AsInt;
		currency = purchaseNode["currency"];
		description = purchaseNode["description"];
		package_info = purchaseNode["package_info"];
		period_type = purchaseNode["period_type"];
		expiration_period_type = purchaseNode["expiration_period_type"];
		recurrent_type = purchaseNode["recurrent_type"];
		date_next_charge = purchaseNode["date_next_charge"];
		amount_next_charge = purchaseNode["amount_next_charge"];
		currency_next_charge = purchaseNode["currency_next_charge"];
		return this;
	}
}
