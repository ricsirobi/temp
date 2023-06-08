using UnityEngine;

public class GemSpawner : MonoBehaviour
{
	public Transform gem1;

	public Transform gem2;

	public Transform gem3;

	public Transform gem4;

	public float spread = 5f;

	private void Update()
	{
		if (Input.GetKey("1"))
		{
			Object.Instantiate(gem1).position = base.transform.position + Random.insideUnitSphere * spread;
		}
		if (Input.GetKey("2"))
		{
			Object.Instantiate(gem2).position = base.transform.position + Random.insideUnitSphere * spread;
		}
		if (Input.GetKey("3"))
		{
			Object.Instantiate(gem3).position = base.transform.position + Random.insideUnitSphere * spread;
		}
		if (Input.GetKey("4"))
		{
			Object.Instantiate(gem4).position = base.transform.position + Random.insideUnitSphere * spread;
		}
	}
}
