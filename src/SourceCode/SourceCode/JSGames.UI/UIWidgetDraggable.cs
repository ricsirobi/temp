using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JSGames.UI;

[RequireComponent(typeof(UIWidget))]
public class UIWidgetDraggable : MonoBehaviour, IDragHandler, IEventSystemHandler, IDropHandler, IBeginDragHandler, IEndDragHandler
{
	[NonSerialized]
	private UIWidget mWidget;

	private void Start()
	{
		mWidget = GetComponent<UIWidget>();
	}

	public bool IsPointerValid(PointerEventData eventData)
	{
		if (eventData.button != 0)
		{
			return false;
		}
		return true;
	}

	public virtual void OnDrag(PointerEventData eventData)
	{
		if (IsPointerValid(eventData) && !(mWidget == null))
		{
			if (mWidget.pInteractableInHierarchy && mWidget.pEventTarget != null)
			{
				mWidget.pEventTarget.TriggerOnDrag(mWidget, eventData);
			}
			mWidget.SetDragging(isDragging: true);
		}
	}

	public virtual void OnDrop(PointerEventData eventData)
	{
		if (IsPointerValid(eventData) && !(mWidget == null) && mWidget.pInteractableInHierarchy && mWidget.pEventTarget != null)
		{
			mWidget.pEventTarget.TriggerOnDrop(mWidget, eventData);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (IsPointerValid(eventData) && !(mWidget == null) && mWidget.pInteractableInHierarchy && mWidget.pEventTarget != null)
		{
			mWidget.pEventTarget.TriggerOnBeginDrag(mWidget, eventData);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (IsPointerValid(eventData) && !(mWidget == null))
		{
			if (mWidget.pInteractableInHierarchy && mWidget.pEventTarget != null)
			{
				mWidget.pEventTarget.TriggerOnEndDrag(mWidget, eventData);
			}
			mWidget.SetDragging(isDragging: false);
		}
	}
}
