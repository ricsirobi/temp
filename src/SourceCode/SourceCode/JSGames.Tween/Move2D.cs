using UnityEngine;

namespace JSGames.Tween;

public class Move2D : TweenerV2
{
	private RectTransform mRectTransform;

	public Move2D(GameObject tweenObject, Vector2 from, Vector2 to)
		: base(tweenObject, from, to)
	{
	}

	public Move2D(RectTransform rectTransform, Vector2 from, Vector2 to)
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
