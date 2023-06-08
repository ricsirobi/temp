namespace Xsolla;

public class CurrencyFormatter
{
	public static string FormatPrice(string currency, string amount)
	{
		amount = currency switch
		{
			"USD" => "$" + amount, 
			"EUR" => "€" + amount, 
			"GBP" => "£" + amount, 
			"BRL" => "R$" + amount, 
			"RUB" => amount ?? "", 
			_ => amount + " " + currency, 
		};
		return amount;
	}
}
