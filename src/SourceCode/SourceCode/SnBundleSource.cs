using UnityEngine;

public class SnBundleSource : SnAudioSource
{
	internal bool mLoadComplete;

	internal bool mPaused;

	internal SnChannel mChannel;

	public override bool pPlaying
	{
		get
		{
			if (!(mSource != null))
			{
				return false;
			}
			if (!mLoadComplete)
			{
				return true;
			}
			return mSource.isPlaying;
		}
	}

	public override bool pPaused
	{
		get
		{
			return false;
		}
		set
		{
			if (mSource != null)
			{
				if (value && mLoadComplete)
				{
					mSource.Pause();
				}
				else
				{
					mPaused = value;
				}
			}
		}
	}

	internal SnBundleSource(AudioSource inSource, SnChannel inChannel)
		: base(inSource)
	{
		mChannel = inChannel;
		RsResourceManager.LoadAssetFromBundle(SnAudioSource.DATA_ROOT + inSource.clip.name, inSource.clip.name, ResourceEventHandler, typeof(AudioClip));
	}

	private void ResourceEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			mLoadComplete = true;
			AudioClip inClip = inObject as AudioClip;
			if (!(mSource != null) || !SnUtility.SafeClipCompare(mSource.clip, inClip))
			{
				break;
			}
			mSource.clip = inObject as AudioClip;
			UtDebug.Assert(mSource.clip != null, "AUDIO CLIP FROM BUNDLE IS NULL!!");
			if (!mPaused)
			{
				if (mSource.enabled && (mSource.gameObject == null || mSource.gameObject.activeSelf))
				{
					mSource.Play();
				}
				if (mChannel != null)
				{
					mChannel.SendEvent(SnEventType.PLAY, mChannel.pClip, mChannel._EventTarget);
				}
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("ERROR: AUDIO BUNDLE FAILED TO DOWNLOAD -- " + inURL);
			mLoadComplete = true;
			if (mChannel != null)
			{
				mChannel.SendEvent(SnEventType.STOP, mChannel.pClip, mChannel._EventTarget);
			}
			break;
		}
	}
}
