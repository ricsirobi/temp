using System;
using UnityEngine;

[Serializable]
public class SoundData
{
	public string _EventName;

	public SnChannel _Channel;

	public string _PoolName;

	public AudioClip[] _Sounds;
}
