using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "InvitePlayerResult", IsNullable = true)]
public class InvitePlayerResult
{
	[XmlElement(ElementName = "Success")]
	public bool Success;

	[XmlElement(ElementName = "InviterStatus")]
	public InvitePlayerStatus InviterStatus;

	[XmlElement(ElementName = "InviteeStatus")]
	public InvitePlayerPlayerResult[] InviteeStatus;
}
