using UnityEngine;

namespace ExpansionMissionBoard;

public class NPCAvatarExpansion : NPCAvatar
{
	protected override bool ShowMissionBoard()
	{
		string[] array = GetMissionBoardAsset().Split('/');
		if (array.Length > 2)
		{
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnExpansionUILoaded, typeof(GameObject));
			return true;
		}
		OnMissionBoardClosed();
		return false;
	}

	private void OnExpansionUILoaded(string inUrl, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			Object.Instantiate((GameObject)inObject).GetComponent<UiExpansionMissionBoard>().Init(_MissionGroupID, OnMissionBoardClosed);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}
}
