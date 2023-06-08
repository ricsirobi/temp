using System.Collections.Generic;
using UnityEngine;

public class UiReleaseNotes : KAUI
{
	private KAWidget mTxtTitle;

	private KAWidget mTxtSubTitle;

	private KAUIMenu mContentMenu;

	private KAWidget mParentWidget;

	private KAWidget mCloseBtn;

	private bool mIsBackBtnAllowed = true;

	public bool pInitDone { get; set; }

	public bool pIsBackBtnAllowed => mIsBackBtnAllowed;

	protected override void Start()
	{
		mTxtTitle = FindItem("TxtTitleReleaseNotes");
		mTxtSubTitle = FindItem("TxtSubTitleReleaseNotes");
		mCloseBtn = FindItem("BtnClose");
		mContentMenu = _MenuList[0];
		base.Start();
	}

	public void Init(ReleaseNotes notes, bool canBeClosed = true)
	{
		if (!pInitDone)
		{
			mTxtTitle.SetText(notes.Title);
			mTxtSubTitle.SetText(notes.SubTitle);
			if (mCloseBtn != null)
			{
				mCloseBtn.SetVisibility(canBeClosed);
			}
			mContentMenu.ClearItems();
			for (int i = 0; i < notes.ContentList.Length; i++)
			{
				KAWidget kAWidget = mContentMenu.AddWidget(mContentMenu._Template.name);
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.SetText(notes.ContentList[i]);
			}
			SetVisibility(!canBeClosed);
			pInitDone = true;
		}
		else
		{
			mTxtTitle.SetText(notes.Title);
			mTxtSubTitle.SetText(notes.SubTitle);
			List<Transform> childList = mContentMenu._DefaultGrid.GetChildList();
			for (int j = 0; j < childList.Count; j++)
			{
				childList[j].GetComponent<KAWidget>().SetText(notes.ContentList[j]);
			}
		}
	}

	protected override void UpdateVisibility(bool inVisible)
	{
		base.UpdateVisibility(inVisible);
		if (inVisible)
		{
			mIsBackBtnAllowed = false;
		}
		else
		{
			Invoke("EnableBackButton", Time.deltaTime);
		}
		if (mContentMenu != null && mContentMenu.pMenuGrid != null)
		{
			mContentMenu.pMenuGrid.Reposition();
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (!(inWidget == mCloseBtn))
		{
			return;
		}
		if (UtPlatform.IsAndroid())
		{
			if (UiLogin.pInstance._RegisterUI.IsActive())
			{
				UiLogin.pInstance._RegisterUI.CloseDB();
			}
			else
			{
				SetVisibility(inVisible: false);
			}
		}
		else
		{
			SetVisibility(inVisible: false);
		}
	}

	private void EnableBackButton()
	{
		mIsBackBtnAllowed = true;
	}
}
