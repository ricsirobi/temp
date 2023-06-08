using System;
using System.Xml.Serialization;

public class TaskStateRequest
{
	[XmlElement(ElementName = "PGID")]
	public int ProductGroupID;

	[XmlElement(ElementName = "PID")]
	public int ProductID;

	[XmlElement(ElementName = "UID")]
	public Guid UserID;

	[XmlElement(ElementName = "MID")]
	public int MissionID;

	[XmlElement(ElementName = "TID")]
	public int TaskID;

	[XmlElement(ElementName = "C")]
	public bool Completed;

	[XmlElement(ElementName = "P")]
	public string Payload;

	[XmlElement(ElementName = "CID")]
	public int? ContainerID;

	[XmlElement(ElementName = "CIR")]
	public CommonInventoryRequest[] CommonInventoryRequestItems;
}
