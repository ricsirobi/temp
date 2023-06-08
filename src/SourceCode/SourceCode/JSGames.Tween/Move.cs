using UnityEngine;

namespace JSGames.Tween;

public class Move : TweenerV3
{
	private RectTransform mRectTransform;

	public Move(GameObject tweenObject, Vector3 from, Vector3 to)
		: base(tweenObject, from, to)
	{
	}

	public Move(RectTransform rectTransform, Vector3 from, Vector3 to)
		: base(rectTransform.gameObject, from, to)
	{
		mRectTransform = rectTransform;
	}

	protected override void DoAnim(float val)
	{
		if (mRectTransform != null)
		{
			mRectTransform.SetPosition(CustomLerp(from, to, val));
		}
		else
		{
			base.pTweenObject.transform.SetPosition(CustomLerp(from, to, val));
		}
	}

	protected override void DoAnimReverse(float val)
	{
		if (mRectTransform != null)
		{
			mRectTransform.SetPosition(CustomLerp(to, from, val));
		}
		else
		{
			base.pTweenObject.transform.SetPosition(CustomLerp(to, from, val));
		}
	}
}
