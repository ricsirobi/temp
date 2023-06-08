using UnityEngine;

namespace JSGames.Tween;

public abstract class TweenerF : Tweener
{
	protected float from;

	protected float to;

	public TweenerF(GameObject tweenObject, float from, float to)
	{
		base.pTweenObject = tweenObject;
		this.from = from;
		this.to = to;
	}
}
