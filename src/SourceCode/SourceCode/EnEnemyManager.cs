using System.Collections.Generic;
using UnityEngine;

public class EnEnemyManager : MonoBehaviour
{
	private List<EnEnemy> mEnemies;

	private static EnEnemyManager mInstance;

	public static EnEnemyManager pInstance => mInstance;

	public static List<EnEnemy> pEnemies => mInstance.mEnemies;

	private void Awake()
	{
		mInstance = this;
		mEnemies = new List<EnEnemy>();
	}

	public static void AddEnemy(EnEnemy enemy)
	{
		if (enemy != null && mInstance != null)
		{
			mInstance.mEnemies.Add(enemy);
		}
	}
}
