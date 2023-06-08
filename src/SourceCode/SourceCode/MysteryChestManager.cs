using System.Collections.Generic;
using UnityEngine;

public class MysteryChestManager : KAMonoBase
{
	public string _EventName;

	public MysteryChestInfo[] _ListOfChests;

	public GameObject _RewardTemplate;

	public GameObject _RewardTextTemplate;

	public LocaleString _PurchaseFailed = new LocaleString("Purchase Failed.");

	public LocaleString _NotEnoughGoldText = new LocaleString("You have insufficient Coins.");

	public LocaleString _NotEnoughGemsText = new LocaleString("You have insufficient Gems. Do you want to buy Gems from the store?");

	public LocaleString _RewardText = new LocaleString("You found {0} {1}!");

	public static bool _CheatSpawnAll;

	private bool CanSpawn(MysteryChestInfo info)
	{
		bool result = (_CheatSpawnAll ? 0f : Random.Range(1f, 100f)) <= info._SpawnChance && info._StoreInfoList.Length != 0 && info._SpawnNodeList.Length != 0;
		if (info._ChestTypeInfo == ChestType.MysteryChest)
		{
			return result;
		}
		if (UtPlatform.IsMobile() && info._ChestTypeInfo == ChestType.AdRewardMysteryChest && !AdManager.pInstance.HideAdEventForMember(AdEventType.WORLD_MYSTERY_CHEST) && AdManager.pInstance.AdAvailable(AdEventType.WORLD_MYSTERY_CHEST, AdType.REWARDED_VIDEO, showErrorMessage: false))
		{
			return result;
		}
		return false;
	}

	public void SpawnChests()
	{
		MysteryChestInfo[] listOfChests = _ListOfChests;
		foreach (MysteryChestInfo mysteryChestInfo in listOfChests)
		{
			if (CanSpawn(mysteryChestInfo))
			{
				string[] array = mysteryChestInfo._AssetPath.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], ResourceEventHandler, typeof(GameObject), inDontDestroy: false, mysteryChestInfo);
			}
		}
	}

	private void ResourceEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject == null)
			{
				break;
			}
			if (inUserData != null)
			{
				MysteryChestInfo mysteryChestInfo = inUserData as MysteryChestInfo;
				List<Transform> list = new List<Transform>(mysteryChestInfo._SpawnNodeList);
				list.RemoveAll((Transform node) => node == null);
				int num = ((!_CheatSpawnAll) ? 1 : list.Count);
				for (int i = 0; i < num; i++)
				{
					int index = Random.Range(0, list.Count);
					SetupChest((GameObject)inObject, inURL, mysteryChestInfo, list[index]);
					list.RemoveAt(index);
				}
			}
			else
			{
				Debug.LogError("ERROR: Mystery Chest Manager - inUserData is null!");
			}
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("ERROR: CHEST MANAGER UNABLE TO LOAD RESOURCE AT: " + inURL);
			break;
		}
	}

	private void SetupChest(GameObject inObject, string inURL, MysteryChestInfo chestInfo, Transform spawnNode)
	{
		if (inObject == null || spawnNode == null)
		{
			Debug.LogError("Mystery chest object or spawn node is null!");
			return;
		}
		GameObject obj = Object.Instantiate(inObject, spawnNode.position, spawnNode.rotation);
		string[] array = inURL.Split('/');
		obj.name = array[2];
		MysteryChest component = obj.GetComponent<MysteryChest>();
		if (component != null)
		{
			int num = Random.Range(0, chestInfo._StoreInfoList.Length);
			component.Init(this, chestInfo._StoreInfoList[num], chestInfo._ChestTypeInfo);
		}
	}
}
