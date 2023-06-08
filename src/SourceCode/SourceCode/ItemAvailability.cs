using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Availability", Namespace = "")]
public class ItemAvailability
{
	[XmlElement(ElementName = "sdate", IsNullable = true)]
	public DateTime? StartDate;

	[XmlElement(ElementName = "edate", IsNullable = true)]
	public DateTime? EndDate;
}
