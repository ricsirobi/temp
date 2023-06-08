using UnityEngine;

namespace JSGames.Tween;

public abstract class TweenerV4 : Tweener
{
	protected Vector4 from;

	protected Vector4 to;

	public TweenerV4(GameObject tweenObject, Vector4 from, Vector4 to)
	{
		base.pTweenObject = tweenObject;
		this.from = from;
		this.to = to;
	}
}
