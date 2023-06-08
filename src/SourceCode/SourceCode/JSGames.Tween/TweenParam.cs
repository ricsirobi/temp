using System;
using UnityEngine;

namespace JSGames.Tween;

[Serializable]
public class TweenParam
{
	public float _DurationOrSpeed = 1f;

	public float _Delay;

	public int _LoopCount = 1;

	public bool _PingPong;

	public EaseType _Type;

	public bool _UseAnimationCurve;

	public AnimationCurve _AnimationCurve;

	public CustomCurve pCustomAnimationCurve => Tween.GetCustomCurve(_Type);

	public OnAnimationCompleteCallback pOnTweenCompleteCallback { get; set; }

	private TweenParam()
	{
	}

	public TweenParam(float durationOrSpeed, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, 0f, 1, pingPong: false, EaseType.Linear, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, EaseType type, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, 0f, 1, pingPong: false, type, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, float delay, EaseType type, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, delay, 1, pingPong: false, type, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, int loopCount, EaseType type, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, 0f, loopCount, pingPong: false, type, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, float delay, int loopCount, EaseType type, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, delay, loopCount, pingPong: false, type, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, bool pingPong, EaseType type, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, 0f, 1, pingPong, type, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, float delay, bool pingPong, EaseType type, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, delay, 1, pingPong, type, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, int loopCount, bool pingPong, EaseType type, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, 0f, loopCount, pingPong, type, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, AnimationCurve animationCurve, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, 0f, animationCurve, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, float delay, AnimationCurve animationCurve, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, delay, 1, pingPong: false, animationCurve, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, int loopCount, AnimationCurve animationCurve, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, 0f, loopCount, pingPong: false, animationCurve, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, float delay, int loopCount, AnimationCurve animationCurve, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, delay, loopCount, pingPong: false, animationCurve, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, bool pingPong, AnimationCurve animationCurve, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, 0f, 1, pingPong, animationCurve, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, float delay, bool pingPong, AnimationCurve animationCurve, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, delay, 1, pingPong, animationCurve, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, int loopCount, bool pingPong, AnimationCurve animationCurve, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		: this(durationOrSpeed, 0f, loopCount, pingPong, animationCurve, onAnimationCompleteCallback)
	{
	}

	public TweenParam(float durationOrSpeed, float delay, int loopCount, bool pingPong, EaseType type, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
	{
		_DurationOrSpeed = durationOrSpeed;
		_Delay = delay;
		_LoopCount = loopCount;
		_PingPong = pingPong;
		_Type = type;
		_AnimationCurve = null;
		_UseAnimationCurve = false;
		pOnTweenCompleteCallback = onAnimationCompleteCallback;
	}

	public TweenParam(float durationOrSpeed, float delay, int loopCount, bool pingPong, AnimationCurve animationCurve, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
	{
		_DurationOrSpeed = durationOrSpeed;
		_Delay = delay;
		_LoopCount = loopCount;
		_PingPong = pingPong;
		_Type = EaseType.Linear;
		_UseAnimationCurve = true;
		_AnimationCurve = animationCurve;
		pOnTweenCompleteCallback = onAnimationCompleteCallback;
	}
}
