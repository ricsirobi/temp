using System.Collections;
using UnityEngine;

namespace StableAbility;

public class KingOfDragonsAbility : BaseAbility
{
	public GameObject _GhostChest;

	public GameObject _ChestLocationVFX;

	public string _PairKey = "KingOfDragons";

	public MysteryChestInfo _MysteryChestInfo;

	public LocaleString _ServerErrorTitleText = new LocaleString("An Error Occurred");

	public LocaleString _ServerErrorBodyText = new LocaleString("Something went wrong while attempting to activate King of Dragons. Would you like to try again?");

	private ObStatus mObStatus;

	private GameObject mSpawnedObj;

	private bool mActivatedByPlayer;

	private bool mChestSpawned;

	public override void OnPairDataLoaded(bool success, PairData pData, object inUserData)
	{
		mObStatus = GetComponent<ObStatus>();
		base.OnPairDataLoaded(success, pData, inUserData);
		if (success)
		{
			Init();
		}
		else if ((bool)mObStatus)
		{
			mObStatus.pIsReady = true;
		}
	}

	private void Init()
	{
		if (CanActivate())
		{
			StopCoroutine(EnableGhostChestIfAvailable());
			StartCoroutine(EnableGhostChestIfAvailable());
			mObStatus.pIsReady = true;
		}
		else
		{
			mActivatedByPlayer = false;
			TryLoadChestFromBundle();
		}
	}

	public override void ActivateAbility()
	{
		if (CanActivate())
		{
			base.ActivateAbility();
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			KAUICursorManager.SetDefaultCursor("Loading");
			mActivatedByPlayer = true;
			TryLoadChestFromBundle();
		}
	}

	private void TryLoadChestFromBundle()
	{
		string[] array = _MysteryChestInfo._AssetPath.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnSpawnChest, typeof(GameObject));
	}

	private void OnSpawnChest(string inUrl, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (mActivatedByPlayer)
			{
				ShowCutScene();
			}
			CheckForDuplicateChestInScene();
			mSpawnedObj = Object.Instantiate((GameObject)inObject, _GhostChest.transform.position, _GhostChest.transform.rotation);
			if (!(mSpawnedObj == null))
			{
				MysteryChest component = mSpawnedObj.GetComponent<MysteryChest>();
				if (component != null)
				{
					component.Init(Object.FindObjectOfType<MysteryChestManager>(), _MysteryChestInfo._StoreInfoList[0], ChestType.MysteryChest, OnOpenChestCallback);
				}
				mObStatus.pIsReady = true;
				mChestSpawned = true;
				_ChestLocationVFX.SetActive(value: false);
				if (mActivatedByPlayer)
				{
					base.pPairData.SetValue(_PairKey, "false");
					StableAbilityManager.pInstance.SaveAbility(this);
				}
				StopCoroutine(EnableGhostChestIfAvailable());
				StartCoroutine(EnableGhostChestIfAvailable());
				KAUICursorManager.SetDefaultCursor("Arrow");
			}
			break;
		case RsResourceLoadEvent.ERROR:
			mObStatus.pIsReady = true;
			OpenAbilityFailedDB();
			Debug.LogError("Chest not found!");
			break;
		}
	}

	private void OpenAbilityFailedDB()
	{
		GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _ServerErrorBodyText.GetLocalizedString(), _ServerErrorTitleText.GetLocalizedString(), base.gameObject, "OnActivateAbilityRetry", "OnCloseDB", "", "", inDestroyOnClick: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	private void OnOpenChestCallback(MysteryChest mysteryChest)
	{
		base.pPairData.SetValueAndSave(_PairKey, "true");
		_ChestLocationVFX.SetActive(value: true);
		StopCoroutine(EnableGhostChestIfAvailable());
		StartCoroutine(EnableGhostChestIfAvailable());
		mChestSpawned = false;
		SetIdleActive();
	}

	private static void SetIdleActive()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	public override bool CanActivate()
	{
		return base.pPairData.GetBoolValue(_PairKey, defaultVal: true);
	}

	private void OnActivateAbilityRetry()
	{
		ActivateAbility();
	}

	private void OnCloseDB()
	{
		SetIdleActive();
	}

	private IEnumerator EnableGhostChestIfAvailable()
	{
		if (StableAbilityManager.pInstance.AbilityCooldownReady(this) || !mChestSpawned)
		{
			yield return null;
		}
		while (!StableAbilityManager.pInstance.AbilityCooldownReady(this) || mChestSpawned)
		{
			if (_GhostChest.activeInHierarchy)
			{
				_GhostChest.SetActive(value: false);
			}
			yield return new WaitForSeconds(1f);
		}
		CheckForDuplicateChestInScene();
		_GhostChest.SetActive(value: true);
	}

	private void CheckForDuplicateChestInScene()
	{
		if (mSpawnedObj != null)
		{
			Object.Destroy(mSpawnedObj);
		}
	}
}
