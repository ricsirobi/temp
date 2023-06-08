using UnityEngine;

public class ObEnemyRespawn : MonoBehaviour
{
	private void OnTriggerEnter(Collider c)
	{
		EnEnemy component = c.gameObject.GetComponent<EnEnemy>();
		if (component != null)
		{
			component.OnEnemyRespawn();
		}
	}
}
