using UnityEngine;

public class ObTriggerArea : ObTrigger
{
	public GameObject _ResetMarker;

	public string _ZoneName;

	public override void OnTriggerEnter(Collider inCollider)
	{
		if (!RsResourceManager.pLevelLoadingScreen && inCollider.gameObject == AvAvatar.pObject && UnlockManager.IsSceneUnlocked(_ZoneName, inShowUi: false, delegate(bool success)
		{
			if (!success)
			{
				AvAvatar.TeleportToObject(_ResetMarker);
			}
		}))
		{
			base.OnTriggerEnter(inCollider);
		}
	}
}
