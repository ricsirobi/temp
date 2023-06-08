using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Data", Namespace = "")]
public class TaskPayload
{
	[XmlElement(ElementName = "S")]
	public bool Started;

	[XmlElement(ElementName = "P")]
	public List<TaskPair> Pairs;

	public TYPE Get<TYPE>(string inKey)
	{
		if (Pairs != null)
		{
			TaskPair taskPair = Pairs.Find((TaskPair p) => p.Key == inKey);
			if (taskPair != null)
			{
				return UtStringUtil.Parse(taskPair.Value, default(TYPE));
			}
		}
		return default(TYPE);
	}

	public void Set(string inKey, string inValue)
	{
		if (Pairs == null)
		{
			Pairs = new List<TaskPair>();
		}
		TaskPair taskPair = Pairs.Find((TaskPair p) => p.Key == inKey);
		if (taskPair != null)
		{
			taskPair.Value = inValue;
			return;
		}
		taskPair = new TaskPair();
		taskPair.Key = inKey;
		taskPair.Value = inValue;
		Pairs.Add(taskPair);
	}
}
