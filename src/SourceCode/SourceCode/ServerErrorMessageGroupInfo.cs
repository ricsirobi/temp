using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ServerErrorMessageGroupInfo", Namespace = "")]
public class ServerErrorMessageGroupInfo
{
	[XmlElement(ElementName = "ErrorText")]
	public string[] ErrorTexts;

	[XmlElement(ElementName = "ErrorMessageText")]
	public LocaleString ErrorMessageText;

	[XmlElement(ElementName = "MaxRetries")]
	public int? MaxRetries;
}
