using UnityEngine;

namespace JSGames.Tween;

public abstract class TweenerV2 : Tweener
{
	protected Vector2 from;

	protected Vector2 to;

	public TweenerV2(GameObject tweenObject, Vector2 from, Vector2 to)
	{
		base.pTweenObject = tweenObject;
		this.from = from;
		this.to = to;
	}
}
