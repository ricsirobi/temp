using System;
using System.Collections.Generic;
using UnityEngine;

public class HatcheryManager : MMOClient
{
	public string _DefaultEggAssetName = "RS_DATA/DragonEgg.unity3d/PfDragonEgg";

	public string _AttachBone = "Shoulders_J";

	public Vector3 _EggPosition = new Vector3(-0.006f, -0.542f, 0.353f);

	[Tooltip("Ticket items to purchase locked Incubator slots in Ascending order of Item cost")]
	[SerializeField]
	private int[] m_SlotPurchaseItems;

	[SerializeField]
	private int m_IncubatorSlotTicketCategoryID = 690;

	[SerializeField]
	private UiMultiEggHatching m_UiMultiEggHatching;

	[SerializeField]
	private int m_IncubatorStartSlotIndex = 1;

	[NonSerialized]
	public GameObject pEgg;

	public bool IsFromStables;

	private List<HatcheryEggLoader> mEggLoaderList = new List<HatcheryEggLoader>();

	[SerializeField]
	private List<Incubator> m_Incubators;

	public Action OnSlotUnlockCostUpdated;

	public static int pIncubatorHatchID;

	public int pIncubatorStartSlotIndex => m_IncubatorStartSlotIndex;

	public List<Incubator> pIncubators => m_Incubators;

	public int pSlotUnlockCost { get; private set; }

	private void OnClick(GameObject clickObject)
	{
		Init();
	}

	public void Init()
	{
		m_UiMultiEggHatching.Init(this);
	}

	public void UpdateIncubatorSlotCosts(Action onSlotUnlockcostUpdated = null)
	{
		if (pIncubators != null && pIncubators.Count > 0)
		{
			UpdateSlotUnlockCost(OnSlotCostUpdate);
		}
		if (onSlotUnlockcostUpdated != null)
		{
			OnSlotUnlockCostUpdated = (Action)Delegate.Combine(OnSlotUnlockCostUpdated, onSlotUnlockcostUpdated);
		}
	}

	public void UpdateSlotUnlockCost(Action onSlotUnlockcostUpdated = null)
	{
		if (onSlotUnlockcostUpdated != null)
		{
			OnSlotUnlockCostUpdated = (Action)Delegate.Combine(OnSlotUnlockCostUpdated, onSlotUnlockcostUpdated);
		}
		pSlotUnlockCost = 0;
		int nextIncubatorUnlockItem = GetNextIncubatorUnlockItem();
		if (nextIncubatorUnlockItem > 0)
		{
			WsWebService.GetItemData(nextIncubatorUnlockItem, OnItemDataLoaded, null);
		}
	}

	public int GetNextIncubatorUnlockItem()
	{
		int result = -1;
		int currentUnlockedIncubatorCount = GetCurrentUnlockedIncubatorCount();
		if (currentUnlockedIncubatorCount < m_SlotPurchaseItems.Length)
		{
			result = m_SlotPurchaseItems[currentUnlockedIncubatorCount];
		}
		else if (currentUnlockedIncubatorCount >= m_SlotPurchaseItems.Length)
		{
			result = m_SlotPurchaseItems[m_SlotPurchaseItems.Length - 1];
		}
		return result;
	}

	public int GetCurrentUnlockedIncubatorCount()
	{
		int num = 0;
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(m_IncubatorSlotTicketCategoryID);
		if (items != null && items.Length != 0)
		{
			for (int i = 0; i < items.Length; i++)
			{
				num += items[i].Quantity;
			}
		}
		if (ParentData.pIsReady)
		{
			UserItemData[] items2 = ParentData.pInstance.pInventory.pData.GetItems(m_IncubatorSlotTicketCategoryID);
			if (items2 != null && items2.Length != 0)
			{
				for (int j = 0; j < items2.Length; j++)
				{
					num += items2[j].Quantity;
				}
			}
		}
		return num;
	}

	private void OnItemDataLoaded(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			ItemData itemData = (ItemData)inObject;
			pSlotUnlockCost = itemData.CashCost;
			OnSlotUnlockCostUpdated?.Invoke();
			break;
		}
		case WsServiceEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			OnSlotUnlockCostUpdated?.Invoke();
			break;
		}
	}

	private void OnSlotCostUpdate()
	{
		OnSlotUnlockCostUpdated = (Action)Delegate.Remove(OnSlotUnlockCostUpdated, new Action(OnSlotCostUpdate));
		SetupIncubators();
	}

	public void SetupIncubators()
	{
		pIncubators.Sort(new IncubatorListSort());
		for (int i = 0; i < pIncubators.Count; i++)
		{
			pIncubators[i].Setup(GetCurrentUnlockedIncubatorCount(), m_UiMultiEggHatching._HatcheryManager != null || pIncubators[i].mPickedUpEgg != null);
			pIncubators[i]._ObProximityStableHatch?.CheckIncubatorState(pIncubators[i]);
		}
	}

	public Incubator GetCurrentIncubator()
	{
		return m_Incubators.Find((Incubator a) => a.pID == StableManager.pCurIncubatorID);
	}

	public void AttachDragonEgg(int inType, GameObject inPlayer, GameObject inReceiver, bool mIsInStables = false)
	{
		IsFromStables = mIsInStables;
		string assetName = GetAssetName(inType);
		HatcheryEggLoader hatcheryEggLoader = new HatcheryEggLoader(this);
		hatcheryEggLoader._InObject = inPlayer;
		hatcheryEggLoader._InReceiver = inReceiver;
		hatcheryEggLoader._EggPosition = _EggPosition;
		mEggLoaderList.Add(hatcheryEggLoader);
		if (!string.IsNullOrEmpty(assetName))
		{
			string[] array = assetName.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], hatcheryEggLoader.OnAssetLoadEvent, typeof(GameObject));
		}
	}

	private string GetAssetName(int inType)
	{
		return SanctuaryData.FindSanctuaryPetTypeInfo(inType)?._DragonEggAssetpath;
	}

	public void OnWaitListCompleted()
	{
		if (pIncubatorHatchID > 0)
		{
			Incubator incubator = pIncubators?.Find((Incubator t) => t.pID == pIncubatorHatchID);
			if (!incubator)
			{
				pIncubatorHatchID = 0;
				return;
			}
			incubator.pMyPetData = RaisedPetData.GetHatchingPet(pIncubatorHatchID);
			pIncubatorHatchID = 0;
			if (incubator.pMyPetData != null)
			{
				SetupIncubators();
				incubator.mPetType = incubator.pMyPetData.PetTypeID;
				incubator.PickUpEgg(fromStable: false);
			}
		}
		else
		{
			UpdateIncubatorSlotCosts();
		}
	}

	public override void RemovePlayer(MMOAvatar avatar)
	{
		foreach (HatcheryEggLoader mEggLoader in mEggLoaderList)
		{
			if (avatar.gameObject == mEggLoader._InObject)
			{
				mEggLoaderList.Remove(mEggLoader);
				break;
			}
		}
	}
}
