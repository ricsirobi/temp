using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AuthorizeJoinResult", IsNullable = true)]
public class AuthorizeJoinResult
{
	[XmlElement(ElementName = "Success")]
	public bool Success;

	[XmlElement(ElementName = "Status")]
	public AuthorizeJoinStatus Status;
}
