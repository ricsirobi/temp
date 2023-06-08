using System;
using System.Collections;
using UnityEngine;

public class UWBreathZoneCSM : ObContextSensitive
{
	public enum BreathZoneSubState
	{
		NONE,
		TRANSIT_IN,
		BREATHING,
		TRANSIT_OUT
	}

	public ContextSensitiveState[] _Menus;

	public string _BreathZoneCSItemName = "BreathZone";

	public string _SwimCSItemName = "SwimZone";

	private BreathZoneSubState mSubState;

	public Transform _BreathMarker;

	public GameObject _CutSceneZoneIn;

	public GameObject _CutSceneZoneOut;

	public GameObject _BreathZoneCamera;

	public float _HealthRefillRate = 0.1f;

	public float _AirRefillRate = 0.2f;

	public float _PetHealthRegenRatePercent = 1000f;

	public float _PetUpdateFrequency = 0.1f;

	private AvAvatarController mAvatarController;

	private float mPrevHealthRegenRate;

	private float mPrevAirUseRate;

	private float mPrevPetHealthRegenRatePercent;

	private float mPrevPetUpdateFrequency;

	private CoAnimController mCurrentCutscene;

	private KAUI mGlobalExclusiveUI;

	private AvAvatarState mCachedAvatarState = AvAvatarState.PAUSED;

	private bool mForceUpdateCSM;

	public BreathZoneSubState pSubState => mSubState;

