using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace KA.Framework;

[Serializable]
[XmlRoot(ElementName = "Attachment", Namespace = "")]
public class Attachment
{
	[XmlElement(ElementName = "AttachmentID", IsNullable = true)]
	public int? AttachmentID;

	[XmlElement(ElementName = "Caption")]
	public string Caption;

	[XmlElement(ElementName = "Description")]
	public string Description;

	[XmlElement(ElementName = "CaptionLinkUrl")]
	public string CaptionLinkUrl;

	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "AttachmentMedia")]
	public List<AttachmentMedia> AttachmentMedia;
}
