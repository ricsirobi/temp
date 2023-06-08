using UnityEngine;

public class UiProfileFav : UiProfileBase
{
	private UiProfileFavMenu mMainMenu;

	private UiProfileFavSubMenu mSubMenu;

	private KAWidget mBtnClose;

	protected override void Start()
	{
		base.Start();
		mMainMenu = (UiProfileFavMenu)GetMenu("UiProfileFavMenu");
		mSubMenu = (UiProfileFavSubMenu)GetMenu("UiProfileFavSubMenu");
		mDropDownMenu = mSubMenu;
		mBtnClose = FindItem("BtnClose");
	}

	public override void OnSetVisibility(bool t)
	{
		base.OnSetVisibility(t);
		if (mSubMenu == null)
		{
			mSubMenu = (UiProfileFavSubMenu)GetMenu("UiProfileFavSubMenu");
		}
		mSubMenu.SetVisibility(t: false);
	}

	public override void ProcessMenuSelection(KAWidget item, int idx)
	{
		base.ProcessMenuSelection(item, idx);
	}

	public void ProcessFavMenuSelection(KAWidget item, int idx)
	{
		ProfileFavQuestionUserData profileFavQuestionUserData = (ProfileFavQuestionUserData)item.GetUserData();
		if (mSelectionItem != null)
		{
			mSelectionItem.SetInteractive(isInteractive: true);
		}
		mSelectionItem = item;
		mDisplayItem = item;
		mSubMenu.SetVisibility(t: true);
		mBkgItem.SetVisibility(inVisible: true);
		mMainMenu.SetVisibility(inVisible: true);
		mSubMenu._TemplateName = "SubMenuTemplate";
		mSubMenu.ChangeCategory(profileFavQuestionUserData._QData.ID);
		if (_MessageObj != null)
		{
			_MessageObj.SendMessage("OnDropDownOpen", this, SendMessageOptions.DontRequireReceiver);
		}
	}

	public override void ProfileDataReady(UserProfile p)
	{
		pProfileData = p;
	}

	public override void OnClick(KAWidget inWidget)
	{
		mSelectionItem = inWidget;
		base.OnClick(inWidget);
		if (inWidget.name == "SubMenuTemplate")
		{
			CloseDropDown();
			ProcessDropDownSelection(inWidget);
		}
		else if (inWidget == mBtnClose)
		{
			CloseDropDown();
			OnSetVisibility(t: false);
		}
	}
}
