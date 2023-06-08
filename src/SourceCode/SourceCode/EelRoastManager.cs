using System.Collections.Generic;
using UnityEngine;

public class EelRoastManager : KAMonoBase
{
	public EelRoastMarkerInfo[] _EelRoastInfos;

	public MinMax _NoOfEelsToSpawn;

	private void Start()
	{
		SpawnEels();
	}

	private void SpawnEels()
	{
		if (_EelRoastInfos == null || _EelRoastInfos.Length < 1 || _NoOfEelsToSpawn.Min < 1f || _NoOfEelsToSpawn.Min > _NoOfEelsToSpawn.Max)
		{
			return;
		}
		List<EelRoastMarkerInfo> list = new List<EelRoastMarkerInfo>(_EelRoastInfos);
		int num = Mathf.Clamp(_NoOfEelsToSpawn.GetRandomInt(), 1, _EelRoastInfos.Length);
		for (int i = 0; i < num; i++)
		{
			int index = Random.Range(0, list.Count);
			string randomEelPath = GetRandomEelPath(list[index]);
			if (!string.IsNullOrEmpty(randomEelPath))
			{
				string[] array = randomEelPath.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], ResourceEventHandler, typeof(GameObject), inDontDestroy: false, list[index]);
			}
			else
			{
				UtDebug.Log("Eel Asset path is empty ");
			}
			list.RemoveAt(index);
		}
	}

	private void ResourceEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			if (inObject == null || inUserData == null)
			{
				UtDebug.LogError("ERROR: Eel's inObject or inUserData is null");
				break;
			}
			EelRoastMarkerInfo eelRoastMarkerInfo = inUserData as EelRoastMarkerInfo;
			SetUpEel((GameObject)inObject, inURL, eelRoastMarkerInfo._SpawnNode);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("ERROR: CHEST MANAGER UNABLE TO LOAD RESOURCE AT: " + inURL);
			break;
		}
	}

	private string GetRandomEelPath(EelRoastMarkerInfo eelRoastMarkerInfo)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < eelRoastMarkerInfo._EelTypes.Length; i++)
		{
			num += eelRoastMarkerInfo._EelTypes[i]._SpawnChance;
		}
		int num3 = Random.Range(0, num);
		for (int j = 0; j < eelRoastMarkerInfo._EelTypes.Length; j++)
		{
			if (eelRoastMarkerInfo._EelTypes[j]._SpawnChance != 0 && num3 >= num2 && num3 < num2 + eelRoastMarkerInfo._EelTypes[j]._SpawnChance)
			{
				return eelRoastMarkerInfo._EelTypes[j]._AssetPath;
			}
			num2 += eelRoastMarkerInfo._EelTypes[j]._SpawnChance;
		}
		return string.Empty;
	}

	private void SetUpEel(GameObject eelObject, string inURL, Transform spawnNode)
	{
		if (eelObject == null || spawnNode == null)
		{
			UtDebug.Log("Eel object or SpawnNode is null");
			return;
		}
		GameObject obj = Object.Instantiate(eelObject, spawnNode.position, spawnNode.rotation);
		string[] array = inURL.Split('/');
		obj.name = array[2];
	}
}
