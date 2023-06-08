using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RegisterMsg", Namespace = "")]
public class RegisterMsg
{
	[XmlElement(ElementName = "Message")]
	public LocaleString mMessage;

	[XmlElement(ElementName = "Category")]
	public string mCategory = "";

	[XmlElement(ElementName = "Credits")]
	public int mCredits = 1000;

	[XmlElement(ElementName = "Frequency")]
	public int mFrequency;
}
