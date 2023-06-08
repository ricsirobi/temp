public class UiRacingFirendsListMenu : KAUIMenu
{
	protected override void Start()
	{
		base.Start();
	}

	public override void OnClick(KAWidget inWidget)
	{
		((UiRacingMultiplayer)_ParentUi).ShowMMODragonInfo(inWidget.name);
		base.OnClick(inWidget);
	}

	public void UpdatePlayerState(string userId, bool isReady)
	{
		KAWidget kAWidget = FindItem(userId);
		if (kAWidget != null)
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem("PlayerStateOn");
			KAWidget kAWidget3 = kAWidget.FindChildItem("PlayerStateOff");
			kAWidget2.SetVisibility(isReady);
			kAWidget3.SetVisibility(!isReady);
		}
	}
}
