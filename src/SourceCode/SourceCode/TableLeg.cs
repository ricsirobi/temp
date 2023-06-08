using UnityEngine;

public class TableLeg : MonoBehaviour
{
	public void PreSplit(Plane[] planes)
	{
		if (base.transform.parent != null)
		{
			base.transform.parent = null;
			base.gameObject.AddComponent<Rigidbody>();
		}
	}
}
