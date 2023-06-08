using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RemoveMemberResult", IsNullable = true)]
public class RemoveMemberResult
{
	[XmlElement(ElementName = "Success")]
	public bool Success;

	[XmlElement(ElementName = "Status")]
	public RemoveMemberStatus Status;
}
