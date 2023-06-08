using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UserGameCurrency", Namespace = "")]
public class UserGameCurrency
{
	[XmlElement(ElementName = "id")]
	public int? UserGameCurrencyID;

	[XmlElement(ElementName = "uid")]
	public Guid? UserID;

	[XmlElement(ElementName = "gc")]
	public int? GameCurrency;

	[XmlElement(ElementName = "cc")]
	public int? CashCurrency;

	public UserGameCurrency()
	{
		GameCurrency = 0;
		CashCurrency = 0;
	}
}
