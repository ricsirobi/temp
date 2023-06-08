using UnityEngine;

public class UiMOBAChallengeMenu : KAUIMenu
{
	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnChallengeTeam")
		{
			KAWidget parentItem = inWidget.GetParentItem();
			MOBALobbyManager.pInstance.SendChallenge(parentItem.name);
			inWidget.SetInteractive(isInteractive: false);
		}
	}

	public void AddTeamToList(string leaderId)
	{
		if (!string.IsNullOrEmpty(leaderId))
		{
			KAWidget kAWidget = DuplicateWidget(_Template);
			kAWidget.name = leaderId;
			kAWidget.SetVisibility(inVisible: true);
			kAWidget.SetDisabled(isDisabled: true);
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
		}
	}

	public void RemoveTeamFromList(string leaderId)
	{
		KAWidget kAWidget = FindItem(leaderId);
		if (kAWidget != null)
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem("TeamName");
			Debug.Log("Removing team : " + kAWidget2.GetText());
			RemoveWidget(kAWidget);
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
				KAWidget kAWidget2 = (KAWidget)inUserData;
				KAWidget kAWidget3 = kAWidget2.FindChildItem("TeamName");
				string text = (string)inObject;
				text += "'s team";
				kAWidget3.SetText(text);
				kAWidget3.SetVisibility(inVisible: true);
				int numItems2 = GetNumItems();
				AddWidgetAt(numItems2, kAWidget2);
				kAWidget2.SetDisabled(isDisabled: false);
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
						KAWidget kAWidget = (KAWidget)inUserData;
						ClanData clanData = new ClanData(group);
						kAWidget.SetUserData(clanData);
						clanData.Load();
						kAWidget.FindChildItem("TeamName").SetText(group.Name);
						kAWidget.FindChildItem("TrophyCount").SetText(group.Points.HasValue ? group.Points.ToString() : "0");
						int numItems = GetNumItems();
						AddWidgetAt(numItems, kAWidget);
						kAWidget.SetDisabled(isDisabled: false);
					}
				}
				break;
			}
			}
			break;
		case WsServiceEvent.ERROR:
			if (inType == WsServiceType.GET_DISPLAYNAME_BY_USER_ID)
			{
				((KAWidget)inUserData).FindChildItem("TeamName").SetText("GetNameFailed");
			}
			break;
		}
	}
}
