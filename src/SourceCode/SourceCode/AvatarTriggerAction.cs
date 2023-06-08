using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarTriggerAction : MonoBehaviour
{
	public string _HandBoneName = string.Empty;

	public Vector3 _CarryObjectOffset = Vector3.zero;

	public Vector3 _CarryObjectScale = Vector3.one;

	[Tooltip("Carrying Object will be detached from the Avatar as well as from the given NPCs on TASK_COMPLETE event for the given TaskID")]
	public int _DetatchObjectTaskID;

	public List<NPCAvatar> _DetachObjectOnNPCs = new List<NPCAvatar>();

	private AvAvatarController mAvatarController;

	private GameObject mCarryObject;

	private Transform mHandT;

	private void Start()
	{
		mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		mHandT = UtUtilities.FindChildTransform(AvAvatar.pObject, _HandBoneName);
		MissionManager.AddMissionEventHandler(OnMissionEvent);
	}

	public void SetObjectOffset(Vector3 offset)
	{
		_CarryObjectOffset = offset;
	}

	public void SetObjectScale(Vector3 scale)
	{
		_CarryObjectScale = scale;
	}

	public void CarryObject(GameObject obj)
	{
		StartCoroutine("InitCarryObject", obj);
	}

	private IEnumerator InitCarryObject(GameObject obj)
	{
		if (mAvatarController != null && obj != null)
		{
			if (SanctuaryManager.pCurPetInstance.pIsMounted)
			{
				SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
				yield return null;
				yield return null;
			}
			mAvatarController.pPlayerCarrying = true;
			if (SanctuaryManager.pCurPetInstance != null && AvAvatar.pSubState != AvAvatarSubState.FLYING)
			{
				KAInput.pInstance.EnableInputType("DragonMount", InputType.ALL, inEnable: false);
			}
			if (mCarryObject != null && mCarryObject != obj)
			{
				DetachCarryingObject();
			}
			obj.transform.parent = mHandT;
			obj.transform.localPosition = _CarryObjectOffset;
			obj.transform.localScale = _CarryObjectScale;
			obj.transform.rotation = Quaternion.identity;
			mCarryObject = obj;
		}
	}

	private void DetachCarryingObject()
	{
		if (SanctuaryManager.pCurPetInstance != null && AvAvatar.pSubState != AvAvatarSubState.FLYING)
		{
			KAInput.pInstance.EnableInputType("DragonMount", InputType.ALL, SanctuaryManager.pCurPetInstance.IsMountAllowed() && FUEManager.IsInputEnabled("DragonMount"));
		}
		mCarryObject.transform.parent = null;
		mCarryObject.transform.localPosition = new Vector3(0f, -5000f, 0f);
		mCarryObject = null;
	}

	public void StopCarrying()
	{
		if (mAvatarController != null && mAvatarController.pPlayerCarrying)
		{
			mAvatarController.pPlayerCarrying = false;
			if ((bool)mCarryObject)
			{
				DetachCarryingObject();
			}
		}
		for (int i = 0; i < _DetachObjectOnNPCs.Count; i++)
		{
			_DetachObjectOnNPCs[i].DetachCarryingObject();
		}
	}

	private void SetupForTask(Task task)
	{
		StopCarrying();
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (_DetatchObjectTaskID > 0 && inEvent == MissionEvent.TASK_COMPLETE && ((Task)inObject).TaskID == _DetatchObjectTaskID)
		{
			StopCarrying();
		}
	}

	private void OnDestroy()
	{
		if (mCarryObject != null)
		{
			StopCarrying();
		}
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
	}
}
