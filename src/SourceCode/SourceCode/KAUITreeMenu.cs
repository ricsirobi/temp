using AnimationOrTween;
using UnityEngine;

public class KAUITreeMenu : KAUIMenu
{
	public Vector3 _TweenScaleFrom = new Vector3(1f, 0.01f, 1f);

	public Vector3 _TweenScaleTo = new Vector3(1f, 1f, 1f);

	public float _TweenDuration = 0.5f;

	public bool _CollapseAll = true;

	public void AddWidget(KAWidget inWidget, KAWidget inParentWidget)
	{
		if (inParentWidget != null)
		{
			UIPlayTween uIPlayTween = inParentWidget.gameObject.AddComponent<UIPlayTween>();
			uIPlayTween.trigger = Trigger.OnClick;
			uIPlayTween.tweenTarget = inWidget.gameObject;
			uIPlayTween.playDirection = Direction.Toggle;
			uIPlayTween.includeChildren = true;
			TweenScale tweenScale = inWidget.gameObject.AddComponent<TweenScale>();
			tweenScale.from = _TweenScaleFrom;
			tweenScale.to = _TweenScaleTo;
			tweenScale.duration = _TweenDuration;
			if (_CollapseAll)
			{
				Vector3 localScale = inWidget.transform.localScale;
				localScale.y = _TweenScaleFrom.y;
				inWidget.transform.localScale = localScale;
				tweenScale.enabled = false;
			}
			tweenScale.updateTable = true;
			inParentWidget.AddChild(inWidget);
			int num = FindItemIndex(inParentWidget);
			if (num >= 0)
			{
				AddWidgetAt(num + 1, inWidget);
			}
			else
			{
				AddWidget(inWidget);
			}
		}
		else
		{
			AddWidget(inWidget);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		Play(inWidget);
	}

	protected void Play(KAWidget inWidget)
	{
		if (inWidget == null)
		{
			return;
		}
		foreach (KAWidget pChildWidget in inWidget.pChildWidgets)
		{
			Play(pChildWidget);
		}
		UIPlayTween[] componentsInChildren = inWidget.gameObject.GetComponentsInChildren<UIPlayTween>();
		if (componentsInChildren != null)
		{
			UIPlayTween[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play(forward: true);
			}
		}
	}
}
