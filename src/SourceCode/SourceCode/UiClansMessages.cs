using UnityEngine;

public class UiClansMessages : KAUI
{
	public UiClansMessageBoard _UiClansMessageBoard;

	public UiMessageBoardTabMenu _UiMessageBoardTabMenu;

	public LocaleString _MessageBoardNameText = new LocaleString("{clanname} Message Board");

	public LocaleString _ClanNonMembersText = new LocaleString("You Need to Be a Member of this Clan to Access This ");

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	private KAUIGenericDB mKAUIGenericDB;

	public void Show(bool show)
	{
		if (show)
		{
			base.gameObject.SetActive(show);
			if (!UserProfile.pProfileData.InGroup(UiClans.pClan.GroupID))
			{
				ShowMessage();
				return;
			}
			new ClanData(UiClans.pClan, FindItem("ClanCrestTemplate")).Load();
			string localizedString = _MessageBoardNameText.GetLocalizedString();
			localizedString = localizedString.Replace("{clanname}", UiClans.pClan.Name);
			FindItem("TxtClanName").SetText(localizedString);
			_UiClansMessageBoard.OnOpenUI(UiClans.pUserID);
			_UiClansMessageBoard.OnClan(UiClans.pClan);
			_UiClansMessageBoard.OnSetVisibility(visible: true);
			_UiMessageBoardTabMenu.OnClan(UiClans.pClan);
			_UiMessageBoardTabMenu.SetSelectedItem(_UiMessageBoardTabMenu._ClanMessagesTabBtn);
			_UiMessageBoardTabMenu._ClanMessagesTabBtn.SetChecked(isChecked: true);
			_UiMessageBoardTabMenu._ClanMessagesTabBtn.SetInteractive(isInteractive: false);
			_UiMessageBoardTabMenu._ClanNewsTabBtn.SetChecked(isChecked: false);
		}
		else
		{
			base.gameObject.SetActive(value: false);
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	public void ShowMessage()
	{
		GameObject gameObject = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
		mKAUIGenericDB = gameObject.GetComponent<KAUIGenericDB>();
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "OnOK";
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
		mKAUIGenericDB.SetText(_ClanNonMembersText.GetLocalizedString(), interactive: false);
	}

	public void OnOK()
	{
		KAUI.RemoveExclusive(mKAUIGenericDB);
		Object.Destroy(mKAUIGenericDB.gameObject);
		base.gameObject.SetActive(value: false);
	}
}
