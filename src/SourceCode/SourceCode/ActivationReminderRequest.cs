using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ActivationReminderRequest", IsNullable = true)]
public class ActivationReminderRequest
{
	[XmlElement(ElementName = "Email")]
	public string Email;

	[XmlElement(ElementName = "Pwd")]
	public string Pwd;
}
