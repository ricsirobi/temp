using System;
using System.Xml.Serialization;

[Serializable]
public class AgeUpData
{
	[XmlAttribute("type")]
	public string type = string.Empty;

	[XmlElement(ElementName = "Trigger")]
	public string Trigger;

	[XmlElement(ElementName = "Repeat")]
	public int Repeat;

	[XmlElement(ElementName = "PetStage")]
	public RaisedPetStage PetStage;

	[XmlElement(ElementName = "TaskID")]
	public int[] TaskID;
}
