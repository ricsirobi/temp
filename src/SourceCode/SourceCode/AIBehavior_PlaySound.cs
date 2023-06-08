using UnityEngine;

public class AIBehavior_PlaySound : AIBehavior
{
	public AudioClip _AudioClip;

	public bool _PlayOnMobile;

	public string _SoundName;

	public bool _StopSoundOnTerminate;

	public bool _WaitForSoundToComplete;

	public SnSettings _Settings = new SnSettings();

	private SnChannel mSound;

	public AudioClip GetSoundClip(AIActor Actor)
	{
		if ((bool)_AudioClip)
		{
			return _AudioClip;
		}
		if (Actor._SoundMapper != null)
		{
			return Actor._SoundMapper.GetAudioClip(_SoundName);
		}
		return null;
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (mSound == null)
		{
			return SetState(AIBehaviorState.FAILED);
		}
		if (IsPlaying() && _WaitForSoundToComplete)
		{
			return SetState(AIBehaviorState.ACTIVE);
		}
		return SetState(AIBehaviorState.COMPLETED);
	}

	public override void OnStart(AIActor Actor)
	{
		base.OnStart(Actor);
		mSound = null;
		AudioClip soundClip = GetSoundClip(Actor);
		if (soundClip != null && (!UtPlatform.IsMobile() || _PlayOnMobile))
		{
			mSound = Actor.GetComponent<SnChannel>();
			mSound.pClip = soundClip;
			mSound.Play(_Settings);
		}
		if (mSound == null)
		{
			SetState(AIBehaviorState.FAILED);
		}
		else if (!_WaitForSoundToComplete)
		{
			SetState(AIBehaviorState.COMPLETED);
		}
		else
		{
			SetState(AIBehaviorState.ACTIVE);
		}
	}

	public override void OnTerminate(AIActor Actor)
	{
		base.OnTerminate(Actor);
		if (_StopSoundOnTerminate && mSound != null)
		{
			mSound.Stop();
		}
		SetState(AIBehaviorState.COMPLETED);
		mSound = null;
	}

	public bool IsPlaying()
	{
		if (mSound != null)
		{
			return mSound.pIsPlaying;
		}
		return false;
	}
}
