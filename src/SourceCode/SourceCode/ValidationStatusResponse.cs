using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "vsr", Namespace = "")]
public class ValidationStatusResponse
{
	[XmlElement(ElementName = "i")]
	public int ItemID { get; set; }

	[XmlElement(ElementName = "ves")]
	public ValidationStatus ValidationStatus { get; set; }

	[XmlElement(ElementName = "vem")]
	public string ErrorMessage { get; set; }
}
