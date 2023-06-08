using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonTaxiManager : MonoBehaviour
{
	[Serializable]
	public class TaxiObjectData
	{
		public List<GameObject> _Objects;

		public MinMax _DistanceFromPlayer;

		public MinMax _ConeAngle;

		public MinMax _YDistanceOffset;

		public MinMax _TimeInSecsForNextObject;
	}

	private static DragonTaxiManager mInstance;

	public static TaxiUISceneData _TargetSceneToLoadData = null;

	public static bool _IsSelectableTrigger;

	public static TaxiDragonData _TaxiDragonData;

	public static NPCAvatar _PillionRider = null;

	public Vector3 _TaxiSpawnPoint = new Vector3(0f, 5000f, 0f);

	public TaxiObjectData[] _ObjectDataList;

	public GameObject _PortalRing;

	public GameObject _TaxiUI;

	public GameObject _CloudsTransition;

	public SnSound _Music;

	public List<string> _MutePool;

	public string _DragonControllerUI = "PfUiAvatarBtn";

	public int _SpawnPortalAfter = 4;

	public float _TaxiSpawnDistance = 5f;

	public float _TaxiSpeed = 10f;

	public float _CloudVelocityScale = 100f;

	public float _DistanceBetweenRings = 100f;

	public string _FlightControlBundle = "RS_DATA/PfUiFlightControlOptionsDM.unity3d/PfUiFlightControlOptionsDM";

	private Vector3 mAvatarPos = new Vector3(0f, 0f, 0f);

	private SanctuaryPet mTaxiPet;

	private SanctuaryPet mAvatarPet;

	private SilverLining mCloudsScript;

	private AvAvatarController mAvatarController;

	private UiFlightControlOptions mControlOptionsUI;

	private KAWidget mPointerArrow;

	private UiOptions mOptionsUI;

	private List<GameObject> mObjects = new List<GameObject>();

	private int mRandSelectedObjectIndex;

	private static bool mNPCColliderEnabled = true;

	private static bool mIsReadyToTaxi = false;

	private int mCount;

	private GameObject mCloudObj;

	public static DragonTaxiManager pInstance => mInstance;

	public static bool pIsReady => mInstance != null;

	private void Awake()
	{
		mInstance = this;
	}

	private void Update()
	{
		if (mAvatarController != null && mIsReadyToTaxi)
		{
			ProcessCollectables();
		}
		if (!(mAvatarController != null))
		{
			return;
		}
		Vector3 windVelocity = Vector3.Scale(-mAvatarController.pVelocity * _CloudVelocityScale, new Vector3(1f, 0f, 1f));
		if (mCloudsScript != null)
		{
			mCloudsScript.windVelocity = windVelocity;
			if (mCloudObj != null)
			{
				mCloudObj.transform.position = new Vector3(AvAvatar.position.x, AvAvatar.position.y + 800f, AvAvatar.position.z);
			}
		}
		GameObject gameObject = GameObject.Find("PfTaxiClouds");
		if (!(gameObject != null))
		{
			return;
		}
		gameObject.transform.position = AvAvatar.position + new Vector3(0f, -300f, 0f);
		Transform transform = gameObject.transform.Find("CloudsBG01");
		if (transform != null)
		{
			GrScrollUV component = transform.GetComponent<GrScrollUV>();
			if (component != null)
			{
				component._USpeed = windVelocity.x * 0.0001f;
				component._VSpeed = windVelocity.z * 0.0001f;
			}
		}
	}

	public static void InitTaxi(string inTaxiManagerBundlePath, TaxiUISceneData inTargetSceneData, bool isSelectableTrigger, TaxiDragonData inData, NPCAvatar inNPCRider = null)
	{
		_TargetSceneToLoadData = inTargetSceneData;
		_IsSelectableTrigger = isSelectableTrigger;
		_TaxiDragonData = inData;
		_PillionRider = inNPCRider;
		mIsReadyToTaxi = false;
		AvAvatar.pState = AvAvatarState.PAUSED;
		if (mInstance == null)
		{
			LoadTaxiManager(inTaxiManagerBundlePath);
		}
		else
		{
			mInstance.OnTaxiManagerLoaded();
		}
	}

	private static void LoadTaxiManager(string inBundlePath)
	{
		if (!string.IsNullOrEmpty(inBundlePath))
		{
			string[] array = inBundlePath.Split('/');
			if (array.Length == 3)
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnTaxiManagerLoadEventHandler, typeof(GameObject));
				return;
			}
		}
		if (_TargetSceneToLoadData != null && !string.IsNullOrEmpty(_TargetSceneToLoadData._LoadLevel))
		{
			if (!string.IsNullOrEmpty(_TargetSceneToLoadData._StartMarker))
			{
				AvAvatar.pStartLocation = _TargetSceneToLoadData._StartMarker;
			}
			RsResourceManager.LoadLevel(_TargetSceneToLoadData._LoadLevel);
		}
	}

	private static void OnTaxiManagerLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.ActivateAll(active: false);
				MainStreetMMOClient.pInstance.Disconnect();
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			UnityEngine.Object.Instantiate((GameObject)inObject);
			mInstance.OnTaxiManagerLoaded();
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Error downloading the!!!" + inURL);
			KAUICursorManager.SetDefaultCursor();
			if (_TargetSceneToLoadData != null && !string.IsNullOrEmpty(_TargetSceneToLoadData._LoadLevel))
			{
				if (!string.IsNullOrEmpty(_TargetSceneToLoadData._StartMarker))
				{
					AvAvatar.pStartLocation = _TargetSceneToLoadData._StartMarker;
				}
				RsResourceManager.LoadLevel(_TargetSceneToLoadData._LoadLevel);
			}
			break;
		}
	}

	private void OnTaxiManagerLoaded()
	{
		if (SanctuaryManager.pCurPetInstance != null && _IsSelectableTrigger)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
		}
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.AIActor != null)
		{
			SanctuaryManager.pCurPetInstance.AIActor.SetState(AISanctuaryPetFSM.CUSTOM);
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(_CloudsTransition);
		if (gameObject != null)
		{
			mCloudObj = gameObject;
			gameObject.transform.position = AvAvatar.position;
			Transform transform = gameObject.transform.Find("_SilverLiningSky");
			if (transform != null)
			{
				mCloudsScript = transform.GetComponent<SilverLining>();
			}
		}
		if (_IsSelectableTrigger)
		{
			CreateTaxiDragon(_TaxiDragonData);
			return;
		}
		mAvatarPet = SanctuaryManager.pCurPetInstance;
		mAvatarPet.SetFollowAvatar(follow: false);
		mAvatarPet.AIActor.SetState(AISanctuaryPetFSM.MOUNTED);
		StartCoroutine(StartTaxi());
	}

	private IEnumerator StartTaxi()
	{
		mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (mAvatarController != null)
		{
			mAvatarController.pFlyingGlidingMode = false;
		}
		mAvatarController.transform.rotation = Quaternion.identity;
		mAvatarController.ShowSoarButton(inShow: false);
		float num = UnityEngine.Random.Range(0, 24) * 15;
		float num2 = Mathf.Cos(num * (MathF.PI / 180f)) * _TaxiSpawnDistance;
		float num3 = Mathf.Sin(num * (MathF.PI / 180f)) * _TaxiSpawnDistance;
		_TaxiSpawnPoint.x += num2;
		_TaxiSpawnPoint.y += num3;
		yield return new WaitForEndOfFrame();
		AvAvatar.TeleportTo(_TaxiSpawnPoint, Vector3.zero, 0f, doTeleportFx: false);
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.SetVisibility(inVisible: false);
			UiAvatarControls.pInstance.DetachFromToolbar();
			UiAvatarControls.pInstance.DisableAllDragonControls();
			SanctuaryPet.RemoveMountEvent(SanctuaryManager.pCurPetInstance, UiAvatarControls.pInstance.OnDragonMount);
		}
		InteractiveTutManager componentInChildren = GetComponentInChildren<InteractiveTutManager>();
		if (componentInChildren != null && !componentInChildren.TutorialComplete())
		{
			if (UiJoystick.pInstance != null)
			{
				KAInput.ShowJoystick(UiJoystick.pInstance.pPos, inShow: false);
			}
			componentInChildren._StepStartedEvent = (StepStartedEvent)Delegate.Combine(componentInChildren._StepStartedEvent, new StepStartedEvent(OnStepStart));
			componentInChildren._StepEndedEvent = (StepEndedEvent)Delegate.Combine(componentInChildren._StepEndedEvent, new StepEndedEvent(OnStepEnd));
			componentInChildren.ShowTutorial();
			AvAvatar.pInputEnabled = false;
		}
		else
		{
			OnStepEnd(0, "", tutQuit: false);
		}
		if (_Music != null)
		{
			_Music.Play();
		}
		if (_MutePool != null)
		{
			foreach (string item in _MutePool)
			{
				SnChannel.MutePool(item, mute: true);
			}
		}
		if (SanctuaryManager.pCurPetInstance.AIActor == null)
		{
			SanctuaryManager.pCurPetInstance.AIActor = SanctuaryManager.pCurPetInstance.GetComponent<AIActor_Pet>();
		}
		mIsReadyToTaxi = true;
	}

	public void OnStepEnd(int stepIdx, string stepName, bool tutQuit)
	{
		InteractiveTutManager componentInChildren = GetComponentInChildren<InteractiveTutManager>();
		if (UtPlatform.IsMobile() && stepName == "SetUpControlsMobile")
		{
			ShowDragonControlButtons();
			return;
		}
		switch (stepName)
		{
		case "TeachFlyControls":
			ForceShowControlButtons(show: true);
			ShowDragonControlButtons();
			break;
		case "ShowBoost":
		case "ShowBrake":
			if (mPointerArrow != null)
			{
				mPointerArrow.SetVisibility(inVisible: false);
			}
			break;
		case "ShowFire":
		case "":
			if (mPointerArrow != null)
			{
				mPointerArrow.SetVisibility(inVisible: false);
			}
			ShowDragonControlButtons();
			ResetAvatar(componentInChildren);
			break;
		}
	}

	private void ResetAvatar(InteractiveTutManager interactiveTutManager)
	{
		if (interactiveTutManager != null)
		{
			if (interactiveTutManager._StepEndedEvent != null)
			{
				interactiveTutManager._StepEndedEvent = (StepEndedEvent)Delegate.Remove(interactiveTutManager._StepEndedEvent, new StepEndedEvent(OnStepEnd));
			}
			if (interactiveTutManager._StepStartedEvent != null)
			{
				interactiveTutManager._StepStartedEvent = (StepStartedEvent)Delegate.Remove(interactiveTutManager._StepStartedEvent, new StepStartedEvent(OnStepStart));
			}
		}
		ForceShowControlButtons(show: false);
		AvAvatar.pInputEnabled = true;
		AvAvatar.pState = AvAvatarState.IDLE;
		mAvatarController.SetFlyingState(FlyingState.TakeOffGliding);
		mAvatarController.pFlightSpeed = 3f;
		mAvatarController.pFlyingData._ManualFlapAccel = 1f;
		mAvatarController.pFlyingData._Speed.Max = _TaxiSpeed;
		mAvatarController.pFlyingData._DiveAccelRate = 0.3f;
		mRandSelectedObjectIndex = UnityEngine.Random.Range(0, _ObjectDataList.Length);
	}

	private void LoadFlightControlBundle()
	{
		if (mControlOptionsUI == null)
		{
			string[] array = _FlightControlBundle.Split('/');
			RsResourceManager.Load(array[0] + "/" + array[1], ControlOptionsReady);
		}
	}

	public void OnStepStart(int stepIdx, string stepName)
	{
		if (mControlOptionsUI == null && stepName == "SetUpControlsMobile")
		{
			LoadFlightControlBundle();
			return;
		}
		switch (stepName)
		{
		case "ShowBoost":
			mPointerArrow = UiAvatarControls.pInstance.FindItem("AniWingFlapPointer");
			if (mPointerArrow != null)
			{
				mPointerArrow.SetVisibility(inVisible: true);
				mPointerArrow.PlayAnim("Play");
			}
			break;
		case "ShowBrake":
			mPointerArrow = UiAvatarControls.pInstance.FindItem("AniBrakePointer");
			if (mPointerArrow != null)
			{
				mPointerArrow.SetVisibility(inVisible: true);
				mPointerArrow.PlayAnim("Play");
			}
			break;
		case "ShowFire":
			mPointerArrow = UiAvatarControls.pInstance.FindItem("AniFirePointer");
			if (mPointerArrow != null)
			{
				mPointerArrow.SetVisibility(inVisible: true);
				mPointerArrow.PlayAnim("Play");
			}
			break;
		}
	}

	private void ForceShowControlButtons(bool show)
	{
	}

	private void ControlOptionsReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromBundle(_FlightControlBundle));
			mControlOptionsUI = gameObject.GetComponent<UiFlightControlOptions>();
			if (mControlOptionsUI != null)
			{
				mControlOptionsUI._MessageObj = base.gameObject;
				mControlOptionsUI._CloseMsg = "CloseFlightControlOptions";
				mControlOptionsUI._BundlePath = _FlightControlBundle;
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("###### Could not load PfUiFlightControlOptio");
			break;
		}
	}

	private void CloseFlightControlOptions()
	{
		if (mControlOptionsUI != null)
		{
			mControlOptionsUI.SetVisibility(inVisible: false);
		}
		if (UiOptions.pIsTiltSteer)
		{
			string[] array = GameConfig.GetKeyData("OptionsAsset").Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OptionsBundleReady, typeof(GameObject));
		}
		else
		{
			AllCalibrationsDone();
		}
		ForceShowControlButtons(show: true);
	}

	private void ProgressTutorialStep()
	{
		InteractiveTutManager componentInChildren = GetComponentInChildren<InteractiveTutManager>();
		if (componentInChildren != null)
		{
			componentInChildren.StepProgressCallback(0f, 0f);
		}
	}

	private void OptionsBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			mOptionsUI = gameObject.GetComponent<UiOptions>();
			if (mOptionsUI != null)
			{
				mOptionsUI.SetVisibility(inVisible: false);
				mOptionsUI._BundlePath = inURL;
				mOptionsUI.gameObject.name = "PfUiOptions";
				mOptionsUI.PromptCalibration();
			}
			UiOptions.OnOptions = AllCalibrationsDone;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("###### Could not load PfUiOptions");
			break;
		}
	}

	private void AllCalibrationsDone()
	{
		if (mControlOptionsUI != null)
		{
			mControlOptionsUI.Destroy();
		}
		UiOptions.OnOptions = null;
		if (mOptionsUI != null)
		{
			mOptionsUI.Destroy();
		}
		ProgressTutorialStep();
	}

	private void CreateTaxiDragon(TaxiDragonData inData)
	{
		SanctuaryManager.CreatePet(RaisedPetData.CreateCustomizedPetData(inData._TypeID, inData._Age, inData._DataPath, inData._Gender, null, noColorMap: true), base.transform.position, Quaternion.identity, base.gameObject, "Player");
	}

	private void OnPetReady(SanctuaryPet pet)
	{
		pet.SetAvatar(AvAvatar.mTransform);
		pet.pMeterPaused = true;
		pet.SetFollowAvatar(follow: false);
		if ((bool)pet.AIActor)
		{
			pet.AIActor.SetState(AISanctuaryPetFSM.MOUNTED);
		}
		pet.Mount(AvAvatar.pObject, PetSpecialSkillType.FLY);
		if (_PillionRider != null)
		{
			mNPCColliderEnabled = true;
			Collider component = _PillionRider.GetComponent<Collider>();
			if (component != null)
			{
				mNPCColliderEnabled = component.enabled;
				component.enabled = false;
			}
			pet.MountPillion(_PillionRider.gameObject);
			_PillionRider.SetState(Character_State.action);
			_PillionRider.PlayAnim("FlyForward", -1, 1f, 1);
		}
		mTaxiPet = pet;
		SanctuaryManager.pCurPetInstance = mTaxiPet;
		SanctuaryManager.pCurPetData = mTaxiPet.pData;
		SanctuaryManager.pCurPetData.pNoSave = true;
		StartCoroutine(StartTaxi());
		if (mAvatarController != null)
		{
			mAvatarController.pFlyingData._YawTurnFactor = 1f;
		}
	}

	private void ProcessCollectables()
	{
		Vector3 vector = AvAvatar.position - mAvatarPos;
		vector.y = 0f;
		if (vector.magnitude >= _DistanceBetweenRings || mObjects.Count == 0)
		{
			GenerateCollectable();
		}
	}

	private void GenerateCollectable()
	{
		RemoveMissedCollectables();
		mAvatarPos = AvAvatar.position;
		TaxiObjectData taxiObjectData = _ObjectDataList[mRandSelectedObjectIndex];
		if (taxiObjectData == null || taxiObjectData._Objects == null || taxiObjectData._Objects.Count == 0)
		{
			return;
		}
		GameObject gameObject = ((_SpawnPortalAfter != mCount) ? UnityEngine.Object.Instantiate(taxiObjectData._Objects[UnityEngine.Random.Range(0, taxiObjectData._Objects.Count)]) : UnityEngine.Object.Instantiate(_PortalRing));
		if (!(gameObject != null))
		{
			return;
		}
		Vector3 position = mAvatarController._FlyingBone.position;
		Vector3 vector = mAvatarController._FlyingBone.forward;
		vector.y = 0f;
		if (mObjects.Count > 0)
		{
			vector = Quaternion.AngleAxis(UnityEngine.Random.Range(taxiObjectData._ConeAngle.Min, taxiObjectData._ConeAngle.Max), Vector3.up) * vector;
		}
		float num = UnityEngine.Random.Range(taxiObjectData._DistanceFromPlayer.Min, taxiObjectData._DistanceFromPlayer.Max);
		Vector3 position2 = position + vector * num;
		if (mObjects.Count > 0)
		{
			position2.y += UnityEngine.Random.Range(taxiObjectData._YDistanceOffset.Min, taxiObjectData._YDistanceOffset.Max);
		}
		gameObject.transform.position = position2;
		gameObject.transform.forward = vector;
		gameObject.transform.Rotate(90f, 0f, 0f);
		mObjects.Add(gameObject);
		if (_SpawnPortalAfter == mCount)
		{
			ObTriggerTaxi component = gameObject.GetComponent<ObTriggerTaxi>();
			if (component != null)
			{
				if (!string.IsNullOrEmpty(_TargetSceneToLoadData._StartMarker))
				{
					component._StartMarker = _TargetSceneToLoadData._StartMarker;
				}
				component._LoadLevel = _TargetSceneToLoadData._LoadLevel;
				if (null != _TargetSceneToLoadData._Icon)
				{
					component.SetIcon(_TargetSceneToLoadData._Icon);
				}
			}
			mCount = -1;
		}
		else
		{
			ObCollect component2 = gameObject.GetComponent<ObCollect>();
			if (component2 != null)
			{
				component2._MessageObject = base.gameObject;
			}
		}
		mCount++;
	}

	private void Collect(GameObject inObjectCollected)
	{
		UnityEngine.Object.Destroy(inObjectCollected);
		mObjects.Remove(inObjectCollected);
	}

	private void RemoveMissedCollectables()
	{
		Vector3 forward = mAvatarController._FlyingBone.forward;
		List<GameObject> list = new List<GameObject>(mObjects);
		foreach (GameObject mObject in mObjects)
		{
			Vector3 rhs = mObject.transform.position - mAvatarController._FlyingBone.position;
			if (Vector3.Dot(forward, rhs) < 0f)
			{
				UnityEngine.Object.Destroy(mObject);
				list.Remove(mObject);
			}
		}
		mObjects = list;
	}

	private void OnLandButtonClick()
	{
		if (_TargetSceneToLoadData != null && !string.IsNullOrEmpty(_TargetSceneToLoadData._LoadLevel))
		{
			if (mTaxiPet != null)
			{
				mTaxiPet.OnFlyDismountImmediate(AvAvatar.pObject);
				if (mTaxiPet.AIActor != null)
				{
					mTaxiPet.AIActor.SetState(AISanctuaryPetFSM.CUSTOM);
				}
			}
			if (!string.IsNullOrEmpty(_TargetSceneToLoadData._StartMarker))
			{
				AvAvatar.pStartLocation = _TargetSceneToLoadData._StartMarker;
			}
			RsResourceManager.LoadLevel(_TargetSceneToLoadData._LoadLevel);
		}
		if (_PillionRider != null)
		{
			Collider component = _PillionRider.GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = mNPCColliderEnabled;
			}
		}
	}

	private void ShowDragonControlButtons()
	{
		if (UiAvatarControls.pInstance != null)
		{
			if (UiJoystick.pInstance != null)
			{
				KAInput.ShowJoystick(UiJoystick.pInstance.pPos, inShow: true);
			}
			UiAvatarControls.pInstance.SetVisibility(inVisible: true);
			UiAvatarControls.pInstance.DisableAllDragonControls();
			UiAvatarControls.pInstance.EnableDragonControl("DragonBrake");
			UiAvatarControls.pInstance.EnableDragonControl("WingFlap");
			UiAvatarControls.EnableTiltControls(enable: true);
		}
	}

	public void UnMount()
	{
		if (mTaxiPet != null)
		{
			mTaxiPet.OnFlyDismountImmediate(AvAvatar.pObject);
			if (mTaxiPet.AIActor != null)
			{
				mTaxiPet.AIActor.SetState(AISanctuaryPetFSM.CUSTOM);
			}
		}
		if (mAvatarPet == null && MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SetRaisedPetString("");
		}
		SanctuaryManager.pMountedState = false;
		SanctuaryManager.pCurPetInstance = mAvatarPet;
		if (mAvatarPet != null)
		{
			mAvatarPet.OnFlyDismountImmediate(AvAvatar.pObject);
			SanctuaryManager.pCurPetData = mAvatarPet.pData;
		}
		else
		{
			SanctuaryManager.pCurPetData = null;
		}
	}

	private void OnDestroy()
	{
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.ActivateAll(active: true);
		}
	}
}
