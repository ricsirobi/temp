using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GetAccessTokenResponse", Namespace = "", IsNullable = true)]
public class GetAccessTokenResponse
{
	[XmlElement(ElementName = "success")]
	public bool Success = true;

	[XmlElement(ElementName = "accessTokenInfo", IsNullable = true)]
	public AccessTokenInfo AccessTokenInfo;
}
