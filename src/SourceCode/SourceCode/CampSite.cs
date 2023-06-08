using System.Collections;

public class CampSite : ObProximity
{
	public float _HealthRegenRate = 2f;

	public float _PetHealthRegenRatePercent = 1f;

	public float _FireCoolDownRegenRate = 0.5f;

	private bool mIsPlayerInProximity;

	private float mPrevAvatarHealthRegenRate;

	private float mPrevPetHealthRegenRatePercent;

	private float mPrevFireCoolDownRegenRate;

	private AvAvatarController mPlayerController;

	private float mPrevPetUpdateFrequency;

	public float _PetUpdateFrequency = 0.2f;

	public bool pIsPlayerInProximity => mIsPlayerInProximity;

	public bool pActivate
	{
		set
		{
			_Active = value;
			if (_ActivateObject != null)
			{
				_ActivateObject.SetActive(_Active);
			}
		}
	}

	private AvAvatarController pPlayerController
	{
		get
		{
			if (mPlayerController == null && AvAvatar.pObject != null)
			{
				mPlayerController = AvAvatar.pObject.GetComponent<AvAvatarController>();
			}
			return mPlayerController;
		}
	}

	public void Start()
	{
		if (UtPlatform.IsWSA())
		{
			UtUtilities.ReAssignShader(_ActivateObject);
		}
	}

	public override void Update()
	{
		if ((!_UseGlobalActive || ObClickable.pGlobalActive) && _Active && !(AvAvatar.pObject == null) && _Range != 0f)
		{
			if (!mIsPlayerInProximity && IsInProximity(AvAvatar.position))
			{
				StartCoroutine(OnEnteringProximity());
			}
			else if (mIsPlayerInProximity && !IsInProximity(AvAvatar.position))
			{
				OnExitingProximity();
			}
			if (mIsPlayerInProximity)
			{
				pPlayerController.SetImmune(isImmune: true, 1f);
			}
		}
	}

	private IEnumerator OnEnteringProximity()
	{
		mIsPlayerInProximity = true;
		yield return null;
		if (pPlayerController != null)
		{
			mPrevAvatarHealthRegenRate = pPlayerController._Stats._HealthRegenRate;
			pPlayerController._Stats._HealthRegenRate = _HealthRegenRate;
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			mPrevFireCoolDownRegenRate = SanctuaryManager.pCurPetInstance.pWeaponManager._FireCoolDownRegenRate;
			SanctuaryManager.pCurPetInstance.pWeaponManager._FireCoolDownRegenRate = _FireCoolDownRegenRate;
			mPrevPetHealthRegenRatePercent = SanctuaryManager.pCurPetInstance.GetDecreaseRateMultiplier(SanctuaryPetMeterType.HEALTH);
			SanctuaryManager.pCurPetInstance.SetDecreaseRateMultiplier(SanctuaryPetMeterType.HEALTH, 0f - _PetHealthRegenRatePercent);
			SanctuaryPetTypeSettings sanctuaryPetSettings = SanctuaryData.GetSanctuaryPetSettings(SanctuaryData.FindSanctuaryPetTypeInfo(SanctuaryManager.pCurPetData.PetTypeID)._Settings);
			mPrevPetUpdateFrequency = sanctuaryPetSettings._UpdateFrequency;
			sanctuaryPetSettings._UpdateFrequency = _PetUpdateFrequency;
		}
	}

	private void OnExitingProximity()
	{
		Reset();
	}

	public void Reset()
	{
		if (mIsPlayerInProximity)
		{
			if (pPlayerController != null)
			{
				pPlayerController._Stats._HealthRegenRate = mPrevAvatarHealthRegenRate;
			}
			if (SanctuaryManager.pCurPetInstance != null)
			{
				SanctuaryManager.pCurPetInstance.pWeaponManager._FireCoolDownRegenRate = mPrevFireCoolDownRegenRate;
				SanctuaryManager.pCurPetInstance.SetDecreaseRateMultiplier(SanctuaryPetMeterType.HEALTH, mPrevPetHealthRegenRatePercent);
				SanctuaryData.GetSanctuaryPetSettings(SanctuaryData.FindSanctuaryPetTypeInfo(SanctuaryManager.pCurPetData.PetTypeID)._Settings)._UpdateFrequency = mPrevPetUpdateFrequency;
			}
		}
		mIsPlayerInProximity = false;
	}
}
