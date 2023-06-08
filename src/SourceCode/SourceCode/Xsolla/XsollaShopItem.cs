using System.Collections.Generic;
using System.Text;
using SimpleJSON;

namespace Xsolla;

public class XsollaShopItem : AXsollaShopItem, IParseble
{
	private long id;

	private string sku;

	private string name;

	private string imageUrl;

	private string description;

	private string descriptionLong;

	private string currency;

	private string virtCurrency = "Coins";

	private float amount;

	private float amountWithoutDiscount;

	private float vcAmount;

	private float vcAmountWithoutDiscount;

	private int quantityLimit;

	private int isFavorite;

	private string[] unsatisfiedUserAttributes;

	private XsollaBonusItem bonusVirtualCurrency;

	private List<XsollaBonusItem> bonusVirtualItems;

	public void SetVirtCurrName(string pName)
	{
		virtCurrency = pName;
	}

	public long GetId()
	{
		return id;
	}

	public string GetBounusString()
	{
		if (bonusVirtualItems.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<color=#2DAE7B>");
			stringBuilder.Append("+ ");
			foreach (XsollaBonusItem bonusVirtualItem in bonusVirtualItems)
			{
				stringBuilder.Append(bonusVirtualItem.name).Append(" free ");
			}
			stringBuilder.Append("</color>");
			return stringBuilder.ToString();
		}
		if (bonusVirtualCurrency != null && bonusVirtualCurrency.quantity != null && !"".Equals(bonusVirtualCurrency.quantity))
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append("<color=#2DAE7B>");
			stringBuilder2.Append("+ ");
			stringBuilder2.Append(bonusVirtualCurrency.quantity).Append(bonusVirtualCurrency.name).Append(" free ");
			stringBuilder2.Append("</color>");
			return stringBuilder2.ToString();
		}
		return "";
	}

	public string GetImageUrl()
	{
		if (imageUrl != null)
		{
			if (imageUrl.StartsWith("https:"))
			{
				return imageUrl;
			}
			return "https:" + imageUrl;
		}
		return null;
	}

	public string GetPriceString()
	{
		if (!IsVirtualPayment())
		{
			if (amount == amountWithoutDiscount)
			{
				return CurrencyFormatter.FormatPrice(currency, amount.ToString());
			}
			string text = CurrencyFormatter.FormatPrice(currency, amountWithoutDiscount.ToString());
			string text2 = CurrencyFormatter.FormatPrice(currency, amount.ToString());
			return "<size=10><color=#a7a7a7>" + text + "</color></size> " + text2;
		}
		if (vcAmount == vcAmountWithoutDiscount)
		{
			return CurrencyFormatter.FormatPrice(virtCurrency, vcAmount.ToString());
		}
		string text3 = CurrencyFormatter.FormatPrice(virtCurrency, vcAmountWithoutDiscount.ToString());
		string text4 = CurrencyFormatter.FormatPrice(virtCurrency, vcAmount.ToString());
		return "<size=10><color=#a7a7a7>" + text3 + "</color></size> " + text4;
	}

	public bool IsVirtualPayment()
	{
		if (!(vcAmount > 0f))
		{
			return vcAmountWithoutDiscount > 0f;
		}
		return true;
	}

	public string GetSku()
	{
		return sku;
	}

	public override string GetKey()
	{
		return sku.ToString();
	}

	public override string GetName()
	{
		return name;
	}

	public string GetDescription()
	{
		return description;
	}

	public string GetLongDescription()
	{
		return descriptionLong;
	}

	public bool IsFavorite()
	{
		if (isFavorite != 0)
		{
			return true;
		}
		return false;
	}

	public IParseble Parse(JSONNode shopItemNode)
	{
		id = shopItemNode["id"].AsInt;
		sku = shopItemNode["sku"].Value;
		name = shopItemNode["name"].Value;
		description = shopItemNode["description"].Value;
		descriptionLong = shopItemNode["long_description"].Value;
		imageUrl = shopItemNode["image_url"].Value;
		amount = shopItemNode["amount"].AsFloat;
		amountWithoutDiscount = shopItemNode["amount_without_discount"].AsFloat;
		vcAmount = shopItemNode["vc_amount"].AsFloat;
		vcAmountWithoutDiscount = shopItemNode["vc_amount_without_discount"].AsFloat;
		currency = shopItemNode["currency"].Value;
		bonusVirtualItems = XsollaBonusItem.ParseMany(shopItemNode["bonus_virtual_items"]);
		XsollaBonusItem xsollaBonusItem = new XsollaBonusItem();
		xsollaBonusItem.Parse(shopItemNode["bonus_virtual_currency"]);
		bonusVirtualCurrency = xsollaBonusItem;
		base.label = shopItemNode["label"].Value;
		isFavorite = shopItemNode["is_favorite"].AsInt;
		base.offerLabel = shopItemNode["offer_label"].Value;
		string value = shopItemNode["advertisement_type"].Value;
		base.advertisementType = AdType.NONE;
		if (amount != amountWithoutDiscount || bonusVirtualItems.Count > 0)
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
