using UnityEngine;

public class GauntletCollisionDetector : MonoBehaviour
{
	public GameObject _MessageObject;

	public void OnTriggerEnter(Collider collider)
	{
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnTriggerEnter", collider, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OnCollisionEnter(Collision iCollision)
	{
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnCollisionEnter", iCollision, SendMessageOptions.DontRequireReceiver);
		}
	}
}
