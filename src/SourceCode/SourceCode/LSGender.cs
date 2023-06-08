using System;
using System.Xml.Serialization;

[Serializable]
public class LSGender
{
	[XmlAttribute("ID")]
	public int Gender;
}