	protected override void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
		inStatesArrData = ((AvAvatar.pState == AvAvatarState.PAUSED) ? null : _Menus);
	}

	protected override void OnMenuActive(ContextSensitiveStateType inMenuType)
	{
		base.OnMenuActive(inMenuType);
		if (pSubState != BreathZoneSubState.BREATHING)
		{
			AttachToToolbar(attach: true);
		}
		if (inMenuType == ContextSensitiveStateType.PROXIMITY)
		{
			ShowBreathZoneCSM(pSubState == BreathZoneSubState.BREATHING);
		}
		if (mForceUpdateCSM)
		{
			mForceUpdateCSM = false;
		}
	}

	private void OnTriggerEnter(Collider inCollider)
	{
		if (AvAvatar.IsCurrentPlayer(inCollider.gameObject) && mSubState == BreathZoneSubState.NONE)
		{
			if (base.pUI == null && !mProximityAlreadyEntered)
			{
				OnProximityEnter();
			}
			if (AvAvatar.pState != AvAvatarState.PAUSED)
			{
				mCachedAvatarState = AvAvatar.pState;
				AvAvatar.pState = AvAvatarState.PAUSED;
			}
			AvAvatar.SetUIActive(inActive: false);
			StartCoroutine(SetStateBreath());
		}
	}

	private IEnumerator SetStateBreath()
	{
		while (base.pUI == null)
		{
			yield return null;
		}
		if (mCachedAvatarState != AvAvatarState.PAUSED)
		{
			AvAvatar.pState = mCachedAvatarState;
		}
		SetState(BreathZoneSubState.BREATHING);
	}

	public void OnContextAction(string inName)
	{
		if (inName.Equals(_BreathZoneCSItemName))
		{
			ContextData contextData = GetContextData(_BreathZoneCSItemName);
			if (contextData != null)
			{
				base.pUI.RemoveContextDataFromList(contextData, enableRefreshItems: true);
			}
			SetVisibility(isVisible: false);
			SetState(BreathZoneSubState.TRANSIT_IN);
		}
		else if (inName.Equals(_SwimCSItemName))
		{
			ContextData contextData2 = GetContextData(_SwimCSItemName);
			if (contextData2 != null)
			{
				base.pUI.RemoveContextDataFromList(contextData2, enableRefreshItems: true);
			}
			SetVisibility(isVisible: false);
			SetState(BreathZoneSubState.TRANSIT_OUT);
		}
	}

	public void ShowBreathZoneCSM(bool isInBreathZone)
	{
		if (!(base.pUI == null))
		{
			SetVisibility(isVisible: true);
			string inName = (isInBreathZone ? _SwimCSItemName : _BreathZoneCSItemName);
			string inName2 = (isInBreathZone ? _BreathZoneCSItemName : _SwimCSItemName);
			ContextData contextData = GetContextData(inName);
			if (contextData != null)
			{
				base.pUI.AddContextDataIntoList(contextData, enableRefreshItems: true);
			}
			contextData = GetContextData(inName2);
			if (contextData != null)
			{
				base.pUI.RemoveContextDataFromList(contextData, enableRefreshItems: true);
			}
		}
	}

	public void SetVisibility(bool isVisible)
	{
		if (base.pUI != null && base.pUI.GetVisibility() != isVisible)
		{
			base.pUI.SetVisibility(isVisible);
		}
	}

	protected override void Start()
	{
		base.Start();
		mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (mAvatarController != null)
		{
			AvAvatarController avAvatarController = mAvatarController;
			avAvatarController.OnAvatarStateChange = (AvAvatarController.OnAvatarStateChanged)Delegate.Combine(avAvatarController.OnAvatarStateChange, new AvAvatarController.OnAvatarStateChanged(OnAvatarStateChange));
		}
	}

	public void SetState(BreathZoneSubState state)
	{
		if (mSubState == state)
		{
			return;
		}
		BreathZoneSubState breathZoneSubState = mSubState;
		mSubState = state;
		switch (mSubState)
		{
		case BreathZoneSubState.TRANSIT_IN:
			mAvatarController.pUWSwimZone.pLastUsedBreathZone = this;
			mAvatarController.pUWSwimZone._AirMeter.AttachToToolbar(attach: false);
			ShowCutScene();
			break;
		case BreathZoneSubState.TRANSIT_OUT:
			if (_BreathZoneCamera != null && breathZoneSubState == BreathZoneSubState.BREATHING)
			{
				_BreathZoneCamera.SetActive(value: false);
			}
			AttachToToolbar(attach: true);
			ShowCutScene();
			break;
		case BreathZoneSubState.BREATHING:
			mGlobalExclusiveUI = KAUI._GlobalExclusiveUI;
			if (mGlobalExclusiveUI != null)
			{
				KAUI.RemoveExclusive(mGlobalExclusiveUI);
			}
			AttachToToolbar(attach: false);
			mAvatarController.StopUWSwimmingImmediate();
			AvAvatar.pInputEnabled = false;
			if (breathZoneSubState != BreathZoneSubState.TRANSIT_IN)
			{
				if (_BreathMarker != null)
				{
					AvAvatar.TeleportTo(_BreathMarker.transform.position, _BreathMarker.transform.forward, 0f, doTeleportFx: false);
				}
				mAvatarController.pUWSwimZone._AirMeter.AttachToToolbar(attach: false);
			}
			AvAvatar.SetUIActive(inActive: false);
			UpdateAirStats(set: true);
			ShowBreathZoneCSM(isInBreathZone: true);
			if (_BreathZoneCamera != null)
			{
				_BreathZoneCamera.SetActive(value: true);
				if (AvAvatar.pAvatarCam != null)
				{
					AvAvatar.pAvatarCam.SetActive(value: false);
				}
			}
			break;
		case BreathZoneSubState.NONE:
			AvAvatar.pInputEnabled = true;
			if (mGlobalExclusiveUI != null)
			{
				KAUI.SetExclusive(mGlobalExclusiveUI);
				mGlobalExclusiveUI = null;
			}
			UpdateAirStats(set: false);
			mAvatarController.pUWSwimZone._AirMeter.AttachToToolbar(attach: true);
			AvAvatar.SetUIActive(inActive: true);
			ShowBreathZoneCSM(isInBreathZone: false);
			break;
		}
	}

	private void ShowCutScene()
	{
		AvAvatar.SetUIActive(inActive: false);
		mAvatarController.ApplySwimAnim(AvAvatarController.SwimAnimState.NONE);
		GameObject gameObject = null;
		if (mSubState == BreathZoneSubState.TRANSIT_IN)
		{
			gameObject = _CutSceneZoneIn;
		}
		else if (mSubState == BreathZoneSubState.TRANSIT_OUT)
		{
			gameObject = _CutSceneZoneOut;
		}
		if (gameObject == null)
		{
			OnCutSceneDone();
			return;
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
		if (gameObject2 != null)
		{
			gameObject2.transform.parent = base.transform;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			mCurrentCutscene = gameObject2.GetComponentInChildren<CoAnimController>();
			mCurrentCutscene._MessageObject = base.gameObject;
			mCurrentCutscene.CutSceneStart();
		}
	}

	private void OnCutSceneDone()
	{
		if (mCurrentCutscene != null)
		{
			mCurrentCutscene.CutSceneDone();
			UnityEngine.Object.Destroy(mCurrentCutscene.gameObject);
			mCurrentCutscene = null;
		}
		if (mSubState == BreathZoneSubState.TRANSIT_IN)
		{
			SetState(BreathZoneSubState.BREATHING);
		}
		else if (mSubState == BreathZoneSubState.TRANSIT_OUT)
		{
			SetState(BreathZoneSubState.NONE);
		}
		if (pSubState == BreathZoneSubState.BREATHING)
		{
			OnProximityEnter();
		}
	}

	private void UpdateAirStats(bool set)
	{
		if (set)
		{
			mPrevHealthRegenRate = mAvatarController._Stats._HealthRegenRate;
			mAvatarController._Stats._HealthRegenRate = _HealthRefillRate;
			mPrevAirUseRate = mAvatarController._Stats._AirUseRate;
			mAvatarController._Stats._AirUseRate = _AirRefillRate;
			if (SanctuaryManager.pCurPetInstance != null)
			{
				mPrevPetHealthRegenRatePercent = SanctuaryManager.pCurPetInstance.GetDecreaseRateMultiplier(SanctuaryPetMeterType.HEALTH);
				mPrevPetUpdateFrequency = SanctuaryManager.pCurPetInstance.GetTypeSettings()._UpdateFrequency;
				float multiplier = 0f - _PetHealthRegenRatePercent / 100f;
				SanctuaryManager.pCurPetInstance.SetDecreaseRateMultiplier(SanctuaryPetMeterType.HEALTH, multiplier);
				SanctuaryManager.pCurPetInstance.GetTypeSettings()._UpdateFrequency = _PetUpdateFrequency;
			}
		}
		else
		{
			mAvatarController._Stats._HealthRegenRate = mPrevHealthRegenRate;
			mAvatarController._Stats._AirUseRate = mPrevAirUseRate;
			if (SanctuaryManager.pCurPetInstance != null)
			{
				SanctuaryManager.pCurPetInstance.SetDecreaseRateMultiplier(SanctuaryPetMeterType.HEALTH, mPrevPetHealthRegenRatePercent);
				SanctuaryManager.pCurPetInstance.GetTypeSettings()._UpdateFrequency = mPrevPetUpdateFrequency;
			}
		}
	}

	public void AttachToToolbar(bool attach)
	{
		if (base.pUI == null)
		{
			return;
		}
		if (attach)
		{
			if (AvAvatar.pToolbar != null)
			{
				base.pUI.transform.parent = AvAvatar.pToolbar.transform;
			}
		}
		else
		{
			base.pUI.transform.parent = null;
		}
	}

	public void OnAvatarStateChange()
	{
		if ((AvAvatar.pState == AvAvatarState.PAUSED || AvAvatar.pPrevState == AvAvatarState.PAUSED) && AvAvatar.pSubState == AvAvatarSubState.UWSWIMMING && mProximityAlreadyEntered)
		{
			SetProximityAlreadyEntered(isEntered: false);
			Refresh();
		}
	}

	protected override void OnDestroy()
	{
		if (mAvatarController != null)
		{
			AvAvatarController avAvatarController = mAvatarController;
			avAvatarController.OnAvatarStateChange = (AvAvatarController.OnAvatarStateChanged)Delegate.Remove(avAvatarController.OnAvatarStateChange, new AvAvatarController.OnAvatarStateChanged(OnAvatarStateChange));
		}
		base.OnDestroy();
	}
}
