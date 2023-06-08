using UnityEngine;

public class ZiplineController : SplineControl
{
	public AudioClip _Music;

	public bool _MusicLoop;

	public bool _StopMusicWhenDone = true;

	private Vector3 mReturnPosition;

	private Vector3 mReturnRotation;

	private bool mUseReturnPos;

	private SnChannel mMusicChannel;

	private GameObject mAvatar;

	public GameObject pAvatar
	{
		get
		{
			return mAvatar;
		}
		set
		{
			mAvatar = value;
		}
	}

	public void SetReturnPosition()
	{
		mReturnPosition = base.transform.position;
		mReturnRotation = base.transform.localEulerAngles;
		mUseReturnPos = true;
	}

	public void ClearSpline()
	{
		if (mSpline != null)
		{
			SetSpline(null);
		}
	}

	public override void SetSpline(Spline sp)
	{
		base.SetSpline(sp);
	}

	public void StartMusic()
	{
		if ((bool)_Music)
		{
			mMusicChannel = SnChannel.Play(_Music, "Music_Pool", inForce: false);
			if (_MusicLoop)
			{
				mMusicChannel.pAudioSource.loop = true;
			}
		}
	}

	public void StopMusic()
	{
		SnChannel.StopPool("Music_Pool");
	}

	public override void Update()
	{
		base.Update();
		if (mEndReached)
		{
			Stop(mEndReached);
		}
	}

	public void Stop(bool endReached)
	{
		ClearSpline();
		if (endReached && _StopMusicWhenDone)
		{
			StopMusic();
		}
		if (mAvatar != null)
		{
			Transform parent = mAvatar.transform.parent;
			mAvatar.transform.parent = null;
			if (endReached)
			{
				mAvatar.GetComponent<AvAvatarController>().pState = AvAvatarState.IDLE;
			}
			if (!mUseReturnPos)
			{
				Object.Destroy(parent.gameObject);
			}
		}
		if (mUseReturnPos)
		{
			base.transform.position = mReturnPosition;
			base.transform.localEulerAngles = mReturnRotation;
		}
	}
}
