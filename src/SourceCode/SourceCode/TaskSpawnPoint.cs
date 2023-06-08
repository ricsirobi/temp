using System;
using UnityEngine;

[Serializable]
public class TaskSpawnPoint
{
	public TaskInfo _BeginTask;

	public TaskInfo _EndTask;

	public Transform[] _SpawnPoints;
}
