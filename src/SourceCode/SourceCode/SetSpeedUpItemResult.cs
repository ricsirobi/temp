using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "SetSpeedUpItemResult", Namespace = "")]
public class SetSpeedUpItemResult
{
	[XmlElement(ElementName = "CIID")]
	public int CommonInventoryID;

	[XmlElement(ElementName = "AMT")]
	public int Amount;

	[XmlElement(ElementName = "USES")]
	public bool Uses;

	[XmlElement(ElementName = "S")]
	public bool Success;

	[XmlElement(ElementName = "STA")]
	public SetSpeedUpItemStatus Status;
}
