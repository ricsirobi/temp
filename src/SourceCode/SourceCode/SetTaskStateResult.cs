using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "SetTaskStateResult", Namespace = "")]
public class SetTaskStateResult
{
	[XmlElement(ElementName = "S")]
	public bool Success;

	[XmlElement(ElementName = "T")]
	public SetTaskStateStatus Status;

	[XmlElement(ElementName = "A")]
	public string AdditionalStatusParams;

	[XmlElement(ElementName = "R")]
	public MissionCompletedResult[] MissionsCompleted;

	[XmlElement(ElementName = "C")]
	public CommonInventoryResponse CommonInvRes;
}
