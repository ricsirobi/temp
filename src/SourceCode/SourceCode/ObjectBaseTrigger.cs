using UnityEngine;

public class ObjectBaseTrigger : MonoBehaviour
{
	private bool mIsInTriggerStay;

	private void OnTriggerStay2D(Collider2D other)
	{
		if (!mIsInTriggerStay)
		{
			base.transform.parent.SendMessage("OnChildTriggerEnter2D", SendMessageOptions.DontRequireReceiver);
		}
		mIsInTriggerStay = true;
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		mIsInTriggerStay = false;
		base.transform.parent.SendMessage("OnChildTriggerExit2D", SendMessageOptions.DontRequireReceiver);
	}
}
