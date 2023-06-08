using UnityEngine;

public class ObProximityUnlock : ObProximity
{
	public GameObject _ResetMarker;

	public string _UnlockLevel = "";

	private void OnProximity()
	{
		if (_UnlockLevel.Length <= 0 || !(_ResetMarker != null))
		{
			return;
		}
		UnlockManager.IsSceneUnlocked(_UnlockLevel, inShowUi: false, delegate(bool success)
		{
			if (!success)
			{
				AvAvatar.TeleportToObject(_ResetMarker);
			}
		});
	}
}
