using UnityEngine;

public class DestroyTarget : MonoBehaviour
{
	public AudioClip _HitSFX;

	public string _HitAnim;

	protected virtual void OnTriggerEnter(Collider collider)
	{
		CheckDestroy(collider.gameObject);
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		CheckDestroy(collision.gameObject);
	}

	protected virtual void CheckDestroy(GameObject go)
	{
		ObAmmo component = go.GetComponent<ObAmmo>();
		if (component != null && AvAvatar.IsCurrentPlayer(component.pCreator.root.gameObject) && AvAvatar.pToolbar != null)
		{
			if (_HitSFX != null)
			{
				SnChannel.Play(_HitSFX, "SFX_Pool", inForce: true);
			}
			Object.Destroy(base.gameObject);
			AvAvatar.pToolbar.BroadcastMessage("Collect", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}
}
