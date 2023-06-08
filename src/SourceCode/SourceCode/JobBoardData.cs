using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "jbd", Namespace = "")]
public class JobBoardData
{
	[XmlElement(ElementName = "s")]
	public JobBoardSlot[] Slots;

	[XmlElement(ElementName = "l")]
	public int? LastTrashedTaskID;

	public static JobBoardData pInstance;

	public static void Init(string xmlString)
	{
		if (!string.IsNullOrEmpty(xmlString))
		{
			pInstance = UtUtilities.DeserializeFromXml(xmlString, typeof(JobBoardData)) as JobBoardData;
		}
	}

	public static void Init(int numSlots)
	{
		pInstance = new JobBoardData();
		pInstance.Slots = new JobBoardSlot[numSlots];
		pInstance.LastTrashedTaskID = null;
		for (int i = 0; i < pInstance.Slots.Length; i++)
		{
			pInstance.Slots[i] = new JobBoardSlot();
		}
	}

	public static JobBoardSlot GetJobBoardSlotFromTaskID(int inTaskID)
	{
		if (pInstance == null || pInstance.Slots == null || pInstance.Slots.Length == 0)
		{
			return null;
		}
		JobBoardSlot[] slots = pInstance.Slots;
		foreach (JobBoardSlot jobBoardSlot in slots)
		{
			if (jobBoardSlot.TaskID == inTaskID)
			{
				return jobBoardSlot;
			}
		}
		return null;
	}

	public static string GetJobBoardDataString()
	{
		return UtUtilities.ProcessSendObject(pInstance);
	}

	public static bool AddTaskID(int index, int taskID)
	{
		if (pInstance == null)
		{
			return false;
		}
		pInstance.Slots[index].TaskID = taskID;
		return true;
	}

	public static bool AddCompletionTime(int index, DateTime? time)
	{
		if (pInstance == null)
		{
			return false;
		}
		pInstance.Slots[index].CompletionTime = time;
		return true;
	}
}
