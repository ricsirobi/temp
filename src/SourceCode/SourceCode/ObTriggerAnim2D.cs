using System.Collections;
using UnityEngine;

public class ObTriggerAnim2D : ObTrigger
{
	public enum ANIM_TYPE
	{
		SimpleFlash,
		CustomAnimation,
		KAAnim2D
	}

	public KAWidget _Widget;

	public string _EnterAnim;

	public string _ExitAnim;

	public float _Delay;

	public float _ReturnFade = 0.3f;

	public WrapMode _WrapMode = WrapMode.Loop;

	public float _FlashRate = 0.25f;

	public ANIM_TYPE _AnimType = ANIM_TYPE.CustomAnimation;

	private bool mCachedWidgetVisibility;

	protected Animation mAnimation;

	private IEnumerator mPlayAnimCoroutine;

	private IEnumerator mStopAnimCoroutine;

	public virtual void Awake()
	{
		if (!(_Widget == null) && _AnimType == ANIM_TYPE.CustomAnimation)
		{
			mAnimation = _Widget.animation;
			if (mAnimation == null)
			{
				UtDebug.LogError("No animation component on " + _Widget.name + "! Falling back to simple flash animation.");
				_AnimType = ANIM_TYPE.SimpleFlash;
			}
			else if (mAnimation != null)
			{
				ReportIsClipInAnimation(_EnterAnim);
				ReportIsClipInAnimation(_ExitAnim);
			}
			else
			{
				mAnimation.Stop();
			}
		}
	}

	private void ReportIsClipInAnimation(string inAnimation)
	{
		if (!mAnimation.GetClip(inAnimation))
		{
			UtDebug.LogError("A clip named " + inAnimation + " is not present in the animator for " + _Widget.name + "!");
		}
	}

	public override void DoTriggerAction(GameObject other)
	{
		base.DoTriggerAction(other);
		if (mStopAnimCoroutine != null)
		{
			StopCoroutine(mStopAnimCoroutine);
		}
		mPlayAnimCoroutine = PlayAnimation();
		StartCoroutine(mPlayAnimCoroutine);
	}

	public override void DoTriggerExit()
	{
		base.DoTriggerExit();
		if (mPlayAnimCoroutine != null)
		{
			StopCoroutine(mPlayAnimCoroutine);
		}
		mStopAnimCoroutine = StopAnimation();
		StartCoroutine(mStopAnimCoroutine);
	}

	private IEnumerator PlayAnimation()
	{
		if ((bool)_Widget)
		{
			if (_AnimType == ANIM_TYPE.CustomAnimation && mAnimation != null && !string.IsNullOrEmpty(_EnterAnim) && !mAnimation.IsPlaying(_EnterAnim))
			{
				yield return new WaitForSeconds(_Delay);
				mAnimation.CrossFade(_EnterAnim);
				mAnimation[_EnterAnim].wrapMode = _WrapMode;
			}
			else if (_AnimType == ANIM_TYPE.SimpleFlash)
			{
				mCachedWidgetVisibility = _Widget.GetVisibility();
				InvokeRepeating("FlashWidget", _FlashRate, _FlashRate);
			}
			else if (_AnimType == ANIM_TYPE.KAAnim2D && !string.IsNullOrEmpty(_EnterAnim))
			{
				_Widget.PlayAnim(_EnterAnim);
			}
			yield return null;
		}
	}

	private IEnumerator StopAnimation()
	{
		if (!_Widget)
		{
			yield break;
		}
		if (mAnimation != null && !string.IsNullOrEmpty(_ExitAnim) && !mAnimation.IsPlaying(_ExitAnim))
		{
			yield return new WaitForSeconds(_Delay);
			mAnimation.CrossFade(_ExitAnim);
			mAnimation[_ExitAnim].wrapMode = _WrapMode;
		}
		else if (mAnimation != null && string.IsNullOrEmpty(_ExitAnim) && _AnimType == ANIM_TYPE.CustomAnimation)
		{
			mAnimation.Stop();
		}
		else if (_AnimType == ANIM_TYPE.SimpleFlash)
		{
			CancelInvoke("FlashWidget");
			_Widget.SetVisibility(mCachedWidgetVisibility);
		}
		else if (_AnimType == ANIM_TYPE.KAAnim2D)
		{
			if (!string.IsNullOrEmpty(_ExitAnim))
			{
				_Widget.PlayAnim(_ExitAnim);
			}
			else
			{
				_Widget.StopAnim();
			}
		}
		yield return null;
	}

	private void FlashWidget()
	{
		if (_Widget != null)
		{
			_Widget.SetVisibility(!_Widget.GetVisibility());
		}
	}
}
