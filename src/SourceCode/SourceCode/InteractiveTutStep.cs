using System;
using UnityEngine;

[Serializable]
public class InteractiveTutStep
{
	public string _Name;

	public AudioClip _VO;

	public string _VO_URL;

	public string _IconName;

	public bool _WaitForUserEvent;

	public bool _IsMobileOnly;

	public bool _IsWebOnly;

	public InteractiveTutInterface[] _Interfaces;

	public InteractiveTutInterface[] _StepEndInterfaces;

	public bool _SaveProgress;

	public LocaleString _StepText = new LocaleString("");

	public InteractiveTutPlayerActions[] _PlayerControls;

	public InteractiveTutGameObject[] _GameObjectControls;

	public InteractiveTutPlayerActions[] _StepEndPlayerControls;

	public InteractiveTutGameObject[] _StepEndGameObjectControls;

	public InteractiveTutReward[] _StepRewards;

	public bool _IsInteractiveStep;

	public bool _DisableBoardButtons;

	public Rect _BoardPosition;

	public KAOrientationUI _OrientationInfo;

	public InteractiveTutStepDetails _StepDetails;

	public bool _CanSkipStep;
}
