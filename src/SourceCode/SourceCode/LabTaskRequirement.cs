using System;
using System.Xml.Serialization;

[Serializable]
public class LabTaskRequirement
{
	[XmlElement(ElementName = "ItemName")]
	public string ItemName;

	[XmlElement(ElementName = "State")]
	public string State;

	[XmlElement(ElementName = "Tool")]
	public string Tool;

	[XmlIgnore]
	public bool pActive = true;
}
