using System;
using System.Collections.Generic;
using UnityEngine;

public class ElementMatchSoundManager : MonoBehaviour
{
	[Serializable]
	public class Audio
	{
		public string _Name;

		public AudioClip _Clip;

		public string _POOL;
	}

	private static ElementMatchSoundManager mInstance;

	public string SFX_POOL = "SFX_POOL";

	public string DEFAULT_POOL = "DEFAULT_POOL";

	public string MUSIC_POOL = "MUSIC_POOL";

	public string AMBIENT_POOL = "AMBIENT_POOL";

	public string VO_POOL = "VO_POOL";

	public AudioClip _Music;

	public AudioClip _HelpVO;

	public AudioClip _TimeLow;

	public Audio[] _Sounds;

	private Dictionary<string, AudioClip> mClipNameMap = new Dictionary<string, AudioClip>();

	private SnChannel mChannelTimeLow;

	private SnChannel mChannelAmbient;

	public static ElementMatchSoundManager pInstance => mInstance;

	private void Awake()
	{
		mInstance = this;
		Audio[] sounds = _Sounds;
		foreach (Audio audio in sounds)
		{
			mClipNameMap.Add(audio._Name, audio._Clip);
		}
		SnChannel.MuteAll(mute: false);
	}

	private void Start()
	{
	}

	public void PlayAmbientMusic()
	{
		mChannelAmbient = SnChannel.Play(_Music, AMBIENT_POOL, inForce: true);
		if (null != mChannelAmbient)
		{
			mChannelAmbient.pLoop = true;
		}
	}

	public void StopAmbientMusic()
	{
		if (null != mChannelAmbient)
		{
			mChannelAmbient.Stop();
		}
	}

	public void Play(string audioName, string whichPool = "DEFAULT_POOL")
	{
		SnChannel.Play(mClipNameMap[audioName], whichPool, inForce: true);
	}

	public void PlayTimeLow()
	{
		if (null == mChannelTimeLow)
		{
			mChannelTimeLow = SnChannel.Play(_TimeLow, MUSIC_POOL, inForce: false);
			mChannelTimeLow.pLoop = true;
		}
	}

	public void StopTimeLow()
	{
		mChannelTimeLow.Stop();
		mChannelTimeLow = null;
		SnChannel.StopPool(MUSIC_POOL);
	}

	public void PlayAmbientMusic(bool play)
	{
		SnChannel.MutePool(AMBIENT_POOL, play);
	}

	public void PlayHelpVO()
	{
		SnChannel.Play(_HelpVO, VO_POOL, inForce: true, base.gameObject);
	}

	public void StopVo()
	{
		SnChannel.StopPool(VO_POOL);
	}
}
