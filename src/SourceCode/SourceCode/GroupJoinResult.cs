using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GroupJoinResult", IsNullable = true)]
public class GroupJoinResult
{
	[XmlElement(ElementName = "Success")]
	public bool Success;

	[XmlElement(ElementName = "Status")]
	public JoinGroupStatus Status;
}
