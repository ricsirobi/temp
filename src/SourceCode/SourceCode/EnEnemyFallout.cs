using UnityEngine;

public class EnEnemyFallout : MonoBehaviour
{
	public void OnTriggerEnter(Collider inCollider)
	{
		if (inCollider.gameObject.GetComponent("EnEnemy") as EnEnemy != null)
		{
			inCollider.gameObject.SendMessage("OnRestart", false, SendMessageOptions.DontRequireReceiver);
		}
	}
}
