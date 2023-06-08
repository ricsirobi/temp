using System;
using UnityEngine;

namespace JSGames.Tween;

public class Timer : Tweener
{
	public Timer()
	{
		GameObject gameObject = new GameObject("Timer");
		base.pTweenObject = gameObject;
	}

	protected override void DoAnim(float val)
	{
		if (val >= 1f)
		{
			UnityEngine.Object.Destroy(base.pTweenObject);
		}
	}

	protected override void DoAnimReverse(float val)
	{
		throw new NotImplementedException();
	}
}
