using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "SetRaisedPetResponse", Namespace = "")]
public class SetRaisedPetResponse
{
	[XmlElement(ElementName = "ErrorMessage")]
	public string ErrorMessage { get; set; }

	[XmlElement(ElementName = "RaisedPetSetResult")]
	public RaisedPetSetResult RaisedPetSetResult { get; set; }

	[XmlElement(ElementName = "cir")]
	public CommonInventoryResponse UserCommonInventoryResponse { get; set; }
}
