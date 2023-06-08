using System;
using System.Xml.Serialization;

namespace KA.Framework;

[Serializable]
[XmlRoot(ElementName = "AttachmentMedia", Namespace = "")]
public class AttachmentMedia
{
	[XmlElement(ElementName = "AttachmentMediaID", IsNullable = true)]
	public int? AttachmentMediaID;

	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Description")]
	public string Description;

	[XmlElement(ElementName = "AttachmentMediaFile")]
	public AttachmentMediaFile AttachmentMediaFile;

	[XmlElement(ElementName = "AttachmentMediaFileType")]
	public AttachmentMediaType AttachmentMediaFileType;
}
