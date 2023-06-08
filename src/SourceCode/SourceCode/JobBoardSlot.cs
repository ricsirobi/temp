using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "JobBoardSlot", Namespace = "")]
public class JobBoardSlot
{
	[XmlElement(ElementName = "t")]
	public int TaskID;

	[XmlElement(ElementName = "ct", IsNullable = true)]
	public DateTime? CompletionTime;

	public bool pAdWatched { get; set; }
}
