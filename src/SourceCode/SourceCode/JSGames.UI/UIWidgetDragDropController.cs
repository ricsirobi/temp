using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JSGames.UI;

public class UIWidgetDragDropController : MonoBehaviour
{
	public delegate void WidgetDropEventHandler(UIWidget droppedWidget, UIWidget backgroudWidget, Vector3 localPosition);

	public delegate void WidgetSelectEventHandler(UIWidget selectedWidget);

	public WidgetDropEventHandler OnWidgetDropped;

	public WidgetSelectEventHandler OnWidgetSelected;

	public Vector2 _Offset;

	public string _PlaceHolderTag;

	public string _DragableObjectTag;

	public AudioClip _PickupSFX;

	public bool _UpdateSiblingIndex;

	public bool _AllowUIBlock = true;

	private EventSystem mEventSystem;

	private int mPointerID = -1;

	private List<RaycastResult> mRaycastResults = new List<RaycastResult>();

	private Vector3 mPreviousLocalPosition;

	private int mPreviousSiblingIndex;

	public UIWidget pSelectedWidget { get; private set; }

	public int pPrevPointerID { get; private set; }

	public bool pUIBlocked
	{
		get
		{
			if (_AllowUIBlock)
			{
				return UI._GlobalExclusiveUI != null;
			}
			return false;
		}
	}

	public void OnAppFocus(bool focus)
	{
		OnAppFocusChanged(focus);
	}

	public void OnUserLogout()
	{
	}

	private void Start()
	{
		mEventSystem = Singleton<UIManager>.pInstance.GetComponent<EventSystem>();
		TouchManager.OnFingerUpEvent = (OnFingerUp)Delegate.Combine(TouchManager.OnFingerUpEvent, new OnFingerUp(OnFingerUp));
	}

	private void OnDestroy()
	{
		TouchManager.OnFingerUpEvent = (OnFingerUp)Delegate.Remove(TouchManager.OnFingerUpEvent, new OnFingerUp(OnFingerUp));
	}

	private void OnFingerUp(int fingerID, Vector2 position)
	{
		if (pSelectedWidget == null)
		{
			SelectDraggableWidget(fingerID, position);
		}
		else if (mPointerID == fingerID && pSelectedWidget != null)
		{
			position.y = (float)Screen.height - position.y;
			pPrevPointerID = mPointerID;
			DropWidget(position);
		}
	}

	private void OnAppFocusChanged(bool hasFocus)
	{
		if (pSelectedWidget != null && hasFocus)
		{
			DropWidget(RectTransformUtility.WorldToScreenPoint(pSelectedWidget.pParentUI.pCanvas.worldCamera, pSelectedWidget.pPosition));
		}
	}

	private void SelectDraggableWidget(int pointerID, Vector2 position)
	{
		if (pUIBlocked)
		{
			return;
		}
		position.y = (float)Screen.height - position.y;
		UIWidget selectedWidget = GetSelectedWidget(position);
		if (selectedWidget != null && selectedWidget.pState == WidgetState.INTERACTIVE && selectedWidget.CompareTag(_DragableObjectTag))
		{
			AttachWidgetToPointer(selectedWidget, pointerID);
			if (_PickupSFX != null)
			{
				SnChannel.Play(_PickupSFX, "SFX_Pool", inForce: true);
			}
		}
	}

	public void AttachWidgetToPointer(UIWidget widget, int pointerID)
	{
		if (!(widget == null) && !widget.pIsAttachedToPointer)
		{
			pSelectedWidget = widget;
			if (_UpdateSiblingIndex)
			{
				mPreviousSiblingIndex = pSelectedWidget.pRectTransform.GetSiblingIndex();
				pSelectedWidget.pRectTransform.SetAsLastSibling();
			}
			mPreviousLocalPosition = pSelectedWidget.pLocalPosition;
			pSelectedWidget.AttachToPointer(pointerID);
			mPointerID = pointerID;
			if (OnWidgetSelected != null)
			{
				OnWidgetSelected(pSelectedWidget);
			}
		}
	}

	public void DetachWidgetFromPointer(UIWidget background = null)
	{
		UIWidget droppedWidget = pSelectedWidget;
		if (pSelectedWidget != null)
		{
			pSelectedWidget.DetachFromPointer();
			if (_UpdateSiblingIndex)
			{
				pSelectedWidget.pRectTransform.SetSiblingIndex(mPreviousSiblingIndex);
			}
		}
		pSelectedWidget = null;
		mPointerID = -1;
		if (OnWidgetDropped != null)
		{
			OnWidgetDropped(droppedWidget, background, mPreviousLocalPosition);
		}
	}

	public void UpdateSelectedWidget(UIWidget widget)
	{
		if (pSelectedWidget != null)
		{
			pSelectedWidget.DetachFromPointer();
		}
		pSelectedWidget = widget;
		pSelectedWidget.AttachToPointer(_Offset, mPointerID);
	}

	private void DropWidget(Vector2 position)
	{
		UIWidget selectedWidget = GetSelectedWidget(position);
		DetachWidgetFromPointer(selectedWidget);
	}

	private UIWidget GetSelectedWidget(Vector2 position)
	{
		PointerEventData pointerEventData = new PointerEventData(mEventSystem);
		pointerEventData.position = position;
		mRaycastResults.Clear();
		mEventSystem.RaycastAll(pointerEventData, mRaycastResults);
		if (mRaycastResults.Count > 0)
		{
			return mRaycastResults[0].gameObject.GetComponent<UIWidget>();
		}
		return null;
	}

	private void Update()
	{
		if (pSelectedWidget != null && pUIBlocked && pSelectedWidget.pParentUI != UI._GlobalExclusiveUI)
		{
			DropWidget(RectTransformUtility.WorldToScreenPoint(pSelectedWidget.pParentUI.pCanvas.worldCamera, pSelectedWidget.pPosition));
		}
	}
}
