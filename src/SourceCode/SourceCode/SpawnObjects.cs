using UnityEngine;

public class SpawnObjects : MonoBehaviour
{
	public GameObject[] _Objects;

	public MinMax _SpawnDelayInSecs;

	public SphereCollider _SpawnArea;

	private float mMaxSpawnDelay;

	private float mSpawnDelay;

	private void Update()
	{
		mSpawnDelay += Time.deltaTime;
		if (mSpawnDelay > mMaxSpawnDelay)
		{
			mSpawnDelay = 0f;
			mMaxSpawnDelay = _SpawnDelayInSecs.GetRandomValue();
			Spawn();
		}
	}

	private void Spawn()
	{
		Vector3 position = _SpawnArea.transform.position;
		Vector3 position2 = _SpawnArea.transform.position + Random.insideUnitSphere * _SpawnArea.radius;
		position2.y = position.y;
		Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
		Object.Instantiate(_Objects[Random.Range(0, _Objects.Length)], position2, rotation).SetActive(value: true);
	}
}
