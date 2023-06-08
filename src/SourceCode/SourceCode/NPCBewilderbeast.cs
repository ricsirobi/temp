using System.Collections.Generic;
using UnityEngine;

public class NPCBewilderbeast : MonoBehaviour
{
	private Animator _BewilderbeastAnimator;

	public string _EnterStableParameterName;

	public string _EnterPlatformParameterName;

	public float _FidgetMinTime = 30f;

	public float _FidgetMaxTime = 40f;

	public List<string> _FidgetTriggerNames = new List<string> { "IdleFidget" };

	private void Start()
	{
		_BewilderbeastAnimator = GetComponent<Animator>();
	}

	private void InvokeFidget()
	{
		Invoke("StartFidget", Random.Range(_FidgetMinTime, _FidgetMaxTime));
	}

	private void StartFidget()
	{
		SetTriggerAnimation(_FidgetTriggerNames[Random.Range(0, _FidgetTriggerNames.Count)]);
		InvokeFidget();
	}

	public void OnSwitchOn(string switchName)
	{
		if (switchName == "EntryTrigger")
		{
			SetTriggerAnimation(_EnterStableParameterName);
			InvokeFidget();
		}
		else if (switchName == "PlatformTrigger")
		{
			SetBoolAnimation(_EnterPlatformParameterName, inBool: true);
			CancelInvoke("StartFidget");
		}
	}

	public void OnSwitchOff(string switchName)
	{
		SetBoolAnimation(_EnterPlatformParameterName, inBool: false);
		InvokeFidget();
	}

	public void SetTriggerAnimation(string inParameter)
	{
		_BewilderbeastAnimator?.SetTrigger(inParameter);
	}

	public void SetBoolAnimation(string inParameter, bool inBool)
	{
		_BewilderbeastAnimator?.SetBool(inParameter, inBool);
	}
}
