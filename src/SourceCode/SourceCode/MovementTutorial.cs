using UnityEngine;

public class MovementTutorial : KAMonoBase
{
	public int _PlayerMoveTaskID;

	public KAUI _TutorialUI;

	public float _MoveOffsetToCompleteTut;

	public JoyStickFlashData[] _Data;

	private bool mIsMobilePlatform;

	private Vector3 mCachedPosition = Vector3.zero;

	private bool mTutorialActive;

	private Task mTask;

	private void Start()
	{
		mIsMobilePlatform = UtPlatform.IsMobile();
		if (UtPlatform.IsWSA())
		{
			mIsMobilePlatform = !UtUtilities.IsKeyboardAttached();
		}
		if (AvAvatar.pToolbar != null)
		{
			base.transform.parent = AvAvatar.pToolbar.transform;
		}
		if (mIsMobilePlatform)
		{
			mTask = MissionManager.pInstance.GetTask(_PlayerMoveTaskID);
		}
		else
		{
			MissionManager.AddMissionEventHandler(OnMissionEvent);
		}
	}

	private void Update()
	{
		if (mTutorialActive)
		{
			if (mIsMobilePlatform)
			{
				if (mTask != null && mTask.pCompleted)
				{
					StopFlash();
					mTutorialActive = false;
				}
			}
			else if ((mCachedPosition - AvAvatar.position).sqrMagnitude > _MoveOffsetToCompleteTut)
			{
				mTutorialActive = false;
				_TutorialUI.SetVisibility(inVisible: false);
				base.enabled = false;
			}
		}
		else if (mIsMobilePlatform && mTask != null && mTask.pStarted && !mTask.pCompleted)
		{
			mTutorialActive = true;
			mCachedPosition = AvAvatar.position;
			StartFlash();
		}
	}

	private void StartFlash()
	{
		for (int i = 0; i < _Data.Length; i++)
		{
			_Data[i]._FlashWidget = UiJoystick.pInstance.FindItem(_Data[i]._FlashWidgetName);
			_Data[i]._FlashWidget.PlayAnim("Flash");
		}
	}

	private void StopFlash()
	{
		for (int i = 0; i < _Data.Length; i++)
		{
			_Data[i]._FlashWidget.PlayAnim("Normal");
		}
	}

	private void OnDestroy()
	{
		if (mTutorialActive && mIsMobilePlatform)
		{
			StopFlash();
		}
		if (!mIsMobilePlatform)
		{
			MissionManager.RemoveMissionEventHandler(OnMissionEvent);
		}
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (inEvent != MissionEvent.OFFER_COMPLETE)
		{
			return;
		}
		MissionManager.Action action = (MissionManager.Action)inObject;
		if (action._Object != null && action._Object.GetType() == typeof(Task))
		{
			Task task = (Task)action._Object;
			if (task != null && task.TaskID == _PlayerMoveTaskID)
			{
				mTutorialActive = true;
				mCachedPosition = AvAvatar.position;
				_TutorialUI.SetVisibility(inVisible: true);
			}
		}
	}
}
