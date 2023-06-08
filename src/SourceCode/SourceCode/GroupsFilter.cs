using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GroupsFilter", IsNullable = true)]
public class GroupsFilter
{
	[XmlElement(ElementName = "GroupType")]
	public GroupType? GroupType;

	[XmlElement(ElementName = "Locale")]
	public string Locale;

	[XmlElement(ElementName = "PointTypeID")]
	public int? PointTypeID;

	[XmlElement(ElementName = "Count")]
	public int? Count { get; set; }

	[XmlElement(ElementName = "FromDate", IsNullable = true)]
	public DateTime? FromDate { get; set; }

	[XmlElement(ElementName = "ToDate", IsNullable = true)]
	public DateTime? ToDate { get; set; }

	[XmlElement(ElementName = "Refresh", IsNullable = true)]
	public bool? Refresh { get; set; }
}
