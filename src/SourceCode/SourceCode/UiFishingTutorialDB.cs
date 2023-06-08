using UnityEngine;

public class UiFishingTutorialDB : KAUI
{
	public GameObject mGameMgrObj;

	public string _YesBtnMsg = "OnTutBoxYes";

	public string _OnNextBtnMsg = "OnTutNextButtonClicked";

	public string _OnBackBtnMsg = "OnTutBackButtonClicked";

	public string _MsgBeforeClosing = "OnGenericInfoBoxExitInit";

	public string _IconName = "PfDWGobber";

	private KAWidget mNextBtn;

	private KAWidget mBackBtn;

	private KAWidget mDoneBtn;

	private KAWidget mYesBtn;

	private KAWidget mNoBtn;

	private KAWidget mCloseBtn;

	private KAWidget mOkBtn;

	private KAWidget mIcon;

	private KAWidget mTitleText;

	private KAWidget mMsgText;

	private static bool mLoading;

	private static UiFishingTutorialDB mInstance;

	private bool mInitMessageBeforeExit;

	public static UiFishingTutorialDB pInstance
	{
		get
		{
			if (mInstance == null)
			{
				Load();
			}
			return mInstance;
		}
	}

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

	public static void Load()
	{
		if (!mLoading)
		{
			mLoading = true;
			string[] array = GameConfig.GetKeyData("FishingTutorialDBRes").Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnTutorialDBReady, typeof(GameObject));
		}
	}

	private static void OnTutorialDBReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				mLoading = false;
				mInstance = (UiFishingTutorialDB)Object.Instantiate((GameObject)inObject).GetComponent(typeof(UiFishingTutorialDB));
				mInstance.SetVisibility(inVisible: false);
			}
			break;
		case RsResourceLoadEvent.ERROR:
			mLoading = false;
			Debug.LogError("Unable to load tutorial Dialog box");
			break;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		mInstance = null;
	}

	public void Initialize()
	{
		mNextBtn = FindItem("BtnHelpNext");
		mDoneBtn = FindItem("BtnHelpDone");
		mBackBtn = FindItem("BtnHelpBack");
		mYesBtn = FindItem("BtnYes");
		mNoBtn = FindItem("BtnNo");
		mCloseBtn = FindItem("BtnHelpClose");
		mOkBtn = FindItem("BtnOK");
		mIcon = FindItem("Icon");
		mTitleText = FindItem("TxtHelpHeader");
		mMsgText = FindItem("uiText");
	}

	public override void SetState(KAUIState inState)
	{
		switch (inState)
		{
		case KAUIState.DISABLED:
			base.SetState(inState);
			break;
		case KAUIState.INTERACTIVE:
			base.SetState(inState);
			break;
		case KAUIState.NOT_INTERACTIVE:
			base.SetState(inState);
			break;
		}
	}

	public void SetText(string inTitleText, string inMsgText)
	{
		if (mTitleText == null)
		{
			Initialize();
		}
		if (mTitleText != null)
		{
			mTitleText.SetText(inTitleText);
			mTitleText.SetVisibility(inVisible: true);
		}
		if (mMsgText != null)
		{
			mMsgText.SetText(inMsgText);
		}
	}

	public void SetTitle(string inText)
	{
		if (mTitleText != null)
		{
			mTitleText.SetText(inText);
		}
	}

	public void SetIconTexture(Texture textureInst)
	{
		if (mIcon != null)
		{
			mIcon.SetTexture(textureInst);
		}
	}

	private void SetButtonsActive(bool inBtnNext, bool inBtnBack, bool inBtnYes, bool inBtnNo, bool inBtnDone, bool inBtnClose, bool inBtnOk)
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
		if (mOkBtn != null)
		{
			mOkBtn.SetVisibility(inBtnOk);
		}
		if (!string.IsNullOrEmpty(_IconName))
		{
			KAWidget kAWidget = FindItem("NPCIcon");
			if (kAWidget != null)
			{
				kAWidget.PlayAnim(_IconName);
			}
		}
	}

	public void Set(string inTitle, string inMsg)
	{
		SetButtonsActive(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose: false, inBtnOk: false);
		SetText(inTitle, inMsg);
	}

	public void SetYesNo(string inTitle, string inMsg)
	{
		SetButtonsActive(inBtnNext: false, inBtnBack: false, inBtnYes: true, inBtnNo: true, inBtnDone: false, inBtnClose: false, inBtnOk: false);
		SetText(inTitle, inMsg);
	}

	public void SetNextBack(string inTitle, string inMsg)
	{
		SetButtonsActive(inBtnNext: true, inBtnBack: true, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose: false, inBtnOk: false);
		SetText(inTitle, inMsg);
	}

	public void SetOk(string inTitle, string inMsg)
	{
		SetButtonsActive(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose: false, inBtnOk: true);
		SetText(inTitle, inMsg);
		KAUI.SetExclusive(this);
	}

	public void Exit()
	{
		if (KAUI._GlobalExclusiveUI == this)
		{
			KAUI.RemoveExclusive(this);
		}
		Show(inShow: false);
		mGameMgrObj.SendMessage("TutOnDone");
	}

	public void SetInteractiveNextBackBtns(bool flag)
	{
		SetInteractiveNextBackBtns(flag, nextBtn: true, backBtn: true);
	}

	public void SetInteractiveNextBackBtns(bool flag, bool nextBtn, bool backBtn)
	{
		Color white = Color.white;
		if (!flag)
		{
			white.a = 0.5f;
		}
		if (mNextBtn != null && nextBtn)
		{
			mNextBtn.SetInteractive(flag);
			mNextBtn.ColorBlendTo(white, white, 0.01f);
		}
		if (mBackBtn != null && backBtn)
		{
			mBackBtn.ColorBlendTo(white, white, 0.01f);
			mBackBtn.SetInteractive(flag);
		}
	}

	private void OnYesBtnClick()
	{
		Show(inShow: false);
		mGameMgrObj.SendMessage(_YesBtnMsg, null);
	}

	private void LateUpdate()
	{
		KAWidget clickedItem = GetClickedItem();
		if (clickedItem == null)
		{
			return;
		}
		if (clickedItem.name == "BtnHelpClose")
		{
			if (!mInitMessageBeforeExit)
			{
				Exit();
			}
			else if (mGameMgrObj != null)
			{
				mGameMgrObj.SendMessage(_MsgBeforeClosing, SendMessageOptions.RequireReceiver);
			}
		}
		else if (clickedItem.name == "BtnHelpDone")
		{
			Exit();
		}
		else if (clickedItem.name == "BtnOK")
		{
			Exit();
		}
		else if (clickedItem.name == "BtnYes")
		{
			OnYesBtnClick();
		}
		else if (clickedItem.name == "BtnNo")
		{
			Exit();
		}
		else if (clickedItem.Equals(mBackBtn))
		{
			if (mGameMgrObj != null)
			{
				mGameMgrObj.SendMessage(_OnBackBtnMsg, SendMessageOptions.RequireReceiver);
			}
		}
		else if (clickedItem.Equals(mNextBtn) && mGameMgrObj != null)
		{
			mGameMgrObj.SendMessage(_OnNextBtnMsg, SendMessageOptions.RequireReceiver);
		}
	}

	public void Show(bool inShow)
	{
		SetVisibility(inShow);
	}

	public void SetPosition(float px, float py)
	{
		Vector3 position = base.transform.position;
		position.x = px;
		position.y = py;
		base.transform.position = position;
	}

	public void ShowHideBigDB(bool show)
	{
		KAWidget kAWidget = FindItem("BkgDBGeneric");
		KAWidget kAWidget2 = FindItem("BkgDBGenericBig");
		KAWidget kAWidget3 = FindItem("NPCIcon");
		if (!show)
		{
			kAWidget2.gameObject.SetActive(value: false);
			kAWidget3.gameObject.SetActive(value: false);
		}
		else
		{
			kAWidget.SetVisibility(inVisible: false);
		}
	}
}
