using UnityEngine;

public class KAUIPages : MonoBehaviour
{
	public enum Transition
	{
		NORMAL,
		LINEAR,
		DRAG_OVER,
		ZOOM_IN,
		ZOOM_OUT
	}

	public enum Direction
	{
		VERTICAL = 1,
		HORIZONTAL,
		CENTER
	}

	public KAUI[] _Pages;

	public KAUI _StartPage;

	public bool _Circular;

	public Transition _TransitionType;

	public Direction _TransitionDirection = Direction.CENTER;

	public float _TransitionTime = 1f;

	public Vector2 _TransitionFromPosition = Vector2.zero;

	public float _MinSwipeForTransition = 100f;

	public KAButton _BtnNext;

	public KAButton _BtnPrevious;

	private Vector3[] mCachedPagePositions;

	private int mPreviousPageIndex;

	private int mCurrentPageIndex;

	private int mNextPageIndex;

	private int mTransitionOrder = 1;

	private bool mInitialized;

	private bool mInTransition;

	private Vector2 mDragLength = Vector2.zero;

	private bool mAnchorCheckDone;

	protected void Start()
	{
		if (_StartPage != null)
		{
			for (int i = 0; i < _Pages.Length; i++)
			{
				if (_StartPage == _Pages[i])
				{
					mCurrentPageIndex = i;
					break;
				}
			}
		}
		mCachedPagePositions = new Vector3[_Pages.Length];
		for (int j = 0; j < _Pages.Length; j++)
		{
			mCachedPagePositions[j] = _Pages[j].transform.position;
		}
	}

	private bool IsAnchorEnabled()
	{
		for (int i = 0; i < _Pages.Length; i++)
		{
			if (_Pages[i].GetComponentInChildren<UIAnchor>().enabled)
			{
				return true;
			}
		}
		return false;
	}

	private void InitPages()
	{
		for (int i = 0; i < _Pages.Length; i++)
		{
			if (i == mCurrentPageIndex)
			{
				continue;
			}
			if (_TransitionType == Transition.DRAG_OVER)
			{
				Vector3 position = mCachedPagePositions[mCurrentPageIndex];
				if (_TransitionDirection == Direction.VERTICAL)
				{
					position = new Vector3(position.x, _TransitionFromPosition.y, position.z);
				}
				else if (_TransitionDirection == Direction.HORIZONTAL)
				{
					position = new Vector3(_TransitionFromPosition.x, position.y, position.z);
				}
				_Pages[i].transform.position = position;
				_Pages[i].SetInteractive(interactive: false);
				_Pages[i].SetVisibility(inVisible: true);
			}
			else
			{
				_Pages[i].SetVisibility(inVisible: false);
			}
		}
		if (_TransitionType == Transition.DRAG_OVER)
		{
			int num = mCurrentPageIndex + 1;
			if (num < _Pages.Length)
			{
				_Pages[num].SetInteractive(interactive: true);
			}
		}
	}

	protected void Awake()
	{
	}

