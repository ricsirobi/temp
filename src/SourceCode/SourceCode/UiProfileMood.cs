using UnityEngine;

public class UiProfileMood : UiProfileBase
{
	private KAWidget mTxtMood;

	private KAWidget mCloseBtn;

	protected override void Start()
	{
		base.Start();
		mDropDownMenu = (KAUIDropDownMenu)_MenuList[0];
		mTxtMood = FindItem("TxtMood");
		mCloseBtn = FindItem("MenuCloseBtn");
		mCloseBtn.SetVisibility(inVisible: false);
	}

	public override void ProcessMenuSelection(KAWidget item, int idx)
	{
		base.ProcessMenuSelection(item, idx);
	}

	public override void OpenDropDown()
	{
		base.OpenDropDown();
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, base.transform.localPosition.z - 3f);
	}

	public override void CloseDropDown()
	{
		base.CloseDropDown();
		mCloseBtn.SetVisibility(inVisible: false);
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, base.transform.localPosition.z + 3f);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mSelectionItem)
		{
			mCloseBtn.SetVisibility(inVisible: true);
		}
		if (item == mTxtMood)
		{
			ProcessClickedSelection();
		}
		else if (item.name == "MenuCloseBtn")
		{
			CloseDropDown();
		}
	}

	public void SetMoodText(string t)
	{
	}

	public override void ProfileDataReady(UserProfile p)
	{
		base.ProfileDataReady(p);
	}
}
