using System;
using System.Collections.Generic;
using UnityEngine;

public class VolcanoMissionActionJump : ActionBase
{
	[Tooltip("Animation to play when the pet moved to marker after jumping on Greendeath")]
	public string _PetFlyAnim = "FlyForward";

	public float _ProximityRangeToSwitchFarClip = 20f;

	public float _FarClipDistance;

	public Transform _PetMarker;

	[Tooltip("Mount will happen on Task_End_Complete event for the given TaskID")]
	public int _PetMountTaskID;

	[Tooltip("Respawn marker if player tries to land on GreenDeath")]
	public GameObject _RespawnMarker;

	[Tooltip("List of colliders that will respawn the avatar if collided with")]
	public List<GameObject> _Colliders;

	private bool mInitDone;

	private bool mWithinRange;

	private float mCacheFarClipDistance;

	private AvAvatarController mAvatarController;

	private bool mCheckForTaskCompletion;

	public override void Start()
	{
		base.Start();
		CoCommonLevel component = GameObject.Find("PfCommonLevel").GetComponent<CoCommonLevel>();
		if (component != null && component.pWaitListCompleted)
		{
			WaitListCompleted();
		}
		else
		{
			CoCommonLevel.WaitListCompleted += WaitListCompleted;
		}
	}

	private void Init()
	{
		mInitDone = true;
		mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		AvAvatarController avAvatarController = mAvatarController;
		avAvatarController.OnAvatarCollision = (AvAvatarController.OnAvatarCollisionDelegate)Delegate.Combine(avAvatarController.OnAvatarCollision, new AvAvatarController.OnAvatarCollisionDelegate(OnAvatarCollide));
	}

	public override bool IsActionAllowed()
	{
		if (base.IsActionAllowed())
		{
			return mAvatarController.pPlayerMounted;
		}
		return false;
	}

	public override void ExecuteAction()
	{
		mCheckForTaskCompletion = true;
		SetupTask();
	}

	private void DisablePetMount(bool disable)
	{
		SanctuaryManager.pCurPetInstance.mMountDisabled = disable;
		SanctuaryManager.pInstance.pDisablePetSwitch = disable;
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.UpdateMountButtonVisibility();
		}
	}

	private bool WithinRange()
	{
		if ((AvAvatar.position - base.transform.position).magnitude < _ProximityRangeToSwitchFarClip)
		{
			return true;
		}
		return false;
	}

	public void Update()
	{
		if (AvAvatar.pObject != null)
		{
			if (!mInitDone)
			{
				Init();
			}
			if (WithinRange())
			{
				OnProximityEnter();
			}
			else
			{
				OnProximityExit();
			}
			if (mCheckForTaskCompletion && !string.IsNullOrEmpty(_ActionName) && MissionManager.pInstance != null && mAvatarController != null && mAvatarController.OnGround())
			{
				mCheckForTaskCompletion = false;
				MissionManager.pInstance.CheckForTaskCompletion("Action", _ActionName, base.gameObject.name);
			}
		}
	}

	public void StartMount()
	{
		DisablePetMount(disable: false);
		SanctuaryManager.pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.FLY);
		mAvatarController.UpdateGliding();
	}

	public void OnProximityEnter()
	{
		if (!mWithinRange)
		{
			mWithinRange = true;
			Camera component = AvAvatar.pAvatarCam.GetComponent<Camera>();
			mCacheFarClipDistance = component.farClipPlane;
			component.farClipPlane = _FarClipDistance;
		}
	}

	public void OnProximityExit()
	{
		if (mWithinRange)
		{
			mWithinRange = false;
			AvAvatar.pAvatarCam.GetComponent<Camera>().farClipPlane = mCacheFarClipDistance;
		}
	}

	public void SetupTask()
	{
		if ((bool)SanctuaryManager.pCurPetInstance)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject, falltoGround: false);
			SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: false);
			if (_PetMarker != null)
			{
				SanctuaryManager.pCurPetInstance.transform.position = _PetMarker.position;
				SanctuaryManager.pCurPetInstance.transform.rotation = _PetMarker.rotation;
			}
			DisablePetMount(disable: true);
			SanctuaryManager.pCurPetInstance.PlayAnimation(_PetFlyAnim, WrapMode.Loop);
			MissionManager.AddMissionEventHandler(OnMissionEvent);
		}
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (inEvent == MissionEvent.TASK_END_COMPLETE && inObject is Task task && task.TaskID == _PetMountTaskID)
		{
			StartMount();
			MissionManager.RemoveMissionEventHandler(OnMissionEvent);
		}
	}

	private void OnDestroy()
	{
		if (MissionManager.pInstance != null)
		{
			Task task = MissionManager.pInstance.GetTask(_PetMountTaskID);
			if (task != null && task.pStarted && !task.pCompleted)
			{
				DisablePetMount(disable: false);
			}
		}
		AvAvatarController avAvatarController = mAvatarController;
		avAvatarController.OnAvatarCollision = (AvAvatarController.OnAvatarCollisionDelegate)Delegate.Remove(avAvatarController.OnAvatarCollision, new AvAvatarController.OnAvatarCollisionDelegate(OnAvatarCollide));
		CoCommonLevel.WaitListCompleted -= WaitListCompleted;
	}

	public void OnAvatarCollide(GameObject avatarObj, GameObject hitObj)
	{
		if (AvAvatar.IsCurrentPlayer(avatarObj) && _Colliders.Contains(hitObj) && IsActionAllowed())
		{
			AvAvatar.TeleportToObject(_RespawnMarker);
		}
	}

	private void WaitListCompleted()
	{
		if (MissionManager.pInstance != null)
		{
			Task task = MissionManager.pInstance.GetTask(_PetMountTaskID);
			if (task != null && task.pStarted && !task.pCompleted)
			{
				AvAvatar.SetPosition(base.transform.position);
				SetupTask();
			}
		}
	}
}
