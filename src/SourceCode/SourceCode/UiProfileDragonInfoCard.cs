public class UiProfileDragonInfoCard : KAUI
{
	private UiDragonsInfoCardItem mInfoCardItem;

	protected override void Start()
	{
		base.Start();
		mInfoCardItem = (UiDragonsInfoCardItem)FindItem("TemplateInfoCardItem");
		mInfoCardItem.SetMessageObject(base.gameObject);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "ExitBtn")
		{
			SetVisibility(inVisible: false);
			KAUI.RemoveExclusive(this);
		}
	}

	public void ShowInfoCard(RaisedPetData pData)
	{
		mInfoCardItem.pSelectedPetData = pData;
		mInfoCardItem.pUserID = UiProfile.pUserProfile.UserID;
		mInfoCardItem.RefreshUI();
		if (UiProfile.pUserProfile.UserID == UserInfo.pInstance.UserID)
		{
			mInfoCardItem.SetButtons(selectBtn: true, visitBtn: true, moveInBtn: false);
		}
		else
		{
			mInfoCardItem.SetButtons(selectBtn: false, visitBtn: false, moveInBtn: false);
		}
		SetVisibility(inVisible: true);
		KAUI.SetExclusive(this);
	}

	private void OnSelectDragonStart(int petID)
	{
		SetInteractive(interactive: false);
	}

	private void OnSelectDragonFinish(int petID)
	{
		SetInteractive(interactive: true);
	}

	private void OnSelectDragonFailed(int petID)
	{
		SetInteractive(interactive: true);
	}
}
