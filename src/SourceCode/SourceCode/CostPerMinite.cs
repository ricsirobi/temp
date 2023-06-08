using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "CostPerMinite", Namespace = "")]
public class CostPerMinite
{
	[XmlElement(ElementName = "ItemID")]
	public int ItemID;

	[XmlElement(ElementName = "Duration")]
	public int Duration;

	private ItemData mCompletionItem;

	[XmlIgnore]
	public ItemData pCompletionItem
	{
		get
		{
			return mCompletionItem;
		}
		set
		{
			mCompletionItem = value;
		}
	}
}
