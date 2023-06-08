using SimpleJSON;

namespace Xsolla;

public class XsollaSummaryItem : IParseble, IXsollaSummaryItem
{
	public float quantity { get; private set; }

	public float amount { get; private set; }

	public string currency { get; private set; }

	public string name { get; private set; }

	public string imageUrl { get; private set; }

	public string description { get; private set; }

	public string longDescription { get; private set; }

	public bool isBonus { get; private set; }

	public string GetImgUrl()
	{
		if (imageUrl == null)
		{
			return "";
		}
		if (imageUrl.StartsWith("https:"))
		{
			return imageUrl;
		}
		return "https:" + imageUrl;
	}

	public string GetName()
	{
		return quantity + " " + name;
	}

	public string GetPrice()
	{
		return PriceFormatter.Format(amount, currency);
	}

	public string GetDescription()
	{
		if (!"null".Equals(description))
		{
			return description;
		}
		return "";
	}

	public string GetBonus()
	{
		if (!isBonus)
		{
			return "";
		}
		return "Bonus";
	}

	public IParseble Parse(JSONNode purchaseNode)
	{
		quantity = purchaseNode["quantity"].AsFloat;
		amount = purchaseNode["amount"].AsFloat;
		currency = purchaseNode["currency"];
		name = purchaseNode["name"];
		imageUrl = purchaseNode["image_url"];
		description = purchaseNode["description"].Value;
		longDescription = purchaseNode["longDescription"].Value;
		isBonus = purchaseNode["is_bonus"].AsBool;
		return this;
	}
}
