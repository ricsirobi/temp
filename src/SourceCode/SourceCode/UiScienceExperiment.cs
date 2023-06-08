using System;
using System.Collections;
using System.Collections.Generic;
using JSGames.UI;
using UnityEngine;

public class UiScienceExperiment : KAUI
{
	public enum InHandObjectType
	{
		TEST_ITEM,
		ICE
	}

	public enum LabSlideState
	{
		SLIDING_IN,
		SLID_IN,
		SLIDING_OUT,
		SLID_OUT
	}

	[Serializable]
	public class LabSlideInfo
	{
		public Vector2 _ToPos;

		public float _Time;
	}

	public class InHandObjectData
	{
		private InHandObjectType mType;

		private ScienceExperimentData mExperimentData;

		private Transform mObject;

		private ScientificExperiment mManager;

		private Vector3 mOffsetPosFromMousePtr = Vector3.zero;

		public Vector3 pOffsetPosFromMousePtr => mOffsetPosFromMousePtr;

		public InHandObjectType pType => mType;

		public ScienceExperimentData pExperimentData => mExperimentData;

		public Transform pObject => mObject;

		public InHandObjectData(InHandObjectType inType, ScienceExperimentData inData, Transform inObject, Vector3 inOffsetPos, ScientificExperiment inManager)
		{
			mType = inType;
			mObject = inObject;
			mExperimentData = inData;
			Rigidbody component = mObject.GetComponent<Rigidbody>();
			if (mObject != null && component != null)
			{
				component.isKinematic = true;
				component.useGravity = true;
			}
			mOffsetPosFromMousePtr = inOffsetPos;
			mManager = inManager;
		}

		public bool Drop()
		{
			if (ScientificExperiment.pInstance == null)
			{
				return false;
			}
			switch (ScientificExperiment.pInstance.pExperimentType)
			{
			case ExperimentType.NORMAL:
			case ExperimentType.SKRILL_LAB:
			case ExperimentType.MAGNETISM_LAB:
			case ExperimentType.SPECTRUM_LAB:
			case ExperimentType.TITRATION_LAB:
			{
				UiScienceExperiment mainUI = mManager._MainUI;
				Vector3 up = mainUI._OhmMeterInfo._OhmMeterKillMarker.up;
				Vector3 to = mainUI.mObjectInHand.pObject.localPosition - mainUI._OhmMeterInfo._OhmMeterKillMarker.localPosition;
				if (ScientificExperiment.pInstance.pExperimentType == ExperimentType.SKRILL_LAB && (!mainUI.mOhmMeterEnabled || (Vector3.Angle(up, to) > 90f && (double)to.magnitude <= 1.0)))
				{
					return false;
				}
				if (mObject == null)
				{
					return false;
				}
				Rigidbody component = mObject.GetComponent<Rigidbody>();
				if (component == null)
				{
					return false;
				}
				component.isKinematic = false;
				component.Sleep();
				mManager.StartCoroutine(WaitForSecond(component));
				return true;
			}
			case ExperimentType.GRONCKLE_IRON:
				if (mManager._Gronckle != null)
				{
					bool flag = mManager._Gronckle.ObjectDroppedToEat(this);
					if (flag || !mManager._Gronckle.pIsEmptyBelly)
					{
						return flag;
					}
					goto case ExperimentType.NORMAL;
				}
				return false;
			default:
				return false;
			}
		}

		private IEnumerator WaitForSecond(Rigidbody inObject)
		{
			if (inObject == null)
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.1f);
			if (inObject != null)
			{
				inObject.WakeUp();
			}
		}

