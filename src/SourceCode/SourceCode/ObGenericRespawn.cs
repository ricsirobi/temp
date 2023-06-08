using UnityEngine;

public class ObGenericRespawn : MonoBehaviour
{
	[SerializeField]
	private Transform respawnMarker;

	public Transform GetRespawnMarker()
	{
		return respawnMarker;
	}
}
