using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AdsWeight", Namespace = "")]
public class AdsWeight
{
	[XmlElement(ElementName = "ProviderName")]
	public string ProviderName;

	[XmlElement(ElementName = "AdType")]
	public AdType AdType;

	[XmlElement(ElementName = "Percent")]
	public float Percentage;

	[XmlElement(ElementName = "ProviderData", IsNullable = true)]
	public string CustomData;

	[XmlElement(ElementName = "SupportedEvents", IsNullable = true)]
	public AdEventType?[] SupportedEvents;

	[XmlElement(ElementName = "UnSupportedEvents", IsNullable = true)]
	public AdEventType?[] UnSupportedEvents;
}
