using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ExternalAuthData", IsNullable = true)]
public class ExternalAuthData
{
	[XmlElement(ElementName = "AccessToken", IsNullable = true)]
	public string AccessToken { get; set; }
}
