using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "VMSG", Namespace = "")]
public class ValidationMessage
{
	[XmlElement(ElementName = "ST", IsNullable = false)]
	public Status Status { get; set; }

	[XmlElement(ElementName = "EM", IsNullable = false)]
	public string Message { get; set; }
}
