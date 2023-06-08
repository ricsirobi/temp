using System;
using UnityEngine;

[Serializable]
public class SnSound : SnISound
{
	public SnSettings _Settings;

	public AudioClip _AudioClip;

	public SnTriggerList _Triggers;

	public SnSound(AudioClip inClip, SnSettings inSettings)
	{
		_AudioClip = inClip;
		_Settings = inSettings;
	}

	public SnSound(AudioClip inClip, SnSettings inSettings, SnTriggerList inTriggers)
	{
		_AudioClip = inClip;
		_Settings = inSettings;
		_Triggers = inTriggers;
	}

	public SnSound(AudioClip inClip, SnSettings inSettings, SnTrigger[] inTriggers)
	{
		_AudioClip = inClip;
		_Settings = inSettings;
		if (inTriggers != null)
		{
			_Triggers = new SnTriggerList(inTriggers);
		}
	}

	public SnChannel Play()
	{
		return Play(inForce: false);
	}

	public SnChannel Play(bool inForce)
	{
		if (_AudioClip == null)
		{
			return null;
		}
		return SnChannel.Play(_AudioClip, _Settings, _Triggers, inForce);
	}
}
