using SimpleJSON;

namespace Xsolla;

public class XsollaUser : IParseble
{
	public struct Requisites
	{
		public string value { get; private set; }

		public bool isVisible { get; private set; }

		public bool idAllowModify { get; private set; }

		public Requisites(string newValue, bool newIsVisible, bool pIdAllowModify)
		{
			this = default(Requisites);
			value = newValue;
			isVisible = newIsVisible;
			idAllowModify = pIdAllowModify;
		}
	}

	public class VirtualCurrencyBalance
	{
		public double amount;

		public VirtualCurrencyBalance(double pAmount)
		{
			amount = pAmount;
		}
	}

	public class VirtualUserBalance
	{
		public string currency;

		public decimal amount;

		public VirtualUserBalance(string pCurrency, decimal pAmount)
		{
			currency = pCurrency;
			amount = pAmount;
		}
	}

	public struct Country
	{
		public string value { get; private set; }

		public bool allowModify { get; private set; }

		public Country(string newValue, bool newAllowModify)
		{
			this = default(Country);
			value = newValue;
			allowModify = newAllowModify;
		}
	}

	private Requisites requisites;

	private string local;

	private Country country;

	private int savedPaymentMethodCount;

	private string acceptLanguage;

	private string acceptEncoding;

	public VirtualUserBalance userBalance;

	public VirtualCurrencyBalance virtualCurrencyBalance;

	public string GetCountryIso()
	{
		return country.value;
	}

	public bool IsAllowChangeCountry()
	{
		return country.allowModify;
	}

	public string GetName()
	{
		return requisites.value;
	}

	public bool IdAllowModify()
	{
		return requisites.idAllowModify;
	}

	public IParseble Parse(JSONNode userNode)
	{
		if (userNode["requisites"].Count > 1)
		{
			requisites = new Requisites(userNode["requisites"]["value"], userNode["requisites"]["isVisible"].AsBool, userNode["requisites"]["id_allow_modify"].AsBool);
		}
		country = new Country(userNode["country"]["value"], userNode["country"]["allow_modify"].AsBool);
		local = userNode["local"];
		savedPaymentMethodCount = userNode["savedPaymentMethodCount"].AsInt;
		acceptLanguage = userNode["acceptLanguage"];
		acceptEncoding = userNode["acceptEncoding"];
		userBalance = new VirtualUserBalance(userNode["user_balance"]["currency"], userNode["user_balance"]["amount"].AsDecimal);
		if (userNode["virtual_currency_balance"]["amount"] != null)
		{
			virtualCurrencyBalance = new VirtualCurrencyBalance(userNode["virtual_currency_balance"]["amount"].AsDouble);
		}
		return this;
	}

	public override string ToString()
	{
		return $"[XsollaUser] \n requisites {requisites}\n local {local}\n country {country}\n savedPaymentMethodCount {savedPaymentMethodCount}\n acceptLanguage {acceptLanguage}\n acceptEncoding {acceptEncoding}";
	}
}
