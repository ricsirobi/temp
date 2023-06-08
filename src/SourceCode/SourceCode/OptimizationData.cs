using System;
using System.Xml.Serialization;

[Serializable]
public class OptimizationData
{
	[Serializable]
	public class LevelData
	{
		[XmlElement]
		public string SceneName;

		[XmlElement]
		public float DisplayRemoveDistance;

		[XmlElement]
		public float RaisedPetRemoveDistance;
	}

	[XmlElement(ElementName = "UpdateFrequency")]
	public int UpdateFrequency = 1;

	[XmlElement(ElementName = "LowMemoryThreshold")]
	public MinMax LowMemoryThreshold = new MinMax(0f, 15f);

	[XmlElement(ElementName = "MediumMemoryThreshold")]
	public MinMax MediumMemoryThreshold = new MinMax(20f, 35f);

	[XmlElement(ElementName = "HighMemoryThreshold")]
	public MinMax HighMemoryThreshold = new MinMax(40f, 99999f);

	[XmlElement(ElementName = "GlobalLevelData")]
	public LevelData GlobalLevelData;

	[XmlElement(ElementName = "LevelData")]
	public LevelData[] SpecificLevelData;

	[XmlElement(ElementName = "GoodFPS")]
	public int GoodFPS = 15;
}
