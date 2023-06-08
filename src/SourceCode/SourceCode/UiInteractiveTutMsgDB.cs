using UnityEngine;

public class UiInteractiveTutMsgDB : KAUI
{
	public string _PageExitMsg = "OnGenericInfoBoxExit";

	public string _PageUpdateMsg = "PageUpdated";

	public string _YesBtnMsg = "OnGenericInfoBoxYes";

	public string _MsgBeforeClosing = "OnGenericInfoBoxExitInit";

	public Transform _Group;

	public string _OnNextBtnMsg = "OnNextButtonClicked";

	public string _OnBackBtnMsg = "OnBackButtonClicked";

	private GameObject mMessageObject;

	private KAWidget mNextBtn;

	private KAWidget mBackBtn;

	private KAWidget mDoneBtn;

	private KAWidget mYesBtn;

	private KAWidget mNoBtn;

	private KAWidget mCloseBtn;

	private KAWidget mIcon;

	private KAWidget mTitleText;

	private KAWidget mMsgText;

	private float mPositionX;

	private float mPositionY;

	private int mDelayFrames = -1;

	private bool mInitMessageBeforeExit;

	public bool pInitMessageBeforeExit
	{
		get
		{
			return mInitMessageBeforeExit;
		}
		set
		{
			mInitMessageBeforeExit = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	private void Initialize()
	{
		mNextBtn = FindItem("BtnHelpNext");
		mDoneBtn = FindItem("BtnHelpDone");
		mBackBtn = FindItem("BtnHelpBack");
		mYesBtn = FindItem("BtnYes");
		mNoBtn = FindItem("BtnNo");
		mCloseBtn = FindItem("BtnHelpClose");
		mIcon = FindItem("Icon");
		mTitleText = FindItem("TxtHelpHeader");
		mMsgText = FindItem("uiText");
	}

	public void SetText(GameObject inMessageObject, string inDefaultHeader, string inDefaultText)
	{
		if (mTitleText == null)
		{
			Initialize();
		}
		if (!string.IsNullOrEmpty(inDefaultText))
		{
			mMessageObject = inMessageObject;
			if (mTitleText != null)
			{
				mTitleText.SetText(inDefaultHeader);
				mTitleText.SetVisibility(inVisible: true);
			}
			if (mMsgText != null)
			{
				mMsgText.SetText(inDefaultText);
			}
		}
	}

	public void SetIconTexture(Texture textureInst)
	{
		if (mIcon != null)
		{
			mIcon.SetTexture(textureInst);
		}
	}

	public void SetButtonsActive(bool inBtnNext, bool inBtnBack, bool inBtnYes, bool inBtnNo, bool inBtnDone, bool inBtnClose, string iconName = null)
	{
		if (mNextBtn != null)
		{
			mNextBtn.SetVisibility(inBtnNext);
		}
		if (mBackBtn != null)
		{
			mBackBtn.SetVisibility(inBtnBack);
		}
		if (mYesBtn != null)
		{
			mYesBtn.SetVisibility(inBtnYes);
		}
		if (mNoBtn != null)
		{
			mNoBtn.SetVisibility(inBtnNo);
		}
		if (mDoneBtn != null)
		{
			mDoneBtn.SetVisibility(inBtnDone);
		}
		if (mCloseBtn != null)
		{
			mCloseBtn.SetVisibility(inBtnClose);
		}
		if (!string.IsNullOrEmpty(iconName))
		{
			KAWidget kAWidget = FindItem("NPCIcon");
			if (kAWidget != null)
			{
				kAWidget.PlayAnim(iconName);
			}
		}
	}

	public void Exit()
	{
		Show(inShow: false);
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage(_PageExitMsg);
		}
		Object.Destroy(base.gameObject);
	}

	public void SetInteractiveNextBackBtns(bool flag, bool nextBtn = true, bool backBtn = true)
	{
		if (mNextBtn != null && nextBtn)
		{
			mNextBtn.SetVisibility(flag);
		}
		if (mBackBtn != null && backBtn)
		{
			mBackBtn.SetVisibility(flag);
		}
	}

	private void OnYesBtnClick()
	{
		Show(inShow: false);
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage(_YesBtnMsg, null);
		}
		Object.Destroy(base.gameObject);
	}

	protected override void Update()
	{
		base.Update();
		if (mDelayFrames >= 0)
		{
			mDelayFrames--;
			if (mDelayFrames == 0)
			{
				ApplyPosition(mPositionX, mPositionY);
			}
		}
	}

	public override void OnClick(KAWidget widget)
	{
		base.OnClick(widget);
		if (widget == mCloseBtn)
		{
			if (!mInitMessageBeforeExit)
			{
				Exit();
			}
			else if (mMessageObject != null)
			{
				mMessageObject.SendMessage(_MsgBeforeClosing, SendMessageOptions.RequireReceiver);
			}
		}
		else if (widget == mDoneBtn)
		{
			Exit();
		}
		else if (widget == mYesBtn)
		{
			OnYesBtnClick();
		}
		else if (widget == mNoBtn)
		{
			Exit();
		}
		else if (widget == mBackBtn)
		{
			if (mMessageObject != null)
			{
				mMessageObject.SendMessage(_OnBackBtnMsg, SendMessageOptions.RequireReceiver);
			}
		}
		else if ((widget == mNextBtn || widget.name == "BtnOk") && mMessageObject != null)
		{
			mMessageObject.SendMessage(_OnNextBtnMsg, SendMessageOptions.RequireReceiver);
		}
	}

	public void Show(bool inShow)
	{
		SetVisibility(inShow);
	}

	public void SetPosition(float px, float py, int delayFrameCount = 0)
	{
		if (delayFrameCount == 0)
		{
			mDelayFrames = -1;
			ApplyPosition(px, py);
		}
		else
		{
			mDelayFrames = delayFrameCount;
			mPositionX = px;
			mPositionY = py;
		}
	}

	private void ApplyPosition(float px, float py)
	{
		if (_Group != null)
		{
			Vector3 localPosition = _Group.localPosition;
			localPosition.x = px;
			localPosition.y = py;
			_Group.localPosition = localPosition;
		}
		else
		{
			Vector3 position = base.transform.position;
			position.x = px;
			position.y = py;
			base.transform.position = position;
		}
	}

	public void ApplyOrientationData(KATransformData orientationTransformData, bool isPortrait)
	{
		base.transform.localPosition = orientationTransformData._LocalPosition;
		base.transform.localRotation = Quaternion.Euler(orientationTransformData._LocalRotation);
		base.transform.localScale = orientationTransformData._LocalScale;
		foreach (KAWidget item in mItemInfo)
		{
			item.UpdateOrientation(isPortrait);
		}
	}
}
