using System;
using System.Xml.Serialization;

namespace KA.Framework;

[Serializable]
[XmlRoot(ElementName = "AttachmentMediaFileImage", Namespace = "")]
public class AttachmentMediaFileImage : AttachmentMediaFile, ICloneable
{
	[XmlElement(ElementName = "LinkUrl")]
	public string LinkUrl;

	[XmlElement(ElementName = "Url")]
	public string Url;

	object ICloneable.Clone()
	{
		return DeepCopy();
	}

	public override AttachmentMediaFile DeepCopy()
	{
		return new AttachmentMediaFileImage
		{
			AttachmentMediaFileID = AttachmentMediaFileID,
			LinkUrl = LinkUrl,
			Url = Url
		};
	}
}
