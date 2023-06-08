using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "TaskObjective", Namespace = "")]
public class TaskObjective
{
	[XmlElement(ElementName = "Pair")]
	public List<TaskPair> Pairs;

	[XmlIgnore]
	public GameObject _Object;

	[XmlIgnore]
	public GameObject _NPC;

	[XmlIgnore]
	public bool _WithinProximity;

	[XmlIgnore]
	public float _ProximityTimer;

	public int _Collected;

	public TYPE Get<TYPE>(string inKey)
	{
		int i = 0;
		for (int count = Pairs.Count; i < count; i++)
		{
			if (Pairs[i].Key == inKey)
			{
				return UtStringUtil.Parse(Pairs[i].Value, default(TYPE));
			}
		}
		return default(TYPE);
	}
}
