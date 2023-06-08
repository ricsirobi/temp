using System;
using System.Xml.Serialization;

[Serializable]
public class DisciplinaryCoreDetail
{
	[Serializable]
	public class StandardData
	{
		[XmlElement(ElementName = "ID")]
		public string ID;

		[XmlElement(ElementName = "Description")]
		public LocaleString Description;
	}

	[XmlElement(ElementName = "Detail")]
	public LocaleString Detail;

	[XmlElement(ElementName = "StandardData")]
	public StandardData[] StandardDatas;
}
