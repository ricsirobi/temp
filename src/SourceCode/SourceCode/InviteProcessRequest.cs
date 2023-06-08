using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "InviteProcessRequest", Namespace = "")]
public class InviteProcessRequest
{
	[XmlElement(ElementName = "InviterID")]
	public Guid InviterID { get; set; }

	[XmlElement(ElementName = "GroupID")]
	public Guid? GroupID { get; set; }

	[XmlElement(ElementName = "InviteeEmail")]
	public string InviteeEmail { get; set; }
}
