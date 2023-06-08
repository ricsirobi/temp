using System;
using UnityEngine;

[Serializable]
public class SplineControlData
{
	public float _Speed;

	public float _Duration;

	public SnSound _Sound;

	public GameObject _LookAtMarker;

	public float _LookAtTweenDuration;

	public GameObject _MessageObject;

	public string _MessageFunctionName;

	public bool _PauseStep;
}
