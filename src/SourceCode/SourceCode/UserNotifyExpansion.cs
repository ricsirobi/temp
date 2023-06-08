using System;
using UnityEngine;

public class UserNotifyExpansion : UserNotify
{
	private const string lastCloseDBDateKey = "LAST_UPSELL_CLOSE_DB_DATE";

	private const string firstShowDBDateKey = "FIRST_UPSELL_SHOW_DB_DATE";

	private const string announcementDoneKey = "UNExpansionAnnouncementDoneKey";

	private static bool mDoneOnce;

	public string _UpsellDBURL = "RS_DATA/PfUiArcticUpsellDO.unity3d/PfUiArcticUpsellDO";

	public LocaleString _QuestUnlockedText = new LocaleString("Go find the headmaster to start your adventure!");

	public LocaleString _QuestLockedText = new LocaleString("Go finish your introductory quests to unlock the arctic from the headmaster.");

	public int _RequiredMissionID = 1605;

	public int _TicketID = 10995;

	public int _PromotionInDays = 14;

	public LocaleString _AnnouncementDBTitleText = new LocaleString("Announcement");

	private KAUIGenericDB mKAUiGenericDB;

	public override void OnWaitBeginImpl()
	{
		if (mDoneOnce || !AnyHatchedDragonsExists() || FUEManager.pIsFUERunning)
		{
			mDoneOnce = true;
			OnWaitEnd();
			return;
		}
		mDoneOnce = true;
		if (SubscriptionInfo.pIsMember)
		{
			if (!ProductData.TutorialComplete("UNExpansionAnnouncementDoneKey_" + _TicketID))
			{
				string textByMissionLock = GetTextByMissionLock(_RequiredMissionID);
				if (!string.IsNullOrEmpty(textByMissionLock))
				{
					AvAvatar.pState = AvAvatarState.PAUSED;
					AvAvatar.SetUIActive(inActive: false);
					ShowGenericDB("Announcement", "DestroyDB", textByMissionLock, _AnnouncementDBTitleText);
					ProductData.AddTutorial("UNExpansionAnnouncementDoneKey_" + _TicketID);
				}
			}
			else
			{
				OnWaitEnd();
			}
		}
		else if (!IsTicketExists(_TicketID) && IsInPromotionTime() && !IsMissionStarted(_RequiredMissionID) && IsFirstTimeInADay())
		{
			UiUpsellDB.Init(_UpsellDBURL, OnUpsellLoadDone, OnUpsellCloseCallback);
		}
		else
		{
			OnWaitEnd();
		}
	}

	private void OnUpsellLoadDone(RsResourceLoadEvent inEvent, UiUpsellDB upsellDB)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (upsellDB != null)
			{
				upsellDB.SetSource(ItemPurchaseSource.EXPANSION_USER_NOTIFY.ToString());
			}
			if (!ProductData.pPairData.KeyExists("FIRST_UPSELL_SHOW_DB_DATE_" + _TicketID))
			{
				ProductData.pPairData.SetValueAndSave("FIRST_UPSELL_SHOW_DB_DATE_" + _TicketID, ServerTime.pCurrentTime.ToString(UtUtilities.GetCultureInfo("en-US")));
			}
			break;
		case RsResourceLoadEvent.ERROR:
			OnWaitEnd();
			break;
		}
	}

	public void OnUpsellCloseCallback(bool isPurchased)
	{
		ProductData.pPairData.SetValueAndSave("LAST_UPSELL_CLOSE_DB_DATE_" + _TicketID, ServerTime.pCurrentTime.ToString(UtUtilities.GetCultureInfo("en-US")));
		OnWaitEnd();
	}

	public static void Reset()
	{
		mDoneOnce = false;
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

	private bool IsInPromotionTime()
	{
		if (ProductData.pPairData != null)
		{
			string value = ProductData.pPairData.GetValue("FIRST_UPSELL_SHOW_DB_DATE_" + _TicketID);
			if (!string.IsNullOrEmpty(value) && value != "___VALUE_NOT_FOUND___")
			{
				DateTime minValue = DateTime.MinValue;
				try
				{
					minValue = DateTime.Parse(value, UtUtilities.GetCultureInfo("en-US"));
				}
				catch (Exception exception)
				{
					minValue = ServerTime.pCurrentTime;
					Debug.LogException(exception);
				}
				if ((ServerTime.pCurrentTime - minValue).Days > _PromotionInDays)
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool IsFirstTimeInADay()
	{
		if (ProductData.pPairData != null)
		{
			string value = ProductData.pPairData.GetValue("LAST_UPSELL_CLOSE_DB_DATE_" + _TicketID);
			if (string.IsNullOrEmpty(value) || !(value != "___VALUE_NOT_FOUND___"))
			{
				return true;
			}
			DateTime minValue = DateTime.MinValue;
			try
			{
				minValue = DateTime.Parse(value, UtUtilities.GetCultureInfo("en-US"));
			}
			catch (Exception exception)
			{
				minValue = ServerTime.pCurrentTime;
				Debug.LogException(exception);
			}
			if ((ServerTime.pCurrentTime - minValue).Days >= 1)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsMissionStarted(int inMissionID)
	{
		return MissionManager.pInstance.GetMission(inMissionID)?.pStarted ?? false;
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

	private bool AnyHatchedDragonsExists()
	{
		if (RaisedPetData.pActivePets != null)
		{
			foreach (RaisedPetData[] value in RaisedPetData.pActivePets.Values)
			{
				if (value != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ShowGenericDB(string inDBName, string okMessage, string inText, LocaleString inTitle)
	{
		if (mKAUiGenericDB != null)
		{
			DestroyDB();
		}
		mKAUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", inDBName);
		mKAUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUiGenericDB._MessageObject = base.gameObject;
		mKAUiGenericDB._OKMessage = okMessage;
		mKAUiGenericDB.SetText(inText, interactive: false);
		mKAUiGenericDB.SetTitle(inTitle.GetLocalizedString());
		KAUI.SetExclusive(mKAUiGenericDB);
	}

	private void DestroyDB()
	{
		if (!(mKAUiGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUiGenericDB);
			UnityEngine.Object.Destroy(mKAUiGenericDB.gameObject);
			mKAUiGenericDB = null;
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			OnWaitEnd();
		}
	}
}
