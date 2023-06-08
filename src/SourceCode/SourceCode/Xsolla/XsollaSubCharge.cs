using SimpleJSON;

namespace Xsolla;

public class XsollaSubCharge : IParseble
{
	private decimal mAmount;

	private string mCurrency;

	public IParseble Parse(JSONNode rootNode)
	{
		mAmount = rootNode["amount"].AsDecimal;
		mCurrency = rootNode["currency"].Value;
		return this;
	}

	public override string ToString()
	{
		return CurrencyFormatter.FormatPrice(mCurrency, mAmount.ToString("0.00"));
	}
}
