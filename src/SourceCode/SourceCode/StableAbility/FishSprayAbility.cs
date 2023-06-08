using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace StableAbility;

public class FishSprayAbility : BaseAbility
{
	public List<Transform> _FishSpawnPoints;

	public List<Transform> _ChestSpawnPoints;

	public List<StableFishGroup> _StableFishGroups;

	private List<StableFish> mSpawnedFish = new List<StableFish>();

	private List<AbilityChestInfo> mSpawnedChestInfos = new List<AbilityChestInfo>();

	public string _PairKey = "FishSpray";

	private SpawnedStableData mSpawnedStableData = new SpawnedStableData();

	private ObStatus mObStatus;

	private int mItemsToLoadCount;

	private int mLoadedItemsCount;

	public AudioClip _CollectSound;

	public List<StableFish> pSpawnedFish
	{
		get
		{
			return mSpawnedFish;
		}
		set
		{
			mSpawnedFish = value;
		}
	}

	public List<AbilityChestInfo> pSpawnedChestInfos
	{
		get
		{
			return mSpawnedChestInfos;
		}
		set
		{
			mSpawnedChestInfos = value;
		}
	}

	public override void OnPairDataLoaded(bool success, PairData pData, object inUserData)
	{
		base.OnPairDataLoaded(success, pData, inUserData);
		if (success)
		{
			StartCoroutine(Init());
		}
		else if ((bool)mObStatus)
		{
			mObStatus.pIsReady = true;
		}
		else
		{
			GetComponent<ObStatus>().pIsReady = true;
		}
	}

	private IEnumerator Init()
	{
		while (_FishSpawnPoints[0] == null || _ChestSpawnPoints[0] == null || GetComponent<ObStatus>() == null)
		{
			yield return new WaitForEndOfFrame();
		}
		mObStatus = GetComponent<ObStatus>();
		SpawnedStableData spawnedStableData = null;
		if (!string.IsNullOrEmpty(base.pPairData.GetStringValue(_PairKey, string.Empty)))
		{
			try
			{
				spawnedStableData = JsonConvert.DeserializeObject<SpawnedStableData>(base.pPairData.GetValue(_PairKey));
			}
			catch (Exception ex)
			{
				UtDebug.LogError("Failed to parse data, deleting key. Caught exception: " + ex);
				base.pPairData.RemoveByKey(_PairKey);
			}
		}
		if (spawnedStableData != null && spawnedStableData.SpawnedChestInfoData.Count + spawnedStableData.SpawnedFishData.Count != 0)
		{
			mSpawnedStableData = spawnedStableData;
			mItemsToLoadCount = spawnedStableData.SpawnedChestInfoData.Count + spawnedStableData.SpawnedFishData.Count;
			foreach (SpawnedStableData.SpawnData spawnedFishDatum in spawnedStableData.SpawnedFishData)
			{
				LoadItem(_StableFishGroups[spawnedFishDatum.g]._FishAssetPaths[spawnedFishDatum.p], spawnedFishDatum);
			}
			{
				foreach (SpawnedStableData.SpawnData spawnedChestInfoDatum in spawnedStableData.SpawnedChestInfoData)
				{
					LoadItem(_StableFishGroups[spawnedChestInfoDatum.g]._AbilityChests[spawnedChestInfoDatum.p]._AssetPath, spawnedChestInfoDatum);
				}
				yield break;
			}
		}
		mObStatus.pIsReady = true;
	}

