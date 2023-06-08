using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace KA.Framework;

[Serializable]
[XmlRoot(ElementName = "StreamPost", Namespace = "")]
public class StreamPost
{
	[XmlElement(ElementName = "StreamPostID", IsNullable = true)]
	public int? StreamPostID;

	[XmlElement(ElementName = "Message")]
	public string Message;

	[XmlElement(ElementName = "Attachment")]
	public Attachment Attachment;

	[XmlElement(ElementName = "ActionLinkText")]
	public string ActionLinkText;

	[XmlElement(ElementName = "ActionLinkUrl")]
	public string ActionLinkUrl;

	[XmlElement(ElementName = "TargetID", IsNullable = true)]
	public long? TargetID;

	[XmlElement(ElementName = "UserID", IsNullable = true)]
	public long? UserID;

	[XmlElement(ElementName = "StreamPostGroupings")]
	public List<StreamPostGrouping> StreamPostGroupings;
}
