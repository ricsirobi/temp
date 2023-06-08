using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ServerErrorMessages", Namespace = "")]
public class ServerErrorMessages
{
	[XmlElement(ElementName = "Error")]
	public ServerErrorMessageGroupInfo[] Errors;

	[XmlElement(ElementName = "ExcludeUrl")]
	public string[] ExcludeUrl;
}
