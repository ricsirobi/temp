using UnityEngine;

public class UiMOBAPreGameLobby : KAUI
{
	public UiMOBASelectDragon _DragonSelectionUI;

	public GameObject _ChatWindow;

	private KAWidget mTimer;

	private AvPhotoManager mPhotoManager;

	private const int DEFAULT_COUNTDOWN_TIME = 30;

	protected override void Start()
	{
		mTimer = FindItem("BkgTimer");
	}

	public void Init(MOBATeam oppTeam)
	{
		SetVisibility(inVisible: true);
		SetState(KAUIState.INTERACTIVE);
		UiChatHistory._IsVisible = true;
		if (_ChatWindow != null)
		{
			_ChatWindow.SetActive(value: true);
		}
		_DragonSelectionUI.Init();
		SetTeams(MOBALobbyManager.pInstance.pCurrentTeam, oppTeam);
		UpdateTimer(30);
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (_ChatWindow != null)
		{
			_ChatWindow.SetActive(inVisible);
		}
		if (_DragonSelectionUI != null)
		{
			_DragonSelectionUI.SetVisibility(inVisible);
		}
	}

	public void UpdateTimer(int timeLeft)
	{
		string text = "{{MM}}:{{SS}}";
		int num = timeLeft / 60;
		int num2 = timeLeft % 60;
		text = text.Replace("{{MM}}", num.ToString("00"));
		text = text.Replace("{{SS}}", num2.ToString("00"));
		mTimer.SetText(text);
	}

	public void SetTeams(MOBATeam myTeam, MOBATeam oppTeam)
	{
		mPhotoManager = AvPhotoManager.Init("PfToolbarPhotoMgr");
		KAWidget kAWidget = FindItem("BkgTeams");
		kAWidget.FindChildItem("MyTeamName").SetText(myTeam.pName);
		KAWidget playerWidget = kAWidget.FindChildItem("MyTeamLeader");
		LoadPlayerThumbnail(myTeam.pMemberIds[0], playerWidget);
		if (myTeam.pMemberIds.Count > 1)
		{
			KAWidget playerWidget2 = kAWidget.FindChildItem("MyTeamPlayer2");
			LoadPlayerThumbnail(myTeam.pMemberIds[1], playerWidget2);
			KAWidget playerWidget3 = kAWidget.FindChildItem("MyTeamPlayer3");
			LoadPlayerThumbnail(myTeam.pMemberIds[2], playerWidget3);
		}
		kAWidget.FindChildItem("OpposingTeamName").SetText(oppTeam.pName);
		KAWidget playerWidget4 = kAWidget.FindChildItem("OpposingTeamLeader");
		LoadPlayerThumbnail(oppTeam.pMemberIds[0], playerWidget4);
		if (oppTeam.pMemberIds.Count > 1)
		{
			KAWidget playerWidget5 = kAWidget.FindChildItem("OpposingTeamPlayer2");
			LoadPlayerThumbnail(oppTeam.pMemberIds[1], playerWidget5);
			KAWidget playerWidget6 = kAWidget.FindChildItem("OpposingTeamPlayer3");
			LoadPlayerThumbnail(oppTeam.pMemberIds[2], playerWidget6);
		}
	}

	public void LoadPlayerThumbnail(string userId, KAWidget playerWidget)
	{
		playerWidget.SetVisibility(inVisible: true);
		KAWidget inUserData = playerWidget.FindChildItem("TxtPlayerName");
		WsWebService.GetDisplayNameByUserID(userId, ServiceEventHandler, inUserData);
		KAWidget udata = playerWidget.FindChildItem("IcoPlayerTexture");
		mPhotoManager.TakePhotoUI(userId, ProfileAvPhotoCallback, udata);
	}

	public void ProfileAvPhotoCallback(Texture tex, object inUserData)
	{
		KAWidget kAWidget = (KAWidget)inUserData;
		if (kAWidget != null)
		{
			kAWidget.SetTexture(tex);
		}
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType == WsServiceType.GET_DISPLAYNAME_BY_USER_ID && inEvent == WsServiceEvent.COMPLETE)
		{
			KAWidget kAWidget = (KAWidget)inUserData;
			if (kAWidget != null)
			{
				kAWidget.SetText((string)inObject);
			}
		}
	}
}
