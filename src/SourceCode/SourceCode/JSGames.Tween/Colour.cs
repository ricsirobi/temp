using UnityEngine;

namespace JSGames.Tween;

public class Colour : TweenerV4
{
	private Renderer mRenderer;

	public Colour(GameObject tweenObject, Renderer renderer, Vector4 from, Vector4 to)
		: base(tweenObject, from, to)
	{
		mRenderer = renderer;
	}

	protected override void DoAnim(float val)
	{
		mRenderer.SetColor(CustomLerp(from, to, val));
	}

	protected override void DoAnimReverse(float val)
	{
		mRenderer.SetColor(CustomLerp(to, from, val));
	}
}
