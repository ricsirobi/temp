using System;
using System.Collections.Generic;
using UnityEngine;

public class UiDragonSelect : KAUI
{
	[Serializable]
	public class DragonData
	{
		public int _PetTypeId;

		public Gender _DragonGender = Gender.Male;

		public RaisedPetStage _PetStage;

		public int _ItemId;

		public KAWidget _Widget;
	}

	public class DragonWidgetData : KAWidgetUserData
	{
		public SanctuaryPetTypeInfo _PetInfo;

		public DragonWidgetData(SanctuaryPetTypeInfo info)
		{
			_PetInfo = info;
		}
	}

	public List<DragonData> _DragonData;

	public string _DragonCustomizationAsset = "RS_DATA/PfUiDragonCustomizationDO.unity3d/PfUiDragonCustomizationDO";

	private DragonData mSelectedDragon;

	private RaisedPetData mSelectedPetData;

	public GameObject _MessageObject;

	public string _CloseMessage;

	private Task mCurrentTask;

	private string mNPCName;

	protected override void Start()
	{
		base.Start();
		KAUI.SetExclusive(this);
		if (_DragonData != null && _DragonData.Count > 0)
		{
			Init();
		}
	}

	public virtual void Init()
	{
		AvAvatar.pInputEnabled = false;
		foreach (DragonData dragonDatum in _DragonData)
		{
			DragonWidgetData userData = new DragonWidgetData(SanctuaryData.FindSanctuaryPetTypeInfo(dragonDatum._PetTypeId));
			dragonDatum._Widget.SetUserData(userData);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == _BackButtonName)
		{
			Exit();
		}
		else if (inWidget.name == "BtnOK")
		{
			if (mSelectedDragon != null)
			{
				SanctuaryManager.pCurPetInstance?.OnFlyDismount(AvAvatar.pObject);
				SetInteractive(interactive: false);
				KAUICursorManager.SetDefaultCursor("Loading");
				if (mSelectedDragon._ItemId != 0)
				{
					CommonInventoryData.pInstance.AddItem(mSelectedDragon._ItemId);
					CommonInventoryData.pInstance.Save(InventorySaveEventHandler, mSelectedDragon);
				}
				else
				{
					AwardDragon(mSelectedDragon);
				}
			}
		}
		else
		{
			DragonData dragonData = _DragonData.Find((DragonData a) => a._Widget == inWidget);
			if (dragonData != null)
			{
				mSelectedDragon = dragonData;
			}
		}
	}

