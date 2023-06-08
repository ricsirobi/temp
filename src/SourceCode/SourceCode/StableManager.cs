using System.Collections.Generic;
using UnityEngine;

public class StableManager : MonoBehaviour
{
	public static StableData pCurrentStableData;

	public static bool mRefreshPets = false;

	public static int pCurIncubatorID = 0;

	public static int pCurrentStableID = 0;

	public static int pVisitNestID = -1;

	public static int _DragonEggsCategory = 456;

	public StableQuestTutorial _StableQuestTutorial;

	public GameObject _ActivePetMarker;

	public GameObject _AvatarMarker;

	public GameObject[] _NestMarkers;

	public GameObject[] _VisitMarkers;

	public GameObject _StableQuestJobBoardVisitMarker;

	public GameObject _StableQuestJobBoardObject;

	public HatcheryManager _HatcheryManager;

	public LocaleString _StableQuestFailedText = new LocaleString("Sorry, Stable quest couldn't be loaded");

	public LocaleString _StableQuestJobBoardInaccessableText = new LocaleString("You don't have enough pets to access this feature.");

	public static PetMovedToNest OnPetMovedToNest = null;

	private GameObject mStableQuestUIInstance;

	private bool mIsStableQuestUiLoading;

	private KAUIGenericDB mKAUIGenericDB;

	private static bool mDidUpdateActivePet = false;

	private static bool mIsLevelReady = false;

	private const int LOG_PRIORITY = 20;

	private List<SanctuaryPet> mPetsInScene = new List<SanctuaryPet>();

	private static StableManager mInstance = null;

	private static bool mIsJobBoardLoaded;

	public GameObject pStableQuestUIInstance => mStableQuestUIInstance;

	public bool mSubscribedToOnSlotStateStatus { get; private set; }

	public static StableManager pInstance => mInstance;

	public static void RefreshPets()
	{
		mRefreshPets = true;
	}

	public static void EnsureActivePetHasNest()
	{
		StableData.UpdateInfo();
		SanctuaryPet pCurPetInstance = SanctuaryManager.pCurPetInstance;
		if (pCurPetInstance == null || StableData.GetByPetID(pCurPetInstance.pData.RaisedPetID) != null)
		{
			return;
		}
		foreach (StableData pStable in StableData.pStableList)
		{
			NestData emptyNest = pStable.GetEmptyNest();
			if (emptyNest != null)
			{
				StableData.AddPetToNest(pStable.ID, emptyNest.ID, pCurPetInstance.pData.RaisedPetID);
				break;
			}
		}
	}

	public static void Init()
	{
		pCurrentStableData = null;
		pCurIncubatorID = 0;
		pCurrentStableID = 0;
		pVisitNestID = -1;
		EnsureActivePetHasNest();
	}

	private void OnDestroy()
	{
		pCurrentStableData = null;
		mIsLevelReady = false;
		mInstance = null;
		TimedMissionManager.pInstance.OnSlotStateStatus -= OnStableQuestComplete;
		mSubscribedToOnSlotStateStatus = false;
	}

	private void Start()
	{
		StableData.UpdateInfo();
		TimedMissionManager.pInstance.OnSlotStateStatus += OnStableQuestComplete;
		mSubscribedToOnSlotStateStatus = true;
		mInstance = this;
	}

