using System;
using SquadTactics;

public class UiAvatarStats : KAUI
{
	private KAWidget mCloseBtn;

	private KAWidget mAvatarNameLabel;

	private KAUIMenu mMenu;

	public LocaleString _PopupHeadingPart = new LocaleString("[Review]'s Stats");

	protected override void Awake()
	{
		base.Awake();
		mMenu = _MenuList[0];
	}

	protected override void Start()
	{
		mCloseBtn = FindItem("BtnClose");
		mAvatarNameLabel = FindItem("AvatarName");
		base.Start();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCloseBtn)
		{
			SetVisibility(inVisible: false);
		}
	}

	public void Initialize(ItemStat[] stats, string displayName = null)
	{
		if (mMenu != null)
		{
			mMenu.ClearItems();
		}
		mAvatarNameLabel.SetText(displayName + _PopupHeadingPart.GetLocalizedString());
		PopulateStats(stats);
		SetVisibility(inVisible: true);
	}

	private void PopulateStats(ItemStat[] stats)
	{
		int i;
		for (i = 0; i < Settings.pInstance._StatInfo.Length; i++)
		{
			if (!Settings.pInstance._StatInfo[i]._Display)
			{
				continue;
			}
			KAWidget kAWidget = mMenu.DuplicateWidget(mMenu._Template);
			mMenu.AddWidget(kAWidget);
			kAWidget.SetVisibility(inVisible: true);
			string localizedString = Settings.pInstance._StatInfo[i]._DisplayText.GetLocalizedString();
			string text = "0";
			if (stats != null)
			{
				ItemStat itemStat = Array.Find(stats, (ItemStat statInfo) => statInfo.ItemStatID == Settings.pInstance._StatInfo[i]._StatID);
				if (itemStat != null)
				{
					text = itemStat.Value;
				}
			}
			KAWidget kAWidget2 = kAWidget.FindChildItem("StatName");
			if (kAWidget2 != null)
			{
				kAWidget2.SetText(localizedString);
			}
			KAWidget kAWidget3 = kAWidget.FindChildItem("Value");
			if (kAWidget3 != null)
			{
				kAWidget3.SetText(text);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (GetVisibility() && KAInput.GetMouseButtonDown(0) && KAUI.GetGlobalMouseOverItem() == null)
		{
			SetVisibility(inVisible: false);
		}
	}
}
