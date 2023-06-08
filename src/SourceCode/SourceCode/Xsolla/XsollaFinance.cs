using SimpleJSON;

namespace Xsolla;

public class XsollaFinance : IParseble
{
	public class FinanceItemBase : IParseble
	{
		public float amount { get; private set; }

		public string currency { get; private set; }

		public FinanceItemBase()
		{
		}

		public FinanceItemBase(float newAmount, string newCurrency)
			: this()
		{
			amount = newAmount;
			currency = newCurrency;
		}

		public IParseble Parse(JSONNode baseFinanceItemNode)
		{
			amount = baseFinanceItemNode["amount"].AsFloat;
			currency = baseFinanceItemNode["currency"];
			return this;
		}
	}

	public class FinanceItem : FinanceItemBase
	{
		public float paymentAmount { get; private set; }

		public string paymentCurrency { get; private set; }

		public FinanceItem()
		{
		}

		public FinanceItem(float newAmount, string newCurrency, float newPaymentAmount, string newPaymentCurrency)
			: base(newAmount, newCurrency)
		{
			paymentAmount = newPaymentAmount;
			paymentCurrency = newPaymentCurrency;
		}

		public new IParseble Parse(JSONNode financeItemNode)
		{
			base.Parse(financeItemNode);
			paymentAmount = financeItemNode["payment_amount"].AsFloat;
			paymentCurrency = financeItemNode["payment_currency"];
			return this;
		}
	}

	public FinanceItem subTotal { get; private set; }

	public FinanceItemBase discount { get; private set; }

	public FinanceItemBase fee { get; private set; }

	public FinanceItem xsollaCredits { get; private set; }

	public FinanceItemBase total { get; private set; }

	public FinanceItemBase vat { get; private set; }

	public IParseble Parse(JSONNode financyNode)
	{
		if (financyNode["sub_total"] != null)
		{
			subTotal = new FinanceItem().Parse(financyNode["sub_total"]) as FinanceItem;
		}
		if (financyNode["discount"] != null)
		{
			discount = new FinanceItemBase().Parse(financyNode["discount"]) as FinanceItemBase;
		}
		if (financyNode["fee"] != null)
		{
			fee = new FinanceItemBase().Parse(financyNode["fee"]) as FinanceItemBase;
		}
		if (financyNode["xsolla_credits"] != null)
		{
			xsollaCredits = new FinanceItem().Parse(financyNode["xsolla_credits"]) as FinanceItem;
		}
		if (financyNode["total"] != null)
		{
			total = new FinanceItemBase().Parse(financyNode["total"]) as FinanceItemBase;
		}
		if (financyNode["vat"] != null)
		{
			vat = new FinanceItemBase().Parse(financyNode["vat"]) as FinanceItemBase;
		}
		return this;
	}
}
