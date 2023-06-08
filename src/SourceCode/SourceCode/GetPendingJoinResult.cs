using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GetPendingJoinResult", IsNullable = true)]
public class GetPendingJoinResult
{
	[XmlElement(ElementName = "Success")]
	public bool Success;

	[XmlElement(ElementName = "Requests")]
	public PendingJoinRequest[] Requests;
}
