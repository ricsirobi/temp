using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UB", Namespace = "", IsNullable = true)]
public class UserBonus
{
	[XmlElement(ElementName = "id", IsNullable = true)]
	public int? UserBonusID;

	[XmlElement(ElementName = "tid", IsNullable = true)]
	public int? UserBonusTypeID;

	[XmlElement(ElementName = "uid", IsNullable = true)]
	public Guid? UserID;

	[XmlElement(ElementName = "lbd", IsNullable = true)]
	public DateTime? LastBonusDate;

	[XmlElement(ElementName = "q", IsNullable = true)]
	public int? Quantity;

	public UserBonus()
	{
		Quantity = 0;
	}
}
