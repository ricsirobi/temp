using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
	private GameObject _MsgObject;

	public void SetMessageObject(GameObject msg)
	{
		_MsgObject = msg;
	}

	private void OnTriggerEnter(Collider col)
	{
		if (_MsgObject != null && col.gameObject.CompareTag("Player"))
		{
			_MsgObject.SendMessage("OnObjectiveCompleted", SendMessageOptions.DontRequireReceiver);
			_MsgObject.SendMessage("EndGame", false, SendMessageOptions.DontRequireReceiver);
		}
	}
}
