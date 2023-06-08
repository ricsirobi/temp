using UnityEngine;

public class PushUp : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerEnter(Collider col)
	{
		col.GetComponent<Rigidbody>().AddForce(Vector3.up * 100f);
	}
}
