using UnityEngine;

public class ObMyRoomDoorExit : MonoBehaviour
{
	public AudioClip _BuildModeExitVO;

	public string _LoadLevel;

	public string _StartMarker;

	private void OnActivate()
	{
		ExitRoom();
	}

	private void OnTrigger()
	{
		ExitRoom();
	}

	private void ExitRoom()
	{
		if (MyRoomsIntMain.pInstance != null && MyRoomsIntMain.pInstance.pIsBuildMode)
		{
			if (_BuildModeExitVO != null)
			{
				SnChannel.Play(_BuildModeExitVO, "VO_Pool", inForce: true);
			}
		}
		else if (_LoadLevel.Length > 0)
		{
			WsUserMessage.pBlockMessages = false;
			TutorialManager.StopTutorials();
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			AvAvatar.SetActive(inActive: false);
			if (!string.IsNullOrEmpty(MyRoomsIntLevel.pHouseExitMarker))
			{
				AvAvatar.pStartLocation = MyRoomsIntLevel.pHouseExitMarker;
			}
			else if (_StartMarker != "")
			{
				AvAvatar.pStartLocation = _StartMarker;
			}
			RsResourceManager.LoadLevel(_LoadLevel);
		}
	}
}
