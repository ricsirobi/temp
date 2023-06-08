using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "DNFC", Namespace = "")]
public class DNFCounter
{
	[XmlElement(ElementName = "typ")]
	public DNFType _Type;

	[XmlElement(ElementName = "ctr")]
	public int _Count;

	public DNFCounter()
	{
		_Type = DNFType.Default;
		_Count = 0;
	}

	public DNFCounter(DNFType type, int count)
	{
		_Type = type;
		_Count = count;
	}
}
