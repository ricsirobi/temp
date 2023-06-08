using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardRandomPet : KAMonoBase
{
	[Serializable]
	public class RewardPetInfo
	{
		public int _PetID;

		public int _PetTicketID;
	}

	public RewardPetInfo[] _RewardPets;

	public string _AnimationName = "IdleSit";

	public int _LevelAchievementID;

	public string _PairKey = "RewardPet";

	public RaisedPetStage _RewardAge;

	public int _PetActivateTaskID;

	public string _ActionName = "Collect";

	private int mRewardPetID = -1;

	private SanctuaryPet mCurrentPet;

	private PairData mAttributePair;

	private bool mActivatePet;

	public virtual void Start()
	{
		MissionManager.AddMissionEventHandler(OnMissionEvent);
		if (ProductData.pPairData.FindByKey(_PairKey) != null)
		{
			mRewardPetID = ProductData.pPairData.GetIntValue(_PairKey, mRewardPetID);
		}
		if (mRewardPetID == -1)
		{
			mRewardPetID = _RewardPets[UnityEngine.Random.Range(0, _RewardPets.Length - 1)]._PetID;
			ProductData.pPairData.SetValueAndSave(_PairKey, mRewardPetID.ToString());
		}
		StartCoroutine(CreatePet(mRewardPetID, _RewardAge, Gender.Male));
		CoCommonLevel component = GameObject.Find("PfCommonLevel").GetComponent<CoCommonLevel>();
		if (component != null && component.pWaitListCompleted)
		{
			WaitListCompleted();
		}
		else
		{
			CoCommonLevel.WaitListCompleted += WaitListCompleted;
		}
	}

	private void WaitListCompleted()
	{
		if (MissionManager.pInstance != null)
		{
			Task task = MissionManager.pInstance.GetTask(_PetActivateTaskID);
			if (task != null && task.pStarted && !task.pCompleted)
			{
				ActivatePet();
			}
		}
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (inEvent == MissionEvent.TASK_STARTED)
		{
			Task task = (Task)inObject;
			if (task != null && task.TaskID == _PetActivateTaskID)
			{
				ActivatePet();
				MissionManager.RemoveMissionEventHandler(OnMissionEvent);
			}
		}
	}

	private void ActivatePet()
	{
		if (mCurrentPet != null)
		{
			ObClickable componentInChildren = mCurrentPet.gameObject.GetComponentInChildren<ObClickable>();
			if (componentInChildren != null)
			{
				componentInChildren._Active = true;
			}
		}
		else
		{
			mActivatePet = true;
		}
	}

	private IEnumerator CreatePet(int inPetTypeID, RaisedPetStage inStage, Gender inGender)
	{
		while (SanctuaryData.pInstance == null)
		{
			yield return 0;
		}
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(inPetTypeID);
		_ = string.Empty;
		int ageIndex = RaisedPetData.GetAgeIndex(inStage);
		string resName = ((sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[0]._Gender != inGender) ? sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[1]._Prefab : sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[0]._Prefab);
		SanctuaryManager.CreatePet(RaisedPetData.InitDefault(inPetTypeID, inStage, resName, inGender, addToActivePets: false), Vector3.zero, Quaternion.identity, base.gameObject, "Full");
	}

	public void OnPetReady(SanctuaryPet pet)
	{
		if (pet != null)
		{
			mCurrentPet = pet;
			mCurrentPet.gameObject.transform.parent = base.transform;
			mCurrentPet.gameObject.transform.localPosition = Vector3.zero;
			mCurrentPet.gameObject.transform.localRotation = Quaternion.identity;
			mCurrentPet.pEnablePetAnim = true;
			mCurrentPet.PlayAnimation(_AnimationName, WrapMode.Loop);
			mCurrentPet.AIActor.SetState(AISanctuaryPetFSM.IDLE);
			ObClickable componentInChildren = mCurrentPet.gameObject.GetComponentInChildren<ObClickable>();
			if (componentInChildren != null)
			{
				componentInChildren._MessageObject = base.gameObject;
				componentInChildren._Active = mActivatePet;
			}
		}
	}

	private void OnClick(GameObject inGameObject)
	{
		if (!(mCurrentPet.gameObject == inGameObject))
		{
			return;
		}
		ObClickable componentInChildren = mCurrentPet.gameObject.GetComponentInChildren<ObClickable>();
		if (componentInChildren != null)
		{
			componentInChildren._Active = false;
		}
		RewardPetInfo rewardPetInfo = Array.Find(_RewardPets, (RewardPetInfo r) => r._PetID == mRewardPetID);
		if (rewardPetInfo == null)
		{
			return;
		}
		int petTicketID = rewardPetInfo._PetTicketID;
		if (CommonInventoryData.pInstance.FindItem(petTicketID) != null)
		{
			MarkMissionComplete(petTicketID);
			return;
		}
		CommonInventoryData.pInstance.AddItem(petTicketID, updateServer: false);
		mAttributePair = new PairData();
		mAttributePair.Init();
		mAttributePair.SetValue("LevelAchievement", _LevelAchievementID.ToString());
		mAttributePair.PrepareArray();
		CommonInventoryRequest commonInventoryRequest = CommonInventoryData.pInstance.FindInUpdateList(petTicketID);
		if (commonInventoryRequest != null)
		{
			commonInventoryRequest.UserItemAttributes = mAttributePair;
		}
		CommonInventoryData.pInstance.Save(InventorySaveEventHandler, petTicketID);
	}

	private void InventorySaveEventHandler(bool inSaveSuccess, object inUserData)
	{
		int num = (int)inUserData;
		UserItemData userItemData = CommonInventoryData.pInstance.FindItem(num);
		if (userItemData != null)
		{
			userItemData.UserItemAttributes = mAttributePair;
		}
		MarkMissionComplete(num);
	}

	private void MarkMissionComplete(int ticketID)
	{
		MissionManager.pInstance.CheckForTaskCompletion("Action", _ActionName, base.name);
		if (UserNotifyDragonTicket.pInstance != null)
		{
			List<int> list = new List<int>();
			list.Add(ticketID);
			UserNotifyDragonTicket.pInstance._ShouldShowDragonSelection = false;
			UserNotifyDragonTicket.pInstance.CheckTickets(list, OnUNDragonTicketDone);
		}
	}

	private void OnUNDragonTicketDone(bool inSuccess)
	{
		if (inSuccess)
		{
			mCurrentPet.gameObject.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		CoCommonLevel.WaitListCompleted -= WaitListCompleted;
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
	}
}
