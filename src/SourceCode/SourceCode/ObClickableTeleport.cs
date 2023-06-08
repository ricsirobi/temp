using UnityEngine;

public class ObClickableTeleport : ObClickable
{
	public string _MarkerName;

	public override void OnActivate()
	{
		GameObject gameObject = GameObject.Find(_MarkerName);
		if (gameObject != null)
		{
			AvAvatar.TeleportToObject(gameObject);
		}
		base.OnActivate();
	}
}
