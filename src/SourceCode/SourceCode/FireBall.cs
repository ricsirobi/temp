using UnityEngine;

public class FireBall : MonoBehaviour
{
	public string pTargetPrtName = "PfPrtDragonTargetFire";

	private void OnTriggerEnter(Collider collisionInfo)
	{
	}

	public void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(new Vector3(0f, 0f, 3f), new Vector3(1f, 1f, 6f));
	}
}
