using UnityEngine;

namespace JSGames.Tween;

public class TextMeshColor : TweenerV4
{
	private TextMesh mTextMesh;

	public TextMeshColor(GameObject tweenObject, TextMesh textMesh, Vector4 from, Vector4 to)
		: base(tweenObject, from, to)
	{
		mTextMesh = textMesh;
	}

	protected override void DoAnim(float val)
	{
		mTextMesh.SetColor(CustomLerp(from, to, val));
	}

	protected override void DoAnimReverse(float val)
	{
		mTextMesh.SetColor(CustomLerp(to, from, val));
	}
}
