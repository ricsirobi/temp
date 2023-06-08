using System.Xml.Serialization;

namespace KA.Framework;

public enum AttachmentMediaType
{
	[XmlEnum("Image")]
	Image = 1,
	[XmlEnum("Flash")]
	Flash,
	[XmlEnum("Mp3")]
	Mp3
}
