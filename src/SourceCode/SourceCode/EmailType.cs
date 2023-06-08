using System;
using System.Xml.Serialization;

[Serializable]
public enum EmailType
{
	[XmlEnum("1")]
	SocialBox = 1
}
