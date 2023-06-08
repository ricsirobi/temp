using UnityEngine;

public class UiIncredibleMachinesInventory : KAUI
{
	public Vector2 _SlideOffset = new Vector3(100f, 0f);

	private Vector2 mStartPos;

	public float _TransitionTime = 0.3f;

	private bool mIsOpen;

	private bool mInitialized;

	protected override void Start()
	{
		base.Start();
		mStartPos = new Vector2(base.transform.localPosition.x, base.transform.localPosition.y);
		SetVisibility(inVisible: false);
	}

	protected override void Update()
	{
		base.Update();
		if (!mInitialized)
		{
			mInitialized = true;
			base.transform.localPosition = new Vector3(mStartPos.x + _SlideOffset.x, mStartPos.y + _SlideOffset.y, base.transform.localScale.z);
		}
	}

	public void ToggleVisibility()
	{
		if (mIsOpen)
		{
			CloseUI();
		}
		else
		{
			OpenUI();
		}
	}

	public void OpenUI()
	{
		mIsOpen = true;
		SetInteractive(interactive: false);
		SetVisibility(inVisible: true);
		SlideToPos(mStartPos, _TransitionTime, "OnOpenDone");
	}

	public void CloseUI()
	{
		mIsOpen = false;
		SetInteractive(interactive: false);
		SlideToPos(mStartPos + _SlideOffset, _TransitionTime, "OnCloseDone");
	}

	public void SlideToPos(Vector2 end, float duration, string callBack)
	{
		TweenPosition obj = TweenPosition.Begin(pos: new Vector3(end.x, end.y, base.transform.localPosition.z), go: base.gameObject, duration: duration);
		obj.eventReceiver = base.gameObject;
		obj.callWhenFinished = callBack;
	}

	public void OnOpenDone()
	{
		SetInteractive(interactive: true);
	}

	public void OnCloseDone()
	{
		SetVisibility(inVisible: false);
	}
}
