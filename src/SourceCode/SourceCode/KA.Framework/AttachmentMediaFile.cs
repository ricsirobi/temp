using System;
using System.Xml.Serialization;

namespace KA.Framework;

[Serializable]
[XmlInclude(typeof(AttachmentMediaFileImage))]
[XmlRoot(ElementName = "AttachmentMediaFile", Namespace = "")]
public abstract class AttachmentMediaFile : ICloneable
{
	[XmlElement(ElementName = "AttachmentMediaFileID", IsNullable = true)]
	public int? AttachmentMediaFileID;

	public AttachmentMediaFile()
	{
	}

	public object Clone()
	{
		return DeepCopy();
	}

	public virtual AttachmentMediaFile DeepCopy()
	{
		return this;
	}
}
