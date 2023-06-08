using UnityEngine;

public class ObAvatarRespawnDO : ObAvatarRespawn
{
	public static GameObject _MessageObject;

	public static string _HitMessage;

	public override void OnTriggerEnter(Collider c)
	{
		if (AvAvatar.IsCurrentPlayer(c.gameObject) && _MessageObject != null && _MessageObject.activeSelf && !string.IsNullOrEmpty(_HitMessage))
		{
			_MessageObject.SendMessage(_HitMessage, SendMessageOptions.RequireReceiver);
		}
	}
}
