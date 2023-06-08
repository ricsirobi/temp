using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Qs", IsNullable = true, Namespace = "")]
public class ProfileQuestion
{
	[XmlElement(ElementName = "CID", IsNullable = false)]
	public int CategoryID;

	[XmlElement(ElementName = "Img")]
	public string ImageURL;

	[XmlElement(ElementName = "A")]
	public string IsActive;

	[XmlElement(ElementName = "L")]
	public string Locale;

	[XmlElement(ElementName = "Ord")]
	public int Ordinal;

	[XmlElement(ElementName = "ID")]
	public int ID;

	[XmlElement(ElementName = "T")]
	public string DisplayText;

	[XmlElement(ElementName = "Answers")]
	public ProfileAnswer[] Answers;

	public ProfileAnswer GetAnswer(int aid)
	{
		ProfileAnswer[] answers = Answers;
		foreach (ProfileAnswer profileAnswer in answers)
		{
			if (profileAnswer.ID == aid)
			{
				return profileAnswer;
			}
		}
		return null;
	}
}
