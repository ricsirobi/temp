using System;
using System.Xml.Serialization;
using KA.Framework;

[Serializable]
[XmlRoot(ElementName = "AdsNetworkEnableInfo", Namespace = "")]
public class AdsNetworkEnableInfo
{
	[XmlElement(ElementName = "ProviderName")]
	public string ProviderName;

	[XmlElement(ElementName = "Disabled")]
	public bool Disabled;

	[XmlElement(ElementName = "Platform")]
	public ProductPlatform Platform;

	[XmlElement(ElementName = "MinimumRAM")]
	public int MinimumRAM;
}
