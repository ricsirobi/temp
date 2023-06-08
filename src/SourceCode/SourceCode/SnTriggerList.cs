using System;
using System.Collections;

[Serializable]
public class SnTriggerList
{
	public SnTrigger[] _TriggerData;

	public int pLength
	{
		get
		{
			if (_TriggerData == null)
			{
				return 0;
			}
			return _TriggerData.Length;
		}
	}

	public SnTrigger this[int inIdx]
	{
		get
		{
			if (_TriggerData == null)
			{
				return null;
			}
			return _TriggerData[inIdx];
		}
		set
		{
			if (_TriggerData != null)
			{
				_TriggerData[inIdx] = value;
			}
		}
	}

	public SnTriggerList()
	{
	}

	public SnTriggerList(int inSize)
	{
		_TriggerData = new SnTrigger[inSize];
	}

	public SnTriggerList(SnTrigger[] inArray)
	{
		_TriggerData = inArray;
	}

	public IEnumerator GetEnumerator()
	{
		return _TriggerData.GetEnumerator();
	}
}
