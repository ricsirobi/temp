using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "LoginData", IsNullable = true)]
public class LoginData
{
	[XmlElement(ElementName = "UserName")]
	public string UserName;

	[XmlElement(ElementName = "Password")]
	public string Password;

	[XmlElement(ElementName = "Locale")]
	public string Locale;

	[XmlElement(ElementName = "ChildName")]
	public string ChildName { get; set; }

	[XmlElement(ElementName = "Age")]
	public int? Age { get; set; }
}
