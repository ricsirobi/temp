using System;
using System.Xml.Serialization;

[Serializable]
public class MaxMMOData
{
	[Serializable]
	public class MMOScene
	{
		[XmlElement("Scene")]
		public string Scene;

		[XmlElement("MaxMMO")]
		public int MaxMMO;

		[XmlElement("MaxFullMMO")]
		public int MaxFullMMO;
	}

	[XmlAttribute("DefaultMaxMMO")]
	public int DefaultMaxMMO;

	[XmlAttribute("DefaultMaxFullMMO")]
	public int DefaultMaxFullMMO;

	[XmlElement("SceneData")]
	public MMOScene[] SceneData;
}
