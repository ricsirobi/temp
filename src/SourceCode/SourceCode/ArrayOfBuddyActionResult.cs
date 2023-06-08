using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfBuddyActionResult", Namespace = "http://api.jumpstart.com/")]
public class ArrayOfBuddyActionResult
{
	[XmlElement(ElementName = "BuddyActionResult")]
	public BuddyActionResult[] BuddyActionResult;
}
