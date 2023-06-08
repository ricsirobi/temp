using UnityEngine;

namespace JSGames.Tween;

public class MoveLocal : TweenerV3
{
	public MoveLocal(GameObject tweenObject, Vector3 from, Vector3 to)
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
