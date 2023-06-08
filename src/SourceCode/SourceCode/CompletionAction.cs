using System;
using System.Xml.Serialization;

[Serializable]
public class CompletionAction
{
	[XmlElement(ElementName = "Transition")]
	public StateTransition Transition;
}
