using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public class Experiment
{
	[XmlElement(ElementName = "ID")]
	public int ID;

	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Priority")]
	public int Priority;

	[XmlElement(ElementName = "ThermometerMin")]
	public float ThermometerMin;

	[XmlElement(ElementName = "ThermometerMax")]
	public float ThermometerMax;

	[XmlElement(ElementName = "LabItem")]
	public string[] Items;

	[XmlElement(ElementName = "LabTask")]
	public LabTask[] Tasks;

	[XmlElement(ElementName = "ResultImage")]
	public string ResultImage;

	[XmlElement(ElementName = "Type")]
	public int Type;

	[XmlElement(ElementName = "DragonType")]
	public int DragonType = 105;

	[XmlElement(ElementName = "DragonStage")]
	public RaisedPetStage DragonStage = RaisedPetStage.TEEN;

	[XmlElement(ElementName = "DragonGender")]
	public Gender DragonGender = Gender.Male;

	[XmlElement(ElementName = "BreathType")]
	public int BreathType;

	[XmlElement(ElementName = "ForceDefaultDragon")]
	public bool ForceDefaultDragon;

	private List<LabItem> mItems;

	public List<LabItem> GetItems()
	{
		if (mItems == null)
		{
			if (!LabData.pIsReady)
			{
				return null;
			}
			mItems = new List<LabItem>();
			string[] items = Items;
			foreach (string inName in items)
			{
				LabItem item = LabData.pInstance.GetItem(inName);
				if (item != null)
				{
					mItems.Add(item);
				}
			}
		}
		return mItems;
	}

	public LabTask GetFirstTask()
	{
		if (Tasks == null || Tasks.Length == 0)
		{
			return null;
		}
		return Tasks[0];
	}

	public LabTask GetLastTask()
	{
		if (Tasks == null && Tasks.Length == 0)
		{
			return null;
		}
		return Tasks[Tasks.Length - 1];
	}

	public LabTask GetNextTask(LabTask inTask)
	{
		if (inTask == null || Tasks == null || Tasks.Length == 0)
		{
			return GetFirstTask();
		}
		for (int i = 0; i < Tasks.Length; i++)
		{
			LabTask labTask = Tasks[i];
			if (labTask != null && labTask.Action == inTask.Action)
			{
				if (Tasks.Length == i + 1)
				{
					return null;
				}
				return Tasks[i + 1];
			}
		}
		return GetFirstTask();
	}

	public LabTask GetAnyIncompleteTask()
	{
		if (Tasks == null || Tasks.Length == 0)
		{
			return null;
		}
		for (int i = 0; i < Tasks.Length; i++)
		{
			if (!Tasks[i].pDone)
			{
				return Tasks[i];
			}
		}
		return null;
	}

	public LabTask GetPreviousTask(LabTask inTask)
	{
		if (inTask == null || Tasks == null || Tasks.Length == 0)
		{
			return GetFirstTask();
		}
		for (int i = 0; i < Tasks.Length; i++)
		{
			LabTask labTask = Tasks[i];
			if (labTask != null && labTask.Action == inTask.Action)
			{
				if (i != 0)
				{
					return Tasks[i - 1];
				}
				return null;
			}
		}
		return GetFirstTask();
	}

	public bool AreAllTasksDone()
	{
		if (Tasks == null || Tasks.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < Tasks.Length; i++)
		{
			if (Tasks[i] == null || !Tasks[i].pDone)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsTaskDone(string inTaskAction)
	{
		if (Tasks == null || Tasks.Length == 0 || string.IsNullOrEmpty(inTaskAction))
		{
			return false;
		}
		for (int i = 0; i < Tasks.Length; i++)
		{
			if (Tasks[i] != null && Tasks[i].Action == inTaskAction && Tasks[i].pDone)
			{
				return true;
			}
		}
		return false;
	}
}
