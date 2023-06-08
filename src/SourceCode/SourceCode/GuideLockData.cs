using System;
using System.Xml.Serialization;

[Serializable]
public class GuideLockData
{
	[XmlAttribute("type")]
	public int Type;

	[XmlAttribute("unlockid")]
	public int UnlockID;

	[XmlAttribute("state")]
	public int State;

	public bool IsUnlocked(int unlockID, int type, int state)
	{
		if (UnlockID == unlockID && Type == type)
		{
			return State == state;
		}
		return false;
	}

	public bool IsUnlocked()
	{
		if (FieldGuideData.pUnlocked)
		{
			return true;
		}
		bool result = false;
		if (Type != 0 && UnlockID != 0 && State != 0)
		{
			if (Type == 1)
			{
				Mission mission = MissionManager.pInstance.GetMission(UnlockID);
				if (mission != null)
				{
					if (State == 1)
					{
						result = mission.pCompleted || mission.pStarted;
					}
					else if (State == 2)
					{
						result = mission.pCompleted;
					}
				}
			}
			else if (Type == 2)
			{
				Task task = MissionManager.pInstance.GetTask(UnlockID);
				if (task != null)
				{
					if (State == 1)
					{
						result = task.pCompleted || task.pStarted;
					}
					else if (State == 2)
					{
						result = task.pCompleted;
					}
				}
			}
		}
		return result;
	}
}