	public override void ActivateAbility()
	{
		base.ActivateAbility();
		if (!SpawnAvailable(_FishSpawnPoints) || !SpawnAvailable(_ChestSpawnPoints))
		{
			return;
		}
		foreach (StableFish item in mSpawnedFish)
		{
			UnityEngine.Object.Destroy(item.pFishReference);
		}
		foreach (AbilityChestInfo mSpawnedChestInfo in mSpawnedChestInfos)
		{
			UnityEngine.Object.Destroy(mSpawnedChestInfo.ChestReference.gameObject);
		}
		mSpawnedChestInfos.Clear();
		mSpawnedFish.Clear();
		mItemsToLoadCount = 0;
		mLoadedItemsCount = 0;
		mSpawnedStableData = new SpawnedStableData();
		int num = 0;
		foreach (StableFishGroup stableFishGroup2 in _StableFishGroups)
		{
			num += stableFishGroup2._Weight;
		}
		float num2 = UnityEngine.Random.Range(1, num + 1);
		StableFishGroup stableFishGroup = null;
		foreach (StableFishGroup stableFishGroup3 in _StableFishGroups)
		{
			if (num2 <= (float)stableFishGroup3._Weight)
			{
				stableFishGroup = stableFishGroup3;
				break;
			}
			num2 -= (float)stableFishGroup3._Weight;
		}
		if (stableFishGroup == null)
		{
			stableFishGroup = _StableFishGroups[0];
		}
		int num3 = _StableFishGroups.IndexOf(stableFishGroup);
		int num4 = UnityEngine.Random.Range((int)stableFishGroup._FishSpawnAmount.Min, (int)Mathf.Clamp(stableFishGroup._FishSpawnAmount.Max, stableFishGroup._FishSpawnAmount.Max, _FishSpawnPoints.Count));
		List<int> list = new List<int>();
		for (int i = 0; i <= _FishSpawnPoints.Count - 1; i++)
		{
			list.Add(i);
		}
		for (int j = 0; j < num4; j++)
		{
			int num5 = UnityEngine.Random.Range(0, stableFishGroup._FishAssetPaths.Count);
			int num6 = list[UnityEngine.Random.Range(0, list.Count)];
			SpawnedStableData.SpawnData spawnData = new SpawnedStableData.SpawnData(num3, num5, num6);
			mSpawnedStableData.SpawnedFishData.Add(spawnData);
			LoadItem(_StableFishGroups[num3]._FishAssetPaths[num5], spawnData);
			list.Remove(num6);
		}
		num2 = UnityEngine.Random.Range(1, 100);
		if (num2 <= (float)stableFishGroup._ChestSpawnChance)
		{
			int num7 = 0;
			foreach (AbilityChestInfo abilityChest in stableFishGroup._AbilityChests)
			{
				num7 += (int)abilityChest._SpawnChance;
			}
			AbilityChestInfo abilityChestInfo = null;
			num2 = UnityEngine.Random.Range(1, num7 + 1);
			foreach (AbilityChestInfo abilityChest2 in stableFishGroup._AbilityChests)
			{
				if (num2 <= abilityChest2._SpawnChance)
				{
					abilityChestInfo = abilityChest2;
					break;
				}
				num2 -= abilityChest2._SpawnChance;
			}
			if (abilityChestInfo == null)
			{
				abilityChestInfo = stableFishGroup._AbilityChests[stableFishGroup._AbilityChests.Count - 1];
			}
			int num8 = stableFishGroup._AbilityChests.IndexOf(abilityChestInfo);
			int inT = UnityEngine.Random.Range(0, _ChestSpawnPoints.Count);
			SpawnedStableData.SpawnData spawnData2 = new SpawnedStableData.SpawnData(num3, num8, inT);
			mSpawnedStableData.SpawnedChestInfoData.Add(spawnData2);
			LoadItem(_StableFishGroups[num3]._AbilityChests[num8]._AssetPath, spawnData2);
		}
		string inValue = JsonConvert.SerializeObject(mSpawnedStableData);
		base.pPairData.SetValue(_PairKey, inValue);
		StableAbilityManager.pInstance.SaveAbility(this);
		ShowCutScene();
	}

	private bool SpawnAvailable(List<Transform> inSpawnPoints)
	{
		if (inSpawnPoints == null)
		{
			UtDebug.LogError("inSpawnPoints is null. Please assign in the inspector.");
			return false;
		}
		if (inSpawnPoints.Contains(null))
		{
			UtDebug.LogError("Spawn point index " + inSpawnPoints.IndexOf(null) + " is null in inSpawnPoints");
			return false;
		}
		return true;
	}

