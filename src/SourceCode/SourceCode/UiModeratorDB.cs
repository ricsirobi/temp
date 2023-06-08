using System;
using UnityEngine;

public class UiModeratorDB : KAUI
{
	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public LocaleString _GenericErrorText = new LocaleString("Server error.");

	[NonSerialized]
	public string _UserID;

	[NonSerialized]
	public GameObject _MessageObject;

	public KAWidget _Moderator;

	public KAWidget _Report;

	private KAWidget mCloseBtn;

	private KAWidget mYesBtn;

	private KAWidget mNoBtn;

	private KAWidget mOKBtn;

	private KAWidget mReasons;

	private KAWidget mSent;

	private GameObject mUiGenericDB;

	protected override void Start()
	{
		base.Start();
		mCloseBtn = FindItem("CloseBtn");
		mYesBtn = FindItem("YesBtn");
		mNoBtn = FindItem("NoBtn");
		mOKBtn = FindItem("OKBtn");
		mReasons = FindItem("Reasons");
		mSent = FindItem("Sent");
		if (!UtPlatform.IsiOS() && mYesBtn != null && mNoBtn != null)
		{
			Vector3 localPosition = mNoBtn.transform.localPosition;
			mNoBtn.transform.localPosition = mYesBtn.transform.localPosition;
			mYesBtn.transform.localPosition = localPosition;
		}
		KAUI.SetExclusive(this, _MaskColor);
	}

	public void SetVisibility(ModeratorType type)
	{
		switch (type)
		{
		case ModeratorType.MODERATOR:
			_Moderator.SetVisibility(inVisible: true);
			break;
		case ModeratorType.REPORT:
			_Report.SetVisibility(inVisible: true);
			break;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCloseBtn || inWidget == mNoBtn || inWidget == mOKBtn)
		{
			Close();
		}
		else if (inWidget == mYesBtn)
		{
			_Report.SetVisibility(inVisible: false);
			mReasons.SetVisibility(inVisible: true);
		}
		else if (inWidget.name.Contains("Reason"))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			SetInteractive(interactive: false);
			int reason = int.Parse(inWidget.name.Substring(inWidget.name.Length - 2));
			WsWebService.ReportUser(_UserID, reason, EventHandler, null);
		}
	}

	public void Close()
	{
		KAUI.RemoveExclusive(this);
		UnityEngine.Object.Destroy(base.gameObject);
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnModeratorClose", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void EventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			mReasons.SetVisibility(inVisible: false);
			mSent.SetVisibility(inVisible: true);
			break;
		case WsServiceEvent.ERROR:
		{
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			mUiGenericDB = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSmSocial"));
			KAUIGenericDB component = mUiGenericDB.GetComponent<KAUIGenericDB>();
			component._MessageObject = base.gameObject;
			component._OKMessage = "OnError";
			component.SetTextByID(_GenericErrorText._ID, _GenericErrorText._Text, interactive: false);
			component.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			KAUI.SetExclusive(component, _MaskColor);
			break;
		}
		}
	}

	public void OnError()
	{
		UnityEngine.Object.Destroy(mUiGenericDB);
		KAUI.SetExclusive(this, _MaskColor);
	}
}
