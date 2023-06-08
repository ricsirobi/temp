public class UiMOBAMyTeamMember : KAUI
{
	private KAWidget mLblPlayerName;

	private KAWidget mBtnInvite;

	private KAWidget mBtnLeave;

	private KAWidget mBtnKick;

	public bool _IsLeader;

	public UiMOBAMyTeam _TeamUI;

	private string mPlayerId;

	public string pPlayerId
	{
		get
		{
			return mPlayerId;
		}
		set
		{
			if (mPlayerId == UserInfo.pInstance.UserID)
			{
				mBtnLeave.SetVisibility(_TeamUI.pTeam != null);
			}
			if (mPlayerId == value)
			{
				return;
			}
			Reset();
			mPlayerId = value;
			if (mPlayerId != null)
			{
				if (mPlayerId == UserInfo.pInstance.UserID)
				{
					mLblPlayerName.SetText(AvatarData.pInstance.DisplayName);
					mLblPlayerName.SetVisibility(inVisible: true);
				}
				else
				{
					WsWebService.GetDisplayNameByUserID(mPlayerId, ServiceEventHandler, null);
				}
			}
		}
	}

	public bool pCanInvite
	{
		set
		{
			if (mBtnInvite != null)
			{
				mBtnInvite.SetInteractive(value);
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		mLblPlayerName = FindItem("PlayerName");
		mBtnLeave = FindItem("BtnLeave");
		if (!_IsLeader)
		{
			mBtnInvite = FindItem("BtnInvite");
			mBtnKick = FindItem("BtnKick");
		}
		Reset();
		if (_IsLeader)
		{
			pPlayerId = UserInfo.pInstance.UserID;
		}
	}

	private void Reset()
	{
		mLblPlayerName.SetVisibility(inVisible: false);
		mBtnLeave.SetVisibility(inVisible: false);
		if (!_IsLeader)
		{
			if (_TeamUI.pTeam == null || UserInfo.pInstance.UserID == _TeamUI.pTeam.pMemberIds[0])
			{
				mBtnInvite.SetVisibility(inVisible: true);
			}
			else
			{
				mBtnInvite.SetVisibility(inVisible: false);
			}
			mBtnKick.SetVisibility(inVisible: false);
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && inType == WsServiceType.GET_DISPLAYNAME_BY_USER_ID)
		{
			mLblPlayerName.SetText((string)inObject);
			mLblPlayerName.SetVisibility(inVisible: true);
			if (_TeamUI.pTeam != null && UserInfo.pInstance.UserID == _TeamUI.pTeam.pMemberIds[0])
			{
				mBtnKick.SetVisibility(inVisible: true);
				mBtnInvite.SetVisibility(inVisible: false);
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnLeave")
		{
			MOBALobbyManager.pInstance.LeaveTeam();
		}
		else if (!(inWidget.name == "BtnInvite") && inWidget.name == "BtnKick")
		{
			MOBALobbyManager.pInstance.KickPlayer(mPlayerId);
			MOBALobbyManager.pInstance.SetTeamReady(isReady: false, MOBALobbyManager.pInstance._ClanLobby.GetVisibility());
		}
	}
}