	public virtual void Exit()
	{
		if (mCurrentTask != null && !string.IsNullOrEmpty(mNPCName))
		{
			mCurrentTask.CheckForCompletion("Meet", mNPCName, "", "");
		}
		if (_MessageObject != null && !string.IsNullOrEmpty(_CloseMessage))
		{
			_MessageObject.SendMessage(_CloseMessage, SendMessageOptions.DontRequireReceiver);
		}
		if ((bool)SanctuaryManager.pCurPetInstance)
		{
			SanctuaryManager.pCurPetInstance.gameObject.SetActive(value: true);
			SanctuaryManager.pCurPetInstance.SetAvatar(AvAvatar.mTransform);
			SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: true);
			SanctuaryManager.pCurPetInstance.MoveToAvatar(postponed: true);
		}
		AvAvatar.pInputEnabled = true;
		KAUI.RemoveExclusive(this);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void InventorySaveEventHandler(bool success, object inUserData)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		DragonData dragonData = (DragonData)inUserData;
		if (success)
		{
			UtDebug.Log("Awarded ticket:: " + dragonData._ItemId);
		}
		else
		{
			UtDebug.LogError("Failed to award ticket:: " + dragonData._ItemId);
		}
		if (mSelectedDragon._PetStage < RaisedPetStage.BABY)
		{
			Exit();
		}
		else
		{
			AwardDragon(mSelectedDragon);
		}
	}

	public virtual void AwardDragon(DragonData selectedDragon)
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		RaisedPetData petData = RaisedPetData.InitDefault(selectedDragon._PetTypeId, selectedDragon._PetStage, "", selectedDragon._DragonGender, addToActivePets: false);
		SanctuaryPetTypeInfo petInfo = ((DragonWidgetData)selectedDragon._Widget.GetUserData())._PetInfo;
		SantuayPetResourceInfo[] array = Array.FindAll(petInfo._AgeData[RaisedPetData.GetAgeIndex(selectedDragon._PetStage)]._PetResList, (SantuayPetResourceInfo p) => p._Gender == petData.Gender);
		int num = UnityEngine.Random.Range(0, array.Length);
		SantuayPetResourceInfo santuayPetResourceInfo = array[num];
		petData.Geometry = santuayPetResourceInfo._Prefab;
		petData.SetState(selectedDragon._PetStage, savedata: true);
		petData.Texture = null;
		PetSkillRequirements[] skillsRequired = petInfo._AgeData[RaisedPetData.GetAgeIndex(selectedDragon._PetStage)]._SkillsRequired;
		foreach (PetSkillRequirements petSkillRequirements in skillsRequired)
		{
			petData.UpdateSkillData(petSkillRequirements._Skill.ToString(), 0f, save: false);
		}
		if (selectedDragon._ItemId > 0)
		{
			petData.SetAttrData("TicketID", selectedDragon._ItemId.ToString(), DataType.INT);
		}
		if (!SanctuaryData.IsNameChangeAllowed(petData))
		{
			petData.Name = SanctuaryData.GetPetDefaultName(petData);
			petData.pIsNameCustomized = true;
		}
		if (!SanctuaryData.IsColorChangeAllowed(petData))
		{
			Color[] petDefaultColors = SanctuaryData.GetPetDefaultColors(petData);
			petData.SetColors(petDefaultColors);
		}
		if (SanctuaryData.GetPetCustomizationType(petData) == PetCustomizationType.None)
		{
			RaisedPetData raisedPetData = petData;
			int i = (int)petData.pStage;
			raisedPetData.SetAttrData("_LastCustomizedStage", i.ToString() ?? "", DataType.INT);
		}
		petData.UserID = new Guid(UserInfo.pInstance.UserID);
		petData.SetupSaveData();
		RaisedPetData.CreateNewPet(selectedDragon._PetTypeId, setAsSelectedPet: true, unselectOtherPets: true, petData, null, RaisedPetCreateCallback, null);
	}

	private void RaisedPetCreateCallback(int ptype, RaisedPetData pdata, object inUserData)
	{
		if (pdata == null || !pdata.IsPetCreated)
		{
			UtDebug.LogError("Dragon or Egg Create failed");
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", SanctuaryManager.pInstance._CreatePetFailureText.GetLocalizedString(), base.gameObject, "OnCreatePetFailOK");
			KAUICursorManager.SetDefaultCursor("Arrow");
			return;
		}
		if (FUEManager.pIsFUERunning)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("name", SanctuaryData.FindSanctuaryPetTypeInfo(ptype)._Name ?? string.Empty);
			AnalyticAgent.LogFTUEEvent(FTUEEvent.DRAGON_SELECTED, dictionary);
		}
		SanctuaryManager.SetAndSaveCurrentType(pdata.PetTypeID);
		SanctuaryManager.pCurPetData = pdata;
		SanctuaryManager.CreatePet(pdata, Vector3.zero, Quaternion.identity, base.gameObject, "Full");
	}

	public void OnCreatePetFailOK()
	{
		Exit();
	}

	public virtual void SetPetHandler(bool success)
	{
		if (success)
		{
			SanctuaryManager.SetAndSaveCurrentType(mSelectedPetData.PetTypeID);
			SanctuaryManager.pCurPetData = mSelectedPetData;
			if (SanctuaryManager.pInstance._CreateInstance)
			{
				if (SanctuaryManager.pCurPetInstance != null)
				{
					UnityEngine.Object.Destroy(SanctuaryManager.pCurPetInstance.gameObject);
					SanctuaryManager.pCurPetInstance = null;
				}
				SanctuaryManager.pInstance.ReloadPet(resetFollowFlag: true, base.gameObject);
			}
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	private void OnPetReady(SanctuaryPet pet)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (SanctuaryManager.pCurPetInstance != null)
		{
			UnityEngine.Object.Destroy(SanctuaryManager.pCurPetInstance.gameObject);
		}
		SanctuaryManager.pCurPetInstance = pet;
		if (SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.OnPetReady(pet);
		}
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryData.GetPetCustomizationType(pet.pData.PetTypeID) == PetCustomizationType.None)
		{
			SanctuaryManager.pCurPetInstance.gameObject.SetActive(value: true);
			SanctuaryManager.pCurPetInstance.SetAvatar(AvAvatar.mTransform);
			SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: true);
			SanctuaryManager.pCurPetInstance.MoveToAvatar(postponed: true);
			if (SanctuaryManager.pInstance != null)
			{
				SanctuaryManager.pInstance.TakePicture(SanctuaryManager.pCurPetInstance.gameObject);
			}
		}
		StableData.UpdateInfo();
		if (StableData.GetByPetID(SanctuaryManager.pCurPetData.RaisedPetID) == null)
		{
			for (int i = 0; i < StableData.pStableList.Count; i++)
			{
				StableData stableData = StableData.pStableList[i];
				NestData emptyNest = stableData.GetEmptyNest();
				if (emptyNest != null)
				{
					StableData.AddPetToNest(stableData.ID, emptyNest.ID, SanctuaryManager.pCurPetInstance.pData.RaisedPetID);
					break;
				}
			}
		}
		if (PlayfabManager<PlayFabManagerDO>.Instance != null)
		{
			PlayfabManager<PlayFabManagerDO>.Instance.UpdateCharacterStatistics("Dragons", RaisedPetData.GetActiveDragons().Count);
		}
		if (SanctuaryData.GetPetCustomizationType(pet.pData.PetTypeID) != PetCustomizationType.None)
		{
			StartDragonCustomization();
		}
		else
		{
			Exit();
		}
	}

	public void StartDragonCustomization()
	{
		SnChannel.StopPool("VO_Pool");
		KAInput.ResetInputAxes();
		string[] array = _DragonCustomizationAsset.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnDragonCustomizationLoaded, typeof(GameObject));
	}

	public void OnDragonCustomizationLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			AvAvatar.pAvatarCam.SetActive(value: true);
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiDragonCustomization";
			UiDragonCustomization component = obj.GetComponent<UiDragonCustomization>();
			component.pPetData = SanctuaryManager.pCurPetData;
			component.SetUiForJournal(isJournal: false);
			UiDragonCustomization.pEnableAvatarInputOnClose = true;
			AvAvatar.pInputEnabled = false;
			KAUI.RemoveExclusive(this);
			SetVisibility(inVisible: false);
			component._MessageObject = base.gameObject;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pAvatarCam.SetActive(value: true);
			UtDebug.LogError("Failed to load Avatar Equipment....");
			Exit();
			break;
		}
	}

	public void OnDragonCustomizationClosed()
	{
		Exit();
	}

	public void SetupScreen(Task inTask, string inNPCName)
	{
		mNPCName = inNPCName;
		mCurrentTask = inTask;
	}
}
