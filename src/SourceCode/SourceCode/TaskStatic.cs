using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Data", Namespace = "")]
public class TaskStatic
{
	[XmlElement(ElementName = "Type")]
	public string Type;

	[XmlElement(ElementName = "Title", IsNullable = true)]
	public MissionInfo Title;

	[XmlElement(ElementName = "Desc", IsNullable = true)]
	public MissionInfo Description;

	[XmlElement(ElementName = "Reminder", IsNullable = true)]
	public MissionInfo Reminder;

	[XmlElement(ElementName = "Failure", IsNullable = true)]
	public MissionInfo Failure;

	[XmlElement(ElementName = "Time")]
	public int Time;

	[XmlElement(ElementName = "AutoComplete", IsNullable = true)]
	public TaskAutoComplete AutoComplete;

	[XmlElement(ElementName = "Setup", IsNullable = true)]
	public List<TaskSetup> Setups;

	[XmlElement(ElementName = "RandomSetup", IsNullable = true)]
	public TaskSetup RandomSetups;

	[XmlElement(ElementName = "Offer", IsNullable = true)]
	public List<MissionAction> Offers;

	[XmlElement(ElementName = "End", IsNullable = true)]
	public List<MissionAction> Ends;

	[XmlElement(ElementName = "Objective")]
	public List<TaskObjective> Objectives;

	[XmlElement(ElementName = "RemoveItem", IsNullable = true)]
	public List<MissionRemoveItem> RemoveItem;
}
