using UnityEngine;

public class SnAudioSource : SnIStream
{
	public static string DATA_ROOT = "RS_SOUND/";

	internal AudioSource mSource;

	public virtual bool pPlaying
	{
		get
		{
			if (!(mSource != null))
			{
				return false;
			}
			return mSource.isPlaying;
		}
	}

	public virtual bool pPaused
	{
		get
		{
			return false;
		}
		set
		{
			if (mSource != null && value)
			{
				mSource.Pause();
			}
		}
	}

	public bool pActive => false;

	public uint pPosition
	{
		get
		{
			if (!(mSource != null))
			{
				return 0u;
			}
			return (uint)Mathf.RoundToInt(mSource.time * 1000f);
		}
	}

	public uint pLength
	{
		get
		{
			if (!(mSource != null))
			{
				return 0u;
			}
			if (!(mSource.clip != null))
			{
				return 0u;
			}
			return (uint)Mathf.RoundToInt(mSource.clip.length * 1000f);
		}
	}

	public float pVolume
	{
		get
		{
			if (!(mSource != null))
			{
				return 0f;
			}
			return mSource.volume;
		}
		set
		{
			if (mSource != null)
			{
				mSource.volume = value;
			}
		}
	}

	public bool pLoop
	{
		get
		{
			if (!(mSource != null))
			{
				return false;
			}
			return mSource.loop;
		}
		set
		{
			if (mSource != null)
			{
				mSource.loop = value;
			}
		}
	}

	public void Stop()
	{
		if (mSource != null)
		{
			mSource.Stop();
		}
	}

	internal SnAudioSource(AudioSource inSource)
	{
		mSource = inSource;
	}
}
