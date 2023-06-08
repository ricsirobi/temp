using System;
using UnityEngine;

namespace JSGames.Tween;

public class Shake2D : Tweener
{
	private Vector2 mOriginalPosition;

	private float mShakeAmount;

	public Shake2D(GameObject tweenObject, float shakeAmount)
	{
		base.pTweenObject = tweenObject;
		mOriginalPosition = base.pTweenObject.transform.localPosition;
		mShakeAmount = shakeAmount;
	}

	protected override void DoAnim(float val)
	{
		base.pTweenObject.transform.SetLocalPosition(mOriginalPosition + UnityEngine.Random.insideUnitCircle * mShakeAmount);
		if (val == 1f)
		{
			base.pTweenObject.transform.localPosition = mOriginalPosition;
		}
	}

	protected override void DoAnimReverse(float val)
	{
		throw new NotImplementedException();
	}
}
