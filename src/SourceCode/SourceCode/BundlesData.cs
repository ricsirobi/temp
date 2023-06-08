using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public class BundlesData
{
	[Serializable]
	public class Category
	{
		[Serializable]
		public class Bundle
		{
			[XmlText]
			public string Value;

			[XmlArray]
			public List<BundleTarget> Targets = new List<BundleTarget>();
		}

		[XmlAttribute("Name")]
		public string Name = "";

		[XmlArray]
		public List<Bundle> Bundles = new List<Bundle>();
	}

	[XmlElement("Category")]
	public Category[] Categories;

	[XmlElement]
	public string SceneTimeOut = "30";
}
