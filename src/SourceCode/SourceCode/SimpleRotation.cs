using UnityEngine;

public class SimpleRotation : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Rotate(Vector3.up, 0.5f * Time.deltaTime, Space.Self);
	}
}
