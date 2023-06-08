using UnityEngine;

namespace JSGames.Tween;

public class Alpha : TweenerF
{
	private Renderer mRenderer;

	public Alpha(GameObject tweenObject, Renderer renderer, float from, float to)
		: base(tweenObject, from, to)
	{
		mRenderer = renderer;
	}

	protected override void DoAnim(float val)
	{
		mRenderer.SetAlpha(CustomLerp(from, to, val));
	}

	protected override void DoAnimReverse(float val)
	{
		mRenderer.SetAlpha(CustomLerp(to, from, val));
	}
}
