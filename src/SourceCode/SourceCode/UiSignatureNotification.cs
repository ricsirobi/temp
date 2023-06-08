using System;
using UnityEngine;

public class UiSignatureNotification : KAUI
{
	public string _HouseLevel = "FarmingDO";

	public LocaleString _JoinBuddyErrorText = new LocaleString("You cannot visit your Friend at this time.");

	private GameObject mMessageObject;

	private KAWidget mBtnClose;

	private UserActivityInstance mUserActivity;

	private string mCurrentUserID;

	private string[] friendsIDs;

	private KAUIGenericDB mKAUIGenericDB;

	protected override void Start()
	{
		base.Start();
		mBtnClose = FindItem("CloseBtn");
		mCurrentUserID = UserInfo.pInstance.UserID;
		mUserActivity = SocialBoxManager.pInstance.pUserActivity;
		friendsIDs = new string[3];
		SetSignatureData();
	}

	private bool CheckIsBuddy(string userId)
	{
		bool result = false;
		Buddy[] pList = BuddyList.pList;
		foreach (Buddy buddy in pList)
		{
			if (buddy.UserID == userId && buddy.Status == BuddyStatus.Approved)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private void SetSignatureData()
	{
		int num = 1;
		string text = "";
		if (mUserActivity.pList != null && mUserActivity.pList.Count >= 0)
		{
			foreach (UserActivity p in mUserActivity.pList)
			{
				if (CheckIsBuddy(p.RelatedUserID.ToString()) && p.UserActivityTypeID == 3 && p.RelatedUserID != new Guid(mCurrentUserID) && !ProductData.pPairData.GetBoolValue(SocialBoxManager.pInstance._SocialBoxPairDataKey + "-" + p.RelatedUserID.ToString(), defaultVal: false))
				{
					Buddy buddy = BuddyList.pInstance.GetBuddy(p.RelatedUserID.ToString());
					KAWidget kAWidget = FindItem("BtnVisitFriendP" + num);
					kAWidget.SetVisibility(inVisible: true);
					string text2 = kAWidget.GetText();
					if (buddy != null)
					{
						text2 = text2.Replace("{friend}", buddy.DisplayName);
					}
					kAWidget.SetText(text2);
					if (num > 1)
					{
						text += ", ";
					}
					text += buddy.DisplayName;
					friendsIDs[num - 1] = p.RelatedUserID.ToString();
					num++;
					ProductData.pPairData.SetValueAndSave(SocialBoxManager.pInstance._SocialBoxPairDataKey + "-" + p.RelatedUserID.ToString(), "true");
				}
			}
		}
		if (num > 1)
		{
			KAWidget kAWidget2 = FindItem("TxtDescSignatures");
			string text3 = kAWidget2.GetText();
			text3 = text3.Replace("{friends}", text);
			kAWidget2.SetText(text3);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name.StartsWith("BtnVisitFriendP"))
		{
			string s = inWidget.name.Substring(inWidget.name.Length - 1, 1);
			int result = 0;
			if (MainStreetMMOClient.pIsMMOEnabled && int.TryParse(s, out result) && MainStreetMMOClient.pInstance.JoinOwnerSpace(_HouseLevel, friendsIDs[result - 1]))
			{
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.SetActive(inActive: false);
				Input.ResetInputAxes();
				mMessageObject.SendMessage("OnExit");
			}
			else
			{
				ShowDialog(_JoinBuddyErrorText);
			}
		}
		else if (inWidget == mBtnClose)
		{
			SetVisibility(inVisible: false);
			KAUI.RemoveExclusive(this);
			mMessageObject.SendMessage("OnExit");
		}
	}

	private void ShowDialog(LocaleString text)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "Message Box");
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "KillGenericDB";
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB.SetTextByID(text._ID, text._Text, interactive: false);
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void KillGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	public void SetMessageObject(GameObject msg)
	{
		mMessageObject = msg;
	}
}
