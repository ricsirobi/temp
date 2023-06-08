using System.Collections.Generic;
using UnityEngine;

public class PetSpawn : MonoBehaviour
{
	public PetSpawnPoint[] _PetSpawnPoints;

	[Tooltip("A random number of pets within the range specified here will be spawned.")]
	public MinMax _PetSpawnRange;

	private bool mSpawned;

	private int mResidentPetNum;

	private void Start()
	{
		mSpawned = false;
	}

	private void Update()
	{
		if (!mSpawned && SanctuaryData.pInstance != null)
		{
			mSpawned = true;
			List<PetSpawnPoint> list = new List<PetSpawnPoint>(_PetSpawnPoints);
			int num = _PetSpawnRange.GetRandomInt();
			if (num > _PetSpawnPoints.Length)
			{
				UtDebug.LogError("_ResidentPetsRange must not be greater than the number of spawn points");
				num = _PetSpawnPoints.Length;
			}
			mResidentPetNum = num;
			for (int i = 0; i < num; i++)
			{
				PetSpawnPoint petSpawnPoint = list[Random.Range(0, list.Count)];
				list.Remove(petSpawnPoint);
				petSpawnPoint.SpawnPet(this);
			}
		}
	}

	public void OnResidentPetReady()
	{
		mResidentPetNum--;
		if (mResidentPetNum == 0)
		{
			ObStatus component = GetComponent<ObStatus>();
			if (component != null)
			{
				component.pIsReady = true;
			}
		}
	}
}
