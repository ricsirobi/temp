using UnityEngine;

namespace JSGames.Tween;

public abstract class TweenerV3 : Tweener
{
	protected Vector3 from;

	protected Vector3 to;

	public TweenerV3(GameObject tweenObject, Vector3 from, Vector3 to)
	{
		base.pTweenObject = tweenObject;
		this.from = from;
		this.to = to;
	}
}
