using System;
using UnityEngine;

[Serializable]
public class ObMissionActivateData
{
	[Tooltip("If the given mission is completed the objects will be activated.")]
	public int _CompleteMission;

	[Tooltip("If the given task is started or completed and EndTask (if defined) is not started or completed the objects will be activated.")]
	public int _StartTask;

	[Tooltip("Wait for the offer of the StartTask to be shown before the objects will be activated.")]
	public bool _WaitForOffer;

	[Tooltip("Wait for the avatar to be in IDLE state before the objects will be activated.")]
	public bool _WaitForAvatar;

	[Tooltip("If the given task is completed and EndTask (if defined) is not started or completed the objects will be activated.")]
	public int _CompleteTask;

	[Tooltip("If the given task is started or completed the objects will not be activated.")]
	public int _EndTask;

	[NonSerialized]
	public Task mStartTask;

	[NonSerialized]
	public Task mCompleteTask;

	[NonSerialized]
	public Task mEndTask;

	[NonSerialized]
	public Mission mCompleteMission;
}
