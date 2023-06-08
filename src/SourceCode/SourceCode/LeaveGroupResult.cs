using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "LeaveGroupResult", IsNullable = true)]
public class LeaveGroupResult
{
	[XmlElement(ElementName = "Success")]
	public bool Success;

	[XmlElement(ElementName = "Status")]
	public LeaveGroupStatus Status;
}
