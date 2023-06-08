using UnityEngine;

public class UWSwimmingZone : KAMonoBase
{
	[Tooltip("The Y position of the top of the swimming zone.")]
	public float _TopYPosition = -2f;

	[Tooltip("The amount the avatar will move on Y when entering UWSwimming from CSM.")]
	public float _AvatarTransitYOffset = 5f;

	[Tooltip("The camera will not be allowed above TopYPosition - this.")]
	public float _CameraDistanceFromSurface = 0.5f;

	[Tooltip("The avatar will not be allowed above TopYPosition - this.")]
	public float _AvatarDistanceFromSurface = 2f;

	[Tooltip("The CSM will be shown if avatar is above TopYPosition - this")]
	public float _CSMTriggerHeight = 2f;

	public UiUWSwimAirMeter _AirMeter;

	public Transform _DeathMarker;

	public UWBreathZoneCSM _DefaultBreathZone;

	public UWSwimmingCSM _UWSwimCSM;

	public EnvironmentalEffects _EnvironmentalEffects;

	public Transform _DragonStayMarker;

	public Transform _SurfaceMarker;

	private Transform mAvatarMarker;

	private AvAvatarController mAvatarController;

	private bool mIsEnvEffectsEnabled;

	public UWBreathZoneCSM pLastUsedBreathZone { get; set; }

	public bool pDoAvatarTransit { get; set; }

	private void Start()
	{
		mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
	}

	private void OnTriggerEnter(Collider inCollider)
	{
		if (!(inCollider.gameObject == AvAvatar.pObject))
		{
			return;
		}
		if (mAvatarController.pUWSwimZone != this)
		{
			if (mAvatarController.pUWSwimZone != null)
			{
				pLastUsedBreathZone = mAvatarController.pUWSwimZone.pLastUsedBreathZone;
				mAvatarController.pUWSwimZone.EnableEffects(enable: false);
			}
			if (pLastUsedBreathZone == null && _DefaultBreathZone != null)
			{
				pLastUsedBreathZone = _DefaultBreathZone;
			}
			mAvatarController.pUWSwimZone = this;
		}
		if (mAvatarController.IsInTriggerZone())
		{
			if (_UWSwimCSM != null)
			{
				_UWSwimCSM.Show(isVisible: true);
			}
		}
		else if (AvAvatar.pSubState != AvAvatarSubState.UWSWIMMING)
		{
			AvAvatar.pSubState = AvAvatarSubState.UWSWIMMING;
		}
		if (!mAvatarController.pIsTransiting && AvAvatar.pSubState == AvAvatarSubState.UWSWIMMING)
		{
			EnableEffects(enable: true);
			if (pLastUsedBreathZone != null && pLastUsedBreathZone.pSubState == UWBreathZoneCSM.BreathZoneSubState.BREATHING && AvAvatar.pAvatarCam != null)
			{
				AvAvatar.pAvatarCam.SetActive(value: false);
			}
		}
	}

	private void OnTriggerStay(Collider inCollider)
	{
		if (!(inCollider.gameObject == AvAvatar.pObject))
		{
			return;
		}
		if (mAvatarController.pUWSwimZone == null)
		{
			mAvatarController.pUWSwimZone = this;
		}
		if (mAvatarController.IsInTriggerZone())
		{
			if (_UWSwimCSM != null)
			{
				_UWSwimCSM.Show(isVisible: true);
			}
		}
		else
		{
			if (_UWSwimCSM != null)
			{
				_UWSwimCSM.Show(isVisible: false);
			}
			if (AvAvatar.pSubState != AvAvatarSubState.UWSWIMMING)
			{
				AvAvatar.pSubState = AvAvatarSubState.UWSWIMMING;
			}
		}
		if (!(mAvatarController.pUWSwimZone == this))
		{
			return;
		}
		if (mAvatarController.pIsTransiting)
		{
			if (AvAvatar.pSubState == AvAvatarSubState.UWSWIMMING)
			{
				if (AvAvatar.pAvatarCam.transform.position.y < _TopYPosition)
				{
					EnableEffects(enable: true);
				}
			}
			else if (AvAvatar.pAvatarCam.transform.position.y > _TopYPosition)
			{
				EnableEffects(enable: false);
			}
		}
		else if (AvAvatar.pSubState == AvAvatarSubState.UWSWIMMING)
		{
			EnableEffects(enable: true);
		}
	}

