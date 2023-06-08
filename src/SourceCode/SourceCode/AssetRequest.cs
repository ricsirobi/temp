using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AssetRequest", Namespace = "")]
public class AssetRequest
{
	[XmlElement(ElementName = "ProductID")]
	public int? ProductID;

	[XmlElement(ElementName = "PlatformID")]
	public int? PlatformID;

	[XmlElement(ElementName = "Locale")]
	public string Locale;

	[XmlElement(ElementName = "AssetName")]
	public string AssetName;

	[XmlElement(ElementName = "AppVersion")]
	public string AppVersion;
}
