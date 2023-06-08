using System;
using UnityEngine;

public class MountableNPCPet : MonoBehaviour
{
	[Serializable]
	public class MountablePetData
	{
		public Gender _Gender;

		public int _TypeID;

		public string _Prefab;

		public RaisedPetStage _Age;

		public string _MountPillionNPC;
	}

	public MountablePetData _MountablePetData;

	public bool _ResetToCurrentPosition;

	private NPCPetCSM mCsmUI;

	private SanctuaryPet mNPCPet;

	private SanctuaryPet mMountablePet;

	private SanctuaryPet mAvatarPet;

	private bool mMountingInProgress;

	private bool mIsReady;

	private CampSite mSpawnedCampSite;

	private bool mNPCColliderEnabled;

	private NPCAvatar mPillionRider;

	private Vector3 mCachedNPCPos;

	private Quaternion mCachedNPCRot;

	private string mPillionNPCName;

	private Character_State mCachedNPCState = Character_State.idle;

	private bool mDisableDefaultPillion;

	private PetSpecialSkillType mPetSpecialSkillType;

	protected void OnEnable()
	{
		if (mCsmUI == null)
		{
			mIsReady = false;
		}
	}

	protected void OnDisable()
	{
		mCsmUI = null;
		SetClickActivateObject(null);
	}

	public void OnDestroy()
	{
		RsResourceManager.LoadLevelStarted -= OnLoadLevelStarted;
		if (mMountablePet != null)
		{
			Reset();
		}
	}

	private void SetClickActivateObject(GameObject obj)
	{
		ObClickable component = base.gameObject.GetComponent<ObClickable>();
		if (component != null)
		{
			component.enabled = obj != null;
			component._ActivateObject = obj;
		}
	}

	private void Start()
	{
		if (!mNPCPet)
		{
			mNPCPet = GetComponent<SanctuaryPet>();
		}
	}

	protected void Update()
	{
		if (!mIsReady && SanctuaryManager.pInstance != null)
		{
			SetClickActivateObject(SanctuaryManager.pInstance._NpcPetClickActivateObject);
			if (SanctuaryManager.pInstance._NpcPetClickActivateObject != null)
			{
				mCsmUI = SanctuaryManager.pInstance._NpcPetClickActivateObject.GetComponent<NPCPetCSM>();
			}
			mIsReady = true;
		}
	}

	public void OnActivate()
	{
		if (mCsmUI != null)
		{
			mCsmUI.SendMessage("OnActivate");
			mCsmUI._NpcPet = this;
		}
	}

	public void StartMount(PetSpecialSkillType petSpecialSkillType = PetSpecialSkillType.RUN)
	{
		if (mMountingInProgress || (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted && SanctuaryManager.pCurPetInstance.pData.PetTypeID == _MountablePetData._TypeID))
		{
			return;
		}
		mPetSpecialSkillType = petSpecialSkillType;
		mMountingInProgress = true;
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(_MountablePetData._TypeID);
		if (string.IsNullOrEmpty(_MountablePetData._Prefab))
		{
			int ageIndex = RaisedPetData.GetAgeIndex(_MountablePetData._Age);
			if (sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[0]._Gender == _MountablePetData._Gender)
			{
				_MountablePetData._Prefab = sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[0]._Prefab;
			}
			else
			{
				_MountablePetData._Prefab = sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[1]._Prefab;
			}
		}
		RaisedPetData pdata = RaisedPetData.InitDefault(_MountablePetData._TypeID, _MountablePetData._Age, _MountablePetData._Prefab, _MountablePetData._Gender, addToActivePets: false);
		string value = (mDisableDefaultPillion ? mPillionNPCName : _MountablePetData._MountPillionNPC);
		GameObject gameObject = null;
		if (!string.IsNullOrEmpty(value))
		{
			gameObject = GameObject.Find(value);
		}
		Vector3 pos = ((gameObject != null) ? gameObject.transform.position : base.transform.position);
		SanctuaryManager.CreatePet(pdata, pos, Quaternion.identity, base.gameObject, "Player");
	}

