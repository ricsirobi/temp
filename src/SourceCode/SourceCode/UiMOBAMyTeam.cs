public class UiMOBAMyTeam : KAUI
{
	public UiMOBAMyTeamMember _TeamLeader;

	public UiMOBAMyTeamMember _TeamMember1;

	public UiMOBAMyTeamMember _TeamMember2;

	private KAWidget mBtnReady;

	private KAWidget mBtnCancel;

	private MOBATeam mTeam;

	public MOBATeam pTeam
	{
		get
		{
			return mTeam;
		}
		set
		{
			mTeam = value;
			bool interactive = true;
			mBtnReady.SetInteractive(interactive);
			if (mTeam == null)
			{
				_TeamLeader.pPlayerId = UserInfo.pInstance.UserID;
				_TeamMember1.pPlayerId = null;
				_TeamMember2.pPlayerId = null;
			}
			else
			{
				_TeamLeader.pPlayerId = mTeam.pMemberIds[0];
				_TeamMember1.pPlayerId = ((mTeam.pMemberIds.Count > 1) ? mTeam.pMemberIds[1] : null);
				_TeamMember2.pPlayerId = ((mTeam.pMemberIds.Count > 2) ? mTeam.pMemberIds[2] : null);
				mBtnReady.SetInteractive(mTeam.pMemberIds.Count == 3);
			}
		}
	}

	protected override void Start()
	{
		mBtnReady = FindItem("BtnReady");
		mBtnCancel = FindItem("BtnCancel");
		mBtnReady.SetVisibility(inVisible: true);
		mBtnCancel.SetVisibility(inVisible: false);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnReady)
		{
			bool visibility = MOBALobbyManager.pInstance._ClanLobby.GetVisibility();
			MOBALobbyManager.pInstance.SetTeamReady(isReady: true, visibility);
			mBtnReady.SetVisibility(inVisible: false);
			mBtnCancel.SetVisibility(inVisible: true);
			_TeamMember1.pCanInvite = false;
			_TeamMember2.pCanInvite = false;
		}
		else if (inWidget == mBtnCancel)
		{
			bool visibility2 = MOBALobbyManager.pInstance._ClanLobby.GetVisibility();
			MOBALobbyManager.pInstance.SetTeamReady(isReady: false, visibility2);
			mBtnCancel.SetVisibility(inVisible: false);
			mBtnReady.SetVisibility(inVisible: true);
			_TeamMember1.pCanInvite = true;
			_TeamMember2.pCanInvite = true;
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		_TeamLeader.SetVisibility(inVisible);
		_TeamMember1.SetVisibility(inVisible);
		_TeamMember2.SetVisibility(inVisible);
	}

	public bool IsReady()
	{
		return !mBtnReady.GetVisibility();
	}
}
