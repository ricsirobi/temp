public class UiStableQuestSlots : KAUI
{
	public LocaleString _CooldownSlotPurchaseText = new LocaleString("Pay {{GEMS}} gems to skip cooldown?");

	public LocaleString _NotEnoughFeeText = new LocaleString("You do not have enough gems to pay, Please buy more!");

	public LocaleString _NonMemberText = new LocaleString("Members can only play this quest, Would you like to become a member.");

	public LocaleString _BuyToothlessText = new LocaleString("Become at least a 3 months member, and get NightFury to play this quest!");

	public UiStableQuestMain _StableQuestMainUI;

	public AdEventType _AdEventType;

	private KAWidget mExitButton;

	protected override void Start()
	{
		base.Start();
		mExitButton = FindItem("CloseBtn");
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mExitButton)
		{
			_StableQuestMainUI.DestroyUI();
		}
	}

	public void ResetUISlot(int slotID)
	{
		((UiStableQuestSlotsMenu)_MenuList[0]).ResetSlot(slotID);
	}
}
