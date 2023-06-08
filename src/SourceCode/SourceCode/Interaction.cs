using UnityEngine;

public class Interaction : MonoBehaviour
{
	public GameObject leftWheel;

	public GameObject rightWheel;

	public GFMachine differentialGeartrain;

	public GFMachine mainEngine;

	private void Start()
	{
	}

	private void Update()
	{
		float normalizedForce = 0f;
		if (Input.GetMouseButton(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo, 999f))
		{
			if (hitInfo.transform.gameObject == leftWheel)
			{
				normalizedForce = 1f;
			}
			else if (hitInfo.transform.gameObject == rightWheel)
			{
				normalizedForce = -1f;
			}
		}
		LockDif(normalizedForce);
	}

	private void LockDif(float normalizedForce)
	{
		differentialGeartrain.Reverse = normalizedForce < 0f;
		differentialGeartrain.speed = mainEngine.speed * Mathf.Abs(normalizedForce);
	}
}
