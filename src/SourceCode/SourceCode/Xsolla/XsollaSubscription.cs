using System.Collections.Generic;
using System.Text;
using SimpleJSON;

namespace Xsolla;

public class XsollaSubscription : IXsollaObject, IParseble
{
	public string id { get; private set; }

	public float chargeAmount { get; private set; }

	public float chargeAmountLocal { get; private set; }

	public float chargeAmountWithoutDiscount { get; private set; }

	public float chargeAmountWithoutDiscountLocal { get; private set; }

	public string chargeCurrency { get; private set; }

	public string chargeCurrencyLocal { get; private set; }

	public bool isActive { get; private set; }

	public bool isPossibleRenew { get; private set; }

	public bool isTrial { get; private set; }

	public int period { get; private set; }

	public int periodTrial { get; private set; }

	public string periodUnit { get; private set; }

	public string name { get; private set; }

	public string offerLabel { get; private set; }

	public string description { get; private set; }

	public int bonusVirtualCurrency { get; private set; }

	public List<XsollaBonusItem> bonusVirtualItems { get; private set; }

	public int promotionChargesCount { get; private set; }

	public string GetBounusString()
	{
		if (bonusVirtualItems.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<color=#2DAE7B>");
			stringBuilder.Append("+ ");
			foreach (XsollaBonusItem bonusVirtualItem in bonusVirtualItems)
			{
				stringBuilder.Append(bonusVirtualItem.name).Append(" ");
			}
			stringBuilder.Append("</color>");
			return stringBuilder.ToString();
		}
		return "";
	}

	public bool isOffer()
	{
		if (chargeAmountWithoutDiscount == chargeAmount && bonusVirtualCurrency == 0)
		{
			return bonusVirtualItems != null;
		}
		return true;
	}

	public string GetPeriodString(string per)
	{
		return per + " " + period + " " + periodUnit;
	}

	public bool IsSpecial()
	{
		return chargeAmount != chargeAmountWithoutDiscount;
	}

	public string GetPriceString()
	{
		if (!IsSpecial())
		{
			return CurrencyFormatter.FormatPrice(chargeCurrency, chargeAmount.ToString());
		}
		string text = CurrencyFormatter.FormatPrice(chargeCurrency, chargeAmountWithoutDiscount.ToString());
		string text2 = CurrencyFormatter.FormatPrice(chargeCurrency, chargeAmount.ToString());
		return "<size=10><color=#a7a7a7>" + text + "</color></size> " + text2;
	}

	public string GetKey()
	{
		return id.ToString();
	}

	public string GetName()
	{
		return name;
	}

	public IParseble Parse(JSONNode subscriptionNode)
	{
		id = subscriptionNode["id"];
		chargeAmount = subscriptionNode["charge_amount"].AsFloat;
		chargeAmountLocal = subscriptionNode["charge_amount_local"].AsFloat;
		chargeAmountWithoutDiscount = subscriptionNode["charge_amount_without_discount"].AsFloat;
		chargeAmountWithoutDiscountLocal = subscriptionNode["charge_amount_without_discount_local"].AsFloat;
		chargeCurrency = subscriptionNode["charge_currency"];
		chargeCurrencyLocal = subscriptionNode["charge_currency_local"];
		isActive = subscriptionNode["is_active"].AsBool;
		isPossibleRenew = subscriptionNode["is_possible_renew"].AsBool;
		isTrial = subscriptionNode["is_trial"].AsBool;
		period = subscriptionNode["period"].AsInt;
		periodTrial = subscriptionNode["perios_trial"].AsInt;
		periodUnit = subscriptionNode["period_unit"];
		name = subscriptionNode["name"];
		offerLabel = subscriptionNode["offer_label"];
		description = subscriptionNode["description"];
		bonusVirtualCurrency = subscriptionNode["bonus_virtual_currency"].AsInt;
		bonusVirtualItems = XsollaBonusItem.ParseMany(subscriptionNode["bonus_virtual_items"]);
		promotionChargesCount = subscriptionNode["promotion_charges_count"].AsInt;
		return this;
	}
}
