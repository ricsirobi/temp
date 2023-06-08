using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimalFarmItem : FarmItem
{
	[Serializable]
	public class AnimationData
	{
		public AnimalState _State;

		public string _Name;
	}

	private List<ItemStateCriteriaConsumable> mConsumedList = new List<ItemStateCriteriaConsumable>();

	public int _FeedConsumptionFrequency = 15;

	public SplineControl _SplineControl;

	public Color _HarvestReadyColor = Color.green;

	public Color _HarvestNotReadyColor = Color.grey;

	public DrinkPoints _DrinkPoint;

	public string _HarvestName = "SheepHarvest";

	public List<string> _NonWalkableStages;

	public MinMax _WalkSpeedMinMax = new MinMax(0.2f, 1f);

	public MinMax _IdleAnimSpeedMinMax = new MinMax(0.3f, 1.5f);

	public float _WalkAnimSpeedFactor = 1f;

	public GameObject _FeedFx;

	private DateTime mLastWaterUseTime;

	protected AnimalState mCurrentState;

	private float mCurrentTime;

	private DateTime mStartTime;

	private Transform mAvatarMarker;

	public float _IdleTime = 10f;

	public float _WalkTime = 15f;

	public float _TroughReachTime = 5f;

	public float _DrinkTime = 5f;

	public GUISkin _TextSkin;

	private float mStateTimer;

	private float mCurrentPosOnPenSpline;

	public List<AnimationData> _AnimationDatas;

	public Transform pAvatarMarker
	{
		set
		{
			mAvatarMarker = value;
		}
	}

	public AnimalPenFarmItem pPen => (AnimalPenFarmItem)mParent;

	protected virtual string GetAnimationName(AnimalState state)
	{
		foreach (AnimationData animationData in _AnimationDatas)
		{
			if (animationData._State == state)
			{
				return animationData._Name;
			}
		}
		return null;
	}

	protected override void Update()
	{
		base.Update();
		UpdateState();
	}

	private bool IsCurrentStageFeedConsumed()
	{
		if (base.pIsStateSet && base.pIsRuleSetInitialized)
		{
			foreach (StateDetails stateDetail in _StateDetails)
			{
				if (stateDetail._ID != base.pCurrentStage._ID || stateDetail._CriteriaConsumables == null)
				{
					continue;
				}
				if (stateDetail._CriteriaConsumables.Count > mConsumedList.Count)
				{
					return false;
				}
				foreach (ItemStateCriteriaConsumable criteriaConsumable in stateDetail._CriteriaConsumables)
				{
					foreach (ItemStateCriteriaConsumable mConsumed in mConsumedList)
					{
						if (mConsumed.ItemID == criteriaConsumable.ItemID && mConsumed.Amount != criteriaConsumable.Amount)
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	public override void ProcessCurrentStage()
	{
		base.ProcessCurrentStage();
		if (mInitialized && base.pIsRuleSetInitialized && base.pIsStateSet)
		{
			UpdateContextIcon();
			base.pCurrentStage._Name.Equals(_HarvestName);
		}
	}

	private void ConsumeFeed()
	{
		if (_StateDetails == null || !base.pIsStateSet || !base.pIsRuleSetInitialized)
		{
			return;
		}
		foreach (StateDetails stateDetail in _StateDetails)
		{
			if (base.pCurrentStage._ID != stateDetail._ID)
			{
				continue;
			}
			foreach (ItemStateCriteriaConsumable criteriaConsumable in stateDetail._CriteriaConsumables)
			{
				int quantity = CommonInventoryData.pInstance.GetQuantity(criteriaConsumable.ItemID);
				if (criteriaConsumable.Amount > quantity)
				{
					continue;
				}
				ItemStateCriteriaConsumable itemStateCriteriaConsumable = new ItemStateCriteriaConsumable();
				itemStateCriteriaConsumable.ItemID = criteriaConsumable.ItemID;
				itemStateCriteriaConsumable.Amount = criteriaConsumable.Amount;
				bool flag = false;
				foreach (ItemStateCriteriaConsumable mConsumed in mConsumedList)
				{
					if (mConsumed.ItemID == itemStateCriteriaConsumable.ItemID)
					{
						mConsumed.Amount = criteriaConsumable.Amount;
						flag = true;
					}
				}
				if (!flag)
				{
					mConsumedList.Add(itemStateCriteriaConsumable);
				}
			}
		}
	}

	public void SetInternalState(AnimalState inState)
	{
		if (mCurrentState == inState)
		{
			return;
		}
		if ((inState == AnimalState.Walk || inState == AnimalState.Flying || inState == AnimalState.InitDrink || inState == AnimalState.PostDrink) && ((base.pCurrentStage != null && _NonWalkableStages != null && _NonWalkableStages.Contains(base.pCurrentStage._Name)) || base.pUI != null || !base.pIsStateSet || !base.pIsRuleSetInitialized || _SplineControl.LinearLength < 1f))
		{
			SetInternalState(AnimalState.Idle);
			return;
		}
		float animSpeed = 0f;
		switch (inState)
		{
		case AnimalState.Walk:
		case AnimalState.Flying:
			if (!base.pIsBuildMode)
			{
				_SplineControl.Speed = UnityEngine.Random.Range(_WalkSpeedMinMax.Min, _WalkSpeedMinMax.Max);
			}
			else
			{
				_SplineControl.Speed = 0f;
			}
			animSpeed = _SplineControl.Speed * _WalkAnimSpeedFactor;
			break;
		case AnimalState.Drink:
			_SplineControl.Speed = 0f;
			break;
		case AnimalState.Idle:
			_SplineControl.Speed = 0f;
			animSpeed = UnityEngine.Random.Range(_IdleAnimSpeedMinMax.Min, _IdleAnimSpeedMinMax.Max);
			break;
		case AnimalState.InitDrink:
			if (mCurrentState == AnimalState.Drink)
			{
				return;
			}
			mStateTimer = 0f;
			mCurrentPosOnPenSpline = _SplineControl.CurrentPos;
			_DrinkPoint = pPen.GetNearestFeedPoint(base.transform.position);
			if (_DrinkPoint != null)
			{
				Vector3 position = _DrinkPoint._DrinkingPoint.position;
				_DrinkPoint._isOccupied = true;
				Spline spline3 = new Spline(2, looping: false, constSpeed: true, alignTangent: true, hasQ: false);
				spline3.SetControlPoint(0, base.transform.position, Quaternion.identity, 0f);
				spline3.SetControlPoint(1, position, Quaternion.identity, 0f);
				spline3.RecalculateSpline();
				_SplineControl.SetSpline(spline3);
				_SplineControl.Speed = UnityEngine.Random.Range(_WalkSpeedMinMax.Min, _WalkSpeedMinMax.Max);
				_SplineControl.Looping = false;
				animSpeed = _SplineControl.Speed * _WalkAnimSpeedFactor;
			}
			else
			{
				SetInternalState(AnimalState.Idle);
			}
			break;
		case AnimalState.PostDrink:
		{
			AnimalPenSpline splineFromFarmItem = pPen.GetSplineFromFarmItem(this);
			if (splineFromFarmItem != null)
			{
				Spline spline = splineFromFarmItem.GetSpline();
				if (spline != null)
				{
					spline.GetPosQuatByDist(mCurrentPosOnPenSpline, out var pos, out var _);
					Spline spline2 = new Spline(2, looping: false, constSpeed: true, alignTangent: true, hasQ: false);
					spline2.SetControlPoint(0, base.transform.position, Quaternion.identity, 0f);
					spline2.SetControlPoint(1, pos, Quaternion.identity, 0f);
					spline2.RecalculateSpline();
					_SplineControl.SetSpline(spline2);
					_SplineControl.Speed = UnityEngine.Random.Range(_WalkSpeedMinMax.Min, _WalkSpeedMinMax.Max);
					_SplineControl.Looping = false;
					_DrinkPoint._isOccupied = false;
					animSpeed = _SplineControl.Speed * _WalkAnimSpeedFactor;
				}
			}
			break;
		}
		case AnimalState.None:
			_SplineControl.Speed = 0f;
			break;
		}
		PlayAnimation(inState, animSpeed);
		mCurrentState = inState;
	}

	public virtual void PlayAnimation(AnimalState inState, float animSpeed = 1f)
	{
		string animationName = GetAnimationName(inState);
		if (!string.IsNullOrEmpty(animationName))
		{
			GameObject currentStageMesh = GetCurrentStageMesh();
			if (!(currentStageMesh != null))
			{
				return;
			}
			Animation component = currentStageMesh.GetComponent<Animation>();
			if (component == null && currentStageMesh.transform.parent != null)
			{
				component = currentStageMesh.transform.parent.GetComponent<Animation>();
			}
			if (component != null && component[animationName] != null && !component.IsPlaying(animationName))
			{
				component.CrossFade(animationName);
				if (!Mathf.Approximately(animSpeed, 0f))
				{
					component[animationName].speed = animSpeed;
				}
			}
		}
		else
		{
			UtDebug.LogError("Animation name is null for state : " + inState);
		}
	}

	public void UpdateState()
	{
		switch (mCurrentState)
		{
		case AnimalState.None:
			PauseState();
			break;
		case AnimalState.Idle:
			UpdateIdle();
			break;
		case AnimalState.Walk:
			UpdateWalk();
			break;
		case AnimalState.InitDrink:
			InitializeDrink();
			break;
		case AnimalState.Drink:
			UpdateDrink();
			break;
		case AnimalState.PostDrink:
			UpdatePostDrink();
			break;
		}
	}

	private void OnSplineEnded()
	{
		AvAvatar.pObject.transform.rotation = mAvatarMarker.rotation;
		if (base.pCurrentStage != null && !base.pIsBuildMode)
		{
			GotoNextStage();
		}
	}

	private void OnPathEndReached()
	{
		AvAvatar.pObject.transform.rotation = mAvatarMarker.rotation;
		AvAvatar.pInputEnabled = true;
		if (base.pCurrentStage != null && !base.pIsBuildMode)
		{
			GotoNextStage();
		}
	}

	public override bool AddChild(FarmItem inFarmItem)
	{
		Debug.LogError("Cannot add child to sheep object!!!");
		return false;
	}

	public override void SetParent(FarmItem inFarmItem)
	{
		if (!IsDependentOnFarmItem(inFarmItem))
		{
			Debug.LogError("Only sheep pen can be added as parent of sheep item!!");
			return;
		}
		AnimalState[] array = new AnimalState[3]
		{
			AnimalState.Idle,
			AnimalState.Drink,
			AnimalState.Walk
		};
		int num = UnityEngine.Random.Range(0, array.Length);
		SetInternalState(array[num]);
		base.SetParent(inFarmItem);
	}

	public DateTime GetNextFeedUseTime()
	{
		if (!mInitialized)
		{
			return DateTime.MaxValue;
		}
		return mLastWaterUseTime.AddSeconds(_FeedConsumptionFrequency);
	}

	public int GetNextFeedUseDuration()
	{
		return GetNextFeedUseTime().Subtract(ServerTime.pCurrentTime).Seconds;
	}

	public bool UseFeed()
	{
		return UseFeedWithDate(ServerTime.pCurrentTime, inSave: true);
	}

	public bool UseFeedWithDate(DateTime inDrinkTime, bool inSave)
	{
		if (pPen == null)
		{
			return false;
		}
		ConsumeFeed();
		mLastWaterUseTime = inDrinkTime;
		SetThirsty(inThirsty: false);
		return true;
	}

	private void UpdateIdle()
	{
		if (mStateTimer <= _IdleTime)
		{
			mStateTimer += Time.deltaTime;
			return;
		}
		mStateTimer = UnityEngine.Random.Range(0f, 2f);
		SetInternalState(AnimalState.Walk);
	}

	private void UpdateWalk()
	{
		if (base.pIsBuildMode || base.pUI != null)
		{
			SetInternalState(AnimalState.Idle);
			return;
		}
		if (mStateTimer <= _WalkTime)
		{
			mStateTimer += Time.deltaTime;
			return;
		}
		mStateTimer = 0f;
		SetInternalState(AnimalState.Idle);
	}

	private void InitializeDrink()
	{
		if (_SplineControl.mEndReached)
		{
			SetInternalState(AnimalState.Drink);
		}
	}

	private void UpdateDrink()
	{
		if (base.pIsBuildMode)
		{
			PauseState();
			return;
		}
		if (mStateTimer <= _DrinkTime)
		{
			mStateTimer += Time.deltaTime;
			return;
		}
		mStateTimer = 0f;
		SetInternalState(AnimalState.PostDrink);
	}

	private void UpdatePostDrink()
	{
		if (!_SplineControl.mEndReached)
		{
			return;
		}
		SetInternalState(AnimalState.Walk);
		AnimalPenSpline animalPenSpline = pPen.SetUnusedSplineForFarmItem(this, inNew: false);
		if (animalPenSpline != null)
		{
			Spline spline = animalPenSpline.GetSpline();
			if (spline != null)
			{
				Vector3 pos;
				Quaternion quat;
				float posQuatByDist = spline.GetPosQuatByDist(mCurrentPosOnPenSpline, out pos, out quat);
				_SplineControl.SetPosOnSpline(posQuatByDist);
			}
		}
	}

	private void PauseState()
	{
		SetInternalState(AnimalState.None);
	}

	private void ApplyColor(bool isHarvestReady)
	{
		if (base.renderer != null)
		{
			base.renderer.material.color = (isHarvestReady ? _HarvestReadyColor : _HarvestNotReadyColor);
		}
	}

	public override void OnBuildModeChanged(bool inBuildMode)
	{
		base.OnBuildModeChanged(inBuildMode);
		if (inBuildMode && mCurrentState != AnimalState.Idle)
		{
			SetInternalState(AnimalState.Idle);
		}
		_SplineControl.Speed = 0f;
		mStateTimer = 0f;
	}

	public void SetThirsty(bool inThirsty)
	{
	}

	public DateTime GetCurrentTime()
	{
		return mStartTime.AddSeconds(mCurrentTime);
	}

	protected override void OnContextAction(string inActionName)
	{
		base.OnContextAction(inActionName);
		switch (inActionName)
		{
		case "Add Feed":
			ConsumeFeed();
			if (IsCurrentStageFeedConsumed())
			{
				if (InteractiveTutManager._CurrentActiveTutorialObject != null)
				{
					InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "FeededSheep");
				}
				ProcessStageUp("");
				SetInternalState(AnimalState.Drink);
			}
			else
			{
				ShowUsesLessThanRequiredDB();
			}
			break;
		case "Shear":
			KAInput.ResetInputAxes();
			ProcessStageUp(base.pFarmManager._HarvestPropName);
			break;
		case "Boost":
			if (CheckGemsAvailable(GetSpeedupCost()))
			{
				GotoNextStage(speedup: true);
			}
			break;
		case "AdBtn":
			if (AdManager.pInstance.AdAvailable(base.pFarmManager._AdEventType, AdType.REWARDED_VIDEO))
			{
				AdManager.DisplayAd(base.pFarmManager._AdEventType, AdType.REWARDED_VIDEO, base.gameObject);
			}
			break;
		}
	}

	protected override void SetupUsesLessThanRequiredDB(string inMessage)
	{
		base.pFarmManager.ShowDialog(base.pFarmManager._DialogAssetName, "PfUiNoFeed", base.pFarmManager._FarmingDBTitleText, "OnOK", "OnNo", string.Empty, string.Empty, destroyDB: true, inMessage, base.gameObject);
	}

	private void ProcessStageUp(string inPropName)
	{
		if (!string.IsNullOrEmpty(inPropName) && base.pCurrentStage._Name.Contains(inPropName) && mAvatarMarker != null)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component.pEndSplineMessageObject = base.gameObject;
				component.MoveTo(mAvatarMarker.position);
				AvAvatar.pInputEnabled = false;
				return;
			}
		}
		if (base.pCurrentStage != null && !base.pIsBuildMode)
		{
			GotoNextStage();
		}
	}

	protected override void OnSetNextItemStateError(SetNextItemStateResult inResult)
	{
		base.OnSetNextItemStateError(inResult);
		SetThirsty(inThirsty: true);
	}

	protected override void OnChangeStage(FarmItemStage prevStage, FarmItemStage curStage)
	{
		base.OnChangeStage(prevStage, curStage);
		mConsumedList.Clear();
	}

	protected override void DoHarvest()
	{
		base.DoHarvest();
		mStartTime = ServerTime.pCurrentTime;
		mCurrentTime = 0f;
	}

	protected override bool CanDestroyOnHarvest()
	{
		return false;
	}

	protected override string GetHarvestStageName()
	{
		return _HarvestName;
	}

	public void LaunchFeedStore()
	{
		if (base.pFarmManager._AnimalFeedStoreInfo != null)
		{
			StoreLoader.Load(setDefaultMenuItem: true, base.pFarmManager._AnimalFeedStoreInfo._Category, base.pFarmManager._AnimalFeedStoreInfo._Store, AvAvatar.pToolbar);
		}
	}

	protected override void OnOK()
	{
		base.OnOK();
		LaunchFeedStore();
	}

	protected override void OnNo()
	{
		base.OnOK();
	}

	public virtual void SetSpline(Spline spline)
	{
		Vector3 pos;
		Quaternion quat;
		float posQuatByDist = spline.GetPosQuatByDist(UnityEngine.Random.Range(0f, spline.mLinearLength), out pos, out quat);
		_SplineControl.SetPosOnSpline(posQuatByDist);
	}

	protected override void OnMenuActive(ContextSensitiveStateType inMenuType)
	{
		base.OnMenuActive(inMenuType);
		if (InteractiveTutManager._CurrentActiveTutorialObject != null && InteractiveTutManager._CurrentActiveTutorialObject == base.pFarmManager._AnimalTutorial.gameObject)
		{
			KAWidget kAWidget = base.pUI.FindItem("AdBtn");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
	}
}
