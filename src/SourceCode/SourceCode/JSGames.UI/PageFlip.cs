using System;
using JSGames.Tween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JSGames.UI;

[RequireComponent(typeof(UIMenu))]
public class PageFlip : UIBehaviour, IEndDragHandler, IEventSystemHandler, IDragHandler
{
	public UIWidget _Previous;

	public UIWidget _Next;

	public float _SnapDuration = 0.5f;

	private bool mIsHorizontal;

	private RectTransform mContent;

	private Vector3 mContentInitialPosition;

	private Vector3 mContentExtremePosition;

	private GridLayoutGroup mGridLayoutGroup;

	private Vector2 mPageSize;

	private int mCurrentPage;

	private bool mWidgetsInitialised;

	private bool mInitialized;

	private UIEvents mEventReceiver = new UIEvents();

	public int pTotalPage { get; private set; }

	public void Initialize(ScrollRect scrollRect)
	{
		if (!mInitialized)
		{
			mInitialized = true;
			mEventReceiver.OnClick += OnWidgetClick;
		}
		mContent = scrollRect.content;
		mContentInitialPosition = mContent.localPosition;
		scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
		scrollRect.inertia = false;
		if (scrollRect.horizontal && scrollRect.vertical)
		{
			UtDebug.LogError("Page flip doesn't work as expected if scroll rect is movable in both horizontal and vertical");
		}
		mIsHorizontal = scrollRect.horizontal;
		mGridLayoutGroup = base.gameObject.GetComponentInChildren<GridLayoutGroup>();
		if (mGridLayoutGroup == null)
		{
			UtDebug.LogError("Cannot use page flip without a grid layout group!");
			return;
		}
		mPageSize = RectTransformUtility.PixelAdjustRect(scrollRect.viewport, GetComponent<Canvas>()).size;
		mCurrentPage = 1;
		UpdatePageCount();
	}

	protected override void Start()
	{
		base.Start();
		InitializeWidgets();
	}

	protected void InitializeWidgets()
	{
		UIMenu component = GetComponent<UIMenu>();
		if (_Next != null)
		{
			_Next.pEventTarget = mEventReceiver;
			_Next.Initialize(component, null);
		}
		if (_Previous != null)
		{
			_Previous.pEventTarget = mEventReceiver;
			_Previous.Initialize(component, null);
		}
		mWidgetsInitialised = true;
		component.OnMenuStateChanged = (Action<WidgetState>)Delegate.Combine(component.OnMenuStateChanged, new Action<WidgetState>(OnMenuStateChanged));
		component.OnVisibilityChanged = (Action<bool>)Delegate.Combine(component.OnVisibilityChanged, new Action<bool>(OnMenuVisibleChanged));
		UpdateArrowStatus();
	}

	private void OnWidgetClick(UIWidget widget, PointerEventData eventData)
	{
		if (widget == _Previous)
		{
			GoTo(mCurrentPage - 1);
		}
		else if (widget == _Next)
		{
			GoTo(mCurrentPage + 1);
		}
	}

	public void UpdatePageCount()
	{
		if (!(mGridLayoutGroup == null))
		{
			if (mContent.childCount > 0)
			{
				float unroundedPageCount = ((!mIsHorizontal) ? ((mGridLayoutGroup.cellSize.y + mGridLayoutGroup.spacing.y) * (float)mContent.childCount / (float)mGridLayoutGroup.constraintCount / mPageSize.y) : ((mGridLayoutGroup.cellSize.x + mGridLayoutGroup.spacing.x) * (float)mContent.childCount / (float)mGridLayoutGroup.constraintCount / mPageSize.x));
				pTotalPage = RoundPageCount(unroundedPageCount);
			}
			else
			{
				pTotalPage = 1;
			}
			if (mIsHorizontal)
			{
				mContentExtremePosition = mContentInitialPosition - new Vector3(mPageSize.x * (float)(pTotalPage - 1), 0f, 0f);
			}
			else
			{
				mContentExtremePosition = mContentInitialPosition + new Vector3(0f, mPageSize.y * (float)(pTotalPage - 1), 0f);
			}
			GoTo(Mathf.Clamp(mCurrentPage, 1, pTotalPage));
			UpdateArrowStatus();
		}
	}

	private int RoundPageCount(float unroundedPageCount)
	{
		return (int)Mathf.Ceil(Mathf.Floor(unroundedPageCount * 10f) / 10f);
	}

	private void GoTo(int pageNumber)
	{
		Vector3 localPosition = mContent.localPosition;
		localPosition = ((!mIsHorizontal) ? (mContentInitialPosition + new Vector3(0f, mPageSize.y * (float)(pageNumber - 1), 0f)) : (mContentInitialPosition - new Vector3(mPageSize.x * (float)(pageNumber - 1), 0f, 0f)));
		TweenParam tweenParam = new TweenParam(_SnapDuration);
		JSGames.Tween.Tween.MoveLocalTo(mContent.gameObject, mContent.localPosition, localPosition, tweenParam);
		mCurrentPage = pageNumber;
		UpdateArrowStatus();
	}

	private void UpdateArrowStatus()
	{
		if (mWidgetsInitialised && !(_Next == null) && !(_Previous == null))
		{
			UIWidget next = _Next;
			bool pVisible = (_Previous.pVisible = pTotalPage > 1);
			next.pVisible = pVisible;
			_Next.pState = ((mCurrentPage < pTotalPage) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
			_Previous.pState = ((mCurrentPage > 1) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		Vector3 localPosition = mContent.transform.localPosition;
		if (mIsHorizontal)
		{
			localPosition.x = Mathf.Clamp(mContent.transform.localPosition.x, mContentExtremePosition.x, mContentInitialPosition.x);
		}
		else
		{
			localPosition.y = Mathf.Clamp(mContent.transform.localPosition.y, mContentExtremePosition.y, mContentInitialPosition.y);
		}
		mContent.transform.localPosition = localPosition;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		int num = 0;
		num = ((!mIsHorizontal) ? Mathf.RoundToInt((mContent.transform.localPosition.y - mContentInitialPosition.y) / mPageSize.y) : Mathf.RoundToInt((mContentInitialPosition.x - mContent.transform.localPosition.x) / mPageSize.x));
		num = Mathf.Clamp(num, 0, pTotalPage - 1);
		GoTo(num + 1);
	}

	public void OnMenuVisibleChanged(bool newVisible)
	{
		if (_Next != null)
		{
			_Next.OnParentSetVisible(newVisible);
		}
		if (_Previous != null)
		{
			_Previous.OnParentSetVisible(newVisible);
		}
	}

	public void OnMenuStateChanged(WidgetState newState)
	{
		if (_Next != null)
		{
			_Next.OnParentSetState(newState);
		}
		if (_Previous != null)
		{
			_Previous.OnParentSetState(newState);
		}
	}
}
