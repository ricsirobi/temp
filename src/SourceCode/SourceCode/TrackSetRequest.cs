using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UT", Namespace = "")]
public class TrackSetRequest
{
	[XmlElement(ElementName = "id", IsNullable = true)]
	public int? UserTrackID;

	[XmlElement(ElementName = "utcid", IsNullable = false)]
	public int UserTrackCategoryID;

	[XmlElement(ElementName = "uid", IsNullable = true)]
	public Guid? UserID;

	[XmlElement(ElementName = "n", IsNullable = true)]
	public string Name;

	[XmlElement(ElementName = "sh", IsNullable = true)]
	public bool? IsShared;

	[XmlElement(ElementName = "sl", IsNullable = true)]
	public int? Slot;

	[XmlElement(ElementName = "utes", IsNullable = true)]
	public TrackElement[] TrackElements;
}
