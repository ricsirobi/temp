using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "MissionRule", Namespace = "")]
public class MissionRule
{
	[XmlElement(ElementName = "Prerequisites")]
	public List<PrerequisiteItem> Prerequisites;

	[XmlElement(ElementName = "Criteria")]
	public MissionCriteria Criteria;

	public MissionRule()
	{
		Prerequisites = new List<PrerequisiteItem>();
		Criteria = new MissionCriteria();
	}

	public TYPE GetPrerequisite<TYPE>(PrerequisiteRequiredType inType)
	{
		PrerequisiteItem prerequisiteItem = Prerequisites.Find((PrerequisiteItem prerequisite) => prerequisite.Type == inType);
		if (prerequisiteItem != null)
		{
			return UtStringUtil.Parse(prerequisiteItem.Value, default(TYPE));
		}
		return default(TYPE);
	}

	public List<TYPE> GetPrerequisites<TYPE>(PrerequisiteRequiredType inType)
	{
		List<PrerequisiteItem> list = Prerequisites.FindAll((PrerequisiteItem prerequisite) => prerequisite.Type == inType);
		List<TYPE> list2 = new List<TYPE>();
		if (list != null)
		{
			foreach (PrerequisiteItem item in list)
			{
				list2.Add(UtStringUtil.Parse(item.Value, default(TYPE)));
			}
		}
		return list2;
	}

	public void Reset()
	{
		foreach (RuleItem ruleItem in Criteria.RuleItems)
		{
			ruleItem.Complete = 0;
		}
	}
}
