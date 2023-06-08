using System;
using UnityEngine;

public class AnimationSoundHandler : KAMonoBase
{
	public SoundData[] _SoundData;

	public void OnPlaySound(string eventName)
	{
		if (_SoundData == null || _SoundData.Length == 0)
		{
			return;
		}
		SoundData soundData = Array.Find(_SoundData, (SoundData item) => item._EventName.Equals(eventName));
		if (soundData != null && soundData._Sounds != null && soundData._Sounds.Length != 0)
		{
			AudioClip audioClip = soundData._Sounds[UnityEngine.Random.Range(0, soundData._Sounds.Length)];
			if (soundData._Channel != null)
			{
				soundData._Channel.pClip = audioClip;
				soundData._Channel.Play();
			}
			else
			{
				SnChannel.Play(audioClip, soundData._PoolName, inForce: true);
			}
		}
	}
}