	private void OnLevelReady()
	{
		mRefreshPets = true;
		if (_StableQuestJobBoardObject != null)
		{
			_StableQuestJobBoardObject.SetActive(TimedMissionManager.pInstance.pIsEnabled);
		}
		pCurrentStableData = StableData.GetByID(pCurrentStableID);
		SanctuaryManager.ResetToActivePet();
		if (pCurrentStableData != null)
		{
			List<NestData> nestList = pCurrentStableData.NestList;
			if (nestList != null)
			{
				for (int i = 0; i < nestList.Count; i++)
				{
					if (_NestMarkers.Length <= i)
					{
						continue;
					}
					GameObject gameObject = _NestMarkers[i];
					if (gameObject != null)
					{
						ObClickableNest componentInChildren = gameObject.GetComponentInChildren<ObClickableNest>();
						if (componentInChildren != null)
						{
							componentInChildren.pNestID = i;
						}
					}
				}
			}
		}
		if (!mIsJobBoardLoaded)
		{
			if (pVisitNestID >= 0 && _VisitMarkers != null && pVisitNestID < _VisitMarkers.Length)
			{
				AvAvatar.TeleportToObject(_VisitMarkers[pVisitNestID]);
			}
		}
		else
		{
			mIsJobBoardLoaded = false;
			if (_StableQuestJobBoardVisitMarker != null)
			{
				AvAvatar.TeleportToObject(_StableQuestJobBoardVisitMarker);
			}
		}
		mDidUpdateActivePet = false;
		mIsLevelReady = true;
	}

	private void OnClick()
	{
		OpenStableQuest();
	}

	private void OpenStableQuestUIOnClose()
	{
		UiDragonsStable.LoadDragonsStableUI();
	}

