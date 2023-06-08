using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SoundMapper
{
	[Serializable]
	public class SoundPair
	{
		public string Name;

		public AudioClip Clip;

		public bool PlayOnMobile;
	}

	public SoundPair[] _Map;

	private List<SnChannel> mPlayingSounds;

	private List<SnChannel> mFreeSounds;

	public AudioClip GetAudioClip(string SoundName)
	{
		for (int i = 0; i < _Map.Length; i++)
		{
			if (_Map[i].Name == SoundName && (!UtPlatform.IsMobile() || _Map[i].PlayOnMobile))
			{
				return _Map[i].Clip;
			}
		}
		return null;
	}

	public AudioClip GetRandomAudioClip(string SoundName)
	{
		List<AudioClip> list = new List<AudioClip>();
		for (int i = 0; i < _Map.Length; i++)
		{
			if (_Map[i].Name == SoundName && (!UtPlatform.IsMobile() || _Map[i].PlayOnMobile))
			{
				list.Add(_Map[i].Clip);
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count - 1)];
	}
}
