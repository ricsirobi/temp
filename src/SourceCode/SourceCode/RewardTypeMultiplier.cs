using System;
using System.Xml.Serialization;

public class RewardTypeMultiplier
{
	[XmlElement(ElementName = "RT")]
	public int RewardTypeID { get; set; }

	[XmlElement(ElementName = "MF")]
	public int MultiplierFactor { get; set; }

	[XmlElement(ElementName = "FD")]
	public DateTime FromDate { get; set; }

	[XmlElement(ElementName = "TD")]
	public DateTime ToDate { get; set; }

	[XmlElement(ElementName = "MO")]
	public bool MemberOnly { get; set; }
}
