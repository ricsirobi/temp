using System.Xml.Serialization;

public class DisplayName
{
	[XmlElement("ID")]
	public int? DisplayNameID;

	public string Name;

	public int? Ordinal;
}
