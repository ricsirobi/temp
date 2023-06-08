using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "QualifyFactorList", Namespace = "")]
public class QualifyFactorList
{
	[XmlElement(ElementName = "IsIncludeList")]
	public bool IsIncludeList;

	[XmlElement(ElementName = "QualifyList")]
	public List<QualifyFactor> QualifyList;
}
