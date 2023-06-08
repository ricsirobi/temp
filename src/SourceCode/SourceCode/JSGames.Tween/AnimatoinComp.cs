using System;
using UnityEngine;

namespace JSGames.Tween;

public class AnimatoinComp : Tweener
{
	private Animation mAnim;

	private Animator mAnimator;

	private string mAnimName;

	public AnimatoinComp(GameObject tweenObject, Animation anim, string animName)
	{
		base.pTweenObject = tweenObject;
		mAnimName = animName;
		mAnim = anim;
		mAnim.Play(mAnimName);
	}

	protected override void DoAnim(float val)
	{
		mValue = mAnim[mAnimName].normalizedTime;
	}

	protected override void DoAnimReverse(float val)
	{
		throw new NotImplementedException();
	}
}
