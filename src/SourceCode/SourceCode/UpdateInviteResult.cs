using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UpdateInviteResult", IsNullable = true)]
public class UpdateInviteResult
{
	[XmlElement(ElementName = "Success")]
	public bool Success;

	[XmlElement(ElementName = "Status")]
	public UpdateInviteStatus Status;
}
