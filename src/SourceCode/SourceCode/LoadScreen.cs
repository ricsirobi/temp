using System;
using System.Xml.Serialization;

[Serializable]
public class LoadScreen
{
	public string Name;

	[XmlElement(ElementName = "Tag")]
	public string[] Tags;

	public string Partner;

	public LSGender Gender;

	public LSAge Age;

	public int? Member;

	public int Index;

	public bool IsAgeMatching(int inUserAge, bool inMatchType)
	{
		if (Age == null)
		{
			if (inMatchType)
			{
				return false;
			}
			return true;
		}
		if (inUserAge >= Age.Min && inUserAge <= Age.Max)
		{
			return true;
		}
		return false;
	}

	public bool IsGenderMatching(Gender inUserGender, bool inMatchType)
	{
		if (Gender == null)
		{
			if (inMatchType)
			{
				return false;
			}
			return true;
		}
		if (Gender.Gender == (int)inUserGender)
		{
			return true;
		}
		return false;
	}

	public bool IsTagMatching(string inTag)
	{
		if (inTag.Equals("Any"))
		{
			return true;
		}
		if (Tags == null)
		{
			return false;
		}
		bool result = false;
		string[] tags = Tags;
		for (int i = 0; i < tags.Length; i++)
		{
			if (tags[i].Equals(inTag))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public bool IsMembershipStatusMatching()
	{
		if (!Member.HasValue)
		{
			return true;
		}
		if (SubscriptionInfo.pIsMember && Member == 1)
		{
			return true;
		}
		if (!SubscriptionInfo.pIsMember && Member == 0)
		{
			return true;
		}
		return false;
	}
}
