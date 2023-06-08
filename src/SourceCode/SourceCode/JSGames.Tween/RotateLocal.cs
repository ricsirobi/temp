using UnityEngine;

namespace JSGames.Tween;

public class RotateLocal : TweenerV3
{
	public RotateLocal(GameObject tweenObject, Vector3 from, Vector3 to)
		: base(tweenObject, from, to)
	{
	}

	protected override void DoAnim(float val)
	{
		base.pTweenObject.transform.SetLocalRotation(CustomLerp(from, to, val));
	}

	protected override void DoAnimReverse(float val)
	{
		base.pTweenObject.transform.SetLocalRotation(CustomLerp(to, from, val));
	}
}
