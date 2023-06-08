using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "SetSpeedUpItemRequest", Namespace = "")]
public class SetSpeedUpItemRequest
{
	[XmlElement(ElementName = "PGID")]
	public int? ProductGroupID;

	[XmlElement(ElementName = "PID")]
	public int? ProductID;

	[XmlElement(ElementName = "UID")]
	public Guid UserID;

	[XmlElement(ElementName = "CIID")]
	public int CommonInventoryID;

	[XmlElement(ElementName = "UIPID")]
	public int UserItemPositionID;

	[XmlElement(ElementName = "IID")]
	public int ItemID;

	[XmlElement(ElementName = "STID")]
	public int StoreID;
}
