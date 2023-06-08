using UnityEngine;

public class ObAvatarRespawnMarker : MonoBehaviour
{
	private void OnTriggerEnter(Collider c)
	{
		if (AvAvatar.IsCurrentPlayer(c.gameObject))
		{
			ObAvatarRespawn._Marker = base.gameObject;
		}
	}
}
