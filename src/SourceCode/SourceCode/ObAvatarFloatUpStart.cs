using UnityEngine;

public class ObAvatarFloatUpStart : MonoBehaviour
{
	public GameObject _FloatUpTrigger;

	public void OnTriggerEnter(Collider other)
	{
		GameObject gameObject = other.gameObject;
		if (IsPlayer(gameObject))
		{
			if (_FloatUpTrigger == null)
			{
				Debug.LogError("_FloatUpTrigger not set for: " + base.gameObject.name);
				Debug.LogError("Floating up will fail");
			}
			else
			{
				_FloatUpTrigger.SendMessage("Use", gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private bool IsPlayer(GameObject player)
	{
		if ((AvAvatarController)player.GetComponent("AvAvatarController") == null)
		{
			return false;
		}
		return true;
	}
}
