using UnityEngine;
using UnityEngine.UI;

namespace JSGames.Tween;

public class GraphicColor : TweenerV4
{
	private Graphic mUIGraphic;

	public GraphicColor(GameObject tweenObject, Graphic uiGraphic, Vector4 from, Vector4 to)
		: base(tweenObject, from, to)
	{
		mUIGraphic = uiGraphic;
	}

	protected override void DoAnim(float val)
	{
		mUIGraphic.SetColor(CustomLerp(from, to, val));
	}

	protected override void DoAnimReverse(float val)
	{
		mUIGraphic.SetColor(CustomLerp(to, from, val));
	}
}
