using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "CPR", Namespace = "")]
public class CreatePetResponse
{
	[XmlElement(ElementName = "rpd")]
	public RaisedPetData RaisedPetData { get; set; }

	[XmlElement(ElementName = "cir")]
	public CommonInventoryResponse UserCommonInventoryResponse { get; set; }
}
