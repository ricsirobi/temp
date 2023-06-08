using System;
using UnityEngine;

[Serializable]
public class GauntletControlData
{
	public float _Duration;

	public AudioClip _AudioClip;

	public string _AudioPoolName;

	public GameObject _LookAtMarker;

	public float _LookAtTweenDuration;

	public GameObject _MessageObject;

	public string _MessageFunctionName;

	public bool _PauseStep;
}
