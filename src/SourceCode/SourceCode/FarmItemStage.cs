using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FarmItemStage
{
	public int _ID;

	public string _Name;

	public string _PropName;

	private int mOrder;

	[NonSerialized]
	public int _Duration;

	[NonSerialized]
	public UserItemState _UserItemState;

	public List<FarmItemContextData> _ContextData;

	public GameObject _StageObject;

	public GameObject _StageFx;

	public GameObject _HarvestedObject;

	public int _HarvestCollectableCount = -1;

	public int pOrder
	{
		get
		{
			return mOrder;
		}
		set
		{
			mOrder = value;
		}
	}

	public List<ContextSensitiveState> GetFarmItemContextData(bool inIsBuildMode)
	{
		List<ContextSensitiveState> list = new List<ContextSensitiveState>();
		foreach (FarmItemContextData contextDatum in _ContextData)
		{
			if (contextDatum._IsBuildMode == inIsBuildMode)
			{
				list.Add(contextDatum._ContextSensitiveState);
			}
		}
		return list;
	}

	public List<ContextSensitiveState> GetFarmItemContextData()
	{
		List<ContextSensitiveState> list = new List<ContextSensitiveState>();
		foreach (FarmItemContextData contextDatum in _ContextData)
		{
			list.Add(contextDatum._ContextSensitiveState);
		}
		return list;
	}
}
