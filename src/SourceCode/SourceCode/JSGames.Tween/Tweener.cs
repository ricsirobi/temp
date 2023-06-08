using UnityEngine;

namespace JSGames.Tween;

public abstract class Tweener
{
	public delegate void AnimationCompleteEvent(Tweener tweener);

	private OnAnimationCompleteCallback pOnAnimationCompleteCallback;

	protected float mValue;

	private AnimationCurve mAnimationCurve;

	private CustomCurve mCustomAnimationCurve;

	private int mLoopCount = 1;

	private int mCompletedLoopCount;

	private float mDeltaMove;

	private float mDelay;

	private float mDelayCounter;

	private bool mPingPong;

	private bool mDoInReverese;

	private bool mUseAnimationCurve;

	public GameObject pTweenObject { get; protected set; }

	public TweenState pState { get; set; }

	public static event AnimationCompleteEvent pOnAnimationCompleted;

	public void DoUpdate()
	{
		if (pTweenObject == null)
		{
			Tweener.pOnAnimationCompleted(this);
		}
		else
		{
			if (pState == TweenState.PAUSE || pState == TweenState.DONE)
			{
				return;
			}
			if (mDelayCounter < mDelay)
			{
				mDelayCounter += Time.deltaTime;
				return;
			}
			mValue = Mathf.MoveTowards(mValue, 1f, Time.deltaTime * mDeltaMove);
			float val = mValue;
			if (mUseAnimationCurve && mAnimationCurve != null)
			{
				val = mAnimationCurve.Evaluate(mValue);
			}
			else if (mCustomAnimationCurve != null)
			{
				val = mCustomAnimationCurve(0f, 1f, mValue);
			}
			if (!mDoInReverese)
			{
				DoAnim(val);
			}
			else
			{
				DoAnimReverse(val);
			}
			if (!(mValue >= 1f))
			{
				return;
			}
			mDoInReverese = (mPingPong ? (!mDoInReverese) : mDoInReverese);
			mCompletedLoopCount++;
			if (mCompletedLoopCount == ((!mPingPong) ? mLoopCount : ((mLoopCount != 1) ? (2 * mLoopCount) : 2)))
			{
				pState = TweenState.DONE;
				Tweener.pOnAnimationCompleted(this);
				if (pOnAnimationCompleteCallback != null)
				{
					pOnAnimationCompleteCallback(null);
				}
			}
			mValue = 0f;
		}
	}

	public void SetData(float deltaMove, float delay, int loopCount, bool pingPong, bool useAnimationCurve, AnimationCurve animationCurve, CustomCurve customAnimationCurve, OnAnimationCompleteCallback onAnimationCompleteCallback)
	{
		mDeltaMove = deltaMove;
		mDelay = delay;
		mLoopCount = ((loopCount == 0) ? 1 : loopCount);
		mPingPong = pingPong;
		mAnimationCurve = animationCurve;
		mCustomAnimationCurve = customAnimationCurve;
		pState = TweenState.RUNNING;
		useAnimationCurve = mUseAnimationCurve;
		pOnAnimationCompleteCallback = onAnimationCompleteCallback;
	}

	protected float CustomLerp(float start, float end, float value)
	{
		return (1f - value) * start + value * end;
	}

	protected Vector2 CustomLerp(Vector2 start, Vector2 end, float value)
	{
		return new Vector2(CustomLerp(start.x, end.x, value), CustomLerp(start.y, end.y, value));
	}

	protected Vector3 CustomLerp(Vector3 start, Vector3 end, float value)
	{
		return new Vector3(CustomLerp(start.x, end.x, value), CustomLerp(start.y, end.y, value), CustomLerp(start.z, end.z, value));
	}

	protected Vector4 CustomLerp(Vector4 start, Vector4 end, float value)
	{
		return new Vector4(CustomLerp(start.x, end.x, value), CustomLerp(start.y, end.y, value), CustomLerp(start.z, end.z, value), CustomLerp(start.w, end.w, value));
	}

	protected abstract void DoAnim(float val);

	protected abstract void DoAnimReverse(float val);
}
