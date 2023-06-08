using System;
using System.Xml.Serialization;

[Serializable]
public class FieldGuideChapter
{
	[XmlElement(ElementName = "Title")]
	public FieldGuideItem Title;

	[XmlElement(ElementName = "Vocabulary")]
	public FieldGuideVocabulary[] Vocabularies;

	[XmlElement(ElementName = "GeneralInformation")]
	public FieldGuideItem GeneralInformation;

	[XmlElement(ElementName = "Experiment")]
	public FieldGuideItem[] Experiments;

	[XmlElement(ElementName = "Image")]
	public FieldGuideImage[] Images;

	public bool IsUnlocked(int unlockId, int type, int state)
	{
		if (Title.IsUnlocked(unlockId, type, state))
		{
			return true;
		}
		FieldGuideVocabulary[] vocabularies = Vocabularies;
		for (int i = 0; i < vocabularies.Length; i++)
		{
			if (vocabularies[i].IsUnlocked(unlockId, type, state))
			{
				return true;
			}
		}
		if (GeneralInformation != null && GeneralInformation.IsUnlocked(unlockId, type, state))
		{
			return true;
		}
		if (Experiments != null)
		{
			FieldGuideItem[] experiments = Experiments;
			for (int i = 0; i < experiments.Length; i++)
			{
				if (experiments[i].IsUnlocked(unlockId, type, state))
				{
					return true;
				}
			}
		}
		if (Images != null)
		{
			FieldGuideImage[] images = Images;
			foreach (FieldGuideItem fieldGuideItem in images)
			{
				if (fieldGuideItem != null && fieldGuideItem.IsUnlocked(unlockId, type, state))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsUnlocked()
	{
		if (Title.IsUnlocked())
		{
			return true;
		}
		FieldGuideVocabulary[] vocabularies = Vocabularies;
		for (int i = 0; i < vocabularies.Length; i++)
		{
			if (vocabularies[i].IsUnlocked())
			{
				return true;
			}
		}
		if (GeneralInformation != null && GeneralInformation.IsUnlocked())
		{
			return true;
		}
		if (Experiments != null && Experiments.Length != 0)
		{
			FieldGuideItem[] experiments = Experiments;
			for (int i = 0; i < experiments.Length; i++)
			{
				if (experiments[i].IsUnlocked())
				{
					return true;
				}
			}
		}
		if (Images != null && Images.Length != 0)
		{
			FieldGuideImage[] images = Images;
			for (int i = 0; i < images.Length; i++)
			{
				if (images[i].IsUnlocked())
				{
					return true;
				}
			}
		}
		return false;
	}
}
