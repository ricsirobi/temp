using System.Collections.Generic;
using UnityEngine;

public class UiGSBuddySelect : KAUI
{
	public GameObject _MessageObject;

	private UiGSBuddySelectMenu mMenu;

	private KAWidget mBtnOk;

	private KAWidget mBtnClose;

	private KAWidget mTxtMessage;

	private KAWidget mBtnSelectAll;

	private List<string> mSelectedBuddies = new List<string>();

	public List<string> pSelectedBuddies => mSelectedBuddies;

	protected override void Start()
	{
		LocaleData.Init();
		base.Start();
		mMenu = (UiGSBuddySelectMenu)GetMenu("UiGSBuddySelectMenu");
		mBtnOk = FindItem("btnOk");
		mBtnClose = FindItem("btnClose");
		mTxtMessage = FindItem("txtUserMessage");
		mBtnSelectAll = FindItem("btnSelectAll");
	}

	public override void SetVisibility(bool t)
	{
		base.SetVisibility(t);
		if (t)
		{
			mBtnOk.SetState(KAUIState.DISABLED);
			mTxtMessage.SetVisibility(inVisible: false);
			mSelectedBuddies.Clear();
			mMenu.ClearItems();
			BuddyList.ReInit(ShowBuddies);
			KAUICursorManager.SetDefaultCursor("Loading");
		}
		else
		{
			BuddyList.RemoveSyncBuddyListEventHandler(ShowBuddies);
		}
	}

	private void AddBuddy(Buddy inBuddy)
	{
		if (inBuddy != null && inBuddy.DisplayName.Length > 0)
		{
			KAToggleButton obj = (KAToggleButton)mMenu.AddWidget("BuddyTemplate");
			obj.name = inBuddy.UserID;
			string displayName = inBuddy.DisplayName;
			obj.SetText(displayName);
			obj.SetChecked(isChecked: false);
			obj.SetVisibility(inVisible: true);
		}
	}

	public void OnBuddyNameSelected(KAWidget inItem)
	{
		string item = inItem.name;
		if (mSelectedBuddies.Contains(item))
		{
			mSelectedBuddies.Remove(item);
		}
		else
		{
			mSelectedBuddies.Add(item);
		}
		mBtnOk.SetDisabled(mSelectedBuddies.Count <= 0);
		if (pSelectedBuddies.Count == mMenu.GetItemCount())
		{
			mBtnSelectAll.SetState(KAUIState.DISABLED);
		}
		else
		{
			mBtnSelectAll.SetState(KAUIState.INTERACTIVE);
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mBtnOk)
		{
			SetVisibility(t: false);
			_MessageObject.SendMessage("OnBuddyInvite", mSelectedBuddies.ToArray(), SendMessageOptions.DontRequireReceiver);
		}
		else if (item == mBtnClose)
		{
			SetVisibility(t: false);
			mSelectedBuddies.Clear();
			_MessageObject.SendMessage("OnBuddyInvite", mSelectedBuddies.ToArray(), SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			if (!(item == mBtnSelectAll))
			{
				return;
			}
			mSelectedBuddies.Clear();
			foreach (KAWidget item2 in mMenu.GetItems())
			{
				KAToggleButton kAToggleButton = (KAToggleButton)item2;
				if (kAToggleButton != null)
				{
					kAToggleButton.SetChecked(isChecked: true);
				}
				OnBuddyNameSelected(item2);
			}
		}
	}

	private void ShowBuddies()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		bool flag = false;
		int num = 0;
		Buddy[] pList = BuddyList.pList;
		foreach (Buddy buddy in pList)
		{
			if (buddy.Status == BuddyStatus.Approved && buddy.Online)
			{
				AddBuddy(buddy);
				flag = true;
				num++;
			}
		}
		if (mBtnSelectAll != null)
		{
			mBtnSelectAll.SetState((num <= 1) ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
		}
		mTxtMessage.SetVisibility(!flag);
	}
}
