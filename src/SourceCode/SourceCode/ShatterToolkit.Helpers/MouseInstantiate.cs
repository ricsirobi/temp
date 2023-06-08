using UnityEngine;

namespace ShatterToolkit.Helpers;

public class MouseInstantiate : MonoBehaviour
{
	public GameObject prefabToInstantiate;

	public float speed = 7f;

	public void Update()
	{
		if (Input.GetMouseButtonDown(0) && prefabToInstantiate != null)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Rigidbody component = Object.Instantiate(prefabToInstantiate, ray.origin, Quaternion.identity).GetComponent<Rigidbody>();
			if (component != null)
			{
				component.velocity = ray.direction * speed;
			}
		}
	}
}
