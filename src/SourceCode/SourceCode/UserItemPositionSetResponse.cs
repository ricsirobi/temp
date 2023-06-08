using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UIPSRS", Namespace = "")]
public class UserItemPositionSetResponse
{
	[XmlElement(ElementName = "s")]
	public bool Success;

	[XmlElement(ElementName = "ids")]
	public int[] CreatedUserItemPositionIDs;

	[XmlElement(ElementName = "r")]
	public ItemPositionValidationResult Result;

	[XmlElement(ElementName = "uciis")]
	public UserItemState[] UserItemStates;
}