		public void SetPosition(Vector3 inPos)
		{
			if (mObject != null)
			{
				mObject.position = inPos;
			}
		}
	}

	public enum Cursor
	{
		DEFAULT,
		PESTLE,
		SCOOP,
		SCOOP_WITH_ICE,
		GLOVE,
		LOADING,
		FEATHER,
		NONE
	}

	[Serializable]
	public class CursorData
	{
		public Cursor _Type;

		public Texture2D _Texture;

		public Vector3 _Size;

		public GameObject _StoolTool;
	}

	[Serializable]
	public class OhmMeterProp
	{
		public float _NeedleVibrateDuration;

		public Vector2 _NeedleVibrateAngleRange;

		public float _NeedleMinReading;

		public float _NeedleMaxReading;

		public float _NeedleMinAngle;

		public float _NeedleMaxAngle;

		public float _NeedleVibrateSpeed;

		public Transform _OhmMeterKillMarker;
	}

	public delegate void UserPromptTextCallback(string inID, object inUserData);

	public OhmMeterProp _OhmMeterInfo;

	public ScientificExperiment _Manager;

	public Camera _MainCamera;

	public Transform _Crucible;

	public KAUIMenu _ExperimentItemMenu;

	public KAUIMenu _TitrationPHMenu;

	public bool _SetCursorOnContextAction = true;

	public List<CursorData> _CursorList;

	public LocaleString _CompleteText = new LocaleString("Experiment completed!!");

	public LabSlideInfo _ThermometerSlideInInfo;

	public LabSlideInfo _ThermometerSlideOutInfo;

	public LabSlideInfo _TimeSlideInInfo;

	public LabSlideInfo _TimeSlideOutInfo;

	public LabSlideInfo _WeighingToolSlideInInfo;

	public LabSlideInfo _WeighingToolSlideOutInfo;

	public LabSlideInfo _OhmMeterSlideInInfo;

	public LabSlideInfo _OhmMeterSlideOutInfo;

	public Transform _Mercury;

	public Renderer _MercuryBottom;

	public Texture2D _MercuryColdTexture;

	public Texture2D _MercuryNormalTexture;

	public LabSlider _Thermometer;

	public LabSlider _Timer;

	public ObRotate _TimerArrow;

	public LabSlider _WeighingMachine;

	public LabSlider _OhmMeter;

	public GameObject _OhmMeterNeedle;

	public float _OhmMeterNeedleSensitivity = 1f;

	public int _PestleTriggerSwipeCount = 8;

	public GameObject _TitrationWidgetGroup;

	public Vector3 _DefaultCursorSize = new Vector3(64f, 64f, 1f);

	public ParticleSystem _ResetParticle;

	private bool mThermometerEnabled;

	private bool mWeighingMachineEnabled;

	private bool mOhmMeterEnabled;

	private bool mElectricFlow;

	private bool mElectrifyDone;

	private KAWidget mTxtTime;

	private KAWidget mTxtThermometer;

	private KAWidget mWeighingMachineWidget;

	private KAWidget mTitrationGoalWidget;

	private KAWidget mTxtExperimentResult;

	private KAWidget mOhmMeterWidget;

	private KAWidget mLblCrucibleItems;

	private SEWeighingMachine mWeighingMachine;

	private SEOhmMeter mOhmMeter;

	private KAWidget mTxtDirections;

	private LabTask mCurrentDirectionTask;

	private Cursor mCurrentCursor;

	private InHandObjectData mObjectInHand;

	private UiJournal mJournal;

	private bool mLoadingJournal;

	private KAWidget mBtnUserPrompt;

	private KAWidget mTxtUserPrompt;

	private KACheckBox mBtnCheckbox;

	private UserPromptTextCallback mUserPromptTextCallback;

	private Dictionary<GameObject, LabSlideState> mSlideStates;

	private bool mUserPromptOn;

	private bool mTimerInteractiveOnUserPrompt = true;

	private object mUserPromptUserData;

	private int mLabItemInitCount;

	private int mLabItemBundleCount;

	private bool mListenClickOnTimer;

	private int mPestleLayerMask = -1;

	public GameObject _StoolObject;

	public Transform _StoolTopPivot;

	public ParticleSystem _StoolSparkleFx;

	private GameObject m3DMouseObject;

	private GameObject mStoolTool;

	private int mFeatherLayerMask = -1;

	public Transform _IceContainerTriggerArea;

	private int mCursorUpdatedFrameCount;

	private bool mMixing;

	private float mMixingTimer;

	private bool mIsLeftSwipe;

	private int mSwipeCount;

	private bool mCheckForSwipeCrucible;

	private float mSwipeTimer;

	private Vector2 mCurrentDirection = Vector3.up;

	public Vector3 pDroppedPosition { get; set; }

	public float pDropStartedTime { get; set; }

	public bool pIsReady
	{
		get
		{
			if (mLabItemBundleCount == 0)
			{
				return mLabItemInitCount <= 0;
			}
			return false;
		}
	}

	public int pCursorUpdatedFrameCount => mCursorUpdatedFrameCount;

	public bool pListenClickOnTimer
	{
		get
		{
			return mListenClickOnTimer;
		}
		set
		{
			mListenClickOnTimer = value;
		}
	}

	public UiJournal pJournal => mJournal;

	public InHandObjectData pObjectInHand => mObjectInHand;

	public SEWeighingMachine pWeighingMachine => mWeighingMachine;

	public SEOhmMeter pOhmMeter => mOhmMeter;

	public LabTask pCurrentDirectionTask => mCurrentDirectionTask;

	public Cursor pCurrentCursor => mCurrentCursor;

	private LabCrucible pCrucible
	{
		get
		{
			if (_Manager != null)
			{
				return _Manager.pCrucible;
			}
			return null;
		}
	}

	private LabThermometer pThermometer
	{
		get
		{
			if (_Manager != null)
			{
				return _Manager.pThermometer;
			}
			return null;
		}
	}

	public bool pMixing
	{
		get
		{
			return mMixing;
		}
		set
		{
			mMixing = false;
		}
	}

	public float pMixingTimer
	{
		get
		{
			return mMixingTimer;
		}
		set
		{
			mMixingTimer = value;
		}
	}

	public bool pLoadingJournal => mLoadingJournal;

	public bool pUserPromptOn => mUserPromptOn;

	public bool pElectricFlow
	{
		get
		{
			return mElectricFlow;
		}
		set
		{
			mElectricFlow = value;
		}
	}

	public bool pElectrifyDone
	{
		get
		{
			return mElectrifyDone;
		}
		set
		{
			mElectrifyDone = value;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		TouchManager.OnDragEvent = (OnDrag)Delegate.Remove(TouchManager.OnDragEvent, new OnDrag(OnDrag));
		TouchManager.OnDragEvent = (OnDrag)Delegate.Remove(TouchManager.OnDragEvent, new OnDrag(OnDragScoop));
		TouchManager.OnFingerUpEvent = (OnFingerUp)Delegate.Remove(TouchManager.OnFingerUpEvent, new OnFingerUp(OnFingerUp));
		TouchManager.OnFingerDownEvent = (OnFingerDown)Delegate.Remove(TouchManager.OnFingerDownEvent, new OnFingerDown(OnFingerDown));
	}

	protected override void Start()
	{
		base.Start();
		mSlideStates = new Dictionary<GameObject, LabSlideState>();
		mTxtTime = FindItem("Time");
		if (mTxtTime != null)
		{
			mTxtTime.SetVisibility(inVisible: false);
		}
		mSlideStates.Add(_Timer.gameObject, LabSlideState.SLID_OUT);
		mTxtThermometer = FindItem("TxtThermometer");
		if (mTxtThermometer != null)
		{
			mTxtThermometer.SetVisibility(inVisible: false);
		}
		mTitrationGoalWidget = FindItem("TitrationGoalColor");
		if (mTitrationGoalWidget != null)
		{
			mTitrationGoalWidget.SetVisibility(inVisible: false);
		}
		mTxtExperimentResult = FindItem("TxtExperimentResult");
		if (mTxtExperimentResult != null)
		{
			mTxtExperimentResult.SetVisibility(inVisible: false);
		}
		if (_TitrationWidgetGroup != null)
		{
			_TitrationWidgetGroup.SetActive(value: false);
		}
		mSlideStates.Add(_Thermometer.gameObject, LabSlideState.SLID_OUT);
		mWeighingMachineWidget = FindItem("WeighingMachine");
		if (mWeighingMachineWidget != null)
		{
			mWeighingMachineWidget.SetVisibility(inVisible: false);
		}
		mSlideStates.Add(_WeighingMachine.gameObject, LabSlideState.SLID_OUT);
		mOhmMeterWidget = FindItem("TxtOhmMeter");
		if (mOhmMeterWidget != null)
		{
			mOhmMeterWidget.SetVisibility(inVisible: false);
		}
		if (_TitrationPHMenu != null)
		{
			_TitrationPHMenu.SetVisibility(inVisible: false);
			_TitrationPHMenu._BackgroundObject.SetActive(value: false);
		}
		mSlideStates.Add(_OhmMeter.gameObject, LabSlideState.SLID_OUT);
		mLblCrucibleItems = FindItem("TxtCrucibleItems");
		mTxtDirections = FindItem("TxtDirections");
		mBtnUserPrompt = FindItem("BtnUserPrompt");
		if (mBtnUserPrompt != null)
		{
			mBtnUserPrompt.SetVisibility(inVisible: false);
		}
		mTxtUserPrompt = FindItem("TxtUserPrompt");
		if (mTxtUserPrompt != null)
		{
			mTxtUserPrompt.SetVisibility(inVisible: false);
		}
		mBtnCheckbox = FindItem("CompleteBox") as KACheckBox;
		SlideOut(_Thermometer.gameObject);
		SlideOut(_WeighingMachine.gameObject);
		SlideOut(_Timer.gameObject);
		SlideOut(_OhmMeter.gameObject);
		if (UtPlatform.IsMobile())
		{
			AdManager.DisplayAd(AdEventType.LAB_ENTERED, AdOption.FULL_SCREEN);
		}
		if (_StoolObject != null)
		{
			_StoolObject.SetActive(value: true);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mJournal == null && (mCurrentCursor == Cursor.DEFAULT || mCurrentCursor == Cursor.GLOVE))
		{
			if (KAInput.GetMouseButton(0))
			{
				ActivateCursor(Cursor.GLOVE);
			}
			else
			{
				ActivateCursor(Cursor.DEFAULT);
			}
		}
		UpdateStopWatch();
		UpdateThermometer();
		UpdateWeighMachine();
		UpdateOhmMeter();
		HandleObjectPickup();
		HandleInHandObject();
		HandleIceScoop();
		HandleMixing();
		if (KAInput.GetMouseButtonDown(0))
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = _Crucible.position.z + 2f;
			Vector3 vector = _MainCamera.ScreenToWorldPoint(mousePosition) - _MainCamera.transform.position;
			if (Physics.Raycast(_MainCamera.transform.position, vector.normalized, out var hitInfo, 50f) && hitInfo.collider.gameObject == _Timer.gameObject)
			{
				OnClickedOnTimer();
			}
		}
	}

	private void OnClickedOnTitrate(KAWidget inWidget)
	{
		string text = inWidget.GetText();
		int result = 0;
		if (int.TryParse(text, out result))
		{
			_Manager.AddAcidity(result);
		}
	}

	private void OnClickedOnTimer()
	{
		if (!mUserPromptOn || mTimerInteractiveOnUserPrompt)
		{
			if (mListenClickOnTimer)
			{
				_Manager.EnableTime(inUserActivated: false);
				_Manager._MainUI.HideUserPrompt();
			}
			else if (_Manager.pTimeEnabled)
			{
				_Manager.DisableTime();
			}
			else
			{
				_Manager.EnableTime(inUserActivated: true);
			}
			if (_Manager.pTimeEnabled)
			{
				_Manager.ResetTime();
			}
			_Manager._MainUI.pListenClickOnTimer = false;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name.Contains("BtnTitration"))
		{
			OnClickedOnTitrate(inWidget);
		}
		switch (inWidget.name)
		{
		case "BtnBmBack":
			if (_Manager != null)
			{
				_Manager.Exit();
			}
			OnExit();
			break;
		case "BtnDirectionLeft":
			ShowExperimentDirection(-1);
			break;
		case "BtnDirectionRight":
			ShowExperimentDirection(1);
			break;
		case "BtnHeat":
			HeatCrucible();
			break;
		case "BtnIce":
			ActivateCursor(Cursor.PESTLE);
			break;
		case "BtnWater":
			TriggerWater();
			break;
		case "BtnPetsle":
			ActivateCursor(Cursor.PESTLE);
			break;
		case "BtnReset":
			Reset();
			break;
		case "BtnJournal":
			TriggerJournal();
			break;
		case "BtnUserPrompt":
			HideUserPrompt("ButtonClick");
			break;
		case "BtnNoteItClose":
			HideUserPrompt();
			break;
		case "Time":
			OnClickedOnTimer();
			break;
		}
	}

	public void SetTitrationPHMenu(LabColorAgentTestObject.PHColorBracket[] phBracket)
	{
		_TitrationPHMenu.ClearItems();
		_TitrationPHMenu.SetVisibility(inVisible: true);
		_TitrationPHMenu._BackgroundObject.SetActive(value: true);
		for (int i = 0; i < phBracket.Length; i++)
		{
			KAWidget kAWidget = _TitrationPHMenu.AddWidget(_TitrationPHMenu._Template.name);
			kAWidget.name = phBracket[i]._PHValue.ToString();
			kAWidget.SetVisibility(inVisible: true);
			kAWidget.pBackground.color = phBracket[i]._Color;
		}
	}

	public void SetTitrationPHValue(string pHValue)
	{
		if (_TitrationPHMenu.GetItemCount() >= 0)
		{
			KAWidget kAWidget = _TitrationPHMenu.GetItems().Find((KAWidget x) => x.name == pHValue);
			if (kAWidget != null)
			{
				_TitrationPHMenu.SetSelectedItem(kAWidget);
			}
		}
	}

	public void SetTitrationGoalColor(Color inColor)
	{
		if (mTitrationGoalWidget != null)
		{
			if (!mTitrationGoalWidget.GetVisibility())
			{
				mTitrationGoalWidget.SetVisibility(inVisible: true);
			}
			mTitrationGoalWidget.pBackground.color = inColor;
		}
	}

	public void UpdateExperimentResultText(string inLocalisedText)
	{
		if (string.IsNullOrEmpty(inLocalisedText))
		{
			mTxtExperimentResult.SetVisibility(inVisible: false);
			return;
		}
		mTxtExperimentResult.SetVisibility(inVisible: true);
		mTxtExperimentResult.SetText(inLocalisedText);
	}

	public void OnContextAction(string inActionName)
	{
		if (string.IsNullOrEmpty(inActionName) || pUserPromptOn)
		{
			return;
		}
		if (_SetCursorOnContextAction)
		{
			ActivateCursor(Cursor.DEFAULT);
		}
		switch (inActionName)
		{
		case "Clock":
			_Manager.pShowClock = !_Manager.pShowClock;
			break;
		case "ThermoMeter":
			mThermometerEnabled = !mThermometerEnabled;
			if (_Thermometer != null)
			{
				if (mThermometerEnabled)
				{
					SlideIn(_Thermometer.gameObject);
				}
				else
				{
					SlideOut(_Thermometer.gameObject);
				}
			}
			break;
		case "WeighingMachine":
			TriggerWeighingMachine(!mWeighingMachineEnabled);
			break;
		case "OhmMeter":
			TriggerOhmMeter(!mOhmMeterEnabled);
			break;
		case "Electrify":
			EnableElectricFlow();
			break;
		case "Heat":
			HeatCrucible();
			break;
		case "Freeze":
			SnChannel.Play(_Manager._IceButtonSFX, "SFX_Pool", inForce: true, null);
			ActivateCursor(Cursor.SCOOP);
			break;
		case "Water":
			TriggerWater();
			break;
		case "Pestle":
			ActivateCursor(Cursor.PESTLE);
			break;
		case "Feather":
			ActivateCursor(Cursor.FEATHER);
			break;
		}
	}

	public void MenuItemDragged(KAUIMenu inMenu, KAWidget inWidget, Vector2 inDelta)
	{
		if (inWidget == null || mObjectInHand != null || LabCrucible.TestItemLoader.IsLoading() || pCrucible == null || LabCrucible.pIsMixing)
		{
			return;
		}
		ScienceExperimentData scienceExperimentData = (ScienceExperimentData)inWidget.GetUserData();
		if (scienceExperimentData == null)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(((Transform)scienceExperimentData._ItemPrefabData._ResObject).gameObject, Vector3.up * 5000f, Quaternion.identity);
		if (!(gameObject != null))
		{
			return;
		}
		SnChannel.Play(_Manager._PickupSFX, "SFX_Pool", inForce: true, null);
		LabTestObject component = gameObject.GetComponent<LabTestObject>();
		if (component != null)
		{
			if (scienceExperimentData.mLabItem != null)
			{
				gameObject.transform.rotation = Quaternion.Euler(scienceExperimentData.mLabItem.pDefaultRotation);
			}
			component.Initialize(scienceExperimentData.mLabItem, _Manager);
			component.transform.localScale = scienceExperimentData.mLabItem.pScale;
			component.name = "[" + scienceExperimentData.mLabItem.Name + "-" + pCrucible.GetUniqueTestItemNameID() + "]";
			mObjectInHand = new InHandObjectData(InHandObjectType.TEST_ITEM, scienceExperimentData, gameObject.transform, component._OffsetFromMousePos, _Manager);
			ActivateCursor(Cursor.GLOVE);
		}
	}

	public void OnLevelReady()
	{
		SetVisibility(inVisible: true);
		UpdateCrucibleData();
		TouchManager.OnDragEvent = (OnDrag)Delegate.Combine(TouchManager.OnDragEvent, new OnDrag(OnDrag));
		TouchManager.OnDragEvent = (OnDrag)Delegate.Combine(TouchManager.OnDragEvent, new OnDrag(OnDragScoop));
		TouchManager.OnFingerUpEvent = (OnFingerUp)Delegate.Combine(TouchManager.OnFingerUpEvent, new OnFingerUp(OnFingerUp));
		TouchManager.OnFingerDownEvent = (OnFingerDown)Delegate.Combine(TouchManager.OnFingerDownEvent, new OnFingerDown(OnFingerDown));
	}

	private void UpdateStopWatch()
	{
		if (_Manager != null)
		{
			mTxtTime.SetText(UtUtilities.GetTimerString((int)Mathf.Round(_Manager.pTimer)) + " Sec");
		}
	}

	private void UpdateThermometer()
	{
		if (mThermometerEnabled && !(_Manager == null) && !(_Mercury == null) && pThermometer != null)
		{
			float num = (Mathf.Min(Mathf.Max(pThermometer.pMinLevel, pThermometer.pThermometerLevel), pThermometer.pMaxLevel) - pThermometer.pMinLevel) * (1f / (pThermometer.pMaxLevel - pThermometer.pMinLevel));
			Vector3 localScale = _Mercury.localScale;
			localScale.y = num * 50f;
			_Mercury.localScale = localScale;
			if (pThermometer.pThermometerLevel < 0f)
			{
				_Mercury.GetComponent<Renderer>().material.mainTexture = _MercuryColdTexture;
				_MercuryBottom.material.mainTexture = _MercuryColdTexture;
			}
			else
			{
				_Mercury.GetComponent<Renderer>().material.mainTexture = _MercuryNormalTexture;
				_MercuryBottom.material.mainTexture = _MercuryNormalTexture;
			}
			if (mTxtThermometer != null)
			{
				mTxtThermometer.SetText(pCrucible.pTemperature.ToString("F1") + " C");
			}
		}
	}

	private void UpdateWeighMachine()
	{
		if (mWeighingMachineEnabled && !(_Manager == null) && mWeighingMachine != null)
		{
			float weight = LabObject.GetWeight();
			mWeighingMachine.SetWeight(weight);
			float num = 0f;
			if (Mathf.Abs(mWeighingMachine.pCurrentWeightPoint - weight) <= 0.01f)
			{
				num = weight;
			}
			else
			{
				num = mWeighingMachine.pCurrentWeightPoint;
				mWeighingMachine.Update(Time.deltaTime);
			}
			mWeighingMachineWidget.SetText(num.ToString("F1") + " Kg");
		}
	}

	private void UpdateOhmMeter()
	{
		if (!mOhmMeterEnabled || _Manager == null || mOhmMeter == null)
		{
			return;
		}
		float resistance = LabObject.GetResistance();
		float num = (float.IsNegativeInfinity(resistance) ? _OhmMeterInfo._NeedleMaxReading : resistance);
		float num2 = ((!mElectricFlow) ? _OhmMeterInfo._NeedleMinReading : (float.IsNegativeInfinity(resistance) ? _OhmMeterInfo._NeedleMaxReading : resistance));
		mOhmMeter.SetResistance(num2);
		float num3 = 0f;
		if (Mathf.Abs(mOhmMeter.pCurrentResistanceReading - num2) <= 0.01f)
		{
			num3 = num2;
			if (num3 == resistance)
			{
				mElectrifyDone = true;
			}
		}
		else
		{
			num3 = mOhmMeter.pCurrentResistanceReading;
			mOhmMeter.Update(Time.deltaTime);
		}
		num3 = mOhmMeter.ComputeNeedleVibration(num3, num);
		_OhmMeterNeedle.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(_OhmMeterInfo._NeedleMinAngle, Mathf.Lerp(_OhmMeterInfo._NeedleMinAngle, _OhmMeterInfo._NeedleMaxAngle, num / _OhmMeterInfo._NeedleMaxReading), num3 / num));
		mOhmMeterWidget.SetText(num3.ToString("F1") + " Ohm");
	}

	private void HandleObjectPickup()
	{
		if (mCurrentCursor != Cursor.GLOVE || mObjectInHand != null || _Manager == null || !KAInput.GetMouseButtonDown(0) || pUserPromptOn)
		{
			return;
		}
		int layerMask = ~(1 << LayerMask.NameToLayer("Wall"));
		if (!Physics.Raycast(_MainCamera.ScreenPointToRay(Input.mousePosition), out var hitInfo, 50f, layerMask))
		{
			return;
		}
		LabTestObject component = hitInfo.collider.gameObject.GetComponent<LabTestObject>();
		if (component != null && component.pTestItem != null && component.pTestItem.Pickable && component.pPickable && !LabTweenScale.IsScaling(component.gameObject))
		{
			if (pCrucible.HasItemInCrucible(component))
			{
				pCrucible.RemoveTestItem(component, inDestroy: false);
			}
			component.SendMessage("OnObjectPickup", SendMessageOptions.DontRequireReceiver);
			mObjectInHand = new InHandObjectData(InHandObjectType.TEST_ITEM, null, hitInfo.collider.transform, component._OffsetFromMousePos, _Manager);
			hitInfo.collider.transform.rotation = Quaternion.Euler(component.pTestItem.pDefaultRotation);
		}
	}

	private void HandleInHandObject()
	{
		if (mObjectInHand == null || _Crucible == null || ScientificExperiment.pInstance == null)
		{
			return;
		}
		if (mObjectInHand.pObject == null)
		{
			mObjectInHand = null;
			return;
		}
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = _Crucible.position.z + 2f;
		Vector3 position = _MainCamera.ScreenToWorldPoint(mousePosition) + mObjectInHand.pOffsetPosFromMousePtr;
		position.z = 0f;
		mObjectInHand.SetPosition(position);
		Vector3 vector = _MainCamera.ScreenToWorldPoint(mousePosition) - _MainCamera.transform.position;
		RaycastHit hitInfo;
		switch (mObjectInHand.pType)
		{
		case InHandObjectType.TEST_ITEM:
			if (KAInput.GetMouseButtonUp(0) && Physics.Raycast(_MainCamera.transform.position, vector.normalized, out hitInfo, 50f) && !(hitInfo.transform != _Crucible) && ScientificExperiment.pInstance.pExperimentType != ExperimentType.SKRILL_LAB)
			{
				pCrucible.AddTestItem(mObjectInHand.pObject.gameObject, LabCrucible.ItemPositionOption.MARKER, Vector3.zero, Quaternion.identity, mObjectInHand.pObject.localScale, null, null, null);
				ActivateCursor(Cursor.DEFAULT);
				mObjectInHand = null;
			}
			break;
		case InHandObjectType.ICE:
			if (KAInput.GetMouseButton(0) && Physics.Raycast(_MainCamera.transform.position, vector.normalized, out hitInfo, 50f) && !(hitInfo.transform != _Manager._CrucibleTriggerBig))
			{
				SnChannel.Play(_Manager._IceDropSFX, "SFX_Pool", inForce: true, null);
				FreezeCrucible();
				UnityEngine.Object.Destroy(mObjectInHand.pObject.gameObject);
				mObjectInHand = null;
				ActivateCursor(Cursor.DEFAULT);
			}
			break;
		}
		if (mObjectInHand == null || ((!KAInput.GetMouseButtonUp(0) || mObjectInHand.pType != 0) && mCurrentCursor != 0 && (!KAInput.GetMouseButtonDown(0) || mObjectInHand.pType != InHandObjectType.ICE)))
		{
			return;
		}
		if (!mObjectInHand.Drop() && mObjectInHand.pObject != null)
		{
			if (ScientificExperiment.pInstance.pExperimentType == ExperimentType.SKRILL_LAB)
			{
				_Manager.ShowRemoveFx(mObjectInHand.pObject);
			}
			UnityEngine.Object.Destroy(mObjectInHand.pObject.gameObject);
		}
		else
		{
			LabTestObject component = mObjectInHand.pObject.GetComponent<LabTestObject>();
			if (component != null && component.pColliderGO != null)
			{
				Ray ray = _MainCamera.ScreenPointToRay(Input.mousePosition);
				if (component.pColliderGO.GetComponent<Collider>().bounds.IntersectRay(ray))
				{
					Vector3 position2 = component.transform.position;
					position2.x = _Crucible.position.x;
					component.transform.position = position2;
					mObjectInHand.pObject.SendMessage("OnObjectRelease", pCrucible, SendMessageOptions.DontRequireReceiver);
				}
				else if (component.pTestItem.DestroyOnInvalidDrop)
				{
					_Manager.ShowRemoveFx(mObjectInHand.pObject);
					UnityEngine.Object.Destroy(mObjectInHand.pObject.gameObject);
				}
			}
			pDroppedPosition = mObjectInHand.pObject.position;
			pDropStartedTime = Time.timeSinceLevelLoad;
		}
		mObjectInHand = null;
		ActivateCursor(Cursor.DEFAULT);
	}

	private void HandleIceScoop()
	{
		if (mCurrentCursor != Cursor.SCOOP || mObjectInHand != null || _Manager == null || !KAInput.GetMouseButtonDown(0) || !Physics.Raycast(_MainCamera.ScreenPointToRay(Input.mousePosition), out var hitInfo, 50f) || !(hitInfo.collider == _Manager._IceBox) || !(_Manager._IceOnCursor != null))
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(_Manager._IceOnCursor, Vector3.up * 5000f, Quaternion.identity);
		if (gameObject != null)
		{
			SnChannel.Play(_Manager._IceScoopSFX, "SFX_Pool", inForce: true, null);
			mObjectInHand = new InHandObjectData(InHandObjectType.ICE, null, gameObject.transform, Vector3.zero, _Manager);
			LabObject component = gameObject.GetComponent<LabObject>();
			if (component != null)
			{
				component.Initialize(_Manager);
			}
		}
	}

	private void HandleMixing()
	{
		if (_Crucible == null || pCrucible == null || mCursorUpdatedFrameCount == Time.frameCount || LabCrucible.TestItemLoader.IsLoading() || mObjectInHand != null)
		{
			return;
		}
		if (mMixing)
		{
			mMixingTimer += Time.deltaTime;
			if (mMixingTimer >= 3f)
			{
				mMixing = false;
				_Manager.Pestle();
				mMixingTimer = 0f;
			}
		}
		if (mSwipeTimer > 0f)
		{
			mSwipeTimer = Mathf.Max(0f, mSwipeTimer - Time.deltaTime);
			if (mSwipeTimer <= 0f)
			{
				pCrucible.StopMixing();
				mSwipeCount = 0;
			}
		}
		if (mCurrentCursor == Cursor.PESTLE && KAInput.GetMouseButtonUp(0))
		{
			ActivateCursor(Cursor.DEFAULT);
		}
	}

	public void UpdateCrucibleData()
	{
		if (mLblCrucibleItems == null || !mLblCrucibleItems.GetVisibility())
		{
			return;
		}
		if (pCrucible == null || pCrucible.pTestItems == null || pCrucible.pTestItems.Count == 0)
		{
			mLblCrucibleItems.SetText("No item in crucible");
			return;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (LabTestObject pTestItem in pCrucible.pTestItems)
		{
			if (pTestItem != null && pTestItem.pTestItem != null)
			{
				string localizedString = pTestItem.pTestItem.DisplayNameText.GetLocalizedString();
				if (dictionary.ContainsKey(localizedString))
				{
					dictionary[localizedString]++;
				}
				else
				{
					dictionary.Add(localizedString, 1);
				}
			}
		}
		if (dictionary.Count == 0)
		{
			mLblCrucibleItems.SetText("No item in crucible");
			return;
		}
		string text = string.Empty;
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			text = text + "\n" + item.Key + " " + item.Value;
		}
		if (string.IsNullOrEmpty(text))
		{
			mLblCrucibleItems.SetText("No item in crucible");
		}
		else
		{
			mLblCrucibleItems.SetText("Items in crucible" + text);
		}
	}

	public bool InitializeExperiment(Experiment inExperiment)
	{
		if (inExperiment == null)
		{
			return false;
		}
		List<LabItem> items = inExperiment.GetItems();
		if (items == null || items.Count == 0)
		{
			return false;
		}
		mLabItemInitCount = items.Count;
		mLabItemBundleCount = mLabItemInitCount;
		if (mLabItemInitCount > 0)
		{
			ActivateCursor(Cursor.LOADING);
			SetState(KAUIState.NOT_INTERACTIVE);
			_ExperimentItemMenu.SetState(KAUIState.NOT_INTERACTIVE);
			foreach (LabItem item in items)
			{
				if (item == null || string.IsNullOrEmpty(item.Icon))
				{
					mLabItemBundleCount--;
					continue;
				}
				item.Initialize(OnLabtItemInitialized, LabData.pInstance.RoomTemperature);
				ScienceExperimentData scienceExperimentData = new ScienceExperimentData(item);
				KAWidget kAWidget = DuplicateWidget(_ExperimentItemMenu._Template);
				kAWidget.gameObject.SetActive(value: true);
				kAWidget.SetUserData(scienceExperimentData);
				kAWidget.SetText(item.DisplayNameText.GetLocalizedString());
				_ExperimentItemMenu.AddWidget(kAWidget);
				kAWidget.SetState(KAUIState.INTERACTIVE);
				scienceExperimentData.LoadResource(OnLabtItemBundleReady);
			}
		}
		return true;
	}

	public void OnLabtItemInitialized()
	{
		mLabItemInitCount--;
		if (mLabItemInitCount == 0)
		{
			ActivateCursor(Cursor.DEFAULT);
			SetState(KAUIState.INTERACTIVE);
			_ExperimentItemMenu.SetState(KAUIState.INTERACTIVE);
		}
	}

	public void OnLabtItemBundleReady()
	{
		mLabItemBundleCount--;
	}

	public void OnExperimentCompleted()
	{
		ShowCompletedText();
	}

	private void ShowCompletedText()
	{
		if (!(mTxtDirections == null) && _CompleteText != null)
		{
			mCurrentDirectionTask = null;
			mTxtDirections.SetText(_CompleteText.GetLocalizedString());
			mTxtDirections.SetDisabled(isDisabled: true);
		}
	}

	public void ShowExperimentDirection(LabTask inTask)
	{
		if (inTask != null)
		{
			mCurrentDirectionTask = inTask;
			ShowExperimentDirection();
		}
	}

	public void ShowExperimentDirection(int inDirection = 0)
	{
		if (_Manager == null)
		{
			return;
		}
		if (mCurrentDirectionTask == null)
		{
			mCurrentDirectionTask = _Manager.pExperiment.GetFirstTask();
		}
		if (mCurrentDirectionTask == null || _Manager == null || _Manager.pExperiment == null)
		{
			mTxtDirections.SetText(string.Empty);
			return;
		}
		if (inDirection < 0)
		{
			mCurrentDirectionTask = _Manager.pExperiment.GetPreviousTask(mCurrentDirectionTask);
			if (mCurrentDirectionTask == null)
			{
				mCurrentDirectionTask = _Manager.pExperiment.GetLastTask();
			}
			ShowExperimentDirection();
		}
		else if (inDirection == 0)
		{
			if (mBtnCheckbox != null)
			{
				mBtnCheckbox.SetChecked(mCurrentDirectionTask.pDone);
			}
			if (mTxtDirections != null)
			{
				mTxtDirections.SetDisabled(mCurrentDirectionTask.pDone);
				mTxtDirections.SetText(mCurrentDirectionTask.InstructionText.GetLocalizedString());
			}
		}
		else if (inDirection > 0)
		{
			mCurrentDirectionTask = _Manager.pExperiment.GetNextTask(mCurrentDirectionTask);
			if (mCurrentDirectionTask == null)
			{
				mCurrentDirectionTask = _Manager.pExperiment.GetFirstTask();
			}
			ShowExperimentDirection();
		}
		_Manager.pCrucible.OnLabTaskUpdated(mCurrentDirectionTask);
	}

	private void HeatCrucible()
	{
		if (_Manager != null)
		{
			_Manager.BreathFlame(inBreath: true);
		}
	}

	private void FreezeCrucible()
	{
		if (_Manager != null)
		{
			_Manager.UseIceSet(inUseIceScoop: true);
		}
	}

	private void EnableElectricFlow()
	{
		if (_Manager != null)
		{
			_Manager.BreatheElectricity(inBreathe: true);
		}
	}

	private void TriggerWater()
	{
		ActivateCursor(Cursor.NONE);
		if (_Manager != null)
		{
			_Manager.AddWater();
		}
	}

	private void Reset()
	{
		if (pCrucible != null)
		{
			pCrucible.Reset();
		}
		mElectrifyDone = false;
		mElectricFlow = false;
		mMixing = false;
		mMixingTimer = 0f;
		if (_TitrationPHMenu != null)
		{
			_TitrationPHMenu.SetVisibility(inVisible: false);
			_TitrationPHMenu._BackgroundObject.SetActive(value: false);
		}
		if (mTitrationGoalWidget != null)
		{
			mTitrationGoalWidget.SetVisibility(inVisible: false);
		}
		if (mTxtExperimentResult != null)
		{
			mTxtExperimentResult.SetVisibility(inVisible: false);
		}
		GameObject[] array = LabObject.pList.ToArray();
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		LabObject.pList.Clear();
		if (mThermometerEnabled)
		{
			mThermometerEnabled = false;
			if (_Thermometer != null)
			{
				if (mThermometerEnabled)
				{
					SlideIn(_Thermometer.gameObject);
				}
				else
				{
					SlideOut(_Thermometer.gameObject);
				}
			}
		}
		if (_Manager.pShowClock)
		{
			_Manager.pShowClock = false;
		}
		if (mWeighingMachineEnabled)
		{
			TriggerWeighingMachine(!mWeighingMachineEnabled);
		}
		if (mOhmMeterEnabled)
		{
			TriggerOhmMeter(!mOhmMeterEnabled);
		}
		_Manager.PlayDragonAnim("LabApprovalResponse", inPlayOnce: true);
		SnChannel snChannel = SnChannel.Play(_Manager._ApprovalSFX, "Default_Pool4", inForce: true, null);
		if (snChannel != null)
		{
			snChannel.pLoop = false;
		}
		ActivateCursor(Cursor.DEFAULT);
		if (_ResetParticle != null)
		{
			_ResetParticle.Play(withChildren: true);
		}
		mObjectInHand = null;
		_Manager.Reset();
	}

	private CursorData GetCursorData(Cursor type)
	{
		return _CursorList.Find((CursorData data) => data._Type == type);
	}

	public void ActivateCursor(Cursor inCursor)
	{
		if (mCurrentCursor == inCursor || LabItemCombination.pIsLoading || LabCrucible.TestItemLoader.IsLoading())
		{
			return;
		}
		if (mCurrentCursor == Cursor.PESTLE)
		{
			mSwipeTimer = 0f;
			pCrucible.StopMixing();
		}
		mCursorUpdatedFrameCount = Time.frameCount;
		SetCursorScale("Activate", _DefaultCursorSize);
		SetCursorScale("Custom", _DefaultCursorSize);
		CursorData cursorData = GetCursorData(inCursor);
		switch (inCursor)
		{
		case Cursor.DEFAULT:
			KAUICursorManager.SetDefaultCursor("Activate");
			break;
		case Cursor.FEATHER:
			if (_Manager._Gronckle != null)
			{
				_Manager._Gronckle.OnFeatherToolActivate();
			}
			goto case Cursor.DEFAULT;
		case Cursor.PESTLE:
		case Cursor.SCOOP:
		case Cursor.SCOOP_WITH_ICE:
			UpdateStoolTool(cursorData._StoolTool);
			break;
		case Cursor.GLOVE:
			KAUICursorManager.SetDefaultCursor("Grab");
			break;
		case Cursor.LOADING:
			KAUICursorManager.SetDefaultCursor("Loading");
			break;
		}
		mCurrentCursor = inCursor;
	}

	private void UpdateCursor(CursorData data)
	{
		SetCursorScale("Custom", data._Size);
		KAUICursorManager.SetCustomCursor(string.Empty, data._Texture);
		KAUICursorManager.SetDefaultCursor("Custom");
	}

	public void TriggerWeighingMachine(bool inEnable)
	{
		mWeighingMachineEnabled = inEnable;
		if (mWeighingMachineEnabled && mWeighingMachine == null && _Manager != null)
		{
			mWeighingMachine = new SEWeighingMachine(_Manager._DefaultMaxWeight, _Manager._WeighingMachineSpeed, _Manager._WeighingMachineBrake);
			mWeighingMachine.Reset();
		}
		if (mWeighingMachine == null)
		{
			return;
		}
		mWeighingMachine.SetWeight(Mathf.Max(0f, LabObject.GetWeight()));
		if (_WeighingMachine != null)
		{
			if (mWeighingMachineEnabled)
			{
				SlideIn(_WeighingMachine.gameObject);
			}
			else
			{
				SlideOut(_WeighingMachine.gameObject);
			}
		}
	}

	public void TriggerOhmMeter(bool inEnable)
	{
		mOhmMeterEnabled = inEnable;
		_Manager.pCrucible.RemoveAllTestItems();
		if (mOhmMeterEnabled && mOhmMeter == null && _Manager != null)
		{
			mOhmMeter = new SEOhmMeter(_OhmMeterInfo);
			mOhmMeter.Reset();
		}
		if (mOhmMeter == null)
		{
			return;
		}
		mOhmMeter.Reset();
		if (_OhmMeter != null)
		{
			if (mOhmMeterEnabled)
			{
				SlideIn(_OhmMeter.gameObject);
			}
			else
			{
				SlideOut(_OhmMeter.gameObject);
			}
		}
	}

	private void TriggerJournal()
	{
		if (!(mJournal != null) && !mLoadingJournal)
		{
			if (SECrucibleContextSensitive.pInstance != null)
			{
				SECrucibleContextSensitive.pInstance.DestroyMe();
			}
			if (SEToolboxContextSensitive.pInstance != null)
			{
				SEToolboxContextSensitive.pInstance.DestroyMe();
			}
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
			}
			if (_Manager.pExperimentType == ExperimentType.TITRATION_LAB && _TitrationPHMenu != null && _TitrationPHMenu.GetVisibility())
			{
				_TitrationPHMenu.SetVisibility(inVisible: false);
				_TitrationPHMenu._BackgroundObject.SetActive(value: false);
			}
			if (_ExperimentItemMenu != null)
			{
				_ExperimentItemMenu.SetVisibility(inVisible: false);
			}
			SetVisibility(inVisible: false);
			mLoadingJournal = true;
			_Manager.OpenJournal(open: true);
			JournalLoader.Load(string.Empty, string.Empty, setDefaultMenuItem: false, base.gameObject);
			ActivateCursor(Cursor.NONE);
			KAUICursorManager.SetDefaultCursor("Loading");
		}
	}

	public void JournalActivated(GameObject inObject)
	{
		if (inObject != null)
		{
			mJournal = inObject.GetComponent<UiJournal>();
			if (mJournal != null)
			{
				mJournal._ExitMessageObject = base.gameObject;
			}
			mLoadingJournal = false;
			if (pCrucible != null)
			{
				pCrucible.OnJournalActivated();
			}
		}
	}

	public void OnJournalExit()
	{
		_Manager.OpenJournal(open: false);
		mJournal = null;
		if (_ExperimentItemMenu != null)
		{
			_ExperimentItemMenu.SetVisibility(inVisible: true);
		}
		if (_Manager.pExperimentType == ExperimentType.TITRATION_LAB && _TitrationPHMenu != null && ((LabTitrationCrucible)pCrucible).pIsTitrationAgentPresent)
		{
			_TitrationPHMenu.SetVisibility(inVisible: true);
			_TitrationPHMenu._BackgroundObject.SetActive(value: true);
		}
		if (MissionManager.pInstance != null && !LabTutorial.InTutorial)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
		}
		SetVisibility(inVisible: true);
		AvAvatar.SetActive(inActive: false);
		if (pCrucible != null)
		{
			pCrucible.OnJournalExit();
		}
		ActivateCursor(Cursor.DEFAULT);
		StartCoroutine(WaitForFrameEnd());
	}

	private IEnumerator WaitForFrameEnd()
	{
		yield return new WaitForEndOfFrame();
		if (_Manager.pCurrentDragon.AIActor != null)
		{
			_Manager.pCurrentDragon.AIActor.SetState(AISanctuaryPetFSM.SCIENCE_LAB);
		}
	}

	public void OnExperimentTaskDone(LabTask inExpTask)
	{
		if (inExpTask != null)
		{
			ShowExperimentDirection(inExpTask);
		}
	}

	public void HideUserPrompt(string inID = "Close")
	{
		if (mBtnUserPrompt != null)
		{
			mBtnUserPrompt.SetText(string.Empty);
			mBtnUserPrompt.SetVisibility(inVisible: false);
		}
		if (mTxtUserPrompt != null)
		{
			mTxtUserPrompt.SetText(string.Empty);
			mTxtUserPrompt.SetVisibility(inVisible: false);
		}
		if (_Manager.pTimeEnabled)
		{
			if (pCrucible.pFreezing)
			{
				SnChannel.Play(_Manager._IceMeltSFX, "IceMelt_Pool", inForce: true, null);
			}
			SnChannel.PlayPool("Timer_Pool");
		}
		mUserPromptOn = false;
		OnUsePromptChanged();
		if (!LabTutorial.InTutorial)
		{
			_ExperimentItemMenu.SetInteractive(interactive: true);
		}
		if (mUserPromptTextCallback != null)
		{
			mUserPromptTextCallback(inID, mUserPromptUserData);
			mUserPromptTextCallback = null;
		}
	}

	public void ShowUserPromptText(string inBtnText, bool inClickable, UserPromptTextCallback inCallback, bool inShowCloseBtn, bool inSetTimerInteractive, object inUserData)
	{
		if (!string.IsNullOrEmpty(inBtnText))
		{
			KAWidget kAWidget = (inClickable ? mBtnUserPrompt : mTxtUserPrompt);
			if (kAWidget != null)
			{
				kAWidget.SetText(inBtnText);
			}
			if (mBtnUserPrompt != null)
			{
				mBtnUserPrompt.SetVisibility(inClickable);
			}
			if (mTxtUserPrompt != null)
			{
				mTxtUserPrompt.SetVisibility(!inClickable);
			}
		}
		mTimerInteractiveOnUserPrompt = inSetTimerInteractive;
		mUserPromptOn = true;
		OnUsePromptChanged();
		SnChannel.PausePool("Timer_Pool");
		_ExperimentItemMenu.SetInteractive(interactive: false);
		mUserPromptTextCallback = inCallback;
		mUserPromptUserData = inUserData;
	}

	public bool IsVisible(LabTool inTool)
	{
		switch (inTool)
		{
		case LabTool.CLOCK:
			if (_Timer != null)
			{
				return mSlideStates[_Timer.gameObject] == LabSlideState.SLID_IN;
			}
			return false;
		case LabTool.THERMOMETER:
			if (_Thermometer != null)
			{
				return mSlideStates[_Thermometer.gameObject] == LabSlideState.SLID_IN;
			}
			return false;
		case LabTool.WEIGHINGMACHINE:
			if (_WeighingMachine != null)
			{
				return mSlideStates[_WeighingMachine.gameObject] == LabSlideState.SLID_IN;
			}
			return false;
		case LabTool.OHMMETER:
			if (_OhmMeter != null)
			{
				return mSlideStates[_OhmMeter.gameObject] == LabSlideState.SLID_IN;
			}
			return false;
		default:
			return false;
		}
	}

	private bool SlideOut(GameObject inObject)
	{
		if (inObject == null)
		{
			return false;
		}
		if (mSlideStates[inObject] == LabSlideState.SLID_IN || mSlideStates[inObject] == LabSlideState.SLIDING_IN)
		{
			Vector2 inTargetPos = Vector2.zero;
			float inTime = 0f;
			bool flag = false;
			LabSlider labSlider = null;
			if (inObject == _Thermometer.gameObject && _ThermometerSlideOutInfo != null)
			{
				labSlider = _Thermometer;
				inTargetPos = _ThermometerSlideOutInfo._ToPos;
				inTime = _ThermometerSlideOutInfo._Time;
				flag = true;
				if (mTxtThermometer != null)
				{
					mTxtThermometer.SetVisibility(inVisible: false);
				}
			}
			else if (inObject == _Timer.gameObject && _TimeSlideOutInfo != null)
			{
				labSlider = _Timer;
				inTargetPos = _TimeSlideOutInfo._ToPos;
				inTime = _TimeSlideOutInfo._Time;
				flag = true;
				if (mTxtTime != null)
				{
					mTxtTime.SetVisibility(inVisible: false);
				}
			}
			else if (inObject == _WeighingMachine.gameObject && _WeighingToolSlideOutInfo != null)
			{
				labSlider = _WeighingMachine;
				inTargetPos = _WeighingToolSlideOutInfo._ToPos;
				inTime = _WeighingToolSlideOutInfo._Time;
				flag = true;
				if (mWeighingMachineWidget != null)
				{
					mWeighingMachineWidget.SetVisibility(inVisible: false);
				}
			}
			else if (inObject == _OhmMeter.gameObject && _OhmMeterSlideOutInfo != null)
			{
				labSlider = _OhmMeter;
				inTargetPos = _OhmMeterSlideOutInfo._ToPos;
				inTime = _OhmMeterSlideOutInfo._Time;
				flag = true;
				if (mOhmMeterWidget != null)
				{
					mOhmMeterWidget.SetVisibility(inVisible: false);
				}
			}
			if (flag)
			{
				if (labSlider != null)
				{
					labSlider.StartMove(inTargetPos, inTime, base.gameObject);
				}
				SnChannel.Play(_Manager._RemoveToolSFX, "SFX_Pool", inForce: true, null);
				mSlideStates[inObject] = LabSlideState.SLIDING_OUT;
			}
			return true;
		}
		return false;
	}

	private bool SlideIn(GameObject inObject)
	{
		if (inObject == null)
		{
			return false;
		}
		if (mSlideStates[inObject] == LabSlideState.SLID_OUT || mSlideStates[inObject] == LabSlideState.SLIDING_OUT)
		{
			Vector2 inTargetPos = Vector2.zero;
			float inTime = 0f;
			bool flag = false;
			LabSlider labSlider = null;
			if (inObject == _Thermometer.gameObject && _ThermometerSlideInInfo != null)
			{
				labSlider = _Thermometer;
				inTargetPos = _ThermometerSlideInInfo._ToPos;
				inTime = _ThermometerSlideInInfo._Time;
				flag = true;
				if (mTxtThermometer != null)
				{
					mTxtThermometer.SetVisibility(inVisible: false);
				}
			}
			else if (inObject == _Timer.gameObject && _TimeSlideInInfo != null)
			{
				labSlider = _Timer;
				inTargetPos = _TimeSlideInInfo._ToPos;
				inTime = _TimeSlideInInfo._Time;
				flag = true;
				if (mTxtTime != null)
				{
					mTxtTime.SetVisibility(inVisible: false);
				}
			}
			else if (inObject == _WeighingMachine.gameObject && _WeighingToolSlideInInfo != null)
			{
				labSlider = _WeighingMachine;
				inTargetPos = _WeighingToolSlideInInfo._ToPos;
				inTime = _WeighingToolSlideInInfo._Time;
				flag = true;
			}
			else if (inObject == _OhmMeter.gameObject && _OhmMeterSlideInInfo != null)
			{
				labSlider = _OhmMeter;
				inTargetPos = _OhmMeterSlideInInfo._ToPos;
				inTime = _OhmMeterSlideInInfo._Time;
				flag = true;
			}
			if (flag)
			{
				if (labSlider != null)
				{
					labSlider.StartMove(inTargetPos, inTime, base.gameObject);
				}
				inObject.SetActive(value: true);
				mSlideStates[inObject] = LabSlideState.SLIDING_IN;
				SnChannel.Play(_Manager._SelectToolSFX, "SFX_Pool", inForce: true, null);
				return true;
			}
		}
		return false;
	}

	public void End3DMoveTo(GameObject inObject)
	{
		EndOfMoveTo(inObject);
	}

	public void EndOfMoveTo(GameObject inObject)
	{
		bool flag = false;
		if (!(inObject == _Thermometer.gameObject) && !(inObject == _Timer.gameObject) && !(inObject == _WeighingMachine.gameObject) && !(inObject == _OhmMeter.gameObject))
		{
			return;
		}
		switch (mSlideStates[inObject])
		{
		case LabSlideState.SLIDING_IN:
			mSlideStates[inObject] = LabSlideState.SLID_IN;
			flag = true;
			if (inObject == _Thermometer.gameObject && mTxtThermometer != null)
			{
				mTxtThermometer.SetVisibility(inVisible: true);
			}
			else if (inObject == _Timer.gameObject && mTxtTime != null)
			{
				mTxtTime.SetVisibility(inVisible: true);
			}
			else if (inObject == _WeighingMachine.gameObject && mWeighingMachineWidget != null)
			{
				mWeighingMachineWidget.SetVisibility(inVisible: true);
			}
			else if (inObject == _OhmMeter.gameObject && mOhmMeterWidget != null)
			{
				mOhmMeterWidget.SetVisibility(inVisible: true);
			}
			break;
		case LabSlideState.SLIDING_OUT:
			inObject.SetActive(value: false);
			mSlideStates[inObject] = LabSlideState.SLID_OUT;
			flag = true;
			if (inObject == _Thermometer.gameObject)
			{
				mTxtThermometer.SetVisibility(inVisible: false);
			}
			else if (inObject == _Timer.gameObject && mTxtTime != null)
			{
				mTxtTime.SetVisibility(inVisible: false);
			}
			else if (inObject == _WeighingMachine.gameObject && mWeighingMachineWidget != null)
			{
				mWeighingMachineWidget.SetVisibility(inVisible: false);
			}
			else if (inObject == _OhmMeter.gameObject && mOhmMeterWidget != null)
			{
				mOhmMeterWidget.SetVisibility(inVisible: false);
			}
			break;
		case LabSlideState.SLID_IN:
			flag = SlideOut(inObject);
			break;
		case LabSlideState.SLID_OUT:
			flag = SlideIn(inObject);
			break;
		}
		if (flag && (mSlideStates[inObject] == LabSlideState.SLID_IN || mSlideStates[inObject] == LabSlideState.SLID_OUT) && pCrucible != null)
		{
			if (inObject == _Thermometer.gameObject)
			{
				OnToolVisibilityChanged(LabTool.THERMOMETER, mSlideStates[inObject] == LabSlideState.SLID_IN);
			}
			else if (inObject == _Timer.gameObject)
			{
				OnToolVisibilityChanged(LabTool.CLOCK, mSlideStates[inObject] == LabSlideState.SLID_IN);
			}
			else if (inObject == mWeighingMachineWidget.gameObject)
			{
				OnToolVisibilityChanged(LabTool.WEIGHINGMACHINE, mSlideStates[inObject] == LabSlideState.SLID_IN);
			}
			else if (inObject == mOhmMeterWidget.gameObject)
			{
				OnToolVisibilityChanged(LabTool.OHMMETER, mSlideStates[inObject] == LabSlideState.SLID_IN);
			}
		}
	}

	private void OnToolVisibilityChanged(LabTool inTool, bool inVisible)
	{
		if (inTool == LabTool.THERMOMETER && !inVisible)
		{
			_Manager.CheckForProcedureHalt("SwitchOff", "Thermometer");
		}
	}

	public void TriggerTimerObject(bool inVisible)
	{
		if (_Timer != null)
		{
			if (inVisible)
			{
				SlideIn(_Timer.gameObject);
			}
			else
			{
				SlideOut(_Timer.gameObject);
			}
		}
	}

	public static void SetCursorScale(string inCursorName, Vector3 inScale)
	{
		JSGames.UI.UIWidget uIWidget = UICursorManager.FindCursorItem(inCursorName);
		if (!(uIWidget == null))
		{
			Transform transform = null;
			transform = ((!(uIWidget._Background != null)) ? uIWidget.transform.Find("Background") : uIWidget._Background.transform);
			if (transform != null)
			{
				transform.localScale = inScale;
			}
		}
	}

	private void OnExit()
	{
		ScienceExperimentData.UnloadAll();
	}

	private void OnUsePromptChanged()
	{
		if (!(SEToolboxContextSensitive.pInstance != null))
		{
			return;
		}
		if (mUserPromptOn)
		{
			if (SEToolboxContextSensitive.pInstance.pClickable != null)
			{
				SEToolboxContextSensitive.pInstance.pClickable._OpenEnabled = false;
			}
			SEToolboxContextSensitive.pInstance.DestroyMe();
		}
		else if (SEToolboxContextSensitive.pInstance.pClickable != null)
		{
			SEToolboxContextSensitive.pInstance.pClickable._OpenEnabled = true;
		}
	}

	public bool OnDrag(Vector2 inNewPosition, Vector2 inOldPosition, int inFingerID)
	{
		if (mCurrentCursor != Cursor.PESTLE || _Crucible == null || _Manager == null || _MainCamera == null || pCrucible == null || m3DMouseObject == null)
		{
			return false;
		}
		if (mSwipeTimer == 0f)
		{
			mSwipeCount = 0;
		}
		Vector2 normalized = (inNewPosition - inOldPosition).normalized;
		if (mCurrentDirection == normalized)
		{
			return false;
		}
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = _Crucible.position.z + 2f;
		Vector3 vector = _MainCamera.ScreenToWorldPoint(mousePosition);
		m3DMouseObject.transform.position = vector;
		Vector3 vector2 = vector - _MainCamera.transform.position;
		if (mPestleLayerMask == -1)
		{
			mPestleLayerMask = ~((1 << LayerMask.NameToLayer("Wall")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Furniture")) | (1 << LayerMask.NameToLayer("Floor")));
		}
		if (Physics.Raycast(_MainCamera.transform.position, vector2.normalized, out var hitInfo, 50f, mPestleLayerMask) && (hitInfo.transform == _Manager._CrucibleTriggerSmall || pCrucible.HasItemInCrucible(hitInfo.transform)))
		{
			if (pCrucible.HasItemInCrucible(hitInfo.transform))
			{
				hitInfo.transform.SendMessage("OnPestleHit", SendMessageOptions.DontRequireReceiver);
			}
			if (mCheckForSwipeCrucible)
			{
				mCheckForSwipeCrucible = false;
				mSwipeCount++;
				if (mSwipeCount >= _PestleTriggerSwipeCount)
				{
					List<LabTestObject> crucibleItems = pCrucible.GetCrucibleItems(LabItemCategory.LIQUID, LabItemCategory.LIQUID_COMBUSTIBLE);
					if (crucibleItems != null && crucibleItems.Count > 0)
					{
						foreach (LabTestObject item in crucibleItems)
						{
							if (item != null)
							{
								item.OnPestleHit();
							}
						}
					}
					if (!mMixing)
					{
						mMixingTimer = 0f;
					}
					mMixing = true;
					pCrucible.ShowMixingEffect();
					mSwipeCount = 0;
				}
				mSwipeTimer = 1f;
			}
		}
		float num = Vector2.Dot(normalized, Vector2.right);
		if ((double)Mathf.Abs(num) > 0.7)
		{
			if (num < 0f)
			{
				if (!mIsLeftSwipe)
				{
					mIsLeftSwipe = true;
					mCheckForSwipeCrucible = true;
				}
			}
			else if (num > 0f && mIsLeftSwipe)
			{
				mIsLeftSwipe = false;
				mCheckForSwipeCrucible = true;
			}
		}
		mCurrentDirection = normalized;
		return false;
	}

	public bool OnDragScoop(Vector2 inNewPosition, Vector2 inOldPosition, int inFingerID)
	{
		if (_Manager == null || pCrucible == null)
		{
			return false;
		}
		if ((pCurrentCursor == Cursor.SCOOP || pCurrentCursor == Cursor.SCOOP_WITH_ICE) && m3DMouseObject != null)
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = _MainCamera.transform.position.z - 0.5f;
			Vector3 vector = _MainCamera.ScreenToWorldPoint(mousePosition);
			m3DMouseObject.transform.position = vector;
			Vector3 vector2 = vector - _MainCamera.transform.position;
			if (mFeatherLayerMask == -1)
			{
				mFeatherLayerMask = ~((1 << LayerMask.NameToLayer("Wall")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Furniture")) | (1 << LayerMask.NameToLayer("Floor")));
			}
			if (Physics.Raycast(vector, vector2.normalized, out var hitInfo, 500f, mFeatherLayerMask))
			{
				if (hitInfo.transform == _IceContainerTriggerArea && mCurrentCursor == Cursor.SCOOP)
				{
					UnityEngine.Object.Destroy(m3DMouseObject);
					m3DMouseObject = null;
					ActivateCursor(Cursor.SCOOP_WITH_ICE);
					m3DMouseObject = mStoolTool;
					mStoolTool = null;
					UpdateStoolTool(null);
					SnChannel.Play(_Manager._IceButtonSFX, "SFX_Pool", inForce: true, null);
				}
				else if (hitInfo.transform == _Manager._CrucibleTriggerBig && mCurrentCursor == Cursor.SCOOP_WITH_ICE)
				{
					Remove3DMouse(playFx: false);
					_Manager.UseIceSet(inUseIceScoop: true);
					SnChannel.Play(_Manager._IceDropSFX, "SFX_Pool", inForce: true, null);
				}
			}
		}
		return false;
	}

	protected void Remove3DMouse(bool playFx = true)
	{
		if (m3DMouseObject != null)
		{
			if (playFx)
			{
				_Manager.ShowRemoveFx(m3DMouseObject.transform);
			}
			UnityEngine.Object.Destroy(m3DMouseObject);
			ActivateCursor(Cursor.DEFAULT);
		}
	}

	public void OnFingerUp(int inFingerID, Vector2 inVecPosition)
	{
		Remove3DMouse();
	}

	public void OnFingerDown(int inFingerID, Vector2 inVecPosition)
	{
		if (!(mStoolTool == null) && !(_Manager == null) && m3DMouseObject == null)
		{
			Vector2 a = _MainCamera.WorldToScreenPoint(mStoolTool.transform.position);
			a.y = (float)Screen.height - a.y;
			if (Vector2.Distance(a, inVecPosition) < 60f)
			{
				m3DMouseObject = mStoolTool;
				KAUICursorManager.SetDefaultCursor("Grab");
				mStoolTool = null;
				UpdateStoolTool(null);
			}
		}
	}

	protected void UpdateStoolTool(GameObject tool)
	{
		if (_StoolTopPivot == null)
		{
			return;
		}
		if (mStoolTool != null)
		{
			UnityEngine.Object.Destroy(mStoolTool);
		}
		if (tool != null)
		{
			mStoolTool = UnityEngine.Object.Instantiate(tool, _StoolTopPivot.position, _StoolTopPivot.rotation);
			if (_StoolSparkleFx != null)
			{
				_StoolSparkleFx.Play();
			}
		}
		else if (_StoolSparkleFx != null)
		{
			_StoolSparkleFx.Stop();
		}
	}
}
