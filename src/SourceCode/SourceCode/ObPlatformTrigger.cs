using UnityEngine;

public class ObPlatformTrigger : MonoBehaviour
{
	private void OnTriggerStay(Collider c)
	{
		if (c.CompareTag("Player"))
		{
			base.gameObject.SendMessageUpwards("OnActivate", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void ToggleTrigger(bool active)
	{
		GetComponent<Collider>().enabled = active;
	}
}
