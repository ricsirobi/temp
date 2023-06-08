using UnityEngine;
using UnityEngine.UI;

namespace JSGames.Tween;

public class Count : TweenerF
{
	private Text mUIText;

	private TextMesh mTextMesh;

	public Count(GameObject tweenObject, Text uiText, float from, float to)
		: base(tweenObject, from, to)
	{
		mUIText = uiText;
	}

	public Count(GameObject tweenObject, TextMesh textMesh, float from, float to)
		: base(tweenObject, from, to)
	{
		mTextMesh = textMesh;
	}

	protected override void DoAnim(float val)
	{
		int num = (int)CustomLerp(from, to, val);
		if (mUIText != null)
		{
			mUIText.SetText(num.ToString());
		}
		else if (mTextMesh != null)
		{
			mTextMesh.SetText(num.ToString());
		}
	}

	protected override void DoAnimReverse(float val)
	{
		int num = (int)CustomLerp(from, to, val);
		if (mUIText != null)
		{
			mUIText.SetText(num.ToString());
		}
		else if (mTextMesh != null)
		{
			mTextMesh.SetText(num.ToString());
		}
	}
}
