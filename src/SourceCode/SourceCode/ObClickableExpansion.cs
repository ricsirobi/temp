using UnityEngine;

public class ObClickableExpansion : ObClickable
{
	public LocaleString _DBTitleText = new LocaleString("Message");

	public LocaleString _QuestUnlockedText = new LocaleString("Go find the headmaster to start your adventure!");

	public LocaleString _QuestLockedText = new LocaleString("Go finish your introductory quests to unlock the arctic from the headmaster.");

	public string _UpsellDBURL = "RS_DATA/PfUiArcticUpsellDO.unity3d/PfUiArcticUpsellDO";

	public int _RequiredMissionID = 1605;

	public int _TicketID = 10995;

	private KAUIGenericDB mKAUiGenericDB;

	public override void OnActivate()
	{
		if (SubscriptionInfo.pIsMember)
		{
			string textByMissionLock = GetTextByMissionLock(_RequiredMissionID);
			if (!string.IsNullOrEmpty(textByMissionLock))
			{
				AvAvatar.pState = AvAvatarState.PAUSED;
				AvAvatar.SetUIActive(inActive: false);
				ShowGenericDB("DestroyDB", textByMissionLock, _DBTitleText);
			}
		}
		else if (!IsTicketExists(_TicketID))
		{
			UiUpsellDB.Init(_UpsellDBURL, OnUpsellLoadDone, null);
		}
	}

	private void OnUpsellLoadDone(RsResourceLoadEvent inEvent, UiUpsellDB upsellDB)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE && upsellDB != null)
		{
			upsellDB.SetSource(ItemPurchaseSource.EXPANSION_OB_CLICKABLE.ToString());
		}
	}

	private string GetTextByMissionLock(int inReqMissionID)
	{
		Mission mission = MissionManager.pInstance.GetMission(inReqMissionID);
		if (mission != null)
		{
			if (MissionManager.pInstance.IsLocked(mission))
			{
				return _QuestLockedText.GetLocalizedString();
			}
			return _QuestUnlockedText.GetLocalizedString();
		}
		return "";
	}

	private void ShowGenericDB(string okMessage, string inText, LocaleString inTitle)
	{
		if (mKAUiGenericDB != null)
		{
			DestroyDB();
		}
		mKAUiGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", inText, inTitle.GetLocalizedString(), base.gameObject, "", "", okMessage, "");
	}

	private void DestroyDB()
	{
		if (!(mKAUiGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUiGenericDB);
			Object.Destroy(mKAUiGenericDB.gameObject);
			mKAUiGenericDB = null;
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
		}
	}

	private bool IsTicketExists(int inTicketID)
	{
		if (ParentData.pIsReady && ParentData.pInstance.HasItem(inTicketID))
		{
			return true;
		}
		if (CommonInventoryData.pIsReady && CommonInventoryData.pInstance.FindItem(inTicketID) != null)
		{
			return true;
		}
		return false;
	}
}
