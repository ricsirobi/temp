using System;
using UnityEngine;

[Serializable]
public class SnRandomSound : SnISound
{
	public SnSettings _Settings;

	public AudioClip[] _ClipList;

	public SnRandomSound(AudioClip[] inClipList)
	{
		_ClipList = inClipList;
		_Settings = new SnSettings();
	}

	public SnRandomSound(AudioClip[] inClipList, SnSettings inSettings)
	{
		_ClipList = inClipList;
		_Settings = inSettings;
	}

	public SnChannel Play()
	{
		return Play(inForce: false);
	}

	public SnChannel Play(bool inForce)
	{
		if (_ClipList == null)
		{
			return null;
		}
		if (_ClipList.Length < 1)
		{
			return null;
		}
		AudioClip audioClip = _ClipList[UnityEngine.Random.Range(0, _ClipList.Length)];
		if (audioClip == null)
		{
			return null;
		}
		return SnChannel.Play(audioClip, _Settings, inForce);
	}
}
