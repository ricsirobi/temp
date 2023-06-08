using UnityEngine;

namespace JSGames.Tween;

public class Scale : TweenerV3
{
	public Scale(GameObject tweenObject, Vector3 from, Vector3 to)
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
