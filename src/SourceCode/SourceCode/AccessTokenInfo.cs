using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AccessTokenInfo", Namespace = "", IsNullable = true)]
public class AccessTokenInfo
{
	[XmlElement(ElementName = "accessToken")]
	public string AccessToken;

	[XmlElement(ElementName = "sandbox")]
	public bool? Sandbox;
}
