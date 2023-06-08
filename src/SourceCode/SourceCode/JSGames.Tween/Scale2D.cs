using UnityEngine;

namespace JSGames.Tween;

public class Scale2D : TweenerV2
{
	public Scale2D(GameObject tweenObject, Vector2 from, Vector2 to)
		: base(tweenObject, from, to)
	{
	}

	protected override void DoAnim(float val)
	{
		base.pTweenObject.transform.SetLocalScale(CustomLerp(from, to, val));
	}

	protected override void DoAnimReverse(float val)
	{
		base.pTweenObject.transform.SetLocalScale(CustomLerp(to, from, val));
	}
}
