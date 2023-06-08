using UnityEngine;

public class ObProximityDestroy : MonoBehaviour
{
	public GameObject _Object;

	public float _ProximityDistance = 0.5f;

	public void Update()
	{
		if (_Object != null && Vector3.Distance(base.transform.position, _Object.transform.position) < _ProximityDistance)
		{
			Object.Destroy(_Object);
			base.enabled = false;
		}
	}
}