	private void ShowNPCPet(bool inShow)
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = inShow;
		}
		ParticleSystem[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			ParticleSystem.EmissionModule emission = componentsInChildren2[i].emission;
			emission.enabled = inShow;
		}
		Collider[] componentsInChildren3 = base.gameObject.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].enabled = inShow;
		}
	}

	private void OnPetReady(SanctuaryPet pet)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Respawn");
		for (int i = 0; i < array.Length; i++)
		{
			CampSite componentInParent = array[i].GetComponentInParent<CampSite>();
			if (componentInParent != null && componentInParent.pIsPlayerInProximity)
			{
				mSpawnedCampSite = componentInParent;
				componentInParent.Reset();
				break;
			}
		}
		mAvatarPet = SanctuaryManager.pCurPetInstance;
		if (mAvatarPet != null)
		{
			mAvatarPet.SetFollowAvatar(follow: false);
			mAvatarPet.AIActor.SetState(AISanctuaryPetFSM.MOUNTED);
		}
		ShowNPCPet(inShow: false);
		pet.mNPC = true;
		mMountablePet = pet;
		mMountablePet.SetAvatar(AvAvatar.mTransform);
		mMountablePet.SetFollowAvatar(follow: false);
		if ((bool)mMountablePet.AIActor)
		{
			mMountablePet.AIActor.SetState(AISanctuaryPetFSM.MOUNTED);
		}
		bool flag = mNPCPet.pIsFlying || mPetSpecialSkillType == PetSpecialSkillType.FLY;
		mMountablePet.Mount(AvAvatar.pObject, flag ? PetSpecialSkillType.FLY : PetSpecialSkillType.RUN);
		if (flag)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if ((bool)component)
			{
				AvAvatar.pSubState = AvAvatarSubState.FLYING;
				component.SetFlyingState(FlyingState.Hover);
				component.SetFlyingHover(inHover: true);
			}
		}
		AttachPillionRider();
		if (MissionManager.pInstance != null)
		{
			int num = mMountablePet.name.IndexOf('(');
			string value = ((num > 0) ? mMountablePet.name.Substring(0, num) : mMountablePet.name);
			MissionManager.pInstance.CheckForTaskCompletion("Action", "MountDragon", value);
		}
		SanctuaryPet.AddMountEvent(mMountablePet, DragonMounted);
		SanctuaryManager.pCurPetInstance = mMountablePet;
		SanctuaryManager.pCurPetData = mMountablePet.pData;
		SanctuaryManager.pCurPetData.pNoSave = true;
		mMountablePet.RegisterAvatarDamage();
		SanctuaryManager.pInstance.InitPetMeter();
		UiAvatarControls.pInstance.Init();
		SanctuaryManager.pInstance.SetWeaponRechargeData(pet);
		PetWeaponManager component2 = pet.gameObject.GetComponent<PetWeaponManager>();
		if (component2 != null)
		{
			component2.pUserControlledWeapon = true;
		}
		MainStreetMMOClient.pInstance.SetRaisedPet(mMountablePet.pData, 0);
		RsResourceManager.LoadLevelStarted += OnLoadLevelStarted;
		mMountingInProgress = false;
	}

	protected void DragonMounted(bool mount, PetSpecialSkillType skill)
	{
		if (!mount)
		{
			RsResourceManager.LoadLevelStarted -= OnLoadLevelStarted;
			Reset();
		}
	}

	protected void Reset()
	{
		if (mSpawnedCampSite != null)
		{
			mSpawnedCampSite.Reset();
			mSpawnedCampSite = null;
		}
		if (mMountablePet != null)
		{
			mMountablePet.UnregisterAvatarDamage();
			SanctuaryPet.RemoveMountEvent(mMountablePet, DragonMounted);
			if (_ResetToCurrentPosition)
			{
				base.transform.position = (mMountablePet.pIsFlying ? AvAvatar.mTransform.position : mMountablePet.transform.position);
				base.transform.rotation = (mMountablePet.pIsFlying ? AvAvatar.mTransform.rotation : mMountablePet.transform.rotation);
				mNPCPet.AIActor.PlayAnimation(mMountablePet.pIsFlying ? mNPCPet.GetFlyAnim() : mNPCPet.GetIdleAnimationName(), force: true, 0.3f, PlayMode.StopAll);
				if (!mMountablePet.pIsFlying)
				{
					mNPCPet.FallToGround();
					mNPCPet.OrientToNormal(mNPCPet.GetGroundNormal());
					mNPCPet.pIsFlying = false;
				}
				else
				{
					mNPCPet.pIsFlying = true;
				}
			}
			UnityEngine.Object.Destroy(mMountablePet.gameObject);
			mMountablePet = null;
		}
		DetachPillionRider();
		ShowNPCPet(inShow: true);
		if (mAvatarPet == null && MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SetRaisedPetString("");
		}
		SanctuaryManager.pMountedState = false;
		SanctuaryManager.pCurPetInstance = mAvatarPet;
		UiAvatarControls.pInstance.pIsReady = false;
		if (mAvatarPet != null)
		{
			mAvatarPet.OnFlyDismountImmediate(AvAvatar.pObject);
			SanctuaryManager.pCurPetData = mAvatarPet.pData;
			SanctuaryManager.pInstance.pPetMeter.RefreshAll();
			return;
		}
		if (SanctuaryManager.pInstance != null && SanctuaryManager.pInstance.pPetMeter != null)
		{
			SanctuaryManager.pInstance.pPetMeter.SetVisibility(inVisible: false);
			SanctuaryManager.pInstance.pPetMeter = null;
		}
		SanctuaryManager.pCurPetData = null;
	}

	private void OnLoadLevelStarted(string inLevelName)
	{
		RsResourceManager.LoadLevelStarted -= OnLoadLevelStarted;
		Reset();
	}

	public void SetPillionRider(string pillionNPC)
	{
		if (!pillionNPC.Equals(mPillionNPCName))
		{
			DetachPillionRider();
			mDisableDefaultPillion = true;
			mPillionNPCName = pillionNPC;
			AttachPillionRider();
		}
	}

	private void AttachPillionRider()
	{
		GameObject gameObject = GameObject.Find(mDisableDefaultPillion ? mPillionNPCName : _MountablePetData._MountPillionNPC);
		if (!(mPillionRider == null) || !(gameObject != null) || !(mMountablePet != null) || (!mMountablePet.pIsMounted && !mMountablePet.pMountPending))
		{
			return;
		}
		mPillionRider = gameObject.GetComponent<NPCAvatar>();
		if (mPillionRider != null)
		{
			mCachedNPCPos = mPillionRider.transform.position;
			mCachedNPCRot = mPillionRider.transform.rotation;
			mCachedNPCState = mPillionRider.GetState();
			mNPCColliderEnabled = true;
			Collider component = mPillionRider.GetComponent<Collider>();
			if (component != null)
			{
				mNPCColliderEnabled = component.enabled;
				component.enabled = false;
			}
			mMountablePet.MountPillion(mPillionRider.gameObject);
			mPillionRider.SetState(Character_State.action);
			mPillionRider.PlayAnim("FlyForward", -1, 1f, 1);
		}
	}

	public void DetachPillionRider()
	{
		if (mPillionRider != null)
		{
			if (mMountablePet != null)
			{
				mMountablePet.UnMountPillion();
			}
			mPillionRider.transform.position = mCachedNPCPos;
			mPillionRider.transform.rotation = mCachedNPCRot;
			mPillionRider.SetState(mCachedNPCState);
			Collider component = mPillionRider.GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = mNPCColliderEnabled;
			}
			mPillionRider = null;
			mCachedNPCPos = Vector3.zero;
			mCachedNPCRot = Quaternion.identity;
			mCachedNPCState = Character_State.idle;
			mNPCColliderEnabled = false;
			mDisableDefaultPillion = false;
			mPillionNPCName = string.Empty;
		}
	}
}
