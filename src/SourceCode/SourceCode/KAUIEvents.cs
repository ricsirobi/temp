using System;
using UnityEngine;

public class KAUIEvents
{
	public event Action<KAWidget, bool> OnHover;

	public event Action<KAWidget, bool> OnPress;

	public event Action<KAWidget, bool> OnSelect;

	public event Action<KAWidget, bool> OnTooltip;

	public event Action<KAWidget, bool> OnPressRepeated;

	public event Action<KAWidget, Vector2> OnDrag;

	public event Action<KAWidget, float> OnScroll;

	public event Action<KAWidget, Vector2> OnSwipe;

	public event Action<KAWidget, KAWidget> OnDrop;

	public event Action<KAWidget, string> OnInput;

	public event Action<KAWidget> OnSubmit;

	public event Action<KAWidget> OnClick;

	public event Action<KAWidget> OnDragStart;

	public event Action<KAWidget> OnDragEnd;

	public event Action<KAWidget> OnDoubleClick;

	public void ProcessHoverEvent(KAWidget inWidget, bool inIsHover)
	{
		if (this.OnHover != null)
		{
			this.OnHover(inWidget, inIsHover);
		}
	}

	public void ProcessPressRepeatedEvent(KAWidget inWidget, bool inIsPressed)
	{
		if (this.OnPressRepeated != null)
		{
			this.OnPressRepeated(inWidget, inIsPressed);
		}
	}

	public void ProcessPressEvent(KAWidget inWidget, bool inIsPressed)
	{
		if (this.OnPress != null)
		{
			this.OnPress(inWidget, inIsPressed);
		}
	}

	public void ProcessSelectEvent(KAWidget inWidget, bool inSelected)
	{
		if (this.OnSelect != null)
		{
			this.OnSelect(inWidget, inSelected);
		}
	}

	public void ProcessToolTipEvent(KAWidget inWidget, bool inShowTooltip)
	{
		if (this.OnTooltip != null)
		{
			this.OnTooltip(inWidget, inShowTooltip);
		}
	}

	public void ProcessScrollEvent(KAWidget inWidget, float inVal)
	{
		if (this.OnScroll != null)
		{
			this.OnScroll(inWidget, inVal);
		}
	}

	public void ProcessDragEvent(KAWidget inWidget, Vector2 inVal)
	{
		if (this.OnDrag != null)
		{
			this.OnDrag(inWidget, inVal);
		}
	}

	public void ProcessDropEvent(KAWidget inDroppedWidget, KAWidget inTargetWidget)
	{
		if (this.OnDrop != null)
		{
			this.OnDrop(inDroppedWidget, inTargetWidget);
		}
	}

	public void ProcessDragStartEvent(KAWidget inWidget)
	{
		if (this.OnDragStart != null)
		{
			this.OnDragStart(inWidget);
		}
	}

	public void ProcessDragEndEvent(KAWidget inWidget)
	{
		if (this.OnDragEnd != null)
		{
			this.OnDragEnd(inWidget);
		}
	}

	public void ProcessInputEvent(KAWidget inWidget, string inText)
	{
		if (this.OnInput != null)
		{
			this.OnInput(inWidget, inText);
		}
	}

	public void ProcessClickEvent(KAWidget inWidget)
	{
		if (this.OnClick != null)
		{
			this.OnClick(inWidget);
		}
	}

	public void ProcessSubmitEvent(KAWidget inWidget)
	{
		if (this.OnSubmit != null)
		{
			this.OnSubmit(inWidget);
		}
	}

	public void ProcessDoubleClickEvent(KAWidget inWidget)
	{
		if (this.OnDoubleClick != null)
		{
			this.OnDoubleClick(inWidget);
		}
	}

	public void ProcessSwipeEvent(KAWidget inSwipedWidget, Vector2 inSwipedTotalDelta)
	{
		if (this.OnSwipe != null)
		{
			this.OnSwipe(inSwipedWidget, inSwipedTotalDelta);
		}
	}

	public void Clear()
	{
		this.OnHover = null;
		this.OnPressRepeated = null;
		this.OnPress = null;
		this.OnClick = null;
		this.OnTooltip = null;
		this.OnInput = null;
		this.OnSelect = null;
		this.OnDrag = null;
		this.OnSubmit = null;
		this.OnScroll = null;
		this.OnDoubleClick = null;
		this.OnDrop = null;
		this.OnSwipe = null;
		this.OnDragStart = null;
		this.OnDragEnd = null;
	}
}
