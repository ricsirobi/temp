using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AREQ", Namespace = "")]
public class UserAchievementInfoRequest
{
	[XmlElement(ElementName = "PID")]
	public int ProductGroupID;

	[XmlElement(ElementName = "UID")]
	public Guid UserID;

	[XmlElement(ElementName = "PTID")]
	public int PointTypeID;

	[XmlElement(ElementName = "T")]
	public RequestType Type;

	[XmlElement(ElementName = "M")]
	public ModeType Mode;

	[XmlElement(ElementName = "P")]
	public int Page;

	[XmlElement(ElementName = "Q")]
	public int Quantity;

	[XmlElement(ElementName = "FBIDS")]
	public List<long> FacebookUserIDs;
}
