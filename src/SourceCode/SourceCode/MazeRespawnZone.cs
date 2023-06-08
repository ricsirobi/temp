using UnityEngine;

public class MazeRespawnZone : MonoBehaviour
{
	[SerializeField]
	private Vector3 respawnOffset;

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("HEY THERE " + other.gameObject.name);
		ObGenericRespawn component = other.gameObject.GetComponent<ObGenericRespawn>();
		if (component != null)
		{
			Transform respawnMarker = component.GetRespawnMarker();
			other.gameObject.transform.position = respawnMarker.position + respawnOffset;
		}
	}
}
