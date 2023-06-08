using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "DisplayNames", Namespace = "")]
public class DisplayNameList : List<DisplayName>
{
}
