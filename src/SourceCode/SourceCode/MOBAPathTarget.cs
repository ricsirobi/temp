using UnityEngine;

public class MOBAPathTarget : KAMonoBase
{
	public bool _Initial;

	public int _TeamID;

	public GameObject _NextPathObject;

	private void Start()
	{
		base.renderer.enabled = false;
	}

	private void Update()
	{
	}

	public Vector3 GetRandomLocation()
	{
		float x = Random.Range(-5f, 5f);
		float z = Random.Range(-5f, 5f);
		Vector3 position = new Vector3(x, 0f, z);
		return base.transform.TransformPoint(position);
	}
}
