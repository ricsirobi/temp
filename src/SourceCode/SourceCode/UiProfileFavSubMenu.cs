using UnityEngine;

public class UiProfileFavSubMenu : UiProfileMenuBase
{
	private Vector2 mUpPos;

	private Vector2 mDnPos;

	private Vector2 mBkgPos;

	private Vector2 mOldPos;

	protected override void Start()
	{
		base.Start();
	}

	public void SyncControls()
	{
	}

	public override void OnClick(KAWidget item)
	{
		mDropDownUI.ProcessMenuSelection(item, 0);
		mDropDownUI.CloseDropDown();
	}
}
