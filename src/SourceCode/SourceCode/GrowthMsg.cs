using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GrowthMsg", Namespace = "")]
public class GrowthMsg
{
	[XmlElement(ElementName = "PetTypeID")]
	public int mPetTypeID = -1;

	[XmlElement(ElementName = "AgeMsg")]
	public AgeMsg[] mAgeMsgs;
}
