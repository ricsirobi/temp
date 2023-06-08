using UnityEngine;

namespace JSGames.Tween;

public class MoveLocal2D : TweenerV2
{
	public MoveLocal2D(GameObject tweenObject, Vector2 from, Vector2 to)
		: base(tweenObject, from, to)
	{
	}

	protected override void DoAnim(float val)
	{
		base.pTweenObject.transform.SetLocalPosition(CustomLerp(from, to, val));
	}

	protected override void DoAnimReverse(float val)
	{
		base.pTweenObject.transform.SetLocalPosition(CustomLerp(to, from, val));
	}
}
