using UnityEngine;

public class ObTriggerScale : MonoBehaviour
{
	public Vector3 _Scale = Vector3.one;

	public float _ChangeTime = 1f;

	public bool _RevertToOriginalScale = true;

	private void OnTriggerEnter(Collider collider)
	{
		if (CheckForAvatar(collider.gameObject))
		{
			ObScale.Set(collider.gameObject, _Scale, _ChangeTime, inStartScale: true, _RevertToOriginalScale);
		}
	}

	private void OnTriggerStay(Collider collider)
	{
		if (CheckForAvatar(collider.gameObject) && collider.gameObject.transform.localScale != _Scale)
		{
			ObScale.Set(collider.gameObject, _Scale, _ChangeTime, inStartScale: true, _RevertToOriginalScale);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (CheckForAvatar(collider.gameObject))
		{
			collider.gameObject.SendMessage("OnRemoveScale", SendMessageOptions.DontRequireReceiver);
		}
	}

	private bool CheckForAvatar(GameObject inObject)
	{
		if (inObject.GetComponent<AvAvatarController>() != null)
		{
			return true;
		}
		return false;
	}
}
