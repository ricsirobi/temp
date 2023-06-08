using System;
using System.Collections.Generic;
using UnityEngine;

public class ObCollectManager : KAMonoBase
{
	[Serializable]
	public class CollectablesData
	{
		public string _ObjectName;

		public int _Count;

		[NonSerialized]
		public int _CollectedCount;
	}

	public List<CollectablesData> _TaskCollectables;

	public string _ActionName;

	public void OnCarriedCollect(GameObject carriedObject)
	{
		if (_TaskCollectables == null || _TaskCollectables.Count == 0)
		{
			UtDebug.Log("Collectables item data is missing");
			return;
		}
		CollectablesData collectablesData = _TaskCollectables.Find((CollectablesData item) => carriedObject.name.Contains(item._ObjectName));
		if (collectablesData != null)
		{
			collectablesData._CollectedCount++;
			collectablesData = _TaskCollectables.Find((CollectablesData item) => item._CollectedCount < item._Count);
			if (collectablesData == null)
			{
				MissionManager.pInstance.CheckForTaskCompletion("Action", _ActionName);
			}
		}
	}
}
