using System;
using System.Collections.Generic;
using UnityEngine;

public class GearDriver : MonoBehaviour
{
	[Serializable]
	public class Settings
	{
		public bool isMotor;

		public bool isShaft;

		public bool isWorm;

		public bool invWormOut;

		public bool updateOnce;

		public bool updateLive;

		public float motorSpeed;

		public List<GearDriver> outputTo;
	}

	public Settings settings;

	public float actualSpeed;

	private int error;

	private bool lastUpdState;

	private void Start()
	{
		if (GetComponent<ProceduralWormGear>() != null)
		{
			settings.isWorm = true;
		}
		else if (GetComponent<ProceduralGear>() == null)
		{
			settings.isShaft = true;
		}
		else
		{
			settings.isShaft = false;
		}
		if (settings.isMotor)
		{
			error++;
			UpdateConnections((!settings.isShaft) ? GetTeethCountFromGearScript() : 0, settings.motorSpeed, error, settings.updateLive);
		}
	}

	private int GetTeethCountFromGearScript()
	{
		if (GetComponent<ProceduralWormGear>() != null)
		{
			if (GetComponent<ProceduralWormGear>().prefs.lr)
			{
				if (!settings.invWormOut)
				{
					return -1;
				}
				return 1;
			}
			if (!settings.invWormOut)
			{
				return 1;
			}
			return -1;
		}
		if (GetComponent<ProceduralGear>() != null)
		{
			return GetComponent<ProceduralGear>().prefs.teethCount;
		}
		return 0;
	}

	private void Update()
	{
		if (settings.isMotor)
		{
			DriveMotor();
		}
		if (!settings.updateLive)
		{
			base.gameObject.transform.Rotate(Vector3.up * Time.deltaTime * (0f - actualSpeed));
		}
	}

	private void DriveMotor()
	{
		if (settings.updateOnce || settings.updateLive || lastUpdState)
		{
			UpdatePowerchain();
		}
	}

	public void UpdateConnections(int _otherTeethCount, float _speed, int _error, bool _updateRotation)
	{
		if (!settings.isMotor)
		{
			if (error == _error)
			{
				Debug.LogWarning("GearDriver.cs : Get two inputs on " + base.gameObject.name + " . Check connections for loop.");
				base.enabled = false;
				return;
			}
			settings.updateLive = _updateRotation;
		}
		error = _error;
		int num = 0;
		if (_otherTeethCount == 0)
		{
			_otherTeethCount = (settings.isShaft ? 1 : (-GetTeethCountFromGearScript()));
		}
		if (!settings.isShaft)
		{
			num = GetTeethCountFromGearScript();
			actualSpeed = (float)_otherTeethCount / (float)num * (0f - _speed);
		}
		else
		{
			actualSpeed = _speed;
		}
		for (int i = 0; i < settings.outputTo.Count; i++)
		{
			if (settings.outputTo[i] != null)
			{
				settings.outputTo[i].UpdateConnections(num, actualSpeed, error, _updateRotation);
			}
			else
			{
				settings.outputTo.RemoveAt(i);
			}
		}
		if (_updateRotation)
		{
			base.gameObject.transform.Rotate(Vector3.up * Time.deltaTime * (0f - actualSpeed));
		}
	}

	public void UpdatePowerchain()
	{
		error = ((error <= 8) ? (error += 2) : 0);
		lastUpdState = settings.updateLive;
		UpdateConnections((!settings.isShaft) ? GetTeethCountFromGearScript() : 0, settings.motorSpeed, error, lastUpdState);
		settings.updateOnce = false;
	}
}
