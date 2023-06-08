namespace Xsolla;

public class PriceFormatter
{
	public static string Format(string amountString, string currency)
	{
		float num = float.Parse(amountString);
		switch (currency)
		{
		case "USD":
			amountString = $"${num:0.00}";
			break;
		case "EUR":
			amountString = $"€{num:0.00}";
			break;
		case "GBP":
			amountString = $"£{num:0.00}";
			break;
		case "BRL":
			amountString = $"R${num:0.00}";
			break;
		case "RUR":
		case "RUB":
			amountString = $"{num:0.00}RUB";
			break;
		default:
			amountString = $"{num:0.00}" + currency;
			break;
		}
		return amountString;
	}

	public static string Format(int amountInt, string currency)
	{
		return Format(amountInt.ToString(), currency);
	}

	public static string Format(float amountFloat, string currency)
	{
		return Format(amountFloat.ToString(), currency);
	}
}
