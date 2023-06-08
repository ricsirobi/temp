public class UiMOBAInviteMenu : KAUIMenu
{
	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnAcceptChallenge")
		{
			KAWidget parentItem = inWidget.GetParentItem();
			MOBALobbyManager.pInstance.AcceptChallenge(parentItem.name);
		}
	}

	public void AddChallengeInvite(string leaderId)
	{
		if (!string.IsNullOrEmpty(leaderId))
		{
			int numItems = GetNumItems();
			KAWidget kAWidget = DuplicateWidget(_Template);
			kAWidget.name = leaderId;
			if (MOBALobbyManager.pInstance._ClanLobby.GetVisibility())
			{
				WsWebService.GetGroups(new GetGroupsRequest
				{
					ForUserID = leaderId
				}, ServiceEventHandler, kAWidget);
			}
			else if (MOBALobbyManager.pInstance._SkirmishLobby.GetVisibility())
			{
				WsWebService.GetDisplayNameByUserID(leaderId, ServiceEventHandler, kAWidget);
			}
			kAWidget.SetVisibility(inVisible: true);
			kAWidget.SetDisabled(isDisabled: false);
			AddWidgetAt(numItems, kAWidget);
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			switch (inType)
			{
			case WsServiceType.GET_DISPLAYNAME_BY_USER_ID:
			{
				KAWidget obj2 = (KAWidget)inUserData;
				KAWidget kAWidget = obj2.FindChildItem("TeamName");
				string text = (string)inObject;
				text += "'s team";
				kAWidget.SetText(text);
				kAWidget.SetVisibility(inVisible: true);
				obj2.SetDisabled(isDisabled: false);
				break;
			}
			case WsServiceType.GET_GROUPS:
			{
				GetGroupsResult getGroupsResult = (GetGroupsResult)inObject;
				if (getGroupsResult != null && getGroupsResult.Groups != null)
				{
					Group group = getGroupsResult.Groups[0];
					if (group != null)
					{
						KAWidget obj = (KAWidget)inUserData;
						ClanData clanData = new ClanData(group);
						obj.SetUserData(clanData);
						clanData.Load();
						obj.FindChildItem("TeamName").SetText(group.Name);
					}
				}
				break;
			}
			}
			break;
		case WsServiceEvent.ERROR:
			if (inType == WsServiceType.GET_DISPLAYNAME_BY_USER_ID)
			{
				RemoveWidget((KAWidget)inObject);
			}
			break;
		}
	}
}