	public void OpenStableQuest()
	{
		int num = 0;
		foreach (RaisedPetData[] value in RaisedPetData.pActivePets.Values)
		{
			if (value == null)
			{
				continue;
			}
			RaisedPetData[] array = value;
			foreach (RaisedPetData raisedPetData in array)
			{
				if (raisedPetData.pStage >= RaisedPetStage.BABY && raisedPetData.IsPetCustomized() && StableData.GetByPetID(raisedPetData.RaisedPetID) != null && !SanctuaryManager.IsPetLocked(raisedPetData))
				{
					num++;
				}
			}
		}
		if (num < 2)
		{
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _StableQuestJobBoardInaccessableText.GetLocalizedString(), base.gameObject, "OnDBClose");
		}
		else if (mStableQuestUIInstance == null && !mIsStableQuestUiLoading)
		{
			mIsStableQuestUiLoading = true;
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array2 = GameConfig.GetKeyData("StableQuestUIAsset").Split('/');
			RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], ResourceEventHandler, typeof(GameObject));
		}
	}

	private void ResourceEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mIsStableQuestUiLoading = false;
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (inObject != null)
			{
				mStableQuestUIInstance = Object.Instantiate((GameObject)inObject);
				RsResourceManager.ReleaseBundleData(inURL);
				if (_StableQuestTutorial != null && !_StableQuestTutorial.TutorialComplete() && !_StableQuestTutorial.IsShowingTutorial())
				{
					_StableQuestTutorial.gameObject.SetActive(value: true);
					_StableQuestTutorial.ShowTutorial();
				}
			}
			else
			{
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _StableQuestFailedText.GetLocalizedString(), base.gameObject, "OnDBClose");
			}
			break;
		case RsResourceLoadEvent.ERROR:
			mIsStableQuestUiLoading = false;
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _StableQuestFailedText.GetLocalizedString(), base.gameObject, "OnDBClose");
			break;
		}
	}

	private void Update()
	{
		if (!mIsLevelReady || (!mRefreshPets && mDidUpdateActivePet))
		{
			return;
		}
		SanctuaryPet pCurPetInstance = SanctuaryManager.pCurPetInstance;
		if (pCurPetInstance == null)
		{
			return;
		}
		if (pCurPetInstance.pIsMounted)
		{
			pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
		}
		mRefreshPets = false;
		mDidUpdateActivePet = true;
		pCurrentStableData = StableData.GetByID(pCurrentStableID);
		bool flag = false;
		if (mPetsInScene.Count > 0)
		{
			for (int i = 0; i < mPetsInScene.Count; i++)
			{
				Object.Destroy(mPetsInScene[i].gameObject);
			}
			mPetsInScene.Clear();
		}
		if (pCurrentStableData == null)
		{
			return;
		}
		List<NestData> nestList = pCurrentStableData.NestList;
		if (nestList != null)
		{
			UtDebug.Log("Moving pet to nest...", 20);
			for (int j = 0; j < nestList.Count; j++)
			{
				if (_NestMarkers.Length <= j)
				{
					continue;
				}
				GameObject gameObject = _NestMarkers[j];
				if (!(gameObject != null))
				{
					continue;
				}
				ObClickableNest componentInChildren = gameObject.GetComponentInChildren<ObClickableNest>();
				if (!componentInChildren)
				{
					continue;
				}
				int pNestID = componentInChildren.pNestID;
				if (nestList[pNestID].PetID != 0)
				{
					RaisedPetData byID = RaisedPetData.GetByID(nestList[pNestID].PetID);
					if (byID == null)
					{
						continue;
					}
					if (byID.RaisedPetID == pCurPetInstance.pData.RaisedPetID)
					{
						flag = true;
					}
					if ((bool)componentInChildren.pCurrPet)
					{
						if (componentInChildren.pCurrPet.pData.RaisedPetID != byID.RaisedPetID || componentInChildren.pCurrPet.pAge != RaisedPetData.GetAgeIndex(byID.pStage))
						{
							if (componentInChildren.pCurrPet.pData.RaisedPetID == pCurPetInstance.pData.RaisedPetID)
							{
								componentInChildren.pCurrPet = null;
							}
							else
							{
								componentInChildren.RemovePet();
							}
						}
						else if (componentInChildren.pCurrPet.pData.RaisedPetID == pCurPetInstance.pData.RaisedPetID && componentInChildren.pCurrPet != pCurPetInstance)
						{
							componentInChildren.RemovePet();
						}
					}
					if (byID.RaisedPetID == pCurPetInstance.pData.RaisedPetID)
					{
						componentInChildren.AssignSanctuaryCurrentPet(pCurPetInstance);
					}
					else if (TimedMissionManager.pInstance != null && TimedMissionManager.pInstance.IsPetEngaged(nestList[pNestID].PetID))
					{
						componentInChildren.RemovePet();
						if (!componentInChildren._BusyStableQuestSign.activeSelf)
						{
							componentInChildren._BusyStableQuestSign.SetActive(value: true);
						}
						if (componentInChildren._Active)
						{
							componentInChildren._Active = false;
						}
					}
					else
					{
						if (componentInChildren._BusyStableQuestSign.activeSelf)
						{
							componentInChildren._BusyStableQuestSign.SetActive(value: false);
						}
						if (!componentInChildren._Active)
						{
							componentInChildren._Active = true;
						}
						componentInChildren.CreatePet(byID);
					}
				}
				else if (componentInChildren.pCurrPet != null && componentInChildren.pCurrPet.pData.RaisedPetID == pCurPetInstance.pData.RaisedPetID)
				{
					componentInChildren.pCurrPet = null;
				}
				else
				{
					componentInChildren.RemovePet();
				}
			}
		}
		if (!flag && _ActivePetMarker != null)
		{
			UtDebug.Log("Moving active pet to marker at " + _ActivePetMarker.transform.position.ToString(), 20);
			pCurPetInstance.transform.position = _ActivePetMarker.transform.position;
			pCurPetInstance.transform.rotation = _ActivePetMarker.transform.rotation;
			pCurPetInstance.pEnablePetAnim = true;
			pCurPetInstance.SetFollowAvatar(follow: false);
			pCurPetInstance.SetState(Character_State.idle);
			pCurPetInstance.AIActor.SetState(AISanctuaryPetFSM.IDLE);
			pCurPetInstance.PlayAnimation(pCurPetInstance._AnimNameIdle, WrapMode.Loop);
			pCurPetInstance.EnablePettingCollider(t: false);
		}
	}

	private void OnStableQuestComplete(TimedMissionState FromState, TimedMissionState ToState)
	{
		if (ToState == TimedMissionState.Started || ToState == TimedMissionState.Ended || ToState == TimedMissionState.Alotted || ToState == TimedMissionState.None)
		{
			mRefreshPets = true;
		}
	}

	private void OnPetReady(SanctuaryPet pet)
	{
		pet.pEnablePetAnim = true;
		pet.SetAvatar(null);
		pet.SetFollowAvatar(follow: false);
		pet.PlayAnimation(pet._AnimNameIdle, WrapMode.Loop);
		pet.AIActor.SetState(AISanctuaryPetFSM.IDLE);
		mPetsInScene.Add(pet);
		UtDebug.Log("Created pet " + pet.name, 20);
	}

	private void DismissDialogBox()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		Object.Destroy(mKAUIGenericDB.gameObject);
		mKAUIGenericDB = null;
	}

	public static void LoadStable(int stableID, int nestID = -1)
	{
		StableData byID = StableData.GetByID(stableID);
		if (byID != null)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(byID.ItemID);
			if (userItemData == null)
			{
				userItemData = ParentData.pInstance.pInventory.pData.FindItem(byID.ItemID);
			}
			if (userItemData != null)
			{
				string assetName = userItemData.Item.AssetName;
				if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
				{
					SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
				}
				pCurrentStableID = stableID;
				pVisitNestID = nestID;
				RsResourceManager.LoadLevel(assetName);
				return;
			}
		}
		UtDebug.LogError("Unable to load stable scene for " + byID.pLocaleName, 20);
	}

	public static void LoadStableWithJobBoard(int stableID)
	{
		mIsJobBoardLoaded = true;
		LoadStable(stableID);
	}

	public static void MovePetToNest(int stableID, int nestID, int petID)
	{
		bool flag = false;
		RaisedPetData petInNest = StableData.GetPetInNest(stableID, nestID);
		if (petInNest != null)
		{
			StableData byPetID = StableData.GetByPetID(petID);
			if (byPetID != null)
			{
				NestData nestData = byPetID.NestList.Find((NestData nd) => nd.PetID == petID);
				if (nestData != null)
				{
					if (byPetID.ID == pCurrentStableID)
					{
						flag = true;
					}
					nestData.PetID = petInNest.RaisedPetID;
				}
			}
		}
		StableData byPetID2 = StableData.GetByPetID(petID);
		flag |= byPetID2 != null && byPetID2.ID == pCurrentStableID;
		StableData.AddPetToNest(stableID, nestID, petID);
		if (OnPetMovedToNest != null)
		{
			OnPetMovedToNest(petID);
		}
		if (stableID == pCurrentStableID || flag)
		{
			mRefreshPets = true;
		}
	}

	public static void RefreshActivePet()
	{
		mDidUpdateActivePet = false;
	}

	public static bool IsPlayerHasEggs()
	{
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(_DragonEggsCategory);
		if (items != null && items.Length != 0)
		{
			return true;
		}
		return false;
	}

	public void RefreshMembership()
	{
		mRefreshPets = true;
		for (int i = 0; i < _NestMarkers.Length; i++)
		{
			if (!(_NestMarkers[i] == null))
			{
				ObClickableNest componentInChildren = _NestMarkers[i].GetComponentInChildren<ObClickableNest>();
				if (componentInChildren != null && componentInChildren._NonMemberSign != null)
				{
					componentInChildren._NonMemberSign.SetActive(!SubscriptionInfo.pIsMember);
				}
			}
		}
	}

	public void UnSubscribeFromOnSlotStateStatusChange()
	{
		if (mSubscribedToOnSlotStateStatus)
		{
			TimedMissionManager.pInstance.OnSlotStateStatus -= OnStableQuestComplete;
			mSubscribedToOnSlotStateStatus = false;
		}
	}

	public void SubscribeToOnSlotStateStatusChange()
	{
		if (!mSubscribedToOnSlotStateStatus)
		{
			TimedMissionManager.pInstance.OnSlotStateStatus += OnStableQuestComplete;
			mSubscribedToOnSlotStateStatus = true;
		}
	}
}
