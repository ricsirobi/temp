using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Answers", IsNullable = true, Namespace = "")]
public class ProfileAnswer
{
	[XmlElement(ElementName = "ID")]
	public int ID;

	[XmlElement(ElementName = "T")]
	public string DisplayText;

	[XmlElement(ElementName = "Img")]
	public string ImageURL;

	[XmlElement(ElementName = "L")]
	public string Locale;

	[XmlElement(ElementName = "O")]
	public int Ordinal;

	[XmlElement(ElementName = "QID")]
	public int QuestionID;

	public ProfileAnswer()
	{
	}

	public ProfileAnswer(int id, int order, string dtext, string icon)
	{
		ID = id;
		Ordinal = order;
		DisplayText = dtext;
		ImageURL = icon;
	}
}
