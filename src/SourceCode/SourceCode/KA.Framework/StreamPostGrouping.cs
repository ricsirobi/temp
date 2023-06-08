using System;
using System.Xml.Serialization;

namespace KA.Framework;

[Serializable]
[XmlRoot(ElementName = "StreamPostGrouping", Namespace = "")]
public class StreamPostGrouping
{
	[XmlElement(ElementName = "StreamPostGroupingID", IsNullable = true)]
	public int? StreamPostGroupingID;

	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Description")]
	public string Description;
}
