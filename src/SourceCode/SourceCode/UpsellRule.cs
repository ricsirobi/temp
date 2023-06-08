using System;
using System.Xml.Serialization;

[Serializable]
public class UpsellRule
{
	[XmlAttribute("result")]
	public string result = "any";

	[XmlElement(ElementName = "ItemData")]
	public UpsellItemData[] ItemData;

	public bool IsResultMatching(string inResult)
	{
		string text = result.ToLower();
		string text2 = inResult.ToLower();
		if (!(text == "any") && !(text2 == "any"))
		{
			return text == text2;
		}
		return true;
	}
}
