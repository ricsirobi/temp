using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "InvitePlayerPlayerResult", IsNullable = true)]
public class InvitePlayerPlayerResult
{
	[XmlElement(ElementName = "InviteeID")]
	public string InviteeID;

	[XmlElement(ElementName = "Status")]
	public InvitePlayerStatus Status;
}
