using System.Collections.Generic;

namespace JSGames.Tween;

public class TweenUpdater : Singleton<TweenUpdater>
{
	private List<Tweener> mTweeners = new List<Tweener>();

	public static List<Tweener> pTweeners => Singleton<TweenUpdater>.pInstance.mTweeners;

	protected override void Awake()
	{
		base.Awake();
		Tweener.pOnAnimationCompleted += OnAnimDone;
	}

	private void Update()
	{
		for (int i = 0; i < mTweeners.Count; i++)
		{
			mTweeners[i].DoUpdate();
		}
	}

	private void OnAnimDone(Tweener tweener)
	{
		mTweeners.Remove(tweener);
	}
}
