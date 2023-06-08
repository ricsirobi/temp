using JSGames.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JSGames.Console;

public class UIConsoleLauncher : JSGames.UI.UI
{
	public UIWidget _DragWidget;

	public UIWidget _LaunchConsoleButton;

	public UIButton _CloseButton;

	private RectTransform mRectTransform;

	private Vector2 mPointerDragStartPos;

	private Vector2 mInitialWindowPos;

	private bool mIsMovingWindow;

	private bool mDragStarted;

	protected override void Awake()
	{
		base.Awake();
		mRectTransform = (RectTransform)base.transform;
	}

	protected override void OnDrag(JSGames.UI.UIWidget widget, PointerEventData eventData)
	{
		base.OnDrag(widget, eventData);
		Vector2 vector = eventData.position - mPointerDragStartPos;
		vector.y /= base.transform.lossyScale.y;
		vector.x /= base.transform.lossyScale.x;
		if (mIsMovingWindow)
		{
			mRectTransform.localPosition = mInitialWindowPos + vector;
			mDragStarted = true;
		}
	}

	protected override void OnPress(JSGames.UI.UIWidget widget, bool isPressed, PointerEventData eventData)
	{
		base.OnPress(widget, isPressed, eventData);
		mPointerDragStartPos = eventData.position;
		if (widget != _CloseButton)
		{
			if (isPressed)
			{
				mDragStarted = false;
			}
			mIsMovingWindow = isPressed;
			mInitialWindowPos = mRectTransform.localPosition;
		}
	}

	protected override void OnClick(JSGames.UI.UIWidget widget, PointerEventData eventData)
	{
		base.OnClick(widget, eventData);
		if (widget == _LaunchConsoleButton)
		{
			if (!mDragStarted)
			{
				JSConsole.mInstance._UIConsole.pVisible = true;
				pVisible = false;
			}
		}
		else if (widget == _CloseButton)
		{
			JSConsole.pConsoleEnabled = false;
		}
	}
}
