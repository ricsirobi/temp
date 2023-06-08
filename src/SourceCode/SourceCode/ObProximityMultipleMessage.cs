using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObProximityMultipleMessage : ObProximity
{
	[Serializable]
	public class RangeParameters
	{
		[Tooltip("The range at which the _EnterMessage will be sent upon the avatar's distance is less than this value and _ExitMessage will be sent upon the avatar's distance being greater than this value")]
		public float _Range;

		[Tooltip("Used to adjust the position where the distance check is done")]
		public Vector3 _Offset;

		[Tooltip("The message sent when avatar's distance < _Range")]
		public string _EnterMessage;

		[Tooltip("The message sent when avatar's distance > _Range")]
		public string _ExitMessage;

		[Tooltip("The scene to load for this range, if applicable")]
		public string _LoadLevel;

		[Tooltip("The object that will receive the _EnterMessage or _ExitMessage. Defaults to the object this script is attached to if null")]
		public GameObject _MessageObject;

		[Tooltip("The object that will be toggled on when entering this range or off when exiting this range")]
		public GameObject _ActivateObject;

		[Tooltip("Toggle drawing this range in the inspector. _Draw must be enabled on the base class")]
		public bool _Draw = true;

		[Tooltip("The color used to draw the range in the inspector")]
		public Color _Color = Color.white;

		[HideInInspector]
		public bool pWithinRange;
	}

	public List<RangeParameters> _RangeParameters;

	public float _DistanceCheckFrequency = 0.5f;

	private void Start()
	{
		_RangeParameters.RemoveAll((RangeParameters t) => t == null);
		_RangeParameters.OrderBy((RangeParameters t) => t._Range);
		if (_RangeParameters.Count > 1)
		{
			foreach (RangeParameters rp in _RangeParameters)
			{
				if (_RangeParameters.Find((RangeParameters t) => t._Range == rp._Range) != null)
				{
					UtDebug.LogWarning("Duplicate ranges on " + base.gameObject.name + ", may cause unintended behavior!");
				}
			}
		}
		StartCoroutine(CheckDistance());
	}

	private IEnumerator CheckDistance()
	{
		while (true)
		{
			yield return new WaitForSeconds(_DistanceCheckFrequency);
			if ((_UseGlobalActive && !ObClickable.pGlobalActive) || !_Active || AvAvatar.pObject == null)
			{
				yield return null;
			}
			foreach (RangeParameters rp in _RangeParameters)
			{
				if (rp.pWithinRange)
				{
					rp.pWithinRange = IsInProximity(AvAvatar.position, rp);
					if (!rp.pWithinRange && !string.IsNullOrEmpty(rp._ExitMessage))
					{
						if (rp._MessageObject != null)
						{
							rp._MessageObject.SendMessage(rp._ExitMessage, null, SendMessageOptions.DontRequireReceiver);
						}
						else
						{
							SendMessage(rp._ExitMessage, null, SendMessageOptions.DontRequireReceiver);
						}
					}
					ActivateObjectWithinRange(rp);
				}
				else if (rp._Range != 0f && !string.IsNullOrEmpty(rp._EnterMessage) && IsInProximity(AvAvatar.position, rp))
				{
					rp.pWithinRange = true;
					_LoadLevel = rp._LoadLevel;
					if (TryLoadLevel(rp._LoadLevel))
					{
						yield return null;
					}
					ActivateObjectWithinRange(rp);
					if (rp._MessageObject != null)
					{
						rp._MessageObject.SendMessage(rp._EnterMessage, null, SendMessageOptions.DontRequireReceiver);
					}
					else
					{
						SendMessage(rp._EnterMessage, null, SendMessageOptions.DontRequireReceiver);
					}
					break;
				}
			}
		}
	}

	public bool IsInProximity(Vector3 position, RangeParameters rangeParameter)
	{
		return Vector3.Distance(base.transform.position + base.transform.TransformDirection(rangeParameter._Offset), position) < rangeParameter._Range;
	}

	private void ActivateObjectWithinRange(RangeParameters rp)
	{
		if (rp._ActivateObject != null)
		{
			rp._ActivateObject.SetActive(rp.pWithinRange);
		}
	}

	private void OnDestroy()
	{
		StopCoroutine(CheckDistance());
	}

	private void OnDrawGizmos()
	{
		if (!_Draw)
		{
			return;
		}
		for (int i = 0; i < _RangeParameters.Count; i++)
		{
			if (_RangeParameters[i]._Range != 0f && _RangeParameters[i]._Draw)
			{
				Gizmos.color = _RangeParameters[i]._Color;
				Gizmos.DrawWireSphere(base.transform.position + base.transform.TransformDirection(_RangeParameters[i]._Offset), _RangeParameters[i]._Range);
			}
		}
	}
}
