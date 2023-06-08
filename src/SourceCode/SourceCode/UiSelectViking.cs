using System.Collections.Generic;
using UnityEngine;

public class UiSelectViking : KAUI
{
	public enum Status
	{
		Loaded,
		Accepted,
		Closed
	}

	public delegate void Callback(Status status, ChildInfo selectedChild);

	private KAUIGenericDB mGenericDB;

	private KAWidget mBtnAccept;

	private KAWidget mBtnClose;

	private KAWidget mTxtDisplay;

	private KAUIMenu mMenu;

	private KAWidget mLastSelectedWidget;

	private string mDisplayText;

	private ChildInfo mSelectedChild;

	private List<ChildInfo> mChildList;

	private const string mWidgetName = "ChildUser";

	private Callback mCallBack;

	public static void Init(Callback callback, string displayText, List<ChildInfo> childList)
	{
		List<object> list = new List<object>();
		list.Add(displayText);
		list.Add(childList);
		list.Add(callback);
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("SelectVikingAsset"), OnBundleReady, typeof(GameObject), inDontDestroy: false, list);
	}

	private static void OnBundleReady(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject gameObject = Object.Instantiate((GameObject)inObject);
			UiSelectViking uiSelectViking = null;
			if (gameObject != null)
			{
				uiSelectViking = gameObject.GetComponent<UiSelectViking>();
			}
			if (uiSelectViking != null && inUserData != null)
			{
				List<object> list = (List<object>)inUserData;
				uiSelectViking.SetData(list[0] as string, list[1] as List<ChildInfo>);
				uiSelectViking.SetCallback(list[2] as Callback);
				((Callback)list[2])?.Invoke(Status.Loaded, null);
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (inUserData != null)
			{
				((Callback)((List<object>)inUserData)[2])?.Invoke(Status.Closed, null);
			}
			break;
		}
	}

	public void SetData(string name, List<ChildInfo> childList)
	{
		mDisplayText = name;
		mChildList = childList;
	}

	public void SetCallback(Callback callback)
	{
		mCallBack = callback;
	}

	public void DisplayNames()
	{
		if (mMenu != null)
		{
			mMenu.ClearItems();
			if (mChildList != null)
			{
				for (int i = 0; i < mChildList.Count; i++)
				{
					ShowInMenu(mChildList[i], i);
				}
			}
		}
		if (mTxtDisplay != null)
		{
			mTxtDisplay.SetText(mDisplayText);
		}
	}

	protected override void Start()
	{
		base.Start();
		mMenu = _MenuList[0];
		mBtnClose = FindItem("BtnClose");
		mBtnAccept = FindItem("BtnAccept");
		mTxtDisplay = FindItem("TxtDisplay");
		if (mBtnAccept != null)
		{
			mBtnAccept.SetDisabled(isDisabled: true);
		}
		DisplayNames();
		KAUI.SetExclusive(this);
	}

	private void ShowInMenu(ChildInfo child, int index)
	{
		KAWidget kAWidget = mMenu.AddWidget("ChildUser");
		kAWidget.SetUserData(new KAWidgetUserData(index));
		kAWidget.SetText(child._Name);
		kAWidget.SetVisibility(inVisible: true);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item.name == "ChildUser")
		{
			if (mLastSelectedWidget != null)
			{
				KACheckBox kACheckBox = mLastSelectedWidget.FindChildItem("CheckBtn") as KACheckBox;
				if (kACheckBox != null)
				{
					kACheckBox.SetChecked(isChecked: false);
				}
			}
			mLastSelectedWidget = item;
			if (mLastSelectedWidget != null)
			{
				KACheckBox kACheckBox2 = mLastSelectedWidget.FindChildItem("CheckBtn") as KACheckBox;
				if (kACheckBox2 != null)
				{
					kACheckBox2.SetChecked(isChecked: true);
				}
				mSelectedChild = mChildList[mLastSelectedWidget.GetUserDataInt()];
			}
			if (mBtnAccept != null)
			{
				mBtnAccept.SetDisabled(isDisabled: false);
			}
		}
		else if (item.name == "AcceptBtn")
		{
			CloseUI(Status.Accepted);
		}
		else if (item.name == "CloseBtn")
		{
			CloseUI(Status.Closed);
		}
	}

	public void CloseUI(Status status)
	{
		KAUI.RemoveExclusive(this);
		Object.Destroy(base.gameObject);
		if (mCallBack != null)
		{
			mCallBack(status, mSelectedChild);
		}
	}
}
