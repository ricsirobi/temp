using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

[RequireComponent(typeof(ScrollRect))]
public class AutoHideUIScrollbar : MonoBehaviour
{
	private class ScrollbarClass
	{
		public Scrollbar bar;

		public bool active;
	}

	public bool alsoDisableScrolling;

	private float disableRange = 0.99f;

	private ScrollRect scrollRect;

	private ScrollbarClass scrollbarVertical;

	private ScrollbarClass scrollbarHorizontal;

	private void Start()
	{
		scrollRect = base.gameObject.GetComponent<ScrollRect>();
		if (scrollRect.verticalScrollbar != null)
		{
			scrollbarVertical = new ScrollbarClass
			{
				bar = scrollRect.verticalScrollbar,
				active = true
			};
		}
		if (scrollRect.horizontalScrollbar != null)
		{
			scrollbarHorizontal = new ScrollbarClass
			{
				bar = scrollRect.horizontalScrollbar,
				active = true
			};
		}
		if (scrollbarVertical == null && scrollbarHorizontal == null)
		{
			Debug.LogWarning("Must have a horizontal or vertical scrollbar attached to the Scroll Rect for AutoHideUIScrollbar to work");
		}
	}

	private void Update()
	{
		if (scrollbarVertical != null)
		{
			SetScrollBar(scrollbarVertical, vertical: true);
		}
		if (scrollbarHorizontal != null)
		{
			SetScrollBar(scrollbarHorizontal, vertical: false);
		}
	}

	private void SetScrollBar(ScrollbarClass scrollbar, bool vertical)
	{
		if (scrollbar.active && scrollbar.bar.size > disableRange)
		{
			SetBar(scrollbar, active: false, vertical);
		}
		else if (!scrollbar.active && scrollbar.bar.size < disableRange)
		{
			SetBar(scrollbar, active: true, vertical);
		}
	}

	private void SetBar(ScrollbarClass scrollbar, bool active, bool vertical)
	{
		scrollbar.bar.gameObject.SetActive(active);
		scrollbar.active = active;
		if (alsoDisableScrolling)
		{
			if (vertical)
			{
				scrollRect.vertical = active;
			}
			else
			{
				scrollRect.horizontal = active;
			}
		}
	}
}
