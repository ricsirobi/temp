using System;
using System.Xml.Serialization;

[Serializable]
public class FieldGuideData
{
	public static bool pUnlocked;

	[XmlElement(ElementName = "Subject")]
	public FieldGuideSubject[] Subjects;
}
