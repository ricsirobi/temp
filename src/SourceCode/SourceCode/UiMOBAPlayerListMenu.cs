using UnityEngine;

public class UiMOBAPlayerListMenu : KAUIMenu
{
	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (!(inWidget.name == "BtnJoinPlayer") && inWidget.name == "BtnInvitePlayer")
		{
			KAWidget parentItem = inWidget.GetParentItem();
			MOBALobbyManager.pInstance.SendTeamInvite(parentItem.name);
		}
	}

	public void AddPlayerToList(string userId, string displayName)
	{
		int numItems = GetNumItems();
		KAWidget kAWidget = DuplicateWidget(_Template);
		kAWidget.name = userId;
		KAWidget kAWidget2 = kAWidget.FindChildItem("Playername");
		kAWidget2.SetText(displayName);
		kAWidget2.SetVisibility(inVisible: true);
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.SetDisabled(isDisabled: false);
		Debug.Log("Adding player : " + displayName);
		AddWidgetAt(numItems, kAWidget);
	}

	public void RemovePlayerFromList(string userId)
	{
		KAWidget kAWidget = FindItem(userId);
		if (kAWidget != null)
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem("Playername");
			Debug.Log("Removing player : " + kAWidget2.GetText());
			RemoveWidget(kAWidget);
		}
	}
}
