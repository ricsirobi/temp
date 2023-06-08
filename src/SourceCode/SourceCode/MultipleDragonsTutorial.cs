using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleDragonsTutorial : InteractiveTutManager
{
	public int _StartTaskID = 381;

	public ParticleSystem _DragonSelectParticle;

	public Vector3 _DragonSelectParticleOffset;

	public PetCSM _PetCSM;

	private ParticleSystem mDragonSelectParticle;

	public override void Start()
	{
		base.Start();
		_StepEndedEvent = (StepEndedEvent)Delegate.Combine(_StepEndedEvent, new StepEndedEvent(OnStepEnded));
		_StepStartedEvent = (StepStartedEvent)Delegate.Combine(_StepStartedEvent, new StepStartedEvent(OnStepStarted));
		MissionManager.AddMissionEventHandler(OnMissionEvent);
		CoCommonLevel.WaitListCompleted += OnWaitListCompleted;
	}

	public override void DeleteInstance()
	{
		_StepEndedEvent = (StepEndedEvent)Delegate.Remove(_StepEndedEvent, new StepEndedEvent(OnStepEnded));
		_StepStartedEvent = (StepStartedEvent)Delegate.Remove(_StepStartedEvent, new StepStartedEvent(OnStepStarted));
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
		AvAvatar.pState = AvAvatarState.PAUSED;
		base.DeleteInstance();
	}

	public void OnStepEnded(int stepIdx, string stepName, bool tutQuit)
	{
		if (!(stepName == "ChooseStableIcon"))
		{
			if (stepName == "Finish" && MissionManager.pInstance != null)
			{
				MissionManager.pInstance.enabled = true;
			}
			return;
		}
		if (mDragonSelectParticle != null)
		{
			UnityEngine.Object.Destroy(mDragonSelectParticle.gameObject);
		}
		ObClickable.pGlobalActive = false;
	}

	public void OnStepStarted(int stepIdx, string stepName)
	{
		if (stepName == "ChooseStableIcon")
		{
			ObClickable.pGlobalActive = true;
			if (_DragonSelectParticle != null)
			{
				mDragonSelectParticle = UnityEngine.Object.Instantiate(_DragonSelectParticle, SanctuaryManager.pCurPetInstance.gameObject.transform);
				mDragonSelectParticle.transform.parent = SanctuaryManager.pCurPetInstance.gameObject.transform;
				mDragonSelectParticle.transform.localPosition = _DragonSelectParticleOffset;
				StartCoroutine(PlayParticle());
			}
		}
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (inEvent != MissionEvent.OFFER_COMPLETE)
		{
			return;
		}
		MissionManager.Action action = (MissionManager.Action)inObject;
		if (action._Object == null || !(action._Object.GetType() == typeof(Task)))
		{
			return;
		}
		Task task = (Task)action._Object;
		if (task != null && task.TaskID == _StartTaskID && !TutorialComplete())
		{
			ShowTutorial();
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.enabled = false;
			}
		}
	}

	private IEnumerator PlayParticle()
	{
		yield return new WaitForEndOfFrame();
		if (mDragonSelectParticle != null)
		{
			mDragonSelectParticle.Play();
		}
	}

	private void OnWaitListCompleted()
	{
		CoCommonLevel.WaitListCompleted -= OnWaitListCompleted;
		Task task = MissionManager.pInstance.GetTask(_StartTaskID);
		if (task == null || !task.pStarted || task.pCompleted)
		{
			return;
		}
		List<MissionAction> offers = task.GetOffers(unplayed: false);
		if (offers != null && offers[0]._Played && !TutorialComplete())
		{
			ShowTutorial();
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.enabled = false;
			}
		}
	}

	private void OnDestroy()
	{
		_StepEndedEvent = (StepEndedEvent)Delegate.Remove(_StepEndedEvent, new StepEndedEvent(OnStepEnded));
		_StepStartedEvent = (StepStartedEvent)Delegate.Remove(_StepStartedEvent, new StepStartedEvent(OnStepStarted));
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
	}
}
