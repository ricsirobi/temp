using System;
using System.Xml.Serialization;

namespace KA.Framework;

[Serializable]
[XmlRoot(ElementName = "StreamPostLog", Namespace = "")]
public class StreamPostLog
{
	[XmlElement(ElementName = "StreamPostLogID")]
	public long? StreamPostLogID;

	[XmlElement(ElementName = "StreamPostID")]
	public int? StreamPostID;

	[XmlElement(ElementName = "ApplicationID")]
	public int? ApplicationID;

	[XmlElement(ElementName = "FacebookUserID")]
	public long? FacebookUserID;

	[XmlElement(ElementName = "CreateDate")]
	public DateTime? CreateDate;
}
