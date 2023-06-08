using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabGronckle : MonoBehaviour
{
	[Serializable]
	public class AnimationData
	{
		public string _Name;

		public AudioClip _Clip;

		public AnimState _State;
	}

	public enum AnimState
	{
		NONE,
		EAT_INIT,
		MOUTH_OPEN,
		CHEW,
		TICKLE,
		VOMIT
	}

	public Transform _TickleTriggerArea;

	public float _EatRadius = 5f;

	public Transform _HeadBone;

	public float _TickleHeatMultiplier = 20f;

	public float _FreezeMultiplier = 6f;

	public UiLabGronckle _UiLabGronckle;

	public ParticleSystem _SmokeFx;

	public ParticleSystem _VomitFx;

	public Transform _VomitPivot;

	public GameObject _StoolObject;

	public Transform _StoolTopPivot;

	public ParticleSystem _StoolSparkleFx;

	public GameObject _FeatherTool;

	public List<AnimationData> _Animations;

	public string _SFXPoolName = "SFX_Pool";

	private bool mTickling;

	private int mFeatherLayerMask = -1;

	private LabGronckleCrucible mGronckleCrucible;

	private ScientificExperiment mManager;

	private GameObject mStoolTool;

	private GameObject m3DMouseObject;

	private bool mTriggeredVomit;

	private Dictionary<LabItem, int> mBellyItems = new Dictionary<LabItem, int>();

	protected AnimState mState;

	private Vector3 mPreviousBonePosition;

	private Vector2 mCurrentDirection = Vector3.up;

	public bool pTriggeredVomit => mTriggeredVomit;

	public LabCrucible pCrucible => mGronckleCrucible;

	public bool pIsBellyPopupShown => _UiLabGronckle.GetVisibility();

	public bool pIsEmptyBelly => pBellyItemCount == 0;

	public int pBellyItemCount => mBellyItems.Count;

	public void Init(ScientificExperiment inManager)
	{
		mManager = inManager;
		mGronckleCrucible = new LabGronckleCrucible(inManager, this, _TickleHeatMultiplier, _FreezeMultiplier);
		_UiLabGronckle.gameObject.SetActive(value: true);
		if (_StoolObject != null)
		{
			_StoolObject.SetActive(value: true);
		}
		TouchManager.OnFingerUpEvent = (OnFingerUp)Delegate.Combine(TouchManager.OnFingerUpEvent, new OnFingerUp(OnFingerUp));
		TouchManager.OnFingerDownEvent = (OnFingerDown)Delegate.Combine(TouchManager.OnFingerDownEvent, new OnFingerDown(OnFingerDown));
		TouchManager.OnDragEvent = (OnDrag)Delegate.Combine(TouchManager.OnDragEvent, new OnDrag(OnDrag));
	}

	private void Update()
	{
		HandleTickling();
		UpdateAnimState();
	}

	private void OnDestroy()
	{
		TouchManager.OnFingerUpEvent = (OnFingerUp)Delegate.Remove(TouchManager.OnFingerUpEvent, new OnFingerUp(OnFingerUp));
		TouchManager.OnFingerDownEvent = (OnFingerDown)Delegate.Remove(TouchManager.OnFingerDownEvent, new OnFingerDown(OnFingerDown));
		TouchManager.OnDragEvent = (OnDrag)Delegate.Remove(TouchManager.OnDragEvent, new OnDrag(OnDrag));
	}

	protected AnimationData GetAnimationData(AnimState state)
	{
		return _Animations.Find((AnimationData data) => data._State == state);
	}

	protected string GetAnimName(AnimState state)
	{
		AnimationData animationData = _Animations.Find((AnimationData data) => data._State == state);
		if (animationData == null)
		{
			return "";
		}
		return animationData._Name;
	}

	protected virtual void SetAnimState(AnimState newState)
	{
		if (mManager == null)
		{
			return;
		}
		AnimationData animationData = GetAnimationData(newState);
		switch (newState)
		{
		case AnimState.NONE:
			mManager.StopDragonAnim();
			break;
		case AnimState.EAT_INIT:
			mPreviousBonePosition = _HeadBone.transform.position;
			mManager.PlayDragonAnim(animationData._Name, inPlayOnce: true, playIdleNext: false);
			StartCoroutine(SetAnimState(AnimState.MOUTH_OPEN, AnimState.EAT_INIT, mManager.GetAnimLength(animationData._Name) - 0.2f));
			break;
		case AnimState.MOUTH_OPEN:
			mManager.PlayDragonAnim(animationData._Name, inPlayOnce: false, playIdleNext: false, 0.05f);
			break;
		case AnimState.CHEW:
			mManager.PlayDragonAnim(animationData._Name, inPlayOnce: true);
			if (animationData._Clip != null)
			{
				SnChannel.Play(animationData._Clip, _SFXPoolName, inForce: true, null);
			}
			break;
		}
		mState = newState;
	}

	protected IEnumerator SetAnimState(AnimState newState, AnimState trigerredState, float waitSecs)
	{
		yield return new WaitForSeconds(waitSecs);
		if (mState == trigerredState)
		{
			SetAnimState(newState);
		}
	}

	protected virtual void UpdateAnimState()
	{
		if (mManager == null || mManager._MainUI == null || mManager.pCurrentDragon == null || mManager.pCurrentDragon.animation == null)
		{
			return;
		}
		switch (mState)
		{
		case AnimState.NONE:
			if (mManager._MainUI.pObjectInHand != null && !(mManager._MainUI.pObjectInHand.pObject == null) && mManager._MainUI.pCurrentCursor != UiScienceExperiment.Cursor.SCOOP_WITH_ICE && CanEat(mManager._MainUI.pObjectInHand.pObject, _EatRadius))
			{
				SetAnimState(AnimState.EAT_INIT);
			}
			break;
		case AnimState.MOUTH_OPEN:
			if (mManager._MainUI.pObjectInHand == null || mManager._MainUI.pObjectInHand.pObject == null || !CanEat(mManager._MainUI.pObjectInHand.pObject, mPreviousBonePosition, _EatRadius))
			{
				SetAnimState(AnimState.NONE);
			}
			break;
		case AnimState.CHEW:
			if (!mManager.pCurrentDragon.IsAnimationPlaying(GetAnimName(AnimState.MOUTH_OPEN)) && !mManager.pCurrentDragon.IsAnimationPlaying(GetAnimName(AnimState.EAT_INIT)) && !mManager.pCurrentDragon.IsAnimationPlaying(GetAnimName(AnimState.CHEW)))
			{
				SetAnimState(AnimState.NONE);
			}
			break;
		case AnimState.EAT_INIT:
			break;
		}
	}

	protected bool CanEat(Transform obj, Vector3 headPos, float eatRadius)
	{
		Vector3 position = obj.position;
		headPos.z = (position.z = 0f);
		return Vector3.Distance(headPos, position) < eatRadius;
	}

	protected bool CanEat(Transform obj, float eatRadius)
	{
		return CanEat(obj, _HeadBone.position, eatRadius);
	}

	public bool ObjectDroppedToEat(UiScienceExperiment.InHandObjectData data)
	{
		if (_HeadBone != null && data.pObject != null)
		{
			if (CanEat(data.pObject, _EatRadius))
			{
				return TryToEat(data);
			}
			if (pIsBellyPopupShown)
			{
				mManager.ShowRemoveFx(data.pObject);
			}
		}
		return false;
	}

	public void OnFeatherToolActivate()
	{
		UpdateStoolTool(_FeatherTool);
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

	private bool TryToEat(UiScienceExperiment.InHandObjectData data)
	{
		LabItem labItem = null;
		if (data.pExperimentData == null || data.pExperimentData.mLabItem == null)
		{
			LabTestObject component = data.pObject.GetComponent<LabTestObject>();
			if (component != null)
			{
				labItem = component.pTestItem;
			}
		}
		else
		{
			labItem = data.pExperimentData.mLabItem;
		}
		if (labItem == null)
		{
			return false;
		}
		if (pIsEmptyBelly)
		{
			mGronckleCrucible.Reset();
		}
		if (!pIsBellyPopupShown)
		{
			SetBellyPopup(visible: true);
			if (_SmokeFx != null)
			{
				_SmokeFx.Play();
			}
		}
		SetAnimState(AnimState.CHEW);
		if (IsItemInBelly(labItem))
		{
			mBellyItems[labItem]++;
			_UiLabGronckle.UpdateBellyItemCount(labItem.Name, mBellyItems[labItem]);
			return false;
		}
		mBellyItems[labItem] = 1;
		mGronckleCrucible.AddTestItem(data.pObject.gameObject, LabCrucible.ItemPositionOption.MARKER, Vector3.zero, Quaternion.identity, data.pObject.localScale, null, null, null);
		_UiLabGronckle.AddBellyIcon(labItem);
		return true;
	}

	public bool IsItemInBelly(LabItem item)
	{
		return mBellyItems.ContainsKey(item);
	}

	public bool IsItemInBelly(string itemName)
	{
		foreach (KeyValuePair<LabItem, int> mBellyItem in mBellyItems)
		{
			if (mBellyItem.Key.Name.Equals(itemName))
			{
				return true;
			}
		}
		return false;
	}

	private void HandleTickling()
	{
		if (mManager == null || mManager._MainUI == null || pBellyItemCount == 0 || mManager._MainUI.pCurrentCursor != UiScienceExperiment.Cursor.FEATHER || !mTickling || mTriggeredVomit)
		{
			return;
		}
		AnimationData animationData = GetAnimationData(AnimState.TICKLE);
		if (animationData != null)
		{
			mManager.PlayDragonAnim(animationData._Name, inPlayOnce: true);
			if (animationData._Clip != null)
			{
				SnChannel.Play(animationData._Clip, _SFXPoolName, inForce: true, null);
			}
		}
		if (mGronckleCrucible != null)
		{
			mGronckleCrucible.Heat();
		}
	}

	public void TriggerVomit()
	{
		if (mTriggeredVomit)
		{
			return;
		}
		mTickling = false;
		mTriggeredVomit = true;
		Remove3DMouse(playFx: false);
		AnimationData animationData = GetAnimationData(AnimState.VOMIT);
		if (animationData != null && mManager.PlayDragonAnim(animationData._Name, inPlayOnce: true))
		{
			mManager.pWaitingForAnimEvent = true;
		}
		if (_VomitFx != null)
		{
			ParticleSystem particleSystem = UnityEngine.Object.Instantiate(_VomitFx, _VomitPivot.position, _VomitPivot.rotation);
			particleSystem.transform.parent = _VomitPivot;
			if (particleSystem != null)
			{
				UnityEngine.Object.Destroy(particleSystem.gameObject, particleSystem.main.startDelay.constant + particleSystem.main.duration);
			}
		}
		mGronckleCrucible.StopHeat();
		SetBellyPopup(visible: false);
		UtDebug.Log("Triggered Vomit");
	}

	private void OnAnimEvent(AvAvatarAnimEvent inEvent)
	{
		if (inEvent == null || inEvent.mData == null)
		{
			return;
		}
		string dataString = inEvent.mData._DataString;
		if (!(dataString == "Vomit"))
		{
			if (dataString == "Vomitted" && mManager != null)
			{
				mManager.pWaitingForAnimEvent = false;
			}
			return;
		}
		mGronckleCrucible.DoStateChange();
		ClearBelly();
		if (_SmokeFx != null)
		{
			_SmokeFx.Stop();
		}
		AnimationData animationData = GetAnimationData(AnimState.VOMIT);
		if (animationData != null && animationData._Clip != null)
		{
			SnChannel.Play(animationData._Clip, _SFXPoolName, inForce: true, null);
		}
		UtDebug.Log("Vomit Done");
	}

	public void ClearBelly()
	{
		mTriggeredVomit = false;
		mBellyItems.Clear();
		SetBellyPopup(visible: false);
		if (_UiLabGronckle != null)
		{
			_UiLabGronckle.Clear();
		}
		UpdateStoolTool(null);
	}

	public void SetBellyPopup(bool visible)
	{
		if (_UiLabGronckle != null)
		{
			_UiLabGronckle.SetVisibility(visible);
		}
	}

	protected void Remove3DMouse(bool playFx = true)
	{
		if (m3DMouseObject != null)
		{
			if (playFx)
			{
				mManager.ShowRemoveFx(m3DMouseObject.transform);
			}
			UnityEngine.Object.Destroy(m3DMouseObject);
			if (mManager != null && mManager._MainUI != null)
			{
				mManager._MainUI.ActivateCursor(UiScienceExperiment.Cursor.DEFAULT);
			}
		}
	}

	public void OnFingerDown(int inFingerId, Vector2 inVecPosition)
	{
		if (!(mStoolTool == null) && !(mManager == null) && !(mManager._MainUI == null) && m3DMouseObject == null)
		{
			Vector2 a = mManager._MainUI._MainCamera.WorldToScreenPoint(mStoolTool.transform.position);
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

	public void OnFingerUp(int inFingerID, Vector2 inVecPosition)
	{
		Remove3DMouse();
	}

	public bool OnDrag(Vector2 inNewPosition, Vector2 inOldPosition, int inFingerID)
	{
		if (mManager == null || mGronckleCrucible == null || mTriggeredVomit)
		{
			return false;
		}
		if (mManager._MainUI.pCurrentCursor == UiScienceExperiment.Cursor.FEATHER && m3DMouseObject != null)
		{
			Camera mainCamera = mManager._MainUI._MainCamera;
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = base.transform.position.z + 2f;
			Vector3 vector = mainCamera.ScreenToWorldPoint(mousePosition);
			mousePosition = Input.mousePosition;
			mousePosition.z = mainCamera.transform.position.z - 1f;
			Vector3 position = mainCamera.ScreenToWorldPoint(mousePosition);
			m3DMouseObject.transform.position = position;
			Vector2 vector2 = inNewPosition - inOldPosition;
			if (mCurrentDirection == vector2.normalized)
			{
				mTickling = false;
				return false;
			}
			Vector3 vector3 = vector - mainCamera.transform.position;
			if (mFeatherLayerMask == -1)
			{
				mFeatherLayerMask = ~((1 << LayerMask.NameToLayer("Wall")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Furniture")) | (1 << LayerMask.NameToLayer("Floor")));
			}
			mTickling = false;
			if (Physics.Raycast(vector, vector3.normalized, out var hitInfo, 50f, mFeatherLayerMask) && hitInfo.transform == _TickleTriggerArea)
			{
				mTickling = true;
			}
			if (mTickling && vector2.sqrMagnitude < 20f)
			{
				mTickling = false;
			}
			mCurrentDirection = vector2.normalized;
		}
		return false;
	}
}
