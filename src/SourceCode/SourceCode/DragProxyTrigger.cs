using UnityEngine;

public class DragProxyTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider collider)
	{
		if (base.transform.parent != null)
		{
			base.transform.parent.SendMessage("AddCollisionObject", collider);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (base.transform.parent != null)
		{
			base.transform.parent.SendMessage("RemoveCollisionObject", collider);
		}
	}

	private void OnTriggerStay(Collider collider)
	{
		if (base.transform.parent != null)
		{
			base.transform.parent.SendMessage("AddCollisionObject", collider);
		}
	}
}
