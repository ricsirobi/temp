using System;
using UnityEngine;

public class ObActivateAudio : MonoBehaviour
{
	[Serializable]
	public class AudioProperties
	{
		public AudioClip _Clip;

		public string _PoolName;

		public bool _PausePool;
	}

	public AudioProperties[] _Audio;

	private void Start()
	{
		for (int i = 0; i < _Audio.Length; i++)
		{
			if (!_Audio[i]._PausePool)
			{
				SnChannel.Play(_Audio[i]._Clip, _Audio[i]._PoolName, inForce: true);
			}
			else
			{
				SnChannel.PausePool(_Audio[i]._PoolName);
			}
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _Audio.Length; i++)
		{
			if (!_Audio[i]._PausePool)
			{
				SnChannel.StopPool(_Audio[i]._PoolName);
			}
			else
			{
				SnChannel.PlayPool(_Audio[i]._PoolName);
			}
		}
	}
}