	protected void Update()
	{
		if (!mAnchorCheckDone && !IsAnchorEnabled())
		{
			InitPages();
			mAnchorCheckDone = true;
		}
		if (mInitialized)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < _Pages.Length; i++)
		{
			if (_Pages[i].pEvents == null)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			RegisterEvents();
			mInitialized = true;
		}
	}

	protected void RegisterEvents()
	{
		for (int i = 0; i < _Pages.Length; i++)
		{
			_Pages[i].pEvents.OnDrag += OnDrag;
			_Pages[i].pEvents.OnClick += OnClick;
			_Pages[i].pEvents.OnDrop += OnDrop;
			_Pages[i].pEvents.OnPress += OnPress;
		}
		KAUI component = base.gameObject.GetComponent<KAUI>();
		if (component != null && component.pEvents != null)
		{
			component.pEvents.OnDrag += OnDrag;
			component.pEvents.OnClick += OnClick;
			component.pEvents.OnDrop += OnDrop;
			component.pEvents.OnPress += OnPress;
		}
	}

	public void OnPress(KAWidget inWidget, bool inPressed)
	{
		mDragLength = Vector2.zero;
	}

	public void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		if (_TransitionType != Transition.LINEAR && _TransitionType != Transition.DRAG_OVER)
		{
			return;
		}
		mDragLength += inDelta;
		if (!(_MinSwipeForTransition < Mathf.Abs(mDragLength.x)) && !(_MinSwipeForTransition < Mathf.Abs(mDragLength.y)))
		{
			return;
		}
		if (Mathf.Abs(mDragLength.x) >= Mathf.Abs(mDragLength.y))
		{
			if (_TransitionDirection == Direction.HORIZONTAL)
			{
				if (mDragLength.x > 0f)
				{
					ShowNextPage(-1);
				}
				else
				{
					ShowNextPage(1);
				}
			}
		}
		else if (_TransitionDirection == Direction.VERTICAL)
		{
			if (mDragLength.y < 0f)
			{
				ShowNextPage(-1);
			}
			else
			{
				ShowNextPage(1);
			}
		}
	}

	public void OnDrop(KAWidget inDroppedWidget, KAWidget inTargetWidget)
	{
		mDragLength = Vector2.zero;
	}

	public void OnClick(KAWidget inWidget)
	{
		if (!(inWidget == null))
		{
			if (inWidget == _BtnNext)
			{
				ShowNextPage(-1);
			}
			else if (inWidget == _BtnPrevious)
			{
				ShowNextPage(1);
			}
		}
	}

	public void ShowNextPage(int order)
	{
		if (!mInTransition)
		{
			int num = mCurrentPageIndex + order;
			if (order < 0)
			{
				mTransitionOrder = -1;
			}
			else
			{
				mTransitionOrder = 1;
			}
			if (_Circular)
			{
				num = (num + _Pages.Length) % _Pages.Length;
			}
			if (num >= _Pages.Length || num < 0)
			{
				Debug.Log("Wrong page index " + num);
			}
			else
			{
				StartTransition(num);
			}
		}
	}

	public void StartTransition(int nextPageIndex)
	{
		mInTransition = true;
		mNextPageIndex = nextPageIndex;
		KAUI kAUI = _Pages[mCurrentPageIndex];
		KAUI kAUI2 = _Pages[nextPageIndex];
		switch (_TransitionType)
		{
		case Transition.NORMAL:
			kAUI.SetVisibility(inVisible: false);
			kAUI2.SetVisibility(inVisible: true);
			TransitionDone();
			break;
		case Transition.LINEAR:
		{
			kAUI.SetInteractive(interactive: false);
			Vector3 vector2 = mCachedPagePositions[mCurrentPageIndex];
			if (_TransitionDirection == Direction.VERTICAL)
			{
				SlideToPos(kAUI, new Vector2(vector2.x, (float)mTransitionOrder * _TransitionFromPosition.y), _TransitionTime, "OnSlideEndCurrentPage");
			}
			else if (_TransitionDirection == Direction.HORIZONTAL)
			{
				SlideToPos(kAUI, new Vector2((float)mTransitionOrder * _TransitionFromPosition.x, vector2.y), _TransitionTime, "OnSlideEndCurrentPage");
			}
			Vector3 vector = mCachedPagePositions[nextPageIndex];
			if (_TransitionDirection == Direction.VERTICAL)
			{
				kAUI2.transform.position = new Vector3(vector.x, (float)(-1 * mTransitionOrder) * _TransitionFromPosition.y, vector.z);
			}
			else if (_TransitionDirection == Direction.HORIZONTAL)
			{
				kAUI2.transform.position = new Vector3((float)(-1 * mTransitionOrder) * _TransitionFromPosition.x, vector.y, vector.z);
			}
			kAUI2.SetVisibility(inVisible: true);
			kAUI2.SetInteractive(interactive: false);
			SlideToPos(kAUI2, vector, _TransitionTime, "OnSlideEndNextPage");
			break;
		}
		case Transition.DRAG_OVER:
			kAUI.SetInteractive(interactive: false);
			if (nextPageIndex > mCurrentPageIndex)
			{
				Vector3 vector = mCachedPagePositions[nextPageIndex];
				if (_TransitionDirection == Direction.VERTICAL)
				{
					kAUI2.transform.position = new Vector3(vector.x, (float)mTransitionOrder * _TransitionFromPosition.y, vector.z);
				}
				else if (_TransitionDirection == Direction.HORIZONTAL)
				{
					kAUI2.transform.position = new Vector3((float)mTransitionOrder * _TransitionFromPosition.x, vector.y, vector.z);
				}
				kAUI2.SetVisibility(inVisible: true);
				kAUI2.SetInteractive(interactive: false);
				SlideToPos(kAUI2, vector, _TransitionTime, "OnSlideEndNextPageDragOver");
			}
			else
			{
				Vector3 vector = mCachedPagePositions[mCurrentPageIndex];
				if (_TransitionDirection == Direction.VERTICAL)
				{
					vector = new Vector3(vector.x, (float)(-1 * mTransitionOrder) * _TransitionFromPosition.y, vector.z);
				}
				else if (_TransitionDirection == Direction.HORIZONTAL)
				{
					vector = new Vector3((float)(-1 * mTransitionOrder) * _TransitionFromPosition.x, vector.y, vector.z);
				}
				kAUI2.SetInteractive(interactive: false);
				SlideToPos(kAUI, vector, _TransitionTime, "OnSlideEndNextPageDragOver");
			}
			break;
		}
	}

	public void SlideToPos(KAUI page, Vector2 end, float duration, string callBack)
	{
		TweenPosition obj = TweenPosition.Begin(pos: new Vector3(end.x, end.y, page.transform.localPosition.z), go: page.gameObject, duration: duration);
		obj.eventReceiver = base.gameObject;
		obj.callWhenFinished = callBack;
	}

	public void OnSlideEndCurrentPage()
	{
	}

	public void OnSlideEndNextPageDragOver()
	{
		_Pages[mCurrentPageIndex].SetInteractive(interactive: true);
		_Pages[mNextPageIndex].SetInteractive(interactive: true);
		TransitionDone();
	}

	public void OnSlideEndNextPage()
	{
		_Pages[mCurrentPageIndex].SetVisibility(inVisible: false);
		_Pages[mNextPageIndex].SetInteractive(interactive: true);
		TransitionDone();
	}

	private void TransitionDone()
	{
		mInTransition = false;
		mPreviousPageIndex = mCurrentPageIndex;
		mCurrentPageIndex = mNextPageIndex;
		mNextPageIndex = -1;
		if (mCurrentPageIndex > 0)
		{
			if ((bool)_BtnPrevious)
			{
				_BtnPrevious.SetVisibility(inVisible: true);
			}
		}
		else if ((bool)_BtnPrevious)
		{
			_BtnPrevious.SetVisibility(inVisible: false);
		}
		if (mCurrentPageIndex < _Pages.Length - 1)
		{
			if ((bool)_BtnNext)
			{
				_BtnNext.SetVisibility(inVisible: true);
			}
		}
		else if ((bool)_BtnNext)
		{
			_BtnNext.SetVisibility(inVisible: false);
		}
		_Pages[mCurrentPageIndex].gameObject.SendMessage("OnTransitionDone", true, SendMessageOptions.DontRequireReceiver);
		_Pages[mPreviousPageIndex].gameObject.SendMessage("OnTransitionDone", false, SendMessageOptions.DontRequireReceiver);
	}
}
