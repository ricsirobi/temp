using System;
using System.Xml.Serialization;

[Serializable]
public class LabTask
{
	[XmlElement(ElementName = "Action")]
	public string Action;

	[XmlElement(ElementName = "InstructionText")]
	public LocaleString InstructionText;

	[XmlElement(ElementName = "StopExciteOnRecordingInJournal")]
	public bool StopExciteOnRecordingInJournal = true;

	[XmlElement(ElementName = "ResultText")]
	public LocaleString ResultText;

	[XmlElement(ElementName = "Requirement")]
	public LabTaskRequirement Requirement;

	[XmlElement(ElementName = "HaltCondition")]
	public LabKeyValue[] HaltConditions;

	[XmlElement(ElementName = "Rule")]
	public LabTaskRule[] Rules;

	[XmlElement(ElementName = "PlayExciteAlways")]
	public bool PlayExciteAlways = true;

	public bool pMetRuleConditions { get; set; }

	public bool pDone { get; set; }

	public bool NeedHalt(string inName, string inValue)
	{
		if (string.IsNullOrEmpty(inName) || string.IsNullOrEmpty(inValue) || HaltConditions == null || HaltConditions.Length == 0)
		{
			return false;
		}
		LabKeyValue[] haltConditions = HaltConditions;
		foreach (LabKeyValue labKeyValue in haltConditions)
		{
			if (labKeyValue != null && labKeyValue.Name == inName && labKeyValue.Value == inValue)
			{
				return true;
			}
		}
		return false;
	}
}
