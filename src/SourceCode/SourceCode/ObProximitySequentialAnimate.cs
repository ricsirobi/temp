using System;
using UnityEngine;

public class ObProximitySequentialAnimate : KAMonoBase
{
	[Serializable]
	public class ObAnimationMap
	{
		public AnimationClip _Animation;

		public WrapMode _WrapMode = WrapMode.Loop;

		public bool _PlayReverse;

		public AudioClip[] _Sounds;

		public float _Range;

		[Tooltip("Set this value to true if checking for out of range")]
		public bool _IsForOutRange;

		public Vector3 _Offset;

		public bool _SoundFinish = true;

		public string _AnimationType = "IDLE";

		public string _NextAnimationType = "IN";

		public float _CrossFade = 0.1f;
	}

	public bool _Draw;

	public ObAnimationMap[] _AnimationMappings;

	public SnSound _Sound;

	private ObAnimationMap mCurrentAnimation;

	private float mLastUpdateTime;

	private string mNextAnimationType;

	private SnChannel mChannel;

	public virtual void Awake()
	{
		if (_AnimationMappings != null && _AnimationMappings.Length != 0)
		{
			mCurrentAnimation = _AnimationMappings[0];
			if (!base.animation.IsPlaying(mCurrentAnimation._Animation.name))
			{
				base.animation.Play(mCurrentAnimation._Animation.name);
			}
			mNextAnimationType = mCurrentAnimation._NextAnimationType;
		}
	}

	public virtual void Update()
	{
		if (base.animation == null || AvAvatar.pObject == null || mCurrentAnimation == null || string.IsNullOrEmpty(mNextAnimationType))
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup - mLastUpdateTime < 0.5f)
		{
			return;
		}
		mLastUpdateTime = realtimeSinceStartup + UnityEngine.Random.value * 0.1f;
		if (!CheckForNextAnimation())
		{
			return;
		}
		ObAnimationMap obAnimationMap = Array.Find(_AnimationMappings, (ObAnimationMap anim) => anim._AnimationType == mNextAnimationType);
		if (obAnimationMap != null)
		{
			if (obAnimationMap._Sounds != null && obAnimationMap._Sounds.Length != 0)
			{
				_Sound._AudioClip = obAnimationMap._Sounds[UnityEngine.Random.Range(0, obAnimationMap._Sounds.Length)];
			}
			if (_Sound._AudioClip != null)
			{
				mChannel.pLoop = !obAnimationMap._SoundFinish;
				mChannel = _Sound.Play();
			}
			if (obAnimationMap._PlayReverse)
			{
				base.animation[obAnimationMap._Animation.name].normalizedTime = 1f;
				base.animation[obAnimationMap._Animation.name].speed = -1f;
			}
			else
			{
				base.animation[obAnimationMap._Animation.name].speed = 1f;
			}
			base.animation[obAnimationMap._Animation.name].wrapMode = obAnimationMap._WrapMode;
			base.animation.CrossFade(obAnimationMap._Animation.name, obAnimationMap._CrossFade);
			mNextAnimationType = obAnimationMap._NextAnimationType;
			mCurrentAnimation = obAnimationMap;
		}
		else
		{
			mNextAnimationType = null;
		}
	}

	private bool CheckForNextAnimation()
	{
		if (mCurrentAnimation._WrapMode == WrapMode.Loop || mCurrentAnimation._WrapMode == WrapMode.PingPong)
		{
			if (CheckRange(GetOffset(mCurrentAnimation._Offset).magnitude, mCurrentAnimation))
			{
				return false;
			}
		}
		else if (base.animation.IsPlaying(mCurrentAnimation._Animation.name))
		{
			return false;
		}
		return true;
	}

	private bool CheckRange(float inPlayerRange, ObAnimationMap inMap)
	{
		if (inMap._IsForOutRange)
		{
			if (inPlayerRange > inMap._Range)
			{
				return true;
			}
		}
		else if (inPlayerRange < inMap._Range)
		{
			return true;
		}
		return false;
	}

	private void OnDrawGizmos()
	{
		if (!_Draw)
		{
			return;
		}
		ObAnimationMap[] animationMappings = _AnimationMappings;
		foreach (ObAnimationMap obAnimationMap in animationMappings)
		{
			if (obAnimationMap._Range != 0f)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(base.transform.position + base.transform.TransformDirection(obAnimationMap._Offset), obAnimationMap._Range);
			}
		}
	}

	protected Vector3 GetOffset(Vector3 inOffset)
	{
		return AvAvatar.position - (base.transform.position + base.transform.TransformDirection(inOffset));
	}
}