	private void LoadItem(string inURL, SpawnedStableData.SpawnData spawnData)
	{
		string[] array = inURL.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], SpawnItem, typeof(GameObject), inDontDestroy: false, spawnData);
	}

	private void SpawnItem(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inSpawnData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			if (gameObject == null)
			{
				break;
			}
			MysteryChest component = gameObject.GetComponent<MysteryChest>();
			gameObject.name = inURL;
			SpawnedStableData.SpawnData spawnData = (SpawnedStableData.SpawnData)inSpawnData;
			if (component != null)
			{
				AbilityChestInfo abilityChestInfo = new AbilityChestInfo();
				MysteryBoxStoreInfo storeInfo = _StableFishGroups[spawnData.g]._AbilityChests[spawnData.p]._StoreInfoList[UnityEngine.Random.Range(0, _StableFishGroups[spawnData.g]._AbilityChests[spawnData.p]._StoreInfoList.Length)];
				abilityChestInfo.ChestSpawnData = spawnData;
				abilityChestInfo.ChestReference = component;
				gameObject.transform.position = _ChestSpawnPoints[spawnData.t].position;
				mSpawnedChestInfos.Add(abilityChestInfo);
				component.Init(UnityEngine.Object.FindObjectOfType<MysteryChestManager>(), storeInfo, ChestType.MysteryChest, OpenChest);
			}
			else
			{
				StableFish stableFish = new StableFish();
				stableFish.pFishReference = gameObject;
				stableFish.FishSpawnData = spawnData;
				gameObject.transform.position = _FishSpawnPoints[spawnData.t].position;
				if (!gameObject.GetComponent<ObCollect>())
				{
					UtDebug.LogWarning("No ObCollect component found on spawned fish. Adding one now...");
					gameObject.AddComponent<ObCollect>()._Sound = _CollectSound;
				}
				if (!gameObject.GetComponent<SphereCollider>())
				{
					UtDebug.LogWarning("No sphere collider found on spawned fish. Adding one now...");
					gameObject.AddComponent<SphereCollider>().isTrigger = true;
				}
				gameObject.GetComponent<ObCollect>()._MessageObject = base.gameObject;
				mSpawnedFish.Add(stableFish);
			}
			mLoadedItemsCount++;
			mObStatus.pIsReady = mItemsToLoadCount == mLoadedItemsCount;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Failed to load item, skipping. Failed URL: " + inURL);
			mLoadedItemsCount++;
			mObStatus.pIsReady = mItemsToLoadCount == mLoadedItemsCount;
			break;
		}
	}

	public override bool CanActivate()
	{
		return mSpawnedStableData.SpawnedChestInfoData.Count + mSpawnedStableData.SpawnedFishData.Count == 0;
	}

	public void OpenChest(MysteryChest inChest)
	{
		AbilityChestInfo chestInfo = pSpawnedChestInfos.Find((AbilityChestInfo t) => t.ChestReference == inChest);
		if (chestInfo != null)
		{
			mSpawnedStableData.SpawnedChestInfoData.Remove(mSpawnedStableData.SpawnedChestInfoData.Find((SpawnedStableData.SpawnData t) => t.g == chestInfo.ChestSpawnData.g && t.p == chestInfo.ChestSpawnData.p && t.t == chestInfo.ChestSpawnData.t));
			string inValue = JsonConvert.SerializeObject(mSpawnedStableData);
			base.pPairData.SetValueAndSave(_PairKey, inValue);
		}
	}

	public void Collect(GameObject inObject)
	{
		StableFish stableFish = mSpawnedFish.Find((StableFish t) => t.pFishReference == inObject);
		if (stableFish != null)
		{
			stableFish.pFishReference.SetActive(value: false);
			mSpawnedFish.Remove(stableFish);
			CommonInventoryData.pInstance.AddItem(stableFish.pItemID);
			CommonInventoryData.pInstance.Save(OnInventorySave, stableFish.FishSpawnData);
		}
	}

	private void OnInventorySave(bool success, object inUserData)
	{
		if (success)
		{
			SpawnedStableData.SpawnData spawnedFishData = (SpawnedStableData.SpawnData)inUserData;
			mSpawnedStableData.SpawnedFishData.Remove(mSpawnedStableData.SpawnedFishData.Find((SpawnedStableData.SpawnData t) => t.g == spawnedFishData.g && t.p == spawnedFishData.p && t.t == spawnedFishData.t));
			string inValue = JsonConvert.SerializeObject(mSpawnedStableData);
			base.pPairData.SetValueAndSave(_PairKey, inValue);
		}
	}
}
