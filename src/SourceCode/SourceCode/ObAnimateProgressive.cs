using System;
using UnityEngine;

public class ObAnimateProgressive : KAMonoBase
{
	[Serializable]
	public class StateConfig
	{
		public string _State;

		public float _ReturnFade = 0.3f;

		public AnimationClip _Animation;

		public WrapMode _WrapMode = WrapMode.Loop;

		public bool _Reverse;

		public SnSound _Sound;

		public AudioClip[] _Sounds;

		public bool _SoundFinish;

		private bool mStateOn;

		public bool pStateOn
		{
			get
			{
				return mStateOn;
			}
			set
			{
				mStateOn = value;
			}
		}
	}

	public StateConfig[] _StateConfig;

	private int mCurrentStateIndex = -1;

	private int mLastStateIndex = -1;

	private string mAnimToPlay = "";

	private bool mReverse;

	public void OnSwitchOn(string switchName)
	{
		int num = 0;
		StateConfig[] stateConfig = _StateConfig;
		foreach (StateConfig stateConfig2 in stateConfig)
		{
			if (!stateConfig2.pStateOn && stateConfig2._State.Equals(switchName))
			{
				stateConfig2.pStateOn = true;
				if (num > mCurrentStateIndex)
				{
					StartTransition();
				}
				break;
			}
			num++;
		}
	}

	public void OnSwitchOff(string switchName)
	{
		int num = 0;
		StateConfig[] stateConfig = _StateConfig;
		foreach (StateConfig stateConfig2 in stateConfig)
		{
			if (stateConfig2.pStateOn && stateConfig2._State.Equals(switchName))
			{
				stateConfig2.pStateOn = false;
				if (num <= mCurrentStateIndex)
				{
					StartTransition();
				}
				break;
			}
			num++;
		}
	}

	private void StartTransition()
	{
		mLastStateIndex = mCurrentStateIndex;
		mCurrentStateIndex = 0;
		for (int num = _StateConfig.Length - 1; num >= 0; num--)
		{
			if (_StateConfig[num].pStateOn)
			{
				mCurrentStateIndex = num;
				break;
			}
		}
		if (mLastStateIndex != mCurrentStateIndex)
		{
			if (mLastStateIndex < mCurrentStateIndex)
			{
				PlayNextState();
			}
			else if (mLastStateIndex > mCurrentStateIndex)
			{
				ReverseCurrentState();
			}
		}
	}

	private void PlayNextState()
	{
		StateConfig stateConfig = _StateConfig[mCurrentStateIndex];
		if (!base.animation.IsPlaying(stateConfig._Animation.name))
		{
			mAnimToPlay = stateConfig._Animation.name;
			AnimationState animationState = base.animation[mAnimToPlay];
			if (stateConfig._Sounds != null && stateConfig._Sounds.Length != 0)
			{
				stateConfig._Sound._AudioClip = stateConfig._Sounds[UnityEngine.Random.Range(0, stateConfig._Sounds.Length)];
			}
			if (stateConfig._Sound._AudioClip != null)
			{
				stateConfig._Sound.Play();
			}
			base.animation.CrossFade(mAnimToPlay);
			animationState.speed = 1f;
			animationState.wrapMode = stateConfig._WrapMode;
		}
	}

	private void PlayPreviousState()
	{
		StateConfig stateConfig = _StateConfig[mCurrentStateIndex];
		if (!base.animation.IsPlaying(stateConfig._Animation.name))
		{
			mAnimToPlay = stateConfig._Animation.name;
			AnimationState animationState = base.animation[mAnimToPlay];
			if (stateConfig._Sounds != null && stateConfig._Sounds.Length != 0)
			{
				stateConfig._Sound._AudioClip = stateConfig._Sounds[UnityEngine.Random.Range(0, stateConfig._Sounds.Length)];
			}
			if (stateConfig._Sound._AudioClip != null)
			{
				stateConfig._Sound.Play();
			}
			base.animation.CrossFade(mAnimToPlay, stateConfig._ReturnFade);
			if (stateConfig._WrapMode == WrapMode.ClampForever || stateConfig._WrapMode == WrapMode.Once)
			{
				animationState.normalizedTime = 1f;
			}
			animationState.speed = 1f;
			animationState.wrapMode = stateConfig._WrapMode;
		}
	}

	private void ReverseCurrentState()
	{
		StateConfig stateConfig = _StateConfig[mLastStateIndex];
		mAnimToPlay = stateConfig._Animation.name;
		if (stateConfig._Reverse && base.animation.IsPlaying(mAnimToPlay))
		{
			AnimationState animationState = base.animation[mAnimToPlay];
			animationState.speed = -1f;
			animationState.wrapMode = WrapMode.ClampForever;
			if (animationState.normalizedTime > 1f)
			{
				animationState.normalizedTime = 1f;
			}
			mReverse = true;
		}
		else
		{
			PlayPreviousState();
		}
	}

	public void Update()
	{
		if (mCurrentStateIndex == -1 && _StateConfig.Length != 0)
		{
			mCurrentStateIndex = (mLastStateIndex = 0);
			_StateConfig[mCurrentStateIndex].pStateOn = true;
			PlayNextState();
		}
		if (mReverse && base.animation[mAnimToPlay].time <= 0f)
		{
			mReverse = false;
			PlayPreviousState();
		}
	}
}
