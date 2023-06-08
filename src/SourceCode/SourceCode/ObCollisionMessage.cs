using UnityEngine;

public class ObCollisionMessage : MonoBehaviour
{
	public static GameObject _MessageObject;

	public static string _CollisionMessage;

	public virtual void OnTriggerEnter(Collider c)
	{
		if (_MessageObject != null && _MessageObject.activeSelf && !string.IsNullOrEmpty(_CollisionMessage))
		{
			_MessageObject.SendMessage(_CollisionMessage, c.gameObject, SendMessageOptions.RequireReceiver);
		}
	}

	public virtual void OnCollisionEnter(Collision c)
	{
		if (_MessageObject != null && _MessageObject.activeSelf && !string.IsNullOrEmpty(_CollisionMessage))
		{
			_MessageObject.SendMessage(_CollisionMessage, c, SendMessageOptions.RequireReceiver);
		}
	}
}
