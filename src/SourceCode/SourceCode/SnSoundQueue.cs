using System;
using UnityEngine;

[Serializable]
public class SnSoundQueue : SnISound
{
	public SnSettings _Settings;

	public AudioClip[] _ClipQueue;

	public SnTriggerList[] _TriggerQueue;

	public SnSoundQueue(int inSize)
	{
		_Settings = new SnSettings();
		_ClipQueue = new AudioClip[inSize];
		_TriggerQueue = new SnTriggerList[inSize];
	}

	public SnSoundQueue(int inSize, SnSettings inSettings)
	{
		_Settings = inSettings;
		_ClipQueue = new AudioClip[inSize];
		_TriggerQueue = new SnTriggerList[inSize];
	}

	public SnSoundQueue(AudioClip[] inClipQueue, SnSettings inSettings)
	{
		_ClipQueue = inClipQueue;
		_Settings = inSettings;
	}

	public SnSoundQueue(AudioClip[] inClipQueue, SnSettings inSettings, SnTriggerList[] inTriggerQueue)
	{
		_ClipQueue = inClipQueue;
		_Settings = inSettings;
		_TriggerQueue = inTriggerQueue;
	}

	public void Clear()
	{
		_Settings = new SnSettings();
		for (int i = 0; i < _ClipQueue.Length; i++)
		{
			_ClipQueue[i] = null;
			_TriggerQueue[i] = null;
		}
	}

	public int GetNextAvailableIndex()
	{
		for (int i = 0; i < _ClipQueue.Length; i++)
		{
			if (_ClipQueue[i] == null)
			{
				return i;
			}
		}
		return -1;
	}

	public int AddClip(AudioClip inClip)
	{
		int nextAvailableIndex = GetNextAvailableIndex();
		if (nextAvailableIndex != -1)
		{
			_ClipQueue[nextAvailableIndex] = inClip;
			if (nextAvailableIndex < _TriggerQueue.Length)
			{
				_TriggerQueue[nextAvailableIndex] = null;
			}
		}
		return nextAvailableIndex;
	}

	public int AddClip(AudioClip inClip, SnTriggerList inTriggers)
	{
		int nextAvailableIndex = GetNextAvailableIndex();
		if (nextAvailableIndex != -1)
		{
			_ClipQueue[nextAvailableIndex] = inClip;
			if (nextAvailableIndex < _TriggerQueue.Length)
			{
				_TriggerQueue[nextAvailableIndex] = inTriggers;
			}
		}
		return nextAvailableIndex;
	}

	public int AddClip(AudioClip inClip, SnTrigger[] inTriggers)
	{
		int nextAvailableIndex = GetNextAvailableIndex();
		if (nextAvailableIndex != -1)
		{
			_ClipQueue[nextAvailableIndex] = inClip;
			if (nextAvailableIndex < _TriggerQueue.Length && inTriggers != null)
			{
				_TriggerQueue[nextAvailableIndex] = new SnTriggerList(inTriggers);
			}
		}
		return nextAvailableIndex;
	}

	public SnChannel Play()
	{
		return Play(inForce: false);
	}

	public SnChannel Play(bool inForce)
	{
		return SnChannel.Play(_ClipQueue, _Settings, _TriggerQueue, inForce);
	}
}
