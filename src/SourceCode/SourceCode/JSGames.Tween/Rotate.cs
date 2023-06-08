using UnityEngine;

namespace JSGames.Tween;

public class Rotate : TweenerV3
{
	public Rotate(GameObject tweenObject, Vector3 from, Vector3 to)
		: base(tweenObject, from, to)
	{
	}

	protected override void DoAnim(float val)
	{
		base.pTweenObject.transform.SetRotation(CustomLerp(from, to, val));
	}

	protected override void DoAnimReverse(float val)
	{
		base.pTweenObject.transform.SetRotation(CustomLerp(to, from, val));
	}
}
