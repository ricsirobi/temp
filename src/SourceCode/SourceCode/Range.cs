using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "R", Namespace = "")]
public class Range
{
	[XmlElement(ElementName = "SD")]
	public DateTime? StartDate;

	[XmlElement(ElementName = "ED")]
	public DateTime? EndDate;
}