	public void Enter()
	{
		if (SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.EnableAllPets(enable: false, _DragonStayMarker);
			if (SanctuaryManager.pCurPetInstance != null)
			{
				SanctuaryManager.pCurPetInstance.pIsFlying = false;
			}
		}
		if (mAvatarMarker != null)
		{
			AvAvatar.TeleportTo(mAvatarMarker.position, mAvatarMarker.forward, 0f, doTeleportFx: false);
		}
		else if (pDoAvatarTransit)
		{
			mAvatarController.TransitAvatar(new Vector3(AvAvatar.GetPosition().x, AvAvatar.GetPosition().y - _AvatarTransitYOffset, AvAvatar.GetPosition().z));
			pDoAvatarTransit = false;
		}
		_AirMeter.SetVisibility(inVisible: true);
		SetAvatarMarker(null);
	}

	public void Exit()
	{
		bool flag = true;
		if (mAvatarMarker == null)
		{
			if ((SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.HEALTH) > SanctuaryManager.pCurPetInstance._MinPetMeterValue) || (SanctuaryManager.pCurPetInstance == null && mAvatarController._Stats._CurrentHealth > 0f))
			{
				if (pDoAvatarTransit)
				{
					mAvatarController.TransitAvatar(new Vector3(AvAvatar.GetPosition().x, (_SurfaceMarker != null) ? _SurfaceMarker.position.y : _DeathMarker.position.y, AvAvatar.GetPosition().z));
					pDoAvatarTransit = false;
					flag = false;
				}
			}
			else
			{
				AvAvatar.TeleportTo(_DeathMarker.position, _DeathMarker.forward, 0f, doTeleportFx: false);
			}
		}
		else
		{
			AvAvatar.TeleportTo(mAvatarMarker.position, mAvatarMarker.forward, 0f, doTeleportFx: false);
		}
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.pSubState = AvAvatarSubState.NORMAL;
		_AirMeter.SetVisibility(inVisible: false);
		if (!mAvatarController.pIsTransiting)
		{
			EnableEffects(enable: false);
		}
		if (flag)
		{
			SanctuaryManager.pInstance.EnableAllPets(enable: true);
		}
	}

	public void OnTriggerExit(Collider inCollider)
	{
		if (!(inCollider.gameObject == AvAvatar.pObject))
		{
			return;
		}
		SetAvatarMarker(null);
		if (mAvatarController.pUWSwimZone == this)
		{
			mAvatarController.pUWSwimZone = null;
			if (AvAvatar.pSubState == AvAvatarSubState.UWSWIMMING)
			{
				pLastUsedBreathZone = null;
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.pSubState = AvAvatarSubState.NORMAL;
				_AirMeter.SetVisibility(inVisible: false);
				SanctuaryManager.pInstance.EnableAllPets(enable: true);
				EnableEffects(enable: false);
			}
			if (_UWSwimCSM != null)
			{
				_UWSwimCSM.Show(isVisible: false);
			}
		}
	}

	public void SetAvatarMarker(Transform avatarMarker)
	{
		mAvatarMarker = avatarMarker;
	}

	private void EnableEffects(bool enable)
	{
		if (mIsEnvEffectsEnabled != enable)
		{
			mIsEnvEffectsEnabled = enable;
			if (_EnvironmentalEffects != null)
			{
				_EnvironmentalEffects.EnableEffects(enable);
			}
		}
	}

	public void OnTransitAvatarDone()
	{
		if (AvAvatar.pSubState != AvAvatarSubState.UWSWIMMING)
		{
			SanctuaryManager.pInstance.EnableAllPets(enable: true);
		}
	}
}
