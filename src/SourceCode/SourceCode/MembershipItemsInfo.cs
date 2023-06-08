using System.Collections.Generic;
using UnityEngine;

public class MembershipItemsInfo
{
	public static LocaleString _BecomeMemberText = new LocaleString("Become at least a 3 months member to use {{ItemName}}!");

	public static LocaleString _BecomeMemberTitleText = new LocaleString("Membership Expired");

	public static LocaleString _UpgradeMemberTitleText = new LocaleString("Upgrade Membership");

	private static List<int> mMemberOnlyTickets = new List<int>();

	private static KAUIGenericDB mKAUIGenericDB = null;

	public static void SaveMemberOnlyItems(List<int> memberOnlyTickets)
	{
		mMemberOnlyTickets = memberOnlyTickets;
	}

	public static bool IsMemberOnlyItem(RaisedPetData petData, string key = "TicketID")
	{
		RaisedPetAttribute raisedPetAttribute = petData.FindAttrData(key);
		if (raisedPetAttribute != null && mMemberOnlyTickets != null && mMemberOnlyTickets.Contains(int.Parse(raisedPetAttribute.Value)))
		{
			return true;
		}
		return false;
	}

	public static void ShowMembershipDB(GameObject gameObj, string itemName)
	{
		LocaleString localeString = new LocaleString(_BecomeMemberText.GetLocalizedString().Replace("{{ItemName}}", itemName));
		if (mKAUIGenericDB == null)
		{
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "MembershipExpired");
		}
		KAUI.SetExclusive(mKAUIGenericDB);
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB.SetText(localeString.GetLocalizedString(), interactive: false);
		if (SubscriptionInfo.pIsMember)
		{
			mKAUIGenericDB.SetTitle(_UpgradeMemberTitleText.GetLocalizedString());
		}
		else
		{
			mKAUIGenericDB.SetTitle(_BecomeMemberTitleText.GetLocalizedString());
		}
		mKAUIGenericDB.SetDestroyOnClick(isDestroy: true);
	}
}
