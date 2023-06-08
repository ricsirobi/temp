using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "TaskAutoComplete", Namespace = "")]
public class TaskAutoComplete
{
	[XmlElement(ElementName = "Pair")]
	public List<TaskPair> Pairs;
}
