using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class LevelLayout : MonoBehaviour
{
	public List<SceneData> _SceneDataList;

	private SceneData mRandomSceneData;

	public SceneData pRandomSceneData => mRandomSceneData;

	public void Init()
	{
		mRandomSceneData = GetRandomSceneData();
		mRandomSceneData.Init();
	}

	private SceneData GetRandomSceneData()
	{
		int index = Random.Range(0, _SceneDataList.Count);
		return _SceneDataList[index];
	}
}
