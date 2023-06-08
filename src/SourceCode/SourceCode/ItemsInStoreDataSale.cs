using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "SL", Namespace = "", IsNullable = true)]
public class ItemsInStoreDataSale
{
	[XmlElement(ElementName = "pcid")]
	public int PriceChangeId;

	[XmlElement(ElementName = "m")]
	public float Modifier;

	[XmlElement(ElementName = "ic")]
	public string Icon;

	[XmlElement(ElementName = "rid", IsNullable = true)]
	public int? RankId;

	[XmlElement(ElementName = "iids")]
	public int[] ItemIDs;

	[XmlElement(ElementName = "cids")]
	public int[] CategoryIDs;

	[XmlElement(ElementName = "ism", IsNullable = true)]
	public bool? ForMembers;

	[XmlElement(ElementName = "sd", IsNullable = true)]
	public DateTime? StartDate;

	[XmlElement(ElementName = "ed", IsNullable = true)]
	public DateTime? EndDate;

	public bool pForMembers
	{
		get
		{
			if (ForMembers.HasValue)
			{
				return ForMembers.Value;
			}
			return false;
		}
	}

	public bool IsOutdated()
	{
		if (StartDate.HasValue && DateTime.Compare(ServerTime.pCurrentTime, StartDate.Value) < 0)
		{
			return true;
		}
		if (EndDate.HasValue && DateTime.Compare(ServerTime.pCurrentTime, EndDate.Value) > 0)
		{
			return true;
		}
		return false;
	}

	public bool HasCategory(int inCategoryID)
	{
		if (CategoryIDs == null)
		{
			return false;
		}
		int[] categoryIDs = CategoryIDs;
		for (int i = 0; i < categoryIDs.Length; i++)
		{
			if (categoryIDs[i] == inCategoryID)
			{
				return true;
			}
		}
		return false;
	}
}
