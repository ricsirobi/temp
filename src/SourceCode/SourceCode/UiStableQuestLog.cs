public class UiStableQuestLog : KAUI
{
	private KAWidget mViewDragonButton;

	private UiStableQuestLogMenu mMenu;

	private int mSlotID;

	protected override void Start()
	{
		base.Start();
		SetVisibility(inVisible: false);
		mMenu = (UiStableQuestLogMenu)_MenuList[0];
		mViewDragonButton = FindItem("BtnViewDragon");
	}

	public void Init(int slotID)
	{
		mSlotID = slotID;
	}

	private void Reset()
	{
		if (mMenu != null)
		{
			mMenu.ClearItems();
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (!inVisible)
		{
			Reset();
		}
		else if (mMenu != null)
		{
			mMenu.PopulateItems(mSlotID);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget == mViewDragonButton)
		{
			SetVisibility(inVisible: false);
		}
	}
}
