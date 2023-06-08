using System;
using UnityEngine;

public class UiMissionActionDB : KAUI
{
	public GameObject _MessageObject;

	public string _CloseMessage = "";

	public string _YesMessage = "";

	public string _NoMessage = "";

	private KAWidget mTxtDialog;

	private KAWidget mLoading;

	private KAWidget mCloseButton;

	private KAWidget mNextButton;

	private KAWidget mBackButton;

	private KAWidget mYesButton;

	private KAWidget mNoButton;

	private bool mInitialized;

	private string[] mPageBreakString = new string[1] { "@@" };

	private string[] mTextPages;

	private int mCurrPageIdx;

	public virtual void Initialize()
	{
		mTxtDialog = FindItem("TxtDialog");
		mCloseButton = FindItem("CloseBtn");
		mNextButton = FindItem("NextBtn");
		mBackButton = FindItem("BackBtn");
		mYesButton = FindItem("YesBtn");
		mNoButton = FindItem("NoBtn");
		mInitialized = true;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if ((inWidget == mCloseButton || inWidget.name == "CloseRdBtn") && _CloseMessage.Length > 0)
		{
			SnChannel.StopPool("VO_Pool");
			OnClickMessage(_CloseMessage);
		}
		else if (inWidget == mBackButton)
		{
			mCurrPageIdx--;
			ShowPage(mCurrPageIdx);
		}
		else if (inWidget == mNextButton)
		{
			mCurrPageIdx++;
			ShowPage(mCurrPageIdx);
		}
		else if (inWidget == mYesButton)
		{
			OnClickMessage(_YesMessage);
		}
		else if (inWidget == mNoButton)
		{
			OnClickMessage(_NoMessage);
		}
	}

	public void OnClickMessage(string inMessage)
	{
		if (_MessageObject != null && !string.IsNullOrEmpty(inMessage))
		{
			_MessageObject.SendMessage(inMessage, null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public virtual void SetItemID(int itemID)
	{
		mLoading = FindItem("Loading");
		mLoading.SetVisibility(inVisible: true);
		WsWebService.GetItemData(itemID, ServiceEventHandler, null);
	}

	public virtual void SetText(string inText)
	{
		if (!mInitialized)
		{
			Initialize();
		}
		SetupPages(inText);
		mCurrPageIdx = 0;
		if (mTextPages != null && mTextPages.Length != 0)
		{
			ShowPage(mCurrPageIdx);
		}
	}

	public virtual void SetTextByID(int inTextID, string defaultText)
	{
		if (!mInitialized)
		{
			Initialize();
		}
		string stringData = StringTable.GetStringData(inTextID, defaultText);
		SetText(stringData);
	}

	public virtual void SetNPCIcon(string iconName)
	{
		if (string.IsNullOrEmpty(iconName))
		{
			return;
		}
		KAWidget kAWidget = FindItem("NPCIcon");
		if (kAWidget != null)
		{
			kAWidget.PlayAnim(iconName);
		}
		if (iconName == "PfPlayer")
		{
			KAWidget kAWidget2 = FindItem("AvatarPic");
			kAWidget2?.SetTexture(UiToolbar.pPortraitTexture);
			kAWidget2?.SetVisibility(inVisible: true);
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
		KAWidget kAWidget3 = FindItem("TxtTitle");
		if (kAWidget3 != null)
		{
			kAWidget3.SetText(MissionManagerDO.GetNPCName(iconName));
		}
	}

	public void ShowPage(int inPageIdx)
	{
		if (inPageIdx >= 0 && inPageIdx < mTextPages.Length)
		{
			mTxtDialog.SetText(mTextPages[inPageIdx]);
			if (mBackButton != null)
			{
				mBackButton.SetVisibility(inPageIdx > 0);
			}
			if (mNextButton != null)
			{
				mNextButton.SetVisibility(inPageIdx < mTextPages.Length - 1);
			}
			if (mCloseButton != null)
			{
				mCloseButton.SetVisibility(inPageIdx == mTextPages.Length - 1);
			}
		}
	}

	public virtual void SetupPages(string inText)
	{
		mTextPages = inText.Split(mPageBreakString, StringSplitOptions.RemoveEmptyEntries);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE)
		{
			CoBundleItemData coBundleItemData = new CoBundleItemData(((ItemData)inObject).IconName, null);
			mLoading.SetUserData(coBundleItemData);
			coBundleItemData.LoadResource();
		}
	}

	protected override void Update()
	{
		if (!KAInput.GetKeyDown(KeyCode.Space))
		{
			return;
		}
		if (mNextButton != null && mNextButton.gameObject.activeSelf && mNextButton.GetVisibility())
		{
			mCurrPageIdx++;
			ShowPage(mCurrPageIdx);
		}
		else if (mCloseButton != null && mCloseButton.gameObject.activeSelf && mCloseButton.GetVisibility())
		{
			SnChannel.StopPool("VO_Pool");
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_CloseMessage, null, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
