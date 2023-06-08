using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "CreateGroupResult", IsNullable = true)]
public class CreateGroupResult
{
	[XmlElement(ElementName = "Success")]
	public bool Success;

	[XmlElement(ElementName = "Status")]
	public CreateGroupStatus Status;

	[XmlElement(ElementName = "Group")]
	public Group Group;
}
