using System;

public class UiClansCreateTypeMenu : KAUIMenu
{
	protected override void Start()
	{
		base.Start();
		if (base.pMenuGrid.sorting == UITable.Sorting.Alphabetic)
		{
			mItemInfo.Sort((KAWidget x, KAWidget y) => string.Compare(x.name, y.name));
		}
	}

	public GroupType GetClanType()
	{
		KAWidget itemAt = GetItemAt(GetTopItemIdx());
		if (itemAt == null)
		{
			itemAt = GetItemAt(0);
		}
		string text = itemAt.name;
		if (text.IndexOf("_") > 0)
		{
			text = text.Substring(itemAt.name.IndexOf("_") + 1);
		}
		return (GroupType)Enum.Parse(typeof(GroupType), text);
	}

	public override void GoToPage(int inPageNumber, bool instant = false)
	{
		base.GoToPage(inPageNumber, instant);
		UiClansCreate uiClansCreate = (UiClansCreate)_ParentUi;
		if (!uiClansCreate)
		{
			return;
		}
		UiClansCreate.ClanTicketType type = UiClansCreate.ClanTicketType.None;
		GroupType clanType = GetClanType();
		if (clanType != uiClansCreate.GetCurrentClanType())
		{
			switch (clanType)
			{
			case GroupType.InviteOnly:
				type = UiClansCreate.ClanTicketType.InviteOnly;
				break;
			case GroupType.Closed:
				type = UiClansCreate.ClanTicketType.Closed;
				break;
			}
		}
		uiClansCreate.UpdateGemsForType(type);
	}
}
