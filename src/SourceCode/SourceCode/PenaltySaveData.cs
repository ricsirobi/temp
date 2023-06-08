using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RPD", Namespace = "")]
public class PenaltySaveData
{
	[XmlElement(ElementName = "pt")]
	public RacingManagerState PenaltyType;

	[XmlElement(ElementName = "ts")]
	public DateTime TimeStamp;

	[XmlElement(ElementName = "et")]
	public DateTime EndTime;

	[XmlElement(ElementName = "ct")]
	public List<DNFCounter> DNFCounts = new List<DNFCounter>();

	public void Reset()
	{
		EndTime = DateTime.MinValue;
		DNFCounts.Clear();
	}

	public void UpdatePenaltyCount(DNFType type, int value)
	{
		for (int i = 0; i < DNFCounts.Count; i++)
		{
			if (DNFCounts[i]._Type == type)
			{
				DNFCounts[i]._Count += value;
				return;
			}
		}
		DNFCounts.Add(new DNFCounter(type, 1));
	}

	public int GetPenaltyCount(DNFType type)
	{
		foreach (DNFCounter dNFCount in DNFCounts)
		{
			if (dNFCount._Type == type)
			{
				return dNFCount._Count;
			}
		}
		return 0;
	}

	public int TotalPenaltyCount()
	{
		int num = 0;
		foreach (DNFCounter dNFCount in DNFCounts)
		{
			num += dNFCount._Count;
		}
		return num;
	}

	public string Serialize()
	{
		TimeStamp = ServerTime.pCurrentTime;
		return UtUtilities.SerializeToString(this);
	}

	public static PenaltySaveData Deserialize(string data)
	{
		return UtUtilities.DeserializeFromXml(data, typeof(PenaltySaveData)) as PenaltySaveData;
	}
}
