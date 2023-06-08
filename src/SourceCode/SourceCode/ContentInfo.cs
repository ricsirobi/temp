using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ContentInfo")]
public class ContentInfo
{
	[XmlElement(ElementName = "DisplayName")]
	public string DisplayName;

	[XmlElement(ElementName = "Description")]
	public string Description;

	[XmlElement(ElementName = "ThumbnailUrl")]
	public string ThumbnailUrl;

	[XmlElement(ElementName = "LinkUrl")]
	public string LinkUrl;

	[XmlElement(ElementName = "TextureUrl")]
	public string TextureUrl;

	[XmlElement(ElementName = "ContentType")]
	public ContentInfoType ContentType;

	[XmlElement(ElementName = "MemberOnly")]
	public bool MemberOnly;

	[XmlElement(ElementName = "RolloverUrl")]
	public string RolloverUrl;

	[XmlElement(ElementName = "CategoryUrl")]
	public string CategoryUrl;

	[XmlElement(ElementName = "LinkType")]
	public ContentLinkType LinkType;
}
