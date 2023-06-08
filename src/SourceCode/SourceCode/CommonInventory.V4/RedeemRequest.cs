using System;
using System.Xml.Serialization;

namespace CommonInventory.V4;

[Serializable]
[XmlRoot(ElementName = "rr", Namespace = "")]
public class RedeemRequest
{
	[XmlElement(ElementName = "i", IsNullable = false)]
	public int ItemID { get; set; }

	[XmlElement(ElementName = "rc", IsNullable = false)]
	public int RedeemItemCount { get; set; }

	[XmlElement(ElementName = "loc", IsNullable = true)]
	public string Locale { get; set; }
}
