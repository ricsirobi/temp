using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ItemStateRule", Namespace = "")]
public class ItemStateRule
{
	[XmlElement(ElementName = "Criterias")]
	public List<ItemStateCriteria> Criterias;

	[XmlElement(ElementName = "CompletionAction")]
	public CompletionAction CompletionAction;
}
