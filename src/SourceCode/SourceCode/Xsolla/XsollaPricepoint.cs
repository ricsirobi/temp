using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaPricepoint : AXsollaShopItem, IParseble
{
	public float outAmount { get; private set; }

	public float outWithoutDiscount { get; private set; }

	public float bonusOut { get; private set; }

	public float sum { get; private set; }

	public float sumWithoutDiscount { get; private set; }

	public string currency { get; private set; }

	public string image { get; private set; }

	public string desc { get; private set; }

	public List<XsollaBonusItem> bonusItems { get; private set; }

	public bool selected { get; private set; }

	public string GetImageUrl()
	{
		if (image.StartsWith("https:"))
		{
			return image;
		}
		return "https:" + image;
	}

	public string GetOutString()
	{
		return outAmount.ToString();
	}

	public string GetPriceString()
	{
		if (sum == sumWithoutDiscount)
		{
			return CurrencyFormatter.FormatPrice(currency, sum.ToString());
		}
		string text = CurrencyFormatter.FormatPrice(currency, sumWithoutDiscount.ToString());
		string text2 = CurrencyFormatter.FormatPrice(currency, sum.ToString());
		return "<size=10><color=#a7a7a7>" + text + "</color></size> " + text2;
	}

	public bool IsSpecialOffer()
	{
		if (sum == sumWithoutDiscount)
		{
			return bonusItems.Count > 0;
		}
		return true;
	}

	public string GetDescription()
	{
		return desc.ToString();
	}

	public override string GetKey()
	{
		return outAmount.ToString();
	}

	public override string GetName()
	{
		return outAmount.ToString();
	}

	public IParseble Parse(JSONNode pricepointNode)
	{
		outAmount = pricepointNode["out"].AsFloat;
		outWithoutDiscount = pricepointNode["outWithoutDiscount"].AsFloat;
		bonusOut = pricepointNode["bonusOut"].AsFloat;
		sum = pricepointNode["sum"].AsFloat;
		sumWithoutDiscount = pricepointNode["sumWithoutDiscount"].AsFloat;
		currency = pricepointNode["currency"].Value;
		image = pricepointNode["image"].Value;
		desc = pricepointNode["description"].Value;
		bonusItems = XsollaBonusItem.ParseMany(pricepointNode["bonusItems"]);
		base.label = pricepointNode["label"].Value;
		base.offerLabel = pricepointNode["offerLabel"].Value;
		selected = pricepointNode["selected"].AsBool;
		string value = pricepointNode["advertisementType"].Value;
		base.advertisementType = AdType.NONE;
		if (sum != sumWithoutDiscount || outAmount != outWithoutDiscount || bonusItems.Count > 0 || bonusOut > 0f)
		{
			base.advertisementType = AdType.SPECIAL_OFFER;
		}
		else if ("best_deal".Equals(value))
		{
			base.advertisementType = AdType.BEST_DEAL;
		}
		else if ("recommended".Equals(value))
		{
			base.advertisementType = AdType.RECCOMENDED;
		}
		return this;
	}
}
