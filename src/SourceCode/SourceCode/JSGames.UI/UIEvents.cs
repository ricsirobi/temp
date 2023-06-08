using System;
using UnityEngine.EventSystems;

namespace JSGames.UI;

public class UIEvents
{
	public event Action<UIWidget, int> OnAnimEnd;

	public event Action<UIWidget, PointerEventData> OnDrag;

	public event Action<UIWidget, PointerEventData> OnDrop;

	public event Action<UIWidget, PointerEventData> OnBeginDrag;

	public event Action<UIWidget, PointerEventData> OnEndDrag;

	public event Action<UIWidget, PointerEventData> OnClick;

	public event Action<UIWidget, bool, PointerEventData> OnPress;

	public event Action<UIWidget> OnPressRepeated;

	public event Action<UIWidget, bool, PointerEventData> OnHover;

	public event Action<UIEditBox, string> OnEndEdit;

	public event Action<UIEditBox, string> OnValueChanged;

	public event Action<UIToggleButton, bool> OnCheckedChanged;

	public event Action<UIWidget, UI> OnSelected;

	public void TriggerOnAnimEnd(UIWidget widget, int animIndex)
	{
		if (this.OnAnimEnd != null)
		{
			this.OnAnimEnd(widget, animIndex);
		}
	}

	public void TriggerOnDrag(UIWidget widget, PointerEventData eventData)
	{
		if (this.OnDrag != null)
		{
			this.OnDrag(widget, eventData);
		}
	}

	public void TriggerOnDrop(UIWidget widget, PointerEventData eventData)
	{
		if (this.OnDrop != null)
		{
			this.OnDrop(widget, eventData);
		}
	}

	public void TriggerOnBeginDrag(UIWidget widget, PointerEventData eventData)
	{
		if (this.OnBeginDrag != null)
		{
			this.OnBeginDrag(widget, eventData);
		}
	}

	public void TriggerOnEndDrag(UIWidget widget, PointerEventData eventData)
	{
		if (this.OnEndDrag != null)
		{
			this.OnEndDrag(widget, eventData);
		}
	}

	public void TriggerOnClick(UIWidget widget, PointerEventData eventData)
	{
		if (this.OnClick != null)
		{
			this.OnClick(widget, eventData);
		}
	}

	public void TriggerOnPress(UIWidget widget, bool isPressed, PointerEventData eventData)
	{
		if (this.OnPress != null)
		{
			this.OnPress(widget, isPressed, eventData);
		}
	}

	public void TriggerOnPressRepeated(UIWidget widget)
	{
		if (this.OnPressRepeated != null)
		{
			this.OnPressRepeated(widget);
		}
	}

	public void TriggerOnHover(UIWidget widget, bool isHovering, PointerEventData eventData)
	{
		if (this.OnHover != null)
		{
			this.OnHover(widget, isHovering, eventData);
		}
	}

	public void TriggerOnEdit(UIEditBox editBox, string text)
	{
		if (this.OnEndEdit != null)
		{
			this.OnEndEdit(editBox, text);
		}
	}

	public void TriggerOnValueChanged(UIEditBox editBox, string text)
	{
		if (this.OnValueChanged != null)
		{
			this.OnValueChanged(editBox, text);
		}
	}

	public void TriggerOnCheckedChanged(UIToggleButton toggleButton, bool isChecked)
	{
		if (this.OnCheckedChanged != null)
		{
			this.OnCheckedChanged(toggleButton, isChecked);
		}
	}

	public void TriggerOnSelected(UIWidget widget, UI fromUI)
	{
		if (this.OnSelected != null)
		{
			this.OnSelected(widget, fromUI);
		}
	}
}
